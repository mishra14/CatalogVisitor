using System;
using System.Threading.Tasks;
using NuGet.Protocol.Core.v3;
using NuGet.Protocol.Core.Types;
using NuGet.Common;
using System.IO;

namespace NuGet.CatalogVisitor
{
    public class PackageMirror
    {
        private static CatalogVisitorContext _context = new CatalogVisitorContext();
        private static FileCursor _cursor = new FileCursor("C:\\CatalogCache\\packageMirrorCursor.txt", DateTimeOffset.UtcNow);
        SourceRepository _outputSource = Repository.Factory.GetCoreV3("https://www.myget.org/F/kaswan/api/v3/index.json");

        public PackageMirror(CatalogVisitorContext catalogContext, string outputSource)
        {
            _context = catalogContext;
            _outputSource = Repository.Factory.GetCoreV3(outputSource);
        }

        public async Task<int> MirrorPackages()
        {
            // Read cursor
            _cursor.Load(_cursor.CursorPath);
            var fileDate = _cursor.Date;

            // Get packages
            var myContext = new CatalogVisitorContext();
            myContext.FeedIndexJsonUrl = "https://api.nuget.org/v3/index.json";
            HttpCatalogVisitor hcv = new HttpCatalogVisitor(myContext);
            var packages = hcv.GetPackages(_cursor).Result;

            // Push packages
            var pushResource = _outputSource.GetResource<PackageUpdateResource>();
            int pushed = 0;
            //directory.GetFiles("*.nupkg");
            foreach (var package in packages)
            {
                var packagePath = _context.CatalogCacheFolder + "Mirror-" + package.Id + "-" + package.Version.ToNormalizedString();
                Uri newUri = new Uri(packagePath);
                /* Do nothing if it is older than the cursor and exists. */
                if (fileDate >= package.CommitTimeStamp && File.Exists(packagePath))
                {
                    Console.WriteLine($"[CACHE] {newUri.AbsoluteUri}");
                }
                else
                {
                    Console.WriteLine($"[GET] {newUri.AbsoluteUri}");
                    await pushResource.Push(packagePath, "", 100, false, GetAPIKey, NullLogger.Instance);
                    pushed++;
                }
            }

            // Save cursor
            _cursor.Save();
            return pushed;
        }

        private static string GetAPIKey(string source)
        {
            return "5d5886c5-e666-4d71-aaef-0af559d3d45a";
        }
    }
}
