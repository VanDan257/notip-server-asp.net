using Microsoft.EntityFrameworkCore;
using notip_server.Data.Configurations;
using notip_server.Models;

namespace notip_server.Data
{
    public class DbChatContext : DbContext
    {
        public DbChatContext(DbContextOptions<DbChatContext> options) : base(options) { }

        public DbSet<Call> Calls { get; set; }

        public DbSet<Contact> Contacts { get; set; }

        public DbSet<Group> Groups { get; set; }

        public DbSet<GroupCall> GroupCalls { get; set; }

        public DbSet<GroupUser> GroupUsers { get; set; }

        public DbSet<Message> Messages { get; set; }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //flutent api cho table Call
            modelBuilder.ApplyConfiguration(new CallConfiguration());

            //flutent api cho table Contact
            modelBuilder.ApplyConfiguration(new ContactConfiguration());

            //flutent api cho table Group
            modelBuilder.ApplyConfiguration(new GroupConfiguration());

            //flutent api cho table GroupCall
            modelBuilder.ApplyConfiguration(new GroupCallConfiguration());

            //flutent api cho table GroupUser
            modelBuilder.ApplyConfiguration(new GroupUserConfiguration());

            //flutent api cho table Message
            modelBuilder.ApplyConfiguration(new MessageConfiguration());

            //flutent api cho table User
            modelBuilder.ApplyConfiguration(new UserConfiguration());
        }
    }
}
