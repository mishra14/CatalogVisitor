using System;

namespace NuGet.CatalogVisitor
{
    public class MemoryCursor : ICursor
    {
        public DateTimeOffset Date { get; set; }
    }
}
