using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NuGet.CatalogVisitor;


public interface ICatalogVisitor
{
    /// <summary>
    /// Returns a list of PackageMetadata that holds the
    /// current ID and Version of the Catalog of Packages.
    /// </summary>
    /// <returns></returns>
    Task<IReadOnlyList<PackageMetadata>> GetPackages();

    /// <summary>
    /// Returns a list of PackageMetadata that holds the
    /// current ID and Version of the Catalog of Packages
    /// between two dates.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    Task<IReadOnlyList<PackageMetadata>> GetPackages(DateTimeOffset start, DateTimeOffset end);

    /// <summary>
    /// Returns a list of PackageMetadata that holds the
    /// current ID and Version of the Catalog of Packages
    /// between cursor date and now.
    /// </summary>
    /// <param name="cursor"></param>
    /// <returns></returns>
    Task<IReadOnlyList<PackageMetadata>> GetPackages(ICursor cursor);

    /// <summary>
    /// Returns latest entry for each Id and Version in the range.
    /// that matches the packageIdPattern.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="packageIdPattern"></param>
    /// <returns></returns>
    Task<IReadOnlyList<PackageMetadata>> GetPackages(DateTimeOffset start, DateTimeOffset end, string packageIdPattern);

    /// <summary>
    /// Returns all catalog entries including duplicates.
    /// </summary>
    /// <returns></returns>
    Task<IReadOnlyList<PackageMetadata>> GetRawPackages();

    /// <summary>
    /// Returns all catalog entries including duplicates
    /// between two dates.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    Task<IReadOnlyList<PackageMetadata>> GetRawPackages(DateTimeOffset start, DateTimeOffset end);
}
