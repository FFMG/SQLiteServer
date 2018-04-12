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
using System.Data;
using NUnit.Framework;
using SQLiteServer.Data.SQLiteServer;

namespace SQLiteServer.Test.SQLiteServer
{
  [TestFixture]
  internal class ConnectionTests : Common
  {
    [Test]
    public void TryingToOpenAnAlreadyOpenDatabase()
    {
      var server = CreateConnection();
      server.Open();
      Assert.Throws<InvalidOperationException>(() => { server.Open(); });
      server.Close();
    }

    [Test]
    public void TryingToRunCodeWhenDatabaseIsNotOpen()
    {
      var con = CreateConnection();
      const string sql = "select 1;";
      using (var command = new SQLiteServerCommand(sql, con))
      {
        Assert.Throws<InvalidOperationException>(() =>
        {
          command.ExecuteNonQuery();
        });
      }
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
      var server = new SQLiteServerConnection("Data Source=;Version=3;", QueryTimeoutMs, Address, Port, Backlog, HeartBeatTimeOut);
      Assert.Throws<ArgumentException>(() => { server.Open(); });

      // make sure it is broken.
      Assert.AreEqual( ConnectionState.Broken, server.State );

      // it is broken, so it will not work.
      Assert.Throws<InvalidOperationException>(() => { server.Open(); });
    }

    [Test]
    public void CloseTheServerConnectionMoreThanOnce()
    {
      var server = CreateConnection();
      server.Open();

      // make sure it is open.
      Assert.AreEqual(ConnectionState.Open, server.State);

      server.Close();
      Assert.Throws<InvalidOperationException>( () =>
      {
        server.Close();
      });
    }

    [Test]
    public void CloseTheClientConnectionMoreThanOnce()
    {
      var server = CreateConnection();
      server.Open();

      var client = CreateConnection();
      client.Open();

      // make sure it is open.
      Assert.AreEqual(ConnectionState.Open, server.State);

      client.Close();
      Assert.Throws<InvalidOperationException>(() =>
      {
        client.Close();
      });
      server.Close();
    }

    [Test]
    public void ClosedClientDoesNotReOpenBecauseServerIsClosing()
    {
      var server = CreateConnection();
      server.Open();

      var client = CreateConnection();
      client.Open();

      // make sure it is open.
      Assert.AreEqual(ConnectionState.Open, server.State);

      client.Close();
      server.Close();

      Assert.AreEqual(ConnectionState.Closed, client.State);
      Assert.AreEqual(ConnectionState.Closed, server.State);
    }

    [Test]
    public void TheServerClosesJustBeforeTheClient()
    {
      var server = CreateConnection();
      server.Open();
      var client = CreateConnection();
      client.Open();

      // make sure it is open.
      Assert.AreEqual(ConnectionState.Open, server.State);
      Assert.AreEqual(ConnectionState.Open, client.State);

      server.Close();
      client.Close();

      Assert.AreEqual(ConnectionState.Closed, server.State);
      Assert.AreEqual(ConnectionState.Closed, client.State);
    }
  }
}
