using System.Collections.Generic;
using NuGet.CatalogVisitor;


public interface ICatalogVisitor
{
    /// <summary>
    /// Returns a list of PackageMetadata that holds the
    /// current ID and Version of the Catalog of Packages.
    /// </summary>
    /// <returns></returns>
    IEnumerable<PackageMetadata> GetPackages();
}
