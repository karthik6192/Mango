using Mango.Web.Models;
using Mango.Web.Service.IService;
using Mango.Web.Utility;

namespace Mango.Web.Service
{
    public class OrderService : IOrderService
    {
        private readonly IBaseService baseService;

        public OrderService(IBaseService baseService)
        {
            this.baseService = baseService;
        }


        public async Task<ResponseDTO>? CreateOrder(CartDTO cartDTO)
        {

            return await baseService.SendAsync(new Models.RequestDTO
            {
                ApiType = Utility.SD.ApiType.POST,
                Data = cartDTO,
                Url = SD.OrderAPIBase + "/api/order/CreateOrder"
            });
        }

        public async Task<ResponseDTO>? CreateStripeSession(StripeRequestDTO stripeRequestDTO)
        {
            return await baseService.SendAsync(new Models.RequestDTO
            {
                ApiType = Utility.SD.ApiType.POST,
                Data = stripeRequestDTO,
                Url = SD.OrderAPIBase + "/api/order/CreateStripeSession"
            });
        }

        public async Task<ResponseDTO>? GetAllOrder(string? userId)
        {
            return await baseService.SendAsync(new Models.RequestDTO
            {
                ApiType = Utility.SD.ApiType.GET,
                Url = SD.OrderAPIBase + "/api/order/GetOrders/" + userId
            });
        }

        public async Task<ResponseDTO>? GetOrder(int orderId)
        {
            return await baseService.SendAsync(new Models.RequestDTO
            {
                ApiType = Utility.SD.ApiType.GET,
                Url = SD.OrderAPIBase + "/api/order/GetOrder/" + orderId
            });
        }

        public async Task<ResponseDTO> UpdateOrderStatus(int orderId, string newStatus)
        {
            return await baseService.SendAsync(new Models.RequestDTO
            {
                ApiType = Utility.SD.ApiType.POST,
                Data = newStatus,
                Url = SD.OrderAPIBase + "/api/order/UpdateOrderStatus/"+ orderId
            });
        }

        public async Task<ResponseDTO>? ValidateStripeSession(int orderHeaderId)
        {
            return await baseService.SendAsync(new Models.RequestDTO
            {
                ApiType = Utility.SD.ApiType.POST,
                Data = orderHeaderId,
                Url = SD.OrderAPIBase + "/api/order/ValidateStripeSession"
            });
        }
    }
}
