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
            //myUrl.Replace("{commitTimeStamp}", commitTimeStamp.ToString());
            Uri myUri = new Uri(myUrl);
            

            HttpClient client = new HttpClient();

            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, myUri))
                using (Stream contentStream = await (await client.SendAsync(request)).Content.ReadAsStreamAsync(),
                    stream = new FileStream(nupkgPath, FileMode.Create, FileAccess.Write, FileShare.None, 16*1024, true))
                {
                    //using (var response = await client.GetAsync(myUrl))
                    //{
                    //if (response.IsSuccessStatusCode)
                        //if (response.)
                        //{
                        
                            // Remove any existing files
                            if (File.Exists(nupkgPath))
                            {
                                File.Delete(nupkgPath);
                            }
                    await contentStream.CopyToAsync(stream);

                            //using (var outputStream = File.Create(nupkgPath))
                            //using (var stream = await response.Content.ReadAsStreamAsync())
                            //{
                            //    await stream.CopyToAsync(outputStream);
                            //}
                        //}
                       // else
                       // {
                            //Console.WriteLine($"Unable to download: {myUrl}");
                       // }
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
        public async Task<HttpCatalogVisitor> DownloadPackagesDateRange(DateTimeOffset start, DateTimeOffset end, string baseDirectory)
        {
            //context.NoCache = true;
            //context.FeedIndexJsonUrl = "https://api.nuget.org/v3/index.json";
            //FileCursor.Load(_cursor.CursorPath);
            //var fileDate = _cursor.Date;

            HttpCatalogVisitor hcv = new HttpCatalogVisitor(_context);

            var packages = await hcv.GetPackages(start, end);

            //string baseDirectory = _context.CatalogCacheFolder;

            foreach (var package in packages)
            {
                string tempDirectory = baseDirectory + package.Id.Replace(".", "-") + package.Version.ToNormalizedString().Replace(".", "-") + ".nupkg";
                Uri newUri = new Uri(tempDirectory);
                /* Do nothing if it is older than the cursor and exists. */
                if ((start > package.CommitTimeStamp || end <= package.CommitTimeStamp) && File.Exists(tempDirectory))
                {
                    Console.WriteLine($"[CACHE] {tempDirectory}");
                }
                else if (start < package.CommitTimeStamp && end >= package.CommitTimeStamp)
                {
                    Console.WriteLine($"[GET] {newUri.AbsoluteUri}");
                    DownloadPackage(package.Id, package.Version, tempDirectory);
                }
            }

            //_cursor.Save();
            return hcv;
        }
    }
}
