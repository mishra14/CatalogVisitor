using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NuGet.CatalogVisitor.Tests
{
    public class HttpCatalogVisitorTests
    {
        [Fact]
        public async Task GetRawPackagesTest()
        {
            // Arrange
            CatalogVisitorContext context = new CatalogVisitorContext();
            context.NoCache = true;
            context.FeedIndexJsonUrl = "https://api.nuget.org/v3/index.json";

            var testHandler = new TestMessageHandler();

            var rootIndex = GetResource("NuGet.CatalogVisitor.Tests.content.rootIndex.json");

            testHandler.Pages.TryAdd("https://api.nuget.org/v3/index.json", rootIndex);
            context.MessageHandler = testHandler;

            HttpCatalogVisitor hcv = new HttpCatalogVisitor(context);

            // Act
            var packages = await hcv.GetRawPackages();

            // Assert
            Assert.Equal(1, packages.Count);
        }

        public async Task GetPackagesTest()
        {
            // Arrange
            CatalogVisitorContext context = new CatalogVisitorContext();
            context.NoCache = true;
            context.FeedIndexJsonUrl = "https://api.nuget.org/v3/index.json";

            var testHandler = new TestMessageHandler();

            var rootIndex = GetResource("NuGet.CatalogVisitor.Tests.content.rootIndex.json");

            testHandler.Pages.TryAdd("https://api.nuget.org/v3/index.json", rootIndex);
            context.MessageHandler = testHandler;

            HttpCatalogVisitor hcv = new HttpCatalogVisitor(context);

            // Act
            var packages = await hcv.GetPackages();

            // Assert
            Assert.Equal(1, packages.Count);
        }

        public async Task GetPackagesCursorTest()
        {
            // Arrange
            CatalogVisitorContext context = new CatalogVisitorContext();
            context.NoCache = true;
            context.FeedIndexJsonUrl = "https://api.nuget.org/v3/index.json";

            var testHandler = new TestMessageHandler();

            var rootIndex = GetResource("NuGet.CatalogVisitor.Tests.content.rootIndex.json");

            testHandler.Pages.TryAdd("https://api.nuget.org/v3/index.json", rootIndex);
            context.MessageHandler = testHandler;

            HttpCatalogVisitor hcv = new HttpCatalogVisitor(context);

            FileCursor cursor = new FileCursor();
            cursor.Date = new DateTimeOffset(2015, 2, 1, 7, 0, 0, new TimeSpan(-8, 0, 0)); // from that date to now
            cursor.CursorPath = "C:\\CatalogCache\\testCursor.txt";

            // Act
            var packages = await hcv.GetPackages(cursor);

            // Assert
            Assert.Equal(1, packages.Count);
        }

        public async Task GetPackagesDatesTest()
        {
            // Arrange
            CatalogVisitorContext context = new CatalogVisitorContext();
            context.NoCache = true;
            context.FeedIndexJsonUrl = "https://api.nuget.org/v3/index.json";

            var testHandler = new TestMessageHandler();

            var rootIndex = GetResource("NuGet.CatalogVisitor.Tests.content.rootIndex.json");

            testHandler.Pages.TryAdd("https://api.nuget.org/v3/index.json", rootIndex);
            context.MessageHandler = testHandler;

            HttpCatalogVisitor hcv = new HttpCatalogVisitor(context);

            DateTimeOffset start = new DateTimeOffset(2015, 2, 1, 7, 0, 0, new TimeSpan(-8, 0, 0));
            DateTimeOffset end = DateTimeOffset.UtcNow;

            // Act
            var packages = await hcv.GetPackages(start, end);

            // Assert
            Assert.Equal(1, packages.Count);
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
