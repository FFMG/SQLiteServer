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
using System.Linq;
using NUnit.Framework;
using SQLiteServer.Fields;
using System.Collections.Generic;

namespace SQLiteServer.Test.Fields
{
  [TestFixture]
  internal class FieldTests
  {
    [Test]
    public void PackIntValueGivenFiledType()
    {
      var field = new Field("Hello", 12);

      Assert.That(field.Pack().SequenceEqual(new byte[]
      {
        5, 0, 0, 0, // length
        72, 101, 108, 108, 111, // "Hello"
        3, 0, 0, 0, // Field.FieldType.Int32
        4, 0, 0, 0, // length
        12, 0, 0, 0, // "12"
      }));
    }

    [Test]
    public void PackLongValueGivenFiledType()
    {
      var field = new Field("Hello", (long) 12);

      Assert.That(field.Pack().SequenceEqual(new byte[]
      {
        5, 0, 0, 0, // length
        72, 101, 108, 108, 111, // "Hello"
        4, 0, 0, 0, // Field.FieldType.Int64
        8, 0, 0, 0, // length
        12, 0, 0, 0, 0, 0, 0, 0 // "12"
      }));
    }

    [Test]
    public void PackIntValueGivenType()
    {
      var field = new Field("Hello", typeof(int), 18);

      Assert.That(field.Pack().SequenceEqual(new byte[]
      {
        5, 0, 0, 0, // length
        72, 101, 108, 108, 111, // "Hello"
        3, 0, 0, 0, // Field.FieldType.Int
        4, 0, 0, 0, // length
        18, 0, 0, 0, // "18"
      }));
    }

    [Test]
    public void PackStringValueGivenFiledType()
    {
      var field = new Field("Hello", "World");

      Assert.That(field.Pack().SequenceEqual(new byte[]
      {
        5, 0, 0, 0, // length
        72, 101, 108, 108, 111, // "Hello"
        5, 0, 0, 0, // Field.FieldType.String
        5, 0, 0, 0, // length
        87, 111, 114, 108, 100 // "World"
      }));
    }

    [Test]
    public void PackShortValueGivenFiledType()
    {
      var field = new Field("Hello", (short)12);

      Assert.That(field.Pack().SequenceEqual(new byte[]
      {
        5, 0, 0, 0, // length
        72, 101, 108, 108, 111, // "Hello"
        2, 0, 0, 0, // Field.FieldType.Int16
        2, 0, 0, 0, // length
        12, 0 // "12"
      }));
    }

    [Test]
    public void PackDoubleValueGivenFiledType()
    {
      var field = new Field("Hello", (double)3.14);

      Assert.That(field.Pack().SequenceEqual(new byte[]
      {
        5, 0, 0, 0, // length
        72, 101, 108, 108, 111, // "Hello"
        6, 0, 0, 0, // Field.FieldType.Double
        8, 0, 0, 0, // length
        31, 133, 235, 81, 184, 30, 9, 64 // "3.14"
      }));
    }

    [Test]
    public void TryingToUnpackANullValue()
    {
      Assert.Throws<ArgumentNullException>(() => { Field.Unpack(null); });
    }

    [Test]
    public void CannotFindLengthToUnpack()
    {
      Assert.Throws<FieldException>(() => { Field.Unpack(new byte[] {2, 0, 0}); });
    }

    [Test]
    public void CannotFindNameToUnpack()
    {
      Assert.Throws<FieldException>(() => { Field.Unpack(new byte[] {2, 0, 0, 0, 0}); });
    }

    [Test]
    public void NoDataToGetTypeToUnpack()
    {
      Assert.Throws<FieldException>(() =>
      {
        Field.Unpack(new byte[]
        {
          2, 0, 0, 0, //  len
          0, 1, //  "AB"

        });
      });
    }

    [Test]
    public void NotEnoughDataToGetTypeToUnpack()
    {
      Assert.Throws<FieldException>(() =>
      {
        Field.Unpack(new byte[]
        {
          2, 0, 0, 0, //  len
          0, 1, //  "AB"
          2, 0,
        });
      });
    }

    [Test]
    public void NoDataToObjectLengthToUnpack()
    {
      Assert.Throws<FieldException>(() =>
      {
        Field.Unpack(new byte[]
        {
          2, 0, 0, 0, //  len
          0, 1, //  "AB"
          0, 0, 0, 0 //  Int                       
        });
      });
    }


    [Test]
    public void NotEnoughDataForObjectLengthToUnpack()
    {
      Assert.Throws<FieldException>(() =>
      {
        Field.Unpack(new byte[]
        {
          2, 0, 0, 0, //  len
          0, 1, //  "AB"
          0, 0, 0, 0, //  Int      
          1, 0, 0
        });
      });
    }

