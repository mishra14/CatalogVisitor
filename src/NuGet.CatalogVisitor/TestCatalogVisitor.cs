using NuGet.CatalogVisitor;
using NuGet.Versioning;
using System;
using System.Collections.Generic;

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
    }
    public IEnumerable<PackageMetadata> GetPackages()
    {
        return _list;
    }
}