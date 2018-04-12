using NUnit.Framework;
using SQLiteServer.Data.SQLiteServer;

namespace SQLiteServer.Test.SQLiteServer
{
  internal class ConnectionStringBuilderTests
  {
    [Test]
    public void CreateWithNoArgument()
    {
      Assert.That( () => new SQLiteServerConnectionStringBuilder(), Throws.Nothing );
    }

    [Test]
    public void CreateWithDataSourceMemoryArgument()
    {
      var sql = new SQLiteServerConnectionStringBuilder("Data Source=:memory:");
      Assert.AreEqual(":memory:", sql["Data Source"]);
      Assert.AreEqual(":memory:", sql.DataSource);
    }
  }
}
