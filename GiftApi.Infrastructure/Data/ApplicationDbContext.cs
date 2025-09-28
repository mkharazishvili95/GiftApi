using Microsoft.EntityFrameworkCore;
using GiftApi.Core.Entities;

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
        }

        public DbSet<User> Users { get; set; }

       
    }
}
