namespace notip_server.Models
{
    public class Contact
    {
        public long Id { get; set; }

        public string UserCode { get; set; }

        public string ContactCode { get; set; }

        public DateTime Created { get; set; }

        public virtual User UserContact { get; set; }

        public virtual User User { get; set; }
    }
}
