using Diploma1.IdentityService.Entities;
using Microsoft.EntityFrameworkCore;

namespace Diploma1.IdentityService;

public class IdentityServiceDbContext(DbContextOptions<IdentityServiceDbContext> options) : DbContext(options)
{
    public DbSet<UserEntity> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserEntity>(options =>
        {
            options.ToTable("Users");
            options.HasKey(x => x.Id);
        });
    }
}