using System;
using System.IO;
using System.Net;
using SQLiteServer.Data.SQLiteServer;

namespace SQLiteServerPerformance
{
  // ReSharper disable once InconsistentNaming
  internal class SQLiteServerTest
  {
    protected static readonly IPAddress Address = IPAddress.Parse("127.0.0.1");
    protected const int Port = 1100;
    protected const int Backlog = 500;
    protected const int HeartBeatTimeOut = 500;
    private const string Table = "table_name";
    private readonly string _source;
    private SQLiteServerConnection _connectionServer;
    private SQLiteServerConnection _connectionClient;
    private readonly bool _useClient;

    public SQLiteServerTest( bool useClient)
    {
      _useClient = useClient;
      _source = ":memory:";
    }

    public SQLiteServerTest(string path, string ext, bool useClient )
    {
      _useClient = useClient;
      _source = Path.Combine(path, $"{Guid.NewGuid().ToString()}.sqlite.{ext}");
      if (File.Exists(_source))
      {
        File.Delete(_source);
      }
    }

    public void Run(int rows)
    {
      OpenDb();
      CreateTable();
      RunInsertTest(rows);
      CloseDb();
    }

    public void RunInsertTest(int rows)
    {
      Console.Write($"Insert {rows} rows : ");

      var watch = System.Diagnostics.Stopwatch.StartNew();
      for (var i = 0; i < rows; ++i)
      {
        var sql = $"insert into {Table}(column_1, column_2, column_3) VALUES ('{Guid.NewGuid().ToString()}', 0, 1 )";
        using (var command = new SQLiteServerCommand(sql, (_useClient ? _connectionClient : _connectionServer)))
        {
          command.ExecuteNonQuery();
        }
      }
      watch.Stop();
      var elapsedMs = watch.ElapsedMilliseconds;
      var c = Console.ForegroundColor;
      Console.ForegroundColor = ConsoleColor.Green;
      Console.Write($"{((double)elapsedMs / 1000):N4}");
      Console.ForegroundColor = c;

      if (_source == ":memory:")
      {
        Console.WriteLine($"s. [SQLite {(_useClient ? "Client via Server memory" : "Server Memory")}]");
      }
      else
      {
        Console.WriteLine($"s. [SQLite {(_useClient ? "Client via Server" : "Server")}]");
      }
    }

    private void OpenDb()
    {
      _connectionServer = new SQLiteServerConnection($"Data Source={_source};Version=3;", Address, Port, Backlog, HeartBeatTimeOut);
      _connectionServer.Open();

      if (!_useClient)
      {
        return;
      }
      _connectionClient = new SQLiteServerConnection($"Data Source={_source};Version=3;", Address, Port, Backlog, HeartBeatTimeOut);
      _connectionClient.Open();
    }

    private void CloseDb()
    {
      _connectionClient?.Close();
      _connectionClient?.Dispose();

      _connectionServer?.Close();
      _connectionServer?.Dispose();
    }

    private void CreateTable()
    {
      var sql = $@"
      CREATE TABLE {Table}(
        column_1 varchar(255),
        column_2 INTEGER,
        column_3 INTEGER
        );";
      using (var command = new SQLiteServerCommand(sql, _useClient ? _connectionClient : _connectionServer))
      {
        command.ExecuteNonQuery();
      }
    }
  }
}
