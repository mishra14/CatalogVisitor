using Newtonsoft.Json.Linq;
using NuGet;
using NuGet.CatalogVisitor;
using NuGet.Packaging;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

public class HttpCatalogVisitor : ICatalogVisitor
{
    private static readonly List<PackageMetadata> _list = new List<PackageMetadata>();
    private static readonly List<PackageData> _list2 = new List<PackageData>();
    private static readonly List<string> _list3 = new List<string>();
    private static readonly List<PackageData> _list4 = new List<PackageData>();
    private static readonly List<string> _list5 = new List<string>();
    //private static string _json2;
    //private static JObject _root2;
    private static List<PackageMetadata> _items2 = new List<PackageMetadata>();
    private static List<DateTimeOffset> _dateTimes = new List<DateTimeOffset>();

    public HttpCatalogVisitor()
    {

    }

    public IEnumerable<PackageMetadata> GetPackages()
    {
        return _list;
    }

    public static void PopulateDateTimes()
    {
        for (int i = 0; i < 250; i++)
        {
            _dateTimes.Add(DateTimeOffset.Now);
        }
    }

    //public static List<Tuple<string, NuGetVersion>> FlagAllDiffIDs()
    //{
    //    List<Tuple<string, NuGetVersion>> retList = new List<Tuple<string, NuGetVersion>>();

    //    //load the file of top 250 IDs, only take in 100
    //    using (var stream = File.Open("C:\\Users\\t-kaswan\\Downloads\\rankings.v1.json", FileMode.Open))
    //    {
    //        byte[] b = new byte[1024];
    //        UTF8Encoding temp = new UTF8Encoding(true);
    //        List<string> fileContent = new List<string>();
    //        while (stream.Read(b, 0, 250) > 0)
    //        {
    //            fileContent.Add(temp.GetString(b));
    //        }

    //        //load data structure (~30 min)
    //        var visitor = CreateHCV(new Uri("https://api.nuget.org/v3/index.json")).Result;
    //        var packages = visitor.GetPackages();

    //        Console.WriteLine("FileContent.Count: {0}", fileContent.Count);
    //        Console.WriteLine("packages.empty?: {0}", packages.IsEmpty());

    //        //go over each ID in data structure
    //        foreach (var element in fileContent)
    //        {
    //            foreach (var package in packages)
    //            {
    //                //if match
    //                if (package.Id.ToLower().Equals(element.ToLower()))
    //                {
    //                    Console.Write("package ID: {0}", package.Id);
    //                    //if case sensitive mismatch
    //                    if (!package.Id.Equals(element))
    //                    {
    //                        Console.WriteLine("Found first mismatch!");
    //                        Environment.Exit(1);
    //                        //retList.Add(new Tuple<string, NuGetVersion>(package.Id, package.Version));
    //                    }
    //                }
    //            }
    //        }
    //    }
    //    //return list
    //    return retList;
    //}

    //public static List<Tuple<string, NuGetVersion>> FlagAllDiffIDs2()
    //{
    //    List<Tuple<string, NuGetVersion>> retList = new List<Tuple<string, NuGetVersion>>();

    //    //load the file of top 250 IDs, only take in 100
    //    using (var stream = File.Open("C:\\Users\\t-kaswan\\Downloads\\rankings.v1.json", FileMode.Open))
    //    {
    //        byte[] b = new byte[1024];
    //        UTF8Encoding temp = new UTF8Encoding(true);
    //        List<string> fileContent = new List<string>();
    //        while (stream.Read(b, 0, 250) > 0)
    //        {
    //            fileContent.Add(temp.GetString(b));
    //        }

    //        //load data structure (~30 min)
    //        var visitor = CreateDiffHCV(new Uri("https://api.nuget.org/v3/index.json")).Result;
    //        var packages = _list2;

    //        Console.WriteLine("FileContent.Count: {0}", fileContent.Count);
    //        Console.WriteLine("packages.empty?: {0}", packages.IsEmpty());

    //        //go over each ID in data structure
    //        foreach (var package in packages)
    //        {
    //            //if more than one id in hashset
    //            if (package.Hashset.Count > 1)
    //            {
    //                Console.WriteLine("Collision - ID: {0}, Hashset: {1}", package.Id, package.Hashset.ToString());
    //                NuGetVersion tempVersion = new NuGetVersion("1.0.0");
    //                Tuple<string, NuGetVersion> tempTuple = new Tuple<string, NuGetVersion>(package.Id, tempVersion);
    //                retList.Add(tempTuple);
    //            }
    //        }
    //    }
    //    //return list
    //    return retList;
    //}

