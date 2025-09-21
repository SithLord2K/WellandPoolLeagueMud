using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WellandPoolLeagueMud.Data
{
    public class WellandPoolLeagueDbContextFactory : IDesignTimeDbContextFactory<WellandPoolLeagueDbContext>
    {
        public WellandPoolLeagueDbContext CreateDbContext(string[] args)
        {
            // Build configuration to read appsettings.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            // Create DbContextOptionsBuilder
            var optionsBuilder = new DbContextOptionsBuilder<WellandPoolLeagueDbContext>();

            // Get connection string from appsettings.json
            var connectionString = configuration.GetConnectionString("WPLStatsDB");

            // Configure the DbContext to use SQL Server
            optionsBuilder.UseSqlServer(connectionString);

            // Return a new instance of the DbContext
            return new WellandPoolLeagueDbContext(optionsBuilder.Options);
        }
    }
}