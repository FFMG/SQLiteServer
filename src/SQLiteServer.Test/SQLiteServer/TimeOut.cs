using NUnit.Framework;
using SQLiteServer.Data.SQLiteServer;

namespace SQLiteServer.Test.SQLiteServer
{
  internal class TimeOut : Common
  {
    [Test]
    public void CheckBusyTimeout()
    {
      var con = CreateConnection();
      const string sql = @"WITH RECURSIVE r(i) AS (
                  VALUES(0)
                  UNION ALL
                  SELECT i FROM r
                  LIMIT 1000000
                )
                SELECT i FROM r WHERE i = 1;";

      con.Open();

      using (var command = new SQLiteServerCommand(sql, con))
      {
        Assert.That(1 == command.ExecuteNonQuery());
      }
      con.Close();
    }
  }
}
