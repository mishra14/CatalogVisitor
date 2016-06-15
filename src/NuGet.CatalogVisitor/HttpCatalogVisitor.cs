using Newtonsoft.Json.Linq;
using NuGet;
using NuGet.CatalogVisitor;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

public class HttpCatalogVisitor : ICatalogVisitor
{
    private static readonly List<PackageMetadata> _list = new List<PackageMetadata>();
    private static List<PackageMetadata> _items2 = new List<PackageMetadata>();

    public HttpCatalogVisitor()
    {

    }

    public IEnumerable<PackageMetadata> GetPackages()
    {
        return _list;
    }

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

    /* Default cursor path */
    private static string CursorPath = "C:\\CatalogCache\\cursor.txt";

    public static void SetCursorPath(string cursorPath)
    {
        CursorPath = cursorPath;
    }

    public static DateTimeOffset GetMinCursor()
    {
        Console.WriteLine("Please insert path to cursor file (.txt): ");
        var cursorPath = Console.ReadLine();
        HttpCatalogVisitor.SetCursorPath(cursorPath);

        DateTimeOffset fileDate = DateTimeOffset.MinValue;

        if (File.Exists(CursorPath))
        {
            var cursorText = File.ReadAllText(CursorPath);
            fileDate = DateTimeOffset.Parse(cursorText);
        }

        return fileDate;
    }

    public static DateTimeOffset GetNowCursor()
    {
        Console.WriteLine("Please insert path to cursor file (.txt): ");
        var cursorPath = Console.ReadLine();
        HttpCatalogVisitor.SetCursorPath(cursorPath);

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
            SaveCursor(DateTimeOffset.Now);
            return v;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }


    public List<Tuple<string, NuGetVersion>> GetNewIDVersions()
    {
        string tempId;
        NuGetVersion tempVersion;
        List<Tuple<string, NuGetVersion>> newIDVersions = new List<Tuple<string, NuGetVersion>>();

        /* Create new Cursor File w current date */
        DateTimeOffset fileDate = GetMinCursor();
            

        DateTimeOffset tempDate;
        /* items2.count */
        for (int i = 0; i < 10; i++)
        {
            tempId = (string)_items2[i].Id;
            tempVersion = NuGetVersion.Parse(_items2[i].Version.ToString());
            tempDate = DateTimeOffset.Parse(_items2[i].CommitTimeStamp.ToString());
            /* Adds only changed or new catalog pages based on cursor */
            if (tempDate > fileDate)
            {
                Tuple<string, NuGetVersion> tempTuple = new Tuple<string, NuGetVersion>(tempId, tempVersion);
                newIDVersions.Add(tempTuple);
            }
        }
        SaveCursor(DateTimeOffset.Now);
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

    public IEnumerable<List<NuGetVersion>> GetIDSPackages(string[] ids)
    {
        string tempId;
        NuGetVersion tempVersion;
        /* _items2.Count when you have time */
        for (int j = 0; j < 10; j++)
        {
            tempId = (string)_items2[j].Id;
            tempVersion = NuGetVersion.Parse(_items2[j].Version.ToString());
            List<NuGetVersion> newList = insideLoop(ids, tempId, tempVersion);
            yield return newList;
        }
    }

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