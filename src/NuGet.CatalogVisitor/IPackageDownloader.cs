using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuGet.Versioning;

namespace NuGet.CatalogVisitor
{
    public interface IPackageDownloader
    {
        /// <summary>
        /// Download a package to the specified directory.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="version"></param>
        /// <param name="downloadDirectory"></param>
        /// <returns></returns>
        Task DownloadPackage(string id, NuGetVersion version, string downloadDirectory);
    }
}
