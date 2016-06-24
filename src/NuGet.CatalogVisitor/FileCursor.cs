using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet.CatalogVisitor
{
    public class FileCursor : ICursor
    {
        public FileCursor()
        {

        }

        public FileCursor(string cursorPath, DateTimeOffset date)
        {
            CursorPath = cursorPath;
            Date = date;
        }

        public string CursorPath { get; set; }
        public DateTimeOffset Date { get; set; }
        public void Save()
        {
            File.WriteAllText(CursorPath, Date.ToString("o"));
        }
        public static FileCursor Load(string path)
        {
            DateTimeOffset fileDate = DateTimeOffset.MinValue;

            if (File.Exists(path))
            {
                var cursorText = File.ReadAllText(path);
                fileDate = DateTimeOffset.Parse(cursorText);
            }

            return new FileCursor()
            {
                CursorPath = path,
                Date = fileDate
            };
        }
    }
}