    [Test]
    public void TheLenghtOfTheIntObjectIsTooLong()
    {
      Assert.Throws<FieldException>(() =>
      {
        Field.Unpack(new byte[]
        {
          2, 0, 0, 0, //  len
          65, 66, //  "AB"
          1, 0, 0, 0, //  Int      
          5, 0, 0, 0, //  len
          12, 0, 0, 0, 0 //  12
        });
      });
    }

    [Test]
    public void UnpackInt()
    {
      var field = Field.Unpack(new byte[]
      {
        2, 0, 0, 0, //  len
        65, 66, //  "AB"
        3, 0, 0, 0, //  Int      
        4, 0, 0, 0, //  len
        12, 0, 0, 0 //  12
      });

      Assert.That(field.Type == FieldType.Int32);
      Assert.That(field.Name == "AB");
      Assert.That(field.Get<int>() == 12);
    }

    [Test]
    public void UnpackLong()
    {
      var field = Field.Unpack(new byte[]
      {
        2, 0, 0, 0, //  len
        65, 66, //  "AB"
        4, 0, 0, 0, //  long
        8, 0, 0, 0, //  len
        12, 0, 0, 0, 0, 0, 0, 0 //  12
      });

      Assert.That(field.Type == FieldType.Int64);
      Assert.That(field.Name == "AB");
      Assert.That(field.Get<long>() == 12);
    }

    [Test]
    public void UnpackString()
    {
      var field = Field.Unpack(new byte[]
      {
        5, 0, 0, 0, // length
        72, 101, 108, 108, 111, // "Hello"
        5, 0, 0, 0, // Field.FieldType.String
        5, 0, 0, 0, // length
        87, 111, 114, 108, 100 // "World"
      });
      Assert.That(field.Type == FieldType.String);
      Assert.That(field.Name == "Hello");
      Assert.That(field.Get<string>() == "World");
    }

    [Test]
    public void UnpackNullString()
    {
      var field = Field.Unpack(new byte[]
      {
        5, 0, 0, 0, // length
        72, 101, 108, 108, 111, // "Hello"
        0, 0, 0, 0, // Field.FieldType.Null
        0, 0, 0, 0 // length
      });
      Assert.That(field.Type == FieldType.Null);
      Assert.That(field.Name == "Hello");
      Assert.IsNull( field.Get<string>());
    }

    [Test]
    public void FieldTypeToTypeInt16()
    {
      Assert.AreEqual(typeof(short), Field.FieldTypeToType(FieldType.Int16));
    }

    [Test]
    public void FieldTypeToTypeInt32()
    {
      Assert.AreEqual(typeof(int), Field.FieldTypeToType(FieldType.Int32));
    }

    [Test]
    public void FieldTypeToTypeInt64()
    {
      Assert.AreEqual(typeof(long), Field.FieldTypeToType(FieldType.Int64));
    }

    [Test]
    public void FieldTypeToTypeDouble()
    {
      Assert.AreEqual(typeof(double), Field.FieldTypeToType(FieldType.Double));
    }

    [Test]
    public void FieldTypeToTypeBytes()
    {
      Assert.AreEqual(typeof(byte[]), Field.FieldTypeToType(FieldType.Bytes));
    }

    [Test]
    public void FieldTypeToTypeCheckAllTypesAreSupported()
    {
      // make sure we handle all the types.
      foreach (FieldType type in Enum.GetValues(typeof(FieldType)))
      {
        Assert.IsInstanceOf<Type>(Field.FieldTypeToType(type));
      }
    }

    [Test]
    public void TypeToFieldTypeInt16()
    {
      Assert.AreEqual(FieldType.Int16, Field.TypeToFieldType(typeof(short)));
    }

    [Test]
    public void TypeToFieldTypeInt32()
    {
      Assert.AreEqual(FieldType.Int32, Field.TypeToFieldType(typeof(int)));
    }

    [Test]
    public void TypeToFieldTypeInt64()
    {
      Assert.AreEqual(FieldType.Int64, Field.TypeToFieldType(typeof(long)));
    }

    [Test]
    public void TypeToFieldTypeDouble()
    {
      Assert.AreEqual(FieldType.Double, Field.TypeToFieldType(typeof(double)));
    }

    [Test]
    public void TypeToFieldTypeString()
    {
      Assert.AreEqual(FieldType.String, Field.TypeToFieldType(typeof(string)));
    }

    [Test]
    public void TypeToFieldTypeBytes()
    {
      Assert.AreEqual(FieldType.Bytes, Field.TypeToFieldType(typeof(byte[])));
    }

    [Test]
    public void TypeToFieldTypeUnknownType()
    {
      Assert.Throws<NotSupportedException>(() =>
      {
        Field.TypeToFieldType(typeof(decimal));
      });
    }


    [Test]
    public void PackListOfInts()
    {
      var values = new List<int> {1, 2, 4, 8};
      var f = new Field( "Hello", values);
      var p = f.Pack();
      var upf = Field.Unpack(p);

      CollectionAssert.AreEqual(values, upf.Get<List<int>>() );
    }

