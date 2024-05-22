namespace notip_server.Interfaces
{
    public interface IFriendService
    {
        Task SendFriendRequest(string userSession, string receiverCode);

        Task AcceptFriendRequest(int requestId);

        Task CancelFriendRequest(int requestId);

        Task BlockUser(string userSession, string receiverCode);

        Task UnBlockUser(int requestId);
    }
}
