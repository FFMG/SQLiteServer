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
using NUnit.Framework;
using SQLiteServer.Data.SQLiteServer;

namespace SQLiteServer.Test.SQLiteServer
{
  [TestFixture]
  internal class TypeTests : Common
  {
    [Test]
    public void ServerGetFieldTypes()
    {
      var server = CreateConnection();
      server.Open();
      const string sqlMaster = "create table tb_config (A varchar(255), B INTEGER, C TEXT, D REAL, E BLOB )";
      using (var command = new SQLiteServerCommand(sqlMaster, server))
      {
        command.ExecuteNonQuery();
      }

      const string sqlSelect = "SELECT * FROM tb_config";
      using (var command = new SQLiteServerCommand(sqlSelect, server))
      {
        using (var reader = command.ExecuteReader())
        {
          Assert.AreEqual( typeof(string), reader.GetFieldType(0) ); // varchar
          Assert.AreEqual(typeof(long), reader.GetFieldType(1));     // integer
          Assert.AreEqual(typeof(string), reader.GetFieldType(2));   // text
          Assert.AreEqual(typeof(double), reader.GetFieldType(3));   // real
          Assert.AreEqual(typeof(byte[]), reader.GetFieldType(4));   // blob
        }
      }
      server.Close();
    }

    [Test]
    public void ServerGetFieldTypesMoreThanOnce()
    {
      var server = CreateConnection();
      server.Open();
      const string sqlMaster = "create table tb_config (A varchar(255), B INTEGER, C TEXT, D REAL, E BLOB )";
      using (var command = new SQLiteServerCommand(sqlMaster, server))
      {
        command.ExecuteNonQuery();
      }

      const string sqlSelect = "SELECT * FROM tb_config";
      using (var command = new SQLiteServerCommand(sqlSelect, server))
      {
        using (var reader = command.ExecuteReader())
        {
          Assert.AreEqual(typeof(string), reader.GetFieldType(0));  // varchar
          Assert.AreEqual(typeof(long), reader.GetFieldType(1));    // integer
          Assert.AreEqual(typeof(string), reader.GetFieldType(2));  // text
          Assert.AreEqual(typeof(double), reader.GetFieldType(3));  // real
          Assert.AreEqual(typeof(byte[]), reader.GetFieldType(4));  // blob

          // do it again...
          Assert.AreEqual(typeof(string), reader.GetFieldType(0));  // varchar
          Assert.AreEqual(typeof(long), reader.GetFieldType(1));    // integer
          Assert.AreEqual(typeof(string), reader.GetFieldType(2));  // text
          Assert.AreEqual(typeof(double), reader.GetFieldType(3));  // real
          Assert.AreEqual(typeof(byte[]), reader.GetFieldType(4));  // blob
        }
      }
      server.Close();
    }

    [Test]
    public void ClientGetFieldTypes()
    {
      var server = CreateConnection();
      server.Open();

      var client = CreateConnection();
      client.Open();

      const string sqlMaster = "create table tb_config (A varchar(255), B INTEGER, C TEXT, D REAL, E BLOB )";
      using (var command = new SQLiteServerCommand(sqlMaster, client))
      {
        command.ExecuteNonQuery();
      }

      const string sqlSelect = "SELECT * FROM tb_config";
      using (var command = new SQLiteServerCommand(sqlSelect, client))
      {
        using (var reader = command.ExecuteReader())
        {
          Assert.AreEqual(typeof(string), reader.GetFieldType(0));  // varchar
          Assert.AreEqual(typeof(long), reader.GetFieldType(1));    // integer
          Assert.AreEqual(typeof(string), reader.GetFieldType(2));  // text
          Assert.AreEqual(typeof(double), reader.GetFieldType(3));  // real
          Assert.AreEqual(typeof(byte[]), reader.GetFieldType(4));  // blob
        }
      }

      client.Close();
      server.Close();
    }

