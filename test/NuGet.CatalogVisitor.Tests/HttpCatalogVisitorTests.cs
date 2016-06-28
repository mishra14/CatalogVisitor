using NuGet.Packaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace NuGet.CatalogVisitor.Tests
{
    public class HttpCatalogVisitorTests
    {
        CatalogVisitorContext _context = new CatalogVisitorContext();

        //[Fact]
        //public async Task GetRawPackagesTest()
        //{
        //    // Arrange
        //    CatalogVisitorContext context = new CatalogVisitorContext();
        //    context.NoCache = true;
        //    context.FeedIndexJsonUrl = "https://api.nuget.org/v3/index.json";

        //    var testHandler = new TestMessageHandler();

        //    var rootIndex = GetResource("NuGet.CatalogVisitor.Tests.content.rootIndex.json");

        //    testHandler.Pages.TryAdd("https://api.nuget.org/v3/index.json", rootIndex);
        //    context.MessageHandler = testHandler;

        //    HttpCatalogVisitor hcv = new HttpCatalogVisitor(context);

        //    // Act
        //    var packages = await hcv.GetRawPackages();

        //    // Assert
        //    Assert.Equal(2700, packages.Count);
        //}

        private void SetUpHttp()
        {
            var testHandler = new TestMessageHandler();

            _context.NoCache = true;
            _context.FeedIndexJsonUrl = "https://api.nuget.org/v3/index.json";

            var rootIndex = GetResource("NuGet.CatalogVisitor.Tests.content.rootIndex.json");
            testHandler.Pages.TryAdd("https://api.nuget.org/v3/index.json", rootIndex);

            var catalogIndex = GetResource("NuGet.CatalogVisitor.Tests.content.catalog_index.json");
            testHandler.Pages.TryAdd("https://api.nuget.org/v3/catalog0/index.json", catalogIndex);

            for (int i = 0; i < 4; i++)
            {
                var levelThree = GetResource($"NuGet.CatalogVisitor.Tests.content.catalog{i}.json");
                testHandler.Pages.TryAdd($"https://api.nuget.org/v3/catalog0/page{i}.json", levelThree);
            }

            var urlArray = new string[4, 4] { { "https://api.nuget.org/v3/catalog0/data/2015.02.01.06.22.45/adam.jsgenerator.1.1.0.json", "https://api.nuget.org/v3/catalog0/data/2015.02.01.06.22.45/agatha-rrsl.1.2.0.json",
                "https://api.nuget.org/v3/catalog0/data/2015.02.01.06.22.45/altairis.mailtoolkit.1.0.0.json", "https://api.nuget.org/v3/catalog0/data/2015.02.01.06.22.45/altairis.web.security.2.0.0.json"},
                { "https://api.nuget.org/v3/catalog0/data/2015.02.01.06.34.14/structuremap.2.6.2.json", "https://api.nuget.org/v3/catalog0/data/2015.02.01.06.33.04/elevate.0.1.0.json",
                "https://api.nuget.org/v3/catalog0/data/2015.02.01.06.30.59/bakery.template.vb.1.0.0.json", "https://api.nuget.org/v3/catalog0/data/2015.02.01.06.39.12/front.0.0.3.json" },
                { "https://api.nuget.org/v3/catalog0/data/2015.02.01.06.48.04/altairis.web.security.2.4.0.json", "https://api.nuget.org/v3/catalog0/data/2015.02.01.06.44.14/chainingassertion.1.4.0.json",
                "https://api.nuget.org/v3/catalog0/data/2015.02.01.06.43.41/wcfrestcontrib.1.6.121.json", "https://api.nuget.org/v3/catalog0/data/2015.02.01.06.47.46/autofac.wp7.2.4.5.724.json" },
                { "https://api.nuget.org/v3/catalog0/data/2015.02.01.06.50.55/vlc.1.1.8.json", "https://api.nuget.org/v3/catalog0/data/2015.02.01.06.53.27/clientdependency.1.3.2.json",
                "https://api.nuget.org/v3/catalog0/data/2015.02.01.06.55.09/routemagic.0.2.2.1.json", "https://api.nuget.org/v3/catalog0/data/2015.02.01.06.50.19/developwithpassion.specifications.nsubstitue.0.4.0.1.json" } };

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    var levelThree = GetResource($"NuGet.CatalogVisitor.Tests.content.catalog{i}page{j}.json");
    
                    testHandler.Pages.TryAdd(urlArray[i, j], levelThree);
                }
            }

            _context.MessageHandler = testHandler;
        }

        [Fact]
        public async Task GetRawPackagesTest()
        {
            // Arrange
            SetUpHttp();

            HttpCatalogVisitor hcv = new HttpCatalogVisitor(_context);

            // Act
            var packages = await hcv.GetRawPackages(DateTimeOffset.MinValue, DateTimeOffset.UtcNow);

            // Assert
            Assert.Equal(16, packages.Count);
        }

        [Fact]
        public async Task GetPackagesTest()
        {
            // Arrange
            SetUpHttp();

            HttpCatalogVisitor hcv = new HttpCatalogVisitor(_context);

            // Act
            var packages = await hcv.GetPackages(DateTimeOffset.MinValue, DateTimeOffset.UtcNow);

            // Assert
            Assert.Equal(16, packages.Count);
        }

        [Fact]
        public async Task GetPackagesCursorTest()
        {
            // Arrange
            var tempDir = Path.Combine(Environment.GetEnvironmentVariable("temp"), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            SetUpHttp();
            _context.CatalogCacheFolder = tempDir;

            var testHandler = new TestMessageHandler();

            HttpCatalogVisitor hcv = new HttpCatalogVisitor(_context);

            MemoryCursor cursor = new MemoryCursor();
            cursor.Date = new DateTimeOffset(2011, 3, 24, 7, 0, 0, new TimeSpan(-8, 0, 0)); // from that date to now

            // Act
            var packages = await hcv.GetPackages(cursor);

            // Assert
            Assert.Equal(16, packages.Count);
        }

        [Fact]
        public async Task GetPackagesDatesTest()
        {
            // Arrange
            SetUpHttp();

            HttpCatalogVisitor hcv = new HttpCatalogVisitor(_context);

            DateTimeOffset start = new DateTimeOffset(2014, 2, 1, 7, 0, 0, new TimeSpan(-8, 0, 0));
            DateTimeOffset end = DateTimeOffset.UtcNow;

            // Act
            var packages = await hcv.GetPackages(start, end);

            // Assert
            Assert.Equal(16, packages.Count);
        }

        /// <summary>
        /// HELPER METHOD
        /// </summary>
        private async Task<IEnumerable<PackageMetadata>> GetPackagesDatesHelper(DateTimeOffset start, DateTimeOffset end)
        {
            // Arrange
            SetUpHttp();

            HttpCatalogVisitor hcv = new HttpCatalogVisitor(_context);

            // Act
            var packages = await hcv.GetPackages(start, end);

            var result = packages.Take(4);

            return result;
        }

        [Fact]
        public void WriteToFileFromFolderTest()
        {
            //Arrange
            string file = "C:\\CatalogCache\\testFile.txt";
            string content = "Test content.";

            //Act
            HttpCatalogVisitor.WriteToFileFromFolder(file, content);
            string fileContent = File.ReadAllText(file);

            //Assert
            Assert.Equal(content, fileContent);
        }

        [Fact]
        public void DownloadPackageTest()
        {
            HttpPackageDownloader hpd = new HttpPackageDownloader();

            hpd.DownloadPackage("Passive", new Versioning.NuGetVersion("0.2.0"), "C:\\CatalogCache\\DownloadPackages\\testDownloadPackage.nupkg");

            // https://api.nuget.org/v3-flatcontainer/adam.jsgenerator/1.1.0/adam.jsgenerator.1.1.0.nupkg

            var myPackage = new PackageArchiveReader("C:\\CatalogCache\\DownloadPackages\\testDownloadPackage.nupkg");
            var id = myPackage.GetIdentity().Id;
            var version = myPackage.GetIdentity().Version;
            Assert.Equal(id, "Passive");
            Assert.Equal(version.ToNormalizedString(), "0.2.0");
        }

        [Fact]
        public void DownloadPackagesDateRangeTest()
        {
            using (var workingDirectory = TestFileSystemUtility.CreateRandomTestFolder())
            {
                HttpPackageDownloader hpd = new HttpPackageDownloader();

                /* Yesterday */
                DownloadPackagesDateRangeTestHelper(new DateTimeOffset(2015, 2, 1, 0, 0, 0, new TimeSpan(0)), 
                    new DateTimeOffset(2015, 2, 2, 0, 0, 0, new TimeSpan(0)), workingDirectory);

                string tempDirectory = Path.Combine(workingDirectory, "Altairis.Web.Security".Replace(".", "-") + "2.0.0".Replace(".", "-") + ".nupkg");

                var myPackage = new PackageArchiveReader(tempDirectory);
                var id = myPackage.GetIdentity().Id;
                var version = myPackage.GetIdentity().Version;
                Assert.Equal(id, "Altairis.Web.Security");
                Assert.Equal(version.ToNormalizedString(), "2.0.0");
            }
        }

        /// <summary>
        /// FOR TESTING
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        private async void DownloadPackagesDateRangeTestHelper(DateTimeOffset start, DateTimeOffset end, string downloadDirectory)
        {
            HttpPackageDownloader hpd = new HttpPackageDownloader();

            var packages = await GetPackagesDatesHelper(start, end);

            foreach (var package in packages)
            {
                var nupkgName = package.Id.Replace(".", "-") + package.Version.ToNormalizedString().Replace(".", "-") + ".nupkg";
                string tempDirectory = Path.Combine(downloadDirectory, nupkgName);
                Console.WriteLine(tempDirectory);
                hpd.DownloadPackage(package.Id, package.Version, tempDirectory);
            }
        }

        public static string GetResource(string name)
        {
            using (var reader = new StreamReader(typeof(HttpCatalogVisitorTests).GetTypeInfo().Assembly.GetManifestResourceStream(name)))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
