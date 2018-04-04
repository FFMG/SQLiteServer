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

namespace SQLiteServer.Test.Fields
{
  [TestFixture]
  internal class FiledsTests
  {
    private struct TestAllTypes
    {
      public short A;
      public int B;
      public long C;
      public string D;
      public double E;
      public byte[] F;
    }

    private struct TestFieldsStructure
    {
      public string A;
      public int B;
    }

    private class TestFieldsClass
    {
      public string A;
      public int B;
    }

    private class TestFieldsClass2
    {
      public string A { get; }
      public int B { get; }

      public TestFieldsClass2() : this( null, 0 )
      {
      }

      public TestFieldsClass2(string a, int b)
      {
        A = a;
        B = b;
      }
    }

    private class TestFieldsClass3
    {
      public string A { get; }
      public int B { get; }
      
      public TestFieldsClass3(string a, int b)
      {
        A = a;
        B = b;
      }
    }

    [Test]
    public void DefaultFieldsHasZeroCount()
    {
      var fields = new global::SQLiteServer.Fields.Fields();
      Assert.That(fields.Count == 0);
    }

    [Test]
    public void DeserializeObjectAllValueTypesAtOnce()
    {
      var x = new TestAllTypes
      {
        A = (short) 20,
        B = 30,
        C = 40,
        D = "Hello",
        E = 3.14,
        F = new byte[] {0, 1, 2, 3}
      };

      // pack it...
      var fieldPack = global::SQLiteServer.Fields.Fields.SerializeObject(x);

      // unpack it
      var fieldsUnpack = global::SQLiteServer.Fields.Fields.DeserializeObject<TestAllTypes>(fieldPack);

      Assert.AreEqual(x.A, fieldsUnpack.A);
      Assert.AreEqual(x.B, fieldsUnpack.B);
      Assert.AreEqual(x.C, fieldsUnpack.C);
      Assert.AreEqual(x.D, fieldsUnpack.D);
      Assert.AreEqual(x.E, fieldsUnpack.E);
      Assert.AreEqual(x.F, fieldsUnpack.F);
      Assert.That(x.F.SequenceEqual(fieldsUnpack.F));
    }

    [Test]
    public void PackAllValueTypesAtOnce()
    {
      var x = new TestAllTypes
      {
        A = (short)20,
        B = 30,
        C = 40,
        D = "Hello",
        E = 3.14,
        F = new byte[] { 0, 1, 2, 3 }
      };

      // pack it...
      var pack = global::SQLiteServer.Fields.Fields.SerializeObject(x).Pack();

      // unpack it
      var unpack = global::SQLiteServer.Fields.Fields.Unpack(pack);
      var fieldsUnpack = global::SQLiteServer.Fields.Fields.DeserializeObject<TestAllTypes>(unpack);

      Assert.AreEqual(x.A, fieldsUnpack.A);
      Assert.AreEqual(x.B, fieldsUnpack.B);
      Assert.AreEqual(x.C, fieldsUnpack.C);
      Assert.AreEqual(x.D, fieldsUnpack.D);
      Assert.AreEqual(x.E, fieldsUnpack.E);
      Assert.AreEqual(x.F, fieldsUnpack.F);
      Assert.That(x.F.SequenceEqual(fieldsUnpack.F));
    }

    [Test]
    public void CheckParameterCount()
    {
      var x = new TestFieldsStructure
      {
        A = "Hello",
        B = 12
      };
      var fields = global::SQLiteServer.Fields.Fields.SerializeObject(x);
      Assert.That( fields.Count == 2 );
    }

    [Test]
    public void SerializeAndDeserializeStructure()
    {
      var x = new TestFieldsStructure
      {
        A = "Hello",
        B = 12
      };

      var fields = global::SQLiteServer.Fields.Fields.SerializeObject(x);
      var y = global::SQLiteServer.Fields.Fields.DeserializeObject<TestFieldsStructure>(fields);

      Assert.AreEqual(x.A, y.A);
      Assert.AreEqual(x.B, y.B);
    }

    [Test]
    public void SerializeAndDeserializeClass()
    {
      var x = new TestFieldsClass
      {
        A = "Hello",
        B = 12
      };

      var fields = global::SQLiteServer.Fields.Fields.SerializeObject(x);
      var y = global::SQLiteServer.Fields.Fields.DeserializeObject<TestFieldsClass>(fields);

      Assert.AreEqual(x.A, y.A);
      Assert.AreEqual(x.B, y.B);
    }

    [Test]
    public void SerializeAndDeserializeClassWithPrivateProperties()
    {
      var x = new TestFieldsClass2( "Hello", 12 );

      var fields = global::SQLiteServer.Fields.Fields.SerializeObject(x);
      var y = global::SQLiteServer.Fields.Fields.DeserializeObject<TestFieldsClass2>(fields);

      Assert.AreEqual(default(string), y.A);
      Assert.AreEqual(default(int), y.B);
    }

