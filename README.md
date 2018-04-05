# MyOddWeb.com SQLiteServer [![Release](https://img.shields.io/badge/release-v0.1.1.0-brightgreen.png?style=flat)](https://github.com/FFMG/SQLiteServer/) [![Build Status](https://travis-ci.org/FFMG/SQLiteServer.svg?branch=master)](https://travis-ci.org/FFMG/SQLiteServer)
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

# Performance
I have no idea, but the process that owns the SQLite connection is definitely faster.

But I cannot say for certain how much slower it is, your millage may vary ... but I would be curious to run some proper tests.

# Todo
## A couple of common SQLite commands are missing.
* <s>`IsDBNull( idx )`</s> 0.1.1.0
* <s>`HasRows`</s> 0.1.1.0
* <s>`FieldCount`</s> 0.1.1.0
* <s>`GetName( idx )`</s> 0.1.1.0
* <s>`GetTableName( idx )`</s> 0.1.1.0

## A little less important, (but still need to be added)
* `GetBoolean( idx )`
* `GetByte( idx )`
* `GetChar( idx )`
* `GetDateTime( idx )`
* `GetDecimal( idx )`
* <s>`GetDouble( idx )`</s> 0.1.1.0
* `GetFloat( idx )`

## Other
* Namepipe might be faster, need to investigate more.
* Create Nuget package
* Some code cleanup
* Performance testing/report.
* `SqliteServerDataReader` should implement 
  * `IDataReader`
  * `IDisposable`
  * `IDataRecord`
