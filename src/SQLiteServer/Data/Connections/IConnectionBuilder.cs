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
using System.Threading.Tasks;
using SQLiteServer.Data.Workers;

namespace SQLiteServer.Data.Connections
{
  internal interface IConnectionBuilder : IDisposable
  {
    /// <summary>
    /// Start the connection
    /// </summary>
    /// <returns></returns>
    Task<bool> ConnectAsync();

    /// <summary>
    /// Open the database, (assumes that we are connected already).
    /// Create a valid worker once we are connected.
    /// </summary>
    /// <param name="connectionString"></param>
    /// <returns></returns>
    Task<ISQLiteServerConnectionWorker> OpenAsync( string connectionString );

    /// <summary>
    /// Close the connections.
    /// </summary>
    void Disconnect();
  }
}
