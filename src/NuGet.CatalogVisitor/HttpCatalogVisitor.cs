using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet.CatalogVisitor
{
    public class HttpCatalogVisitor : ICatalogVisitor
    {
        public Task<IReadOnlyList<PackageMetadata>> GetPackages()
        {
            throw new NotImplementedException();
        }

        public async Task<IReadOnlyList<PackageMetadata>> GetPackages(FileCursor cursor)
        {
            var packages = await GetPackages(cursor.Date, DateTimeOffset.UtcNow);
            return packages;
        }

        public Task<IReadOnlyList<PackageMetadata>> GetPackages(DateTimeOffset start, DateTimeOffset end)
        {
            //call GetRawPackages and then filter to latest ID version
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<PackageMetadata>> GetPackages(DateTimeOffset start, DateTimeOffset end, string packageIdPattern)
        {
            throw new NotImplementedException();
        }
        
        public Task<IReadOnlyList<PackageMetadata>> GetRawPackages()
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<PackageMetadata>> GetRawPackages(DateTimeOffset start, DateTimeOffset end)
        {
            throw new NotImplementedException();
        }
    }
}
