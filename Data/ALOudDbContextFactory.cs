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
            LoadEnvironmentVariablesFromFile();
            var connectionString = GetConnectionString();
            var options = BuildDbContextOptions(connectionString);
            return new ALOudDbContext(options);
        }

        private void LoadEnvironmentVariablesFromFile()
        {
            var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
            if (!File.Exists(envPath))
                return;

            foreach (var line in File.ReadAllLines(envPath))
            {
                ProcessEnvironmentVariableLine(line);
            }
        }

        private void ProcessEnvironmentVariableLine(string line)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#'))
                return;

            var idx = trimmed.IndexOf('=');
            if (idx <= 0)
                return;

            var key = trimmed[..idx].Trim();
            var value = trimmed[(idx + 1)..].Trim();
            value = RemoveQuotesFromValue(value);
            Environment.SetEnvironmentVariable(key, value);
        }

        private string RemoveQuotesFromValue(string value)
        {
            if ((value.StartsWith('"') && value.EndsWith('"')) ||
                (value.StartsWith('\'') && value.EndsWith('\'')))
            {
                return value[1..^1];
            }
            return value;
        }

        private string GetConnectionString()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
                                  ?? configuration.GetConnectionString("DefaultConnection");
            
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("Database connection string is missing");

            return connectionString;
        }

        private DbContextOptions<ALOudDbContext> BuildDbContextOptions(string connectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ALOudDbContext>();
            optionsBuilder.UseSqlServer(connectionString);
            return optionsBuilder.Options;
        }
    }
}
