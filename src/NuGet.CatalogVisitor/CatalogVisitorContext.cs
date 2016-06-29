using Newtonsoft.Json.Linq;
using NuGet.Versioning;
using System;
using System.Net.Http;

namespace NuGet.CatalogVisitor
{
    public class CatalogVisitorContext
    {
        public CatalogVisitorContext()
        {

        }

        public CatalogVisitorContext(string feed)
        {
            FeedIndexJsonUrl = feed;
        }
        
        public string FeedIndexJsonUrl { get; set; }

        /// <summary>
        /// If you would like id, version, or time stamp used,
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

        public static PackageMetadata GetMetadata(JObject entry)
        {
            var tempId = entry["nuget:id"].ToObject<string>();
            var tempVersion = NuGetVersion.Parse(entry["nuget:version"].ToObject<string>());
            var tempDate = DateTimeOffset.Parse(entry["commitTimeStamp"].ToObject<string>());

            return new PackageMetadata(tempVersion, tempId, tempDate);
        }
    }
}
