using Mango.Services.AuthAPI.Data;
using Mango.Services.AuthAPI.Models;
using Mango.Services.AuthAPI.Models.DTO;
using Mango.Services.AuthAPI.Service.IService;
using Microsoft.AspNetCore.Identity;

namespace Mango.Services.AuthAPI.Service
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext dbContext;

        private readonly UserManager<ApplicationUser> userManager;
        private readonly IJWTTokenGenerator jWTTokenGenerator;
        private readonly RoleManager<IdentityRole> roleManager;



        public AuthService(AppDbContext dbContext, 
                           RoleManager<IdentityRole> roleManager,
                           UserManager<ApplicationUser> userManager,
                           IJWTTokenGenerator jWTTokenGenerator)
        {
            this.dbContext = dbContext;
            this.roleManager = roleManager;
            this.userManager = userManager;
            this.jWTTokenGenerator = jWTTokenGenerator;
        }

        public async Task<bool> AssignRole(string email, string roleName)
        {
            var user = dbContext.ApplicationUsers.FirstOrDefault(u => u.Email.ToLower() == email.ToLower());

            if(user !=null)
            {
                if(!roleManager.RoleExistsAsync(roleName).GetAwaiter().GetResult())
                {
                    // create role if not exists.
                    roleManager.CreateAsync(new IdentityRole(roleName)).GetAwaiter().GetResult();
                }

                await userManager.AddToRoleAsync(user, roleName);
                return true;
            }

            return false;
        }

        public async Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO)
        {
            var user = dbContext.ApplicationUsers.FirstOrDefault(u => u.UserName.ToLower() == loginRequestDTO.UserName.ToLower());

            bool isValid = await userManager.CheckPasswordAsync(user, loginRequestDTO.Password);

            if(user == null || isValid == false)
            {
                return new LoginResponseDTO { User = null, Token = "" };
            }

            // if user was found then generate JWT Token resp.

            var roles = await userManager.GetRolesAsync(user);
            var token = jWTTokenGenerator.GenerateToken(user, roles);
            UserDTO userDTO = new()
            {
                Email = user.Email,
                ID = user.Id,
                Name = user.Name,
                PhoneNumber = user.PhoneNumber
            };

            LoginResponseDTO loginResponseDTO = new()
            {
                User = userDTO,
                Token = token
            };

            return loginResponseDTO;
        }

        public async Task<string> Register(RegistrationRequestDTO registrationRequestDTO)
        {
            ApplicationUser user = new()
            {
                UserName = registrationRequestDTO.Email,
                Email = registrationRequestDTO.Email,
                NormalizedEmail = registrationRequestDTO.Email.ToUpper(),
                Name = registrationRequestDTO.Name,
                PhoneNumber = registrationRequestDTO.PhoneNumber
            };

            try
            {
                var result = await userManager.CreateAsync(user, registrationRequestDTO.Password);

                if(result.Succeeded) 
                {
                    var userToReturn = dbContext.ApplicationUsers.FirstOrDefault(u => u.UserName == registrationRequestDTO.Email);

                    UserDTO userDTO = new()
                    {
                        Email = userToReturn.Email,
                        ID = userToReturn.Id,
                        Name = userToReturn.Name,
                        PhoneNumber = userToReturn.PhoneNumber
                    };

                    return "";
                
                }
                else
                {
                    return result.Errors.FirstOrDefault().Description;
                }
            }

            catch(Exception ex) { 
                
            
            }

            return "Error Encountered";
        }
    }
}
