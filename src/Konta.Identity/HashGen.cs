using System;
namespace Konta.Identity;
public class HashGen {
    public static void Main() {
        Console.WriteLine(BCrypt.Net.BCrypt.HashPassword("Admin123!", 11));
    }
}
