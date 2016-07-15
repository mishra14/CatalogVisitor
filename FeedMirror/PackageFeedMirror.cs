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
                /* Hard-coded examples for args in case none were entered or you want to run in debug/not exe or command line. */
                var feed = "https://api.nuget.org/v3-flatcontainer/{id}/{version}/{id}.{version}.nupkg";
                var output = "https://www.myget.org/F/theotherfeed/api/v3/index.json";
                var fileName = "*";
                var version = "*";
                var givenCursor = "C:\\CatalogCache\\mainMirrorCursor.txt";

                /* Reading in the args based on different lengths. */
                if (args.Length == 2)
                {
                    feed = args[0];
                    output = args[1];
                }
                if (args.Length <= 3 && args.Length >= 2)
                {
                    fileName = args[2];
                }
                if (args.Length <= 4 && args.Length >= 2)
                {
                    version = args[3];
                }
                if (args.Length <= 5 && args.Length >= 2)
                {
                    givenCursor = args[4];
                }

                /* If no args, use hardcoded values. */
                if (args.Length == 0)
                {

                    CatalogVisitorContext context = new CatalogVisitorContext();
                    context.CatalogCacheFolder = "C:\\CatalogCache\\MirrorPackages\\";
                    FileCursor cursor = new FileCursor();

                    /* Set to original or read in values. */
                    context.IncomingFeedUrl = feed;
                    string mySource = output;
                    var verGlobPattern = fileName;
                    var idGlobPattern = version;
                    cursor.CursorPath = givenCursor;

                    /* cursor.Date now has correct date */
                    FileCursor.Load(cursor.CursorPath);
                    context.FeedIndexJsonUrl = "https://api.nuget.org/v3/index.json";
                    //cursor.Date = new DateTimeOffset(2016, 7, 6, 9, 53, 30, new TimeSpan(-7, 0, 0));
                    Console.WriteLine($"Mirroring packages from {context.FeedIndexJsonUrl} between {cursor.Date.ToLocalTime()} and {DateTimeOffset.UtcNow.ToLocalTime()}.");

                    PackageMirror myPM = new PackageMirror(context, mySource);

                    /* Use version with id and version globbing. */
                    var pushed = await myPM.MirrorPackages(cursor.Date, DateTimeOffset.UtcNow, fileName, version);

                    cursor.Save();
                    Console.WriteLine($"{pushed} pushed.");
                }
                else
                {
                    Console.WriteLine("Incorrect # of args");
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex);
            }
        }
    }
}
