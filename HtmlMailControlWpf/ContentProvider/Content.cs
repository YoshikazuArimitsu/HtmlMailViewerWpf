using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HtmlMailControlWpf.ContentProvider
{
    public class Content : IDisposable
    {
        public string BaseDir { get; set; }
        public string HtmlUri { get; set; }

        public Content()
        {
            string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);
            BaseDir = tempDirectory;
            HtmlUri = new System.Uri(Path.Combine(BaseDir, "index.html")).AbsoluteUri;
        }

        public static void RecursiveDelete(DirectoryInfo baseDir)
        {
            if (!baseDir.Exists)
                return;

            foreach (var dir in baseDir.EnumerateDirectories())
            {
                RecursiveDelete(dir);
            }
            var files = baseDir.GetFiles();
            foreach (var file in files)
            {
                file.IsReadOnly = false;
                file.Delete();
            }
            baseDir.Delete();
        }

        public void Dispose()
        {
            RecursiveDelete(new DirectoryInfo(BaseDir));
        }
    }
}
