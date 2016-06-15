using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet.CatalogVisitor
{
    public interface ICursor
    {
        DateTimeOffset Date { get; set; }
    }
}
