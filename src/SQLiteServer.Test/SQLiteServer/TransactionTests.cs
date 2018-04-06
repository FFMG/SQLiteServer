using NUnit.Framework;
using SQLiteServer.Data.Exceptions;
using SQLiteServer.Data.SQLiteServer;

namespace SQLiteServer.Test.SQLiteServer
{
  internal class TransactionTests : Common
  {
    [Test]
    public void SimpleServerTransaction()
    {
      var server = CreateConnection();
      server.Open();
      const string sqlMaster = "create table tb_config (name varchar(20), value INTEGER)";
      using (var command = new SQLiteServerCommand(sqlMaster, server))
      {
        command.ExecuteNonQuery();
      }

      const int numRows = 5;
      var trans = server.BeginTransaction();
      for (var i = 0; i < numRows; i++)
      {
        var sql = $"insert into tb_config(name, value) VALUES ('a', {i} )";
        using (var command = new SQLiteServerCommand(sql, server))
        {
          command.ExecuteNonQuery();
        }
      }
      trans.Commit();
      server.Close();

      server = CreateConnection();
      server.Open();
      const string select = "select count(*) FROM tb_config";
      using (var command = new SQLiteServerCommand(select, server))
      {
        using (var reader = command.ExecuteReader())
        {
          Assert.IsTrue(reader.HasRows);
          while (reader.Read())
          {
            Assert.AreEqual(numRows, reader.GetInt16(0));
          }
        }
      }
      server.Close();

    }

    [Test]
    public void SimpleClientTransaction()
    {
      var server = CreateConnection();
      server.Open();

      var client = CreateConnection();
      client.Open();

      const string sqlMaster = "create table tb_config (name varchar(20), value INTEGER)";
      using (var command = new SQLiteServerCommand(sqlMaster, client))
      {
        command.ExecuteNonQuery();
      }

      const int numRows = 5;
      var trans = client.BeginTransaction();
      for (var i = 0; i < numRows; i++)
      {
        var sql = $"insert into tb_config(name, value) VALUES ('a', {i} )";
        using (var command = new SQLiteServerCommand(sql, client))
        {
          command.ExecuteNonQuery();
        }
      }
      trans.Commit();
      client.Close();

      client = CreateConnection();
      client.Open();
      const string select = "select count(*) FROM tb_config";
      using (var command = new SQLiteServerCommand(select, client))
      {
        using (var reader = command.ExecuteReader())
        {
          Assert.IsTrue(reader.HasRows);
          while (reader.Read())
          {
            Assert.AreEqual(numRows, reader.GetInt16(0));
          }
        }
      }

      client.Close();
      server.Close();
    }

    [Test]
    public void ClosingConnectionWithOpenTransactions()
    {
      var server = CreateConnection();
      server.Open();
      var client = CreateConnection();
      client.Open();

      const string sqlMaster = "create table tb_config (name varchar(20), value INTEGER)";
      using (var command = new SQLiteServerCommand(sqlMaster, client))
      {
        command.ExecuteNonQuery();
      }

      client.BeginTransaction();
      for (var i = 0; i < 5; i++)
      {
        var sql = $"insert into tb_config(name, value) VALUES ('a', {i} )";
        using (var command = new SQLiteServerCommand(sql, client))
        {
          command.ExecuteNonQuery();
        }
      }
      // we are now going to close without commiting
      client.Close();

      const string select = "select * FROM tb_config";
      using (var command = new SQLiteServerCommand(select, server))
      {
        using (var reader = command.ExecuteReader())
        {
          Assert.IsFalse( reader.HasRows );
        }
      }
      server.Close();
    }

    [Test]
    public void ServerTryingToCreateTransactionWhileClientHasTransaction()
    {
      var server = CreateConnection();
      server.Open();

      var client = CreateConnection();
      client.Open();

      var trans = client.BeginTransaction();
      Assert.Throws<SQLiteServerException>( () =>
      {
        server.BeginTransaction();
      });
      trans.Commit();
      client.Close();
      
      server.Close();
    }

    [Test]
    public void ClientTryingToCreateTransactionWhileServerHasTransaction()
    {
      var server = CreateConnection();
      server.Open();

      var client = CreateConnection();
      client.Open();

      var trans = server.BeginTransaction();
      Assert.Throws<SQLiteServerException>(() =>
      {
        client.BeginTransaction();
      });
      trans.Commit();
      client.Close();
      server.Close();
    }

    [Test]
    public void ServerTryingToCreateMoreThanOneTransaction()
    {
      var server = CreateConnection();
      server.Open();

      var trans1 = server.BeginTransaction();
      Assert.Throws<SQLiteServerException>(() =>
      {
        server.BeginTransaction();
      });
      trans1.Commit();
      server.Close();
    }
  }
}
