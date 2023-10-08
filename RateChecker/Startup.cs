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
using Microsoft.AspNetCore.Diagnostics;

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
        app.UseExceptionHandler(appError =>
        {
            appError.Run(async context =>
            {
                context.Response.StatusCode = 500; // или любой другой код ошибки
                context.Response.ContentType = "application/json";

                var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                if (contextFeature != null)
                {
                    // Здесь вы можете выполнить любое действие, которое вам нужно.
                    // Например, записать исключение в журнал или вернуть пользователю сообщение об ошибке.
                    await context.Response.WriteAsync(new
                    {
                        StatusCode = context.Response.StatusCode,
                        Message = "Internal Server Error."
                    }.ToString());
                }
            });
        });
        app.MapControllers();
        
        RunWithMigrate(app, args);

    }

    private static void RunWithMigrate(IHost host, string[] args)
    {
        
        host.Run();
    }
}
