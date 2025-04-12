using System;
using System.Text.Json.Serialization;
using AuthPrototype.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OAuthToolkit;
using Serilog;

namespace AuthPrototype;

public class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        try
        {
            Log.Information("Starting up the application");

            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            _ = builder.Host.UseSerilog();

            _ = builder.Services.
                AddSingleton(svp => {
                    ILogger<UsersFileStore> logger = svp.GetRequiredService<ILogger<UsersFileStore>>();
                    return new UsersFileStore("users.txt", logger);
                }).
                AddControllers().
                AddJsonOptions(options => {
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                    options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
                });

            _ = builder.Services.ConfigureOAuthToolkit(builder.Configuration);

            WebApplication app = builder.Build();

            if (app.Environment.IsDevelopment())
                _ = app.UseDeveloperExceptionPage();

            _ = app.UseHttpsRedirection();
            _ = app.UseAuthorization();
            _ = app.MapControllers();

            _ = app.MapGet("/", () => $"AuthPrototype in {app.Environment.EnvironmentName} mode");

            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application start-up failed");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
