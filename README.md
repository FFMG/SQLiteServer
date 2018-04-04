# MyOddWeb.com SQLiteServer [![Release](https://img.shields.io/badge/release-v0.1.0.0-brightgreen.png?style=flat)](https://github.com/FFMG/SQLiteServer/)
A library to allow multiple applications/processes to share a single SQLite database

## Why?
A common issue with SQLite is that, by design, only one process can connect to the database, while this is a perfectly normal use case, (and by design), there are some cases where more than one applications might want to share some data, (one does all the insert while another does the queries.)

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

# Todo
- A couple of common SQLite commands are missing.
- Namepipe might be faster, need to investigate more.
- Some code cleanup
- Create Nuget package