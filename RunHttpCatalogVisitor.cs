using NuGet.Versioning;
using System;
using System.Collections.Generic;
using NuGet.CatalogVisitor;

namespace NuGet.CatalogVisitor
{
    public class RunHttpCatalogVisitor : ICatalogVisitor
    {
        public RunHttpCatalogVisitor()
        {

        }

        public IEnumerable<PackageMetadata> GetPackages()
        {
            return null;
        }
    }
}