using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace NuGet.CatalogVisitor.Tests
{
    public class EndToEndTests
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
        public async Task E2EGetGlobbingPackagesTest()
        {
            // Arrange
            SetUpHttp();
            _context.CatalogCacheFolder = "C:\\CatalogCache\\MirrorPackages\\";
            HttpCatalogVisitor visitor = new HttpCatalogVisitor(_context);

            /* Gets latest version for each ID from date in cursor to now. */
            FileCursor cursor = FileCursor.Load("C:\\CatalogCache\\e2eGlobbingCursor.txt");
            cursor.Date = DateTimeOffset.MinValue;
            cursor.CursorPath = "C:\\CatalogCache\\e2eGlobbingCursor.txt";

            // Act
            IReadOnlyList<PackageMetadata> packages = await visitor.GetPackages(cursor.Date, DateTimeOffset.UtcNow, "Altairis*");
            cursor.Save();

            // Assert
            Assert.Equal(3, packages.Count);
        }

        [Fact]
        public async Task E2EGetRawPackagesTest()
        {
            // Arrange
            SetUpHttp();
            _context.CatalogCacheFolder = "C:\\CatalogCache\\MirrorPackages\\";
            HttpCatalogVisitor visitor = new HttpCatalogVisitor(_context);

            /* Gets latest version for each ID from date in cursor to now. */
            FileCursor cursor = FileCursor.Load("C:\\CatalogCache\\e2eRawPackages.txt");
            cursor.Date = DateTimeOffset.MinValue;
            cursor.CursorPath = "C:\\CatalogCache\\e2eRawPackages.txt";

            // Act
            var packages = await visitor.GetRawPackages(DateTimeOffset.MinValue, DateTimeOffset.UtcNow);
            cursor.Save();

            // Assert
            Assert.Equal(16, packages.Count);
        }

        [Fact]
        public async Task E2EGetPackagesTest()
        {
            // Arrange
            SetUpHttp();
            _context.CatalogCacheFolder = "C:\\CatalogCache\\MirrorPackages\\";
            HttpCatalogVisitor visitor = new HttpCatalogVisitor(_context);

            /* Gets latest version for each ID from date in cursor to now. */
            FileCursor cursor = FileCursor.Load("C:\\CatalogCache\\e2ePackages.txt");
            cursor.Date = DateTimeOffset.MinValue;
            cursor.CursorPath = "C:\\CatalogCache\\e2ePackages.txt";

            // Act
            var packages = await visitor.GetPackages(DateTimeOffset.MinValue, DateTimeOffset.UtcNow);
            cursor.Save();

            // Assert
            Assert.Equal(16, packages.Count);
        }

        [Fact]
        public async Task E2EGetPackagesDatesTest()
        {
            // Arrange
            SetUpHttp();
            _context.CatalogCacheFolder = "C:\\CatalogCache\\MirrorPackages\\";
            HttpCatalogVisitor visitor = new HttpCatalogVisitor(_context);

            /* Gets latest version for each ID from date in cursor to now. */
            FileCursor cursor = FileCursor.Load("C:\\CatalogCache\\e2ePackagesDates.txt");
            var start = new DateTimeOffset(2015, 2, 1, 6, 0, 0, new TimeSpan(0, 0, 0));
            var end = new DateTimeOffset(2015, 8, 13, 8, 0, 0, new TimeSpan(0, 0, 0));
            cursor.Date = start;
            cursor.CursorPath = "C:\\CatalogCache\\e2ePackagesDates.txt";

            // Act
            var packages = await visitor.GetPackages(start, end);
            cursor.Save();

            // Assert
            Assert.Equal(16, packages.Count);
        }

        [Fact]
        public async Task E2EGetPackagesCursorTest()
        {
            // Arrange
            var tempDir = Path.Combine(Environment.GetEnvironmentVariable("temp"), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            SetUpHttp();
            _context.CatalogCacheFolder = tempDir;
            var testHandler = new TestMessageHandler();
            //_context.CatalogCacheFolder = "C:\\CatalogCache\\MirrorPackages\\";
            HttpCatalogVisitor visitor = new HttpCatalogVisitor(_context);
            HttpPackageDownloader HPD = new HttpPackageDownloader(_context);

            /* Gets latest version for each ID from date in cursor to now. */
            FileCursor cursor = FileCursor.Load("C:\\CatalogCache\\e2ePackagesCursor.txt");
            cursor.Date = new DateTimeOffset(2011, 3, 24, 7, 0, 0, new TimeSpan(-8, 0, 0)); // from that date to now
            cursor.CursorPath = "C:\\CatalogCache\\e2ePackages.txt";

            // Act
            var packages = await visitor.GetPackages(cursor);
            cursor.Save();

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

        public static string GetResource(string name)
        {
            using (var reader = new StreamReader(typeof(HttpCatalogVisitorTests).GetTypeInfo().Assembly.GetManifestResourceStream(name)))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
