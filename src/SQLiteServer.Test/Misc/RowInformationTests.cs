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
using System.Data;
using NUnit.Framework;
using SQLiteServer.Data.Data;
using SQLiteServer.Fields;

namespace SQLiteServer.Test.Misc
{
  [TestFixture]
  internal class RowInformationTests
  {
    /// <summary>
    /// Create a valid field item
    /// </summary>
    private static Field ValidField => new Field("Hello", 42);

    [Test]
    public void CannotAddNullColumn()
    {
      var r = new RowInformation();
      Assert.Throws<ArgumentNullException>(() => r.Add(null));
    }

    [Test]
    public void CannotAddSameOrdinal()
    {
      var r = new RowInformation();
      r.Add(new ColumnInformation(ValidField, 0, "cola"));
      r.Add(new ColumnInformation(ValidField, 1, "colb"));
      Assert.Throws<DuplicateNameException>( () => r.Add( new ColumnInformation( ValidField, 0, "colc")));
    }

    [Test]
    public void CannotAddSameName()
    {
      var r = new RowInformation();
      r.Add(new ColumnInformation(ValidField, 0, "cola"));
      r.Add(new ColumnInformation(ValidField, 1, "colb"));
      Assert.Throws<DuplicateNameException>(() => r.Add(new ColumnInformation(ValidField, 2, "cola")));
    }

    [Test]
    public void TryingToGetByNameThatDoesNotExist()
    {
      var r = new RowInformation();
      r.Add(new ColumnInformation(ValidField, 0, "cola"));
      r.Add(new ColumnInformation(ValidField, 1, "colb"));

      Assert.Throws<ArgumentOutOfRangeException>( () => r.Get("colc"));
    }

    [Test]
    public void TryingToGetByOrdinalThatDoesNotExist()
    {
      var r = new RowInformation();
      r.Add(new ColumnInformation(ValidField, 0, "cola"));
      r.Add(new ColumnInformation(ValidField, 1, "colb"));

      Assert.Throws<ArgumentOutOfRangeException>( () => r.Get(2));
    }

    [Test]
    public void GetByName()
    {
      var f = ValidField;
      var r = new RowInformation();
      r.Add(new ColumnInformation(f, 0, "cola"));
      r.Add(new ColumnInformation(ValidField, 1, "colb"));

      var c = r.Get("cola");
      Assert.AreEqual(c.Ordinal, 0);
      Assert.AreEqual(c.Name, "cola");
      Assert.AreEqual(f.Name, c.Field.Name);
      Assert.AreEqual(f.Type, c.Field.Type);
    }

    [Test]
    public void GetByNameCaseInsensitive()
    {
      var f = ValidField;
      var r = new RowInformation();
      r.Add(new ColumnInformation(f, 0, "cola"));
      r.Add(new ColumnInformation(ValidField, 1, "colb"));

      Assert.IsNotNull( r.Get("cola"));
      Assert.IsNotNull(r.Get("COLA"));
      Assert.IsNotNull(r.Get("ColA"));
    }

    [Test]
    public void GetByOrdinal()
    {
      var f = ValidField;
      var r = new RowInformation();
      r.Add(new ColumnInformation(f, 0, "cola"));
      r.Add(new ColumnInformation(ValidField, 1, "colb"));

      var c = r.Get(0);
      Assert.AreEqual(c.Ordinal, 0 );
      Assert.AreEqual(c.Name, "cola");
      Assert.AreEqual(f.Name, c.Field.Name);
      Assert.AreEqual(f.Type, c.Field.Type);
    }
  }
}
