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

using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using NUnit.Framework;
using SQLiteServer.Data.SQLiteServer;

namespace SQLiteServer.Test.SQLiteServer
{
  [TestFixture]
  internal class Common
  {
    protected static readonly IPAddress Address = IPAddress.Parse("127.0.0.1");
    protected const int Port = 1100;
    protected const int Backlog = 500;
    protected const int HeartBeatTimeOut = 500;

    private string _source;
    private readonly List<SQLiteServerConnection> _connections = new List<SQLiteServerConnection>();

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
        foreach (var connection in _connections)
        {
          if (connection.State != ConnectionState.Closed)
          {
            connection.Close();
          }
        }
        File.Delete(_source);
      }
      catch
      {
        // ignored
      }
    }

    protected SQLiteServerConnection CreateConnection()
    {
      var connection  = new SQLiteServerConnection($"Data Source={_source};Version=3;", Address, Port, Backlog, HeartBeatTimeOut);
      _connections.Add(connection);
      return connection;
    }
  }
}
