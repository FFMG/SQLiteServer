using System;
using System.Data.SQLite;
using NUnit.Framework;
using SQLiteServer.Data.SQLiteServer;

namespace SQLiteServer.Test.SQLiteServer
{
  internal class ConnectionStringBuilderTests
  {
    [Test]
    public void CreateWithNoArgument()
    {
      Assert.That( () => new SQLiteServerConnectionStringBuilder(), Throws.Nothing );
    }

    [Test]
    public void CreateWithDataSourceMemoryArgument()
    {
      var sql = new SQLiteServerConnectionStringBuilder("Data Source=:memory:");
      Assert.AreEqual(":memory:", sql["Data Source"]);
      Assert.AreEqual(":memory:", sql.DataSource);
    }

    [Test]
    public void DefaultDataSource()
    {
      var sql = new SQLiteServerConnectionStringBuilder();
      Assert.AreEqual("", sql.DataSource);
    }

    [Test]
    public void SetDataSource()
    {
      var sql = new SQLiteServerConnectionStringBuilder
      {
        DataSource = "Blah"
      };
      Assert.AreEqual("Blah", sql.DataSource);
    }

    [Test]
    public void DefaultUri()
    {
      var sql = new SQLiteServerConnectionStringBuilder();
      Assert.AreEqual(null, sql.Uri);
    }

    [Test]
    public void SetUri()
    {
      var sql = new SQLiteServerConnectionStringBuilder
      {
        Uri = "Bla"
      };
      Assert.AreEqual("Bla", sql.Uri);
    }

    [Test]
    public void GivenUriInConnectionString()
    {
      var sql = new SQLiteServerConnectionStringBuilder( "Uri=Bla" );
      Assert.AreEqual("Bla", sql.Uri);
    }

    [Test]
    public void DefaultFullUri()
    {
      var sql = new SQLiteServerConnectionStringBuilder();
      Assert.AreEqual(null, sql.FullUri);
    }

    [Test]
    public void SetFullUri()
    {
      var sql = new SQLiteServerConnectionStringBuilder
      {
        FullUri = "Bla"
      };
      Assert.AreEqual("Bla", sql.FullUri);
    }

    [Test]
    public void GivenFullUriInConnectionString()
    {
      var sql = new SQLiteServerConnectionStringBuilder("FullUri=Bla");
      Assert.AreEqual("Bla", sql.FullUri);
    }

    [Test]
    public void DefaultVersion()
    {
      var sql = new SQLiteServerConnectionStringBuilder();
      Assert.AreEqual(3, sql.Version);
    }

    [Test]
    public void SetInvalidVersion()
    {
      var sql = new SQLiteServerConnectionStringBuilder();
      Assert.Throws<NotSupportedException>( () =>
      {
        sql.Version = 2;
      });
    }

    [Test]
    public void InvalidVersionGivenByConnectionString()
    {
      var sql = new SQLiteServerConnectionStringBuilder("Version=2");
      Assert.AreEqual(2, sql.Version);
    }

    [Test]
    public void DefaultSynchronousMode()
    {
      var sql = new SQLiteServerConnectionStringBuilder();
      Assert.AreEqual(SynchronizationModes.Normal, sql.SyncMode);
    }

    [Test]
    public void GivenSynchronousMode()
    {
      var sql = new SQLiteServerConnectionStringBuilder("Synchronous=Full");
      Assert.AreEqual(SynchronizationModes.Full, sql.SyncMode);
    }

    [Test]
    public void CanChangeValueOfSynchronousMode()
    {
      var sql = new SQLiteServerConnectionStringBuilder("Synchronous=Full");
      Assert.AreEqual(SynchronizationModes.Full, sql.SyncMode);
      sql.SyncMode = SynchronizationModes.Off;
      Assert.AreEqual(SynchronizationModes.Off, sql.SyncMode);
    }

    [Test]
    public void InvalidSynchronousModeIsDefault()
    {
      var sql = new SQLiteServerConnectionStringBuilder("Synchronous=BlahBlah");
      Assert.AreEqual(SynchronizationModes.Normal, sql.SyncMode);
    }

    [Test]
    public void UseUTF16EncodingDefaultValue()
    {
      var sql = new SQLiteServerConnectionStringBuilder();
      Assert.IsFalse(sql.UseUTF16Encoding);
    }

    [Test]
    public void GivenUseUTF16EncodingAsAString()
    {
      var sql1 = new SQLiteServerConnectionStringBuilder("useutf16encoding=false");
      Assert.IsFalse(sql1.UseUTF16Encoding);

      var sql2 = new SQLiteServerConnectionStringBuilder("useutf16encoding=true");
      Assert.IsTrue(sql2.UseUTF16Encoding);
    }

    [Test]
    public void SetUseUTF16EncodingAsAString()
    {
      var sql1 = new SQLiteServerConnectionStringBuilder("useutf16encoding=false");
      Assert.IsFalse(sql1.UseUTF16Encoding);
      sql1.UseUTF16Encoding = true;
      Assert.IsTrue(sql1.UseUTF16Encoding);

      var sql2 = new SQLiteServerConnectionStringBuilder("useutf16encoding=true");
      Assert.IsTrue(sql2.UseUTF16Encoding);
      sql2.UseUTF16Encoding = false;
      Assert.IsFalse(sql2.UseUTF16Encoding);
    }

