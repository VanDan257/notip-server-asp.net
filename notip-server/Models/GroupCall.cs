namespace notip_server.Models
{
    public class GroupCall
    {
        public string Code { get; set; }

        /// <summary>
        /// single: chat 1-1
        /// multi: chat 1-n
        /// </summary>
        public string Type { get; set; }

        public string Avatar { get; set; }

        public string Name { get; set; }

        public DateTime Created { get; set; }

        public string CreatedBy { get; set; }

        public DateTime LastActive { get; set; }

        public virtual ICollection<Call> Calls { get; set; }
    }
}
