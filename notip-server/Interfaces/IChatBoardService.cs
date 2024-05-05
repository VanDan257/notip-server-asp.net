using notip_server.Dto;
using notip_server.ViewModel.ChatBoard;

namespace notip_server.Interfaces
{
    public interface IChatBoardService
    {
        Task<List<GroupDto>> GetHistory(string userSession);
        Task<List<GroupDto>> SearchChatGroup(string userCode, string keySearch);
        Task<List<MessageDto>> AccessChatGroup(string userCode, string groupCode);
        Task<object> GetInfo(string userSession, string groupCode);
        Task AddGroup(string userCode, AddGroupRequest request);
        Task AddMembersToGroup(AddMembersToGroupRequest request);
        Task UpdateGroupName(UpdateGroupNameRequest request);
        Task OutGroup(string userSession, string groupCode);
        Task UpdateGroupAvatar(UpdateGroupAvatarRequest request);
        Task SendMessage(string userCode, string groupCode, MessageDto message);
        Task<List<MessageDto>> GetMessageByGroup(string userCode, string groupCode);
        Task<List<MessageDto>> GetMessageByContact(string userCode, string contactCode);
    }
}
