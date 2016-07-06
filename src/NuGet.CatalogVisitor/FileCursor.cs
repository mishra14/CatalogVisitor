using System;
using System.IO;

namespace NuGet.CatalogVisitor
{
    public class FileCursor : ICursor
    {
        /// <summary>
        /// A class to keep track of a file that keeps track of
        /// and controls when the last time a function was run.
        /// The user controls their own FileCursor, setting it
        /// and saving it before and after they run the code they would
        /// like to run. This means they can do what they want to do
        /// with whatever packages they would like to do it with.
        /// 
        /// Empty constructor to have complete freedom.
        /// </summary>
        public FileCursor()
        {

        }

        /// <summary>
        /// If you know the path of cursor file ".txt" and date
        /// you want it to be when creating it. Don't have to call
        /// load or do anything else after this.
        /// </summary>
        /// <param name="cursorPath"></param>
        /// <param name="date"></param>
        public FileCursor(string cursorPath, DateTimeOffset date)
        {
            CursorPath = cursorPath;
            Date = date;
        }

        /// <summary>
        /// Path to .txt file that holds cursor (DateTimeOffset format).
        /// </summary>
        public string CursorPath { get; set; }

        /// <summary>
        /// Date it is currently set at.
        /// </summary>
        public DateTimeOffset Date { get; set; }

        /// <summary>
        /// Save Date to current time (DateTimeOffset.UtcNow).
        /// </summary>
        public void Save()
        {
            File.WriteAllText(CursorPath, Date.ToString("o"));
        }

        /// <summary>
        /// Sets date to .txt file.
        /// If .txt file doesn't exist, Date is now DateTimeOffset.MinValue.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
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
