using Mahsul.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Mahsul.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
       : base(options)
        {
        }
        /*protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=.\\SQLEXPRESS;Database=mahsul;Trusted_Connection=True;MultipleActiveResultSets=true");
        }*/
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Base sınıfın OnModelCreating metodu çağrıldı
            modelBuilder.Entity<IdentityUser>(b =>
            {
                b.HasKey(u => u.Id);
                b.ToTable("AspNetUsers");
            });
            modelBuilder.Entity<Farmer>()
                .HasMany(f => f.Products)
                .WithOne(p => p.Farmer)
                .HasForeignKey(p => p.FarmerID);

            modelBuilder.Entity<Product>()
                .HasMany(p => p.Purchases)
                .WithOne(pr => pr.Product)
                .HasForeignKey(pr => pr.ProductID);

            modelBuilder.Entity<Product>()
                .HasMany(p => p.Ratings)
                .WithOne(r => r.Product)
                .HasForeignKey(r => r.ProductID);

            // Farmer ve Product arasındaki ilişkiyi belirtmek için:
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Farmer)
                .WithMany(f => f.Products)
                .HasForeignKey(p => p.FarmerID);

            modelBuilder.Entity<Purchase>()
                .HasOne(pr => pr.Product)
                .WithMany(p => p.Purchases)
                .HasForeignKey(pr => pr.ProductID)
                .OnDelete(DeleteBehavior.Restrict);
        }

        public DbSet<Farmer> ?farmers { get; set; }
        public DbSet<Product>? Product { get; set; }
        public DbSet<Purchase>? Purchase { get; set; }
        public DbSet<Rating>? Rating { get; set; }
        public DbSet<Category>? categories { get; set; }
        public DbSet<Messages> Messages { get; set; }
        public DbSet<ProductImage> ProductImage { get; set; }


    }
}
