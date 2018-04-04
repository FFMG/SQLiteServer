# SQLiteServer
A library to allow multiple applications to share a single SQLite database

## Sample

### Sample application
In the folder `\console\` there is a sample application, start one or more instance of this app and you can run queries.

### Application #1
```csharp
var connection = new SQLiteServerConnection($"Data Source={source};Version=3;", Address, Port, Backlog, HeartBeatTimeOut);
connection.Open();
try
{
  using (var command = new SQLiteServerCommand(s, connection))
  {
    using (var reader = command.ExecuteReader())
    {
      while (reader.Read())
      {
        Console.WriteLine($"\t{reader[0]}");
      }
    }
  }
}
catch (SQLiteServerException e)
{
  Console.WriteLine( e.Message );
}
connection.Close();
```

### Application #2
Basically do the same thing...

```csharp
var connection = new SQLiteServerConnection($"Data Source={source};Version=3;", Address, Port, Backlog, HeartBeatTimeOut);
connection.Open();
try
{
  using (var command = new SQLiteServerCommand(s, connection))
  {
    using (var reader = command.ExecuteReader())
    {
      while (reader.Read())
      {
        Console.WriteLine($"\t{reader[0]}");
      }
    }
  }
}
catch (SQLiteServerException e)
{
  Console.WriteLine( e.Message );
}
connection.Close();
```
