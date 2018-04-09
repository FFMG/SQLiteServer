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
using SQLiteServer.Data.Connections;
using SQLiteServer.Data.Enums;

namespace SQLiteServer.Test.Packets
{
  [TestFixture]
  internal class PacketTests
  {
    [Test]
    public void CreateNullPacket()
    {
      var p = new Packet((SQLiteMessage)1, (byte[])null);
      Assert.That( 
        p.Packed.SequenceEqual( new byte [] {0,0,0,0,1,0,0,0}));
    }

    [Test]
    public void CreateNullPacketLength()
    {
      var p = new Packet((SQLiteMessage)1, (byte[])null);
      Assert.That( p.Length == 8 );
    }

    [Test]
    public void CreateSingleBytePacketLength()
    {
      var p = new Packet((SQLiteMessage)1, new byte[] {1});
      Assert.That(p.Length == 9);
    }

    [Test]
    public void CreateSingleBytePacket()
    {
      var p = new Packet((SQLiteMessage)1, new byte[] { 1 });
      Assert.That(
        p.Packed.SequenceEqual(new byte[] { 1, 0, 0, 0, 1, 0, 0, 0, 1 }));
    }

    [Test]
    public void StringConstructor()
    {
      var pack = new Packet((SQLiteMessage)1, "Hello" );
      Assert.That(
        pack.Packed.SequenceEqual(new byte[] { 5, 0, 0, 0, 1, 0, 0, 0, 72, 101, 108, 108, 111 }));
    }

    [Test]
    public void IntConstructor()
    {
      var pack1 = new Packet((SQLiteMessage)1, 9);
      Assert.That(
        pack1.Packed.SequenceEqual(new byte[] { 4, 0, 0, 0, 1, 0, 0, 0, 9, 0, 0, 0 }));

      var pack2 = new Packet((SQLiteMessage)1, 1024);
      Assert.That(
        pack2.Packed.SequenceEqual(new byte[] { 4, 0, 0, 0, 1, 0, 0, 0, 0, 4, 0, 0 }));
    }

    [Test]
    public void StringConstructorGetValue()
    {
      const string s = "Hello";
      var pack = new Packet((SQLiteMessage)1, s);
      Assert.That( s == pack.Get<string>());
    }

    [Test]
    public void IntConstructorGetValue()
    {
      var r = new Random();
      var i = r.Next(0, int.MaxValue);
      var pack = new Packet((SQLiteMessage)1, i);
      Assert.That(i == pack.Get<int>());
    }

    [Test]
    public void IntConstructorGetNegativeValue()
    {
      var r = new Random();
      var i = -1*r.Next(1, int.MaxValue);
      var pack = new Packet((SQLiteMessage)1, i);
      Assert.That(i == pack.Get<int>());
    }

    [Test]
    public void StringConstructorGetNullValue()
    {
      const string s = null;
      var pack = new Packet((SQLiteMessage)1, s);
      Assert.That(s == pack.Get<string>());
    }

    [Test]
    public void NullStringConstructor()
    {
      var pack = new Packet((SQLiteMessage)1, (string)null );
      Assert.That(
        pack.Packed.SequenceEqual(new byte[] { 0, 0, 0, 0, 1, 0, 0, 0 }));
    }

    [Test]
    public void PayloadIsNotBigEnoughToGiveASize()
    {
      //  we need 4 bytes for size.
      Assert.Throws<ArgumentOutOfRangeException>(
        () => new Packet(new byte[] {0, 1})
      );
    }

    [Test]
    public void PayloadIsNotBigEnoughToGiveAType()
    {
      //  we need 4 bytes for size.
      Assert.Throws<ArgumentOutOfRangeException>(
        () => new Packet(new byte[] { 1, 0, 0, 0, 2, 0 })
      );
    }

    [Test]
    public void PayloadDoesNotHaveEnoughForData()
    {
      //  we need 4 bytes for size.
      Assert.Throws<ArgumentOutOfRangeException>(
        () => new Packet(new byte[] { 1, 0, 0, 0, 2, 0, 0, 0 })
      );
    }

    [Test]
    public void PayloadIsTooSmall()
    {
      //  we need 4 bytes for size.
      Assert.Throws<ArgumentOutOfRangeException>(
        () => new Packet(new byte[] { 1, 0, 0, 0, 2, 0, 0, 0, 0, 0 })
      );
    }

    [Test]
    public void PayloadIsTooBig()
    {
      //  we need 4 bytes for size.
      Assert.Throws<ArgumentOutOfRangeException>(
        () => new Packet(new byte[] { 1, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0 })
      );
    }

    [Test]
    public void PayloadIsValid()
    {
      //  we need 4 bytes for size.
      var p = new Packet(new byte[] {
          4, 0, 0, 0,
          2, 0, 0, 0,
          6, 0, 0, 0 });

      Assert.That( p.Get<int>() == 6 );
      Assert.That( (uint)p.Message == 2);
    }

