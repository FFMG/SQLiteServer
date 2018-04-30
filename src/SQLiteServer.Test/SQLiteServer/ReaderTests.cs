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
using NUnit.Framework;
using SQLiteServer.Data.Exceptions;
using SQLiteServer.Data.SQLiteServer;

namespace SQLiteServer.Test.SQLiteServer
{
  [TestFixture]
  internal class ReaderTests : Common
  {
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
      var long1 = RandomNumber<long>();
      var sqlInsert1 = $"insert into tb_config(name, value) VALUES ('a', {long1})";
      using (var command = new SQLiteServerCommand(sqlInsert1, client))
      {
        command.ExecuteNonQuery();
      }

      var long2 = RandomNumber<long>();
      var sqlInsert2 = $"insert into tb_config(name, value) VALUES ('b', {long2})";
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
          Assert.AreEqual(long1, reader.GetInt64(1));

          Assert.IsTrue(reader.Read());
          Assert.AreEqual("b", reader.GetString(0));
          Assert.AreEqual(long2, reader.GetInt64(1));

          Assert.IsFalse(reader.Read());
        }
      }
      client.Close();
      server.Close();
    }

    [Test]
    public void ClientGetFloatValue()
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
      var float1 = RandomNumber<float>();
      var sqlInsert1 = $"insert into tb_config(name, value) VALUES ('a', {float1:G32})";
      using (var command = new SQLiteServerCommand(sqlInsert1, client))
      {
        command.ExecuteNonQuery();
      }

      var float2 = RandomNumber<float>();
      var sqlInsert2 = $"insert into tb_config(name, value) VALUES ('b', {float2:G32})";
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
          Assert.AreEqual(float1, reader.GetFloat(1));

          Assert.IsTrue(reader.Read());
          Assert.AreEqual("b", reader.GetString(0));
          Assert.AreEqual(float2, reader.GetFloat(1));

          Assert.IsFalse(reader.Read());
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

      var double1 = RandomNumber<double>();
      var sqlInsert1 = $"insert into tb_config(name, value) VALUES ('a', {double1:G64})";
      using (var command = new SQLiteServerCommand(sqlInsert1, client))
      {
        command.ExecuteNonQuery();
      }

      var double2 = RandomNumber<double>();
      var sqlInsert2 = $"insert into tb_config(name, value) VALUES ('b', {double2:G64})";
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
          Assert.AreEqual(double1, reader.GetDouble(1));

          Assert.IsTrue(reader.Read());
          Assert.AreEqual("b", reader.GetString(0));
          Assert.AreEqual(double2, reader.GetDouble(1));

          Assert.IsFalse(reader.Read());
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

      var double1 = RandomNumber<double>();
      var sqlInsert1 = $"insert into tb_config(name, value) VALUES ('a', {double1:G64})";
      using (var command = new SQLiteServerCommand(sqlInsert1, server))
      {
        command.ExecuteNonQuery();
      }

      var double2 = RandomNumber<double>();
      var sqlInsert2 = $"insert into tb_config(name, value) VALUES ('b', {double2:G64})";
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
          Assert.AreEqual(double1, reader.GetDouble(1));

          Assert.IsTrue(reader.Read());
          Assert.AreEqual("b", reader.GetString(0));
          Assert.AreEqual(double2, reader.GetDouble(1));
        }
      }
      server.Close();
    }

    [Test]
    public void ServerGetFloatValue()
    {
      var server = CreateConnection();
      server.Open();
      const string sqlMaster = "create table tb_config (name varchar(20), value REAL)";
      using (var command = new SQLiteServerCommand(sqlMaster, server))
      {
        command.ExecuteNonQuery();
      }

      var float1 = RandomNumber<float>();
      var sqlInsert1 = $"insert into tb_config(name, value) VALUES ('a', {float1:G64})";
      using (var command = new SQLiteServerCommand(sqlInsert1, server))
      {
        command.ExecuteNonQuery();
      }

      var float2 = RandomNumber<float>();
      var sqlInsert2 = $"insert into tb_config(name, value) VALUES ('b', {float2:G64})";
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
          Assert.AreEqual(float1, reader.GetFloat(1));

          Assert.IsTrue(reader.Read());
          Assert.AreEqual("b", reader.GetString(0));
          Assert.AreEqual(float2, reader.GetFloat(1));
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
    public void ServerHasRows()
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
          Assert.IsTrue( reader.HasRows );
        }
      }
      server.Close();
    }

    [Test]
    public void ServerHasSomeRows()
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

      const string sqlSelect = "SELECT * FROM tb_config where name='a'";
      using (var command = new SQLiteServerCommand(sqlSelect, server))
      {
        using (var reader = command.ExecuteReader())
        {
          Assert.IsTrue(reader.HasRows);
        }
      }
      server.Close();
    }

    [Test]
    public void ServerHasNoRows()
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

      const string sqlSelect = "SELECT * FROM tb_config where name='c'";
      using (var command = new SQLiteServerCommand(sqlSelect, server))
      {
        using (var reader = command.ExecuteReader())
        {
          Assert.IsFalse(reader.HasRows);
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
      var long1 = RandomNumber<long>();
      var sqlInsert1 = $"insert into tb_config(name, value) VALUES ('a', {long1})";
      using (var command = new SQLiteServerCommand(sqlInsert1, client))
      {
        command.ExecuteNonQuery();
      }
      var long2 = RandomNumber<long>();
      var sqlInsert2 = $"insert into tb_config(name, value) VALUES ('b', {long2})";
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
          Assert.AreEqual(long1, reader["value"]);

          Assert.IsTrue(reader.Read());
          Assert.AreEqual("b", reader["name"]);
          Assert.AreEqual(long2, reader["value"]);

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

      var long1 = RandomNumber<long>();
      var sqlInsert1 = $"insert into tb_config(name, value) VALUES ('a', {long1})";
      using (var command = new SQLiteServerCommand(sqlInsert1, server))
      {
        command.ExecuteNonQuery();
      }
      var long2 = RandomNumber<long>();
      var sqlInsert2 = $"insert into tb_config(name, value) VALUES ('b', {long2})";
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
          Assert.AreEqual(long1, reader.GetInt64(1));

          Assert.IsTrue(reader.Read());
          Assert.AreEqual("b", reader.GetString(0));
          Assert.AreEqual(long2, reader.GetInt64(1));

          Assert.IsFalse(reader.Read());
        }
      }
      server.Close();
    }

    [Test]
    public void ServerGetIntValue()
    {
      var server = CreateConnection();
      server.Open();
      const string sqlMaster = "create table tb_config (name varchar(20), value INTEGER)";
      using (var command = new SQLiteServerCommand(sqlMaster, server))
      {
        command.ExecuteNonQuery();
      }

      var int1 = RandomNumber<int>();
      var sqlInsert1 = $"insert into tb_config(name, value) VALUES ('a', {int1})";
      using (var command = new SQLiteServerCommand(sqlInsert1, server))
      {
        command.ExecuteNonQuery();
      }
      var int2 = RandomNumber<int>();
      var sqlInsert2 = $"insert into tb_config(name, value) VALUES ('b', {int2})";
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
          Assert.AreEqual(int1, reader.GetInt32(1));

          Assert.IsTrue(reader.Read());
          Assert.AreEqual("b", reader.GetString(0));
          Assert.AreEqual(int2, reader.GetInt32(1));

          Assert.IsFalse(reader.Read());
        }
      }
      server.Close();
    }

    [Test]
    public void ClientGetIntValue()
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

      var int1 = RandomNumber<int>();
      var sqlInsert1 = $"insert into tb_config(name, value) VALUES ('a', {int1})";
      using (var command = new SQLiteServerCommand(sqlInsert1, client))
      {
        command.ExecuteNonQuery();
      }
      var int2 = RandomNumber<int>();
      var sqlInsert2 = $"insert into tb_config(name, value) VALUES ('b', {int2})";
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
          Assert.AreEqual(int1, reader.GetInt32(1));

          Assert.IsTrue(reader.Read());
          Assert.AreEqual("b", reader.GetString(0));
          Assert.AreEqual(int2, reader.GetInt32(1));

          Assert.IsFalse(reader.Read());
        }
      }
      client.Close();
      server.Close();
    }

    [Test]
    public void ClientGetBoolValue()
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

      var int1 = RandomNumber<int>();
      var sqlInsert1 = $"insert into tb_config(name, value) VALUES ('a', {int1})";
      using (var command = new SQLiteServerCommand(sqlInsert1, client))
      {
        command.ExecuteNonQuery();
      }

      var sqlInsert2 = "insert into tb_config(name, value) VALUES ('b', 1)";
      using (var command = new SQLiteServerCommand(sqlInsert2, client)){ command.ExecuteNonQuery(); }

      sqlInsert2 = "insert into tb_config(name, value) VALUES ('c', 0)";
      using (var command = new SQLiteServerCommand(sqlInsert2, client)){command.ExecuteNonQuery();}

      const string sqlSelect = "SELECT * FROM tb_config";
      using (var command = new SQLiteServerCommand(sqlSelect, client))
      {
        using (var reader = command.ExecuteReader())
        {
          Assert.IsTrue(reader.Read());
          Assert.AreEqual("a", reader.GetString(0));
          if (int1 == 0)
          {
            Assert.IsFalse(reader.GetBoolean(1));
          }
          else
          {
            Assert.IsTrue(reader.GetBoolean(1));
          }
          Assert.IsTrue(reader.Read());
          Assert.IsTrue(reader.GetBoolean(1));

          Assert.IsTrue(reader.Read());
          Assert.IsFalse(reader.GetBoolean(1));

          Assert.IsFalse(reader.Read());
        }
      }
      client.Close();
      server.Close();
    }

    [Test]
    public void ServerGetBoolValue()
    {
      var server = CreateConnection();
      server.Open();

      const string sqlMaster = "create table tb_config (name varchar(20), value INTEGER)";
      using (var command = new SQLiteServerCommand(sqlMaster, server))
      {
        command.ExecuteNonQuery();
      }

      var int1 = RandomNumber<int>();
      var sqlInsert1 = $"insert into tb_config(name, value) VALUES ('a', {int1})";
      using (var command = new SQLiteServerCommand(sqlInsert1, server))
      {
        command.ExecuteNonQuery();
      }

      var sqlInsert2 = "insert into tb_config(name, value) VALUES ('b', 1)";
      using (var command = new SQLiteServerCommand(sqlInsert2, server))
      {
        command.ExecuteNonQuery();
      }
      sqlInsert2 = "insert into tb_config(name, value) VALUES ('b', 0)";
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
          if (int1 == 0)
          {
            Assert.IsFalse(reader.GetBoolean(1));
          }
          else
          {
            Assert.IsTrue(reader.GetBoolean(1));
          }
          Assert.IsTrue(reader.Read());
          Assert.IsTrue(reader.GetBoolean(1));

          Assert.IsTrue(reader.Read());
          Assert.IsFalse(reader.GetBoolean(1));

          Assert.IsFalse(reader.Read());
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
              // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
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
    public void ServerGetOrdinalNonExistentValues()
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
          Assert.AreEqual( -1,  reader.GetOrdinal("blah") );
        }
      }
      server.Close();
    }

    [Test]
    public void ClientGetOrdinalNonExistentValues()
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
          Assert.AreEqual(-1, reader.GetOrdinal("blah"));
        }
      }

      client.Close();
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
              // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
              reader.GetString(0);
            });
        }
      }

      client.Close();
      server.Close();
    }

    [Test]
    // ReSharper disable once InconsistentNaming
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
    // ReSharper disable once InconsistentNaming
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
           // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
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
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
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
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            reader.GetName(12);
          });
        }
      }
      client.Close();
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
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            reader.GetName(-1);
          });
        }
      }

      client.Close();
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

    [Test]
    public void ClientGetTableNameWithReadCalled()
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
          Assert.AreEqual("tb_config", reader.GetTableName(0));
          Assert.AreEqual("tb_config", reader.GetTableName(1));

          Assert.IsTrue(reader.Read());
          Assert.AreEqual("name", reader.GetName(0));
          Assert.AreEqual("value", reader.GetName(1));
          Assert.AreEqual("tb_config", reader.GetTableName(0));
          Assert.AreEqual("tb_config", reader.GetTableName(1));

          Assert.IsFalse(reader.Read());
        }
      }

      client.Close();
      server.Close();
    }

    [Test]
    public void ClientGetTableNameWithReadNotCalled()
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
          Assert.AreEqual("tb_config", reader.GetTableName(0));
          Assert.AreEqual("tb_config", reader.GetTableName(1));
        }
      }

      client.Close();
      server.Close();
    }

    [Test]
    public void ServerGetTableNameMultipleTables()
    {
      var server = CreateConnection();
      server.Open();

      var sql = "create table tb_A (name varchar(20), value INTEGER)";
      using (var command = new SQLiteServerCommand(sql, server)) { command.ExecuteNonQuery(); }

      sql = "create table tb_B (name varchar(20), value INTEGER)";
      using (var command = new SQLiteServerCommand(sql, server)) { command.ExecuteNonQuery(); }

      const string sqlSelect = "SELECT * FROM tb_A, tb_B where tb_A.name = tb_B.name;";
      using (var command = new SQLiteServerCommand(sqlSelect, server))
      {
        using (var reader = command.ExecuteReader())
        {
          Assert.AreEqual("tb_A", reader.GetTableName(0));  // A.Name
          Assert.AreEqual("tb_A", reader.GetTableName(1));  // A.Value

          Assert.AreEqual("tb_B", reader.GetTableName(2));  // B.Name
          Assert.AreEqual("tb_B", reader.GetTableName(3));  // B.Value
        }
      }
      server.Close();
    }

    [Test]
    public void ServerGetTableNameMultipleTablesWithAlias()
    {
      var server = CreateConnection();
      server.Open();

      var sql = "create table tb_A (name varchar(20), value INTEGER)";
      using (var command = new SQLiteServerCommand(sql, server)) { command.ExecuteNonQuery(); }

      sql = "create table tb_B (name varchar(20), value INTEGER)";
      using (var command = new SQLiteServerCommand(sql, server)) { command.ExecuteNonQuery(); }

      const string sqlSelect = "SELECT * FROM tb_A A, tb_B B where A.name = B.name;";
      using (var command = new SQLiteServerCommand(sqlSelect, server))
      {
        using (var reader = command.ExecuteReader())
        {
          Assert.AreEqual("tb_A", reader.GetTableName(0));  // A.Name
          Assert.AreEqual("tb_A", reader.GetTableName(1));  // A.Value

          Assert.AreEqual("tb_B", reader.GetTableName(2));  // B.Name
          Assert.AreEqual("tb_B", reader.GetTableName(3));  // B.Value
        }
      }
      server.Close();
    }

    [Test]
    public void ClientGetTableNameMultipleTables()
    {
      var server = CreateConnection();
      server.Open();

      var client = CreateConnection();
      client.Open();

      var sql = "create table tb_A (name varchar(20), value INTEGER)";
      using (var command = new SQLiteServerCommand(sql, client)){command.ExecuteNonQuery();}

      sql = "create table tb_B (name varchar(20), value INTEGER)";
      using (var command = new SQLiteServerCommand(sql, client)) { command.ExecuteNonQuery(); }

      const string sqlSelect = "SELECT * FROM tb_A, tb_B where tb_A.name = tb_B.name;";
      using (var command = new SQLiteServerCommand(sqlSelect, client))
      {
        using (var reader = command.ExecuteReader())
        {
          Assert.AreEqual("tb_A", reader.GetTableName(0));  // A.Name
          Assert.AreEqual("tb_A", reader.GetTableName(1));  // A.Value

          Assert.AreEqual("tb_B", reader.GetTableName(2));  // B.Name
          Assert.AreEqual("tb_B", reader.GetTableName(3));  // B.Value
        }
      }
      client.Close();
      server.Close();
    }

    [Test]
    public void ClientGetTableNameMultipleTablesAndAlias()
    {
      var server = CreateConnection();
      server.Open();

      var client = CreateConnection();
      client.Open();

      var sql = "create table tb_A (name varchar(20), value INTEGER)";
      using (var command = new SQLiteServerCommand(sql, client)) { command.ExecuteNonQuery(); }

      sql = "create table tb_B (name varchar(20), value INTEGER)";
      using (var command = new SQLiteServerCommand(sql, client)) { command.ExecuteNonQuery(); }

      const string sqlSelect = "SELECT A.Name, B.Name FROM tb_A A, tb_B B where A.name = B.name;";
      using (var command = new SQLiteServerCommand(sqlSelect, client))
      {
        using (var reader = command.ExecuteReader())
        {
          Assert.AreEqual("tb_A", reader.GetTableName(0));  // A.Name
          Assert.AreEqual("tb_B", reader.GetTableName(1));  // B.Name
        }
      }
      client.Close();
      server.Close();
    }

    [Test]
    public void ServerGetTableNameWithReadCalled()
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
          Assert.AreEqual("tb_config", reader.GetTableName(0));
          Assert.AreEqual("tb_config", reader.GetTableName(1));

          Assert.IsTrue(reader.Read());
          Assert.AreEqual("name", reader.GetName(0));
          Assert.AreEqual("value", reader.GetName(1));
          Assert.AreEqual("tb_config", reader.GetTableName(0));
          Assert.AreEqual("tb_config", reader.GetTableName(1));

          Assert.IsFalse(reader.Read());
        }
      }
      server.Close();
    }

    [Test]
    public void ServerGetTableNameWithReadNotCalled()
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
          Assert.AreEqual("name", reader.GetName(0));
          Assert.AreEqual("tb_config", reader.GetTableName(0));
          Assert.AreEqual("tb_config", reader.GetTableName(1));
        }
      }
      server.Close();
    }

    [Test]
    public void ServerGetTableNameInvalidColumnNumber()
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
          Assert.AreEqual("name", reader.GetName(0));
          Assert.AreEqual("tb_config", reader.GetTableName(0));
          Assert.AreEqual(string.Empty, reader.GetTableName(12));
        }
      }
      server.Close();
    }

    [Test]
    public void ClientGetTableNameInvalidColumnNumber()
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
          Assert.AreEqual("name", reader.GetName(0));
          Assert.AreEqual("tb_config", reader.GetTableName(0));
          Assert.AreEqual(string.Empty, reader.GetTableName(12));
        }
      }

      client.Close();
      server.Close();
    }

    [Test]
    public void ClientHasRows()
    {
      var server = CreateConnection();
      server.Open();

      var client = CreateConnection();
      client.Open();

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
      using (var command = new SQLiteServerCommand(sqlSelect, client))
      {
        using (var reader = command.ExecuteReader())
        {
          Assert.IsTrue(reader.HasRows);
        }
      }

      client.Close();
      server.Close();
    }

    [Test]
    public void ClientHasSomeRows()
    {
      var server = CreateConnection();
      server.Open();

      var client = CreateConnection();
      client.Open();

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

      const string sqlSelect = "SELECT * FROM tb_config where name='a'";
      using (var command = new SQLiteServerCommand(sqlSelect, client))
      {
        using (var reader = command.ExecuteReader())
        {
          Assert.IsTrue(reader.HasRows);
        }
      }

      client.Close();
      server.Close();
    }

    [Test]
    public void ClientHasNoRows()
    {
      var server = CreateConnection();
      server.Open();

      var client = CreateConnection();
      client.Open();

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

      const string sqlSelect = "SELECT * FROM tb_config where name='c'";
      using (var command = new SQLiteServerCommand(sqlSelect, client))
      {
        using (var reader = command.ExecuteReader())
        {
          Assert.IsFalse(reader.HasRows);
        }
      }

      client.Close();
      server.Close();
    }

    [Test]
    public void ClientBecomesServerWhenTheMasterCloses()
    {
      // open a server and client.
      var server = CreateConnection(); 
      server.Open();
      var client = CreateConnection();
      client.Open();

      const string sqlMaster = "create table tb_config (name varchar(20), value INTEGER)";
      using (var command = new SQLiteServerCommand(sqlMaster, server)){ command.ExecuteNonQuery(); }

      const string sqlInsert = "insert into tb_config(name, value) VALUES ('a', '10')";
      using (var command = new SQLiteServerCommand(sqlInsert, client)){ command.ExecuteNonQuery(); }

      // do a select from client to server
      const string sqlSelect = "SELECT * FROM tb_config";
      using (var command = new SQLiteServerCommand(sqlSelect, client))
      {
        using (var reader = command.ExecuteReader())
        {
          while (reader.Read())
          {
            Assert.AreEqual("a", reader.GetString(0));
            Assert.AreEqual(10, reader.GetInt64(1));
          }
        }
      }
      server.Close();

      // and run the request again.
      using (var command = new SQLiteServerCommand(sqlSelect, client))
      {
        using (var reader = command.ExecuteReader())
        {
          while (reader.Read())
          {
            Assert.AreEqual("a", reader.GetString(0));
            Assert.AreEqual(10, reader.GetInt64(1));
          }
        }
      }
      client.Close();
    }

    [Test]
    public void ServerCommandBehaviorSingleRowOnly()
    {
      var server = CreateConnection();
      server.Open();
      const string sqlMaster = "create table tb_config (name varchar(20), value REAL)";
      using (var command = new SQLiteServerCommand(sqlMaster, server))
      {
        command.ExecuteNonQuery();
      }

      // add multiple rows
      const string sql = "insert into tb_config(name, value) VALUES ('a', 3.14)";
      using (var command = new SQLiteServerCommand(sql, server)) { command.ExecuteNonQuery(); }
      using (var command = new SQLiteServerCommand(sql, server)) { command.ExecuteNonQuery(); }
      using (var command = new SQLiteServerCommand(sql, server)) { command.ExecuteNonQuery(); }

      const string sqlSelect = "SELECT * FROM tb_config";
      using (var command = new SQLiteServerCommand(sqlSelect, server))
      {
        using (var reader = command.ExecuteReader(CommandBehavior.Default | CommandBehavior.SingleRow))
        {
          // first read is valid
          Assert.IsTrue(reader.Read());
          Assert.AreEqual("a", reader.GetString(0));

          // then no more rows can be read.
          Assert.IsFalse(reader.Read());
        }
      }
      server.Close();
    }

    [Test]
    public void ClientCommandBehaviorSingleRowOnly()
    {
      var server = CreateConnection();
      server.Open();
      var client = CreateConnection();
      client.Open();

      const string sqlMaster = "create table tb_config (name varchar(20), value REAL)";
      using (var command = new SQLiteServerCommand(sqlMaster, client))
      {
        command.ExecuteNonQuery();
      }

      // add multiple rows
      const string sql = "insert into tb_config(name, value) VALUES ('a', 3.14)";
      using (var command = new SQLiteServerCommand(sql, client)){ command.ExecuteNonQuery(); }
      using (var command = new SQLiteServerCommand(sql, client)) { command.ExecuteNonQuery(); }
      using (var command = new SQLiteServerCommand(sql, client)) { command.ExecuteNonQuery(); }

      const string sqlSelect = "SELECT * FROM tb_config";
      using (var command = new SQLiteServerCommand(sqlSelect, client))
      {
        using (var reader = command.ExecuteReader( CommandBehavior.Default | CommandBehavior.SingleRow ))
        {
          // first read is valid
          Assert.IsTrue(reader.Read());
          Assert.AreEqual("a", reader.GetString(0));

          // then no more rows can be read.
          Assert.IsFalse(reader.Read());
        }
      }
      client.Close();
      server.Close();
    }

    [Test]
    public void ServerGetNextResult()
    {
      var server = CreateConnection();
      server.Open();
      var sql = "create table t1 (name varchar(20), value INTEGER)";
      using (var command = new SQLiteServerCommand(sql, server)){command.ExecuteNonQuery();}

      sql = "create table t2 (name varchar(20), value REAL)";
      using (var command = new SQLiteServerCommand(sql, server)) { command.ExecuteNonQuery(); }

      sql = "insert into t1(name, value) VALUES ('t1', 10)";
      using (var command = new SQLiteServerCommand(sql, server)) { command.ExecuteNonQuery(); }

      sql = "insert into t2(name, value) VALUES ('t2', 3.14)";
      using (var command = new SQLiteServerCommand(sql, server)) { command.ExecuteNonQuery(); }

      const string sqlSelect = "SELECT * FROM t1; select * from t2;";
      using (var command = new SQLiteServerCommand(sqlSelect, server))
      {
        using (var reader = command.ExecuteReader())
        {
          // t1
          Assert.IsTrue(reader.Read());
          Assert.AreEqual("t1", reader.GetString(0));
          Assert.IsFalse(reader.Read());

          Assert.IsTrue(reader.NextResult());

          // t2
          Assert.IsTrue(reader.Read());
          Assert.AreEqual("t2", reader.GetString(0));
          Assert.IsFalse(reader.Read());

          Assert.IsFalse(reader.NextResult());
        }
      }
      server.Close();
    }

    [Test]
    public void ClientGetNextResult()
    {
      var server = CreateConnection();
      server.Open();
      var client = CreateConnection();
      client.Open();

      var sql = "create table t1 (name varchar(20), value INTEGER)";
      using (var command = new SQLiteServerCommand(sql, client)) { command.ExecuteNonQuery(); }

      sql = "create table t2 (name varchar(20), value REAL)";
      using (var command = new SQLiteServerCommand(sql, client)) { command.ExecuteNonQuery(); }

      sql = "insert into t1(name, value) VALUES ('t1', 10)";
      using (var command = new SQLiteServerCommand(sql, client)) { command.ExecuteNonQuery(); }

      sql = "insert into t2(name, value) VALUES ('t2', 3.14)";
      using (var command = new SQLiteServerCommand(sql, client)) { command.ExecuteNonQuery(); }

      const string sqlSelect = "SELECT * FROM t1; select * from t2;";
      using (var command = new SQLiteServerCommand(sqlSelect, client))
      {
        using (var reader = command.ExecuteReader())
        {
          // t1
          Assert.IsTrue(reader.Read());
          Assert.AreEqual("t1", reader.GetString(0));
          Assert.IsFalse(reader.Read());

          Assert.IsTrue(reader.NextResult());

          // t2
          Assert.IsTrue(reader.Read());
          Assert.AreEqual("t2", reader.GetString(0));
          Assert.IsFalse(reader.Read());

          Assert.IsFalse(reader.NextResult());
        }
      }
      client.Close();
      server.Close();
    }

    [Test]
    public void ServerGetGuidValidString()
    {
      var server = CreateConnection();
      server.Open();
      const string sqlMaster = "create table tb_a (guid varchar(255))";
      using (var command = new SQLiteServerCommand(sqlMaster, server))
      {
        command.ExecuteNonQuery();
      }

      var guid1 = Guid.NewGuid();
      var sql = $"insert into tb_a(guid) VALUES ('{guid1.ToString()}')";
      using (var command = new SQLiteServerCommand(sql, server)){ command.ExecuteNonQuery();}

      var guid2 = Guid.NewGuid();
      sql = $"insert into tb_a(guid) VALUES ('{guid2.ToString()}')";
      using (var command = new SQLiteServerCommand(sql, server)) { command.ExecuteNonQuery(); }

      // select it.
      const string sqlSelect = "SELECT guid FROM tb_a;";
      using (var command = new SQLiteServerCommand(sqlSelect, server))
      {
        using (var reader = command.ExecuteReader())
        {
          //
          Assert.IsTrue(reader.Read());
          var g1 = reader.GetGuid(0);
          Assert.AreEqual( guid1, g1 );

          Assert.IsTrue(reader.Read());
          var g2 = reader.GetGuid(0);
          Assert.AreEqual(guid2, g2);

          Assert.IsFalse(reader.Read());
        }
      }

      server.Close();
    }

    [Test]
    public void ClientGetGuidValidString()
    {
      var server = CreateConnection();
      server.Open();

      var client = CreateConnection();
      client.Open();

      const string sqlMaster = "create table tb_a (guid varchar(255))";
      using (var command = new SQLiteServerCommand(sqlMaster, client))
      {
        command.ExecuteNonQuery();
      }

      var guid1 = Guid.NewGuid();
      var sql = $"insert into tb_a(guid) VALUES ('{guid1.ToString()}')";
      using (var command = new SQLiteServerCommand(sql, client)) { command.ExecuteNonQuery(); }

      var guid2 = Guid.NewGuid();
      sql = $"insert into tb_a(guid) VALUES ('{guid2.ToString()}')";
      using (var command = new SQLiteServerCommand(sql, client)) { command.ExecuteNonQuery(); }

      // select it.
      const string sqlSelect = "SELECT guid FROM tb_a;";
      using (var command = new SQLiteServerCommand(sqlSelect, client))
      {
        using (var reader = command.ExecuteReader())
        {
          //
          Assert.IsTrue(reader.Read());
          var g1 = reader.GetGuid(0);
          Assert.AreEqual(guid1, g1);

          Assert.IsTrue(reader.Read());
          var g2 = reader.GetGuid(0);
          Assert.AreEqual(guid2, g2);

          Assert.IsFalse(reader.Read());
        }
      }
      server.Close();
      client.Close();
    }

    [Test]
    public void ServerGetGuidNullValue()
    {
      var server = CreateConnection();
      server.Open();
      const string sqlMaster = "create table tb_a (guid varchar(255))";
      using (var command = new SQLiteServerCommand(sqlMaster, server))
      {
        command.ExecuteNonQuery();
      }

      const string sql = "insert into tb_a(guid) VALUES (null)";
      using (var command = new SQLiteServerCommand(sql, server)) { command.ExecuteNonQuery(); }

      // select it.
      const string sqlSelect = "SELECT guid FROM tb_a;";
      using (var command = new SQLiteServerCommand(sqlSelect, server))
      {
        using (var reader = command.ExecuteReader())
        {
          //
          Assert.IsTrue(reader.Read());
          Assert.Throws<SQLiteServerException>( () =>
          {
            // ReSharper disable once UnusedVariable
            var guid = reader.GetGuid(0);
          });

          Assert.IsFalse(reader.Read());
        }
      }

      server.Close();
    }

    [Test]
    public void ClientGetGuidNullValue()
    {
      var server = CreateConnection();
      server.Open();
      var client = CreateConnection();
      client.Open();

      const string sqlMaster = "create table tb_a (guid varchar(255))";
      using (var command = new SQLiteServerCommand(sqlMaster, client))
      {
        command.ExecuteNonQuery();
      }

      const string sql = "insert into tb_a(guid) VALUES (null)";
      using (var command = new SQLiteServerCommand(sql, client)) { command.ExecuteNonQuery(); }

      // select it.
      const string sqlSelect = "SELECT guid FROM tb_a;";
      using (var command = new SQLiteServerCommand(sqlSelect, client))
      {
        using (var reader = command.ExecuteReader())
        {
          //
          Assert.IsTrue(reader.Read());
          Assert.Throws<SQLiteServerException>(() =>
          {
            // ReSharper disable once UnusedVariable
            var guid = reader.GetGuid(0);
          });

          Assert.IsFalse(reader.Read());
        }
      }

      client.Close();
      server.Close();
    }

    [Test]
    public void ServerGetGuidStringIsNotAGuid()
    {
      var server = CreateConnection();
      server.Open();
      const string sqlMaster = "create table tb_a (guid varchar(255))";
      using (var command = new SQLiteServerCommand(sqlMaster, server))
      {
        command.ExecuteNonQuery();
      }

      const string sql = "insert into tb_a(guid) VALUES ('blah')";
      using (var command = new SQLiteServerCommand(sql, server)) { command.ExecuteNonQuery(); }

      // select it.
      const string sqlSelect = "SELECT guid FROM tb_a;";
      using (var command = new SQLiteServerCommand(sqlSelect, server))
      {
        using (var reader = command.ExecuteReader())
        {
          //
          Assert.IsTrue(reader.Read());
          Assert.Throws<InvalidCastException>(() =>
          {
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            reader.GetGuid(0);
          });

          Assert.IsFalse(reader.Read());
        }
      }

      server.Close();
    }

    [Test]
    public void ClientGetGuidStringIsNotAGuid()
    {
      var server = CreateConnection();
      server.Open();
      var client = CreateConnection();
      client.Open();

      const string sqlMaster = "create table tb_a (guid varchar(255))";
      using (var command = new SQLiteServerCommand(sqlMaster, client))
      {
        command.ExecuteNonQuery();
      }

      const string sql = "insert into tb_a(guid) VALUES ('blah')";
      using (var command = new SQLiteServerCommand(sql, client)) { command.ExecuteNonQuery(); }

      // select it.
      const string sqlSelect = "SELECT guid FROM tb_a;";
      using (var command = new SQLiteServerCommand(sqlSelect, client))
      {
        using (var reader = command.ExecuteReader())
        {
          //
          Assert.IsTrue(reader.Read());
          Assert.Throws<InvalidCastException>(() =>
          {
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            reader.GetGuid(0);
          });

          Assert.IsFalse(reader.Read());
        }
      }

      client.Close();
      server.Close();
    }

    [Test]
    public void ServerGetCharJustOneCharLen()
    {
      var server = CreateConnection();
      server.Open();
      const string sqlMaster = "create table t1 (char varchar(255))";
      using (var command = new SQLiteServerCommand(sqlMaster, server))
      {
        command.ExecuteNonQuery();
      }

      var cs = new char[char.MaxValue, char.MinValue, 'a', '1', 'z', 'Z', 128, RandomNumber<char>(), RandomNumber<char>(), RandomNumber<char>()];
      foreach (var c in cs)
      {
        var sql = $"insert into t1(char) VALUES ('{c.ToString()}')";
        using (var command = new SQLiteServerCommand(sql, server)) { command.ExecuteNonQuery(); }
      }

      // select it.
      const string sqlSelect = "SELECT char FROM t1;";
      using (var command = new SQLiteServerCommand(sqlSelect, server))
      {
        using (var reader = command.ExecuteReader())
        {
          //
          foreach (var c in cs)
          {
            Assert.IsTrue(reader.Read());
            var c1 = reader.GetChar(0);
            Assert.AreEqual(c, c1);
          }
          Assert.IsFalse(reader.Read());
        }
      }
      server.Close();
    }

    [Test]
    public void ServerGetCharMoreThanOneCharLen()
    {
      var server = CreateConnection();
      server.Open();
      const string sqlMaster = "create table t1 (string varchar(255))";
      using (var command = new SQLiteServerCommand(sqlMaster, server))
      {
        command.ExecuteNonQuery();
      }

      var cs = new List<string> { RandomString(5), RandomString(10), RandomString(100)};
      foreach (var c in cs)
      {
        var sql = $"insert into t1(string) VALUES ('{c}')";
        using (var command = new SQLiteServerCommand(sql, server)) { command.ExecuteNonQuery(); }
      }

      // select it.
      const string sqlSelect = "SELECT string FROM t1;";
      using (var command = new SQLiteServerCommand(sqlSelect, server))
      {
        using (var reader = command.ExecuteReader())
        {
          //
          foreach (var unused in cs)
          {
            Assert.IsTrue(reader.Read());
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            Assert.Throws< SQLiteServerException>( () => reader.GetChar(0));
          }
          Assert.IsFalse(reader.Read());
        }
      }
      server.Close();
    }

    [Test]
    public void ClientGetCharJustOneCharLen()
    {
      var server = CreateConnection();
      server.Open();

      var client = CreateConnection();
      client.Open();

      const string sqlMaster = "create table t1 (char varchar(255))";
      using (var command = new SQLiteServerCommand(sqlMaster, client))
      {
        command.ExecuteNonQuery();
      }

      var cs = new char[char.MaxValue, char.MinValue, 'a', '1', 'z', 'Z', 128, RandomNumber<char>(), RandomNumber<char>(), RandomNumber<char>()];
      foreach (var c in cs)
      {
        var sql = $"insert into t1(char) VALUES ('{c.ToString()}')";
        using (var command = new SQLiteServerCommand(sql, client)) { command.ExecuteNonQuery(); }
      }

      // select it.
      const string sqlSelect = "SELECT char FROM t1;";
      using (var command = new SQLiteServerCommand(sqlSelect, client))
      {
        using (var reader = command.ExecuteReader())
        {
          //
          foreach (var c in cs)
          {
            Assert.IsTrue(reader.Read());
            var c1 = reader.GetChar(0);
            Assert.AreEqual(c, c1);
          }
          Assert.IsFalse(reader.Read());
        }
      }

      client.Close();
      server.Close();
    }

    [Test]
    public void ClientGetCharMoreThanOneCharLen()
    {
      var server = CreateConnection();
      server.Open();

      var client = CreateConnection();
      client.Open();

      const string sqlMaster = "create table t1 (string varchar(255))";
      using (var command = new SQLiteServerCommand(sqlMaster, client))
      {
        command.ExecuteNonQuery();
      }

      var cs = new List<string> { RandomString(5), RandomString(10), RandomString(100) };
      foreach (var c in cs)
      {
        var sql = $"insert into t1(string) VALUES ('{c}')";
        using (var command = new SQLiteServerCommand(sql, client)) { command.ExecuteNonQuery(); }
      }

      // select it.
      const string sqlSelect = "SELECT string FROM t1;";
      using (var command = new SQLiteServerCommand(sqlSelect, client))
      {
        using (var reader = command.ExecuteReader())
        {
          //
          foreach (var unused in cs)
          {
            Assert.IsTrue(reader.Read());
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            Assert.Throws<SQLiteServerException>(() => reader.GetChar(0));
          }
          Assert.IsFalse(reader.Read());
        }
      }
      client.Close();
      server.Close();
    }

    [Test]
    public void ServerGetCharNull()
    {
      var server = CreateConnection();
      server.Open();
      const string sqlMaster = "create table t1 (string varchar(255))";
      using (var command = new SQLiteServerCommand(sqlMaster, server))
      {
        command.ExecuteNonQuery();
      }

      const string sql = "insert into t1(string) VALUES (null)";
      using (var command = new SQLiteServerCommand(sql, server)) { command.ExecuteNonQuery(); }

      // select it.
      const string sqlSelect = "SELECT string FROM t1;";
      using (var command = new SQLiteServerCommand(sqlSelect, server))
      {
        using (var reader = command.ExecuteReader())
        {
          //
          Assert.IsTrue(reader.Read());
          // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
          Assert.Throws<SQLiteServerException>(() => reader.GetChar(0));
          Assert.IsFalse(reader.Read());
        }
      }
      server.Close();
    }

    [Test]
    public void ClientGetCharNull()
    {
      var server = CreateConnection();
      server.Open();

      var client = CreateConnection();
      client.Open();

      const string sqlMaster = "create table t1 (string varchar(255))";
      using (var command = new SQLiteServerCommand(sqlMaster, client))
      {
        command.ExecuteNonQuery();
      }

      const string sql = "insert into t1(string) VALUES (null)";
      using (var command = new SQLiteServerCommand(sql, client)) { command.ExecuteNonQuery(); }

      // select it.
      const string sqlSelect = "SELECT string FROM t1;";
      using (var command = new SQLiteServerCommand(sqlSelect, client))
      {
        using (var reader = command.ExecuteReader())
        {
          //
          Assert.IsTrue(reader.Read());
          // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
          Assert.Throws<SQLiteServerException>(() => reader.GetChar(0));
          Assert.IsFalse(reader.Read());
        }
      }

      client.Close();
      server.Close();
    }
  }
}
