using CustomMiddleWare.Interfaces;
using CustomMiddleWare.Middlewares;
using CustomMiddleWare.Models;
using CustomMiddleWare.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CustomMiddleWare.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IRegistration _registrationService;
        private readonly IConfiguration _configuration;
        private readonly CacheService _cacheService;
        public UserController(IRegistration registrationService, IConfiguration config, CacheService cacheService)
        {
            _cacheService = cacheService;
            _configuration = config;
            _registrationService = registrationService;
        }

        
        [HttpPost("RegisterUser")]
        public async Task<ResultModel<object>> registerUserData(RegistrationModel oRegistration)
        {
            ResultModel<object> result = new ResultModel<object>();
            try
            {
                if (ModelState.IsValid)
                {
                    var registrationData = await _registrationService.Add(oRegistration);

                    if(registrationData != null)
                    {
                        result.success = true;
                        result.message = registrationData.message;
                    }
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
        public async Task<ResultModel<AccessTokenDetails>> LoginUserData(LoginModel oLogin)
        {
            ResultModel<AccessTokenDetails> result = new ResultModel<AccessTokenDetails>();
            try
            {
                if (ModelState.IsValid) 
                {
                    IActionResult response = Unauthorized();
                    var loginData = await _registrationService.LoginUser(oLogin);

                    if (loginData != null)
                    {
                        AccessTokenDetails oAccessToken = await GenerateToken("http://localhost:5183/connect/token", (RegistrationModel)loginData.LstModel[0]);
                        await _cacheService.SetAsync("User_token_" + oLogin.firstname, oAccessToken, TimeSpan.FromMinutes(5));
                        if (loginData.error)
                        {
                            result.success = true;
                            result.data = oAccessToken;
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

        private async static Task<AccessTokenDetails> GenerateToken(string IdentityServer, RegistrationModel loginData)
        {
            HttpClient client = new HttpClient();
            string data = JsonConvert.SerializeObject(loginData);
            var values = new Dictionary<string, string> {
                { "client_id", "mvc" },
                { "client_secret", "secret" },
                { "grant_type", "password" },
                { "username", "username" },
                { "password", "password" },
                { "userdata", JsonConvert.SerializeObject(loginData) } // or plain string
            };
            var content = new FormUrlEncodedContent(values);
            var response = await client.PostAsync(IdentityServer, content);
            var responseString = await response.Content.ReadAsStringAsync();
            AccessTokenDetails oAccessTokenDetails = JsonConvert.DeserializeObject<AccessTokenDetails>(responseString);
            return oAccessTokenDetails;
        }
    }
}
