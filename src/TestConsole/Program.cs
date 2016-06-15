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
            /* URI of initial NuGet API page */
            var visitor = HttpCatalogVisitor.CreateHCV(new Uri("https://api.nuget.org/v3/index.json")).Result;

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

            visitor.DownloadIDVersions();


            //var temp = CaseCollisions.FlagAllDiffIDs().Result;

        }
    }
}