    [Test]
    public void ClientGetFieldTypesMoreThanOnce()
    {
      var server = CreateConnection();
      server.Open();

      var client = CreateConnection();
      client.Open();

      const string sqlMaster = "create table tb_config (A varchar(255), B INTEGER, C TEXT, D REAL, E BLOB )";
      using (var command = new SQLiteServerCommand(sqlMaster, client))
      {
        command.ExecuteNonQuery();
      }

      const string sqlSelect = "SELECT * FROM tb_config";
      using (var command = new SQLiteServerCommand(sqlSelect, client))
      {
        using (var reader = command.ExecuteReader())
        {
          Assert.AreEqual(typeof(string), reader.GetFieldType(0));  // varchar
          Assert.AreEqual(typeof(long), reader.GetFieldType(1));    // integer
          Assert.AreEqual(typeof(string), reader.GetFieldType(2));  // text
          Assert.AreEqual(typeof(double), reader.GetFieldType(3));  // real
          Assert.AreEqual(typeof(byte[]), reader.GetFieldType(4));  // blob

          // do it again...
          Assert.AreEqual(typeof(string), reader.GetFieldType(0));  // varchar
          Assert.AreEqual(typeof(long), reader.GetFieldType(1));    // integer
          Assert.AreEqual(typeof(string), reader.GetFieldType(2));  // text
          Assert.AreEqual(typeof(double), reader.GetFieldType(3));  // real
          Assert.AreEqual(typeof(byte[]), reader.GetFieldType(4));  // blob
        }
      }

      client.Close();
      server.Close();
    }

    [Test]
    public void ServerGetAnyTypeType()
    {
      var server = CreateConnection();
      server.Open();
      const string sqlMaster = "CREATE TABLE t1(x SOMETYPE, y INTEGER, z)";
      using (var command = new SQLiteServerCommand(sqlMaster, server))
      {
        command.ExecuteNonQuery();
      }

      const string sqlSelect = "SELECT * FROM t1";
      using (var command = new SQLiteServerCommand(sqlSelect, server))
      {
        using (var reader = command.ExecuteReader())
        {
          Assert.AreEqual(typeof(object), reader.GetFieldType(0)); // SOMETYPE = object
          Assert.AreEqual(typeof(long), reader.GetFieldType(1));   // integer
        }
      }
      server.Close();
    }

    [Test]
    public void ClientGetAnyTypeType()
    {
      var server = CreateConnection();
      server.Open();
      var client = CreateConnection();
      client.Open();

      const string sqlMaster = "CREATE TABLE t1(x SOMETYPE, y INTEGER, z)";
      using (var command = new SQLiteServerCommand(sqlMaster, client))
      {
        command.ExecuteNonQuery();
      }

      const string sqlSelect = "SELECT * FROM t1";
      using (var command = new SQLiteServerCommand(sqlSelect, client))
      {
        using (var reader = command.ExecuteReader())
        {
          Assert.AreEqual(typeof(object), reader.GetFieldType(0)); // SOMETYPE = object
          Assert.AreEqual(typeof(long), reader.GetFieldType(1));   // integer
        }
      }

      client.Close();
      server.Close();
    }

    [Test]
    public void ServerGetBlobTypeType()
    {
      var server = CreateConnection();
      server.Open();
      const string sqlMaster = "CREATE TABLE t1(x BLOB, y INTEGER, z)";
      using (var command = new SQLiteServerCommand(sqlMaster, server))
      {
        command.ExecuteNonQuery();
      }

      const string sqlSelect = "SELECT * FROM t1";
      using (var command = new SQLiteServerCommand(sqlSelect, server))
      {
        using (var reader = command.ExecuteReader())
        {
          Assert.AreEqual(typeof(byte[]), reader.GetFieldType(0)); // BLOB = byte[]
          Assert.AreEqual(typeof(long), reader.GetFieldType(1));   // integer
        }
      }
      server.Close();
    }

    [Test]
    public void ClientGetBlobTypeType()
    {
      var server = CreateConnection();
      server.Open();
      var client = CreateConnection();
      client.Open();

      const string sqlMaster = "CREATE TABLE t1(x BLOB, y INTEGER, z)";
      using (var command = new SQLiteServerCommand(sqlMaster, client))
      {
        command.ExecuteNonQuery();
      }

      const string sqlSelect = "SELECT * FROM t1";
      using (var command = new SQLiteServerCommand(sqlSelect, client))
      {
        using (var reader = command.ExecuteReader())
        {
          Assert.AreEqual(typeof(byte[]), reader.GetFieldType(0)); // BLOB = byte[]
          Assert.AreEqual(typeof(long), reader.GetFieldType(1));   // integer
        }
      }

      client.Close();
      server.Close();
    }

    [Test]
    public void ServerGetFieldTypeWithNoGivenTye()
    {
      var server = CreateConnection();
      server.Open();
      const string sqlMaster = "CREATE TABLE t1(x SOMETYPE, y INTEGER, z)";
      using (var command = new SQLiteServerCommand(sqlMaster, server))
      {
        command.ExecuteNonQuery();
      }

      const string sqlSelect = "SELECT * FROM t1";
      using (var command = new SQLiteServerCommand(sqlSelect, server))
      {
        using (var reader = command.ExecuteReader())
        {
          Assert.AreEqual(typeof(object), reader.GetFieldType(0)); // SOMETYPE = object
          Assert.AreEqual(typeof(long), reader.GetFieldType(1));   // integer
          Assert.AreEqual(typeof(object), reader.GetFieldType(2)); // "" = object
        }
      }
      server.Close();
    }

