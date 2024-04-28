using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using notip_server.Models;

namespace notip_server.Data.Configurations
{
    public class ContactConfiguration : IEntityTypeConfiguration<Contact>
    {
        public void Configure(EntityTypeBuilder<Contact> builder)
        {
            builder.ToTable("Contact");

            builder.Property(e => e.ContactCode)
                .HasMaxLength(32)
                .IsUnicode(false);
            builder.Property(e => e.Created).HasColumnType("datetime");
            builder.Property(e => e.UserCode)
                .HasMaxLength(32)
                .IsUnicode(false);

            builder.HasOne(d => d.UserContact).WithMany(p => p.Contacts)
                .HasForeignKey(d => d.ContactCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Contact_User1");

            builder.HasOne(d => d.User).WithMany(p => p.ContactUsers)
                .HasForeignKey(d => d.UserCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Contact_User");
        }
    }
}
