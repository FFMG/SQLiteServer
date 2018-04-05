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
using System.IO;
using System.Net;
using NUnit.Framework;
using SQLiteServer.Data.Exceptions;
using SQLiteServer.Data.SQLiteServer;

namespace SQLiteServer.Test.SQLiteServer
{
  [TestFixture]
  internal class ReaderTests
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
    public void ClientGetLongValue()
    {
      var server = CreateConnection();
      server.Open();
      const string sqlMaster = "create table tb_config (name varchar(20), value INTEGER)";
      using (var command = new SQLiteServerCommand(sqlMaster, server))
      {
        command.ExecuteNonQuery();
      }

      var client = CreateConnection();
      client.Open();
      const string sqlInsert1 = "insert into tb_config(name, value) VALUES ('a', '10')";
      using (var command = new SQLiteServerCommand(sqlInsert1, client))
      {
        command.ExecuteNonQuery();
      }
      const string sqlInsert2 = "insert into tb_config(name, value) VALUES ('b', '20')";
      using (var command = new SQLiteServerCommand(sqlInsert2, client))
      {
        command.ExecuteNonQuery();
      }

      const string sqlSelect = "SELECT * FROM tb_config";
      using (var command = new SQLiteServerCommand(sqlSelect, client))
      {
        using (var reader = command.ExecuteReader())
        {
          var i = 0;
          while (reader.Read())
          {
            if (i == 0)
            {
              Assert.AreEqual("a", reader.GetString(0));
              Assert.AreEqual(10, reader.GetInt64(1));
            }
            else
            {
              Assert.AreEqual("b", reader.GetString(0));
              Assert.AreEqual(20, reader.GetInt64(1));
            }

            ++i;
          }
        }
      }
      client.Close();
      server.Close();
    }

    [Test]
    public void ClientFieldCount()
    {
      var server = CreateConnection();
      server.Open();
      const string sqlMaster = "create table tb_config (name varchar(20), value REAL)";
      using (var command = new SQLiteServerCommand(sqlMaster, server))
      {
        command.ExecuteNonQuery();
      }

      var client = CreateConnection();
      client.Open();
      const string sqlInsert1 = "insert into tb_config(name, value) VALUES ('a', 3.14)";
      using (var command = new SQLiteServerCommand(sqlInsert1, client))
      {
        command.ExecuteNonQuery();
      }
      const string sqlInsert2 = "insert into tb_config(name, value) VALUES ('b', 1.1)";
      using (var command = new SQLiteServerCommand(sqlInsert2, client))
      {
        command.ExecuteNonQuery();
      }

      const string sqlSelect = "SELECT * FROM tb_config";
      using (var command = new SQLiteServerCommand(sqlSelect, client))
      {
        using (var reader = command.ExecuteReader())
        {
          Assert.IsTrue(reader.Read());
          Assert.AreEqual(2, reader.FieldCount);

          Assert.IsTrue(reader.Read());
          Assert.AreEqual(2, reader.FieldCount);
        }
      }
      client.Close();
      server.Close();
    }

    [Test]
    public void ClientGetDoubleValue()
    {
      var server = CreateConnection();
      server.Open();
      const string sqlMaster = "create table tb_config (name varchar(20), value REAL)";
      using (var command = new SQLiteServerCommand(sqlMaster, server))
      {
        command.ExecuteNonQuery();
      }

      var client = CreateConnection();
      client.Open();
      const string sqlInsert1 = "insert into tb_config(name, value) VALUES ('a', 3.14)";
      using (var command = new SQLiteServerCommand(sqlInsert1, client))
      {
        command.ExecuteNonQuery();
      }
      const string sqlInsert2 = "insert into tb_config(name, value) VALUES ('b', 1.1)";
      using (var command = new SQLiteServerCommand(sqlInsert2, client))
      {
        command.ExecuteNonQuery();
      }

      const string sqlSelect = "SELECT * FROM tb_config";
      using (var command = new SQLiteServerCommand(sqlSelect, client))
      {
        using (var reader = command.ExecuteReader())
        {
          Assert.IsTrue(reader.Read());
          Assert.AreEqual("a", reader.GetString(0));
          Assert.AreEqual(3.14, reader.GetDouble(1));

          Assert.IsTrue(reader.Read());
          Assert.AreEqual("b", reader.GetString(0));
          Assert.AreEqual(1.1, reader.GetDouble(1));
        }
      }
      client.Close();
      server.Close();
    }

