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

namespace SQLiteServer.Test.Packets
{
  [TestFixture]
  internal class PacketsTests
  {
    [Test]
    public void EmptyQueueStillReturnsANonNullList()
    {
      var p = new Data.Connections.Packets();
      Assert.That( !p.UnQueue().Any() );
    }

    [Test]
    public void TryingToQueueMoreItemsThanInBuffer()
    {
      var b = new byte[] { 0, 1 };
      var p = new Data.Connections.Packets();
      Assert.Throws<ArgumentOutOfRangeException>( () =>
      {
        p.Queue(b, 5);
      });
    }

    [Test]
    public void QueueDataInSequences()
    {
      var b1 = new byte[] { 5, 0, 0, 0, 1, 0, 0, 0 };  //  len = 5 and type = 1
      var b2 = new byte[] { 0, 0, 0, 0, 1 };  //  the data itself.
      var p = new Data.Connections.Packets(); 
      p.Queue(b1, b1.Length );
      // we have nothing
      Assert.That(!p.UnQueue().Any());

      p.Queue(b2, b2.Length);
      Assert.That(p.UnQueue().Count() == 1 );
    }

    [Test]
    public void QueuePartialDataInSequences()
    {
      var b1 = new byte[] { 5, 0, 0, 0, 1, 0, 0, 0 };  //  len = 5 and type = 1
      var b2 = new byte[] { 0, 0, 0, 0 };  //  the data itself.
      var b3 = new byte[] { 1 };  //  the data itself.
      var p = new Data.Connections.Packets();
      p.Queue(b1, b1.Length);
      // we have nothing
      Assert.That(!p.UnQueue().Any());

      p.Queue(b2, b2.Length);
      p.Queue(b3, b3.Length);
      Assert.That(p.UnQueue().Count() == 1);
    }

    [Test]
    public void MultiplePartialDataInSequences()
    {
      var b1 = new byte[] { 5, 0, 0, 0, 1};
      var b2 = new byte[] { 0, 0, 0, 0, 0, 0};
      var b3 = new byte[] { 0, 1 };             //  the data itself.
      var b4 = new byte[] { 5, 0, 0, 0, 1, 0};
      var b5 = new byte[] { 0, 0, 0, 0, 0, 0, 1 };
      var p = new Data.Connections.Packets();
      p.Queue(b1, b1.Length);
      p.Queue(b2, b2.Length);
      p.Queue(b3, b3.Length);
      p.Queue(b4, b4.Length);
      p.Queue(b5, b5.Length);
      Assert.That(p.UnQueue().Count() == 2);
    }

    [Test]
    public void MultipleFullPartQueue()
    {
      var b1 = new byte[] { 5, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1 };             //  the data itself.
      var b2 = new byte[] { 5, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1 };             //  the data itself.
      var p = new Data.Connections.Packets();
      p.Queue(b1, b1.Length);
      p.Queue(b2, b2.Length);
      Assert.That(p.UnQueue().Count() == 2);
    }

    [Test]
    public void MultiplePacketsAtOnce()
    {
      var b = new byte[] { 5, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 5, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1 };             //  the data itself.
      var p = new Data.Connections.Packets();
      p.Queue(b, b.Length);
      Assert.That(p.UnQueue().Count() == 2);
    }

    [Test]
    public void OnByteAtATime()
    {
      var p = new Data.Connections.Packets();
      var bs = new byte[] { 5, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 5, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1 };             //  the data itself.
      foreach (var b in bs)
      {
        p.Queue( new [] {b}, 1);
      }
      Assert.That(p.UnQueue().Count() == 2);
    }

    [Test]
    public void AfterUnqueueItemIsRemoved()
    {
      var p = new Data.Connections.Packets();
      var b = new byte[] { 4, 0, 0, 0,
        1, 0, 0, 0,
        3, 0, 0, 0,
        4, 0, 0, 0,
        2, 0, 0, 0,
        6, 0, 0, 0 };
      p.Queue(b, b.Length);
      Assert.That(p.UnQueue().Count() == 2);
      Assert.That( !p.UnQueue().Any());
    }
  }
}