    [Test]
    public void PackListOfNullableInts()
    {
      var values = new List<int?> { 1, 2, null, 4, 8 };
      var f = new Field("Hello", values);
      var p = f.Pack();
      var upf = Field.Unpack(p);

      CollectionAssert.AreEqual(values, upf.Get<List<int?>>());
    }

    [Test]
    public void PackListOfNullableIntsWithNoNull()
    {
      var values = new List<int?> { 1, 2, 4, 8 };
      var f = new Field("Hello", values);
      var p = f.Pack();
      var upf = Field.Unpack(p);

      CollectionAssert.AreEqual(values, upf.Get<List<int?>>());
    }

    [Test]
    public void PackEmptyListOfInts()
    {
      var values = new List<int>();
      var f = new Field("Hello", values);
      var p = f.Pack();
      var upf = Field.Unpack(p);

      CollectionAssert.AreEqual(values, upf.Get<List<int>>());
    }

    [Test]
    public void NullStringValue()
    {
      const string s = null;
      var f = new Field("Hello", s );
      var p = f.Pack();
      var upf = Field.Unpack(p);

      Assert.IsNull( upf.Get<string>());
    }

    [Test]
    public void NullStringValueToInt()
    {
      const string s = null;
      var f = new Field("Hello", s);
      var p = f.Pack();
      var upf = Field.Unpack(p);

      Assert.AreEqual( 0, upf.Get<int>());
    }

    [Test]
    public void PackAndUnpackBytes()
    {
      var bs = new byte[] { 1, 2, 3, 4 };
      var f = new Field("Hello", bs);
      var p = f.Pack();
      var upf = Field.Unpack(p);

      Assert.AreEqual(bs, upf.Get<byte[]>());
    }

    [Test]
    public void NullStringValueToNullInt()
    {
      const string s = null;
      var f = new Field("Hello", s);
      var p = f.Pack();
      var upf = Field.Unpack(p);

      Assert.IsNull(upf.Get<int?>());
    }

    [Test]
    public void NullStringValueToNullShort()
    {
      const string s = null;
      var f = new Field("Hello", s);
      var p = f.Pack();
      var upf = Field.Unpack(p);

      Assert.IsNull(upf.Get<short?>());
    }

    [Test]
    public void NullStringValueToNullLong()
    {
      const string s = null;
      var f = new Field("Hello", s);
      var p = f.Pack();
      var upf = Field.Unpack(p);

      Assert.IsNull(upf.Get<long?>());
    }

    [Test]
    public void NullStringValueToNullDouble()
    {
      const string s = null;
      var f = new Field("Hello", s);
      var p = f.Pack();
      var upf = Field.Unpack(p);

      Assert.IsNull(upf.Get<double?>());
    }

    [Test]
    public void NullStringValueIsFalse()
    {
      const string s = null;
      var f = new Field("Hello", s);
      var p = f.Pack();
      var upf = Field.Unpack(p);

      Assert.IsFalse(upf.Get<bool>());
    }

    [TestCase(0)]
    [TestCase(12)]
    [TestCase(-12)]
    [TestCase(int.MinValue)]
    [TestCase(int.MaxValue)]
    public void FieldOfField( int value )
    {
      var inner = new Field( "inner", value );
      var outer = new Field( "outer", inner);
      var pInner = outer.Pack();
      var upInner = Field.Unpack(pInner);

      Assert.AreEqual(value , upInner.Get<int>());
    }

    [Test]
    public void FieldIsListOfFields()
    {
      var fs = new List<Field>
      {
        new Field("a", 0),
        new Field("b", 1),
        new Field("c", 2),
        new Field("d", 4)
      };

      var outer = new Field("outer", fs);
      var pInner = outer.Pack();
      var upInner = Field.Unpack(pInner);

      var ufs = upInner.Get<List<Field>>();

      Assert.AreEqual(0, ufs[0].Get<int>());
      Assert.AreEqual(1, ufs[1].Get<int>());
      Assert.AreEqual(2, ufs[2].Get<int>());
      Assert.AreEqual(4, ufs[3].Get<int>());
    }

    [Test]
    public void FieldIsListOfFieldsWithNull()
    {
      var fs = new List<Field>
      {
        new Field("a", 0),
        new Field("b", 1),
        null,
        new Field("d", 4)
      };

      var outer = new Field("outer", fs);
      var pInner = outer.Pack();
      var upInner = Field.Unpack(pInner);

      var ufs = upInner.Get<List<Field>>();

      Assert.AreEqual(0, ufs[0].Get<int>());
      Assert.AreEqual(1, ufs[1].Get<int>());
      Assert.IsNull(ufs[2]);
      Assert.AreEqual(4, ufs[3].Get<int>());
    }
  }
}
