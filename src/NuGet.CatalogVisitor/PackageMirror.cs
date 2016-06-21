using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuGet.Protocol.Core.v3;
using NuGet.Protocol.Core.Types;
using NuGet.Common;

namespace NuGet.CatalogVisitor
{
    public class PackageMirror
    {
        public PackageMirror(CatalogVisitorContext catalogContext, string outputSource)
        {

        }

        public async Task MirrorPackages()
        {
            // Read cursor

            // Get packages

            // Push packages
            var source = Repository.Factory.GetCoreV3(@"https://www.myget.org/F/kaswan/api/v3/index.json");
            var pushResource = source.GetResource<PackageUpdateResource>();
            //directory.GetFiles("*.nupkg");
            await pushResource.Push("", "", 100, false, GetAPIKey, NullLogger.Instance);

            // Save cursor
        }

        private static string GetAPIKey(string source)
        {
            // TODO: return api key here
            return string.Empty;
        }
    }
}
