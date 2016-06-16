using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuGet.Versioning;
using System.IO;

namespace NuGet.CatalogVisitor
{
    public class HttpPackageDownloader : IPackageDownloader
    {
        /// <summary>
        /// API to download package to folder (e).
        /// </summary>
        /// <param name="id"></param>
        /// <param name="version"></param>
        /// <param name="downloadDirectory"></param>
        /// <returns></returns>
        public Task DownloadPackage(string id, NuGetVersion version, string downloadDirectory)
        {
            var newStr = id + "\r\n" + version;
            HttpCatalogVisitor.WriteToFileFromFolder(downloadDirectory, newStr);
            return Task.Delay(0);
        }
    }
}
