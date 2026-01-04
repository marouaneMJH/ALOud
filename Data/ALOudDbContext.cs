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

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }

        public DbSet<User> Users { get; set; }
        public DbSet<ALOud.Models.EmailVerification> EmailVerifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Product entity
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Price).HasPrecision(10, 2);
                entity.Property(e => e.ImageUrl).HasMaxLength(500);

                // Configure relationship
                entity.HasOne(p => p.Category)
                      .WithMany(c => c.Products)
                      .HasForeignKey(p => p.CategoryId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Category entity
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            });

            // Configure User Entity
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

        }
    }
}