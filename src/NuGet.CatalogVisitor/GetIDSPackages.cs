partial class HttpCatalogVisitor : ICatalogVisitor
{
    public IEnumerable<List<NuGetVersion>> GetIDSPackages(string[] ids)
    {
        string tempId;
        NuGetVersion tempVersion;
        /* _items2.Count when you have time */
        for (int j = 0; j < 10; j++)
        {
            tempId = (string)_items2[j]["nuget:id"];
            tempVersion = NuGetVersion.Parse((string)_items2[j]["nuget:version"]);
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