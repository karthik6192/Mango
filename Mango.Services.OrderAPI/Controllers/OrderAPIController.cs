using AutoMapper;
using Azure;
using Mango.MessageBus;
using Mango.Services.OrderAPI.Data;
using Mango.Services.OrderAPI.Models;
using Mango.Services.OrderAPI.Models.DTO;
using Mango.Services.OrderAPI.Service.IService;
using Mango.Services.OrderAPI.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;

namespace Mango.Services.OrderAPI.Controllers
{
    [Route("api/order")]
    [ApiController]
    public class OrderAPIController : ControllerBase
    {
        private readonly AppDbContext dbContext;
        private readonly IMapper mapper;
        private readonly IProductService productService;
        private readonly IMessageBus messageBus;
        private readonly IConfiguration configuration;
        private ResponseDTO response;

        public OrderAPIController(AppDbContext dbContext,
                                  IMapper mapper,
                                  IProductService productService,
                                  IMessageBus messageBus,
                                  IConfiguration configuration)
        {
            this.dbContext = dbContext;
            this.mapper = mapper;
            this.productService = productService;
            this.messageBus = messageBus;
            this.configuration = configuration;
            response = new ResponseDTO();
        }


        [Authorize]
        [HttpPost("CreateOrder")]

        public async Task<ResponseDTO> CreateOrder([FromBody] CartDTO cartDTO)
        {
            try
            {
                OrderHeaderDTO orderHeaderDTO = mapper.Map<OrderHeaderDTO>(cartDTO.CartHeader);
                orderHeaderDTO.OrderTime = DateTime.Now;
                orderHeaderDTO.Status = SD.Status_Pending;
                orderHeaderDTO.OrderDetails = mapper.Map<IEnumerable<OrderDetailsDTO>>(cartDTO.CartDetails);


                OrderHeader orderCreated = dbContext.OrderHeaders.Add(mapper.Map<OrderHeader>(orderHeaderDTO)).Entity;
                await dbContext.SaveChangesAsync();

                orderHeaderDTO.OrderHeaderId = orderCreated.OrderHeaderId;
                response.Result = orderHeaderDTO;

            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }

            return response;
        }



        [Authorize]
        [HttpPost("CreateStripeSession")]
        public async Task<ResponseDTO> CreateStripeSession([FromBody] StripeRequestDTO stripeRequestDTO)
        {
            try
            {

                var options = new SessionCreateOptions
                {
                    SuccessUrl = stripeRequestDTO.ApprovedUrl,
                    CancelUrl = stripeRequestDTO.CancelUrl,
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                
                };

                var discountsObj = new List<SessionDiscountOptions>()
                {
                    new SessionDiscountOptions
                    {
                        Coupon = stripeRequestDTO.OrderHeader.CouponCode
                    }
                };
                foreach (var item in stripeRequestDTO.OrderHeader.OrderDetails)
                {
                    var sessionLineItems = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Price * 100),
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.Name
                            }
                        },
                        Quantity = item.Count
                    };

                    options.LineItems.Add(sessionLineItems);
                }

                if(stripeRequestDTO.OrderHeader.Discount > 0)
                {
                    options.Discounts = discountsObj;
                }
                var service = new SessionService();
                Session session = service.Create(options);

                stripeRequestDTO.StripeSessionUrl = session.Url;
                OrderHeader orderHeader = dbContext.OrderHeaders.First(u => u.OrderHeaderId == stripeRequestDTO.OrderHeader.OrderHeaderId);
                orderHeader.StripeSessionId = session.Id;
                dbContext.SaveChanges();
                response.Result = stripeRequestDTO;

            }

            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }

            return response;
        }



        [Authorize]
        [HttpPost("ValidateStripeSession")]
        public async Task<ResponseDTO> ValidateStripeSession([FromBody] int orderHeaderId)
        {
            try
            {

                OrderHeader orderHeader = dbContext.OrderHeaders.First(u => u.OrderHeaderId == orderHeaderId);
               
                
                var service = new SessionService();
                Session session = service.Get(orderHeader.StripeSessionId);

                var paymentIntentService = new PaymentIntentService();
                PaymentIntent paymentIntent = paymentIntentService.Get(session.PaymentIntentId);

                if(paymentIntent.Status == "succeeded")
                {
                    //then payment was successfull.
                    orderHeader.PaymentIntentId = paymentIntent.Id;
                    orderHeader.Status = SD.Status_Approved;
                    dbContext.SaveChanges();
                    RewardsDTO rewardsDTO = new()
                    {
                        OrderId = orderHeader.OrderHeaderId,
                        RewardsActivity = Convert.ToInt32(orderHeader.OrderTotal),
                        UserId = orderHeader.UserId
                    };
                    string topicName = configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedTopic");
                    await messageBus.PublishMessage(rewardsDTO, topicName);
                    response.Result = mapper.Map<OrderHeaderDTO>(orderHeader);
                }

            }

            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }

            return response;
        }


        [Authorize]
        [HttpGet("GetOrders")]
        public ResponseDTO? Get(string? userId = "")
        {
            try
            {
                IEnumerable<OrderHeader> objList;

                if(User.IsInRole(SD.RoleAdmin))
                {
                    objList = dbContext.OrderHeaders.Include(u => u.OrderDetails)
                                .OrderByDescending(u => u.OrderHeaderId).ToList();
                }
                else
                {
                    objList = dbContext.OrderHeaders.Include(u => u.OrderDetails).Where(u => u.UserId == userId)
                                    .OrderByDescending(u => u.OrderHeaderId).ToList();
                }

                response.Result = mapper.Map<IEnumerable<OrderHeaderDTO>>(objList);
            }

            catch(Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }

            return response;
        }



        [Authorize]
        [HttpGet("GetOrder/{id:int}")]
        public ResponseDTO? Get(int id)
        {
            try
            {
                OrderHeader orderHeader = dbContext.OrderHeaders.Include(u => u.OrderDetails)
                                          .First(u => u.OrderHeaderId == id);

                response.Result = mapper.Map<OrderHeaderDTO>(orderHeader);
            }

            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }

            return response;
        }


        [Authorize]
        [HttpPost("UpdateOrderStatus/{orderId:int}")]

        public async Task<ResponseDTO> UpdateOrderStatus( int orderId, [FromBody] string newStatus)
        {
            try
            {
                OrderHeader orderHeader = dbContext.OrderHeaders.First(u => u.OrderHeaderId == orderId);

                if(orderHeader != null)
                {
                    if(newStatus == SD.Status_Cancelled)
                    {
                        //we will give refund

                        var options = new RefundCreateOptions
                        {
                            Reason = RefundReasons.RequestedByCustomer,
                            PaymentIntent = orderHeader.PaymentIntentId,

                        };

                        var service = new RefundService();
                        Refund refund = service.Create(options);
                        orderHeader.Status = newStatus;
                    }
                    orderHeader.Status = newStatus;
                    dbContext.SaveChanges();
                }
            }

            catch( Exception ex )
            {
                response.IsSuccess = false; 
                response.Message = ex.Message;  
            }

            return response;    
        }
    }
}
