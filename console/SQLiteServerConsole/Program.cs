using System;
using System.IO;
using System.Net;
using SQLiteServer.Data.Exceptions;
using SQLiteServer.Data.SQLiteServer;

namespace SQLiteServerConsole
{
  internal class Program
  {
    private static readonly IPAddress Address = IPAddress.Parse("127.0.0.1");
    private const int Port = 1100;
    private const int Backlog = 500;
    private const int HeartBeatTimeOut = 500;

    private static void Main(string[] args)
    {
      var path = Directory.GetCurrentDirectory();
      var source = Path.Combine(path, "sample.db");

      Console.WriteLine( $"Db: {source}");

      var connection = new SQLiteServerConnection($"Data Source={source};Version=3;", Address, Port, Backlog, HeartBeatTimeOut);
      try
      {
        connection.Open();

        Console.CursorVisible = true;
        Console.WriteLine("Press Exit to ... exit (duh)");
        while (true)
        {
          var s = Console.ReadLine();
          if (s == null)
          {
            continue;
          }
          if (s.ToLower() == "exit")
          {
            Console.WriteLine("Ok, bye");
            break;
          }

          TryExecute(s, connection);
        }

        connection.Close();
      }
      catch (Exception e)
      {
        Console.WriteLine( $"Unable to open '{source}', please check your permissions.");
        Console.WriteLine( $"Error was : {e.Message}.");
      }
    }

    private static void TryExecute(string s, SQLiteServerConnection connection)
    {
      try
      {
        using (var command = new SQLiteServerCommand(s, connection))
        {
          using (var reader = command.ExecuteReader())
          {
            while (reader.Read())
            {
              for (var i = 0; i < reader.FieldCount; ++i )
              {
                Console.Write($"\t{reader[i]}");
              }
              Console.WriteLine("");
            }
            var r = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("The command(s) completed successfully");
            Console.ForegroundColor = r;
          }
        }
      }
      catch (SQLiteServerException e)
      {
        var r = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine( e.Message );
        Console.ForegroundColor = r;
      }
    }
  }
}
