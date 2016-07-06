using NuGet.CatalogVisitor;
using System;
using System.Threading.Tasks;

namespace FeedMirror
{
    /// <summary>
    /// An exe that I can give to someone else 
    /// that mirrors packages from one feed to another.
    /// </summary>
    class PackageFeedMirror
    {
        static void Main(string[] args)
        {
            Run(args).Wait();
        }

        private static async Task Run(string[] args)
        {
            try
            {
                var feed = args[0];
                var output = args[1];

                CatalogVisitorContext context = new CatalogVisitorContext();
                context.CatalogCacheFolder = "C:\\CatalogCache\\MirrorPackages\\";

                /* Hard-coded examples in case you want to run in debug/not exe or command line. */
                //context.IncomingFeedUrl = "https://api.nuget.org/v3-flatcontainer/{id}/{version}/{id}.{version}.nupkg";
                //string mySource = "https://www.myget.org/F/theotherfeed/api/v3/index.json";

                string mySource = output;
                context.IncomingFeedUrl = feed;
                context.FeedIndexJsonUrl = "https://api.nuget.org/v3/index.json";
                FileCursor cursor = new FileCursor();
                cursor.Date = new DateTimeOffset(2016, 7, 5, 10, 5, 0, new TimeSpan(-7, 0, 0));
                cursor.CursorPath = "C:\\CatalogCache\\mainMirrorCursor.txt";
                Console.WriteLine($"Mirroring packages from {context.FeedIndexJsonUrl} between {cursor.Date.ToLocalTime()} and {DateTimeOffset.UtcNow.ToLocalTime()}.");

                PackageMirror myPM = new PackageMirror(context, mySource);

                var pushed = await myPM.MirrorPackages(cursor.Date, DateTimeOffset.UtcNow);

                cursor.Save();
                Console.WriteLine($"{pushed} pushed.");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex);
            }
        }
    }
}
