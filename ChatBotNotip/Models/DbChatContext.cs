using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ChatBotNotip.Models
{
    public partial class DbChatContext : DbContext
    {
        public DbChatContext()
        {
        }

        public DbChatContext(DbContextOptions<DbChatContext> options)
            : base(options)
        {
        }

        public virtual DbSet<AspNetRole> AspNetRoles { get; set; }
        public virtual DbSet<AttributesShop> AttributesShops { get; set; }
        public virtual DbSet<Call> Calls { get; set; }
        public virtual DbSet<EfmigrationsHistory> EfmigrationsHistories { get; set; }
        public virtual DbSet<Friend> Friends { get; set; }
        public virtual DbSet<Group> Groups { get; set; }
        public virtual DbSet<GroupCall> GroupCalls { get; set; }
        public virtual DbSet<GroupUser> GroupUsers { get; set; }
        public virtual DbSet<LoginUserHistory> LoginUserHistories { get; set; }
        public virtual DbSet<Message> Messages { get; set; }
        public virtual DbSet<RoleClaim> RoleClaims { get; set; }
        public virtual DbSet<Shop> Shops { get; set; }
        public virtual DbSet<TrafficStatisticsResult> TrafficStatisticsResults { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserClaim> UserClaims { get; set; }
        public virtual DbSet<UserLogin> UserLogins { get; set; }
        public virtual DbSet<UserToken> UserTokens { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // #warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseMySql("server=127.0.0.1;database=notipchatdb;user=root;password=vandan25072002;port=3306;allowpublickeyretrieval=True;sslmode=none", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.4.0-mysql"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseCollation("utf8mb4_0900_ai_ci")
                .HasCharSet("utf8mb4");

            modelBuilder.Entity<AspNetRole>(entity =>
            {
                entity.HasIndex(e => e.NormalizedName, "RoleNameIndex")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .UseCollation("ascii_general_ci")
                    .HasCharSet("ascii");

                entity.Property(e => e.Name).HasMaxLength(256);

                entity.Property(e => e.NormalizedName).HasMaxLength(256);
            });

            modelBuilder.Entity<AttributesShop>(entity =>
            {
                entity.HasIndex(e => e.ShopId, "IX_AttributesShops_ShopId");

                entity.Property(e => e.Name).IsRequired();

                entity.Property(e => e.ShopId)
                    .UseCollation("ascii_general_ci")
                    .HasCharSet("ascii");

                entity.Property(e => e.Value).IsRequired();

                entity.HasOne(d => d.Shop)
                    .WithMany(p => p.AttributesShops)
                    .HasForeignKey(d => d.ShopId);
            });

            modelBuilder.Entity<Call>(entity =>
            {
                entity.ToTable("Call");

                entity.HasIndex(e => e.GroupCallCode, "IX_Call_GroupCallCode");

                entity.HasIndex(e => e.UserCode, "IX_Call_UserCode");

                entity.Property(e => e.Created).HasColumnType("datetime");

                entity.Property(e => e.GroupCallCode)
                    .UseCollation("ascii_general_ci")
                    .HasCharSet("ascii");

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(36);

                entity.Property(e => e.Url)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.UserCode)
                    .UseCollation("ascii_general_ci")
                    .HasCharSet("ascii");

                entity.HasOne(d => d.GroupCallCodeNavigation)
                    .WithMany(p => p.Calls)
                    .HasForeignKey(d => d.GroupCallCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Call_GroupCall");

                entity.HasOne(d => d.UserCodeNavigation)
                    .WithMany(p => p.Calls)
                    .HasForeignKey(d => d.UserCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Call_User");
            });

            modelBuilder.Entity<EfmigrationsHistory>(entity =>
            {
                entity.HasKey(e => e.MigrationId)
                    .HasName("PRIMARY");

                entity.ToTable("__EFMigrationsHistory");

                entity.Property(e => e.MigrationId).HasMaxLength(150);

                entity.Property(e => e.ProductVersion)
                    .IsRequired()
                    .HasMaxLength(32);
            });

            modelBuilder.Entity<Friend>(entity =>
            {
                entity.ToTable("Friend");

                entity.Property(e => e.ReceiverCode)
                    .UseCollation("ascii_general_ci")
                    .HasCharSet("ascii");

                entity.Property(e => e.SenderCode)
                    .UseCollation("ascii_general_ci")
                    .HasCharSet("ascii");

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(36);
            });

            modelBuilder.Entity<Group>(entity =>
            {
                entity.HasKey(e => e.Code)
                    .HasName("PRIMARY");

                entity.ToTable("Group");

                entity.Property(e => e.Code)
                    .UseCollation("ascii_general_ci")
                    .HasCharSet("ascii");

                entity.Property(e => e.Created).HasColumnType("datetime");

                entity.Property(e => e.CreatedBy)
                    .UseCollation("ascii_general_ci")
                    .HasCharSet("ascii");

                entity.Property(e => e.LastActive).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(250);

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasComment("single: chat 1-1\r\nmulti: chat 1-n");
            });

            modelBuilder.Entity<GroupCall>(entity =>
            {
                entity.HasKey(e => e.Code)
                    .HasName("PRIMARY");

                entity.ToTable("GroupCall");

                entity.Property(e => e.Code)
                    .UseCollation("ascii_general_ci")
                    .HasCharSet("ascii");

                entity.Property(e => e.Avatar).IsRequired();

                entity.Property(e => e.Created).HasColumnType("datetime");

                entity.Property(e => e.CreatedBy)
                    .UseCollation("ascii_general_ci")
                    .HasCharSet("ascii");

                entity.Property(e => e.LastActive).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasComment("single: chat 1-1\r\nmulti: chat 1-n");
            });

            modelBuilder.Entity<GroupUser>(entity =>
            {
                entity.ToTable("GroupUser");

                entity.HasIndex(e => e.GroupCode, "IX_GroupUser_GroupCode");

                entity.HasIndex(e => e.UserCode, "IX_GroupUser_UserCode");

                entity.Property(e => e.GroupCode)
                    .UseCollation("ascii_general_ci")
                    .HasCharSet("ascii");

                entity.Property(e => e.UserCode)
                    .UseCollation("ascii_general_ci")
                    .HasCharSet("ascii");

                entity.HasOne(d => d.GroupCodeNavigation)
                    .WithMany(p => p.GroupUsers)
                    .HasForeignKey(d => d.GroupCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_GroupUser_Group");

                entity.HasOne(d => d.UserCodeNavigation)
                    .WithMany(p => p.GroupUsers)
                    .HasForeignKey(d => d.UserCode)
                    .HasConstraintName("FK_GroupUser_User");
            });

            modelBuilder.Entity<LoginUserHistory>(entity =>
            {
                entity.HasIndex(e => e.UserId, "IX_LoginUserHistories_UserId");

                entity.Property(e => e.LoginTime).HasMaxLength(6);

                entity.Property(e => e.UserId)
                    .UseCollation("ascii_general_ci")
                    .HasCharSet("ascii");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.LoginUserHistories)
                    .HasForeignKey(d => d.UserId);
            });

            modelBuilder.Entity<Message>(entity =>
            {
                entity.ToTable("Message");

                entity.HasIndex(e => e.CreatedBy, "IX_Message_CreatedBy");

                entity.HasIndex(e => e.GroupCode, "IX_Message_GroupCode");

                entity.Property(e => e.Content).IsRequired();

                entity.Property(e => e.Created).HasColumnType("datetime");

                entity.Property(e => e.CreatedBy)
                    .UseCollation("ascii_general_ci")
                    .HasCharSet("ascii");

                entity.Property(e => e.GroupCode)
                    .UseCollation("ascii_general_ci")
                    .HasCharSet("ascii");

                entity.Property(e => e.Path).HasMaxLength(255);

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasComment("text\r\nmedia\r\nattachment");

                entity.HasOne(d => d.CreatedByNavigation)
                    .WithMany(p => p.Messages)
                    .HasForeignKey(d => d.CreatedBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Message_User");

                entity.HasOne(d => d.GroupCodeNavigation)
                    .WithMany(p => p.Messages)
                    .HasForeignKey(d => d.GroupCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Message_Group");
            });

            modelBuilder.Entity<RoleClaim>(entity =>
            {
                entity.HasIndex(e => e.RoleId, "IX_RoleClaims_RoleId");

                entity.Property(e => e.RoleId)
                    .UseCollation("ascii_general_ci")
                    .HasCharSet("ascii");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.RoleClaims)
                    .HasForeignKey(d => d.RoleId);
            });

            modelBuilder.Entity<Shop>(entity =>
            {
                entity.Property(e => e.Id)
                    .UseCollation("ascii_general_ci")
                    .HasCharSet("ascii");

                entity.Property(e => e.Created).HasMaxLength(6);

                entity.Property(e => e.Description).IsRequired();

                entity.Property(e => e.Name).IsRequired();

                entity.Property(e => e.Updated).HasMaxLength(6);

                entity.Property(e => e.UserId)
                    .UseCollation("ascii_general_ci")
                    .HasCharSet("ascii");
            });

            modelBuilder.Entity<TrafficStatisticsResult>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("TrafficStatisticsResult");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User");

                entity.HasIndex(e => e.NormalizedEmail, "EmailIndex");

                entity.HasIndex(e => e.NormalizedUserName, "UserNameIndex")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .UseCollation("ascii_general_ci")
                    .HasCharSet("ascii");

                entity.Property(e => e.Address).HasMaxLength(255);

                entity.Property(e => e.CurrentSession).HasMaxLength(500);

                entity.Property(e => e.Dob).HasMaxLength(50);

                entity.Property(e => e.Email).HasMaxLength(50);

                entity.Property(e => e.Gender).HasMaxLength(10);

                entity.Property(e => e.LastLogin).HasColumnType("datetime");

                entity.Property(e => e.LockoutEnd).HasMaxLength(6);

                entity.Property(e => e.NormalizedEmail).HasMaxLength(256);

                entity.Property(e => e.NormalizedUserName).HasMaxLength(256);

                entity.Property(e => e.PasswordHash).HasMaxLength(255);

                entity.Property(e => e.PasswordSalt)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.PhoneNumber).HasMaxLength(50);

                entity.Property(e => e.UserName).HasMaxLength(50);

                entity.HasMany(d => d.Roles)
                    .WithMany(p => p.Users)
                    .UsingEntity<Dictionary<string, object>>(
                        "UserRole",
                        l => l.HasOne<AspNetRole>().WithMany().HasForeignKey("RoleId"),
                        r => r.HasOne<User>().WithMany().HasForeignKey("UserId"),
                        j =>
                        {
                            j.HasKey("UserId", "RoleId").HasName("PRIMARY").HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

                            j.ToTable("UserRoles");

                            j.HasIndex(new[] { "RoleId" }, "IX_UserRoles_RoleId");

                            j.IndexerProperty<Guid>("UserId").UseCollation("ascii_general_ci").HasCharSet("ascii");

                            j.IndexerProperty<Guid>("RoleId").UseCollation("ascii_general_ci").HasCharSet("ascii");
                        });
            });

            modelBuilder.Entity<UserClaim>(entity =>
            {
                entity.HasIndex(e => e.UserId, "IX_UserClaims_UserId");

                entity.Property(e => e.UserId)
                    .UseCollation("ascii_general_ci")
                    .HasCharSet("ascii");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserClaims)
                    .HasForeignKey(d => d.UserId);
            });

            modelBuilder.Entity<UserLogin>(entity =>
            {
                entity.HasKey(e => new { e.LoginProvider, e.ProviderKey })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

                entity.HasIndex(e => e.UserId, "IX_UserLogins_UserId");

                entity.Property(e => e.UserId)
                    .UseCollation("ascii_general_ci")
                    .HasCharSet("ascii");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserLogins)
                    .HasForeignKey(d => d.UserId);
            });

            modelBuilder.Entity<UserToken>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0, 0 });

                entity.Property(e => e.UserId)
                    .UseCollation("ascii_general_ci")
                    .HasCharSet("ascii");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserTokens)
                    .HasForeignKey(d => d.UserId);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
