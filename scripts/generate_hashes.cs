using System;
using BCrypt.Net;

class Program {
    static void Main() {
        string[] passwords = { "Admin123!", "Password123!" };
        foreach (var p in passwords) {
            Console.WriteLine($"{p}: {BCrypt.Net.BCrypt.HashPassword(p, 11)}");
        }
    }
}
