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
        public DbSet<EmailVerification> EmailVerifications { get; set; }

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
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

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
        }
    }
}