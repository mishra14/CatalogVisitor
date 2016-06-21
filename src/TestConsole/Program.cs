using NuGet.CatalogVisitor;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestConsole
{
    class Program
    {
        static void Main()
        {
            CatalogVisitorContext context = new CatalogVisitorContext();
            HttpCatalogVisitor visitor = new HttpCatalogVisitor(context);
            HttpPackageDownloader HPD = new HttpPackageDownloader();

            //HPD.DownloadPackage("Passive", new NuGetVersion("0.2.0"), "C:\\CatalogCache\\CurrentHPD.txt");

            /* Doesn't make main async. */
            //var result = HPD.DownloadPackagesDateRange(new DateTimeOffset(2011, 01, 01, 01, 01, 01, new TimeSpan(0)), new DateTimeOffset(2013, 01, 01, 01, 01, 01, new TimeSpan(0))).Result;
            var result = HPD.DownloadPackagesDateRange(DateTimeOffset.MinValue, DateTimeOffset.UtcNow).Result;

            /* Gets all packages
            IReadOnlyList<PackageMetadata> packages = visitor.GetPackages().Result;
            foreach (PackageMetadata package in packages)
            {
                Console.WriteLine("Package - ID: {0}, Version: {1}", package.Id, package.Version);
            }
            */

            /* Gets latest version for each ID from date in cursor to now.
            FileCursor cursor = new FileCursor();
            cursor.Date = new DateTimeOffset(2015, 2, 1, 7, 0, 0, new TimeSpan(-8, 0, 0)); // from that date to now
            cursor.CursorPath = "C:\\CatalogCache\\mainCursor.txt";
            IReadOnlyList<PackageMetadata> packages = visitor.GetPackages(cursor).Result;
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
