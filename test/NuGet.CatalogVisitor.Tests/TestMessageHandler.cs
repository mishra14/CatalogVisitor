using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace NuGet.CatalogVisitor.Tests
{
    public class TestMessageHandler : HttpMessageHandler
    {
        public ConcurrentDictionary<string, string> Pages { get; } = new ConcurrentDictionary<string, string>();

        public TestMessageHandler()
        {

        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string page = null;
            if (Pages.TryGetValue(request.RequestUri.AbsoluteUri, out page))
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new TestContent(page)
                });
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        }
    }
}
