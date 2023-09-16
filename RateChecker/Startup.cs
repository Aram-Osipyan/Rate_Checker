using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using Microsoft.AspNetCore.Http;

namespace RateChecker;
public sealed class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration) => _configuration = configuration;

    public void ConfigureServices(IServiceCollection services)
    {

        var connectionString = _configuration["ConnectionString"];

        /*services
            .AddFluentMigrator(
                connectionString,
                typeof(SqlMigration).Assembly);*/

        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
    }

    public void Configure(WebApplication app, IWebHostEnvironment env, string[] args)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        var accessKey = _configuration["AccessKey"];
        RunWithMigrate(app, args);

    }

    private static void RunWithMigrate(IHost host, string[] args)
    {
        if (args.Length > 0 && args[0].Equals("migrate", StringComparison.InvariantCultureIgnoreCase))
        {
            using var scope = host.Services.CreateScope();
            // var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

            //runner.MigrateUp();
        }
        else
            host.Run();
    }
}
