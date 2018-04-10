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
The initial tests showed that server was as fast as the default libraries, (but of course slower than the C++ library itself)

Data files are either _similar_ speed or a tiny bit slower, (see the results below)
`:memory:` datasource, is, of course, a lot slower.

![0.1.1.1](https://i.imgur.com/Ve6sI4P.png)

Please see the performance app to run your own tests, `\performance\SQLiteServerPerformance.sln`.

## Past results

* [0.1.1.0](https://i.imgur.com/Ve6sI4P.png)

# Todo
## Performance 
While 'similar' performance will never be achieved, I am aiming for a degradation of no more than 5%.

I am using the `\performance\SQLiteServerPerformance.sln` application to run comparaison tests.

## A couple of common SQLite commands are missing.
* <s>`IsDBNull( idx )`</s> 0.1.1.0
* <s>`HasRows`</s> 0.1.1.0
* <s>`FieldCount`</s> 0.1.1.0
* <s>`GetName( idx )`</s> 0.1.1.0
* <s>`GetTableName( idx )`</s> 0.1.1.0

## A little less important, (but still need to be added)
* <s>`GetBoolean( idx )`</s> 0.1.1.2
* `GetByte( idx )`
* `GetChar( idx )`
* `GetDateTime( idx )`
* <s>`GetDataTypeName( idx )`</s> 0.1.1.3
* <s>`GetDecimal( idx )`</s> 0.1.1.2
* <s>`GetDouble( idx )`</s> 0.1.1.0
* <s>`GetFloat( idx )`</s> 0.1.1.2
* <s>`NextResult()`</s> 0.1.1.3

## Other
* Namepipe might be faster, need to investigate more.
* Create Nuget package
* Some code cleanup
* <s>`Performance testing/report.`</s> 0.1.1.0
* <s>`SQLiteServerConnection` should implement `DbConnection`</s> 0.1.1.1
* `SQLiteServerCommand` should implement `DbCommand`
  * Need to implement `DbCommand CreateDbCommand(){}` when this is done
* `SqliteServerDataReader` should implement `DbDataReader`
* <s>`SQLiteServerTransaction` should implement `DbTransaction`</s> 0.1.1.1

## Acknowledgement

* [sqlite.org](http://sqlite.org/index.html "sqlite.org") (duh!)
* [System.Data.SQLite](https://system.data.sqlite.org/index.html/doc/trunk/www/downloads.wiki")

If I forgot someone, please let me know and I will gladly add them here :)
