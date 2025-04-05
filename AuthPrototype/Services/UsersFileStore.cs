using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using AuthPrototype.Models;

namespace AuthPrototype.Services;

public class UsersFileStore
{
    private readonly string _filePath;

    public UsersFileStore(string filePath) => _filePath = filePath;

    public List<User> GetAllUsers()
    {
        if (!File.Exists(_filePath)) return [];

        try
        {
            var lines = File.ReadAllLines(_filePath);
            return lines.Select(line => JsonSerializer.Deserialize<User>(line))
                        .Where(user => user is not null)
                        .ToList()!;
        }
        catch (Exception)
        {
            // Log the exception
            return [];
        }
    }

    public void AddUser(User user)
    {
        try
        {
            var userJson = JsonSerializer.Serialize(user);
            File.AppendAllLines(_filePath, [userJson]);
        }
        catch (Exception)
        {
            // Log the exception
        }
    }
}
