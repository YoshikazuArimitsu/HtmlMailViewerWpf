using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HtmlMailControlWpf.ContentProvider
{
    public class HtmlContentService : IContentProvider
    {
        public Content ParseData(string data)
        {
            var content = new Content();

            var savePath = Path.Combine(content.BaseDir, "index.html");

            using (StreamWriter outputFile = new StreamWriter(Path.Combine(content.BaseDir, "index.html")))
            {
                outputFile.WriteLine(data);
            }
            return content;
        }

        public Content ParseFile(string filename)
        {
            throw new NotImplementedException();
        }
    }
}
