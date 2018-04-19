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
  }
}