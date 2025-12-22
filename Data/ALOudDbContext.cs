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

            // Seed data
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Men's Fragrances" },
                new Category { Id = 2, Name = "Women's Fragrances" },
                new Category { Id = 3, Name = "Unisex Fragrances" }
            );

            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Name = "ALOud Royal", Description = "A luxurious woody fragrance with notes of oud and rose", Price = 299.99m, CategoryId = 3, ImageUrl = "/images/products/aloud-royal.jpg", Stock = 50 },
                new Product { Id = 2, Name = "ALOud Elegance", Description = "An elegant floral fragrance perfect for special occasions", Price = 199.99m, CategoryId = 2, ImageUrl = "/images/products/aloud-elegance.jpg", Stock = 30 },
                new Product { Id = 3, Name = "ALOud Classic", Description = "A timeless masculine scent with woody and spicy notes", Price = 149.99m, CategoryId = 1, ImageUrl = "/images/products/aloud-classic.jpg", Stock = 75 }
            );
        }
    }
}