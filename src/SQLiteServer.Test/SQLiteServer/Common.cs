//This file is part of SQLiteServer.
//
//    SQLiteServer is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    SQLiteServer is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with SQLiteServer.  If not, see<https://www.gnu.org/licenses/gpl-3.0.en.html>.

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using NUnit.Framework;
using SQLiteServer.Data.Connections;
using SQLiteServer.Data.SQLiteServer;

namespace SQLiteServer.Test.SQLiteServer
{
  [TestFixture]
  internal class Common
  {
    protected static readonly IPAddress Address = IPAddress.Parse("127.0.0.1");
    protected const int Port = 1201;
    protected const int Backlog = 500;
    protected const int HeartBeatTimeOut = 500;
    protected const int QueryTimeoutMs = 30000;

    private readonly List<string> _sources = new List<string>();
    private readonly List<SQLiteServerConnection> _connections = new List<SQLiteServerConnection>();
    private readonly Random _random = new Random();

    [SetUp]
    public void SetUp()
    {
      _sources.Add( GetTempFileName() );
    }

    private static string GetTempFileName()
    {
      var path = Path.GetTempFileName();
      try
      {
        File.Delete(path);
      }
      catch
      {
        // ignored
      }

      path = Path.ChangeExtension(path, "sqlite");
      File.Create(path).Close();
      return path;
    }

    [TearDown]
    public void TearDown()
    {
      try
      {
        foreach (var connection in _connections)
        {
          if (connection.State != ConnectionState.Closed)
          {
            connection.Close();
          }
        }

        foreach (var source in _sources)
        {
          try
          {
            File.Delete(source);
          }
          catch
          {
            // ignored
          }
        }
        // remove all the files from the list.
        _sources.Clear();
      }
      catch
      {
        // ignored
      }
    }

    protected SQLiteServerConnection CreateConnectionNewSource(IConnectionBuilder connectionBuilder = null
      , int? defaultTimeout = null)
    {
      var source = GetTempFileName();
      _sources.Add(source);
      return CreateConnection(connectionBuilder, defaultTimeout, source);
    }

    protected SQLiteServerConnection CreateConnection(
      IConnectionBuilder connectionBuilder,
      SQLiteServerConnection parent )
    {
      var connection = new SQLiteServerConnection(parent.ConnectionString, connectionBuilder);
      _connections.Add(connection);
      return connection;
    }

    protected SQLiteServerConnection CreateConnection(IConnectionBuilder connectionBuilder = null
      , int? defaultTimeout = null,
      string source = null)
    {
      if (connectionBuilder == null)
      {
        connectionBuilder = new SocketConnectionBuilder(Address, Port, Backlog, HeartBeatTimeOut);
      }

      var connectionString = $"Data Source={source ?? _sources.First()}";
      if (defaultTimeout != null)
      {
        connectionString = $"{connectionString};Default Timeout={(int)defaultTimeout}";
      }
      var connection = new SQLiteServerConnection(connectionString, connectionBuilder);

      _connections.Add(connection);
      return connection;
    }

    protected string RandomString(int len)
    {
      const string chars = "$%#@!*abcdefghijklmnopqrstuvwxyz1234567890?;:ABCDEFGHIJKLMNOPQRSTUVWXYZ^&";
      var value = "";
      for (var i = 0; i < len; i++)
      {
        var num = _random.Next(0, chars.Length - 1);
        value += chars[num];
      }
      return value;
    }

    protected T RandomNumber<T>()
    {
      if (typeof(T) == typeof(double))
      {
        return (T) Convert.ChangeType(_random.NextDouble(), typeof(T));
      }

      if (typeof(T) == typeof(char))
      {
        var buffer = new byte[2]; //  2 bytes per char.
        _random.NextBytes(buffer);
        return (T)Convert.ChangeType(BitConverter.ToChar(buffer, 0), typeof(T));
      }

      if (typeof(T) == typeof(byte))
      {
        var buffer = new byte[1]; //  2 bytes per char.
        _random.NextBytes(buffer);
        return (T)Convert.ChangeType(buffer[0], typeof(T));
      }

      if (typeof(T) == typeof(float))
      {
        var buffer = new byte[4];
        _random.NextBytes(buffer);
        return (T)Convert.ChangeType(BitConverter.ToSingle(buffer, 0), typeof(T));
      }

      if (typeof(T) == typeof(long))
      {
        var buffer = new byte[8];
        _random.NextBytes(buffer);
        return (T)Convert.ChangeType(BitConverter.ToInt64(buffer, 0), typeof(T));
      }

      if (typeof(T) == typeof(int) ||
          typeof(T) == typeof(uint))
      {
        return (T)Convert.ChangeType(_random.Next(), typeof(T));
      }

      throw new NotSupportedException();
    }
  }
}
