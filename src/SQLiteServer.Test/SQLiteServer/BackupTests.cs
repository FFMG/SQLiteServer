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
using NUnit.Framework;
using SQLiteServer.Data.Connections;
using SQLiteServer.Data.SQLiteServer;

namespace SQLiteServer.Test.SQLiteServer
{
  [TestFixture]
  internal class BackupTests : Common
  {
    [Test]
    public void SourceIsNotOpen()
    {
      var source = CreateConnection();
      var destination = CreateConnection();
      Assert.Throws<InvalidOperationException>( () => source.BackupDatabase( destination, "main", "main", -1, null, 0 ));
    }

    [Test]
    public void DestinationIsNotOpen()
    {
      var source = CreateConnection();
      source.Open();
      var destination = CreateConnection();
      Assert.Throws<InvalidOperationException>(() => source.BackupDatabase(destination, "main", "main", -1, null, 0));
      source.Close();
    }

    [Test]
    public void DestinationIsNull()
    {
      var source = CreateConnection();
      source.Open();
      const SQLiteServerConnection destination = null;
      Assert.Throws<ArgumentNullException>(() => source.BackupDatabase(destination, "main", "main", -1, null, 0));
      source.Close();
    }

    [Test]
    public void SourceNameIsNull()
    {
      var source = CreateConnection();
      source.Open();
      var destination = CreateConnection();
      destination.Open();
      Assert.Throws<ArgumentNullException>(() => source.BackupDatabase(destination, null, "main", -1, null, 0));
      source.Close();
      destination.Close();
    }

    [Test]
    public void DestinationNameIsNull()
    {
      var source = CreateConnection();
      source.Open();
      var destination = CreateConnection();
      destination.Open();
      Assert.Throws<ArgumentNullException>(() => source.BackupDatabase(destination, "main", null, -1, null, 0));
      source.Close();
      destination.Close();
    }

    [Test]
    public void BackupMasterToMaster()
    {
      var source = CreateConnectionNewSource(new SocketConnectionBuilder(Address, 1202, Backlog, HeartBeatTimeOut), null);
      var destination = CreateConnectionNewSource(new SocketConnectionBuilder(Address, 1203, Backlog, HeartBeatTimeOut), null );
      source.Open();
      destination.Open();

      // add data to source
      const string sqlMaster = "create table tb_config (name varchar(20), value INTEGER)";
      using (var command = new SQLiteServerCommand(sqlMaster, source)) { command.ExecuteNonQuery(); }
      var long1 = RandomNumber<long>();
      var sql = $"insert into tb_config(name, value) VALUES ('a', {long1})";
      using (var command = new SQLiteServerCommand(sql, source)){ command.ExecuteNonQuery(); }

      // backup
      source.BackupDatabase(destination, "main", "main", -1, null, 0);

      // check that the backup now has the data
      sql = "select * FROM tb_config";
      using (var command = new SQLiteServerCommand(sql, destination))
      {
        using (var reader = command.ExecuteReader())
        {
          Assert.IsTrue(reader.Read());
          Assert.AreEqual("a", reader.GetString(0));
          Assert.AreEqual(long1, reader.GetInt64(1));

          Assert.IsFalse(reader.Read());
        }
      }

      source.Close();
      destination.Close();
    }

    [Test]
    public void CallbackMasterToMaster()
    {
      var source = CreateConnectionNewSource(new SocketConnectionBuilder(Address, 1202, Backlog, HeartBeatTimeOut), null);
      var destination = CreateConnectionNewSource(new SocketConnectionBuilder(Address, 1203, Backlog, HeartBeatTimeOut), null);
      source.Open();
      destination.Open();

      // add data to source
      const string sqlMaster = "create table tb_config (name varchar(20), value INTEGER)";
      using (var command = new SQLiteServerCommand(sqlMaster, source)) { command.ExecuteNonQuery(); }

      for (var i = 0; i < 100; ++i)
      {
        var long1 = RandomNumber<long>();
        var sql = $"insert into tb_config(name, value) VALUES ('a', {long1})";
        using (var command = new SQLiteServerCommand(sql, source))
        {
          command.ExecuteNonQuery();
        }
      }

      var called = 0;
      var callback =
      new SQLiteServerBackupCallback(
        (sc, sn, dc, dn, page, remainingPages, totalPages, retry) =>
        {
          Assert.AreEqual(page, 1 );
          Assert.AreEqual(remainingPages, 1);
          Assert.AreEqual(totalPages, 2);
          Assert.AreEqual(retry, false);

          ++called;
          return true;
        });

      // backup
      // page size is 1 - (or 1024)
      // we added 100 long = 4 bytes
      // and we added '100' 'a' = 2 bytes 
      // 400 + 200 = 600 = one page
      source.BackupDatabase(destination, "main", "main", 1, callback, 0);

      // check that this was called exactly once
      // that way we know the other asserts are checked.
      Assert.AreEqual( 1, called );

      source.Close();
      destination.Close();
    }
  }
}