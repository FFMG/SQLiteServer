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
using System.Net;
using NUnit.Framework;
using SQLiteServer.Data.SQLiteServer;

namespace SQLiteServer.Test.SQLiteServer
{
  [TestFixture]
  internal class ConnectionTests
  {
    private static readonly IPAddress Address = IPAddress.Parse("127.0.0.1");
    private const int Port = 1100;
    private const int Backlog = 500;
    private const int HeartBeatTimeOut = 500;

    private string _source;
    private readonly List<SQLiteServerConnection> _connections = new List<SQLiteServerConnection>();

    [SetUp]
    public void SetUp()
    {
      _source = Path.GetTempFileName();
    }

    [TearDown]
    public void TearDown()
    {
      try
      {
        foreach (var connection in _connections)
        {
          if (connection.State == ConnectionState.Open)
          {
            connection.Close();
          }
        }
        File.Delete(_source);
      }
      catch
      {
        // ignored
      }
    }

    protected SQLiteServerConnection CreateConnection()
    {
      var connection = new SQLiteServerConnection($"Data Source={_source};Version=3;", Address, Port, Backlog, HeartBeatTimeOut);
      _connections.Add(connection);
      return connection;
    }

    [Test]
    public void TryingToOpenAnAlreadyOpenDatabase()
    {
      var server = CreateConnection();
      server.Open();
      Assert.Throws<InvalidOperationException>(() => { server.Open(); });
      server.Close();
    }

    [Test]
    public void OpenDatabaseIsOpen()
    {
      var server = CreateConnection();
      server.Open();
      // make sure it is open.
      Assert.AreEqual(ConnectionState.Open, server.State);
      server.Close();
    }

    [Test]
    public void BadDatabaseNameBecomesBroken()
    {
      var server = new SQLiteServerConnection("Data Source=;Version=3;", Address, Port, Backlog, HeartBeatTimeOut);
      Assert.Throws<ArgumentException>(() => { server.Open(); });

      // make sure it is broken.
      Assert.AreEqual( ConnectionState.Broken, server.State );

      // it is broken, so it will not work.
      Assert.Throws<InvalidOperationException>(() => { server.Open(); });
    }
  }
}
