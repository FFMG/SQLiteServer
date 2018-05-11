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
      var r = new RowInformation(new List<string> { "cola", "colb" });
      Assert.Throws<ArgumentNullException>(() => r.Add(null));
    }

    [Test]
    public void TheListOfColumnsCannotContainNulls()
    {
      // ReSharper disable once ObjectCreationAsStatement
      Assert.Throws<ArgumentNullException>(() => new RowInformation(new List<string> { "cola", null }));
    }

    [Test]
    public void TheListOfColumnsCannotContainSpaces()
    {
      // ReSharper disable once ObjectCreationAsStatement
      Assert.Throws<ArgumentException>(() => new RowInformation(new List<string> { "cola", "    " }));
    }

    [Test]
    public void NamesOfColumnsCannotBeEmpty()
    {
      // ReSharper disable once ObjectCreationAsStatement
      Assert.Throws<ArgumentException>(() => new RowInformation(new List<string> { "cola", "" }));
    }

    [Test]
    public void TheListOfColumnsGivenCannotBeEmpty()
    {
      // ReSharper disable once ObjectCreationAsStatement
      Assert.Throws<ArgumentException>(() => new RowInformation(new List<string>()));
    }

    [Test]
    public void CannotAddSameOrdinal()
    {
      var r = new RowInformation( new List<string>{"cola", "colb", "colc" });
      r.Add(new ColumnInformation(ValidField, 0, "cola", false));
      r.Add(new ColumnInformation(ValidField, 1, "colb", false));
      Assert.Throws<DuplicateNameException>( () => r.Add( new ColumnInformation( ValidField, 0, "colc", false)));
    }

    [Test]
    public void AddingAColumnNameThatDoesNotExist()
    {
      var r = new RowInformation(new List<string> { "cola", "colb" });
      r.Add(new ColumnInformation(ValidField, 0, "cola", false));
      Assert.Throws<ArgumentException>(() => r.Add(new ColumnInformation(ValidField, 1, "colc", false)));
    }

    [Test]
    public void AddingAColumnOrdinalThatDoesNotExist()
    {
      var r = new RowInformation(new List<string> { "cola", "colb" });
      r.Add(new ColumnInformation(ValidField, 0, "cola", false));
      Assert.Throws<ArgumentException>(() => r.Add(new ColumnInformation(ValidField, 3, "colb", false)));
    }

    [Test]
    public void OrdinalDoesNotMatchTheNsmr()
    {
      var r = new RowInformation(new List<string> { "cola", "colb" });
      Assert.Throws<ArgumentException>(() => r.Add(new ColumnInformation(ValidField, 0, "colb", false)));
    }

    [Test]
    public void CannotAddSameName()
    {
      var r = new RowInformation(new List<string> { "cola", "colb" });
      r.Add(new ColumnInformation(ValidField, 0, "cola", false));
      r.Add(new ColumnInformation(ValidField, 1, "colb", false));
      Assert.Throws<DuplicateNameException>(() => r.Add(new ColumnInformation(ValidField, 2, "cola", false)));
    }

    [Test]
    public void TryingToGetByNameThatDoesNotExist()
    {
      var r = new RowInformation(new List<string> { "cola", "colb" });
      r.Add(new ColumnInformation(ValidField, 0, "cola", false));
      r.Add(new ColumnInformation(ValidField, 1, "colb", false));

      Assert.Throws<IndexOutOfRangeException>( () => r.Get("colc"));
    }

    [Test]
    public void TryingToGetByOrdinalThatDoesNotExist()
    {
      var r = new RowInformation(new List<string> { "cola", "colb" });
      r.Add(new ColumnInformation(ValidField, 0, "cola", false));
      r.Add(new ColumnInformation(ValidField, 1, "colb", false));

      Assert.Throws<IndexOutOfRangeException>( () => r.Get(2));
    }

    [Test]
    public void GetByName()
    {
      var f = ValidField;
      var r = new RowInformation(new List<string> { "cola", "colb" });
      r.Add(new ColumnInformation(f, 0, "cola", false));
      r.Add(new ColumnInformation(ValidField, 1, "colb", false));

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
      var r = new RowInformation(new List<string> { "cola", "colb" });
      r.Add(new ColumnInformation(f, 0, "cola", false));
      r.Add(new ColumnInformation(ValidField, 1, "colb", false));

      Assert.IsNotNull( r.Get("cola"));
      Assert.IsNotNull(r.Get("COLA"));
      Assert.IsNotNull(r.Get("ColA"));
    }

    [Test]
    public void GetByOrdinal()
    {
      var f = ValidField;
      var r = new RowInformation(new List<string> { "cola", "colb" });
      r.Add(new ColumnInformation(f, 0, "cola", false));
      r.Add(new ColumnInformation(ValidField, 1, "colb", false));

      var c = r.Get(0);
      Assert.AreEqual(c.Ordinal, 0 );
      Assert.AreEqual(c.Name, "cola");
      Assert.AreEqual(f.Name, c.Field.Name);
      Assert.AreEqual(f.Type, c.Field.Type);
    }
  }
}
