using Microsoft.EntityFrameworkCore;
using ALOud.Models;

namespace ALOud.Data
{
    public class ALOudDbContext : DbContext
    {
        public ALOudDbContext(DbContextOptions<ALOudDbContext> options)
            : base(options)
        {
        }

        // User entities
        public DbSet<User> Users { get; set; }
        public DbSet<UserAddress> UserAddresses { get; set; }
        public DbSet<EmailVerification> EmailVerifications { get; set; }

        // Checkout entities
        public DbSet<Checkout> Checkouts { get; set; }
        public DbSet<CheckoutAddress> CheckoutAddresses { get; set; }
        public DbSet<StockReservation> StockReservations { get; set; }

        // Order entities
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderStatusHistory> OrderStatusHistory { get; set; }
        public DbSet<OrderShipment> OrderShipments { get; set; }
        public DbSet<OrderShipmentItem> OrderShipmentItems { get; set; }
        public DbSet<OrderPayment> OrderPayments { get; set; }

        // Perfume domain entities
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Perfume> Perfumes { get; set; }
        public DbSet<Family> Families { get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<Accord> Accords { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Season> Seasons { get; set; }
        public DbSet<Occasion> Occasions { get; set; }

        // Junction tables
        public DbSet<PerfumeFamily> PerfumeFamilies { get; set; }
        public DbSet<PerfumeNote> PerfumeNotes { get; set; }
        public DbSet<PerfumeAccord> PerfumeAccords { get; set; }
        public DbSet<PerfumeTag> PerfumeTags { get; set; }
        public DbSet<PerfumeSeason> PerfumeSeasons { get; set; }
        public DbSet<PerfumeOccasion> PerfumeOccasions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User Entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
            });

            // =====================================================
            // USER ADDRESS CONFIGURATION
            // =====================================================

            // UserAddress
            modelBuilder.Entity<UserAddress>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(ua => ua.User)
                      .WithMany(u => u.Addresses)
                      .HasForeignKey(ua => ua.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Index for finding user's default address
                entity.HasIndex(ua => new { ua.UserId, ua.IsDefault });
            });

            // =====================================================
            // CHECKOUT CONFIGURATION
            // =====================================================