    public static Task<string> GetContent(string url)
    {
        return GetContentUri(new Uri(url));
    }


    public static async Task<string> GetContentUri(Uri uri)
    {
        using (System.Net.Http.HttpClient hc = new System.Net.Http.HttpClient())
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
    

    //public static async Task<HttpCatalogVisitor> FlagAllDiffIDs3()
    //{
    //    var v = new HttpCatalogVisitor();
    //    _dateTimes.Clear();
    //    var fileDate = GetCursor2();
    //    List<string> fileContent = new List<string>();

    //    JObject rankingsJson = JObject.Parse(File.ReadAllText("C:\\Users\\t-kaswan\\Downloads\\rankings.v1.json"));
    //    JArray ranks = (JArray)rankingsJson["Rank"];
    //    fileContent = ranks.Select(item => item.ToString()).ToList();
    //    int count3 = 0;
    //    int collisions = 0;

    //    foreach (var elem in fileContent)
    //    {
    //        string json;
    //        var element = elem;
    //        string url = "https://api.nuget.org/v3-flatcontainer/" + element.ToLower() + "/index.json";
    //        json = await GetContent(url);
    //        JObject root = JObject.Parse(json);
    //        JArray versions = (JArray)root["versions"];
    //        foreach (var version in versions)
    //        {
    //            string tempUrl = "https://api.nuget.org/v3-flatcontainer/" + element.ToLower() + '/' + version.ToString().ToLower() + '/' + element.ToLower() + ".nuspec";
    //            if (fileDate >= _dateTimes[count3] && File.Exists(tempUrl))
    //            {
    //                Console.WriteLine($"[CACHE] {tempUrl}");
    //            }
    //            else
    //            {
    //                Console.WriteLine($"[GET] {tempUrl}");

    //                var nuspecString = await GetContent(tempUrl);
    //                //trouble!!!

    //                if (!nuspecString.Equals(null))
    //                {
    //                    var xml = XDocument.Parse(nuspecString);
    //                    var nuspec = new NuspecReader(xml);
    //                    var id = nuspec.GetId();



    //                    if (_list5.Contains(id.ToLower()))
    //                    {
    //                        foreach (var el in _list4)
    //                        {
    //                            if (id.ToLower().Equals(el.Id.ToLower()))
    //                            {
    //                                Console.WriteLine("adding to hashset: {0}", id);
    //                                el.AddToHashset(id);
    //                                //if more than one id in hashset
    //                                if (el.Hashset.Count > 1)
    //                                {
    //                                    collisions++;
    //                                    Console.WriteLine("Collision - ID: {0}, Hashset: {1}", el.Id, el.Hashset.ToString());
    //                                }
    //                            }
    //                        }
    //                    }
    //                    else
    //                    {
    //                        Console.WriteLine("list 4 : {0}", id);
    //                        _list4.Add(new PackageData(id));
    //                        _list5.Add(id.ToLower());
    //                    }

    //                    Console.WriteLine("Count: {0}", count3);
    //                    Console.WriteLine("Collisions: {0}", collisions);
    //                }

    //            }
    //        }
    //        count3++;
    //    }

    //    var packages = _list4;

    //    Console.WriteLine("FileContent.Count: {0}", fileContent.Count);
    //    Console.WriteLine("packages.empty?: {0}", packages.IsEmpty());

    //    //go over each ID in data structure
    //    foreach (var package in packages)
    //    {
    //        //if more than one id in hashset
    //        if (package.Hashset.Count > 1)
    //        {
    //            var ids = string.Join(", ", package.Hashset);
    //            Console.WriteLine("Collision - ID: {0}, Hashset: {1}", package.Id, ids);
    //        }
    //    }

    //    SaveCursor2(DateTimeOffset.Now);

    //    return v;
    //}

    private static string CursorPath = "C:\\CatalogCache\\cursor.txt";
    private static string CursorPath2 = "C:\\CatalogCache\\cursor2.txt";

    public static DateTimeOffset GetMinCursor()
    {
        DateTimeOffset fileDate = DateTimeOffset.MinValue;

        if (File.Exists(CursorPath))
        {
            var cursorText = File.ReadAllText(CursorPath);
            fileDate = DateTimeOffset.Parse(cursorText);
        }

        return fileDate;
    }

    public static DateTimeOffset GetCursor2()
    {
        DateTimeOffset fileDate = DateTimeOffset.Now;

        if (File.Exists(CursorPath2))
        {
            var cursorText = File.ReadAllText(CursorPath2);
            fileDate = DateTimeOffset.Parse(cursorText);
        }

        return fileDate;
    }


    public static DateTimeOffset GetNowCursor()
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

    public static void SaveCursor2(DateTimeOffset date)
    {
        try
        {
            File.WriteAllText(CursorPath2, date.ToString("o"));
        }
        catch
        {
            // fix this later
        }
    }

    public static async Task<HttpCatalogVisitor> CreateHCV(Uri baseUri)
    {
        try
        {
            string json;

            var v = new HttpCatalogVisitor();
            var fileDate = GetMinCursor();

            json = await GetContentUri(baseUri);
            JObject root = JObject.Parse(json);
            JArray resources = (JArray)root["resources"];
            string catalogUri = (string)resources.Last["@id"];
            json = await GetContent(catalogUri);
            var fileName = baseUri.LocalPath.Replace("/", "-");
            var path = $"C:\\CatalogCache\\{fileName}";
            File.WriteAllText(path, json);

            root = JObject.Parse(json);
            JArray items = (JArray)root["items"];
            var pageCommitTime = DateTimeOffset.MinValue;
            /* items.Count when you have time */
            for (int i = 0; i < items.Count; i++)
            {
                pageCommitTime = items[i]["commitTimeStamp"].ToObject<DateTimeOffset>();
                Uri newUri = new Uri((string)items[i]["@id"]);
                fileName = newUri.LocalPath.Replace("/", "-");
                var cachePath = path = $"C:\\CatalogCache\\{fileName}";

                JObject root2 = null;

                // load file from disk if it is older than the cursor and exists
                if (fileDate >= pageCommitTime && File.Exists(cachePath))
                {
                    Console.WriteLine($"[CACHE] {newUri.AbsoluteUri}");
                    root2 = JObject.Parse(File.ReadAllText(cachePath));
                }
                else
                {
                    Console.WriteLine($"[GET] {newUri.AbsoluteUri}");

                    var json2 = await GetContent((string)items[i]["@id"]);
                    File.WriteAllText(cachePath, json2);

                    if (json2[0] != '<')
                    {
                        root2 = JObject.Parse(json2);
                        JArray tempItems = (JArray)root2["items"];

                        foreach (var item in tempItems)
                        {
                            var metadata = GetMetadata((JObject)item);

                            _list.Add(metadata);
                            _items2.Add(metadata);
                        }
                    }
                }
            }
            SaveCursor(pageCommitTime);
            return v;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    //public static async Task<HttpCatalogVisitor> CreateDiffHCV(Uri baseUri)
    //{
    //    try
    //    {
    //        string json;

    //        var v = new HttpCatalogVisitor();
    //        var fileDate = GetNowCursor();

    //        using (System.Net.Http.HttpClient hc = new System.Net.Http.HttpClient())
    //        using (HttpResponseMessage response = await hc.GetAsync(baseUri))
    //        using (HttpContent content = response.Content)
    //        {
    //            json = await content.ReadAsStringAsync();
    //            JObject root = JObject.Parse(json);
    //            JArray resources = (JArray)root["resources"];
    //            string catalogUri = (string)resources.Last["@id"];
    //            using (HttpResponseMessage response2 = await hc.GetAsync(catalogUri))
    //            using (HttpContent content2 = response2.Content)
    //            {
    //                json = await content2.ReadAsStringAsync();
    //                //var fileName = baseUri.LocalPath.Replace("/", "-");
    //                //var path = $"C:\\CatalogCache\\{fileName}";
    //                //File.WriteAllText(path, json);


    //                root = JObject.Parse(json);
    //                JArray items = (JArray)root["items"];
    //                /* items.Count when you have time */
    //                for (int i = 0; i < items.Count; i++)
    //                {
    //                    var pageCommitTime = items[i]["commitTimeStamp"].ToObject<DateTimeOffset>();
    //                    Uri newUri = new Uri((string)items[i]["@id"]);
    //                    var fileName = newUri.LocalPath.Replace("/", "-");
    //                    var cachePath = $"C:\\CatalogCache\\{fileName}";

    //                    JObject root2 = null;

    //                    //load file from disk if it is older than the cursor and exists
    //                    if (fileDate >= pageCommitTime && File.Exists(cachePath))
    //                    {
    //                        Console.WriteLine($"[CACHE] {newUri.AbsoluteUri}");
    //                        Console.WriteLine("{0} Elements in List2", _list2.Count);
    //                        root2 = JObject.Parse(File.ReadAllText(cachePath));
    //                    }
    //                    else
    //                    {
    //                        Console.WriteLine($"[GET] {newUri.AbsoluteUri}");

    //                        using (HttpResponseMessage tempResponse = await hc.GetAsync((string)items[i]["@id"]))
    //                        using (HttpContent tempContent = tempResponse.Content)
    //                        {
    //                            var json2 = await tempContent.ReadAsStringAsync();
    //                            //File.WriteAllText(cachePath, json2);

    //                            if (json2[0] != '<')
    //                            {
    //                                root2 = JObject.Parse(json2);
    //                                JArray tempItems = (JArray)root2["items"];

    //                                foreach (var item in tempItems)
    //                                {
    //                                    var tempId = item["nuget:id"].ToObject<string>();
                                        
    //                                    if (_list3.Contains(tempId))
    //                                    {
    //                                        foreach (var element in _list2)
    //                                        {
    //                                            if (element.Id.ToLower().Equals(tempId.ToLower()))
    //                                            {
    //                                                element.AddToHashset(tempId);
    //                                                break;
    //                                            }
    //                                        }
    //                                    }
    //                                    else
    //                                    {
    //                                        _list2.Add(new PackageData(tempId));
    //                                        _list3.Add(tempId);
    //                                    }
    //                                }
    //                            }
    //                        }
    //                    }

    //                    SaveCursor(pageCommitTime);
    //                }
    //            }
    //        }
    //        return v;
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine("Ex: {0}", ex.ToString());
    //        throw ex;
    //    }
    //}

    //public static async Task<HttpCatalogVisitor> CreateFromBeg(Uri baseUri)
    //{
    //    try
    //    {
    //        string json;

    //        var v = new HttpCatalogVisitor();
    //        var fileDate = GetMinCursor();

    //        using (System.Net.Http.HttpClient hc = new System.Net.Http.HttpClient())
    //        using (HttpResponseMessage response = await hc.GetAsync(baseUri))
    //        using (HttpContent content = response.Content)
    //        {
    //            json = await content.ReadAsStringAsync();
    //            JObject root = JObject.Parse(json);
    //            JArray resources = (JArray)root["resources"];
    //            string catalogUri = (string)resources.Last["@id"];
    //            using (HttpResponseMessage response2 = await hc.GetAsync(catalogUri))
    //            using (HttpContent content2 = response2.Content)
    //            {
    //                json = await content2.ReadAsStringAsync();
    //                var fileName = baseUri.LocalPath.Replace("/", "-");
    //                var path = $"C:\\CatalogCache\\{fileName}";
    //                File.WriteAllText(path, json);


    //                root = JObject.Parse(json);
    //                JArray items = (JArray)root["items"];
    //                /* items.Count when you have time */
    //                for (int i = 0; i < items.Count; i++)
    //                {
    //                    var pageCommitTime = items[i]["commitTimeStamp"].ToObject<DateTimeOffset>();
    //                    Uri newUri = new Uri((string)items[i]["@id"]);
    //                    fileName = newUri.LocalPath.Replace("/", "-");
    //                    var cachePath = path = $"C:\\CatalogCache\\{fileName}";

    //                    JObject root2 = null;

    //                    //load file from disk if it is older than the cursor and exists
    //                    if (fileDate >= pageCommitTime && File.Exists(cachePath))
    //                    {
    //                        Console.WriteLine($"[CACHE] {newUri.AbsoluteUri}");
    //                        root2 = JObject.Parse(File.ReadAllText(cachePath));
    //                    }
    //                    else
    //                    {
    //                        Console.WriteLine($"[GET] {newUri.AbsoluteUri}");

    //                        using (HttpResponseMessage tempResponse = await hc.GetAsync((string)items[i]["@id"]))
    //                        using (HttpContent tempContent = tempResponse.Content)
    //                        {
    //                            var json2 = await tempContent.ReadAsStringAsync();
    //                            File.WriteAllText(cachePath, json2);

    //                            if (json2[0] != '<')
    //                            {
    //                                root2 = JObject.Parse(json2);
    //                                JArray tempItems = (JArray)root2["items"];

    //                                foreach (var item in tempItems)
    //                                {
    //                                    var tempId = item["nuget:id"].ToObject<string>();

    //                                    if (_list3.Contains(tempId))
    //                                    {
    //                                        foreach (var element in _list2)
    //                                        {
    //                                            if (element.Id.ToLower().Equals(tempId))
    //                                            {
    //                                                element.AddToHashset(tempId);
    //                                                break;
    //                                            }
    //                                        }
    //                                    }
    //                                    else
    //                                    {
    //                                        _list2.Add(new PackageData(tempId));
    //                                        _list3.Add(tempId);
    //                                    }
    //                                }
    //                            }
    //                        }
    //                    }

    //                    SaveCursor(pageCommitTime);
    //                }
    //            }
    //        }
    //        return v;
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine("Ex: {0}", ex.ToString());
    //        throw ex;
    //    }
    //}

    public List<Tuple<string, NuGetVersion>> GetNewIDVersions()
    {
        string tempId;
        NuGetVersion tempVersion;
        List<Tuple<string, NuGetVersion>> newIDVersions = new List<Tuple<string, NuGetVersion>>();

        /* Create new Cursor File w current date */
        SaveCursor(DateTimeOffset.Now);

        DateTimeOffset fileDate = GetMinCursor();
            

        DateTime tempDate;
        for (int i = 0; i < 10; i++)
        {
            tempId = (string)_items2[i].Id;
            tempVersion = NuGetVersion.Parse(_items2[i].Version.ToString());
            tempDate = DateTime.Parse(_items2[i].CommitTimeStamp.ToString());
            /* Adds only changed or new catalog pages based on cursor */
            if (tempDate > fileDate)
            {
                Tuple<string, NuGetVersion> tempTuple = new Tuple<string, NuGetVersion>(tempId, tempVersion);
                newIDVersions.Add(tempTuple);
            }
        }
        return newIDVersions;
    }

    private static PackageMetadata GetMetadata(JObject entry)
    {
        var tempId = entry["nuget:id"].ToObject<string>();
        var tempVersion = NuGetVersion.Parse(entry["nuget:version"].ToObject<string>());
        var tempDate = DateTimeOffset.Parse(entry["commitTimeStamp"].ToObject<string>());

        return new PackageMetadata(tempVersion, tempId, tempDate);
    }

    private static PackageData GetPackageData(JObject entry)
    {
        var tempId = entry["nuget:id"].ToObject<string>();

        return new PackageData(tempId);
    }

    public void DownloadIDVersions()
    {
        List<Tuple<string, NuGetVersion>> newIdVersions = new List<Tuple<string, NuGetVersion>>();
        newIdVersions = GetNewIDVersions();

        foreach (Tuple<string, NuGetVersion> item in newIdVersions)
        {
            var path = "https:\\api.nuget.org\v3-flatcontainer\\" + item.Item1 + '\\' + item.Item2.ToString() + ".nupkg";

            ZipPackage packageContent = new ZipPackage(path);
            var fileContent = packageContent.GetContentFiles();
            var cont = "";
            foreach (var fileCont in fileContent)
            {
                cont += fileCont;
            }

            var fileName = item.Item1.Replace(".", "-");
            var newPath = $"C:\\CatalogCache\\NewIDVersions\\{fileName}";
            using (System.Net.Http.HttpClient hc = new System.Net.Http.HttpClient())
            {
                if (File.Exists(newPath))
                {
                    File.AppendAllText(newPath, cont);
                }
                else
                {
                    File.WriteAllText(newPath, cont);
                }
            }
        }
    }

    //public IEnumerable<List<NuGetVersion>> GetIDSPackages(string[] ids)
    //{
    //    string tempId;
    //    NuGetVersion tempVersion;
    //    /* _items2.Count when you have time */
    //    for (int j = 0; j < 10; j++)
    //    {
    //        tempId = (string)_items2[j].Id;
    //        tempVersion = NuGetVersion.Parse(_items2[j].Version.ToString());
    //        List<NuGetVersion> newList = insideLoop(ids, tempId, tempVersion);
    //        yield return newList;
    //    }
    //}

    private List<NuGetVersion> insideLoop(string[] ids, string tempId, NuGetVersion tempVersion)
    {
        List<NuGetVersion> newList = new List<NuGetVersion>();
        for (int i = 0; i < ids.Length; i++)
        {
            if (tempId.Equals(ids[i]))
            {
                newList.Add(new NuGetVersion(tempVersion));
            }
            else
            {
                continue;
            }
        }
        return newList;
    }

    /* Not Used
    public IEnumerable<NuGetVersion> GetIDPackages(string id)
        {
            string tempId;
            NuGetVersion tempVersion;
            // _items2.Count when you have time
            for (int j = 0; j < 10; j++)
            {
                tempId = (string)_items2[j]["nuget:id"];
                if (tempId.Equals(id))
                {
                    tempVersion = NuGetVersion.Parse((string)_items2[j]["nuget:version"]);
                    yield return new NuGetVersion(tempVersion);
                }
            }
        }
    */
}