    [Test]
    public void PayloadGetShort()
    {
      //  we need 4 bytes for size.
      var p = new Packet(SQLiteMessage.ExecuteReaderGetInt16Request, (short)12);
      var u = new Packet(p.Packed);
      Assert.That(u.Get<short>() == 12);

      p = new Packet(SQLiteMessage.ExecuteReaderGetInt16Request, short.MaxValue );
      u = new Packet(p.Packed);
      Assert.That(u.Get<short>() == short.MaxValue);

      p = new Packet(SQLiteMessage.ExecuteReaderGetInt16Request, short.MinValue);
      u = new Packet(p.Packed);
      Assert.That(u.Get<short>() == short.MinValue);
    }

    [Test]
    public void PayloadGetInt()
    {
      //  we need 4 bytes for size.
      var p = new Packet( SQLiteMessage.ExecuteReaderGetInt16Request, (int)12 );
      var u = new Packet(p.Packed);
      Assert.That( u.Get<int>() == 12 );

      p = new Packet(SQLiteMessage.ExecuteReaderGetInt16Request, int.MaxValue);
      u = new Packet(p.Packed);
      Assert.That(u.Get<int>() == int.MaxValue);

      p = new Packet(SQLiteMessage.ExecuteReaderGetInt16Request, int.MinValue);
      u = new Packet(p.Packed);
      Assert.That(u.Get<int>() == int.MinValue);
    }

    [Test]
    public void PayloadGetLong()
    {
      //  we need 4 bytes for size.
      var p = new Packet(SQLiteMessage.ExecuteReaderGetInt16Request, (long)12);
      var u = new Packet(p.Packed);
      Assert.That(u.Get<long>() == 12);

      p = new Packet(SQLiteMessage.ExecuteReaderGetInt16Request, long.MaxValue);
      u = new Packet(p.Packed);
      Assert.That(u.Get<long>() == long.MaxValue);

      p = new Packet(SQLiteMessage.ExecuteReaderGetInt16Request, long.MinValue);
      u = new Packet(p.Packed);
      Assert.That(u.Get<long>() == long.MinValue);
    }

    [Test]
    public void PayloadGetDouble()
    {
      const double tolerance = 0.1;
      //  we need 4 bytes for size.
      var p = new Packet(SQLiteMessage.ExecuteReaderGetInt16Request, (double)3.14);
      var u = new Packet(p.Packed);
      Assert.That(Math.Abs(u.Get<double>() - 3.14) < tolerance);

      p = new Packet(SQLiteMessage.ExecuteReaderGetInt16Request, double.MaxValue);
      u = new Packet(p.Packed);
      Assert.That(Math.Abs(u.Get<double>() - double.MaxValue) < tolerance);

      p = new Packet(SQLiteMessage.ExecuteReaderGetInt16Request, double.MinValue);
      u = new Packet(p.Packed);
      Assert.That(Math.Abs(u.Get<double>() - double.MinValue) < tolerance);
    }

    [Test]
    public void OriginalyIntToLong()
    {
      //  save it as an int, (4 bytes)
      var p = new Packet(SQLiteMessage.ExecuteReaderGetInt16Request, (int)12);
      var u = new Packet(p.Packed);

      // but try and read it as a long, (8 bytes)
      Assert.That(u.Get<long>() == 12);
    }

    [Test]
    public void OriginalyShortToLong()
    {
      //  save it as an int, (4 bytes)
      var p = new Packet(SQLiteMessage.ExecuteReaderGetInt16Request, (short)12);
      var u = new Packet(p.Packed);

      // but try and read it as a long, (8 bytes)
      Assert.That(u.Get<long>() == 12);
    }

    [Test]
    public void OriginalyByteToLong()
    {
      //  save it as an int, (4 bytes)
      var p = new Packet(SQLiteMessage.ExecuteReaderGetInt16Request, new byte[]{12} );
      var u = new Packet(p.Packed);

      // but try and read it as a long, (8 bytes)
      Assert.That(u.Get<long>() == 12);
    }

    [Test]
    public void OriginalyIntToShort()
    {
      //  save it as an int, (4 bytes)
      var p = new Packet(SQLiteMessage.ExecuteReaderGetInt16Request, (int)12);
      var u = new Packet(p.Packed);

      // but try and read it as a long, (8 bytes)
      Assert.That(u.Get<short>() == 12);
    }

    [Test]
    public void OriginalyLongToShort()
    {
      //  save it as an int, (4 bytes)
      var p = new Packet(SQLiteMessage.ExecuteReaderGetInt16Request, (long)12);
      var u = new Packet(p.Packed);

      // but try and read it as a long, (8 bytes)
      Assert.That(u.Get<short>() == 12);
    }

