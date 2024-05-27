using notip_server.ViewModel.Common;
using notip_server.ViewModel.Friend;
using notip_server.ViewModel.User;

namespace notip_server.Interfaces
{
    public interface IFriendService
    {
        Task<PagingResult<FriendResponse>> GetListFriend(string userSession, GetContactRequest request);

        Task<PagingResult<FriendResponse>> GetListFriendInvite(string userSession, GetContactRequest request);

        Task SendFriendRequest(string userSession, string receiverCode);

        Task AcceptFriendRequest(string userSession, string receiverCode);

        Task CancelFriendRequest(string userSession, string receiverCode);

        Task BlockUser(string userSession, string receiverCode);

        Task UnBlockUser(string userSession, string receiverCode);
    }
}
