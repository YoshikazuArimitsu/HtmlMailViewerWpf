using AngleSharp.Html;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlMailControlWpf.ContentProvider
{
    public class EmlParserService : IContentProvider
    {
        public EmlParserService()
        {
        }

        private string getCharset(IHtmlDocument doc)
        {
            var contentType = doc.QuerySelector("meta[http-equiv='Content-Type']")?.GetAttribute("content");

            if(contentType != null)
            {
                if(contentType.ToLower().Contains("iso-2022-jp")) {
                    return "iso-2022-jp";
                }
            }

            return "utf-8";
        }

        public Content Parse(Stream stream)
        {
            var content = new Content();
            var mimeMessage = MimeMessage.Load(stream);

            var htmlStream = new MemoryStream();
            var cidDictionary = new Dictionary<string, string>();

            // ディレクトリ下に保存
            foreach (var part in mimeMessage.BodyParts)
            {
                var mimePart = part as MimePart;
                var textPart = part as TextPart;

                if (mimePart != null)
                {
                    if (textPart != null)
                    {
                        if (textPart.IsHtml)
                        {
                            textPart.Content.DecodeTo(htmlStream);
                        }

                    }
                    else
                    {
                        var savePath = Path.Combine(content.BaseDir, mimePart.FileName);

                        using (var s = File.Create(savePath))
                            mimePart.Content.DecodeTo(s);

                        if (mimePart.ContentId != null)
                        {
                            cidDictionary.Add(mimePart.ContentId, mimePart.FileName);
                        }

                    }
                }
            }

            // cidイメージ対応
            {
                htmlStream.Seek(0, SeekOrigin.Begin);
                var parser = new HtmlParser();
                var doc = parser.ParseDocument(htmlStream);

                var charset = getCharset(doc);

                var imgNodes = doc.QuerySelectorAll("img");
                foreach (var imgNode in imgNodes)
                {
                    var imgSrc = imgNode.GetAttribute("src");
                    if (imgSrc != null && imgSrc.StartsWith("cid:"))
                    {
                        // cid:* をファイル名に置換
                        var cid = imgSrc.Substring(4);
                        string fileName;
                        if (cidDictionary.TryGetValue(cid, out fileName))
                        {
                            imgNode.SetAttribute("src", fileName);
                        }
                    }
                }

                using (StreamWriter outputFile = new StreamWriter(Path.Combine(content.BaseDir, "index.html"), false,
                     System.Text.Encoding.GetEncoding(charset)))
                {
                    doc.ToHtml(outputFile, new PrettyMarkupFormatter());
                }
            }

            return content;
        }

        public Content ParseFile(string path)
        {
            using (var fs = File.OpenRead(path))
            {
                return Parse(fs);
            }
        }
        public Content ParseData(string data)
        {
            throw new NotImplementedException();
        }
    }
}