    [Test]
    public void ServerGetDoubleValue()
    {
      var server = CreateConnection();
      server.Open();
      const string sqlMaster = "create table tb_config (name varchar(20), value REAL)";
      using (var command = new SQLiteServerCommand(sqlMaster, server))
      {
        command.ExecuteNonQuery();
      }

      const string sqlInsert1 = "insert into tb_config(name, value) VALUES ('a', 3.14)";
      using (var command = new SQLiteServerCommand(sqlInsert1, server))
      {
        command.ExecuteNonQuery();
      }
      const string sqlInsert2 = "insert into tb_config(name, value) VALUES ('b', 1.1)";
      using (var command = new SQLiteServerCommand(sqlInsert2, server))
      {
        command.ExecuteNonQuery();
      }

      const string sqlSelect = "SELECT * FROM tb_config";
      using (var command = new SQLiteServerCommand(sqlSelect, server))
      {
        using (var reader = command.ExecuteReader())
        {
          Assert.IsTrue(reader.Read());
          Assert.AreEqual("a", reader.GetString(0));
          Assert.AreEqual(3.14, reader.GetDouble(1));

          Assert.IsTrue(reader.Read());
          Assert.AreEqual("b", reader.GetString(0));
          Assert.AreEqual(1.1, reader.GetDouble(1));
        }
      }
      server.Close();
    }

    [Test]
    public void ServerFieldCount()
    {
      var server = CreateConnection();
      server.Open();
      const string sqlMaster = "create table tb_config (name varchar(20), value REAL)";
      using (var command = new SQLiteServerCommand(sqlMaster, server))
      {
        command.ExecuteNonQuery();
      }

      const string sqlInsert1 = "insert into tb_config(name, value) VALUES ('a', 3.14)";
      using (var command = new SQLiteServerCommand(sqlInsert1, server))
      {
        command.ExecuteNonQuery();
      }
      const string sqlInsert2 = "insert into tb_config(name, value) VALUES ('b', 1.1)";
      using (var command = new SQLiteServerCommand(sqlInsert2, server))
      {
        command.ExecuteNonQuery();
      }

      const string sqlSelect = "SELECT * FROM tb_config";
      using (var command = new SQLiteServerCommand(sqlSelect, server))
      {
        using (var reader = command.ExecuteReader())
        {
          Assert.IsTrue(reader.Read());
          Assert.AreEqual(2, reader.FieldCount);

          Assert.IsTrue(reader.Read());
          Assert.AreEqual(2, reader.FieldCount);
        }
      }
      server.Close();
    }

    [Test]
    public void ServerFieldCountBeforeRead()
    {
      var server = CreateConnection();
      server.Open();
      const string sqlMaster = "create table tb_config (name varchar(20), value REAL)";
      using (var command = new SQLiteServerCommand(sqlMaster, server))
      {
        command.ExecuteNonQuery();
      }

      const string sqlSelect = "SELECT * FROM tb_config";
      using (var command = new SQLiteServerCommand(sqlSelect, server))
      {
        using (var reader = command.ExecuteReader())
        {
          Assert.AreEqual(2, reader.FieldCount);
        }
      }
      server.Close();
    }

    [Test]
    public void ClientFieldCountBeforeRead()
    {
      var server = CreateConnection();
      server.Open();
      const string sqlMaster = "create table tb_config (name varchar(20), value REAL)";
      using (var command = new SQLiteServerCommand(sqlMaster, server))
      {
        command.ExecuteNonQuery();
      }

      var client = CreateConnection();
      client.Open();
      const string sqlSelect = "SELECT * FROM tb_config";
      using (var command = new SQLiteServerCommand(sqlSelect, client))
      {
        using (var reader = command.ExecuteReader())
        {
          Assert.AreEqual(2, reader.FieldCount);
        }
      }
      client.Close();
      server.Close();
    }

