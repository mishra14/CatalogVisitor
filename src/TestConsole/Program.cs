using FeedMirror;
using NuGet.CatalogVisitor;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Run(args).Wait();
        }

        private static async Task Run(string[] args)
        {
            var feed = args[0];
            var output = args[1];

            CatalogVisitorContext context = new CatalogVisitorContext();
            context.CatalogCacheFolder = "C:\\CatalogCache\\MirrorPackages\\";
            context.IncomingFeedUrl = "https://api.nuget.org/v3-flatcontainer/{id}/{version}/{id}.{version}.nupkg";
            string mySource = "https://www.myget.org/F/theotherfeed/api/v3/index.json";
            //context.IncomingFeedUrl = feed;
            //string mySource = output;
            context.FeedIndexJsonUrl = "https://api.nuget.org/v3/index.json";
            FileCursor cursor = new FileCursor();
            cursor.Date = new DateTimeOffset(2016, 7, 5, 10, 5, 0, new TimeSpan(-7, 0, 0));
            cursor.CursorPath = "C:\\CatalogCache\\mainMirrorCursor.txt";

            PackageMirror myPM = new PackageMirror(context, mySource);

            var pushed = await myPM.MirrorPackages(cursor.Date, DateTimeOffset.UtcNow);

            cursor.Save();


            /*
            CatalogVisitorContext context = new CatalogVisitorContext();
            context.IncomingFeedUrl = "https://api.nuget.org/v3-flatcontainer/{id}/{version}/{id}.{version}.nupkg";
            context.FeedIndexJsonUrl = "https://api.nuget.org/v3/index.json";
            context.CatalogCacheFolder = "C:\\CatalogCache\\";
            //FileCursor cursor = FileCursor.Load("C:\\CatalogCache\\httpPackageDownloaderCursor.txt");
            HttpCatalogVisitor visitor = new HttpCatalogVisitor(context);
            HttpPackageDownloader HPD = new HttpPackageDownloader(context);

            /* Gets latest version for each ID from date in cursor to now.
            FileCursor cursor = new FileCursor();
            cursor.Date = DateTimeOffset.UtcNow;
                //new DateTimeOffset(2016, 7, 1, 10, 0, 0, new TimeSpan(0, 0, 0)); // from today, last half hour, to now
            cursor.CursorPath = "C:\\CatalogCache\\mainCursor.txt";
            IReadOnlyList<PackageMetadata> packages = await visitor.GetPackages(cursor);
            foreach (PackageMetadata package in packages)
            {
                Console.WriteLine("Package - ID: {0}, Version: {1}", package.Id, package.Version);
            }
            cursor.Save();
            */



            //HPD.DownloadPackage("Passive", new NuGetVersion("0.2.0"), "C:\\CatalogCache\\CurrentHPD.nupkg");

            /* Doesn't make main async. */
            /*
            var result = await HPD.DownloadPackagesDateRange(new DateTimeOffset(2015, 2, 1, 14, 0, 0, new TimeSpan(0)), new DateTimeOffset(2015, 2, 1, 15, 1, 5, new TimeSpan(0)), @"c:\catalogcache\downloads");
            cursor.Date = new DateTimeOffset(2015, 2, 1, 15, 1, 5, new TimeSpan(0));
            cursor.Save();
            */

            //var result = HPD.DownloadPackagesDateRange(DateTimeOffset.MinValue, DateTimeOffset.UtcNow).Result;


            /* Gets all packages
            IReadOnlyList<PackageMetadata> packages = visitor.GetPackages().Result;
            foreach (PackageMetadata package in packages)
            {
                Console.WriteLine("Package - ID: {0}, Version: {1}", package.Id, package.Version);
            }
            */



            /*
             IEnumerable<PackageMetadata> myData = visitor.GetPackages();
             foreach (PackageMetadata dat in myData)
             {
             Console.WriteLine("ID: {0}, Version: {1}", dat.Id, dat.Version);
             }
            */
            /*
            string[] ids = new string[2] { "Altairis.MailToolkit", "Argotic.Common" };
            var versions = visitor.GetIDSPackages(ids);
            foreach (List<NuGetVersion> version in versions)
            {
                foreach (NuGetVersion vers in version)
                {
                    Console.WriteLine("Version: {0}", vers);
                }
            }
            */

            /*
            var idVersions = visitor.GetNewIDVersions();
            foreach (Tuple<string, NuGetVersion> idVersion in idVersions)
            {
                Console.WriteLine("New ID/Version Pair - ID: {0}, Version: {1}", idVersion.Item1, idVersion.Item2);
            }
            */


            //var temp = CaseCollisions.FlagAllDiffIDs().Result;

        }
    }
}
