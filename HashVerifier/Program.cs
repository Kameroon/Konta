using System;

public class Program
{
    public static void Main()
    {
        string password = "Password123!";
        string hash = "$2a$11$OvHGeoGRS694M.8YPOrKUuFaHQHh9ixo7VjZeYCSk4IGmqF9Vif/6";
        
        try {
            bool match = BCrypt.Net.BCrypt.Verify(password, hash);
            Console.WriteLine($"Match Old: {match}");
        } catch (Exception ex) {
            Console.WriteLine($"Error Old: {ex.Message}");
        }
        
        string newHash = BCrypt.Net.BCrypt.HashPassword(password);
        Console.WriteLine($"New Hash: {newHash}");
        
        bool matchNew = BCrypt.Net.BCrypt.Verify(password, newHash);
        Console.WriteLine($"Match New: {matchNew}");
    }
}
