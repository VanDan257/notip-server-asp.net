using notip_server.Dto;
using notip_server.ViewModel.User;

namespace notip_server.Interfaces
{
    public interface IUserService
    {
        Task<UserDto> GetProfile(string userCode);
        Task<UserDto> UpdateProfile(string userCode, UpdateProfileRequest request);
        Task<List<UserDto>> GetContact(string userCode);
        Task<List<UserDto>> SearchContact(string userCode, string keySearch);
        Task AddContact(string userCode, UserDto user);
    }
}
