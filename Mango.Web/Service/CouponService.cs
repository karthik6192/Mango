using Mango.Web.Models;
using Mango.Web.Service.IService;
using Mango.Web.Utility;

namespace Mango.Web.Service
{
    public class CouponService : ICouponService
    {
        private readonly IBaseService baseService;

        public CouponService(IBaseService baseService)
        {
            this.baseService = baseService;
        }

        public async Task<ResponseDTO> CreateCouponAsync(CouponDTO couponDTO)
        {
            return await baseService.SendAsync(new Models.RequestDTO
            {
                ApiType = Utility.SD.ApiType.POST,
                Data = couponDTO,
                Url = SD.CouponAPIBase + "/api/coupon" 
            });
        }

        public async Task<ResponseDTO> DeleteCouponAsync(int id)
        {
            return await baseService.SendAsync(new Models.RequestDTO
            {
                ApiType = Utility.SD.ApiType.DELETE,
                Url = SD.CouponAPIBase + "/api/coupon/" + id
            });
        }

        public async Task<ResponseDTO> GetAllCouponsAsync()
        {
            return await baseService.SendAsync(new Models.RequestDTO
            {
                ApiType = Utility.SD.ApiType.GET,
                Url = SD.CouponAPIBase + "/api/coupon"
            });
        }

        public async Task<ResponseDTO> GetCouponAsync(string couponCode)
        {
            return await baseService.SendAsync(new Models.RequestDTO
            {
                ApiType = Utility.SD.ApiType.GET,
                Url = SD.CouponAPIBase + "/api/coupon/GetByCode" + couponCode
            });
        }

        public async Task<ResponseDTO> GetCouponByIdAsync(int id)
        {
            return await baseService.SendAsync(new Models.RequestDTO
            {
                ApiType = Utility.SD.ApiType.GET,
                Url = SD.CouponAPIBase + "/api/coupon/" + id
            });
        }

        public async Task<ResponseDTO> UpdateCouponAsync(CouponDTO couponDTO)
        {
            return await baseService.SendAsync(new Models.RequestDTO
            {
                ApiType = Utility.SD.ApiType.PUT,
                Data = couponDTO,
                Url = SD.CouponAPIBase + "/api/coupon"
            });
        }
    }
}
