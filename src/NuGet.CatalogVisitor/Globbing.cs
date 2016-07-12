using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NuGet.CatalogVisitor
{
    /// <summary>
    /// From: http://stackoverflow.com/questions/398518/how-to-implement-glob-in-c-sharp
    /// Answer #1, Mark Maxham.
    /// 
    /// JK, FROM: http://brianary.blogspot.com/2007/09/globbing-using-filename-wildcard.html
    /// </summary>
    class Globbing
    {


        //static public List<string> GetFile(string pattern)
        //{
        //    List<string> fileList = new List<string>();
        //    fileList.AddRange(GetFiles(pattern));
        //    return fileList;
        //}

        //static public List<string> GetFiles(string patternList)
        //{
        //    List<string> fileList = new List<string>();
        //    foreach (string pattern in patternList.Split(Path.GetInvalidPathChars()))
        //    {
        //        string dir = Path.GetDirectoryName(pattern);
        //        if(string.IsNullOrEmpty(dir))
        //        {
        //            dir = Directory.GetCurrentDirectory();
        //        }
        //        fileList.AddRange(Directory.GetFiles(Path.GetFullPath(dir), Path.GetFileName(pattern)));
        //    }
        //    return fileList;
        //}

        //static char DirSep = Path.DirectorySeparatorChar;

        //public static IEnumerable<string> Glob(string globPattern)
        //{
        //    foreach (string path in Glob(PathHead(globPattern) + DirSep, PathTail(globPattern)))
        //    {
        //        yield return path;
        //    }
        //}

        //public static IEnumerable<string> Glob(string head, string tail)
        //{
        //    if (PathTail(tail) == tail)
        //    {
        //        foreach (string path in Directory.GetFiles(head, tail).OrderBy(s => s))
        //        {
        //            yield return path;
        //        }
        //    }
        //    else
        //    {
        //        foreach (string dir in Directory.GetDirectories(head, PathHead(tail)).OrderBy(s => s))
        //        {
        //            foreach (string path in Glob(Path.Combine(head, dir), PathTail(tail)))
        //            {
        //                yield return path;
        //            }
        //        }
        //    }
        //}

        //static string PathHead(string path)
        //{
        //    if (!path.Contains(DirSep))
        //    {
        //        return ".";
        //    }

        //    if (path.StartsWith("" + DirSep + DirSep))
        //    {
        //        return path.Substring(0, 2) + path.Substring(2).Split(DirSep)[0] + DirSep + path.Substring(2).Split(DirSep)[1];
        //    }

        //    return path.Split(DirSep)[0];
        //}

        //static string PathTail(string path)
        //{
        //    if (!path.Contains(DirSep))
        //    {
        //        return path;
        //    }

        //    return path.Substring(1 + PathHead(path).Length);
        //}


    }
}
