﻿using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SqlServerTester
{
    internal static class Program
    {
        private static readonly TimeSpan MaxConnectionTime = TimeSpan.FromSeconds(30);
        
        private static async Task Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += CurrentDomainOnProcessExit;
            Console.CancelKeyPress += ConsoleOnCancelKeyPress;
            
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
                var openAsyncTask = connection.OpenAsync();
                if (await Task.WhenAny(openAsyncTask, Task.Delay(MaxConnectionTime)) != openAsyncTask)
                {
                    throw new TimeoutException($"Connection time has expired ({MaxConnectionTime.TotalSeconds} secs)");
                }
                loadingCancellation.Cancel();
                ConsoleExt.Success("Connection succeeded");
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

                for (var i = 0;; i++)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    var loadingCharacterIndex = i % loadingCharacters.Length;
                    var loadingCharacter = loadingCharacters[loadingCharacterIndex];

                    ResetPosition();
                    Console.Write($"{loadingCharacter} {title}");

                    await Task.Delay(TimeSpan.FromSeconds(.25), cancellationToken);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
            finally
            {
                Console.ResetColor();
                ResetPosition();
                Console.Write(new string(' ', title.Length + 2));
                ResetPosition();
                Console.CursorVisible = true;
            }
        }
        
        private static void CurrentDomainOnProcessExit(object sender, EventArgs e)
        {
            Console.CursorVisible = true;
            Console.ResetColor();
        }
        
        private static void ConsoleOnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            CurrentDomainOnProcessExit(null, null);
        }
    }
}