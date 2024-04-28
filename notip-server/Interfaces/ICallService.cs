using notip_server.Dto;

namespace notip_server.Interfaces
{
    public interface ICallService
    {
        Task<List<GroupCallDto>> GetCallHistory(string userSession);
        Task<List<CallDto>> GetHistoryById(string userSession, string groupCallCode);
        Task<string> Call(string userSession, string callTo);
        Task JoinVideoCall(string userSession, string url);
        Task CancelVideoCall(string userSession, string url);

    }
}
