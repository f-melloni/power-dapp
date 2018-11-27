using System;
using System.IO;

namespace PowerDapp_UserArea.Tools
{
    public class Tools
    {
        public static string GetFileVersion(string path)
        {
            string p = path.TrimStart('~');
            p = p.Replace('/', '\\');
            p = p.TrimStart('\\');
            string absolutePath = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') + '\\' + p;

            DateTime lastModified = File.GetLastWriteTime(absolutePath);
            return path.TrimStart('~') + "?version=" + lastModified.Ticks.ToString();

        }
    }
}
