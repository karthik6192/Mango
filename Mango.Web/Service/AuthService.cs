using Mango.Web.Models;
using Mango.Web.Service.IService;
using Mango.Web.Utility;

namespace Mango.Web.Service
{
    public class AuthService : IAuthService
    {
        private readonly IBaseService baseService;

        public AuthService(IBaseService baseService)
        {
            this.baseService = baseService;
        }
        public async Task<ResponseDTO> AssignToRoleAsync(RegistrationRequestDTO registrationRequestDTO)
        {
            return await baseService.SendAsync(new Models.RequestDTO
            {
                ApiType = Utility.SD.ApiType.POST,
                Data = registrationRequestDTO,
                Url = SD.AuthAPIBase + "/api/auth/AssignRole"
            });
        }

        public async Task<ResponseDTO> LoginAsync(LoginRequestDTO loginRequestDTO)
        {
            return await baseService.SendAsync(new Models.RequestDTO
            {
                ApiType = Utility.SD.ApiType.POST,
                Data = loginRequestDTO,
                Url = SD.AuthAPIBase + "/api/auth/login"
            }, withBearer: false);
        }

        public async Task<ResponseDTO> RegisterAsync(RegistrationRequestDTO registrationRequestDTO)
        {
            return await baseService.SendAsync(new Models.RequestDTO
            {
                ApiType = Utility.SD.ApiType.POST,
                Data = registrationRequestDTO,
                Url = SD.AuthAPIBase + "/api/auth/register"
            }, withBearer: false);
        }
    }
}
