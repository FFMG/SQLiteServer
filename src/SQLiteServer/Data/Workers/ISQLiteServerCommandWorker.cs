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
using System.Threading;
using System.Threading.Tasks;

namespace SQLiteServer.Data.Workers
{
  // ReSharper disable once InconsistentNaming
  internal interface ISQLiteServerCommandWorker : IDisposable
  {
    /// <summary>
    /// How long we will run a query before timing out.
    /// </summary>
    int CommandTimeout { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Try and cancel the command. 
    /// </summary>
    void Cancel();

    /// <summary>
    /// Create the reader that will get data from the db.
    /// </summary>
    /// <returns></returns>
    ISQLiteServerDataReaderWorker CreateReaderWorker();
  }
}