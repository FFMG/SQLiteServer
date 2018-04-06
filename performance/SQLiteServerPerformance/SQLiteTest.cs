using System;
using System.IO;
using System.Data.SQLite;

namespace SQLiteServerPerformance
{
  // ReSharper disable once InconsistentNaming
  internal class SQLiteTest
  {
    private const string Table = "table_name";
    private readonly string _source;
    private SQLiteConnection _connection;

    public SQLiteTest(string path, string ext)
    {
      _source = Path.Combine(path, $"{Guid.NewGuid().ToString()}.sqlite.{ext}");
      if (File.Exists(_source))
      {
        File.Delete(_source);
      }
    }

    public void Run( int rows )
    {
      OpenDb();
      CreateTable();
      RunInsertTest( rows );
      CloseDb();
    }

    public void RunInsertTest( int rows)
    {
      Console.Write( $"Insert {rows} rows : ");

      var watch = System.Diagnostics.Stopwatch.StartNew();
      for (var i = 0; i < rows; ++i)
      {
        var sql = $"insert into {Table}(column_1, column_2, column_3) VALUES ('{Guid.NewGuid().ToString()}', 0, 1 )";
        using (var command = new SQLiteCommand(sql, _connection ))
        {
          command.ExecuteNonQuery();
        }
      }
      watch.Stop();
      var elapsedMs = watch.ElapsedMilliseconds;
      var c = Console.ForegroundColor;
      Console.ForegroundColor = ConsoleColor.Green;
      Console.Write($"{((double)elapsedMs/1000):N4}");
      Console.ForegroundColor = c;
      Console.WriteLine("s. [SQLite]");
    }

    private void OpenDb()
    {
      _connection = new SQLiteConnection($"Data Source={_source};Version=3;");
      _connection.Open();
    }

    private void CloseDb()
    {
      _connection?.Close();
      _connection?.Dispose();

      SQLiteConnection.ConnectionPool?.ClearPool(_source);
      if (File.Exists(_source))
      {
        File.Delete(_source);
      }
    }

    private void CreateTable()
    {
      var sql = $@"
      CREATE TABLE {Table}(
        column_1 varchar(255),
        column_2 INTEGER,
        column_3 INTEGER
        );";
      using (var command = new SQLiteCommand(sql, _connection))
      {
        command.ExecuteNonQuery();
      }
    }
  }
}
