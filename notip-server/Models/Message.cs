using System.ComponentModel.DataAnnotations.Schema;

namespace notip_server.Models
{
    public class Message
    {
        public long Id { get; set; }

        /// <summary>
        /// text
        /// media
        /// attachment
        /// </summary>
        public string Type { get; set; }

        public string GroupCode { get; set; }

        public string Content { get; set; }

        public string Path { get; set; }

        public DateTime Created { get; set; }

        public string CreatedBy { get; set; }

        public virtual User UserCreatedBy { get; set; }
        [ForeignKey("CreatedBy")]
        public virtual Group Group { get; set; }
    }
}
