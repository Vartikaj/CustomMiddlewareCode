using CustomMiddleWare.Interfaces;
using CustomMiddleWare.Models;
using CustomMiddleWare.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CustomMiddleWare.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IRegistration _registrationService;
        public UserController(IRegistration registrationService)
        {
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
                    return await _registrationService.Add(oRegistration);
                }

            } catch(Exception ex)
            {
                throw ex;
            }
            
            return result;
        }

        [HttpPost("LoginUser")]
        public async Task<ResultModel<object>> LoginUserData(LoginModel oLogin)
        {
            ResultModel<object> result = new ResultModel<object>();
            try
            {
                if (ModelState.IsValid)
                {
                    return await _registrationService.LoginUser(oLogin);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }
    }


}
