using Xunit;
using FeedMirror;
using System;
using System.Threading.Tasks;

namespace NuGet.CatalogVisitor.Tests
{
    public class PackageMirrorTests
    {
        CatalogVisitorContext _context = new CatalogVisitorContext();

        private void SetUpHttp()
        {
            var testHandler = new TestMessageHandler();

            _context.NoCache = true;
            _context.FeedIndexJsonUrl = "https://api.nuget.org/v3/index.json";

            var rootIndex = HttpCatalogVisitorTests.GetResource("NuGet.CatalogVisitor.Tests.content.rootIndex.json");
            testHandler.Pages.TryAdd("https://api.nuget.org/v3/index.json", rootIndex);

            var catalogIndex = HttpCatalogVisitorTests.GetResource("NuGet.CatalogVisitor.Tests.content.catalog_index.json");
            testHandler.Pages.TryAdd("https://api.nuget.org/v3/catalog0/index.json", catalogIndex);

            for (int i = 0; i < 4; i++)
            {
                var levelThree = HttpCatalogVisitorTests.GetResource($"NuGet.CatalogVisitor.Tests.content.catalog{i}.json");
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
                    var levelThree = HttpCatalogVisitorTests.GetResource($"NuGet.CatalogVisitor.Tests.content.catalog{i}page{j}.json");

                    testHandler.Pages.TryAdd(urlArray[i, j], levelThree);
                }
            }

            _context.MessageHandler = testHandler;
        }
        
        [Fact]
        public async Task MirrorPackagesTest()
        {
            // Arrange
            SetUpHttp();

            _context.CatalogCacheFolder = "C:\\CatalogCache\\MirrorPackages\\";
            _context.IncomingFeedUrl = "https://api.nuget.org/v3-flatcontainer/{id}/{version}/{id}.{version}.nupkg";
            string mySource = "https://www.myget.org/F/theotherfeed/api/v3/index.json";

            PackageMirror myPM = new PackageMirror(_context, mySource);

            // Act
            var pushed = await myPM.MirrorPackages(DateTimeOffset.MinValue, DateTimeOffset.UtcNow);

            // Assert
            Assert.Equal(16, pushed);
        }

        [Fact]
        public async Task MirrorPackagesGlobbingTest()
        {
            // Arrange
            SetUpHttp();

            _context.CatalogCacheFolder = "C:\\CatalogCache\\MirrorPackages\\";
            _context.IncomingFeedUrl = "https://api.nuget.org/v3-flatcontainer/{id}/{version}/{id}.{version}.nupkg";
            string mySource = "https://www.myget.org/F/theotherfeed/api/v3/index.json";

            PackageMirror myPM = new PackageMirror(_context, mySource);

            // Act
            var pushed = await myPM.MirrorPackages(DateTimeOffset.MinValue, DateTimeOffset.UtcNow, "Altairis*", "2.*");

            // Assert
            Assert.Equal(1, pushed.Item1);
        }
    }
}
