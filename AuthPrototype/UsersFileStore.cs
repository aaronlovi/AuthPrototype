using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace AuthPrototype;

public class UsersFileStore {
    private readonly string _filePath;

    public UsersFileStore(string filePath) => _filePath = filePath;

    public List<User> GetAllUsers() {
        if (!File.Exists(_filePath)) return [];

        var lines = File.ReadAllLines(_filePath);
        return lines.Select(line => JsonSerializer.Deserialize<User>(line))
                    .Where(user => user is not null)
                    .ToList()!;
    }

    public void AddUser(User user) {
        var userJson = JsonSerializer.Serialize(user);
        File.AppendAllLines(_filePath, [userJson]);
    }
}
