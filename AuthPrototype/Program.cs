using AuthPrototype.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AuthPrototype;

public class Program {
    public static void Main(string[] args) {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddSingleton(new UsersFileStore("users.txt"));
        builder.Services.AddHttpClient<TokenService>();
        builder.Services.AddControllers();
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
}
