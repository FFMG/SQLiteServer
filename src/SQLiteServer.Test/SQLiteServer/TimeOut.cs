using System;
using NUnit.Framework;
using SQLiteServer.Data.Connections;
using SQLiteServer.Data.SQLiteServer;

namespace SQLiteServer.Test.SQLiteServer
{
  internal class TimeOut : Common
  {
    [Test]
    public void ClientZeroTimeoutNeverErrors()
    {
      const int timeout = 0;
      var shortTimeout1 = new SocketConnectionBuilder(Address, Port, Backlog, HeartBeatTimeOut);
      var shortTimeout2 = new SocketConnectionBuilder(Address, Port, Backlog, HeartBeatTimeOut);
      var con1 = CreateConnection(shortTimeout1, timeout);
      var con2 = CreateConnection(shortTimeout2, timeout);

      const string sql = @"WITH RECURSIVE r(i) AS (
                  VALUES(0)
                  UNION ALL
                  SELECT i FROM r
                  LIMIT 25000000
                )
                SELECT i FROM r WHERE i = 1;";

      con1.Open();
      con2.Open();
      using (var command = new SQLiteServerCommand(sql, con2))
      {
        // even with no timeout, we should never get an error
        Assert.AreEqual(-1, command.ExecuteNonQuery());
      }
      con2.Close();
      con1.Close();
    }

    [Test]
    public void ServerZeroTimeoutNeverErrors()
    {
      const int timeout = 0;
      var shortTimeout1 = new SocketConnectionBuilder(Address, Port, Backlog, HeartBeatTimeOut);
      var con1 = CreateConnection(shortTimeout1, timeout);

      const string sql = @"WITH RECURSIVE r(i) AS (
                  VALUES(0)
                  UNION ALL
                  SELECT i FROM r
                  LIMIT 25000000
                )
                SELECT i FROM r WHERE i = 1;";

      con1.Open();
      using (var command = new SQLiteServerCommand(sql, con1))
      {
        // even with no timeout, we should never get an error
        Assert.AreEqual(-1, command.ExecuteNonQuery());
      }
      con1.Close();
    }

    [Test]
    public void ClientCheckBusyTimeout()
    {
      const int timeout = 1;
      var shortTimeout1 = new SocketConnectionBuilder(Address, Port, Backlog, HeartBeatTimeOut);
      var shortTimeout2 = new SocketConnectionBuilder(Address, Port, Backlog, HeartBeatTimeOut);
      var con1 = CreateConnection(shortTimeout1, timeout);
      var con2 = CreateConnection(shortTimeout2, timeout);

      const string sql = @"WITH RECURSIVE r(i) AS (
                  VALUES(0)
                  UNION ALL
                  SELECT i FROM r
                  LIMIT 25000000
                )
                SELECT i FROM r WHERE i = 1;";

      con1.Open();
      con2.Open();
      using (var command = new SQLiteServerCommand(sql, con2))
      {
        Assert.Throws<TimeoutException>(() => command.ExecuteNonQuery());
      }
      con2.Close();
      con1.Close();
    }

    [Test]
    public void ServerCheckBusyTimeout()
    {
      const int timeout = 1;
      var shortTimeout1 = new SocketConnectionBuilder(Address, Port, Backlog, HeartBeatTimeOut);
      var con1 = CreateConnection(shortTimeout1, timeout);

      const string sql = @"WITH RECURSIVE r(i) AS (
                  VALUES(0)
                  UNION ALL
                  SELECT i FROM r
                  LIMIT 25000000
                )
                SELECT i FROM r WHERE i = 1;";

      con1.Open();
      using (var command = new SQLiteServerCommand(sql, con1))
      {
        Assert.Throws<TimeoutException>( () => command.ExecuteNonQuery());
      }
      con1.Close();
    }

    [Test]
    public void ClientCheckLongDefaultTimeout()
    {
      const int timeout = 60;
      var shortTimeout1 = new SocketConnectionBuilder(Address, Port, Backlog, HeartBeatTimeOut);
      var shortTimeout2 = new SocketConnectionBuilder(Address, Port, Backlog, HeartBeatTimeOut);
      var con1 = CreateConnection(shortTimeout1, timeout);
      var con2 = CreateConnection(shortTimeout2, timeout);

      const string sql = @"WITH RECURSIVE r(i) AS (
                  VALUES(0)
                  UNION ALL
                  SELECT i FROM r
                  LIMIT 25000000
                )
                SELECT i FROM r WHERE i = 1;";

      con1.Open();
      con2.Open();
      using (var command = new SQLiteServerCommand(sql, con2))
      {
        Assert.AreEqual(-1, command.ExecuteNonQuery());
      }
      con2.Close();
      con1.Close();
    }

    [Test]
    public void ServerCheckLongDefaultTimeout()
    {
      const int timeout = 60;
      var shortTimeout1 = new SocketConnectionBuilder(Address, Port, Backlog, HeartBeatTimeOut);
      var con1 = CreateConnection(shortTimeout1, timeout);

      const string sql = @"WITH RECURSIVE r(i) AS (
                  VALUES(0)
                  UNION ALL
                  SELECT i FROM r
                  LIMIT 25000000
                )
                SELECT i FROM r WHERE i = 1;";

      con1.Open();
      using (var command = new SQLiteServerCommand(sql, con1))
      {
        Assert.AreEqual(-1, command.ExecuteNonQuery());
      }
      con1.Close();
    }
  }
}
