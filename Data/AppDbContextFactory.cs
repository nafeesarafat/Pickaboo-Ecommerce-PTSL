using Ecommerce.Api.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Options;

namespace Ecommerce.Api.Data;

/// <summary>
/// Used only by the EF Core tools (dotnet ef migrations / database update) so the tools can
/// build the context without booting the whole web host.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        string connectionString = ResolveConnectionString(args);
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }

    private static string ResolveConnectionString(string[] args)
    {
        // A connection string may be passed directly to the tool:
        // dotnet ef database update -- "Server=...;Database=..."
        if (args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]))
        {
            return args[0];
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var section = configuration.GetSection("ConnectionStrings");
        var options = section.Get<DatabaseOptions>();

        if (string.IsNullOrWhiteSpace(options?.DefaultConnection))
        {
            throw new InvalidOperationException(
                "No connection string found. Set ConnectionStrings:DefaultConnection in appsettings.json " +
                "or provide the environment variable ConnectionStrings__DefaultConnection.");
        }

        return options.DefaultConnection;
    }
}
