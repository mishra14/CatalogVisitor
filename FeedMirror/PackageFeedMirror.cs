using NuGet.CatalogVisitor;
using System;
using System.IO;
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
                if (args.Length == 1 && (args[0] == "/?" || args[0] == "--help"))
                {
                    Console.WriteLine("FeedMirror mirrors data from a given feed ({id} where id wanted, etc.)");
                    Console.WriteLine("to a given url (myget feed or normal url) or hard drive (C:\\ format).");
                    Console.WriteLine("You can also use globbing to specify a group of ids (ex: Altairis*)");
                    Console.WriteLine("as well as to specify a group of versions (ex: 2.0.*).");
                    Console.WriteLine("You can also specify a path to your own pre-existing cursor file and time (ex: C:\\myCursor.txt)");
                    Console.WriteLine("as well as one that does not exist for which we will use Min Time.");
                    Console.WriteLine("FeedMirror.exe (0) from feed (1) to feed (2) id globbing (3) version globbing (4) cursor file.");
                    Console.WriteLine("Parameters 2, 3, and 4 are optional, but you cannot have 3 without 4, etc.");
                }
                else if (args.Length > 5)
                {
                    Console.WriteLine($"Incorrect # of args: {args.Length}");
                }
                else
                {
                    /* Hard-coded examples for args in case none were entered or you want to run in debug/not exe or command line. */
                    var feed = "https://api.nuget.org/v3-flatcontainer/{id}/{version}/{id}.{version}.nupkg";
                    var output = "https://www.myget.org/F/theotherfeed/api/v3/index.json";
                    var fileName = "*";
                    var version = "*";
                    var givenCursor = "C:\\mainMirrorCursor.txt";

                    /* Reading in the args based on different lengths. */
                    if (args.Length >= 2)
                    {
                        feed = args[0];
                        output = args[1];
                    }
                    if (args.Length >= 3)
                    {
                        fileName = args[2];
                    }
                    if (args.Length >= 4)
                    {
                        version = args[3];
                    }
                    if (args.Length == 5)
                    {
                        givenCursor = args[4];
                    }

                    CatalogVisitorContext context = new CatalogVisitorContext();
                    context.CatalogCacheFolder = "C:\\MirrorPackages\\";
                    if (!Directory.Exists(context.CatalogCacheFolder))
                    {
                        DirectoryInfo di = Directory.CreateDirectory(context.CatalogCacheFolder);
                    }
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
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex);
            }
        }
    }
}
