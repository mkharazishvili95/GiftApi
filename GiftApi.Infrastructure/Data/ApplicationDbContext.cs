using GiftApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .Property(u => u.Balance)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Voucher>()
                .Property(v => v.Amount)
                .HasColumnType("decimal(18,2)");
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<GiftApi.Domain.Entities.File> Files { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }
        public DbSet<VoucherDeliveryInfo> VoucherDeliveryInfos { get; set; }

        public DbSet<LoginAudit> LoginAudits { get; set; }
    }
}
