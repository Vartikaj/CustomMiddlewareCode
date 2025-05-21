using CustomMiddleWare.Interfaces;
using CustomMiddleWare.Models;
using CustomMiddleWare.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CustomMiddleWare.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IRegistration _registrationService;
        private readonly IConfiguration _configuration;
        public UserController(IRegistration registrationService, IConfiguration config)
        {
            _configuration = config;
            _registrationService = registrationService;
        }

        [Authorize]
        [HttpPost("RegisterUser")]
        public async Task<ResultModel<object>> registerUserData(RegistrationModel oRegistration)
        {
            ResultModel<object> result = new ResultModel<object>();
            try
            {
                if (ModelState.IsValid)
                {
                    return await _registrationService.Add(oRegistration);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }

        [AllowAnonymous]
        [HttpPost("LoginUser")]
        public async Task<ResultModel<RegistrationModel>> LoginUserData(LoginModel oLogin)
        {
            ResultModel<RegistrationModel> result = new ResultModel<RegistrationModel>();
            try
            {
                if (ModelState.IsValid)
                {
                    IActionResult response = Unauthorized();
                    var loginData = await _registrationService.LoginUser(oLogin);

                    if (loginData != null)
                    {
                        if (loginData.error)
                        {
                            result.success = true;
                            result.message = GenerateToken((RegistrationModel)loginData.LstModel[0]);
                            return result;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }

        private string GenerateToken(RegistrationModel loginData)
        {
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:key"]);

            var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature);
            var subject = new ClaimsIdentity(new[]
            {
                //new Claim(
                //    JwtRegisteredClaimNames.Email, loginData.id
                //),
                new Claim("id", loginData.id)
             });

            var expires = DateTime.UtcNow.AddDays(10);
            var tokenDescription = new SecurityTokenDescriptor
            {
                Subject = subject,
                Issuer = issuer,
                Expires = expires,
                Audience = audience,
                SigningCredentials = signingCredentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescription);
            var jwtToken = tokenHandler.WriteToken(token);

            return jwtToken;
        }
    }
}
