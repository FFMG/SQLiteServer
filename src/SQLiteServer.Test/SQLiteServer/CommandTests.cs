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
using SQLiteServer.Data.SQLiteServer;

namespace SQLiteServer.Test.SQLiteServer
{
  [TestFixture]
  internal class CommandTests : Common
  {
    [Test]
    public void DbConnectionIsNull()
    {
      var command = new SQLiteServerCommand();
      Assert.Throws<InvalidOperationException>( () =>
      {
        command.ExecuteNonQuery();
      });
    }

    [Test]
    public void CommandTextIsNull()
    {
      // make sure that the connection is valid.
      var server = CreateConnection();
      server.Open();
      var command = new SQLiteServerCommand(server);
      Assert.Throws<InvalidOperationException>(() =>
      {
        command.ExecuteNonQuery();
      });
      server.Close();
    }

    [Test]
    public void CommandTextIsEmpty()
    {
      // make sure that the connection is valid.
      var server = CreateConnection();
      server.Open();
      var command = new SQLiteServerCommand(server);
      Assert.Throws<InvalidOperationException>(() =>
      {
        command.ExecuteNonQuery();
      });
      server.Close();
    }

    [Test]
    public void CommandTextIsSpaces()
    {
      // make sure that the connection is valid.
      var server = CreateConnection();
      server.Open();
      var command = new SQLiteServerCommand(server);
      Assert.Throws<InvalidOperationException>(() =>
      {
        command.ExecuteNonQuery();
      });
      server.Close();
    }
  }
}
