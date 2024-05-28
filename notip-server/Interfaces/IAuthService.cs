using notip_server.Dto;
using notip_server.Models;
using notip_server.ViewModel.Auth;

namespace notip_server.Interfaces
{
    public interface IAuthService
    {
        Task<AccessToken> Login(LoginRequest request);
        Task SignUp(SignUpRequest request);
    }
}
