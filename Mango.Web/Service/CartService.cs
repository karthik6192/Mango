using Mango.Web.Models;
using Mango.Web.Service.IService;
using Mango.Web.Utility;

namespace Mango.Web.Service
{
    public class CartService : ICartService
    {
        private readonly IBaseService baseService;

        public CartService(IBaseService baseService)
        {
            this.baseService = baseService;
        }

        public async Task<ResponseDTO> ApplyCouponAsync(CartDTO cartDTO)
        {
            return await baseService.SendAsync(new Models.RequestDTO
            {
                ApiType = Utility.SD.ApiType.POST,
                Data = cartDTO,
                Url = SD.ShoppingCartAPIBase + "/api/cart/ApplyCoupon"
            });
        }

       

        public async Task<ResponseDTO> GetCartByUserIdAsync(string userId)
        {
            return await baseService.SendAsync(new Models.RequestDTO
            {
                ApiType = Utility.SD.ApiType.GET,
                Url = SD.ShoppingCartAPIBase + "/api/cart/GetCart/" + userId
            });
        }

        public async Task<ResponseDTO> RemoveFromCartAsync(int cartDetailsId)
        {
            return await baseService.SendAsync(new Models.RequestDTO
            {
                ApiType = Utility.SD.ApiType.POST,
                Data = cartDetailsId,
                Url = SD.ShoppingCartAPIBase + "/api/cart/RemoveCart"
            });
        }

        public async Task<ResponseDTO> UpsertCartAsync(CartDTO cartDTO)
        {
            return await baseService.SendAsync(new Models.RequestDTO
            {
                ApiType = Utility.SD.ApiType.POST,
                Data = cartDTO,
                Url = SD.ShoppingCartAPIBase + "/api/cart/CartUpsert"
            });
        }

        public async Task<ResponseDTO> EmailCart(CartDTO cartDTO)
        {
            return await baseService.SendAsync(new Models.RequestDTO
            {
                ApiType = Utility.SD.ApiType.POST,
                Data = cartDTO,
                Url = SD.ShoppingCartAPIBase + "/api/cart/EmailCartRequest"
            });
        }
    }
}
