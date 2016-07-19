using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace NuGet.CatalogVisitor
{
    public class HttpCatalogVisitor : ICatalogVisitor
    {
        private CatalogVisitorContext _context = new CatalogVisitorContext();
        private HttpClient _client;

        /// <summary>
        /// A class that has many functions that visit the catalog between specified dates
        /// (or none at all) and does things to them (returns the packages, etc.).
        /// </summary>
        /// <param name="context">User tells the class where to get JSON from, etc.</param>
        public HttpCatalogVisitor(CatalogVisitorContext context)
        {
            _context = context;

            var handler = context.MessageHandler ?? new HttpClientHandler();

            _client = new HttpClient(handler);
        }

        /// <summary>
        /// Gets all packages' latest edit of each version.
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
        /// From: http://stackoverflow.com/questions/6907720/need-to-perform-wildcard-etc-search-on-a-string-using-regex
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static string WildcardToRegex(string pattern)
        {
            return "^" + Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".") + "$";
        }

        /// <summary>
        /// Return all packages within two dates that match a globbing pattern.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="packageIdPattern"></param>
        /// <returns></returns>
        public async Task<IReadOnlyList<PackageMetadata>> GetPackages(DateTimeOffset start, DateTimeOffset end, string packageIdPattern)
        {
            //Matcher myMatcher = new Matcher();
            //myMatcher.AddInclude(packageIdPattern);
            //var validFiles = Globbing.GetFile(packageIdPattern);

            string regexPattern = WildcardToRegex(packageIdPattern.ToLower());
            /* From: http://www.dotnetperls.com/regex-file */
            Regex g = new Regex(regexPattern);

            //GetPackages but check if == fpm before adding to newList
            try
            {
                /* Create new HttpCatalogVisitor object to return. */
            List<PackageMetadata> newList = new List<PackageMetadata>();

                string json = await GetCatalogIndexUri(new Uri(_context.FeedIndexJsonUrl));

                /* Parse json string and find second level - catalog - from index page. */
                JObject root = await GetJson(_context.FeedIndexJsonUrl);
                JArray resources = (JArray)root["resources"];
                string catalogUri = (string)resources.Last["@id"];

                /* Get json from catalog uri found from previous index.json, write to file. */
                root = await GetJson(catalogUri);
                Uri baseUri = new Uri(_context.FeedIndexJsonUrl);
                var fileName = baseUri.LocalPath.Replace("/", "-");
                var path = _context.CatalogCacheFolder + fileName;
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
                    bool useCache = true;
                    string json2 = null;

                    /* Do nothing if it is older than the cursor and exists. */
                    if (start < pageCommitTime && end >= pageCommitTime)
                    {
                        useCache = false;
                    }

                    if (useCache)
                    {
                        Console.WriteLine($"[CACHE] {newUri.AbsoluteUri}");
                    }
                    else
                    {
                        Console.WriteLine($"[ADDING] {newUri.AbsoluteUri}");

                        /* Get json string in 3rd level from url in 2nd level, then write that to its own file. */
                        json2 = await GetCatalogIndexUri(new Uri((string)items[i]["@id"]));
                        /* Caching to disk (c). */
                        //WriteCacheFile(cachePath, json2, pageCommitTime);

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

                                var url = (string)item["@id"];
                                var dir = url.Substring(59);
                                //DirectoryInfo hi = new DirectoryInfo(dir);
                                //DirectoryInfoWrapper hello = new DirectoryInfoWrapper(hi);
                                //PatternMatchingResult result = myMatcher.Execute(hello);
                                /* If the url matches the pattern they pass in. */
                                /* result.Files = list of FilePatternMatch, compare to url to see if it is a match? */
                                //foreach (var file in result.Files)
                                //{
                                //    Console.WriteLine($"file: {file}");
                                //}
                                //if (validFiles.Contains(dir))

                                Match m = g.Match(dir);
                                if (m.Success)
                                {
                                    // Add to empty list if in between start and end dates.
                                    if (metadata.CommitTimeStamp > start && metadata.CommitTimeStamp <= end)
                                    {
                                        if (newList.Contains(metadata))
                                        {
                                            var index = newList.IndexOf(metadata);
                                            var containedElement = newList.ElementAt(index);
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

                return newList;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
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
                
                string json = await GetCatalogIndexUri(new Uri(_context.FeedIndexJsonUrl));

                /* Parse json string and find second level - catalog - from index page. */
                JObject root = await GetJson(_context.FeedIndexJsonUrl);
                JArray resources = (JArray)root["resources"];
                string catalogUri = (string)resources.Last["@id"];

                /* Get json from catalog uri found from previous index.json, write to file. */
                root = await GetJson(catalogUri);
                Uri baseUri = new Uri(_context.FeedIndexJsonUrl);
                var fileName = baseUri.LocalPath.Replace("/", "-");
                var path = _context.CatalogCacheFolder + fileName;
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
                    bool useCache = true;
                    string json2 = null;

                    /* Do nothing if it is older than the cursor and exists. */
                    if (start < pageCommitTime && end >= pageCommitTime)
                    {
                        useCache = false;
                    }

                    if (useCache)
                    {
                        Console.WriteLine($"[CACHE] {newUri.AbsoluteUri}");
                    }
                    else
                    {
                        Console.WriteLine($"[ADDING] {newUri.AbsoluteUri}");

                        /* Get json string in 3rd level from url in 2nd level, then write that to its own file. */
                        json2 = await GetCatalogIndexUri(new Uri((string)items[i]["@id"]));
                        /* Caching to disk (c). */
                        //WriteCacheFile(cachePath, json2, pageCommitTime);

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

                                // Add to empty list if in between start and end dates.
                                if (metadata.CommitTimeStamp > start && metadata.CommitTimeStamp <= end)
                                {
                                    if (newList.Contains(metadata))
                                    {
                                        var index = newList.IndexOf(metadata);
                                        var containedElement = newList.ElementAt(index);
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

                return newList;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        ///// <summary>
        ///// Return all packages within two dates that match a globbing pattern.
        ///// </summary>
        ///// <param name="start"></param>
        ///// <param name="end"></param>
        ///// <param name="packageIdPattern"></param>
        ///// <returns></returns>
        //public async Task<IReadOnlyList<PackageMetadata>> GetPackages(DateTimeOffset start, DateTimeOffset end, string packageIdPattern)
        //{
        //    /* New directory on their machine containing these packages in this date range. */
        //    Directory.CreateDirectory("C://GlobbingResults//");
        //    HttpPackageDownloader hpd = new HttpPackageDownloader(_context);
        //    await hpd.DownloadPackagesDateRange(start, end, "C://GlobbingResults//");
        //    /* Get all potential files in that date range. */
        //    IReadOnlyList<PackageMetadata> filesToMatch = await GetPackages(start, end);
        //    /* Get all files matching pattern from directory made and downloaded to. */
        //    string[] matchingFiles = Directory.GetFiles("C://GlobbingResults//", packageIdPattern);
        //    List<PackageMetadata> newList = new List<PackageMetadata>();

        //    for (int i = 0; i < matchingFiles.Length; i++)
        //    {
        //        foreach (var fileMatch in filesToMatch)
        //        {
        //            var tempPath = "C://GlobbingResults//" + fileMatch.Id.Replace(".", "-") + fileMatch.Version.ToNormalizedString().Replace(".", "-") + ".nupkg";
        //            if (tempPath.Equals(matchingFiles[i]))
        //            {
        //                newList.Add(fileMatch);
        //            }
        //        }
        //    }

        //    /* Delete files in and directory off their hard drive to save space. */
        //    DirectoryInfo di = new DirectoryInfo("C://GlobbingResults//");
        //    foreach (FileInfo file in di.GetFiles())
        //    {
        //        file.Delete();
        //    }
        //    Directory.Delete("C://GlobbingResults//");
        //    return newList;
        //}

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

                string json = await GetCatalogIndexUri(new Uri(_context.FeedIndexJsonUrl));

                /* Parse json string and find second level - catalog - from index page. */
                JObject root = await GetJson(_context.FeedIndexJsonUrl);
                JArray resources = (JArray)root["resources"];
                string catalogUri = (string)resources.Last["@id"];

                /* Get json from catalog uri found from previous index.json, write to file. */
                root = await GetJson(catalogUri);
                Uri baseUri = new Uri(_context.FeedIndexJsonUrl);
                var fileName = baseUri.LocalPath.Replace("/", "-");
                var path = _context.CatalogCacheFolder + fileName;
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
                    bool useCache = true;

                    /* Do nothing if it is older than the cursor and exists. */
                    if (start < pageCommitTime && end >= pageCommitTime)
                    {
                        useCache = false;
                    }

                    string json2 = null;

                    if (useCache)
                    {
                        Console.WriteLine($"[CACHE] {newUri.AbsoluteUri}");
                    }
                    else
                    {
                        Console.WriteLine($"[ADDING] {newUri.AbsoluteUri}");

                        /* Get json string in 3rd level from url in 2nd level, then write that to its own file. */
                        json2 = await GetCatalogIndexUri(new Uri((string)items[i]["@id"]));
                        /* Caching to disk (c). */
                        //WriteCacheFile(cachePath, json2, pageCommitTime);
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

                            // Add to empty list if in between start and end dates.
                            if (metadata.CommitTimeStamp >= start && metadata.CommitTimeStamp <= end)
                            {
                                newList.Add(metadata);
                            }
                        }
                    }
                } 

                return newList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Opens file from uri, reads content, returns as string.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public async Task<string> GetCatalogIndexUri(Uri uri)
        {
            Debug.Assert(uri.AbsoluteUri.StartsWith("http", StringComparison.OrdinalIgnoreCase), $"Invalid URL: {uri.AbsoluteUri}");

            using (HttpResponseMessage response = await _client.GetAsync(uri))
            {
                if (response.Equals(null))
                {
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
