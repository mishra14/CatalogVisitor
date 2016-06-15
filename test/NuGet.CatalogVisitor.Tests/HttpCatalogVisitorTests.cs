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
        public async Task BasicTest()
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

        public static string GetResource(string name)
        {
            using (var reader = new StreamReader(typeof(HttpCatalogVisitorTests).GetTypeInfo().Assembly.GetManifestResourceStream(name)))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
