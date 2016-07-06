using System;
using System.Threading.Tasks;
using NuGet.Protocol.Core.v3;
using NuGet.Protocol.Core.Types;
using NuGet.Common;
using System.IO;
using System.Net.Http;
using NuGet.CatalogVisitor;

namespace FeedMirror
{
    public class PackageMirror
    {
        /* Default feed. */
        private CatalogVisitorContext _context;
        private SourceRepository _outputSource;

        /// <summary>
        /// User passes in their feed with catalogContext.IncomingFeedUrl,
        /// user passes in feed they want to push to (myGet feed for example)
        /// as a string (url?) as outputSource.
        /// 
        /// Mirrors packages from input to output.
        /// </summary>
        /// <param name="catalogContext"></param>
        /// <param name="outputSource"></param>
        public PackageMirror(CatalogVisitorContext catalogContext, string outputSource)
        {
            _context = catalogContext;
            _outputSource = Repository.Factory.GetCoreV3(outputSource);
        }

        /// <summary>
        /// The function that gets all packages using context user passes in,
        /// and for each package, tries to open up the incomingFeedUrl and get the contents,
        /// then push the contents to the feed specified.
        /// If there is an error, it moves on to the next package, the error usually means
        /// the package doesn't exist so we don't want to push it or stop the program.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public async Task<int> MirrorPackages(DateTimeOffset start, DateTimeOffset end)
        {
            // Get packages
            HttpCatalogVisitor hcv = new HttpCatalogVisitor(_context);
            var packages = await hcv.GetPackages(start, end);

            Console.WriteLine($"Found {packages.Count} packages.");
            Console.WriteLine($"Pushing");

            // Push packages
            var pushResource = _outputSource.GetResource<PackageUpdateResource>();
            int pushed = 0;


            foreach (var package in packages)
            {
                /* Not hard coded: added onto end of context.CatalogCache... */
                var packagePath = _context.CatalogCacheFolder + "Mirror-" + package.Id + "-" + package.Version.ToNormalizedString() + ".nupkg";
                /* Do nothing if it is older than the cursor and exists. */
                if ((start > package.CommitTimeStamp || end <= package.CommitTimeStamp) && File.Exists(packagePath))
                {
                    Console.WriteLine($"[CACHE] {packagePath}");
                }
                else if (start < package.CommitTimeStamp && end >= package.CommitTimeStamp)
                {
                    Console.WriteLine($"[GET] {packagePath}");
                    HttpClient client = new HttpClient();
                    var myUrl = _context.IncomingFeedUrl;
                    myUrl = myUrl.Replace("{id}", package.Id.ToLower());
                    myUrl = myUrl.Replace("{version}", package.Version.ToNormalizedString().ToLower());
                    myUrl = myUrl.Replace("{commitTimeStamp}", package.CommitTimeStamp.ToString());
                    try
                    {
                        using (var stream = await client.GetStreamAsync(myUrl))
                        using (var outputStream = File.Create(packagePath))
                        {
                            await stream.CopyToAsync(outputStream);
                        }

                        await pushResource.Push(packagePath, "", 500, false, GetAPIKey, NullLogger.Instance);
                        pushed++;

                        // Clean up
                        File.Delete(packagePath);
                    }
                    catch (Exception ex)
                    {
                        /* BlueprintCSS 1.0.0 url doesn't work */
                        //throw new InvalidOperationException($"Failed {myUrl} exception: {ex.ToString()}");
                        /* don't download package */
                        Console.WriteLine($"Not downloading {myUrl} because it does not exist: \r\n{ex}.");
                        //throw;
                        /* Move onto next var in loop, don't throw to catch in main, etc. */
                        continue;
                    }
                }
            }

            return pushed;
        }

        /// <summary>
        /// How do I hide this key? Do I need to pass in this key since it changes based on feed passed in?
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private static string GetAPIKey(string source)
        {
            return "5d5886c5-e666-4d71-aaef-0af559d3d45a";
        }
    }
}
