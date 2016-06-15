using System.Collections.Generic;

namespace NuGet.CatalogVisitor
{
    class PackageData
    {
        public PackageData(string id)
        {
            Id = id.ToLower();
            Hashset = new HashSet<string>();
            Hashset.Add(id);
            Count = 1;
        }

        public string Id { get; }

        public HashSet<string> Hashset { get; }

        public void AddToHashset(string id)
        {
            if (Hashset.Contains(id))
            {
                Count++;
            }
            else
            {
                Hashset.Add(id);
                Count++;
            }
        }

        public int Count { get; set; }
    }
}
