using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Collections.ObjectModel;

namespace NuGet.CatalogVisitor
{
    public class HttpCatalogVisitor : ICatalogVisitor
    {
        private static CatalogVisitorContext _context = new CatalogVisitorContext();
        private static FileCursor _cursor = new FileCursor();
        private static HttpClient _client = new HttpClient();

        private static readonly List<PackageMetadata> _list = new List<PackageMetadata>();
        private static List<PackageMetadata> _items = new List<PackageMetadata>();

        public HttpCatalogVisitor()
        {

        }

        public HttpCatalogVisitor(CatalogVisitorContext context)
        {
            _context = context;

            var handler = context.MessageHandler ?? new HttpClientHandler();

            _client = new HttpClient(handler);

            //Console.WriteLine("Please enter the file path for your cursor folder: ");
            //var cursor = Console.ReadLine();
            var cursor = "C:\\CatalogCache\\myCursor.txt";
            _cursor.CursorPath = cursor;

            //Console.WriteLine("Please enter the file path for your cache folder: ");
            //C:\\CatalogCache\
            //var contextPath = Console.ReadLine();
            var contextPath = "C:\\CatalogCache\\";
            _context.CatalogCacheFolder = contextPath;
        }

        /// <summary>
        /// Fills private var _list with all id/version pairs as metadata objects.
        /// Caching to disk *with _context.CatalogCacheFolder* (c).
        /// Cursor support *with _cursor* (d).
        /// </summary>
        /// <param name="baseUrl"></param>
        /// <returns></returns>
        public static async Task<HttpCatalogVisitor> FillList(string baseUrl)
        {
            try
            {
                /* Create new HttpCatalogVisitor object to return. */
                CatalogVisitorContext context = new CatalogVisitorContext();
                var v = new HttpCatalogVisitor();
                
                /* Get cursor date, get json string from given uri (main index.json). */
                /* Cursor support (d). */
                _cursor.Load(_cursor.CursorPath);
                var fileDate = _cursor.Date;
                string json = GetCatalogIndexUri(new Uri(baseUrl)).Result;

                /* Parse json string and find second level - catalog - from index page. */
                JObject root = GetJson(baseUrl);
                JArray resources = (JArray)root["resources"];
                string catalogUri = (string)resources.Last["@id"];

                /* Get json from catalog uri found from previous index.json, write to file. */
                root = GetJson(catalogUri);
                Uri baseUri = new Uri(baseUrl);
                var fileName = baseUri.LocalPath.Replace("/", "-");
                var path = _context.CatalogCacheFolder.Substring(0, 15) + '\\' + fileName;
                /* Caching to disk (c). */
                WriteToFileFromFolder(path, json);

                /* Parse json and get list of package urls so we can open 3rd level. */
                JArray items = (JArray)root["items"];
                var pageCommitTime = DateTimeOffset.MinValue;

                /* items.Count when you have time */
                for (int i = 0; i < 5; i++)
                {
                    /* Go through each item in 2nd level and parse out commit time and url, then write to file. */
                    pageCommitTime = items[i]["commitTimeStamp"].ToObject<DateTimeOffset>();
                    Uri newUri = new Uri((string)items[i]["@id"]);
                    fileName = newUri.LocalPath.Replace("/", "-");
                    var cachePath = _context.CatalogCacheFolder.Substring(0, 15) + '\\' + fileName;
                    JObject root2 = null;

                    /* Do nothing if it is older than the cursor and exists. */
                    if (fileDate >= pageCommitTime && File.Exists(cachePath))
                    {
                        Console.WriteLine($"[CACHE] {newUri.AbsoluteUri}");
                    }
                    else
                    {
                        Console.WriteLine($"[GET] {newUri.AbsoluteUri}");

                        /* Get json string in 3rd level from url in 2nd level, then write that to its own file. */
                        var json2 = await GetCatalogIndexUri(new Uri((string)items[i]["@id"]));
                        /* Caching to disk (c). */
                        WriteToFileFromFolder(cachePath, json2);

                        /* If not XML... */
                        if (json2[0] != '<')
                        {
                            /* Parse out list of items to get to 4th level. */
                            root2 = JObject.Parse(json2);
                            JArray tempItems = (JArray)root2["items"];

                            /* Add each item in 4th level to two lists as metadata w ID, version, and commitTimeStamp. */
                            foreach (var item in tempItems)
                            {
                                var metadata = CatalogVisitorContext.GetMetadata((JObject)item);

                                _list.Add(metadata);
                                _items.Add(metadata);
                            }
                        }
                    }
                }
                /* Save cursor file to the time of now, return the HCV object. */
                /* Cursor support (d). */
                _cursor.Save();
                return v;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
        /// <summary>
        /// Gets all packages in _list.
        /// </summary>
        /// <returns></returns>
        public async Task<IReadOnlyList<PackageMetadata>> GetPackages()
        {
            HttpCatalogVisitor myHCV = await FillList("https://api.nuget.org/v3/index.json");
            return _list;
        }

        /// <summary>
        /// Gets packages from when cursor is sitting to now.
        /// Cursor support (d).
        /// </summary>
        /// <param name="cursor"></param>
        /// <returns></returns>
        public Task<IReadOnlyList<PackageMetadata>> GetPackages(FileCursor cursor)
        {
            var packages = GetPackages(cursor.Date, DateTimeOffset.UtcNow);
            cursor.Save();
            return packages;
        }

        /// <summary>
        /// Discovers all rolled up latest entries for each id/version in a date range (b).
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public async Task<IReadOnlyList<PackageMetadata>> GetPackages(DateTimeOffset start, DateTimeOffset end)
        {
            // Call GetPackages and then filter to latest ID version.
            var myList = await GetPackages();
            List<PackageMetadata> newList = new List<PackageMetadata>();
            foreach (var element in myList)
            {
                // Add to empty list if in between start and end dates.
                if (element.CommitTimeStamp >= start && element.CommitTimeStamp <= end)
                {
                    if (newList.Contains(element))
                    {
                        var index = newList.IndexOf(element);
                        var containedElement = newList.ElementAt<PackageMetadata>(index);
                        if (element.Version > containedElement.Version)
                        {
                            // Replace most recent version in list.
                            newList.Remove(containedElement);
                            newList.Add(element);
                        }
                    }
                    else
                    {
                        newList.Add(element);
                    }
                }
            }
            return newList;
        }

        // Figure out globbing!!!
        public async Task<IReadOnlyList<PackageMetadata>> GetPackages(DateTimeOffset start, DateTimeOffset end, string packageIdPattern)
        {
            var myList = await GetPackages();
            List<PackageMetadata> newList = new List<PackageMetadata>();
            foreach (var element in myList)
            {
                // Add to empty list if in between start and end dates.
                if (element.CommitTimeStamp >= start && element.CommitTimeStamp <= end)
                {
                    if (newList.Contains(element))
                    {
                        var index = newList.IndexOf(element);
                        var containedElement = newList.ElementAt<PackageMetadata>(index);
                        if (element.Version > containedElement.Version)
                        {
                            //if (Matches globbing pattern)
                                {
                                // Replace most recent version in list.
                                newList.Remove(containedElement);
                                newList.Add(element);
                            }
                        }
                    }
                    else
                    {
                        //if (Matches globbing pattern)
                        {
                            newList.Add(element);
                        }
                    }
                }
            }
            return newList;
        }

        /// <summary>
        /// Is this different than GetPackages???
        /// </summary>
        /// <returns></returns>
        public async Task<IReadOnlyList<PackageMetadata>> GetRawPackages()
        {
            HttpCatalogVisitor myHCV = await FillList("https://api.nuget.org/v3/index.json");
            return _list;
        }

        /// <summary>
        /// Discover all catalog entries in a date range (a).
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public async Task<IReadOnlyList<PackageMetadata>> GetRawPackages(DateTimeOffset start, DateTimeOffset end)
        {
            // implement this first
            var myList = await GetRawPackages();
            List<PackageMetadata> newList = new List<PackageMetadata>();
            foreach (var element in myList)
            {
                //add to empty list if in between start and end dates
                if (element.CommitTimeStamp >= start && element.CommitTimeStamp <= end)
                {
                    newList.Add(element);
                }
            }
            return newList;
        }

        /// <summary>
        /// Opens file from uri, reads content, returns as string.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static async Task<string> GetCatalogIndexUri(Uri uri)
        {
            using (HttpClient hc = new HttpClient())
            using (HttpResponseMessage response = await hc.GetAsync(uri))
            using (HttpContent content = response.Content)
            {
                if (response.Equals(null))
                {
                    //response.EnsureSuccessStatusCode();
                    return null;
                }
                else
                {
                    return await content.ReadAsStringAsync();
                }
            }
        }

        public static void WriteToFileFromFolder(string folder, string content)
        {
            File.WriteAllText(folder, content);
        }

        /// <summary>
        /// Reads json from a url, returns it as a JObject.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private static JObject GetJson(string url)
        {
            var json = GetCatalogIndexUri(new Uri(url)).Result;
            JObject root = JObject.Parse(json);
            return root;
        }
    }
}