    [Test]
    public void ServerGetShortValue()
    {
      var server = CreateConnection();
      server.Open();
      const string sqlMaster = "create table tb_config (name varchar(20), value INTEGER)";
      using (var command = new SQLiteServerCommand(sqlMaster, server))
      {
        command.ExecuteNonQuery();
      }

      const string sqlInsert1 = "insert into tb_config(name, value) VALUES ('a', '10')";
      using (var command = new SQLiteServerCommand(sqlInsert1, server))
      {
        command.ExecuteNonQuery();
      }
      const string sqlInsert2 = "insert into tb_config(name, value) VALUES ('b', '20')";
      using (var command = new SQLiteServerCommand(sqlInsert2, server))
      {
        command.ExecuteNonQuery();
      }

      const string sqlSelect = "SELECT * FROM tb_config";
      using (var command = new SQLiteServerCommand(sqlSelect, server))
      {
        using (var reader = command.ExecuteReader())
        {
          Assert.IsTrue(reader.Read());
          Assert.AreEqual(10, reader.GetInt16(1));

          Assert.IsTrue(reader.Read());
          Assert.AreEqual(20, reader.GetInt16(1));

          Assert.IsFalse(reader.Read());
        }
      }
      server.Close();
    }

    [Test]
    public void ClientGetShortValue()
    {
      var server = CreateConnection();
      server.Open();
      const string sqlMaster = "create table tb_config (name varchar(20), value INTEGER)";
      using (var command = new SQLiteServerCommand(sqlMaster, server))
      {
        command.ExecuteNonQuery();
      }

      var client = CreateConnection();
      client.Open();
      const string sqlInsert1 = "insert into tb_config(name, value) VALUES ('a', '10')";
      using (var command = new SQLiteServerCommand(sqlInsert1, client))
      {
        command.ExecuteNonQuery();
      }
      const string sqlInsert2 = "insert into tb_config(name, value) VALUES ('b', '20')";
      using (var command = new SQLiteServerCommand(sqlInsert2, client))
      {
        command.ExecuteNonQuery();
      }

      const string sqlSelect = "SELECT * FROM tb_config";
      using (var command = new SQLiteServerCommand(sqlSelect, client))
      {
        using (var reader = command.ExecuteReader())
        {
          Assert.IsTrue(reader.Read());
          Assert.AreEqual(10, reader.GetInt16(1));

          Assert.IsTrue(reader.Read());
          Assert.AreEqual(20, reader.GetInt16(1));

          Assert.IsFalse(reader.Read());
        }
      }
      client.Close();
      server.Close();
    }

    [Test]
    public void ClientGetLongValueByName()
    {
      var server = CreateConnection();
      server.Open();
      const string sqlMaster = "create table tb_config (name varchar(20), value INTEGER)";
      using (var command = new SQLiteServerCommand(sqlMaster, server))
      {
        command.ExecuteNonQuery();
      }

      var client = CreateConnection();
      client.Open();
      const string sqlInsert1 = "insert into tb_config(name, value) VALUES ('a', '10')";
      using (var command = new SQLiteServerCommand(sqlInsert1, client))
      {
        command.ExecuteNonQuery();
      }
      const string sqlInsert2 = "insert into tb_config(name, value) VALUES ('b', '20')";
      using (var command = new SQLiteServerCommand(sqlInsert2, client))
      {
        command.ExecuteNonQuery();
      }

      const string sqlSelect = "SELECT * FROM tb_config";
      using (var command = new SQLiteServerCommand(sqlSelect, client))
      {
        using (var reader = command.ExecuteReader())
        {
          Assert.IsTrue(reader.Read());
          Assert.AreEqual("a", reader["name"]);
          Assert.AreEqual(10, reader["value"]);

          Assert.IsTrue(reader.Read());
          Assert.AreEqual("b", reader["name"]);
          Assert.AreEqual(20, reader["value"]);

          Assert.IsFalse(reader.Read());
        }
      }
      client.Close();
      server.Close();
    }

    [Test]
    public void ServerGetLongValue()
    {
      var server = CreateConnection();
      server.Open();
      const string sqlMaster = "create table tb_config (name varchar(20), value INTEGER)";
      using (var command = new SQLiteServerCommand(sqlMaster, server))
      {
        command.ExecuteNonQuery();
      }

      const string sqlInsert1 = "insert into tb_config(name, value) VALUES ('a', '10')";
      using (var command = new SQLiteServerCommand(sqlInsert1, server))
      {
        command.ExecuteNonQuery();
      }
      const string sqlInsert2 = "insert into tb_config(name, value) VALUES ('b', '20')";
      using (var command = new SQLiteServerCommand(sqlInsert2, server))
      {
        command.ExecuteNonQuery();
      }

      const string sqlSelect = "SELECT * FROM tb_config";
      using (var command = new SQLiteServerCommand(sqlSelect, server))
      {
        using (var reader = command.ExecuteReader())
        {
          var i = 0;
          while (reader.Read())
          {
            if (i == 0)
            {
              Assert.AreEqual("a", reader.GetString(0));
              Assert.AreEqual(10, reader.GetInt64(1));
            }
            else
            {
              Assert.AreEqual("b", reader.GetString(0));
              Assert.AreEqual(20, reader.GetInt64(1));
            }

            ++i;
          }
        }
      }
      server.Close();
    }

