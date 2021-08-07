using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
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
                Console.Write("Enter the connection string: ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                connectionString = Console.ReadLine();
                Console.ResetColor();
            }

            var loadingCancellation = new CancellationTokenSource();
            ShowLoading("Connecting", loadingCancellation.Token);

            try
            {
                await using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();
                loadingCancellation.Cancel();
                ConsoleExt.Success("Successfully connected");
                Console.WriteLine($"Database: {connection.Database ?? "<none>"}");
                Console.WriteLine($"Data Source: {connection.DataSource ?? "<none>"}");
                Console.WriteLine($"Server Version: {connection.ServerVersion ?? "<none>"}");
                Console.WriteLine($"Connection State: {connection.State}");
            }
            catch (Exception exception)
            {
                loadingCancellation.Cancel();
                ConsoleExt.Error("Connection failed");
                await Console.Error.WriteLineAsync(exception.Message);
            }
        }

        private static async void ShowLoading(string title, CancellationToken cancellationToken)
        {
            var loadingCharacters = new[] { "/", "-", "\\", "|" };
            var (left, top) = Console.GetCursorPosition();
            
            void ResetPosition() => Console.SetCursorPosition(left, top);

            try
            {
                Console.CursorVisible = false;
                Console.ForegroundColor = ConsoleColor.Cyan;
                
                for (var i = 0; !cancellationToken.IsCancellationRequested; i++)
                {
                    var loadingCharacterIndex = i % loadingCharacters.Length;
                    var loadingCharacter = loadingCharacters[loadingCharacterIndex];

                    ResetPosition();
                    Console.Write($"{loadingCharacter} {title}");
                
                    await Task.Delay(TimeSpan.FromSeconds(.25), cancellationToken);
                }
            }
            finally
            {
                Console.ResetColor();
                ResetPosition();
                Console.Write(new string(' ', title.Length + 2));
                ResetPosition();
                Console.CursorVisible = true;
            }
        }
    }
}