using notip_server.Dto;

namespace notip_server.ViewModel.ChatBoard
{
    public class AddMembersToGroupRequest
    {
        public string Code { get; set; }
        public List<UserDto> Users { get; set; }
    }
}
