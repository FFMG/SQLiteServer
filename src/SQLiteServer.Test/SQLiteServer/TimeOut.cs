using NUnit.Framework;
using SQLiteServer.Data.Connections;
using SQLiteServer.Data.SQLiteServer;

namespace SQLiteServer.Test.SQLiteServer
{
  internal class TimeOut : Common
  {
    [Test]
    public void CheckBusyTimeout()
    {
      const int timeout = 1000;
      var shortTimeout1 = new SocketConnectionBuilder(timeout, Address, Port, Backlog, HeartBeatTimeOut);
      var shortTimeout2 = new SocketConnectionBuilder(timeout, Address, Port, Backlog, HeartBeatTimeOut);
      var con1 = CreateConnection(shortTimeout1);
      var con2 = CreateConnection(shortTimeout2);

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
        Assert.AreEqual(-1 , command.ExecuteNonQuery());
      }
      con2.Close();
      con1.Close();
    }
  }
}
