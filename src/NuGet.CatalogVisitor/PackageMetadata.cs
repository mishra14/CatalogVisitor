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
        /// <summary>
        /// Constructor with all three private member vars
        /// for convenience.
        /// </summary>
        /// <param name="version"></param>
        /// <param name="id"></param>
        /// <param name="commitTimeStamp"></param>
        public PackageMetadata(NuGetVersion version, string id, DateTimeOffset commitTimeStamp)
        {
            Version = version;
            Id = id;
            CommitTimeStamp = commitTimeStamp;
        }

        public NuGetVersion Version { get; }

        public string Id { get; }

        public DateTimeOffset CommitTimeStamp { get; }

        /// <summary>
        /// Make it easier to read which PackageMetadata we are looking at.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{Id} {Version}";
        }
    }
}