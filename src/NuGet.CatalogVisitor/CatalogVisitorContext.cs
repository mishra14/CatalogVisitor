using Newtonsoft.Json.Linq;
using NuGet.Versioning;
using System;
using System.Net.Http;

namespace NuGet.CatalogVisitor
{
    public class CatalogVisitorContext
    {
        /// <summary>
        /// This is an object you pass into other classes that holds
        /// lots of information you need in order to implement the functionality
        /// of the classes (HttpCatalogVisitor, etc.)
        /// </summary>
        public CatalogVisitorContext()
        {

        }
        
        /// <summary>
        /// The JSON that is read into the classes and parsed,
        /// assumed to have a certain structure - 4 levels
        /// Level 1 has an array w keyword "resources" and we get the last "@id" (url) to go to next level
        /// Level 2 and 3 have arrays w keywords "items" and we get the urls from all level 3s and 4s pages from "@id"
        /// Level 4 is the lowest page and contains the PackageMetadata info (id, version, pageCommitTime)
        /// </summary>
        public string FeedIndexJsonUrl { get; set; }

        /// <summary>
        /// The feed to move to another later specified feed.
        /// 
        /// *If you would like id, version, or time stamp used,
        /// please use {id}, {version}, and {commitTimeStamp}
        /// exactly as shown above in your url to be replaced.
        /// </summary>
        public string IncomingFeedUrl { get; set; }

        public HttpMessageHandler MessageHandler { get; set; }

        /// <summary>
        /// Where you write the catalog to.
        /// </summary>
        public string CatalogCacheFolder { get; set; }

        /// <summary>
        /// Disable caching.
        /// </summary>
        public bool NoCache { get; set; }

        /// <summary>
        /// Helper method to convert JObject to PackageMetadata.
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        public static PackageMetadata GetMetadata(JObject entry)
        {
            var tempId = entry["nuget:id"].ToObject<string>();
            var tempVersion = NuGetVersion.Parse(entry["nuget:version"].ToObject<string>());
            var tempDate = DateTimeOffset.Parse(entry["commitTimeStamp"].ToObject<string>());

            return new PackageMetadata(tempVersion, tempId, tempDate);
        }
    }
}
