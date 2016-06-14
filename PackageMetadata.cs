using NuGet.Versioning;
using System;

namespace NuGet.CatalogVisitor
{
    /// <summary>
    /// A class used to store data for ICatalogVisitor types.
    /// Can add functionality, store additional items and do
    /// additional calculations as needed.
    /// </summary>
    public class PackageMetadata
    {
        public PackageMetadata(NuGetVersion version, string id, DateTimeOffset commitTimeStamp)
        {
            Version = version;
            Id = id;
            CommitTimeStamp = commitTimeStamp;
        }

        public NuGetVersion Version { get; }

        public string Id { get; }

        public DateTimeOffset CommitTimeStamp { get; }
    }
}