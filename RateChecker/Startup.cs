using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using Microsoft.AspNetCore.Http;
using RateChecker.SeleniumServices.States;
using RateChecker.StateMachine;
using RateChecker.Common;
using RateChecker.SeleniumServices;

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
        services
            .AddSingleton(new AuthenticatorCodeEntering(StateEnum.AuthenticatorCodeEntering))
            .AddSingleton(new DriverInitialization(StateEnum.DriverInitialization))
            .AddSingleton(new PasswordEntering(StateEnum.PasswordEntering))
            .AddSingleton(new CaptureSolving(StateEnum.CaptureSolving))
            .AddSingleton(new UsernameEntering(StateEnum.UsernameEntering))
            .AddSingleton(new EmailCodeEntering(StateEnum.EmailCodeEntering))
            .AddSingleton(new TokenFetching(StateEnum.TokenFetching))
            .AddScoped<IWebDriverLogin, WebDriverLogin>()
            .AddSingleton<IStateMachineFactory, StateMachineFactory>();
            
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

        //app.UseHttpsRedirection();
        //app.UseHtt
        //app.UseAuthorization();
        
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
