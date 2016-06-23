using NuGet.CatalogVisitor;
using System;

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
            /* This url is where user would set their own feed source to get the packages from. */
            CatalogVisitorContext context = new CatalogVisitorContext("https://api.nuget.org/v3/index.json");
            context.CatalogCacheFolder = "C:\\CatalogCache\\MirrorPackages\\";
            string mySource = "https://www.myget.org/F/kaswan/api/v3/index.json";

            PackageMirror myPM = new PackageMirror(context, mySource);
            
            var pushed = myPM.MirrorPackages().Result;

            Console.WriteLine($"{pushed} pushed.");
        }
    }
}