    [Test]
    public void OriginalyByteToShort()
    {
      //  save it as an int, (4 bytes)
      var p = new Packet(SQLiteMessage.ExecuteReaderGetInt16Request, new byte[] { 12 });
      var u = new Packet(p.Packed);

      // but try and read it as a long, (8 bytes)
      Assert.That(u.Get<short>() == 12);
    }

    [Test]
    public void OriginalyShortToInt()
    {
      //  save it as an int, (4 bytes)
      var p = new Packet(SQLiteMessage.ExecuteReaderGetInt16Request, (int)12);
      var u = new Packet(p.Packed);

      // but try and read it as a long, (8 bytes)
      Assert.That(u.Get<short>() == 12);
    }

    [Test]
    public void OriginalyLongToInt()
    {
      //  save it as an int, (4 bytes)
      var p = new Packet(SQLiteMessage.ExecuteReaderGetInt16Request, (long)12);
      var u = new Packet(p.Packed);

      // but try and read it as a long, (8 bytes)
      Assert.That(u.Get<int>() == 12);
    }

    [Test]
    public void OriginalyByteToInt()
    {
      //  save it as an int, (4 bytes)
      var p = new Packet(SQLiteMessage.ExecuteReaderGetInt16Request, new byte[] { 12 });
      var u = new Packet(p.Packed);

      // but try and read it as a long, (8 bytes)
      Assert.That(u.Get<int>() == 12);
    }

    [Test]
    public void OriginalyStringToInt()
    {
      //  save it as an int, (4 bytes)
      var p = new Packet(SQLiteMessage.ExecuteReaderGetInt16Request, "123");
      var u = new Packet(p.Packed);

      // but try and read it as a long, (8 bytes)
      Assert.That(u.Get<int>() == 123);
    }

    [Test]
    public void OriginalyTwoByteStringToInt()
    {
      //  save it as an int, (4 bytes)
      var p = new Packet(SQLiteMessage.ExecuteReaderGetInt16Request, "12");
      var u = new Packet(p.Packed);

      // because this is 2 bytes, the "12" is actually converted to 12849
      // it is up to the caller to know what packet type they are using.
      Assert.AreEqual(u.Get<int>(), 12849 );
      Assert.AreEqual(u.Get<string>(), "12");
    }

    [Test]
    public void OriginalyShortToDouble()
    {
      //  save it as an int, (4 bytes)
      var p = new Packet(SQLiteMessage.ExecuteReaderGetInt16Request, (int)12);
      var u = new Packet(p.Packed);

      Assert.AreEqual(u.Get<double>(), 12);
    }

    [Test]
    [Ignore("This test is deliberately ignored, we cannot test 8 bytes long with 8 bytes double...")]
    public void OriginalyLongToDouble()
    {
      //  save it as an int, (4 bytes)
      var p = new Packet(SQLiteMessage.ExecuteReaderGetInt16Request, (long)12);
      var u = new Packet(p.Packed);

      Assert.AreEqual(u.Get<double>(), 12);
    }

    [Test]
    public void OriginalyByteToDouble()
    {
      //  save it as an int, (4 bytes)
      var p = new Packet(SQLiteMessage.ExecuteReaderGetInt16Request, new byte[] { 12 });
      var u = new Packet(p.Packed);

      Assert.AreEqual(u.Get<double>(), 12);
    }

    [Test]
    public void OriginalyShortToBool()
    {
      //  save it as an int, (4 bytes)
      var p = new Packet(SQLiteMessage.ExecuteReaderGetInt16Request, (short)12);
      var u = new Packet(p.Packed);

      Assert.AreEqual(u.Get<bool>(), true);

      var p2 = new Packet(SQLiteMessage.ExecuteReaderGetInt16Request, (short)0);
      var u2 = new Packet(p2.Packed);

      Assert.AreEqual(u2.Get<bool>(), false);
    }

    [Test]
    public void OriginalyLongToBool()
    {
      var p = new Packet(SQLiteMessage.ExecuteReaderGetInt16Request, (long)12);
      var u = new Packet(p.Packed);

      Assert.AreEqual(u.Get<bool>(), true);

      var p2 = new Packet(SQLiteMessage.ExecuteReaderGetInt16Request, (long) 0);
      var u2 = new Packet(p2.Packed);

      Assert.AreEqual(u2.Get<bool>(), false);
    }

    [Test]
    public void OriginalyByteToBool()
    {
      var p = new Packet(SQLiteMessage.ExecuteReaderGetInt16Request, new byte[] { 12 });
      var u = new Packet(p.Packed);

      Assert.AreEqual(u.Get<bool>(), true);

      var p2 = new Packet(SQLiteMessage.ExecuteReaderGetInt16Request, new byte[] { 0 });
      var u2 = new Packet(p2.Packed);

      Assert.AreEqual(u2.Get<bool>(), false);
    }
  }
}
