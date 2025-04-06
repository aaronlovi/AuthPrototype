using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AuthPrototype.Models;
using Microsoft.Extensions.Logging;

namespace AuthPrototype.Services;

public class UsersFileStore
{
    private readonly string _filePath;
    private readonly ILogger<UsersFileStore> _logger;

    public UsersFileStore(string filePath, ILogger<UsersFileStore> logger)
    {
        _filePath = filePath;
        _logger = logger;
    }

    public List<User> GetAllUsers()
    {
        if (!File.Exists(_filePath)) return [];

        try
        {
            var lines = File.ReadAllLines(_filePath);
            return lines.Select(line => Conventions.Deserialize<User>(line))
                        .Where(user => user is not null)
                        .ToList()!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading user file");
            return [];
        }
    }

    public void AddOrUpdateUser(User user)
    {
        try
        {
            var users = GetAllUsers();
            bool found = false;
            for (int i = 0; i < users.Count; i++)
            {
                if (users[i].Email != user.Email) continue;

                found = true;
                users[i] = user;
                break;
            }

            if (!found) users.Add(user);

            var userJsonLines = users.Select(u => Conventions.Serialize(u)).ToList();
            File.WriteAllLines(_filePath, userJsonLines);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error writing user to file");
        }
    }
}
