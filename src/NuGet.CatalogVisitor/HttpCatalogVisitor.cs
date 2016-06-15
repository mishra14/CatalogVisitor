using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace NuGet.CatalogVisitor
{
    public class HttpCatalogVisitor : ICatalogVisitor
    {
        private CatalogVisitorContext _context;
        private HttpClient _client;

        public HttpCatalogVisitor(CatalogVisitorContext context)
        {
            _context = context;

            var handler = context.MessageHandler ?? new HttpClientHandler();

            _client = new HttpClient(handler);
        }

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
            // implement this first
            throw new NotImplementedException();
        }

        private Task<string> GetCatalogIndexUri()
        {
            throw new NotImplementedException();
        }

        private async Task<JObject> GetJson(string url)
        {
            throw new NotImplementedException();
        }
    }
}
