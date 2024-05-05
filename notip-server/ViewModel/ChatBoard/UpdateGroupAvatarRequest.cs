namespace notip_server.ViewModel.ChatBoard
{
    public class UpdateGroupAvatarRequest
    {
        public string Code { get; set; }
        public List<IFormFile> Image{ get; set; }
    }
}
