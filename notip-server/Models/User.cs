namespace notip_server.Models
{
    public class User
    {
        public string Code { get; set; }

        public string PasswordHash { get; set; }

        public string PasswordSalt { get; set; }

        public string FullName { get; set; }

        public string? Dob { get; set; }

        public string? Phone { get; set; }

        public string Email { get; set; }

        public string? Address { get; set; }

        public string? Avatar { get; set; }

        public string? Gender { get; set; }

        public DateTime? LastLogin { get; set; }

        public string? CurrentSession { get; set; }

        public virtual ICollection<Call> Calls { get; set; }

        public virtual ICollection<Contact> Contacts { get; set; }

        public virtual ICollection<Contact> ContactUsers { get; set; }

        public virtual ICollection<GroupUser> GroupUsers { get; set; }

        public virtual ICollection<Message> Messages { get; set; }
    }
}
