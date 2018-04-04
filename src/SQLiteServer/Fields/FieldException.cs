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

namespace SQLiteServer.Fields
{
  internal class FieldException : Exception
  {
    /// <summary>
    /// Initializes a new instance of the FieldsException class.
    /// </summary>
    public FieldException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the FieldsException class with an error message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public FieldException(string message)
      : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the FieldsException class
    /// with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The exception.</param>
    public FieldException(string message, Exception innerException)
      : base(message, innerException)
    {
    }
  }
}
