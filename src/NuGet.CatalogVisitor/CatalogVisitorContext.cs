using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet.CatalogVisitor
{
    public class CatalogVisitorContext
    {
        /// <summary>
        /// Directory where cursor files are stored.
        /// </summary>
        public string CursorFolder { get; }

        /// <summary>
        /// Where you write the catalog to.
        /// </summary>
        public string CatalogCacheFolder { get; }

        /// <summary>
        /// Disable caching.
        /// </summary>
        public bool NoCache { get; }
    }
}
