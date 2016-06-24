using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.IO;

namespace NuGet.CatalogVisitor
{
    public class HttpCatalogVisitor : ICatalogVisitor
    {
        private CatalogVisitorContext _context = new CatalogVisitorContext();
        private FileCursor _cursor = new FileCursor("C:\\CatalogCache\\httpCatalogVisitor.txt", DateTimeOffset.MinValue);
        private HttpClient _client;

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
            //_cursor.CursorPath = cursor;

            //Console.WriteLine("Please enter the file path for your cache folder: ");
            //C:\\CatalogCache\
            //var contextPath = Console.ReadLine();
            var contextPath = "C:\\CatalogCache\\";
            _context.CatalogCacheFolder = contextPath;
            _context.FeedIndexJsonUrl = "https://api.nuget.org/v3/index.json";
        }



        /// <summary>
        /// Gets all packages latest edit of each version.
        /// </summary>
        /// <returns></returns>
        public Task<IReadOnlyList<PackageMetadata>> GetPackages()
        {
            return GetPackages(DateTimeOffset.MinValue, DateTimeOffset.UtcNow);
        }

        /// <summary>
        /// Gets packages from when cursor is sitting to now.
        /// Cursor support (d).
        /// </summary>
        /// <param name="cursor"></param>
        /// <returns></returns>
        public Task<IReadOnlyList<PackageMetadata>> GetPackages(ICursor cursor)
        {
            return GetPackages(cursor.Date, DateTimeOffset.UtcNow);
        }

        /// <summary>
        /// FOR TESTING ONLY.
        /// Gets packages from when cursor is sitting to now.
        /// Cursor support (d).
        /// </summary>
        /// <param name="cursor"></param>
        /// <returns></returns>s
        public IReadOnlyList<PackageMetadata> GetPackagesDisk(ICursor cursor)
        {
            return GetPackagesDisk(cursor.Date, DateTimeOffset.UtcNow);
        }

