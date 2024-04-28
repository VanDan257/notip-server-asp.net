using notip_server.Models;

namespace notip_server.Dto
{
    public class GroupDto
    {
        public string Code { get; set; }

        public string Type { get; set; }

        public string Avatar { get; set; }

        public string Name { get; set; }

        public DateTime Created { get; set; }

        public string CreatedBy { get; set; }

        public DateTime LastActive { get; set; }

        public List<UserDto> Users { get; set; }

        public MessageDto LastMessage { get; set; }
    }
}
