using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace NuGet.CatalogVisitor.Tests
{
    public class HttpCatalogVisitorTests
    {
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

        [Fact]
        public void GetRawPackagesTest()
        {
            // Arrange
            CatalogVisitorContext context = new CatalogVisitorContext();
            context.NoCache = true;
            context.FeedIndexJsonUrl = "C:\\CatalogCache\\TestJson\\index.json";

            var testHandler = new TestMessageHandler();

            //var rootIndex = GetResource("C:\\CatalogCache\\TestJson\\index.txt");
            var rootIndex = File.ReadAllText("C:\\CatalogCache\\TestJson\\index.json");

            testHandler.Pages.TryAdd("C:\\CatalogCache\\TestJson\\index.json", rootIndex);
            context.MessageHandler = testHandler;

            HttpCatalogVisitor hcv = new HttpCatalogVisitor(context);

            // Act
            var packages = hcv.GetRawPackagesDisk(DateTimeOffset.MinValue, DateTimeOffset.UtcNow);

            // Assert
            Assert.Equal(16, packages.Count);
        }

        [Fact]
        public void GetPackagesTest()
        {
            // Arrange
            CatalogVisitorContext context = new CatalogVisitorContext();
            context.NoCache = true;
            context.FeedIndexJsonUrl = "C:\\CatalogCache\\TestJson\\index.json";

            var testHandler = new TestMessageHandler();

            //var rootIndex = GetResource("NuGet.CatalogVisitor.Tests.content.rootIndex.json");
            var rootIndex = File.ReadAllText("C:\\CatalogCache\\TestJson\\index.json");

            testHandler.Pages.TryAdd("C:\\CatalogCache\\TestJson\\index.json", rootIndex);
            context.MessageHandler = testHandler;

            HttpCatalogVisitor hcv = new HttpCatalogVisitor(context);

            // Act
            var packages = hcv.GetPackagesDisk(DateTimeOffset.MinValue, DateTimeOffset.UtcNow);

            // Assert
            Assert.Equal(16, packages.Count);
        }

        [Fact]
        public void GetPackagesCursorTest()
        {
            // Arrange
            var tempDir = Path.Combine(Environment.GetEnvironmentVariable("temp"), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            CatalogVisitorContext context = new CatalogVisitorContext();
            context.NoCache = true;
            context.FeedIndexJsonUrl = "C:\\CatalogCache\\TestJson\\index.json";
            context.CatalogCacheFolder = tempDir;

            var testHandler = new TestMessageHandler();

            //var rootIndex = GetResource("NuGet.CatalogVisitor.Tests.content.rootIndex.json");
            var rootIndex = File.ReadAllText("C:\\CatalogCache\\TestJson\\index.json");

            testHandler.Pages.TryAdd("C:\\CatalogCache\\TestJson\\index.json", rootIndex);
            context.MessageHandler = testHandler;

            HttpCatalogVisitor hcv = new HttpCatalogVisitor(context);

            MemoryCursor cursor = new MemoryCursor();
            cursor.Date = new DateTimeOffset(2011, 3, 24, 7, 0, 0, new TimeSpan(-8, 0, 0)); // from that date to now

            // Act
            var packages = hcv.GetPackagesDisk(cursor);

            // Assert
            Assert.Equal(16, packages.Count);
        }

        [Fact]
        public async Task GetPackagesDatesTest()
        {
            // Arrange
            CatalogVisitorContext context = new CatalogVisitorContext();
            context.NoCache = true;
            context.FeedIndexJsonUrl = "C:\\CatalogCache\\TestJson\\index.json";

            var testHandler = new TestMessageHandler();

            //var rootIndex = GetResource("NuGet.CatalogVisitor.Tests.content.rootIndex.json");
            var rootIndex = File.ReadAllText("C:\\CatalogCache\\TestJson\\index.json");

            testHandler.Pages.TryAdd("C:\\CatalogCache\\TestJson\\index.json", rootIndex);
            context.MessageHandler = testHandler;

            HttpCatalogVisitor hcv = new HttpCatalogVisitor(context);

            DateTimeOffset start = new DateTimeOffset(2015, 2, 1, 7, 0, 0, new TimeSpan(-8, 0, 0));
            DateTimeOffset end = DateTimeOffset.UtcNow;

            // Act
            var packages = await hcv.GetPackages(start, end);

            // Assert
            Console.WriteLine(packages.Count);
            Assert.Equal(460, packages.Count);
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

            hpd.DownloadPackage("MyID", new Versioning.NuGetVersion("1.0.0"), "C:\\CatalogCache\\testDownloadPackage.txt");

            string fileContent = File.ReadAllText("C:\\CatalogCache\\testDownloadPackage.txt");
            string expectedContent = "MyID\r\n1.0.0";
            Assert.Equal(fileContent, expectedContent);
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
