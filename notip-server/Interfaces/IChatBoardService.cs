using notip_server.Dto;

namespace notip_server.Interfaces
{
    public interface IChatBoardService
    {
        Task<List<GroupDto>> GetHistory(string userSession);
        Task<object> GetInfo(string userSession, string groupCode, string contactCode);
        Task AddGroup(string userCode, GroupDto group);
        Task<GroupDto> UpdateGroupAvatar(GroupDto group);
        Task SendMessage(string userCode, string groupCode, MessageDto message);
        Task<List<MessageDto>> GetMessageByGroup(string userCode, string groupCode);
        Task<List<MessageDto>> GetMessageByContact(string userCode, string contactCode);
    }
}
