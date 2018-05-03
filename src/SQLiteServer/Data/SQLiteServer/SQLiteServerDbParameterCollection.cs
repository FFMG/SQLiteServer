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
using System.Collections;
using System.Data.Common;

namespace SQLiteServer.Data.SQLiteServer
{
  /// <inheritdoc />
  // ReSharper disable once InconsistentNaming
  public class SQLiteServerDbParameterCollection : DbParameterCollection
  {
    public override int Add(object value)
    {
      throw new NotImplementedException();
    }

    public override bool Contains(object value)
    {
      throw new NotImplementedException();
    }

    public override void Clear()
    {
      throw new NotImplementedException();
    }

    public override int IndexOf(object value)
    {
      throw new NotImplementedException();
    }

    public override void Insert(int index, object value)
    {
      throw new NotImplementedException();
    }

    public override void Remove(object value)
    {
      throw new NotImplementedException();
    }

    public override void RemoveAt(int index)
    {
      throw new NotImplementedException();
    }

    public override void RemoveAt(string parameterName)
    {
      throw new NotImplementedException();
    }

    protected override void SetParameter(int index, DbParameter value)
    {
      throw new NotImplementedException();
    }

    protected override void SetParameter(string parameterName, DbParameter value)
    {
      throw new NotImplementedException();
    }

    public override int Count { get; }
    public override object SyncRoot { get; }

    public override int IndexOf(string parameterName)
    {
      throw new NotImplementedException();
    }

    public override IEnumerator GetEnumerator()
    {
      throw new NotImplementedException();
    }

    protected override DbParameter GetParameter(int index)
    {
      throw new NotImplementedException();
    }

    protected override DbParameter GetParameter(string parameterName)
    {
      throw new NotImplementedException();
    }

    public override bool Contains(string value)
    {
      throw new NotImplementedException();
    }

    public override void CopyTo(Array array, int index)
    {
      throw new NotImplementedException();
    }

    public override void AddRange(Array values)
    {
      throw new NotImplementedException();
    }
  }
}
