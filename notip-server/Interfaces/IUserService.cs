using notip_server.Dto;
using notip_server.ViewModel.Common;
using notip_server.ViewModel.Friend;
using notip_server.ViewModel.User;

namespace notip_server.Interfaces
{
    public interface IUserService
    {
        Task<UserDto> GetProfile(string userCode);
        Task<UserDto> UpdateProfile(string userCode, UpdateProfileRequest request);
        Task AddContact(string userCode, UserDto user);
        Task<PagingResult<FriendResponse>> GetContact(string userSession, GetContactRequest request);
    }
}
