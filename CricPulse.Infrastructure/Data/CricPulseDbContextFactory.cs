using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CricPulse.Infrastructure.Data;

public class CricPulseDbContextFactory : IDesignTimeDbContextFactory<CricPulseDbContext>
{
    public CricPulseDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CricPulseDbContext>();

        optionsBuilder.UseSqlServer(
            "Data Source=NANI\\SQLEXPRESS;Initial Catalog=CricPulse;Integrated Security=True;Encrypt=False;TrustServerCertificate=True");

        return new CricPulseDbContext(optionsBuilder.Options);
    }
} 