using Dialogs.Infrastructure.SagaStates;
using Microsoft.EntityFrameworkCore;

namespace Dialogs.Infrastructure.DbContexts;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DialogMessageSagaData>().HasKey(s => s.CorrelationId);
    }

    public DbSet<DialogMessageSagaData> SagaData {get; set;}
}