    [Test]
    public void ServerGetFieldTypeWithNoGivenTyeAfterInsert()
    {
      var server = CreateConnection();
      server.Open();
      const string sqlMaster = "CREATE TABLE t1(x SOMETYPE, y INTEGER, z)";
      using (var command = new SQLiteServerCommand(sqlMaster, server))
      {
        command.ExecuteNonQuery();
      }

      var sql = "INSERT INTO t1 (x, y, z) VALUES(RANDOMBLOB(1), 1, 'z');";
      using (var command = new SQLiteServerCommand(sql, server)) { command.ExecuteNonQuery(); }
      sql = "INSERT INTO t1 (x, y, z) VALUES(RANDOMBLOB(1), 1, 12);";
      using (var command = new SQLiteServerCommand(sql, server)) { command.ExecuteNonQuery(); }

      const string sqlSelect = "SELECT * FROM t1";
      using (var command = new SQLiteServerCommand(sqlSelect, server))
      {
        using (var reader = command.ExecuteReader())
        {
          Assert.IsTrue(reader.Read());
          // we now now the types.
          Assert.AreEqual(typeof(byte[]), reader.GetFieldType(0)); // SOMETYPE = object
          Assert.AreEqual(typeof(long), reader.GetFieldType(1));   // integer
          Assert.AreEqual(typeof(string), reader.GetFieldType(2)); // string

          Assert.IsTrue(reader.Read());
          Assert.AreEqual(typeof(byte[]), reader.GetFieldType(0)); // SOMETYPE = object
          Assert.AreEqual(typeof(long), reader.GetFieldType(1));   // integer
          Assert.AreEqual(typeof(long), reader.GetFieldType(2));   // int
        }
      }
      server.Close();
    }

    [Test]
    public void ClientGetFieldTypeWithNoGivenTyeAfterInsert()
    {
      var server = CreateConnection();
      server.Open();
      var client = CreateConnection();
      client.Open();

      const string sqlMaster = "CREATE TABLE t1(x SOMETYPE, y INTEGER, z)";
      using (var command = new SQLiteServerCommand(sqlMaster, client))
      {
        command.ExecuteNonQuery();
      }

      var sql = "INSERT INTO t1 (x, y, z) VALUES(RANDOMBLOB(1), 1, 'z');";
      using (var command = new SQLiteServerCommand(sql, client)) { command.ExecuteNonQuery(); }
      sql = "INSERT INTO t1 (x, y, z) VALUES(RANDOMBLOB(1), 1, 12);";
      using (var command = new SQLiteServerCommand(sql, client)) { command.ExecuteNonQuery(); }

      const string sqlSelect = "SELECT * FROM t1";
      using (var command = new SQLiteServerCommand(sqlSelect, client))
      {
        using (var reader = command.ExecuteReader())
        {
          Assert.IsTrue(reader.Read());
          // we now now the types.
          Assert.AreEqual(typeof(byte[]), reader.GetFieldType(0)); // SOMETYPE = object
          Assert.AreEqual(typeof(long), reader.GetFieldType(1));   // integer
          Assert.AreEqual(typeof(string), reader.GetFieldType(2)); // string

          Assert.IsTrue(reader.Read());
          Assert.AreEqual(typeof(byte[]), reader.GetFieldType(0)); // SOMETYPE = object
          Assert.AreEqual(typeof(long), reader.GetFieldType(1));   // integer
          Assert.AreEqual(typeof(long), reader.GetFieldType(2));   // int
        }
      }
      server.Close();
    }

    [Test]
    public void ClientGetFieldTypeWithNoGivenType()
    {
      var server = CreateConnection();
      server.Open();
      var client = CreateConnection();
      client.Open();
      const string sqlMaster = "CREATE TABLE t1(x SOMETYPE, y INTEGER, z)";
      using (var command = new SQLiteServerCommand(sqlMaster, client))
      {
        command.ExecuteNonQuery();
      }

      const string sqlSelect = "SELECT * FROM t1";
      using (var command = new SQLiteServerCommand(sqlSelect, client))
      {
        using (var reader = command.ExecuteReader())
        {
          Assert.AreEqual(typeof(object), reader.GetFieldType(0)); // SOMETYPE = object
          Assert.AreEqual(typeof(long), reader.GetFieldType(1));   // integer
          Assert.AreEqual(typeof(object), reader.GetFieldType(2)); // "" = object
        }
      }

      client.Close();
      server.Close();
    }
  }
}
