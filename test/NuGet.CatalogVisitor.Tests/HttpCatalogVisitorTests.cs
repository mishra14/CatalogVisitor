using NuGet.Packaging;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace NuGet.CatalogVisitor.Tests
{
    public class HttpCatalogVisitorTests
    {
        CatalogVisitorContext _context = new CatalogVisitorContext();

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
            _context.IncomingFeedUrl = "https://api.nuget.org/v3-flatcontainer/{id}/{version}/{id}.{version}.nupkg";
        }

        [Fact]
        public async Task GetGlobbingPackagesTest()
        {
            // Arrange
            SetUpHttp();

            HttpCatalogVisitor hcv = new HttpCatalogVisitor(_context);

            // Act
            var packages = await hcv.GetPackages(DateTimeOffset.MinValue, DateTimeOffset.UtcNow, "*.json");

            // Assert
            Assert.Equal(16, packages.Count);
        }

        [Fact]
        public void WildCardToRegexTest()
        {
            // Arrange
            SetUpHttp();
            HttpCatalogVisitor hcv = new HttpCatalogVisitor(_context);

            // Act
            string regex = HttpCatalogVisitor.WildcardToRegex("foo*.xls?");
            string expected = "^foo.*";
            expected = expected + "\\";
            expected = expected + ".xls.$";

            // Assert
            Assert.Equal(expected, regex);
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

            /* Delete files in and directory off their hard drive to save space. */
            DirectoryInfo di = new DirectoryInfo(tempDir);
            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            Directory.Delete(tempDir);
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
        public async Task DownloadPackageTest()
        {
            CatalogVisitorContext context = new CatalogVisitorContext();
            context.IncomingFeedUrl = "https://api.nuget.org/v3-flatcontainer/{id}/{version}/{id}.{version}.nupkg";
            context.FeedIndexJsonUrl = "https://api.nuget.org/v3/index.json";
            context.CatalogCacheFolder = "C:\\CatalogCache\\DownloadPackages\\";
            HttpPackageDownloader hpd = new HttpPackageDownloader(context);

            await hpd.DownloadPackage("Passive", new Versioning.NuGetVersion("0.2.0"), "C:\\CatalogCache\\DownloadPackages\\testDownloadPackage.nupkg");

            // ex: https://api.nuget.org/v3-flatcontainer/adam.jsgenerator/1.1.0/adam.jsgenerator.1.1.0.nupkg

            var myPackage = new PackageArchiveReader("C:\\CatalogCache\\DownloadPackages\\testDownloadPackage.nupkg");
            var id = myPackage.GetIdentity().Id;
            var version = myPackage.GetIdentity().Version;
            Assert.Equal(id, "Passive");
            Assert.Equal(version.ToNormalizedString(), "0.2.0");
        }

        [Fact]
        public async Task DownloadPackageTestDateRange()
        {
            SetUpHttp();
            _context.CatalogCacheFolder = "C:\\CatalogCache\\DownloadPackagesTest\\";
            HttpPackageDownloader hpd = new HttpPackageDownloader(_context);

            await hpd.DownloadPackagesDateRange(new DateTimeOffset(2015, 2, 1, 0, 0, 0, new TimeSpan(0)), new DateTimeOffset(2015, 2, 2, 0, 0, 0, new TimeSpan(0)), "C:\\CatalogCache\\DownloadPackagesTest\\");

            // ex: https://api.nuget.org/v3-flatcontainer/adam.jsgenerator/1.1.0/adam.jsgenerator.1.1.0.nupkg

            var myPackage = new PackageArchiveReader("C:\\CatalogCache\\DownloadPackagesTest\\Altairis-Web-Security2-0-0.nupkg");
            var id = myPackage.GetIdentity().Id;
            var version = myPackage.GetIdentity().Version;
            Assert.Equal(id, "Altairis.Web.Security");
            Assert.Equal(version.ToNormalizedString(), "2.0.0");
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
