using System;
using Npgsql;
using Dapper;
using System.Linq;

namespace DbCheck {
    class Program {
        static void Main() {
            string connString = "Host=localhost;Port=5432;Database=finances_db;Username=postgres;Password=@dmin1212;";
            try {
                using var conn = new NpgsqlConnection(connString);
                conn.Open();
                Console.WriteLine("Connexion DB réussie. Recherche de verrous...");
                
                var locks = conn.Query(@"
                    SELECT 
                        l.pid, relname, locktype, mode, granted,
                        query as blocking_query,
                        state as session_state
                    FROM pg_locks l 
                    JOIN pg_class c ON l.relation = c.oid 
                    JOIN pg_stat_activity a ON l.pid = a.pid
                    WHERE NOT granted;");

                foreach (var l in locks) {
                    Console.WriteLine($"PID: {l.pid}, Table: {l.relname}, Type: {l.locktype}, Mode: {l.mode}, Grant: {l.granted}");
                    Console.WriteLine($"Query: {l.blocking_query}");
                }

                if (!locks.Any()) {
                    Console.WriteLine("Aucun verrou bloquant trouvé.");
                }

                var activity = conn.Query("SELECT pid, state, query, duration FROM (SELECT pid, state, query, now() - query_start AS duration FROM pg_stat_activity WHERE state != 'idle') x ORDER BY duration DESC LIMIT 10;");
                Console.WriteLine("\nRequêtes actives :");
                foreach (var a in activity) {
                    Console.WriteLine($"PID: {a.pid}, State: {a.state}, Duration: {a.duration}, Query: {a.query}");
                }
            } catch (Exception ex) {
                Console.WriteLine($"Erreur : {ex.Message}");
            }
        }
    }
}