    [Test]
    public void GivenUseUTF16EncodingAsANumber()
    {
      var sql1 = new SQLiteServerConnectionStringBuilder("useutf16encoding=0");
      Assert.IsFalse(sql1.UseUTF16Encoding);

      var sql2 = new SQLiteServerConnectionStringBuilder("useutf16encoding=1");
      Assert.IsTrue(sql2.UseUTF16Encoding);

      var sql3 = new SQLiteServerConnectionStringBuilder("useutf16encoding=1234");
      Assert.IsTrue(sql3.UseUTF16Encoding);
    }

    [Test]
    public void UPoolingDefaultValue()
    {
      var sql = new SQLiteServerConnectionStringBuilder();
      Assert.IsFalse(sql.Pooling);
    }

    [Test]
    public void GivenPoolingAsAString()
    {
      var sql1 = new SQLiteServerConnectionStringBuilder("pooling=false");
      Assert.IsFalse(sql1.Pooling);

      var sql2 = new SQLiteServerConnectionStringBuilder("Pooling=true");
      Assert.IsTrue(sql2.Pooling);
    }

    [Test]
    public void SetPooling()
    {
      var sql1 = new SQLiteServerConnectionStringBuilder("pooling=false");
      Assert.IsFalse(sql1.Pooling);
      sql1.Pooling = true;
      Assert.IsTrue(sql1.Pooling);

      var sql2 = new SQLiteServerConnectionStringBuilder("Pooling=true");
      Assert.IsTrue(sql2.Pooling);
      sql2.Pooling = false;
      Assert.IsFalse(sql2.Pooling);
    }

    [Test]
    public void GivenPoolingAsANumber()
    {
      var sql1 = new SQLiteServerConnectionStringBuilder("Pooling=0");
      Assert.IsFalse(sql1.Pooling);

      var sql2 = new SQLiteServerConnectionStringBuilder("pooling=1");
      Assert.IsTrue(sql2.Pooling);

      var sql3 = new SQLiteServerConnectionStringBuilder("pooling=1234");
      Assert.IsTrue(sql3.Pooling);
    }

    [Test]
    public void GetTheDefaultTimeOutDefaultValue()
    {
      var sql = new SQLiteServerConnectionStringBuilder();
      Assert.AreEqual( 30, sql.DefaultTimeout);
    }

    [Test]
    public void GetTheDefaultTimeOutGivenInConnectionString()
    {
      var sql = new SQLiteServerConnectionStringBuilder("Default Timeout=60");
      Assert.AreEqual(60, sql.DefaultTimeout);
    }

    [Test]
    public void SetTheDefaultTimeOutGivenInConnectionString()
    {
      var sql = new SQLiteServerConnectionStringBuilder("Default Timeout=60")
      {
        DefaultTimeout = 40
      };
      Assert.AreEqual(40, sql.DefaultTimeout);
    }

    [Test]
    public void GetTheBusyTimeOutDefaultValue()
    {
      var sql = new SQLiteServerConnectionStringBuilder();
      Assert.AreEqual(0, sql.BusyTimeout);
    }

    [Test]
    public void GetTheBusyTimeOutGivenInConnectionString()
    {
      var sql = new SQLiteServerConnectionStringBuilder("BusyTimeout=60");
      Assert.AreEqual(60, sql.BusyTimeout);
    }

    [Test]
    public void SetTheBusyTimeOutGivenInConnectionString()
    {
      var sql = new SQLiteServerConnectionStringBuilder("BusyTimeout=60")
      {
        BusyTimeout = 40
      };
      Assert.AreEqual(40, sql.BusyTimeout);
    }

    [Test]
    public void IsNotReadOnlyByDefault()
    {
      var sql = new SQLiteServerConnectionStringBuilder();
      Assert.IsFalse(sql.ReadOnly);
    }

    [Test]
    public void SetIsReadOnly()
    {
      var sql = new SQLiteServerConnectionStringBuilder {ReadOnly = true};
      Assert.IsTrue(sql.ReadOnly);
    }

    [Test]
    public void ReadOnlyGivenAsAString()
    {
      var sql1 = new SQLiteServerConnectionStringBuilder( "Read Only=true");
      Assert.IsTrue(sql1.ReadOnly);

      var sql2 = new SQLiteServerConnectionStringBuilder("Read Only=false");
      Assert.IsFalse(sql2.ReadOnly);
    }

    [Test]
    public void ReadOnlyGivenAsANumber()
    {
      var sql1 = new SQLiteServerConnectionStringBuilder("Read Only=0");
      Assert.IsFalse(sql1.ReadOnly);

      var sql2 = new SQLiteServerConnectionStringBuilder("Read Only=1");
      Assert.IsTrue(sql2.ReadOnly);
    }
  }
}
