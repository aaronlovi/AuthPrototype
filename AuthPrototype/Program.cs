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

            var builder = WebApplication.CreateBuilder(args);
            builder.Host.UseSerilog();

            builder.Services.
                AddSingleton(svp => {
                    var logger = svp.GetRequiredService<ILogger<UsersFileStore>>();
                    return new UsersFileStore("users.txt", logger);
                }).
                AddControllers().
                AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                    options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
                });

            builder.Services.ConfigureOAuthToolkit(builder.Configuration);

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.MapGet("/", () => $"AuthPrototype in {app.Environment.EnvironmentName} mode");

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
