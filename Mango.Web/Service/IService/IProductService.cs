using Mango.Web.Models;
using Mango.Web.Models.DTO;

namespace Mango.Web.Service.IService
{
    public interface IProductService  
    {
        Task<ResponseDTO> GetAllProductsAsync();

        Task<ResponseDTO> GetProductByIdAsync(int id);

        Task<ResponseDTO> CreateProductAsync(ProductDTO couponDTO);

        Task<ResponseDTO> UpdateProductAsync(ProductDTO couponDTO);

        Task<ResponseDTO> DeleteProductAsync(int id);


    }
}
