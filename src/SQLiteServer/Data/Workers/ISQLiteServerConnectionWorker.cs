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

using System.Data.SQLite;
using System.Threading.Tasks;

namespace SQLiteServer.Data.Workers
{
  // ReSharper disable once InconsistentNaming
  internal interface ISQLiteServerConnectionWorker
  {
    /// <summary>
    /// How long we will run a query before timing out.
    /// </summary>
    int CommandTimeout { get; }

    /// <summary>
    /// Get the direct SQLite connection, that could me blocking all other
    /// clients from accessing the data.
    /// </summary>
    Task<SQLiteConnection> LockConnectionAsync();

    /// <summary>
    /// Release the SQLiteConnection lock.
    /// </summary>
    Task UnLockConnectionAsync();

    /// <summary>
    /// Open the database using the connection string.
    /// </summary>
    void Open();

    /// <summary>
    /// Close the database and the related commands.
    /// </summary>
    void Close();

    /// <summary>
    /// Create a command worker.
    /// </summary>
    /// <param name="commandText"></param>
    /// <returns></returns>
    Task<ISQLiteServerCommandWorker> CreateCommandAsync(string commandText);
  }
}