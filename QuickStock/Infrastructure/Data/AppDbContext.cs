using Microsoft.EntityFrameworkCore;
using QuickStock.Domain.Accounts;
using QuickStock.Domain.Apparel;
using QuickStock.Domain.Consumable;
using QuickStock.Domain.Furniture;
using QuickStock.Domain.ITassets;
using QuickStock.Domain.Library;
using QuickStock.Domain.Locations;
using QuickStock.Domain.Shared;
using QuickStock.Domain.Social;

// NOTE: Accounts, Shared, Locations, and ITassets have been removed from the 
// top imports because they are already declared globally in your project.

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
        public DbSet<ConsumableUnit> ConsumableUnits { get; set; }
        public DbSet<ConsumableRequest> ConsumableRequests { get; set; }
        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ======================================================
            // Seed Data Configuration
            // ======================================================

            modelBuilder.Entity<Campus>().HasData(
                new Campus { CampusId = 1, Name = "GSC-Cebu", Address = "Cebu City", Description = "Main Campus" },
                new Campus { CampusId = 2, Name = "GSC-Manila", Address = "Metro Manila", Description = "Luzon Branch" }
            );

           

            // ======================================================
            // Account & Security Relationships
            // ======================================================

            modelBuilder.Entity<AccountCampus>()
                .HasOne(ac => ac.Account)
                .WithMany(a => a.AccountCampuses)
                .HasForeignKey(ac => ac.AccountId);

            modelBuilder.Entity<AccountCampus>()
                .HasOne(ac => ac.Campus)
                .WithMany()
                .HasForeignKey(ac => ac.CampusId);

            modelBuilder.Entity<Account>()
                .HasOne(a => a.Profile)
                .WithOne(p => p.Account)
                .HasForeignKey<Profile>(p => p.AccountId);

            // ======================================================
            // Database Relations Integrity (Cascades & Restrictions)
            // ======================================================

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

            // Librarydata Base Key Mapping
            modelBuilder.Entity<Librarydata>()
                .HasKey(l => l.ItemId);

            // Librarydata -> Campus (Restrict deletion if books exist)
            modelBuilder.Entity<Librarydata>()
                .HasOne(l => l.Campus)
                .WithMany()
                .HasForeignKey(l => l.CampusId)
                .OnDelete(DeleteBehavior.Restrict);

            // LibraryBookItem -> Librarydata (Delete copies automatically if parent record is dropped)
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

            // ======================================================
            // Consumable Inventory Configurations
            // ======================================================

            // LABEL: Explicitly configure ConsumableUnit Auto-Increment Sequence 
            modelBuilder.Entity<ConsumableUnit>(entity =>
            {
                entity.ToTable("ConsumableUnit");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                      .HasColumnName("ID")
                      .ValueGeneratedOnAdd(); // Forces DB-level identity tracking (1, 2, 3...)
            });
        }
    }
}
