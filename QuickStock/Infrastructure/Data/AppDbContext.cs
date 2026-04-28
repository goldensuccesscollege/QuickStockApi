using Microsoft.EntityFrameworkCore;
using QuickStock.Domain.ITassets;
using QuickStock.Domain.Apparel;

namespace QuickStock.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
           : base(options)
        {
        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<ChatGroup> ChatGroups { get; set; }
        public DbSet<ChatGroupMember> ChatGroupMembers { get; set; }
        public DbSet<ProfilePost> ProfilePosts { get; set; }
        public DbSet<PostImage> PostImages { get; set; }
        public DbSet<PostComment> PostComments { get; set; }
        public DbSet<PostReaction> PostReactions { get; set; }
        public DbSet<ItAsset> Itassets { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Campus> Campuses { get; set; }
        public DbSet<AccountCampus> AccountCampuses { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<StoredImage> StoredImages { get; set; }
        public DbSet<Appareldata> ApparelList { get; set; }
        public DbSet<ApparelItem> ApparelItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Campus>().HasData(
                new Campus { CampusId = 1, Name = "Cebu Campus", Address = "Cebu City", Description = "Main Campus" },
                new Campus { CampusId = 2, Name = "Manila Campus", Address = "Metro Manila", Description = "Luzon Branch" }
            );

            modelBuilder.Entity<Room>().HasData(
                new Room { RoomId = 1, CampusId = 1, RoomName = "Room 403", RoomFloor = "4th Floor", RoomDescription = "General IT Office" },
                new Room { RoomId = 2, CampusId = 1, RoomName = "IT Lab", RoomFloor = "2nd Floor", RoomDescription = "Hardware Testing and Maintenance" },
                new Room { RoomId = 3, CampusId = 1, RoomName = "Server Room 1", RoomFloor = "Basement", RoomDescription = "Critical Infrastructure" }
            );

            modelBuilder.Entity<AccountCampus>()
                .HasOne(ac => ac.Account)
                .WithMany(a => a.AccountCampuses)
                .HasForeignKey(ac => ac.AccountId);

            modelBuilder.Entity<AccountCampus>()
                .HasOne(ac => ac.Campus)
                .WithMany()
                .HasForeignKey(ac => ac.CampusId);

            modelBuilder.Entity<ProfilePost>()
                .HasMany(p => p.Images)
                .WithOne(i => i.Post)
                .HasForeignKey(i => i.PostId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Account>()
                .HasOne(a => a.Profile)
                .WithOne(p => p.Account)
                .HasForeignKey<Profile>(p => p.AccountId);

            modelBuilder.Entity<ChatMessage>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderAccountId);

            modelBuilder.Entity<ChatGroup>()
                .HasOne(g => g.CreatedBy)
                .WithMany()
                .HasForeignKey(g => g.CreatedByAccountId);

            modelBuilder.Entity<ChatGroupMember>()
                .HasOne(gm => gm.Group)
                .WithMany(g => g.Members)
                .HasForeignKey(gm => gm.ChatGroupId);

            modelBuilder.Entity<ChatGroupMember>()
                .HasOne(gm => gm.Account)
                .WithMany()
                .HasForeignKey(gm => gm.AccountId);

            modelBuilder.Entity<PostComment>(entity =>
            {
                entity.HasOne(c => c.Post)
                    .WithMany(p => p.Comments)
                    .HasForeignKey(c => c.PostId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.Author)
                    .WithMany()
                    .HasForeignKey(c => c.AuthorAccountId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PostReaction>(entity =>
            {
                entity.HasOne(r => r.Post)
                    .WithMany(p => p.Reactions)
                    .HasForeignKey(r => r.PostId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.Author)
                    .WithMany()
                    .HasForeignKey(r => r.AuthorAccountId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
