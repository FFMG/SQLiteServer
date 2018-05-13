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
using System.Collections.Generic;
using SQLiteServer.Fields;

namespace SQLiteServer.Data.Data
{
  internal class RowHeader
  {
    /// <summary>
    /// All the column names
    /// </summary>
    public List<string> Names;

    /// <summary>
    /// The field types
    /// </summary>
    public List<int> Types;

    /// <summary>
    /// If we have rows
    /// </summary>
    public bool HasRows;

    /// <summary>
    /// The number of fields
    /// </summary>
    public int FieldCount => Names.Count;

    /// <summary>
    /// Get the ordinal of a certain item.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public int GetOrdinal(string name)
    {
      if (string.IsNullOrWhiteSpace(name))
      {
        throw new ArgumentException("The name cannot be empty or null");
      }
      return Names.FindIndex(n => n.Equals(name, StringComparison.OrdinalIgnoreCase));
    }


    /// <summary>
    /// Get the column field type.
    /// </summary>
    /// <param name="ordinal"></param>
    /// <returns></returns>
    public Type GetType(int ordinal)
    {
      if (ordinal < 0 || ordinal >= Types.Count)
      {
        throw new IndexOutOfRangeException($"Could not find column {ordinal}!");
      }
      return Field.FieldTypeToType((FieldType)Types[ordinal]);
    }
  }
}
