using notip_server.ViewModel.Common;

namespace notip_server.ViewModel.User
{
    public class GetContactRequest : PagingRequest
    {
        public string? UserCode { get; set; }
        public string? KeySearch { get; set; }
    }
}
