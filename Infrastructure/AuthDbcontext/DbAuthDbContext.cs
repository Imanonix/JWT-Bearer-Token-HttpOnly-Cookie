using Domain.Models;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.AuthDbContext
{
    public class DbAuthDbContext : DbContext
    {
        public DbAuthDbContext(DbContextOptions options) : base(options)
        {

        }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .Property(u => u.Roles)
                .HasConversion<string>();
        }
    }
    public class BloggingContextFactory : IDesignTimeDbContextFactory<DbAuthDbContext>
    {
        public DbAuthDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DbAuthDbContext>();
            optionsBuilder.UseSqlServer("Server=DESKTOP-663BMID;Database=DbAuthDbContext;Trusted_Connection=True;TrustServerCertificate=True");

            return new DbAuthDbContext(optionsBuilder.Options);
        }
    }
}
