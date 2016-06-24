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

        /// <summary>
        /// API to download single specified nupkg package to specified folder (e).
        /// </summary>
        /// <param name="id"></param>
        /// <param name="version"></param>
        /// <param name="downloadDirectory"></param>
        /// <returns></returns>
        public async void DownloadPackage(string id, NuGetVersion version, string downloadDirectory)
        {
            // https://api.nuget.org/v3-flatcontainer/{id-lower}/{version-lower}/{id-lower}.{version-lower}.nupkg


            var myUrl = "https://api.nuget.org/v3-flatcontainer/" + id.ToLower() + "/" + version.ToString().ToLower() + "/" + id.ToLower() + "." + version.ToString().ToLower() + ".nupkg";
            // https://api.nuget.org/v3-flatcontainer/adam.jsgenerator/1.1.0/adam.jsgenerator.1.1.0.nupkg

            HttpClient client = new HttpClient();
            //client.DownloadFile(myUrl, downloadDirectory);

            try
            {
                using (var stream = await client.GetStreamAsync(myUrl))
                using (var outputStream = File.Create(downloadDirectory))
                {
                    await stream.CopyToAsync(outputStream);
                }
            }
            catch (Exception ex)
            {
                /* BlueprintCSS 1.0.0 url doesn't work */
                //throw new InvalidOperationException($"Failed {myUrl} exception: {ex.ToString()}");
                /* don't download package */
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
        public async Task<HttpCatalogVisitor> DownloadPackagesDateRange(DateTimeOffset start, DateTimeOffset end, string downloadDirectory)
        {
            CatalogVisitorContext context = new CatalogVisitorContext();
            context.NoCache = true;
            context.FeedIndexJsonUrl = "https://api.nuget.org/v3/index.json";
            FileCursor.Load(_cursor.CursorPath);
            var fileDate = _cursor.Date;

            HttpCatalogVisitor hcv = new HttpCatalogVisitor(context);

            var packages = await hcv.GetPackages(start, end);

            string baseDirectory = "C:\\CatalogCache\\DownloadPackages\\";

            foreach (var package in packages)
            {
                string tempDirectory = baseDirectory + package.Id.Replace(".", "-") + package.Version.ToString().Replace(".", "-") + ".nupkg";
                Uri newUri = new Uri(tempDirectory);
                /* Do nothing if it is older than the cursor and exists. */
                if (fileDate >= package.CommitTimeStamp && File.Exists(tempDirectory))
                {
                    Console.WriteLine($"[CACHE] {newUri.AbsoluteUri}");
                }
                else
                {
                    Console.WriteLine($"[GET] {newUri.AbsoluteUri}");
                    DownloadPackage(package.Id, package.Version, tempDirectory);
                }
            }

            _cursor.Save();
            return hcv;
        }
    }
}
