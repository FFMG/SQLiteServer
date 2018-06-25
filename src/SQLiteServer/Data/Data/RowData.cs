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
using SQLiteServer.Fields;

namespace SQLiteServer.Data.Data
{
  internal class RowData
  {
    /// <summary>
    /// The actual values/types
    /// </summary>
    public List<Field> Columns;

    /// <summary>
    /// If the columns are null or not.
    /// </summary>
    public List<bool> Nulls;
  }
}
