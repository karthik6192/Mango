using AutoMapper;
using Mango.MessageBus;
using Mango.Services.ShoppingCartAPI.Data;
using Mango.Services.ShoppingCartAPI.Models;
using Mango.Services.ShoppingCartAPI.Models.DTO;
using Mango.Services.ShoppingCartAPI.Service.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection.PortableExecutable;

namespace Mango.Services.ShoppingCartAPI.Controllers
{
    [Route("api/cart")]
    [ApiController]
    public class CartAPIController : ControllerBase
    {
        private readonly AppDbContext dbContext;
        private readonly IMapper mapper;
        private readonly IProductService productService;
        private readonly ICouponService couponService;
        private readonly IMessageBus messageBus;
        private readonly IConfiguration configuration;
        private ResponseDTO _response;

        public CartAPIController(AppDbContext dbContext,
                                 IMapper mapper, 
                                 IProductService productService,
                                 ICouponService couponService,
                                 IMessageBus messageBus,
                                 IConfiguration configuration)
        {
            this.dbContext = dbContext;
            this.mapper = mapper;
            this.productService = productService;
            this.couponService = couponService;
            this.messageBus = messageBus;
            this.configuration = configuration;
            _response = new ResponseDTO();
        }


        [HttpGet("GetCart/{userId}")]

        public async Task<ResponseDTO> GetCart(string userId)
        {
            try
            {
                CartDTO cart = new()
                {
                    CartHeader = mapper.Map<CartHeaderDTO>(dbContext.CartHeaders
                            .First(u => u.UserId == userId))
                };

                cart.CartDetails = mapper.Map<IEnumerable<CartDetailsDTO>>(dbContext.CartDetails
                            .Where(u => u.CartHeaderId == cart.CartHeader.CartHeaderId));

                IEnumerable<ProductDTO> productDtos = await productService.GetProducts();

                foreach(var item in cart.CartDetails)
                {
                    item.Product = productDtos.FirstOrDefault(u => u.ProductId == item.ProductId);
                    cart.CartHeader.CartTotal += (item.Count * item.Product.Price);
                }

                //apply coupon If any

                if(!string.IsNullOrEmpty(cart.CartHeader.CouponCode))
                {
                    CouponDTO coupon = await couponService.GetCoupon(cart.CartHeader.CouponCode);

                    if(coupon != null && cart.CartHeader.CartTotal > coupon.MinAmount)
                    {
                        cart.CartHeader.CartTotal -= coupon.DiscountAmount;
                        cart.CartHeader.Discount = coupon.DiscountAmount;

                    }
                }

                _response.Result = cart;
            }

            catch(Exception ex) 
            {
                _response.Message = ex.Message.ToString();
                _response.IsSuccess = false;
            }   

            return _response;
        }



        [HttpPost("CartUpsert")]
        public async Task<ResponseDTO> CartUpsert(CartDTO cartDTO)
        {
            try
            {
                var cartHeaderFromDb = await dbContext.CartHeaders.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.UserId == cartDTO.CartHeader.UserId);

                if(cartHeaderFromDb == null)
                {
                    //create cart header and details
                    CartHeader cartHeader = mapper.Map<CartHeader>(cartDTO.CartHeader);
                    await dbContext.CartHeaders.AddAsync(cartHeader);
                    await dbContext.SaveChangesAsync();

                    cartDTO.CartDetails.First().CartHeaderId = cartHeader.CartHeaderId;
                   await dbContext.CartDetails.AddAsync(mapper.Map<CartDetails>(cartDTO.CartDetails.First()));
                   await dbContext.SaveChangesAsync();
                }

                else
                {
                    // if header is not null
                    //check if details has same product

                    var cartDetailsFromDb = await dbContext.CartDetails.AsNoTracking().FirstOrDefaultAsync(
                        u => u.ProductId == cartDTO.CartDetails.First().ProductId &&
                        u.CartHeaderId == cartHeaderFromDb.CartHeaderId);

                    if( cartDetailsFromDb == null)
                    {
                        //create cartDetails
                        cartDTO.CartDetails.First().CartHeaderId = cartHeaderFromDb.CartHeaderId;
                        await dbContext.CartDetails.AddAsync(mapper.Map<CartDetails>(cartDTO.CartDetails.First()));
                        await dbContext.SaveChangesAsync();

                    }

                    else
                    {
                        //update count in cart details

                        cartDTO.CartDetails.First().Count += cartDetailsFromDb.Count;
                        cartDTO.CartDetails.First().CartHeaderId = cartDetailsFromDb.CartHeaderId;
                        cartDTO.CartDetails.First().CartDetailsId = cartDetailsFromDb.CartDetailsId;
                        dbContext.CartDetails.Update(mapper.Map<CartDetails>(cartDTO.CartDetails.First()));
                        await dbContext.SaveChangesAsync();
                    }
                }

                _response.Result = cartDTO; 

            }

            catch (Exception ex)
            {
                _response.Message = ex.Message.ToString();
                _response.IsSuccess = false;
            }

            return _response;
        }

        [HttpPost("RemoveCart")]
        public async Task<ResponseDTO> RemoveCart([FromBody] int cartDetailsId)
        {
            try
            {
                CartDetails cartDetails =  dbContext.CartDetails.First
                    (u => u.CartDetailsId == cartDetailsId);

                int totalCountOfCartItem = dbContext.CartDetails.Where(u => u.CartHeaderId == cartDetails.CartHeaderId).Count();

                dbContext.CartDetails.Remove(cartDetails);

                if(totalCountOfCartItem == 1)
                {
                    var cartHeaderToRemove = dbContext.CartHeaders.First(u => u.CartHeaderId == cartDetails.CartHeaderId);
                    dbContext.CartHeaders.Remove(cartHeaderToRemove);
                }

                await dbContext.SaveChangesAsync();

                _response.Result = true;

            }

            catch (Exception ex)
            {
                _response.Message = ex.Message.ToString();
                _response.IsSuccess = false;
            }

            return _response;
        }


        [HttpPost("ApplyCoupon")]
        public async Task<object> ApplyCoupon([FromBody] CartDTO cartDTO)
        {
            try
            {
                var cartFromDb = await dbContext.CartHeaders.FirstAsync(u => u.UserId == cartDTO.CartHeader.UserId);
                cartFromDb.CouponCode = cartDTO.CartHeader.CouponCode;

                dbContext.CartHeaders.Update(cartFromDb);
                await dbContext.SaveChangesAsync();
                _response.Result = true;

            }

            catch(Exception ex) 
            {
                _response.Message = ex.Message.ToString();
                _response.IsSuccess = false;
            }
            return _response;   
        }


        [HttpPost("EmailCartRequest")]
        public async Task<object> EmailCartRequest([FromBody] CartDTO cartDTO)
        {
            try
            {
                await messageBus.PublishMessage(cartDTO, configuration.GetValue<string>("TopicAndQueueNames:EmailShoppingCartQueue"));
                _response.Result = true;

            }

            catch (Exception ex)
            {
                _response.Message = ex.Message.ToString();
                _response.IsSuccess = false;
            }
            return _response;
        }


    }
}
