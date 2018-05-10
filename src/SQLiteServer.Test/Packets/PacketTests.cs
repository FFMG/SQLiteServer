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
        // ReSharper disable once ObjectCreationAsStatement
        () => new Packet(new byte[] {0, 1})
      );
    }

    [Test]
    public void PayloadIsNotBigEnoughToGiveAType()
    {
      //  we need 4 bytes for size.
      Assert.Throws<ArgumentOutOfRangeException>(
        // ReSharper disable once ObjectCreationAsStatement
        () => new Packet(new byte[] { 1, 0, 0, 0, 2, 0 })
      );
    }

    [Test]
    public void PayloadDoesNotHaveEnoughForData()
    {
      //  we need 4 bytes for size.
      Assert.Throws<ArgumentOutOfRangeException>(
        // ReSharper disable once ObjectCreationAsStatement
        () => new Packet(new byte[] { 1, 0, 0, 0, 2, 0, 0, 0 })
      );
    }

    [Test]
    public void PayloadIsTooSmall()
    {
      //  we need 4 bytes for size.
      Assert.Throws<ArgumentOutOfRangeException>(
        // ReSharper disable once ObjectCreationAsStatement
        () => new Packet(new byte[] { 1, 0, 0, 0, 2, 0, 0, 0, 0, 0 })
      );
    }

    [Test]
    public void PayloadIsTooBig()
    {
      //  we need 4 bytes for size.
      Assert.Throws<ArgumentOutOfRangeException>(
        // ReSharper disable once ObjectCreationAsStatement
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
  }
}
