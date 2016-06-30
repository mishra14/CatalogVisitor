using System;
using System.Threading.Tasks;
using NuGet.Versioning;
using System.IO;
using System.Net.Http;

namespace NuGet.CatalogVisitor
{
    public class HttpPackageDownloader : IPackageDownloader
    {
        private static FileCursor _cursor = new FileCursor("C:\\CatalogCache\\httpPackageDownloaderCursor.txt", DateTimeOffset.MinValue);
        private CatalogVisitorContext _context = new CatalogVisitorContext();

        public HttpPackageDownloader(CatalogVisitorContext context)
        {
            _context = context;
        }

        /// <summary>
        /// API to download single specified nupkg package to specified folder (e).
        /// </summary>
        /// <param name="id"></param>
        /// <param name="version"></param>
        /// <param name="nupkgPath"></param>
        /// <returns></returns>
        public async Task DownloadPackage(string id, NuGetVersion version, string nupkgPath)
        {
            // ex:
            // https://api.nuget.org/v3-flatcontainer/{id-lower}/{version-lower}/{id-lower}.{version-lower}.nupkg
            // https://api.nuget.org/v3-flatcontainer/adam.jsgenerator/1.1.0/adam.jsgenerator.1.1.0.nupkg

            var myUrl = _context.IncomingFeedUrl;
            myUrl = myUrl.Replace("{id}", id.ToLower());
            myUrl = myUrl.Replace("{version}", version.ToNormalizedString().ToLower());
            Uri myUri = new Uri(myUrl);
            

            HttpClient client = new HttpClient();

            try
            {
                // Remove any existing files
                if (File.Exists(nupkgPath))
                {
                    File.Delete(nupkgPath);
                }

                using (var request = new HttpRequestMessage(HttpMethod.Get, myUri))
                using (Stream contentStream = await (await client.SendAsync(request)).Content.ReadAsStreamAsync(),
                    stream = new FileStream(nupkgPath, FileMode.Create, FileAccess.Write, FileShare.None, 16*1024, true))
                {
                    await contentStream.CopyToAsync(stream);
                }
            }
            catch (Exception ex)
            {
                /* BlueprintCSS 1.0.0 url doesn't work */
                //throw new InvalidOperationException($"Failed {myUrl} exception: {ex.ToString()}");
                /* don't download package, don't throw error */
                Console.WriteLine($"Not downloading {myUrl} because it does not exist: \r\n{ex}.");
            }
        }

        /// <summary>
        /// Downloads all packages in date range to directory.
        /// Tues, 6/21 deadline.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public async Task DownloadPackagesDateRange(DateTimeOffset start, DateTimeOffset end, string baseDirectory)
        {
            HttpCatalogVisitor hcv = new HttpCatalogVisitor(_context);

            var packages = await hcv.GetPackages(start, end);
            bool useCache = true;

            foreach (var package in packages)
            {
                string tempDirectory = baseDirectory + package.Id.Replace(".", "-") + package.Version.ToNormalizedString().Replace(".", "-") + ".nupkg";
                useCache = true;
                if (start < package.CommitTimeStamp && end >= package.CommitTimeStamp)
                {
                    useCache = false;
                }

                /* Do nothing if it is older than the cursor and exists. */
                if (useCache)
                {
                    Console.WriteLine($"[CACHE] {tempDirectory}");
                }
                else
                {
                    Console.WriteLine($"[ADDING] {tempDirectory}");
                    await DownloadPackage(package.Id, package.Version, tempDirectory);
                }
            }
        }
    }
}
