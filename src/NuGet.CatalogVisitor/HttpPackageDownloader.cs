using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuGet.Versioning;
using System.IO;
using System.Net.Http;

namespace NuGet.CatalogVisitor
{
    public class HttpPackageDownloader : IPackageDownloader
    {
        /// <summary>
        /// API to download single specified nupkg package to specified folder (e).
        /// </summary>
        /// <param name="id"></param>
        /// <param name="version"></param>
        /// <param name="downloadDirectory"></param>
        /// <returns></returns>
        public void DownloadPackage(string id, NuGetVersion version, string downloadDirectory)
        {
            // https://api.nuget.org/v3-flatcontainer/{id-lower}/{version-lower}/{id-lower}.{version-lower}.nupkg


            var myUrl = "https://api.nuget.org/v3-flatcontainer/" + id.ToLower() + "/" + version.ToString().ToLower() + "/" + id.ToLower() + "." + version.ToString().ToLower() + ".nupkg";
            // https://api.nuget.org/v3-flatcontainer/adam.jsgenerator/1.1.0/adam.jsgenerator.1.1.0.nupkg

            System.Net.WebClient client = new System.Net.WebClient();
            client.DownloadFile(myUrl, downloadDirectory);
        }
        
        /// <summary>
        /// Downloads all packages in date range to directory.
        /// Tues, 6/21 deadline.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public async Task<HttpCatalogVisitor> DownloadPackagesDateRange(DateTimeOffset start, DateTimeOffset end)
        {
            CatalogVisitorContext context = new CatalogVisitorContext();
            context.NoCache = true;
            context.FeedIndexJsonUrl = "https://api.nuget.org/v3/index.json";

            HttpCatalogVisitor hcv = new HttpCatalogVisitor(context);

            var packages = await hcv.GetPackages(start, end);

            string baseDirectory = "C:\\CatalogCache\\DownloadPackages\\";

            foreach (var package in packages)
            {
                string tempDirectory = baseDirectory + package.Id.Replace(".", "-") + package.Version.ToString().Replace(".", "-") + ".nupkg";
                DownloadPackage(package.Id, package.Version, tempDirectory);
            }
            return hcv;
        }
    }
}
