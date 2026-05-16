using Microsoft.EntityFrameworkCore;
using QuickStock.Domain.ITassets;
using QuickStock.Domain.Messaging;
using QuickStock.Domain.Social;
using QuickStock.Domain.Locations;
using QuickStock.Domain.Shared;
using QuickStock.Domain.Accounts;
using QuickStock.Domain.Apparel;
using QuickStock.Domain.Library;
using QuickStock.Domain.Furniture;
using QuickStock.Domain.Consumables;

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
        public DbSet<Librarydata> LibraryBooks { get; set; }
        public DbSet<LibraryBookItem> LibraryBookItems { get; set; }
        public DbSet<Furniture> Furnitures { get; set; }
        public DbSet<ConsumableData> ConsumableList { get; set; }
        public DbSet<ConsumableItem> ConsumableItems { get; set; }
        public DbSet<ConsumableOutRequest> ConsumableOutRequests { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ... (Existing HasData and existing relations)

            // ConsumableList -> Campus
            modelBuilder.Entity<QuickStock.Domain.Consumables.ConsumableData>()
                .ToTable("ConsumableList")
                .HasOne<Campus>()
                .WithMany()
                .HasForeignKey(c => c.CampusId)
                .OnDelete(DeleteBehavior.Restrict);

            // ConsumableItem -> ConsumableData
            modelBuilder.Entity<QuickStock.Domain.Consumables.ConsumableItem>()
                .HasOne(i => i.ConsumableData)
                .WithMany(d => d.Items)
                .HasForeignKey(i => i.ConsumableDataId)
                .OnDelete(DeleteBehavior.Cascade);

            // ConsumableOutRequest -> ConsumableItem
            modelBuilder.Entity<QuickStock.Domain.Consumables.ConsumableOutRequest>()
                .HasOne(r => r.ConsumableItem)
                .WithMany()
                .HasForeignKey(r => r.ConsumableItemId)
                .OnDelete(DeleteBehavior.Cascade);

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

            // --- Database Relations Integrity ---

            // Room -> Campus (Restrict deletion if rooms exist)
            modelBuilder.Entity<Room>()
                .HasOne(r => r.Campus)
                .WithMany()
                .HasForeignKey(r => r.CampusId)
                .OnDelete(DeleteBehavior.Restrict);

            // ItAsset -> Campus (Restrict deletion if assets exist)
            modelBuilder.Entity<ItAsset>()
                .HasOne(a => a.Campus)
                .WithMany()
                .HasForeignKey(a => a.CampusId)
                .OnDelete(DeleteBehavior.Restrict);

            // ItAsset -> Room (Restrict deletion if assets exist)
            modelBuilder.Entity<ItAsset>()
                .HasOne(a => a.Room)
                .WithMany()
                .HasForeignKey(a => a.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            // Appareldata -> Campus (Restrict deletion if apparel exists)
            modelBuilder.Entity<Appareldata>()
                .HasOne(ap => ap.Campus)
                .WithMany()
                .HasForeignKey(ap => ap.CampusId)
                .OnDelete(DeleteBehavior.Restrict);

            // Librarydata -> Campus (Restrict deletion if books exist)
            modelBuilder.Entity<Librarydata>()
                .HasKey(l => l.ItemId);

            modelBuilder.Entity<Librarydata>()
                .HasOne(l => l.Campus)
                .WithMany()
                .HasForeignKey(l => l.CampusId)
                .OnDelete(DeleteBehavior.Restrict);

            // LibraryBookItem -> Librarydata
            modelBuilder.Entity<LibraryBookItem>()
                .HasOne(i => i.Book)
                .WithMany(b => b.Items)
                .HasForeignKey(i => i.LibrarydataId)
                .OnDelete(DeleteBehavior.Cascade);

            // LibraryBookItem -> Campus
            modelBuilder.Entity<LibraryBookItem>()
                .HasOne(i => i.Campus)
                .WithMany()
                .HasForeignKey(i => i.CampusId)
                .OnDelete(DeleteBehavior.Restrict);

            // Furniture -> Campus
            modelBuilder.Entity<Furniture>()
                .HasOne(f => f.Campus)
                .WithMany()
                .HasForeignKey(f => f.CampusId)
                .OnDelete(DeleteBehavior.Restrict);

            // Furniture -> Room
            modelBuilder.Entity<Furniture>()
                .HasOne(f => f.Room)
                .WithMany()
                .HasForeignKey(f => f.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(modelBuilder);

        }
    }
}
