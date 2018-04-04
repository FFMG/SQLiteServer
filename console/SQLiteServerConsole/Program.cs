using System;
using System.IO;
using System.Net;
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
        }

        connection.Close();
      }
      catch (Exception e)
      {
        Console.WriteLine( $"Unable to open '{source}', please check your permissions.");
        Console.WriteLine( $"Error was : {e.Message}.");
      }
    }
  }
}
