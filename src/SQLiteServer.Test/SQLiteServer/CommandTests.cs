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
using System.IO;
using System.Net;
using NUnit.Framework;
using SQLiteServer.Data.SQLiteServer;

namespace SQLiteServer.Test.SQLiteServer
{
  [TestFixture]
  internal class CommandTests
  {
    private static readonly IPAddress Address = IPAddress.Parse("127.0.0.1");
    private const int Port = 1100;
    private const int Backlog = 500;
    private const int HeartBeatTimeOut = 500;

    private string _source;

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
        File.Delete(_source);
      }
      catch
      {
        // ignored
      }
    }

    protected SQLiteServerConnection CreateConnection()
    {
      return new SQLiteServerConnection($"Data Source={_source};Version=3;", Address, Port, Backlog, HeartBeatTimeOut);
    }

    [Test]
    public void DbConnectionIsNull()
    {
      var command = new SQLiteServerCommand();
      Assert.Throws<InvalidOperationException>( () =>
      {
        command.ExecuteNonQuery();
      });
    }

    [Test]
    public void CommandTextIsNull()
    {
      // make sure that the connection is valid.
      var server = CreateConnection();
      server.Open();
      var command = new SQLiteServerCommand(server);
      Assert.Throws<InvalidOperationException>(() =>
      {
        command.ExecuteNonQuery();
      });
      server.Close();
    }

    [Test]
    public void CommandTextIsEmpty()
    {
      // make sure that the connection is valid.
      var server = CreateConnection();
      server.Open();
      var command = new SQLiteServerCommand(server);
      Assert.Throws<InvalidOperationException>(() =>
      {
        command.ExecuteNonQuery();
      });
      server.Close();
    }

    [Test]
    public void CommandTextIsSpaces()
    {
      // make sure that the connection is valid.
      var server = CreateConnection();
      server.Open();
      var command = new SQLiteServerCommand(server);
      Assert.Throws<InvalidOperationException>(() =>
      {
        command.ExecuteNonQuery();
      });
      server.Close();
    }
  }
}