        /// <summary>
        /// Discovers all rolled up latest entries for each id/version in a date range (b).
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public async Task<IReadOnlyList<PackageMetadata>> GetPackages(DateTimeOffset start, DateTimeOffset end)
        {
            try
            {
                /* Create new HttpCatalogVisitor object to return. */
                List<PackageMetadata> newList = new List<PackageMetadata>();

                /* Get cursor date, get json string from given uri (main index.json). */
                /* Cursor support (d). */
                //FileCursor.Load(_cursor.CursorPath);
                //var fileDate = _cursor.Date;
                string json = await GetCatalogIndexUri(new Uri(_context.FeedIndexJsonUrl));

                /* Parse json string and find second level - catalog - from index page. */
                JObject root = await GetJson(_context.FeedIndexJsonUrl);
                JArray resources = (JArray)root["resources"];
                string catalogUri = (string)resources.Last["@id"];

                /* Get json from catalog uri found from previous index.json, write to file. */
                root = await GetJson(catalogUri);
                Uri baseUri = new Uri(_context.FeedIndexJsonUrl);
                var fileName = baseUri.LocalPath.Replace("/", "-");
                //var path = _context.CatalogCacheFolder + fileName;
                /* Caching to disk (c). */
                //WriteToFileFromFolder(path, json);

                /* Parse json and get list of package urls so we can open 3rd level. */
                JArray items = (JArray)root["items"];
                var pageCommitTime = DateTimeOffset.MinValue;

                /* items.Count when you have time */
                for (int i = 0; i < items.Count; i++)
                {
                    /* Go through each item in 2nd level and parse out commit time and url, then write to file. */
                    pageCommitTime = items[i]["commitTimeStamp"].ToObject<DateTimeOffset>();
                    Uri newUri = new Uri((string)items[i]["@id"]);
                    fileName = newUri.LocalPath.Replace("/", "-");
                    var cachePath = _context.CatalogCacheFolder + fileName;
                    JObject root2 = null;

                    /* Do nothing if it is older than the cursor and exists. */
                    if (start < pageCommitTime && end >= pageCommitTime)
                    {
                        bool useCache = false;

                        // Check if we already have this file
                        if (File.Exists(cachePath))
                        {
                            var timeStamp = File.GetLastWriteTimeUtc(cachePath);

                            // If the timestamp hasn't changed we can use the same file
                            useCache = timeStamp >= pageCommitTime;
                        }

                        string json2 = null;

                        if (useCache)
                        {
                            Console.WriteLine($"[CACHE] {newUri.AbsoluteUri}");

                            json2 = File.ReadAllText(cachePath);
                        }
                        else
                        {
                            Console.WriteLine($"[ADDING] {newUri.AbsoluteUri}");

                            /* Get json string in 3rd level from url in 2nd level, then write that to its own file. */
                            json2 = await GetCatalogIndexUri(new Uri((string)items[i]["@id"]));
                            /* Caching to disk (c). */
                            WriteCacheFile(cachePath, json2, pageCommitTime);
                        }

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

                                /* If within the dates specified. */
                                if (metadata.CommitTimeStamp >= start && metadata.CommitTimeStamp <= end)
                                {
                                    // Add to empty list if in between start and end dates.
                                    if (metadata.CommitTimeStamp >= start && metadata.CommitTimeStamp <= end)
                                    {
                                        if (newList.Contains(metadata))
                                        {
                                            var index = newList.IndexOf(metadata);
                                            var containedElement = newList.ElementAt<PackageMetadata>(index);
                                            /* Add latest addition of each version. */
                                            if (metadata.Version.Equals(containedElement.Version) && metadata.CommitTimeStamp > containedElement.CommitTimeStamp)
                                            {
                                                newList.Remove(containedElement);
                                                newList.Add(metadata);
                                            }
                                        }
                                        else
                                        {
                                            newList.Add(metadata);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    /* Save cursor file to the time of now, return the HCV object. */
                    /* Cursor support (d). */
                    //_cursor.Save();                   
                }

                return newList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// FOR TESTING.
        /// Discovers all rolled up latest entries for each id/version in a date range (b).
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public IReadOnlyList<PackageMetadata> GetPackagesDisk(DateTimeOffset start, DateTimeOffset end)
        {
            //try
            {
                /* Create new HttpCatalogVisitor object to return. */
                List<PackageMetadata> newList = new List<PackageMetadata>();
                string baseUrl = "C:\\CatalogCache\\TestJson\\index.json";
                string ourCatalogCacheFolder = "C:\\CatalogCache\\TestJson\\";

                /* Get cursor date, get json string from given uri (main index.json). */
                /* Cursor support (d). */
                FileCursor.Load(_cursor.CursorPath);
                var fileDate = _cursor.Date;
                string json = File.ReadAllText(baseUrl);

                /* Parse json string and find second level - catalog - from index page. */
                JObject root = JObject.Parse(json);
                JArray resources = (JArray)root["resources"];
                string catalogUri = (string)resources[3]["@id"];

                /* Get json from catalog uri found from previous index.json, write to file. */
                var fileContent2 = File.ReadAllText(catalogUri);
                root = JObject.Parse(fileContent2);
                Uri baseUri = new Uri(baseUrl);
                var fileName = baseUri.LocalPath.Replace("\\", "-");
                fileName = fileName.Substring(3);
                var path = ourCatalogCacheFolder + fileName;
                /* Caching to disk (c). */
                WriteToFileFromFolder(path, json);

                /* Parse json and get list of package urls so we can open 3rd level. */
                JArray items = (JArray)root["items"];
                var pageCommitTime = DateTimeOffset.MinValue;

                /* items.Count when you have time */
                for (int i = 0; i < 4; i++)
                {
                    /* Go through each item in 2nd level and parse out commit time and url, then write to file. */
                    pageCommitTime = items[i]["commitTimeStamp"].ToObject<DateTimeOffset>();
                    Uri newUri = new Uri((string)items[i]["@id"]);
                    fileName = newUri.LocalPath.Replace("\\", "-");
                    fileName = fileName.Substring(3);
                    var cachePath = ourCatalogCacheFolder + fileName;
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
                        var json2 = File.ReadAllText((string)items[i]["@id"]);
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

                                /* If within the dates specified. */
                                if (metadata.CommitTimeStamp >= start && metadata.CommitTimeStamp <= end)
                                {
                                    // Add to empty list if in between start and end dates.
                                    if (metadata.CommitTimeStamp >= start && metadata.CommitTimeStamp <= end)
                                    {
                                        if (newList.Contains(metadata))
                                        {
                                            var index = newList.IndexOf(metadata);
                                            var containedElement = newList.ElementAt<PackageMetadata>(index);
                                            /* Add latest addition of each version. */
                                            if (metadata.Version.Equals(containedElement.Version) && metadata.CommitTimeStamp > containedElement.CommitTimeStamp)
                                            {
                                                newList.Remove(containedElement);
                                                newList.Add(metadata);
                                            }
                                        }
                                        else
                                        {
                                            newList.Add(metadata);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                /* Save cursor file to the time of now, return the HCV object. */
                /* Cursor support (d). */
                _cursor.Save();
                return newList;
            }
            //catch (Exception ex)
            {
                //throw ex;
            }
        }


        // Figure out globbing!!! (implement by mon, 6/27)
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
        /// Gets all packages.
        /// </summary>
        /// <returns></returns>
        public Task<IReadOnlyList<PackageMetadata>> GetRawPackages()
        {
            return GetRawPackages(DateTimeOffset.MinValue, DateTimeOffset.UtcNow);
        }

        /// <summary>
        /// Discover all catalog entries in a date range (a).
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public async Task<IReadOnlyList<PackageMetadata>> GetRawPackages(DateTimeOffset start, DateTimeOffset end)
        {
            try
            {
                /* Create new HttpCatalogVisitor object to return. */
                List<PackageMetadata> newList = new List<PackageMetadata>();
                string baseUrl = "https://api.nuget.org/v3/index.json";

                /* Get cursor date, get json string from given uri (main index.json). */
                /* Cursor support (d). */
                FileCursor.Load(_cursor.CursorPath);
                var fileDate = _cursor.Date;
                string json = await GetCatalogIndexUri(new Uri(baseUrl));

                /* Parse json string and find second level - catalog - from index page. */
                JObject root = await GetJson(baseUrl);
                JArray resources = (JArray)root["resources"];
                string catalogUri = (string)resources.Last["@id"];

                /* Get json from catalog uri found from previous index.json, write to file. */
                root = await GetJson(catalogUri);
                Uri baseUri = new Uri(baseUrl);
                var fileName = baseUri.LocalPath.Replace("/", "-");
                var path = _context.CatalogCacheFolder + fileName;
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
                    var cachePath = _context.CatalogCacheFolder + fileName;
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

                                /* If within the dates specified. */
                                if (metadata.CommitTimeStamp >= start && metadata.CommitTimeStamp <= end)
                                {
                                    newList.Add(metadata);
                                }
                            }
                        }
                    }
                }
                /* Save cursor file to the time of now, return the HCV object. */
                /* Cursor support (d). */
                _cursor.Save();
                return newList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// FOR TESTING.
        /// Discover all catalog entries in a date range (a) (w file on disk instead of online).
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public IReadOnlyList<PackageMetadata> GetRawPackagesDisk(DateTimeOffset start, DateTimeOffset end)
        {
            //try
            {
                /* Create new HttpCatalogVisitor object to return. */
                List<PackageMetadata> newList = new List<PackageMetadata>();
                string baseUrl = "C:\\CatalogCache\\TestJson\\index.json";
                string ourCatalogCacheFolder = "C:\\CatalogCache\\TestJson\\";

                /* Get cursor date, get json string from given uri (main index.json). */
                /* Cursor support (d). */
                FileCursor.Load(_cursor.CursorPath);
                var fileDate = _cursor.Date;
                string json = File.ReadAllText(baseUrl);

                /* Parse json string and find second level - catalog - from index page. */
                JObject root = JObject.Parse(json);
                JArray resources = (JArray)root["resources"];
                string catalogUri = (string)resources[3]["@id"];

                /* Get json from catalog uri found from previous index.json, write to file. */
                var fileContent2 = File.ReadAllText(catalogUri);
                root = JObject.Parse(fileContent2);
                Uri baseUri = new Uri(baseUrl);
                var fileName = baseUri.LocalPath.Replace("\\", "-");
                fileName = fileName.Substring(3);
                var path = ourCatalogCacheFolder + fileName;
                /* Caching to disk (c). */
                WriteToFileFromFolder(path, json);

                /* Parse json and get list of package urls so we can open 3rd level. */
                JArray items = (JArray)root["items"];
                var pageCommitTime = DateTimeOffset.MinValue;

                /* items.Count when you have time */
                for (int i = 0; i < 4; i++)
                {
                    /* Go through each item in 2nd level and parse out commit time and url, then write to file. */
                    pageCommitTime = items[i]["commitTimeStamp"].ToObject<DateTimeOffset>();
                    Uri newUri = new Uri((string)items[i]["@id"]);
                    fileName = newUri.LocalPath.Replace("\\", "-");
                    fileName = fileName.Substring(3);
                    var cachePath = ourCatalogCacheFolder + fileName;
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
                        var json2 = File.ReadAllText((string)items[i]["@id"]);
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

                                /* If within the dates specified. */
                                if (metadata.CommitTimeStamp >= start && metadata.CommitTimeStamp <= end)
                                {
                                    newList.Add(metadata);
                                }
                            }
                        }
                    }
                }
                /* Save cursor file to the time of now, return the HCV object. */
                /* Cursor support (d). */
                _cursor.Save();
                return newList;
            }
            //catch (Exception ex)
            {
                //throw ex;
            }
        }

        /// <summary>
        /// Opens file from uri, reads content, returns as string.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public async Task<string> GetCatalogIndexUri(Uri uri)
        {
            using (HttpResponseMessage response = await _client.GetAsync(uri))
            {
                if (response.Equals(null))
                {
                    //response.EnsureSuccessStatusCode();
                    return null;
                }
                else
                {
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }

        public static void WriteToFileFromFolder(string file, string content)
        {
            File.WriteAllText(file, content);
        }

        public static void WriteCacheFile(string file, string content, DateTimeOffset writeTime)
        {
            File.WriteAllText(file, content);
            File.SetLastWriteTimeUtc(file, writeTime.UtcDateTime);
        }

        /// <summary>
        /// Reads json from a url, returns it as a JObject.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private async Task<JObject> GetJson(string url)
        {
            var json = await GetCatalogIndexUri(new Uri(url));
            JObject root = JObject.Parse(json);
            return root;
        }
    }
}
