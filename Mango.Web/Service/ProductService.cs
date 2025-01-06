using Mango.Web.Models;
using Mango.Web.Models.DTO;
using Mango.Web.Service.IService;
using Mango.Web.Utility;

namespace Mango.Web.Service
{
    public class ProductService : IProductService
    {
        private readonly IBaseService baseService;

        public ProductService(IBaseService baseService)
        {
            this.baseService = baseService;
        }

        public async Task<ResponseDTO> CreateProductAsync(ProductDTO ProductDTO)
        {
            return await baseService.SendAsync(new Models.RequestDTO
            {
                ApiType = Utility.SD.ApiType.POST,
                Data = ProductDTO,
                Url = SD.ProductAPIBase + "/api/product",
                ContentType = SD.ContentType.MultipartFormData
            });
        }

        public async Task<ResponseDTO> DeleteProductAsync(int id)
        {
            return await baseService.SendAsync(new Models.RequestDTO
            {
                ApiType = Utility.SD.ApiType.DELETE,
                Url = SD.ProductAPIBase + "/api/product/" + id
            });
        }

        public async Task<ResponseDTO> GetAllProductsAsync()
        {
            return await baseService.SendAsync(new Models.RequestDTO
            {
                ApiType = Utility.SD.ApiType.GET,
                Url = SD.ProductAPIBase + "/api/product"
            });
        }

        public async Task<ResponseDTO> GetProductByIdAsync(int id)
        {
            return await baseService.SendAsync(new Models.RequestDTO
            {
                ApiType = Utility.SD.ApiType.GET,
                Url = SD.ProductAPIBase + "/api/product/" + id
            });
        }

        public async Task<ResponseDTO> UpdateProductAsync(ProductDTO ProductDTO)
        {
            return await baseService.SendAsync(new Models.RequestDTO
            {
                ApiType = Utility.SD.ApiType.PUT,
                Data = ProductDTO,
                Url = SD.ProductAPIBase + "/api/product",
                ContentType = SD.ContentType.MultipartFormData
            });
        }
    }
}
