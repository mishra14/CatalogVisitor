﻿using System;
using System.Threading.Tasks;
using NuGet.Protocol.Core.v3;
using NuGet.Protocol.Core.Types;
using NuGet.Common;
using System.IO;
using System.Net.Http;
using NuGet.CatalogVisitor;

namespace FeedMirror
{
    /// <summary>
    /// [‎6/‎23/‎2016 2:38 PM] Justin Emgarten: 
    /// yep!
    /// and you can take the feedindexjsonurl as a command line parameter, but don't worry about that at first. Just hook it all up to mirror packages from nuget to your myget feed
    /// also have a cursor(I think you already have that)
    /// and just run it every 10 minutes or so and observe that it finds new packages
    /// oh, you'll want to start the cursor out at like yesterday so you don't mirror 600K packages to your feed
    /// let me know if you hit any issues. once it's going we can take a look at some command line libraries to help parse the inputs
    /// then you can do mirror.exe -source http://nuget.org/.. -output http://myget..
    /// </summary>
    public class PackageMirror
    {
        /* Default feed. */
        private static CatalogVisitorContext _context = new CatalogVisitorContext("https://api.nuget.org/v3/index.json");
        /* Yesterday. */
        private static FileCursor _cursor = new FileCursor("C:\\CatalogCache\\packageMirrorCursor.txt", new DateTimeOffset(2016, 6, 22, 1, 1, 1, new TimeSpan(0)));
        SourceRepository _outputSource = Repository.Factory.GetCoreV3("https://www.myget.org/F/kaswan/api/v3/index.json");

        /// <summary>
        /// User passes in their feed with context.Feed.
        /// </summary>
        /// <param name="catalogContext"></param>
        /// <param name="outputSource"></param>
        public PackageMirror(CatalogVisitorContext catalogContext, string outputSource)
        {
            _context = catalogContext;
            _outputSource = Repository.Factory.GetCoreV3(outputSource);
        }

        public async Task<int> MirrorPackages()
        {
            // Read cursor
            _cursor = _cursor.Load(_cursor.CursorPath);
            var fileDate = _cursor.Date;

            // Get packages
            HttpCatalogVisitor hcv = new HttpCatalogVisitor(_context);
            var packages = hcv.GetPackages(_cursor).Result;

            // Push packages
            var pushResource = _outputSource.GetResource<PackageUpdateResource>();
            int pushed = 0;
            //directory.GetFiles("*.nupkg");
            foreach (var package in packages)
            {
                var packagePath = _context.CatalogCacheFolder + "Mirror-" + package.Id + "-" + package.Version.ToNormalizedString() + ".nupkg";
                Uri newUri = new Uri(packagePath);
                /* Do nothing if it is older than the cursor and exists. */
                if (fileDate >= package.CommitTimeStamp && File.Exists(packagePath))
                {
                    Console.WriteLine($"[CACHE] {newUri.AbsoluteUri}");
                }
                else
                {
                    Console.WriteLine($"[GET] {newUri.AbsoluteUri}");
                    HttpClient client = new HttpClient();
                    var myUrl = "https://api.nuget.org/v3-flatcontainer/" + package.Id.ToLower() + "/" + package.Version.ToString().ToLower() + "/" + package.Id.ToLower() + "." + 
                        package.Version.ToString().ToLower() + ".nupkg";
                    try
                    {
                        using (var stream = await client.GetStreamAsync(myUrl))
                        using (var outputStream = File.Create(packagePath))
                        {
                            await stream.CopyToAsync(outputStream);
                        }

                        /* How does this, specifically packagePath, work? */
                        await pushResource.Push(packagePath, "", 100, false, GetAPIKey, NullLogger.Instance);
                        pushed++;
                    }
                    catch (Exception ex)
                    {
                        /* BlueprintCSS 1.0.0 url doesn't work */
                        //throw new InvalidOperationException($"Failed {myUrl} exception: {ex.ToString()}");
                        /* don't download package */
                        Console.WriteLine($"Not downloading {myUrl} because it does not exist: \r\n{ex}.");
                    }
                }
            }

            // Save cursor
            _cursor.Save();
            return pushed;
        }

        private static string GetAPIKey(string source)
        {
            return "5d5886c5-e666-4d71-aaef-0af559d3d45a";
        }
    }
}