using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NuGet.CatalogVisitor
{
    public class CatalogVisitorContext
    {
        public string FeedIndexJsonUrl { get; set; }

        public HttpMessageHandler MessageHandler { get; set; }

        /// <summary>
        /// Directory where cursor files are stored.
        /// </summary>
        public string CursorFolder { get; set; }

        /// <summary>
        /// Where you write the catalog to.
        /// </summary>
        public string CatalogCacheFolder { get; set; }

        /// <summary>
        /// Disable caching.
        /// </summary>
        public bool NoCache { get; set; }
    }
}