            // Checkout
            modelBuilder.Entity<Checkout>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.ExpiresAt)
                    .HasDefaultValueSql("DATEADD(MINUTE, 30, GETUTCDATE())");

                entity.HasOne(c => c.User)
                      .WithMany(u => u.Checkouts)
                      .HasForeignKey(c => c.UserId)
                      .OnDelete(DeleteBehavior.SetNull);

                // Indexes for common queries
                entity.HasIndex(c => c.CartId);
                entity.HasIndex(c => c.Status);
                entity.HasIndex(c => c.Email);
                entity.HasIndex(c => c.ExpiresAt);
            });

            // CheckoutAddress
            modelBuilder.Entity<CheckoutAddress>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(ca => ca.Checkout)
                      .WithMany(c => c.CheckoutAddresses)
                      .HasForeignKey(ca => ca.CheckoutId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ca => ca.UserAddress)
                      .WithMany()
                      .HasForeignKey(ca => ca.UserAddressId)
                      .OnDelete(DeleteBehavior.SetNull);

                // Index for finding addresses by checkout and type
                entity.HasIndex(ca => new { ca.CheckoutId, ca.AddressType });
            });

            // StockReservation
            modelBuilder.Entity<StockReservation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.ReservedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.ExpiresAt)
                    .HasDefaultValueSql("DATEADD(MINUTE, 30, GETUTCDATE())");

                entity.HasOne(sr => sr.Checkout)
                      .WithMany(c => c.StockReservations)
                      .HasForeignKey(sr => sr.CheckoutId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(sr => sr.Perfume)
                      .WithMany()
                      .HasForeignKey(sr => sr.PerfumeId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Indexes for inventory management
                entity.HasIndex(sr => sr.PerfumeId);
                entity.HasIndex(sr => sr.Status);
                entity.HasIndex(sr => sr.ExpiresAt);
                entity.HasIndex(sr => new { sr.CheckoutId, sr.PerfumeId });
            });

            // =====================================================
            // PERFUME DOMAIN CONFIGURATION
            // =====================================================

            // Brand
            modelBuilder.Entity<Brand>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // Perfume
            modelBuilder.Entity<Perfume>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(300);
                entity.Property(e => e.Intensity).HasMaxLength(100);
                entity.Property(e => e.Longevity).HasMaxLength(100);
                entity.Property(e => e.Sillage).HasMaxLength(100);
                entity.Property(e => e.GenderProfile).HasMaxLength(100);
                entity.Property(e => e.PriceRange).HasMaxLength(100);
                entity.Property(e => e.Price).HasPrecision(10, 2);
                entity.Property(e => e.Description).HasMaxLength(2000);
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(p => p.Brand)
                      .WithMany(b => b.Perfumes)
                      .HasForeignKey(p => p.BrandId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Family
            modelBuilder.Entity<Family>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // Note
            modelBuilder.Entity<Note>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Category).HasMaxLength(100);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // Accord
            modelBuilder.Entity<Accord>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // Tag
            modelBuilder.Entity<Tag>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // Season
            modelBuilder.Entity<Season>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // Occasion
            modelBuilder.Entity<Occasion>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // =====================================================
            // JUNCTION TABLES (Many-to-Many Relationships)
            // =====================================================

            // PerfumeFamily
            modelBuilder.Entity<PerfumeFamily>(entity =>
            {
                entity.HasKey(pf => new { pf.PerfumeId, pf.FamilyId });

                entity.HasOne(pf => pf.Perfume)
                      .WithMany(p => p.PerfumeFamilies)
                      .HasForeignKey(pf => pf.PerfumeId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pf => pf.Family)
                      .WithMany(f => f.PerfumeFamilies)
                      .HasForeignKey(pf => pf.FamilyId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // PerfumeNote
            modelBuilder.Entity<PerfumeNote>(entity =>
            {
                entity.HasKey(pn => new { pn.PerfumeId, pn.NoteId });
                entity.Property(pn => pn.NoteLevel).HasMaxLength(50);

                entity.HasOne(pn => pn.Perfume)
                      .WithMany(p => p.PerfumeNotes)
                      .HasForeignKey(pn => pn.PerfumeId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pn => pn.Note)
                      .WithMany(n => n.PerfumeNotes)
                      .HasForeignKey(pn => pn.NoteId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // PerfumeAccord
            modelBuilder.Entity<PerfumeAccord>(entity =>
            {
                entity.HasKey(pa => new { pa.PerfumeId, pa.AccordId });
                entity.Property(pa => pa.Intensity).HasMaxLength(50);

                entity.HasOne(pa => pa.Perfume)
                      .WithMany(p => p.PerfumeAccords)
                      .HasForeignKey(pa => pa.PerfumeId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pa => pa.Accord)
                      .WithMany(a => a.PerfumeAccords)
                      .HasForeignKey(pa => pa.AccordId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // PerfumeTag
            modelBuilder.Entity<PerfumeTag>(entity =>
            {
                entity.HasKey(pt => new { pt.PerfumeId, pt.TagId });

                entity.HasOne(pt => pt.Perfume)
                      .WithMany(p => p.PerfumeTags)
                      .HasForeignKey(pt => pt.PerfumeId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pt => pt.Tag)
                      .WithMany(t => t.PerfumeTags)
                      .HasForeignKey(pt => pt.TagId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // PerfumeSeason
            modelBuilder.Entity<PerfumeSeason>(entity =>
            {
                entity.HasKey(ps => new { ps.PerfumeId, ps.SeasonId });

                entity.HasOne(ps => ps.Perfume)
                      .WithMany(p => p.PerfumeSeasons)
                      .HasForeignKey(ps => ps.PerfumeId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ps => ps.Season)
                      .WithMany(s => s.PerfumeSeasons)
                      .HasForeignKey(ps => ps.SeasonId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // PerfumeOccasion
            modelBuilder.Entity<PerfumeOccasion>(entity =>
            {
                entity.HasKey(po => new { po.PerfumeId, po.OccasionId });

                entity.HasOne(po => po.Perfume)
                      .WithMany(p => p.PerfumeOccasions)
                      .HasForeignKey(po => po.PerfumeId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(po => po.Occasion)
                      .WithMany(o => o.PerfumeOccasions)
                      .HasForeignKey(po => po.OccasionId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Order entity configuration
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasIndex(o => o.OrderNumber).IsUnique();
                entity.HasIndex(o => o.CheckoutId).IsUnique();
                entity.HasIndex(o => o.CustomerEmail);
                entity.HasIndex(o => o.Status);
                entity.HasIndex(o => o.CreatedAt);
                
                entity.Property(o => o.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
                entity.Property(o => o.UpdatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(o => o.User)
                      .WithMany()
                      .HasForeignKey(o => o.UserId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(o => o.Checkout)
                      .WithOne()
                      .HasForeignKey<Order>(o => o.CheckoutId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // OrderItem entity configuration
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasIndex(oi => oi.OrderId);
                entity.HasIndex(oi => oi.ProductId);
                entity.HasIndex(oi => oi.Status);
                
                entity.Property(oi => oi.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
                entity.Property(oi => oi.UpdatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(oi => oi.Order)
                      .WithMany(o => o.OrderItems)
                      .HasForeignKey(oi => oi.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(oi => oi.Product)
                      .WithMany()
                      .HasForeignKey(oi => oi.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // OrderStatusHistory entity configuration
            modelBuilder.Entity<OrderStatusHistory>(entity =>
            {
                entity.HasIndex(osh => osh.OrderId);
                entity.HasIndex(osh => osh.CreatedAt);
                entity.HasIndex(osh => osh.NewStatus);
                
                entity.Property(osh => osh.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(osh => osh.Order)
                      .WithMany(o => o.StatusHistory)
                      .HasForeignKey(osh => osh.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(osh => osh.ChangedByUser)
                      .WithMany()
                      .HasForeignKey(osh => osh.ChangedByUserId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // OrderShipment entity configuration
            modelBuilder.Entity<OrderShipment>(entity =>
            {
                entity.HasIndex(os => os.OrderId);
                entity.HasIndex(os => os.TrackingNumber);
                entity.HasIndex(os => os.ShipmentNumber).IsUnique();
                entity.HasIndex(os => os.Status);
                
                entity.Property(os => os.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
                entity.Property(os => os.UpdatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(os => os.Order)
                      .WithMany(o => o.Shipments)
                      .HasForeignKey(os => os.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // OrderShipmentItem entity configuration
            modelBuilder.Entity<OrderShipmentItem>(entity =>
            {
                entity.HasIndex(osi => osi.OrderShipmentId);
                entity.HasIndex(osi => osi.OrderItemId);
                
                entity.Property(osi => osi.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
                entity.Property(osi => osi.UpdatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(osi => osi.OrderShipment)
                      .WithMany(os => os.ShipmentItems)
                      .HasForeignKey(osi => osi.OrderShipmentId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(osi => osi.OrderItem)
                      .WithMany()
                      .HasForeignKey(osi => osi.OrderItemId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // OrderPayment entity configuration
            modelBuilder.Entity<OrderPayment>(entity =>
            {
                entity.HasIndex(op => op.OrderId);
                entity.HasIndex(op => op.PaymentReference).IsUnique();
                entity.HasIndex(op => op.ExternalPaymentId);
                entity.HasIndex(op => op.Status);
                entity.HasIndex(op => op.CreatedAt);
                
                entity.Property(op => op.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
                entity.Property(op => op.UpdatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(op => op.Order)
                      .WithMany(o => o.Payments)
                      .HasForeignKey(op => op.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(op => op.RefundedByUser)
                      .WithMany()
                      .HasForeignKey(op => op.RefundedByUserId)
                      .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}