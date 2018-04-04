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
using SQLiteServer.Data.SQLiteServer;

namespace SQLiteServer.Test.SQLiteServer
{
  [TestFixture]
  internal class TypeTests
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
  }
}
