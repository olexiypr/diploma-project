using Microsoft.EntityFrameworkCore;
using Services.LlmService.Entities;

namespace Services.LlmService;

public class LlmServiceDbContext(DbContextOptions<LlmServiceDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("vector");

        modelBuilder.Entity<MessageEntity>(options =>
        {
            options.Property(e => e.Embedding).HasColumnType("vector(768)");
            
            options.Property(e => e.DateCreated).ValueGeneratedOnAdd();
            options.Property(e => e.DateModified).ValueGeneratedOnUpdate();
        });
    }
}