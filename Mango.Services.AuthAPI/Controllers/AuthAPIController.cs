using Mango.MessageBus;
using Mango.Services.AuthAPI.Models.DTO;
using Mango.Services.AuthAPI.Service.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Mango.Services.AuthAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthAPIController : ControllerBase
    {
        private readonly IAuthService authService;
        private readonly IMessageBus messageBus;
        private readonly IConfiguration configuration;
        protected ResponseDTO response;

        public AuthAPIController(IAuthService authService,
                                 IMessageBus messageBus,
                                 IConfiguration configuration)
        {
            this.authService = authService;
            this.messageBus = messageBus;
            this.configuration = configuration;
            response = new();

        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistrationRequestDTO model)
        {
            var errorMessage = await authService.Register(model);
            if(!string.IsNullOrEmpty(errorMessage))
            {
                response.IsSuccess = false;
                response.Message = errorMessage;
                return BadRequest(response);
            }
            await messageBus.PublishMessage(model.Email, configuration.GetValue<string>("TopicAndQueueNames:RegisterUserQueue"));
            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO model)
        {
            var loginResponse = await authService.Login(model);

            if(loginResponse.User == null)
            {
                response.IsSuccess = false;
                response.Message = "Username or Password Incorrect!";
                return BadRequest(response);
            }
            response.Result = loginResponse;
            return Ok(response);


        }

        [HttpPost("AssignRole")]
        public async Task<IActionResult> AssignRole([FromBody] RegistrationRequestDTO model)
        {
            var assignRoleSuccessful = await authService.AssignRole(model.Email,model.Role.ToUpper());

            if (!assignRoleSuccessful)
            {
                response.IsSuccess = false;
                response.Message = "Error encountered!";
                return BadRequest(response);
            }
            response.Result = assignRoleSuccessful;
            return Ok(response);


        }
    }
}
