using NuGet.CatalogVisitor;
using System;
using System.Threading.Tasks;

namespace FeedMirror
{
    /// <summary>
    /// Create an exe that you can give to someone else 
    /// that mirrors packages from one feed to another.
    /// Make what you have in your API usable now :)
    /// </summary>
    class PackageFeedMirror
    {
        static void Main()
        {
            Run().Wait();
        }

        private static async Task Run()
        {
            try
            {
                /* This url is where user would set their own feed source to get the packages from. */
                CatalogVisitorContext context = new CatalogVisitorContext("https://api.nuget.org/v3/index.json");
                context.CatalogCacheFolder = "C:\\CatalogCache\\MirrorPackages\\";
                context.IncomingFeedUrl = "https://api.nuget.org/v3-flatcontainer/{id}/{version}/{id}.{version}.nupkg";
                string mySource = "https://www.myget.org/F/theotherfeed/api/v3/index.json";

                var cursor = FileCursor.Load("C:\\CatalogCache\\packageMirrorCursor.txt");
                //var endTime = new DateTimeOffset(2013, 7, 29, 1, 1, 1, new TimeSpan(0));
                var endTime = DateTimeOffset.UtcNow;

                Console.WriteLine($"Mirroring packages from {context.FeedIndexJsonUrl} between {cursor.Date.ToLocalTime()} and {endTime.ToLocalTime()}.");

                PackageMirror myPM = new PackageMirror(context, mySource);

                var pushed = await myPM.MirrorPackages(cursor.Date, endTime);

                // Once all packages are updated it is safe to update the cursor to the original end time.
                cursor.Date = endTime;
                cursor.Save();

                Console.WriteLine($"{pushed} pushed.");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex);
                Console.Clear();
            }
        }
    }
}