    [Test]
    public void ServerGetValueByName()
    {
      var server = CreateConnection();
      server.Open();
      const string sqlMaster = "create table tb_config (name varchar(20), value INTEGER)";
      using (var command = new SQLiteServerCommand(sqlMaster, server))
      {
        command.ExecuteNonQuery();
      }

      const string sqlInsert1 = "insert into tb_config(name, value) VALUES ('a', '10')";
      using (var command = new SQLiteServerCommand(sqlInsert1, server))
      {
        command.ExecuteNonQuery();
      }
      const string sqlInsert2 = "insert into tb_config(name, value) VALUES ('b', '20')";
      using (var command = new SQLiteServerCommand(sqlInsert2, server))
      {
        command.ExecuteNonQuery();
      }

      const string sqlSelect = "SELECT * FROM tb_config";
      using (var command = new SQLiteServerCommand(sqlSelect, server))
      {
        using (var reader = command.ExecuteReader())
        {
          Assert.IsTrue(reader.Read());
          Assert.AreEqual("a", reader["name"]);
          Assert.AreEqual(10, reader["value"]);

          Assert.IsTrue(reader.Read());
          Assert.AreEqual("b", reader["name"]);
          Assert.AreEqual(20, reader["value"]);

          Assert.IsFalse(reader.Read());
        }
      }
      server.Close();
    }

    [Test]
    public void ServerTryingToReadStringBeforeReadIsCalled()
    {
      var server = CreateConnection();
      server.Open();
      const string sqlMaster = "create table tb_config (name varchar(20), value INTEGER)";
      using (var command = new SQLiteServerCommand(sqlMaster, server))
      {
        command.ExecuteNonQuery();
      }
      
      const string sqlSelect = "SELECT * FROM tb_config";
      using (var command = new SQLiteServerCommand(sqlSelect, server))
      {
        using (var reader = command.ExecuteReader())
        {
          // we did not call read.
          Assert.Throws<SQLiteServerException>( 
            () =>
            {
              reader.GetString(0);
            });
        }
      }
      server.Close();
    }

    [Test]
    public void ServerGetOrdinal()
    {
      var server = CreateConnection();
      server.Open();
      const string sqlMaster = "create table tb_config (name varchar(20), value INTEGER)";
      using (var command = new SQLiteServerCommand(sqlMaster, server))
      {
        command.ExecuteNonQuery();
      }

      const string sqlSelect = "SELECT name, value FROM tb_config";
      using (var command = new SQLiteServerCommand(sqlSelect, server))
      {
        using (var reader = command.ExecuteReader())
        {
          Assert.AreEqual( 0, reader.GetOrdinal("name"));
          Assert.AreEqual(1, reader.GetOrdinal("value"));
        }
      }
      server.Close();
    }

    [Test]
    public void ClientGetOrdinal()
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

