using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthPrototype;

public class Program {
    private static readonly UsersFileStore _userStore = new("users.txt");

    public static void Main(string[] args) {
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();

        app.MapGet("/", () => "Hello World!");
        app.MapPost("/authenticate", AuthenticateUser);

        app.Run();
    }

    private static IResult AuthenticateUser([FromBody] User user) {
        List<User> users = _userStore.GetAllUsers();
        User? existingUser = users.FirstOrDefault(u => u.Email == user.Email && u.Provider == user.Provider);

        if (existingUser is not null) return Results.Ok(existingUser);

        _userStore.AddUser(user);
        return Results.Ok(user);
    }
}
