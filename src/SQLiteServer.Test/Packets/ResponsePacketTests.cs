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
using System.Text;
using NUnit.Framework;
using SQLiteServer.Data.Connections;
using SQLiteServer.Data.Enums;

namespace SQLiteServer.Test.Packets
{
  [TestFixture]
  internal class ResponsePacketTests
  {
    [Test]
    public void CreateWithNoGuid()
    {
      var type = SQLiteMessage.CreateCommandRequest;
      var guid = Guid.NewGuid().ToString();
      var bguid = Encoding.ASCII.GetBytes(guid);
      var rp = new PacketResponse(type, bguid);

      Assert.That(rp.Type == type);
      Assert.That(bguid.SequenceEqual(rp.Payload));
    }

    [Test]
    public void CreateWithValidGuid()
    {
      var type = SQLiteMessage.CreateCommandRequest;
      var guid = Guid.NewGuid().ToString();
      var bguid = Encoding.ASCII.GetBytes(guid);
      var rp = new PacketResponse(type, bguid, Guid.NewGuid().ToString() );

      Assert.That(rp.Type == type);
      Assert.That(bguid.SequenceEqual(rp.Payload));
    }

    [Test]
    public void CreateWithInValidGuid()
    {
      var type = SQLiteMessage.CreateCommandRequest;
      var guid = Guid.NewGuid().ToString();
      var bguid = Encoding.ASCII.GetBytes(guid);

      Assert.Throws< Exception>( () =>
      {
        new PacketResponse(type, bguid, "Invalid");
      });
    }

    [Test]
    public void CreateWithNullGuid()
    {
      var type = SQLiteMessage.CreateCommandRequest;
      var guid = Guid.NewGuid().ToString();
      var bguid = Encoding.ASCII.GetBytes(guid);

      Assert.Throws<ArgumentNullException>(() =>
      {
        new PacketResponse(type, bguid, null );
      });
    }

    [Test]
    public void PackAndUnpack()
    {
      var type = SQLiteMessage.CreateCommandRequest;
      var guid = Guid.NewGuid().ToString();
      var bguid = Encoding.ASCII.GetBytes(guid);
      var rp = new PacketResponse(type, bguid);

      var pack = rp.Packed;
      var rp2 = new PacketResponse(pack);

      Assert.That( rp2.Type == type );
      Assert.That(bguid.SequenceEqual(rp2.Payload));
    }
  }
}