      const string sqlSelect = "SELECT name, value FROM tb_config";
      using (var command = new SQLiteServerCommand(sqlSelect, client))
      {
        using (var reader = command.ExecuteReader())
        {
          // we did not call read.
          Assert.AreEqual(0, reader.GetOrdinal("name"));
          Assert.AreEqual(1, reader.GetOrdinal("value"));
        }
      }
      server.Close();
      client.Close();
    }

    [Test]
    public void ClientGetOrdinalCaseInsensitive()
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

      const string sqlSelect = "SELECT name, value FROM tb_config";
      using (var command = new SQLiteServerCommand(sqlSelect, client))
      {
        using (var reader = command.ExecuteReader())
        {
          // we did not call read.
          Assert.AreEqual(0, reader.GetOrdinal("name"));
          Assert.AreEqual(0, reader.GetOrdinal("NAME"));
          Assert.AreEqual(0, reader.GetOrdinal("Name"));
          Assert.AreEqual(0, reader.GetOrdinal("NaMe"));
        }
      }
      server.Close();
      client.Close();
    }

    [Test]
    public void ClientTryingToReadStringBeforeReadIsCalled()
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

      const string sqlSelect = "SELECT * FROM tb_config";
      using (var command = new SQLiteServerCommand(sqlSelect, client))
      {
        using (var reader = command.ExecuteReader())
        {
          // we did not call read.
          Assert.Throws<SQLiteServerException>(
            () =>
            {
              reader.GetString(0);
            });
        }
      }
      server.Close();
      client.Close();
    }

    [Test]
    public void ServerIsDBNull()
    {
      var server = CreateConnection();
      server.Open();
      const string sqlMaster = "create table tb_config (name varchar(20), value INTEGER)";
      using (var command = new SQLiteServerCommand(sqlMaster, server))
      {
        command.ExecuteNonQuery();
      }

      const string sqlInsert1 = "insert into tb_config(name, value) VALUES ('a', NULL )";
      using (var command = new SQLiteServerCommand(sqlInsert1, server))
      {
        command.ExecuteNonQuery();
      }
      const string sqlInsert2 = "insert into tb_config(name, value) VALUES ('b', '20')";
      using (var command = new SQLiteServerCommand(sqlInsert2, server))
      {
        command.ExecuteNonQuery();
      }

      const string sqlSelect = "SELECT * FROM tb_config";
      using (var command = new SQLiteServerCommand(sqlSelect, server))
      {
        using (var reader = command.ExecuteReader())
        {
          Assert.IsTrue(reader.Read());
          Assert.IsTrue(reader.IsDBNull(1));

          Assert.IsTrue(reader.Read());
          Assert.IsFalse(reader.IsDBNull(1));

          Assert.IsFalse(reader.Read());
        }
      }
      server.Close();
    }

    [Test]
    public void ClientIsDBNull()
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

      const string sqlInsert1 = "insert into tb_config(name, value) VALUES ('a', NULL )";
      using (var command = new SQLiteServerCommand(sqlInsert1, client))
      {
        command.ExecuteNonQuery();
      }
      const string sqlInsert2 = "insert into tb_config(name, value) VALUES ('b', '20')";
      using (var command = new SQLiteServerCommand(sqlInsert2, client))
      {
        command.ExecuteNonQuery();
      }

      const string sqlSelect = "SELECT * FROM tb_config";
      using (var command = new SQLiteServerCommand(sqlSelect, client))
      {
        using (var reader = command.ExecuteReader())
        {
          Assert.IsTrue(reader.Read());
          Assert.IsTrue(reader.IsDBNull(1));

          Assert.IsTrue(reader.Read());
          Assert.IsFalse(reader.IsDBNull(1));

          Assert.IsFalse(reader.Read());
        }
      }

      client.Close();
      server.Close();
    }

    [Test]
    public void ServerGetNameWithoutReadCalled()
    {
      var server = CreateConnection();
      server.Open();
      const string sqlMaster = "create table tb_config (name varchar(20), value INTEGER)";
      using (var command = new SQLiteServerCommand(sqlMaster, server))
      {
        command.ExecuteNonQuery();
      }

      const string sqlSelect = "SELECT * FROM tb_config";
      using (var command = new SQLiteServerCommand(sqlSelect, server))
      {
        using (var reader = command.ExecuteReader())
        {
          Assert.AreEqual( "name", reader.GetName(0));
          Assert.AreEqual("value", reader.GetName(1));
        }
      }
      server.Close();
    }

    [Test]
    public void ServerGetNameForNonExistentColumn()
    {
      var server = CreateConnection();
      server.Open();
      const string sqlMaster = "create table tb_config (name varchar(20), value INTEGER)";
      using (var command = new SQLiteServerCommand(sqlMaster, server))
      {
        command.ExecuteNonQuery();
      }

      const string sqlSelect = "SELECT * FROM tb_config";
      using (var command = new SQLiteServerCommand(sqlSelect, server))
      {
        using (var reader = command.ExecuteReader())
        {
         Assert.Throws<SQLiteServerException>(() =>
         {
           reader.GetName(12);
         });
        }
      }
      server.Close();
    }

    [Test]
    public void ServerGetNameForNonExistentColumnNegative()
    {
      var server = CreateConnection();
      server.Open();
      const string sqlMaster = "create table tb_config (name varchar(20), value INTEGER)";
      using (var command = new SQLiteServerCommand(sqlMaster, server))
      {
        command.ExecuteNonQuery();
      }

      const string sqlSelect = "SELECT * FROM tb_config";
      using (var command = new SQLiteServerCommand(sqlSelect, server))
      {
        using (var reader = command.ExecuteReader())
        {
          Assert.Throws<SQLiteServerException>(() =>
          {
            reader.GetName(-1);
          });
        }
      }
      server.Close();
    }

    [Test]
    public void ClientGetNameForNonExistentColumn()
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

      const string sqlSelect = "SELECT * FROM tb_config";
      using (var command = new SQLiteServerCommand(sqlSelect, client))
      {
        using (var reader = command.ExecuteReader())
        {
          Assert.Throws<SQLiteServerException>(() =>
          {
            reader.GetName(12);
          });
        }
      }
      server.Close();
    }

    [Test]
    public void ClientGetNameForNonExistentColumnNegative()
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

      const string sqlSelect = "SELECT * FROM tb_config";
      using (var command = new SQLiteServerCommand(sqlSelect, client))
      {
        using (var reader = command.ExecuteReader())
        {
          Assert.Throws<SQLiteServerException>(() =>
          {
            reader.GetName(-1);
          });
        }
      }
      server.Close();
    }

    [Test]
    public void ServerGetNameWithReadCalled()
    {
      var server = CreateConnection();
      server.Open();
      const string sqlMaster = "create table tb_config (name varchar(20), value INTEGER)";
      using (var command = new SQLiteServerCommand(sqlMaster, server))
      {
        command.ExecuteNonQuery();
      }

      const string sqlInsert1 = "insert into tb_config(name, value) VALUES ('a', NULL )";
      using (var command = new SQLiteServerCommand(sqlInsert1, server))
      {
        command.ExecuteNonQuery();
      }
      const string sqlInsert2 = "insert into tb_config(name, value) VALUES ('b', '20')";
      using (var command = new SQLiteServerCommand(sqlInsert2, server))
      {
        command.ExecuteNonQuery();
      }

      const string sqlSelect = "SELECT * FROM tb_config";
      using (var command = new SQLiteServerCommand(sqlSelect, server))
      {
        using (var reader = command.ExecuteReader())
        {
          Assert.IsTrue(reader.Read());
          Assert.AreEqual("name", reader.GetName(0));
          Assert.AreEqual("value", reader.GetName(1));


          Assert.IsTrue(reader.Read());
          Assert.AreEqual("name", reader.GetName(0));
          Assert.AreEqual("value", reader.GetName(1));

          Assert.IsFalse(reader.Read());
        }
      }
      server.Close();
    }

    [Test]
    public void ClientGetNameWithoutReadCalled()
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

      const string sqlSelect = "SELECT * FROM tb_config";
      using (var command = new SQLiteServerCommand(sqlSelect, client))
      {
        using (var reader = command.ExecuteReader())
        {
          Assert.AreEqual("name", reader.GetName(0));
          Assert.AreEqual("value", reader.GetName(1));
        }
      }

      client.Close();
      server.Close();
    }

    [Test]
    public void ClientGetNameWithReadCalled()
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

      const string sqlInsert1 = "insert into tb_config(name, value) VALUES ('a', NULL )";
      using (var command = new SQLiteServerCommand(sqlInsert1, client))
      {
        command.ExecuteNonQuery();
      }
      const string sqlInsert2 = "insert into tb_config(name, value) VALUES ('b', '20')";
      using (var command = new SQLiteServerCommand(sqlInsert2, client))
      {
        command.ExecuteNonQuery();
      }

      const string sqlSelect = "SELECT * FROM tb_config";
      using (var command = new SQLiteServerCommand(sqlSelect, client))
      {
        using (var reader = command.ExecuteReader())
        {
          Assert.IsTrue(reader.Read());
          Assert.AreEqual("name", reader.GetName(0));
          Assert.AreEqual("value", reader.GetName(1));


          Assert.IsTrue(reader.Read());
          Assert.AreEqual("name", reader.GetName(0));
          Assert.AreEqual("value", reader.GetName(1));

          Assert.IsFalse(reader.Read());
        }
      }

      client.Close();
      server.Close();
    }
  }
}
