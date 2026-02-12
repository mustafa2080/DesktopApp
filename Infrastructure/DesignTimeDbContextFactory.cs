using GraceWay.AccountingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GraceWay.AccountingSystem.Infrastructure;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        
        // Use PostgreSQL connection string
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=graceway_accounting;Username=postgres;Password=123456");

        return new AppDbContext(optionsBuilder.Options);
    }
}
