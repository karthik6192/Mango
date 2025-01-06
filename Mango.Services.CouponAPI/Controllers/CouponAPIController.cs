using AutoMapper;
using Mango.Services.CouponAPI.Data;
using Mango.Services.CouponAPI.Models;
using Mango.Services.CouponAPI.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.CouponAPI.Controllers
{
    [Route("api/coupon")]
    [ApiController]
    public class CouponAPIController : ControllerBase
    {
        private readonly AppDbContext dbContext;
        private readonly IMapper mapper;
        private ResponseDTO _response;

        public CouponAPIController(AppDbContext dbContext,IMapper mapper)
        {
            this.dbContext = dbContext;
            this.mapper = mapper;
            _response = new ResponseDTO();   
        }

        [HttpGet]
        public ResponseDTO Get()
        {
            try
            {
                IEnumerable<Coupon> objList = dbContext.Coupons.ToList();
                _response.Result = mapper.Map<IEnumerable<CouponDTO>>(objList);
            }

            catch (Exception ex) 
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }

            return _response;
        }

        [HttpGet]
        [Route("{id}")]
        public ResponseDTO Get(int id) 
        {
            try
            {
              Coupon obj = dbContext.Coupons.First(x => x.CouponId == id);
              _response.Result = mapper.Map<CouponDTO>(obj);
            }

            catch (Exception ex) 
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }

            return _response;
        }


        [HttpGet]
        [Route("GetByCode/{code}")]
        public ResponseDTO GetByCode(string code)
        {
            try
            {
                Coupon obj = dbContext.Coupons.FirstOrDefault(x => x.CouponCode.ToLower() == code.ToLower());
                
                if(obj == null)
                {
                    _response.IsSuccess = false;
                }
                _response.Result = mapper.Map<CouponDTO>(obj);
            }

            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }

            return _response;
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public ResponseDTO Post([FromBody] CouponDTO couponDTO)
        {
            try
            {
                
                Coupon obj = mapper.Map<Coupon>(couponDTO);
                
                dbContext.Coupons.Add(obj);
                dbContext.SaveChanges();

                

                var options = new Stripe.CouponCreateOptions
                {
                    AmountOff = (long)(couponDTO.DiscountAmount * 100),
                    Name = couponDTO.CouponCode,
                    Currency = "usd",
                    Id = couponDTO.CouponCode 
                };
                var service = new Stripe.CouponService();
                service.Create(options);

                _response.Result = mapper.Map<CouponDTO>(obj);
            }

            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }

            return _response;
        }


        [HttpPut]
        [Authorize(Roles = "ADMIN")]
        public ResponseDTO Put([FromBody] CouponDTO couponDTO)
        {
            try
            {

                Coupon obj = mapper.Map<Coupon>(couponDTO);

                dbContext.Coupons.Update(obj);
                dbContext.SaveChanges();

                _response.Result = mapper.Map<CouponDTO>(obj);
            }

            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }

            return _response;
        }


        [HttpDelete]
        [Route("{id:int}")]
        [Authorize(Roles = "ADMIN")]
        public ResponseDTO Delete(int id)
        {
            try
            {

                Coupon obj = dbContext.Coupons.FirstOrDefault(x => x.CouponId == id);

                dbContext.Coupons.Remove(obj);
                dbContext.SaveChanges();

                var service = new Stripe.CouponService();
                service.Delete(obj.CouponCode);

            }

            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }

            return _response;
        }
    }
}
