using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace SqlServerTester
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            var connectionString = args.ElementAtOrDefault(0);
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                Console.Write("Enter a connection string: ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                connectionString = Console.ReadLine();
                Console.ResetColor();
            }

            try
            {
                await using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();
                ConsoleExt.Success("Successfully connected");
                Console.WriteLine($"Database: {connection.Database ?? "<none>"}");
                Console.WriteLine($"Data Source: {connection.DataSource ?? "<none>"}");
                Console.WriteLine($"Server Version: {connection.ServerVersion ?? "<none>"}");
                Console.WriteLine($"Connection State: {connection.State}");
            }
            catch (Exception exception)
            {
                ConsoleExt.Error("Connection failed");
                await Console.Error.WriteLineAsync(exception.Message);
            }
        }
    }
}