using Newtonsoft.Json.Linq;
using NuGet.CatalogVisitor;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

internal class TestCatalogVisitor
{
    private readonly List<PackageMetadata> _list = new List<PackageMetadata>();

    /// <summary>
    /// This class creates its own version of a PackageMetadata list, like
    /// the HttpCatalogVisitor class would, just without visiting any websites.
    /// It tests if the PackageMetadata class is working.
    /// </summary>
    public TestCatalogVisitor()
    {
        _list.Add(new PackageMetadata(new NuGetVersion("1.0"), "myID", DateTimeOffset.UtcNow));

        Assert.Equal(1, _list.Count);
    }

    [Fact]
    public void TestPackageMetadata()
    {
        NuGetVersion version = new NuGetVersion("1.0.0");
        string id = "NuGet.Core";
        DateTimeOffset date = DateTimeOffset.UtcNow;

        PackageMetadata myPackage = new PackageMetadata(version, id, date);

        Assert.Equal(myPackage.Id, id);
        Assert.Equal(myPackage.Version, version);
        Assert.Equal(myPackage.CommitTimeStamp, date);
    }

    [Fact]
    public void TestFileCursor()
    {
        FileCursor cursor = new FileCursor();
        cursor.CursorPath = "C:\\CatalogCache\\testFileCursor.txt";
        DateTimeOffset now = DateTimeOffset.UtcNow;
        cursor.Date = now;

        Assert.Equal(cursor.CursorPath, "C:\\CatalogCache\\testFileCursor.txt");
        Assert.Equal(cursor.Date, now);

        FileCursor newCursor = FileCursor.Load(cursor.CursorPath);
        Assert.Equal(newCursor.CursorPath, cursor.CursorPath);
        var cursorText = File.ReadAllText(newCursor.CursorPath);
        var fileDate = DateTimeOffset.Parse(cursorText);
        Assert.Equal(newCursor.Date, fileDate);

        cursor.Save();
        /* Today and tomorrow. */
        Assert.InRange(cursor.Date, new DateTimeOffset(2016, 6, 17, 0, 0, 0, new TimeSpan(0)), new DateTimeOffset(2016, 6, 18, 0, 0, 0, new TimeSpan(0)));
    }

    [Fact]
    public void TestCatalogVisitorContext()
    {
        string jsonString = "{ nuget:id: Hello, nuget:version: 1.0.0, commitTimeStamp: 2015-02-01T07:00:00.0000000-08:00 }";
        JObject json = JObject.Parse(jsonString);
        PackageMetadata myData = CatalogVisitorContext.GetMetadata(json);

        Assert.Equal(myData.Id, "Hello");
        Assert.Equal(myData.Version, new NuGetVersion("1.0.0"));
        Assert.Equal(myData.CommitTimeStamp, new DateTimeOffset(2015, 2, 1, 7, 0, 0, new TimeSpan(-8, 0, 0)));
    }

    public IEnumerable<PackageMetadata> GetPackages()
    {
        return _list;
    }
}