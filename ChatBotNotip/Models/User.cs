using System;
using System.Collections.Generic;

namespace ChatBotNotip.Models
{
    public partial class User
    {
        public User()
        {
            Calls = new HashSet<Call>();
            GroupUsers = new HashSet<GroupUser>();
            LoginUserHistories = new HashSet<LoginUserHistory>();
            Messages = new HashSet<Message>();
            UserClaims = new HashSet<UserClaim>();
            UserLogins = new HashSet<UserLogin>();
            UserTokens = new HashSet<UserToken>();
            Roles = new HashSet<AspNetRole>();
        }

        public Guid Id { get; set; }
        public string PasswordSalt { get; set; }
        public string Dob { get; set; }
        public string Address { get; set; }
        public string Avatar { get; set; }
        public string Gender { get; set; }
        public DateTime? LastLogin { get; set; }
        public string CurrentSession { get; set; }
        public string UserName { get; set; }
        public string NormalizedUserName { get; set; }
        public string Email { get; set; }
        public string NormalizedEmail { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PasswordHash { get; set; }
        public string SecurityStamp { get; set; }
        public string ConcurrencyStamp { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public DateTime? LockoutEnd { get; set; }
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }

        public virtual ICollection<Call> Calls { get; set; }
        public virtual ICollection<GroupUser> GroupUsers { get; set; }
        public virtual ICollection<LoginUserHistory> LoginUserHistories { get; set; }
        public virtual ICollection<Message> Messages { get; set; }
        public virtual ICollection<UserClaim> UserClaims { get; set; }
        public virtual ICollection<UserLogin> UserLogins { get; set; }
        public virtual ICollection<UserToken> UserTokens { get; set; }

        public virtual ICollection<AspNetRole> Roles { get; set; }
    }
}