    [Test]
    public void SerializeWithNoDefaultConstructor()
    {
      var x = new TestFieldsClass3("Hello", 12);

      Assert.Throws<FieldsException>(
        () => { global::SQLiteServer.Fields.Fields.SerializeObject(x); });
    }

    [Test]
    public void AddFieldsManuallyToStructure()
    {
      var x = new TestFieldsStructure
      {
        A = "Hello",
        B = 12
      };

      var fields = new global::SQLiteServer.Fields.Fields();
      fields.Add( new Field( "A", typeof(string), "Hello"));
      fields.Add( new Field( "B", typeof(int), 12 ));

      var y = global::SQLiteServer.Fields.Fields.DeserializeObject<TestFieldsStructure>(fields);

      Assert.AreEqual(x.A, y.A);
      Assert.AreEqual(x.B, y.B);
    }

    [Test]
    public void AddFieldsManuallyToClassWithPrivateVariables()
    {
      var x = new TestFieldsClass2("Hello", 12);

      var fields = new global::SQLiteServer.Fields.Fields();
      fields.Add(new Field("A", typeof(string), "Hello"));
      fields.Add(new Field("B", typeof(int), 12));

      var y = global::SQLiteServer.Fields.Fields.DeserializeObject<TestFieldsClass2>(fields);

      // we cannot set private values...
      Assert.AreEqual(default(string), y.A);
      Assert.AreEqual(default(int), y.B);
    }

    [Test]
    public void UnpackWithNullbytes()
    {
      Assert.Throws<ArgumentNullException>(
        () => { global::SQLiteServer.Fields.Fields.Unpack(null); });
    }

    [Test]
    public void CannotUnpackBecauseMissingLength()
    {
      Assert.Throws<FieldsException>(
        () => { global::SQLiteServer.Fields.Fields.Unpack( new byte[]
        {
          1, 0, 0
        }); });
    }

    [Test]
    public void CannotUnpackBecauseThereIsNoDataAfterTheLength()
    {
      Assert.Throws<FieldsException>(
        () => {
          global::SQLiteServer.Fields.Fields.Unpack(new byte[]
          {
            5, 0, 0, 0
          });
        });
    }

    [Test]
    public void PackAndUnpackSingleField()
    {
      var fields = new global::SQLiteServer.Fields.Fields();
      fields.Add( new Field( "Hello", "World" ) );

      var pack = fields.Pack();
      var fields2 = global::SQLiteServer.Fields.Fields.Unpack(pack);
      Assert.That( fields2.Count == 1 );
    }

    [Test]
    public void SerializeObjectPackAndUnpackAndDeserializeObject()
    {
      var s1 = new TestFieldsStructure
      {
        A = "Hello",
        B = 12
      };
      
      var fields = global::SQLiteServer.Fields.Fields.SerializeObject(s1);
      var pack = fields.Pack();

      var fields2 = global::SQLiteServer.Fields.Fields.Unpack(pack);
      var s2 = global::SQLiteServer.Fields.Fields.DeserializeObject<TestFieldsStructure>(fields2);

      Assert.AreEqual(s1.A, s2.A);
      Assert.AreEqual(s1.B, s2.B);
    }

    [Test]
    public void ManualCreatePackAndUnpackAndDeserializeObject()
    {
      var s1 = new TestFieldsStructure
      {
        A = "Hello",
        B = 12
      };

      var fields = new global::SQLiteServer.Fields.Fields();
      fields.Add(new Field("A", "Hello"));
      fields.Add(new Field("B", 12 ));

      var pack = fields.Pack();
      var fields2 = global::SQLiteServer.Fields.Fields.Unpack(pack);
      var s2 = global::SQLiteServer.Fields.Fields.DeserializeObject<TestFieldsStructure>(fields2);

      Assert.AreEqual(s1.A, s2.A);
      Assert.AreEqual(s1.B, s2.B);
    }

    [Test]
    public void ManualCreateMoreValuesThanNeededPackAndUnpackAndDeserializeObject()
    {
      var s1 = new TestFieldsStructure
      {
        A = "Hello",
        B = 12
      };

      var fields = new global::SQLiteServer.Fields.Fields();
      fields.Add(new Field("A", "Hello"));
      fields.Add(new Field("B", 12));
      fields.Add(new Field("C", 19));
      fields.Add(new Field("D", "World"));

      var pack = fields.Pack();
      var fields2 = global::SQLiteServer.Fields.Fields.Unpack(pack);
      var s2 = global::SQLiteServer.Fields.Fields.DeserializeObject<TestFieldsStructure>(fields2);

      Assert.AreEqual(s1.A, s2.A);
      Assert.AreEqual(s1.B, s2.B);
    }
  }
}
