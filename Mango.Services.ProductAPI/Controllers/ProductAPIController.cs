using AutoMapper;
using Mango.Services.ProductAPI.Data;
using Mango.Services.ProductAPI.Models;
using Mango.Services.ProductAPI.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Data;

namespace Mango.Services.ProductAPI.Controllers
{
    [Route("api/product")]
    [ApiController]
    public class ProductAPIController : ControllerBase
    {
        private readonly AppDbContext dbContext;
        private readonly IMapper mapper;
        private ResponseDTO _response;

        public ProductAPIController(AppDbContext dbContext, IMapper mapper)
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
                IEnumerable<Product> objList = dbContext.Products.ToList();
                _response.Result = mapper.Map<IEnumerable<ProductDTO>>(objList);
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
                Product obj = dbContext.Products.First(x => x.ProductId == id);
                _response.Result = mapper.Map<ProductDTO>(obj);
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

        public ResponseDTO Post([FromForm]  ProductDTO ProductDTO)
        {
            try
            {

                Product product = mapper.Map<Product>(ProductDTO);

                dbContext.Products.Add(product);
                dbContext.SaveChanges();

                if(ProductDTO.Image !=null)
                {
                    string fileName = product.ProductId + Path.GetExtension(ProductDTO.Image.FileName);
                    string filePath = @"wwwroot\ProductImages\" + fileName;
                    string filePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), filePath);

                    using(var fileStream = new FileStream(filePathDirectory,FileMode.Create))
                    {
                        ProductDTO.Image.CopyTo(fileStream);
                    }

                    var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";
                    product.ImageUrl = baseUrl + "/ProductImages/" + fileName; 
                    product.ImageLocalPath = filePath;

                }
                else
                {
                    product.ImageUrl = "https://placehold.co/600x400";
                }

                dbContext.Products.Update(product); 
                dbContext.SaveChanges();
                _response.Result = mapper.Map<ProductDTO>(product);
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
        public ResponseDTO Put([FromForm] ProductDTO ProductDTO)
        {
            try
            {

                Product product = mapper.Map<Product>(ProductDTO);

                if (ProductDTO.Image != null)
                {
                    //Need to delete the existing/old Image.
                    if (!string.IsNullOrEmpty(product.ImageLocalPath))
                    {
                        var oldFilePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), product.ImageLocalPath);
                        FileInfo file = new FileInfo(oldFilePathDirectory);

                        if (file.Exists)
                        {
                            file.Delete();
                        }
                    }

                    string fileName = product.ProductId + Path.GetExtension(ProductDTO.Image.FileName);
                    string filePath = @"wwwroot\ProductImages\" + fileName;
                    string filePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), filePath);

                    using (var fileStream = new FileStream(filePathDirectory, FileMode.Create))
                    {
                        ProductDTO.Image.CopyTo(fileStream);
                    }

                    var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";
                    product.ImageUrl = baseUrl + "/ProductImages/" + fileName;
                    product.ImageLocalPath = filePath;

                }

                dbContext.Products.Update(product);
                dbContext.SaveChanges();

                _response.Result = mapper.Map<ProductDTO>(product);
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

                Product obj = dbContext.Products.FirstOrDefault(x => x.ProductId == id);

                if(!string.IsNullOrEmpty(obj.ImageLocalPath))
                {
                    var oldFilePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), obj.ImageLocalPath);
                    FileInfo file = new FileInfo(oldFilePathDirectory);

                    if(file.Exists)
                    {
                        file.Delete();
                    }
                }

                dbContext.Products.Remove(obj);
                dbContext.SaveChanges();

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
