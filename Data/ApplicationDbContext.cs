using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SystemMagazynu.Models;

namespace SystemMagazynu.Data
{
    public class ApplicationDbContext:IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
            : base(options)
        { }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Warehouse> Warehouse { get; set; }
        public DbSet<WarehouseStock> WarehouseStocks { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Delivery> Deliveries { get; set; }
        public DbSet<DeliveryItem> DeliveryItems { get; set; }
        public DbSet<WarehouseMovement> WarehouseMovements { get; set; }
        public DbSet<StockAlert> StockAlerts { get; set; }
        public DbSet<ChangeHistory> ChangeHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.CatalogNumber).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.CatalogNumber).IsUnique();
                entity.Property(e => e.Description).HasMaxLength(1000);

                entity.HasOne(e => e.Category)
                      .WithMany(c => c.Products)
                      .HasForeignKey(e => e.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Warehouse>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Street).HasMaxLength(200);
                entity.Property(e => e.BuildingNumber).HasMaxLength(20);
                entity.Property(e => e.PostalCode).HasMaxLength(20);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.Country).HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
            });

            modelBuilder.Entity<WarehouseStock>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.WarehouseId, e.ProductId }).IsUnique();
                entity.HasOne(e => e.Warehouse)
                      .WithMany(w => w.WarehouseStocks)
                      .HasForeignKey(e => e.WarehouseId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Product)
                      .WithMany(p => p.WarehouseStocks)
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Supplier>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.NIP).HasMaxLength(20);
                entity.Property(e => e.Email).HasMaxLength(200);
                entity.Property(e => e.Phone).HasMaxLength(30);
            });

            modelBuilder.Entity<Delivery>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DeliveryNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Notes).HasMaxLength(1000);

                entity.HasOne(e => e.Supplier)
                      .WithMany(s => s.Deliveries)
                      .HasForeignKey(e => e.SupplierId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Warehouse)
                      .WithMany(w => w.Deliveries)
                      .HasForeignKey(e => e.WarehouseId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.User)
                      .WithMany(u => u.Deliveries)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<DeliveryItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UnitPrice).HasPrecision(18, 2);

                entity.HasOne(e => e.Delivery)
                      .WithMany(d => d.DeliveryItems)
                      .HasForeignKey(e => e.DeliveryId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Product)
                      .WithMany(p => p.DeliveryItems)
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<WarehouseMovement>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SourceDocument).HasMaxLength(100);
                entity.Property(e => e.Notes).HasMaxLength(500);

                entity.HasOne(e => e.Product)
                      .WithMany(p => p.WarehouseMovements)
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Warehouse)
                      .WithMany(w => w.WarehouseMovements)
                      .HasForeignKey(e => e.WarehouseId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.User)
                      .WithMany(u => u.WarehouseMovements)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<StockAlert>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Product)
                      .WithMany(p => p.StockAlerts)
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Warehouse)
                      .WithMany(w => w.StockAlerts)
                      .HasForeignKey(e => e.WarehouseId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ChangeHistory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TableName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.OperationType).IsRequired().HasMaxLength(50);

                entity.HasOne(e => e.User)
                      .WithMany(u => u.ChangeHistories)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }

    }
}
