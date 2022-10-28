using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HtmlMailControlWpf.ContentProvider
{
    internal interface IContentProvider
    {
        public Content ParseFile(string filename);

        public Content ParseData(string data);
    }
}
