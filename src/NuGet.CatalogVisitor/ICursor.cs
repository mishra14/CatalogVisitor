using System;

namespace NuGet.CatalogVisitor
{
    public interface ICursor
    {
        DateTimeOffset Date { get; set; }
    }
}
