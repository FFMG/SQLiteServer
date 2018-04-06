using System;
using System.IO;
using System.Linq;

namespace SQLiteServerPerformance
{
  class Program
  {
    static void Main(string[] args)
    {
      const string ext = "performance";
      ClearOldFiles(ext);

      const int numberOfTests = 3;
      const int numberOfRows = 50;
      var path = Directory.GetCurrentDirectory();

      for (var i = 0; i < numberOfTests; ++i)
      {
        Console.WriteLine( $"Test Number {i+1}:");
        Console.WriteLine(  "=================");
        // sqlite
        var sqlite = new SQLiteTest(path, ext);
        sqlite.Run(numberOfRows);

        // sqlite server
        var sqliteServer = new SQLiteServerTest(path, ext, false);
        sqliteServer.Run(numberOfRows);

        var sqliteClient = new SQLiteServerTest(path, ext, true);
        sqliteClient.Run(numberOfRows);

        Console.WriteLine("" );
      }

      Console.WriteLine("Press any key to continue...");
      Console.ReadKey();
    }

    private static void ClearOldFiles( string ext)
    {
      var di = new DirectoryInfo(@"C:\");
      var files = di.GetFiles( $"*.{ext}") .Where(p => p.Extension == $".{ext}").ToArray();
      foreach (var file in files)
      {
        try
        {
          file.Attributes = FileAttributes.Normal;
          File.Delete(file.FullName);
        }
        catch
        {
          // ignored
        }
      }
    }
  }
}
