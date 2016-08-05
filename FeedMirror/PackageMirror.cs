using System;
using System.Threading.Tasks;
using NuGet.Protocol.Core.v3;
using NuGet.Protocol.Core.Types;
using NuGet.Common;
using System.IO;
using System.Net.Http;
using NuGet.CatalogVisitor;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace FeedMirror
{
    public class PackageMirror
    {
        /* Default feed. */
        private CatalogVisitorContext _context;
        private SourceRepository _outputSource;
        private string _sourceStr;

        /// <summary>
        /// User passes in their feed with catalogContext.IncomingFeedUrl,
        /// user passes in feed they want to push to (myGet feed for example)
        /// as a string (url?) as outputSource.
        /// 
        /// Mirrors packages from input to output.
        /// </summary>
        /// <param name="catalogContext"></param>
        /// <param name="outputSource"></param>
        public PackageMirror(CatalogVisitorContext catalogContext, string outputSource)
        {
            _context = catalogContext;
            _sourceStr = outputSource;
            _outputSource = Repository.Factory.GetCoreV3(outputSource);
        }

        /// <summary>
        /// The function that gets all packages using context user passes in,
        /// and for each package, tries to open up the incomingFeedUrl and get the contents,
        /// then push the contents to the feed specified.
        /// If there is an error, it moves on to the next package, the error usually means
        /// the package doesn't exist so we don't want to push it or stop the program.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public async Task<int> MirrorPackages(DateTimeOffset start, DateTimeOffset end)
        {
            // Get packages
            HttpCatalogVisitor hcv = new HttpCatalogVisitor(_context);
            var packages = await hcv.GetPackages(start, end);

            Console.WriteLine($"Found {packages.Count} packages.");
            Console.WriteLine($"Pushing");

            // Push packages
            var pushResource = _outputSource.GetResource<PackageUpdateResource>();
            int pushed = 0;


            foreach (var package in packages)
            {
                /* Not hard coded: added onto end of context.CatalogCache... */
                var packagePath = _context.CatalogCacheFolder + "Mirror-" + package.Id + "-" + package.Version.ToNormalizedString() + ".nupkg";
                /* Do nothing if it is older than the cursor and exists. */
                if ((start > package.CommitTimeStamp || end <= package.CommitTimeStamp) && File.Exists(packagePath))
                {
                    Console.WriteLine($"[CACHE] {packagePath}");
                }
                else if (start < package.CommitTimeStamp && end >= package.CommitTimeStamp)
                {
                    Console.WriteLine($"[GET] {packagePath}");
                    /* If to feed is a url. */
                    if (_sourceStr.StartsWith("http"))
                    {
                        try
                        {
                            GetUrlPush(package, packagePath, pushResource);
                            pushed++;
                        }
                        catch (Exception ex)
                        {
                            /* BlueprintCSS 1.0.0 url doesn't work */
                            //throw new InvalidOperationException($"Failed {myUrl} exception: {ex.ToString()}");
                            /* don't download package */
                            Console.WriteLine($"Not downloading url because it does not exist: \r\n{ex}.");
                            //throw;
                            /* Move onto next var in loop, don't throw to catch in main, etc. */
                            continue;
                        }
                    } /* If to feed is a file system. */
                    else if (_sourceStr.StartsWith("C:"))
                    {
                        try
                        {
                            GetDirPush(package, packagePath, pushResource);
                            pushed++;
                        }
                        catch (Exception ex)
                        {
                            /* BlueprintCSS 1.0.0 url doesn't work */
                            //throw new InvalidOperationException($"Failed {myUrl} exception: {ex.ToString()}");
                            /* don't download package */
                            Console.WriteLine($"Not downloading dir because it does not exist: \r\n{ex}.");
                            //throw;
                            /* Move onto next var in loop, don't throw to catch in main, etc. */
                            continue;
                        }
                    }
                }
            }

            return pushed;
        }

        /// <summary>
        /// W/ package name and version globbing!
        /// The function that gets all packages using context user passes in,
        /// and for each package, tries to open up the incomingFeedUrl and get the contents,
        /// then push the contents to the feed specified.
        /// If there is an error, it moves on to the next package, the error usually means
        /// the package doesn't exist so we don't want to push it or stop the program.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public async Task<Tuple<int, DateTimeOffset>> MirrorPackages(DateTimeOffset start, DateTimeOffset end, string packagePattern = "*", string versionPattern = "*")
        {
            // Get packages
            HttpCatalogVisitor hcv = new HttpCatalogVisitor(_context);
            DateTimeOffset returnDate = DateTimeOffset.UtcNow;

            //var packages = await hcv.GetPackages(start, end);
            var packages = await hcv.GetPackages(start, end, packagePattern, versionPattern);

            Console.WriteLine($"Found {packages.Count} packages.");
            Console.WriteLine($"Pushing");

            // Push packages
            var pushResource = _outputSource.GetResource<PackageUpdateResource>();
            int pushed = 0;

            /* See if version and name globbing matches. */
            //var newPackPattern = HttpCatalogVisitor.WildcardToRegex(packagePattern);
            //Regex g = new Regex(newPackPattern);
            //var newVerPattern = HttpCatalogVisitor.WildcardToRegex(versionPattern);
            //Regex x = new Regex(newVerPattern);

            List<string> cached = new List<string>();
            List<string> added = new List<string>();


            foreach (var package in packages)
            {
                /* Not hard coded: added onto end of context.CatalogCache... */
                var packageEnding = "Mirror-" + package.Id + "-" + package.Version.ToNormalizedString() + ".nupkg";
                var packagePath = _context.CatalogCacheFolder + packageEnding;

                /* See if version and name globbing matches. */
                //Match idMatch = g.Match(package.Id);
                //Match versionMatch = x.Match(package.Version.ToNormalizedString());
                //bool matchSuccess = idMatch.Success && versionMatch.Success;
                

                /* Only add anything if the id and version globbing both match. */
                //if (matchSuccess)
                {
                    /* Do nothing if it is older than the cursor and exists. */
                    //if ((start > package.CommitTimeStamp || end <= package.CommitTimeStamp) && File.Exists(packagePath))
                    //{
                    //    var tempPath = packagePath.Split('\\');
                    //    cached.Add(tempPath[tempPath.Length - 1]);
                    //}
                    if (start < package.CommitTimeStamp && end >= package.CommitTimeStamp)
                    {
                        added.Add(packageEnding);
                        if (_sourceStr.StartsWith("http"))
                        {
                            try
                            {
                                GetUrlPush(package, packagePath, pushResource);
                                pushed++;
                            }
                            catch (Exception ex)
                            {
                                /* BlueprintCSS 1.0.0 url doesn't work */
                                //throw new InvalidOperationException($"Failed {myUrl} exception: {ex.ToString()}");
                                /* don't download package */
                                Console.WriteLine($"Not downloading url because it does not exist: \r\n{ex}.");
                                //throw;
                                /* Move onto next var in loop, don't throw to catch in main, etc. */
                                continue;
                            }
                        }
                        else if (_sourceStr.StartsWith("C:") || _sourceStr.StartsWith("c:"))
                        {
                            try
                            {
                                GetDirPush(package, packagePath, pushResource);
                                pushed++;
                            }
                            catch (Exception ex)
                            {
                                /* BlueprintCSS 1.0.0 url doesn't work */
                                //throw new InvalidOperationException($"Failed {myUrl} exception: {ex.ToString()}");
                                /* don't download package */
                                Console.WriteLine($"Not downloading dir because it does not exist: \r\n{ex}.");
                                //throw;
                                /* Move onto next var in loop, don't throw to catch in main, etc. */
                                continue;
                            }
                        }
                    }
                }
            }
            /* Show results in a more compact, English, user-friendly manner. */
            //var cachedMsg = "";
            var addedMsg = "";
            //if (cached.Count == 0)
            //{
            //    cachedMsg = "No results were found *outside* of the dates and parameters specified.\n";
            //}
            //else
            //{
            //    cachedMsg = "Results: " + cached[0] + " until " + cached[cached.Count - 1] + " did not fall into the parameters specified and were *not* downloaded.\n";
            //}
            if (added.Count == 0)
            {
                addedMsg = "Results: No results were found *inside* of the dates and parameters specified.\n";
            }
            else
            {
                addedMsg = "Results: \"" + added[0] + "\" until \"" + added[added.Count - 1] + "\" *did* fall into the date range and " + added.Count + " packages have been downloaded.\n";
            }
            Console.Write(addedMsg);

            var returnTuple = Tuple.Create(pushed, returnDate);

            return returnTuple;
        }

        /// <summary>
        /// Pushes one package to a myget feed url.
        /// </summary>
        /// <param name="package"></param>
        /// <param name="packagePath"></param>
        /// <param name="pushResource"></param>
        private async void GetUrlPush(PackageMetadata package, string packagePath, PackageUpdateResource pushResource)
        {
            try
            {
                HttpClient client = new HttpClient();
                var myUrl = _context.IncomingFeedUrl;

                /* Create the url you will use. */
                myUrl = myUrl.Replace("{id}", package.Id.ToLower());
                myUrl = myUrl.Replace("{version}", package.Version.ToNormalizedString().ToLower());
                myUrl = myUrl.Replace("{commitTimeStamp}", package.CommitTimeStamp.ToString());

                /* Read from url, push that into packagePath file. */
                using (var stream = await client.GetStreamAsync(myUrl))
                using (var outputStream = File.Create(packagePath))
                {
                    await stream.CopyToAsync(outputStream);
                }

                /* Push packagePath file contents onto myget feed url (pushResource). */
                await pushResource.Push(packagePath, "", 500, false, GetAPIKey, NullLogger.Instance);

                /* Clean up */
                File.Delete(packagePath);
            }
            catch (Exception ex)
            {
                // Do nothing.
            }
        }

        /// <summary>
        /// Pushes one package to new path in file path user passed in.
        /// </summary>
        /// <param name="package"></param>
        /// <param name="packagePath"></param>
        /// <param name="pushResource"></param>
        private async void GetDirPush(PackageMetadata package, string packagePath, PackageUpdateResource pushResource)
        {
            try
            {
                // Create from url
                HttpClient client = new HttpClient();
                var myUrl = _context.IncomingFeedUrl;
                myUrl = myUrl.Replace("{id}", package.Id.ToLower());
                myUrl = myUrl.Replace("{version}", package.Version.ToNormalizedString().ToLower());
                myUrl = myUrl.Replace("{commitTimeStamp}", package.CommitTimeStamp.ToString());

                // Create to file path
                var newFilePath = _context.IncomingFeedUrl + "Mirror-" + package.Id + "-" + package.Version.ToNormalizedString() + ".nupkg";

                // Copy from from url to to file path
                using (var stream = await client.GetStreamAsync(myUrl))
                using (var outputStream = File.Create(newFilePath))
                {
                    await stream.CopyToAsync(outputStream);
                }

                // Clean up
                File.Delete(packagePath);
            }
            catch (Exception ex)
            {
                // Do nothing.
            }
        }

        /// <summary>
        /// How do I hide this key? Do I need to pass in this key since it changes based on feed passed in?
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private static string GetAPIKey(string source)
        {
            return "5d5886c5-e666-4d71-aaef-0af559d3d45a";
        }

        public static string GetMyGetString()
        {
            return "https://www.myget.org/F/theotherfeed/api/v3/index.json";
        }
    }
}
