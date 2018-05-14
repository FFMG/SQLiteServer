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
      var header = new RowHeader
      {
        HasRows = true,
        Names = new List<string> { "cola" },
        Types = new List<int> { (int)FieldType.Object }
      };

      var r = new RowInformation( header);
      Assert.Throws<ArgumentNullException>(() => r.Add(null));
    }

    [Test]
    public void TheListOfColumnsCannotContainNulls()
    {
      var header = new RowHeader
      {
        HasRows = true,
        Names = new List<string> { "cola", null },
        Types = new List<int> { (int)FieldType.Object, (int)FieldType.String }
      };
      // ReSharper disable once ObjectCreationAsStatement
      Assert.Throws<ArgumentNullException>(() => new RowInformation(header));
    }

    [Test]
    public void TheListOfColumnsCannotContainSpaces()
    {
      var header = new RowHeader
      {
        HasRows = true,
        Names = new List<string> { "cola", "     " },
        Types = new List<int> { (int)FieldType.Object, (int)FieldType.String }
      };

      // ReSharper disable once ObjectCreationAsStatement
      Assert.Throws<ArgumentException>(() => new RowInformation(header));
    }

    [Test]
    public void NamesOfColumnsCannotBeEmpty()
    {
      var header = new RowHeader
      {
        HasRows = true,
        Names = new List<string> { "cola", "" },
        Types = new List<int> { (int)FieldType.Object, (int)FieldType.String }
      };

      // ReSharper disable once ObjectCreationAsStatement
      Assert.Throws<ArgumentException>(() => new RowInformation(header));
    }

    [Test]
    public void TheListOfColumnsGivenCannotBeEmpty()
    {
      var header = new RowHeader
      {
        HasRows = true,
        Names = new List<string>(),
        Types = new List<int>()
      };

      // ReSharper disable once ObjectCreationAsStatement
      Assert.Throws<ArgumentException>(() => new RowInformation(header));
    }

    [Test]
    public void CannotAddSameOrdinal()
    {
      var header = new RowHeader
      {
        HasRows = true,
        Names = new List<string> { "cola", "colb", "colc" },
        Types = new List<int> { (int)FieldType.Object, (int)FieldType.String, (int)FieldType.Double }
      };

      var r = new RowInformation( header);
      r.Add(new ColumnInformation(ValidField, 0, "cola", false));
      r.Add(new ColumnInformation(ValidField, 1, "colb", false));
      Assert.Throws<DuplicateNameException>( () => r.Add( new ColumnInformation( ValidField, 0, "colc", false)));
    }

    [Test]
    public void AddingAColumnNameThatDoesNotExist()
    {
      var header = new RowHeader
      {
        HasRows = true,
        Names = new List<string> { "cola", "colb" },
        Types = new List<int> { (int)FieldType.Object, (int)FieldType.String }
      };

      var r = new RowInformation(header);
      r.Add(new ColumnInformation(ValidField, 0, "cola", false));
      Assert.Throws<ArgumentException>(() => r.Add(new ColumnInformation(ValidField, 1, "colc", false)));
    }

    [Test]
    public void AddingAColumnOrdinalThatDoesNotExist()
    {
      var header = new RowHeader
      {
        HasRows = true,
        Names = new List<string> { "cola", "colb" },
        Types = new List<int> { (int)FieldType.Object, (int)FieldType.String }
      };

      var r = new RowInformation( header);
      r.Add(new ColumnInformation(ValidField, 0, "cola", false));
      Assert.Throws<ArgumentException>(() => r.Add(new ColumnInformation(ValidField, 3, "colb", false)));
    }

    [Test]
    public void OrdinalDoesNotMatchTheName()
    {
      var header = new RowHeader
      {
        HasRows = true,
        Names = new List<string> { "cola", "colb", "colc" },
        Types = new List<int> { (int)FieldType.Object, (int)FieldType.String, (int)FieldType.Double }
      };

      var r = new RowInformation(header);
      Assert.Throws<ArgumentException>(() => r.Add(new ColumnInformation(ValidField, 0, "colb", false)));
    }

    [Test]
    public void CannotAddSameName()
    {
      var header = new RowHeader
      {
        HasRows = true,
        Names = new List<string> { "cola", "colb", "colc" },
        Types = new List<int> { (int)FieldType.Object, (int)FieldType.String, (int)FieldType.Double }
      };

      var r = new RowInformation(header);
      r.Add(new ColumnInformation(ValidField, 0, "cola", false));
      r.Add(new ColumnInformation(ValidField, 1, "colb", false));
      Assert.Throws<DuplicateNameException>(() => r.Add(new ColumnInformation(ValidField, 2, "cola", false)));
    }

    [Test]
    public void TryingToGetByNameThatDoesNotExist()
    {
      var header = new RowHeader
      {
        HasRows = true,
        Names = new List<string> { "cola", "colb" },
        Types = new List<int> { (int)FieldType.Object, (int)FieldType.String }
      };

      var r = new RowInformation(header);
      r.Add(new ColumnInformation(ValidField, 0, "cola", false));
      r.Add(new ColumnInformation(ValidField, 1, "colb", false));

      Assert.Throws<IndexOutOfRangeException>( () => r.Get("colc"));
    }

    [Test]
    public void TryingToGetByOrdinalThatDoesNotExist()
    {
      var header = new RowHeader
      {
        HasRows = true,
        Names = new List<string> { "cola", "colb"},
        Types = new List<int> { (int)FieldType.Object, (int)FieldType.String }
      };

      var r = new RowInformation(header);
      r.Add(new ColumnInformation(ValidField, 0, "cola", false));
      r.Add(new ColumnInformation(ValidField, 1, "colb", false));

      Assert.Throws<IndexOutOfRangeException>( () => r.Get(2));
    }

    [Test]
    public void GetByName()
    {
      var header = new RowHeader
      {
        HasRows = true,
        Names = new List<string> { "cola", "colb", "colc" },
        Types = new List<int> { (int)FieldType.Object, (int)FieldType.String, (int)FieldType.Double }
      };

      var f = ValidField;
      var r = new RowInformation(header);
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
      var header = new RowHeader
      {
        HasRows = true,
        Names = new List<string> { "cola", "colb", "colc" },
        Types = new List<int> { (int)FieldType.Object, (int)FieldType.String, (int)FieldType.Double }
      };

      var f = ValidField;
      var r = new RowInformation(header);
      r.Add(new ColumnInformation(f, 0, "cola", false));
      r.Add(new ColumnInformation(ValidField, 1, "colb", false));

      Assert.IsNotNull( r.Get("cola"));
      Assert.IsNotNull(r.Get("COLA"));
      Assert.IsNotNull(r.Get("ColA"));
    }

    [Test]
    public void CheckNullValue()
    {
      var header = new RowHeader
      {
        HasRows = true,
        Names = new List<string> { "cola", "colb" },
        Types = new List<int> { (int)FieldType.Object, (int)FieldType.String }
      };

      var f = ValidField;
      var r = new RowInformation(header);
      r.Add(new ColumnInformation(f, 0, "cola", true));
      r.Add(new ColumnInformation(ValidField, 1, "colb", false));

      Assert.IsTrue( r.Get(0).IsNull );
      Assert.IsFalse( r.Get(1).IsNull);
    }

    [Test]
    public void GetByOrdinal()
    {
      var header = new RowHeader
      {
        HasRows = true,
        Names = new List<string> { "cola", "colb" },
        Types = new List<int> { (int)FieldType.Object, (int)FieldType.String }
      };

      var f = ValidField;
      var r = new RowInformation(header);
      r.Add(new ColumnInformation(f, 0, "cola", false));
      r.Add(new ColumnInformation(ValidField, 1, "colb", false));

      var c = r.Get(0);
      Assert.AreEqual(c.Ordinal, 0 );
      Assert.AreEqual(c.Name, "cola");
      Assert.AreEqual(f.Name, c.Field.Name);
      Assert.AreEqual(f.Type, c.Field.Type);
    }

    [Test]
    public void FieldTypeWithNoValue()
    {
      var row = new RowHeader
      {
        HasRows = true,
        Names = new List<string> { "cola", "colb" },
        Types = new List<int> { (int)FieldType.String, (int)FieldType.Object }
      };
      Assert.AreEqual(row.GetType(0), typeof(string));
      Assert.AreEqual(row.GetType(1), typeof(object));
    }

    [Test]
    public void FieldTypeObjectWithNoValue()
    {
      var r = new RowHeader { HasRows = true,
          Names = new List<string> { "cola" },
          Types = new List<int> { (int)FieldType.Object } 
          };

      Assert.AreEqual(r.GetType(0), typeof(object));
    }

    [Test]
    public void TheNumberOfFieldTypesDoesNotMatchTheNumberOfColumnNames()
    {
      var row = new RowHeader
      {
        HasRows = true,
        Names = new List<string> { "cola", "colb" },
        Types = new List<int> { (int)FieldType.Object }
      };
      // ReSharper disable once ObjectCreationAsStatement
      Assert.Throws<ArgumentException>( () => new RowInformation( row ));
    }

    [Test]
    public void FieldTypesCannotBeNull()
    {
      var row = new RowHeader
      {
        HasRows = true,
        Names = new List<string> {"cola"},
        Types = null
      };
      // ReSharper disable once ObjectCreationAsStatement
      Assert.Throws<ArgumentNullException>(() => new RowInformation( row ));
    }
  }
}
