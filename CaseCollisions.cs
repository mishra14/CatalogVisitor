using Newtonsoft.Json.Linq;
using NuGet.Packaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NuGet.CatalogVisitor
{
    class CaseCollisions
    {
        private static List<DateTimeOffset> _dateTimes = new List<DateTimeOffset>();
        private static readonly List<PackageData> _list4 = new List<PackageData>();
        private static readonly List<string> _list5 = new List<string>();

        public CaseCollisions()
        {
            PopulateDateTimes();
        }

        public static void PopulateDateTimes()
        {
            for (int i = 0; i < 250; i++)
            {
                _dateTimes.Add(DateTimeOffset.Now);
            }
        }

        private static string CursorPath = "C:\\CatalogCache\\cursor2.txt";

        public static DateTimeOffset GetCursor()
        {
            DateTimeOffset fileDate = DateTimeOffset.Now;

            if (File.Exists(CursorPath))
            {
                var cursorText = File.ReadAllText(CursorPath);
                fileDate = DateTimeOffset.Parse(cursorText);
            }

            return fileDate;
        }

        public static void SaveCursor(DateTimeOffset date)
        {
            try
            {
                File.WriteAllText(CursorPath, date.ToString("o"));
            }
            catch
            {
                // fix this later
            }
        }

        public static async Task<HttpCatalogVisitor> FlagAllDiffIDs()
        {
            var v = new HttpCatalogVisitor();
            _dateTimes.Clear();
            var fileDate = GetCursor();
            List<string> fileContent = new List<string>();

            JObject rankingsJson = JObject.Parse(File.ReadAllText("C:\\Users\\t-kaswan\\Downloads\\rankings.v1.json"));
            JArray ranks = (JArray)rankingsJson["Rank"];
            fileContent = ranks.Select(item => item.ToString()).ToList();
            int count3 = 0;
            int collisions = 0;

            foreach (var elem in fileContent)
            {
                string json;
                var element = elem;
                string url = "https://api.nuget.org/v3-flatcontainer/" + element.ToLower() + "/index.json";
                json = await HttpCatalogVisitor.GetContent(url);
                JObject root = JObject.Parse(json);
                JArray versions = (JArray)root["versions"];
                foreach (var version in versions)
                {
                    string tempUrl = "https://api.nuget.org/v3-flatcontainer/" + element.ToLower() + '/' + version.ToString().ToLower() + '/' + element.ToLower() + ".nuspec";
                    if (fileDate >= _dateTimes[count3] && File.Exists(tempUrl))
                    {
                        Console.WriteLine($"[CACHE] {tempUrl}");
                    }
                    else
                    {
                        Console.WriteLine($"[GET] {tempUrl}");

                        var nuspecString = await HttpCatalogVisitor.GetContent(tempUrl);
                        //trouble!!!

                        if (!nuspecString.Equals(null))
                        {
                            var xml = XDocument.Parse(nuspecString);
                            var nuspec = new NuspecReader(xml);
                            var id = nuspec.GetId();



                            if (_list5.Contains(id.ToLower()))
                            {
                                foreach (var el in _list4)
                                {
                                    if (id.ToLower().Equals(el.Id.ToLower()))
                                    {
                                        Console.WriteLine("adding to hashset: {0}", id);
                                        el.AddToHashset(id);
                                        //if more than one id in hashset
                                        if (el.Hashset.Count > 1)
                                        {
                                            collisions++;
                                            Console.WriteLine("Collision - ID: {0}, Hashset: {1}", el.Id, el.Hashset.ToString());
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine("list 4 : {0}", id);
                                _list4.Add(new PackageData(id));
                                _list5.Add(id.ToLower());
                            }

                            Console.WriteLine("Count: {0}", count3);
                            Console.WriteLine("Collisions: {0}", collisions);
                        }

                    }
                }
                count3++;
            }

            var packages = _list4;

            Console.WriteLine("FileContent.Count: {0}", fileContent.Count);
            Console.WriteLine("packages.empty?: {0}", packages.IsEmpty());

            //go over each ID in data structure
            foreach (var package in packages)
            {
                //if more than one id in hashset
                if (package.Hashset.Count > 1)
                {
                    var ids = string.Join(", ", package.Hashset);
                    Console.WriteLine("Collision - ID: {0}, Hashset: {1}", package.Id, ids);
                }
            }

            SaveCursor(DateTimeOffset.Now);

            return v;
        }
    }
}
