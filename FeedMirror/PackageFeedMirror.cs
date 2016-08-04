using NuGet.CatalogVisitor;
using System;
using System.IO;
using System.Threading.Tasks;

namespace FeedMirror
{
    /// <summary>
    /// An exe that I can give to someone else 
    /// that mirrors packages from one feed to another
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
                /* Print out the help menu if there are no params, or a help command. */
                if ((args.Length == 1 && (args[0] == "/?" || args[0] == "--help")) || args.Length == 0)
                {
                    Console.WriteLine("Displays how to run the FeedMirror exe on the command line.\n");
                    Console.WriteLine("FeedMirror.exe [from url] [to url/path] [id glob] [version glob] [cursor file]\n");
                    Console.WriteLine("[from url]\tFeedMirror mirrors data from a given feed url (ex: https://api.nuget.org/v3/index.json)");
                    Console.WriteLine("[to url/path]\tto a given url (myget feed or normal url) or hard drive (C:\\ format).");
                    Console.WriteLine("[id glob]\tYou can also use globbing to specify a group of ids (ex: Altairis*)");
                    Console.WriteLine("[version glob]\tas well as to specify a group of versions (ex: 2.0.*).");
                    Console.WriteLine("[cursor file]\tYou can also specify a path to your own pre-existing cursor file and time (ex: C:\\myCursor.txt)");
                    Console.WriteLine("\t\t***make sure this file is readable by our program,");
                    Console.WriteLine("\t\tex: Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + \"\\mainMirrorCursor.txt\"");
                    Console.WriteLine("\t\tas well as one that does not exist which we will create and use Min Time.\n");
                    Console.WriteLine("\t\tParameters 3, 4, and 5 are optional, but you cannot have 3 without 4, etc.\n");
                }
                else if (args.Length > 5 || args.Length == 1)
                {
                    Console.WriteLine($"Incorrect # of args: {args.Length}");
                }
                else
                {
                    CatalogVisitorContext context = new CatalogVisitorContext();
                    context.CatalogCacheFolder = "C:\\MirrorPackages\\"; // Randomly created folder for mirrored packages.
                    context.FeedIndexJsonUrl = "https://api.nuget.org/v3/index.json"; // The v3 index.json url.
                    if (!Directory.Exists(context.CatalogCacheFolder))
                    {
                        DirectoryInfo di = Directory.CreateDirectory(context.CatalogCacheFolder);
                    }
                    FileCursor cursor = new FileCursor();
                    HttpCatalogVisitor hcv = new HttpCatalogVisitor(context);

                    /* Hard-coded examples for args in case none were entered or you want to run in debug/not exe or command line. */
                    //var feed = await hcv.GetFlatContainerUrl();
                    var feed = "https://api.nuget.org/v3/index.json";
                    var output = PackageMirror.GetMyGetString(); // My MyGet Feed.
                    var fileName = "*";
                    var version = "*";
                    var givenCursor = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\mainMirrorCursor.txt";
                    //var givenCursor = "C:\\mainMirrorCursor.txt"; // Randomly created cursor file.

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

                    /* Set to original or read in values. */
                    var urlBeg = await hcv.GetNewFlatContainerUrl(feed);
                    var urlComplete = urlBeg + "{id}/{version}/{id}.{version}.nupkg";
                    context.IncomingFeedUrl = urlComplete;
                    string mySource = output;
                    var verGlobPattern = fileName;
                    var idGlobPattern = version;
                    cursor.CursorPath = givenCursor;
                    cursor.Date = DateTimeOffset.MinValue;
                    //cursor.Date = new DateTimeOffset(2016, 3, 1, 1, 0, 0, new TimeSpan(-7, 0, 0));

                    /* cursor.Date now has correct date (replaces hardcoded w user date if applicable) */
                    cursor = FileCursor.Load(cursor.CursorPath);
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
                Console.WriteLine(ex);
            }
        }
    }
}
