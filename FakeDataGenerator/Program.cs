using Lamar;
using FakeDataGenerator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ApiPlayground.Data.Context;

internal class Program
{
    private static void Main(string[] args)
    {
        var services = CreateServices();

        Job app = services.GetRequiredService<Job>();
        app.Main();
    }

    private static ServiceProvider CreateServices()
    {
        var services = new ServiceRegistry();

        services.AddLogging();

        services.AddDbContext<AppDbContext>(options =>
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            options.UseSqlServer(connectionString);
        });

        services.AddTransient<Job>();

        return services.BuildServiceProvider();
    }
}