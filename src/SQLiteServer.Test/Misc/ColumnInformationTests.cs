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
using SQLiteServer.Data.Data;
using SQLiteServer.Fields;

namespace SQLiteServer.Test.Misc
{
  [TestFixture]
  internal class ColumnInformationTests
  {
    /// <summary>
    /// Create a valid field item
    /// </summary>
    private static Field ValidField => new Field("Hello", 42);

    [Test]
    public void CheckValuesAreSaved()
    {
      const string name = "World";
      const int ordinal = 13;
      var f = new Field("Hello", 42);
      var c = new ColumnInformation(f, ordinal, name );
      Assert.AreEqual( name, c.Name );
      Assert.AreEqual(ordinal, c.Ordinal);
      Assert.AreEqual("Hello", f.Name);
      Assert.AreEqual(42, f.Get<int>());
    }

    [Test]
    public void NameCannotBeNull()
    {
      // ReSharper disable once ObjectCreationAsStatement
      Assert.Throws<ArgumentException>(() => new ColumnInformation(ValidField, 12, null));
    }

    [Test]
    public void NameCannotBeEmpty()
    {
      // ReSharper disable once ObjectCreationAsStatement
      Assert.Throws<ArgumentException>(() => new ColumnInformation(ValidField, 12, ""));
    }

    [Test]
    public void NameCannotBeEmptyWithSpaces()
    {
      // ReSharper disable once ObjectCreationAsStatement
      Assert.Throws<ArgumentException>(() => new ColumnInformation(ValidField, 12, "           "));
    }

    [Test]
    public void FieldCannotBeNull()
    {
      // ReSharper disable once ObjectCreationAsStatement
      Assert.Throws<ArgumentNullException>(() => new ColumnInformation(null, 12, "Hello"));
    }

    [TestCase(-1)]
    [TestCase(-2)]
    [TestCase(-100)]
    [TestCase( int.MinValue)]
    public void OrdinalCannotBeNegative( int value )
    {
      // ReSharper disable once ObjectCreationAsStatement
      Assert.Throws<ArgumentException>(() => new ColumnInformation(ValidField, value, "Hello"));
    }
  }
}
