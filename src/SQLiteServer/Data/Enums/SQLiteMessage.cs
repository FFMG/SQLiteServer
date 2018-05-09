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
namespace SQLiteServer.Data.Enums
{
  // ReSharper disable InconsistentNaming
  /// <inheritdoc />
  internal enum SQLiteMessage : uint
  {
    Unknown,
    SendAndWaitRequest,
    SendAndWaitResponse,
    SendAndWaitBusy,
    SendAndWaitTimeOut,

    CreateCommandRequest,
    CreateCommandResponse,
    DisposeCommand,
    CancelCommand,
    CreateCommandException,

    LockConnectionRequest,
    UnLockConnectionRequest,
    LockConnectionResponse,
    LockConnectionException,

    ExecuteNonQueryRequest,
    ExecuteNonQueryResponse,
    ExecuteNonQueryException,

    ExecuteReaderRequest,
    ExecuteReaderReadRequest,
    ExecuteReaderNextResultRequest,
    ExecuteReaderFieldCountRequest,
    ExecuteReaderHasRowsRequest,
    ExecuteReaderGetRow,
    ExecuteReaderGetOrdinalRequest,
    ExecuteReaderGetStringRequest,
    ExecuteReaderGetInt16Request,
    ExecuteReaderGetInt32Request,
    ExecuteReaderGetInt64Request,
    ExecuteReaderGetDoubleRequest,
    ExecuteReaderGetFieldTypeRequest,
    ExecuteReaderGetDataTypeNameRequest,
    ExecuteReaderGetIsDBNullRequest,
    ExecuteReaderGetNameRequest,
    ExecuteReaderGetTableNameRequest,
    ExecuteReaderResponse,
    ExecuteReaderException,
  }
}