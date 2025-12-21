using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;
using ALOud.Data;

namespace ALOud.Data
{
    public class ALOudDbContextFactory
        : IDesignTimeDbContextFactory<ALOudDbContext>
    {
        public ALOudDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<ALOudDbContext>();

            optionsBuilder.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection")
            );

            return new ALOudDbContext(optionsBuilder.Options);
        }
    }
}
