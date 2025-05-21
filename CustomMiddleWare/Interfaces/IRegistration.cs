using CustomMiddleWare.Models;

namespace CustomMiddleWare.Interfaces
{
    public interface IRegistration
    {
        Task<ResultModel<object>> Add(RegistrationModel model);
        Task<ResultModel<RegistrationModel>> LoginUser(LoginModel loginModel);
        Task<ResultModel<RegistrationModel>> GetHashCode(string email);
    }
}
