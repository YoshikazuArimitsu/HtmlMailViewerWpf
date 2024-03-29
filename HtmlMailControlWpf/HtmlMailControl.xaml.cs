﻿using HtmlMailControlWpf.ContentProvider;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HtmlMailControlWpf
{
    /// <summary>
    /// モード
    /// </summary>
    public enum SourceType
    {
        EmlFile,
        BodyText,
        BodyHtml
    }

    /// <summary>
    /// リンク押下時イベント
    /// </summary>
    public class LinkClickedEventArgs : RoutedEventArgs
    {
        private readonly string _href;

        public string Href
        {
            get { return _href; }
        }

        public LinkClickedEventArgs(RoutedEvent routedEvent, string href) : base(routedEvent)
        {
            _href = href;
        }
    }

    /// <summary>
    /// WebViewに埋め込むホステッドオブジェクト
    /// </summary>
    [ClassInterface(ClassInterfaceType.None)]
    [ComVisible(true)]
    public class HtmlMailControlHostedObject
    {
        public HtmlMailControl HtmlMailControl { get; set; }
        public void HostCallback(string strText)
        {
            HtmlMailControl.HostCallback(strText);
        }
    }

    /// <summary>
    /// HtmlMailControl.xaml の相互作用ロジック
    /// </summary>
    public partial class HtmlMailControl : UserControl
    {
        private const string EMBED_SCRIPT = @"
// 指定文字列を含む Node の検索
const getTextNodes = (pattern) => {
    const xPathResult = document.evaluate(
        pattern ? `//text()[contains(., ""${pattern}"")]` : '//text()',
        document,
        null,
        XPathResult.ORDERED_NODE_SNAPSHOT_TYPE,
        null
    );
    const result = [];
    let { snapshotLength } = xPathResult;

    while (snapshotLength--)
    {
        result.unshift(xPathResult.snapshotItem(snapshotLength));
    }

    return result;
};

// C#側コールバック呼び出し
const hostCallback = (arg) => {
    chrome.webview.hostObjects.class.HostCallback(arg);
}

// TextNode -> AnchorNode 置き換え
// 対象文字列を**含む** テキストノードの対象文字列部分のみをリンクに置き換える為、
// TextNode -> Span要素 ([対象文字列より前の部分], [対象文字列], [対象文字列より後の部分]) への
// 置き換えを行う。
// モノによってはレイアウトが崩れるかもしれない。
const replaceTextNode = (n, p) => {
    const targetNode = document.createElement('span');

    const splitContent = n.textContent.split(p);

    const beforeNode = n.cloneNode();
    beforeNode.textContent = splitContent[0];

    const anchorNode = document.createElement('a');
    anchorNode.href = `javascript:hostCallback('${p}');`;
    anchorNode.textContent = p;

    const afterNode = n.cloneNode();
    afterNode.textContent = splitContent[1];

    targetNode.appendChild(beforeNode);
    targetNode.appendChild(anchorNode);
    targetNode.appendChild(afterNode);

    n.replaceWith(targetNode);
}

// 指定文字列を含むテキストノードを <a～ に差し替える
const replaceHostCallbackNode = (pattern) => {
    const results = getTextNodes(pattern)

    results.forEach( (r) => {
        replaceTextNode(r, pattern);
    });
}

// dom構築後、パターンを C# 側呼び出しのリンクに置換
document.addEventListener('DOMContentLoaded', () => {
    callbackPatterns.forEach((p) => replaceHostCallbackNode(p));
});
";

        private Task<string> _embedScriptId;
        private HtmlMailControlHostedObject _hostedObject;
        private Content _content;

        /// <summary>
        /// 動作モード
        /// </summary>
        public SourceType SourceType
        {
            get
            {
                return (SourceType)GetValue(SourceTypeProperty);
            }
            set
            {
                SetValue(SourceTypeProperty, value);
            }
        }

        public static readonly DependencyProperty SourceTypeProperty =
            DependencyProperty.Register("SourceType", typeof(SourceType), typeof(HtmlMailControl),
                    new FrameworkPropertyMetadata(SourceType.EmlFile, new PropertyChangedCallback(OnSourceChanged)));

        /// <summary>
        /// 開くEMLファイルのパス(EmlFile用)
        /// </summary>
        public string EmlFile
        {
            get
            {
                return (string)GetValue(EmlFileProperty);
            }
            set
            {
                SetValue(EmlFileProperty, value);
            }
        }

        public static readonly DependencyProperty EmlFileProperty =
            DependencyProperty.Register("EmlFile", typeof(string), typeof(HtmlMailControl),
                    new FrameworkPropertyMetadata("EmlFile", new PropertyChangedCallback(OnSourceChanged)));


        /// <summary>
        /// テキストデータ(BodyText/Html用)
        /// </summary>
        public string Body
        {
            get
            {
                return (string)GetValue(BodyProperty);
            }
            set
            {
                SetValue(BodyProperty, value);
            }
        }

        public static readonly DependencyProperty BodyProperty =
            DependencyProperty.Register("Body", typeof(string), typeof(HtmlMailControl),
                    new FrameworkPropertyMetadata("Body", new PropertyChangedCallback(OnSourceChanged)));

        /// <summary>
        /// 開くURL
        /// </summary>
        public string SourceUri
        {
            get
            {
                return (string)GetValue(SourceUriProperty);
            }
            set
            {
                SetValue(SourceUriProperty, value);
            }
        }

        public static readonly DependencyProperty SourceUriProperty =
            DependencyProperty.Register("SourceUri", typeof(string), typeof(HtmlMailControl), new PropertyMetadata(null));

        /// <summary>
        /// リンクに置換する対象
        /// </summary>
        public string[] Patterns
        {
            get
            {
                return (string[])GetValue(PatternProperty);
            }
            set
            {
                SetValue(PatternProperty, value);
            }
        }

        public static readonly DependencyProperty PatternProperty =
            DependencyProperty.Register("Patterns", typeof(string[]), typeof(HtmlMailControl),
                new FrameworkPropertyMetadata(new string[0], new PropertyChangedCallback(OnPatternChanged)));

        public bool EnableContextMenu
        {
            get
            {
                return (bool)GetValue(EnableContextMenuProperty);
            }
            set
            {
                SetValue(EnableContextMenuProperty, value);
            }
        }

        public static readonly DependencyProperty EnableContextMenuProperty =
            DependencyProperty.Register("EnableContextMenu", typeof(bool), typeof(HtmlMailControl),
                new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnSettingChanged)));

        /// <summary>
        /// 置換リンクを踏んだ時のイベント
        /// </summary>
        public static readonly RoutedEvent LinkClickedEvent = EventManager.RegisterRoutedEvent(
            name: "LinkClicked",
            routingStrategy: RoutingStrategy.Bubble,
            handlerType: typeof(RoutedEventHandler),
            ownerType: typeof(HtmlMailControl));

        public event RoutedEventHandler LinkClicked
        {
            add { AddHandler(LinkClickedEvent, value); }
            remove { RemoveHandler(LinkClickedEvent, value); }
        }

        /// <summary>
        /// .ctor
        /// </summary>
        public HtmlMailControl()
        {
            InitializeComponent();

            _hostedObject = new HtmlMailControlHostedObject()
            {
                HtmlMailControl = this
            };

            webView.CoreWebView2InitializationCompleted += WebView_CoreWebView2InitializationCompleted;
            webView.NavigationStarting += WebView_NavigationStarting;
        }

        /// <summary>
        /// ナビゲーションイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WebView_NavigationStarting(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
        {
            // ローカルファイル以外のURLはOSで開く
            if (null != e.Uri && e.Uri.StartsWith("file://") == false)
            {
                e.Cancel = true;
                Process.Start(new ProcessStartInfo
                {
                    FileName = e.Uri,
                    UseShellExecute = true
                });
            }
        }

        /// <summary>
        /// 埋め込みスクリプトの更新
        /// </summary>
        private void updateEmbedScript()
        {
            if(webView.CoreWebView2 == null)
            {
                return;
            }

            if (_embedScriptId != null)
            {
                webView.CoreWebView2.RemoveScriptToExecuteOnDocumentCreated(_embedScriptId.Result);
            }

            Func<string> makePattern = () =>
            {
                return (Patterns == null || Patterns.Count() == 0) ? "[]" : $"['{string.Join("','", Patterns)}']";
            };


            var initLine = $"const callbackPatterns = {makePattern()};\r\n";
            var script = initLine + EMBED_SCRIPT;

            _embedScriptId = webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(script);
        }

        /// <summary>
        /// CoreWebView初期化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WebView_CoreWebView2InitializationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
        {
            webView.CoreWebView2.AddHostObjectToScript("class", _hostedObject);
            webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = EnableContextMenu;
            webView.EnsureCoreWebView2Async();
            updateEmbedScript();
        }


        /// <summary>
        /// DataContext変更ハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// https://github.com/MicrosoftEdge/WebView2Feedback/issues/1136#issuecomment-1255470804
        /// 上記記事で DataContextChanged を捕まえて中 WebView2 の DataContextごと null にすると
        /// 問題を回避できるというコメントに従い実装
        /// </remarks>
        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
            {
                webView.DataContext = null;
            }
        }

        public void HostCallback(string arg)
        {
            var eventArgs = new LinkClickedEventArgs(LinkClickedEvent, arg);
            RaiseEvent(eventArgs);
        }

        /// <summary>
        /// Source変更
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="e"></param>
        /// <exception cref="InvalidOperationException"></exception>
        private static void OnSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            HtmlMailControl ctrl = obj as HtmlMailControl;

            if (ctrl != null)
            {
                switch(ctrl.SourceType)
                {
                    case SourceType.EmlFile:
                        var es = new EmlParserService();
                        try
                        {
                            ctrl._content = es.ParseFile(ctrl.EmlFile);
                            ctrl.SourceUri = ctrl._content.HtmlUri;
                        } catch(Exception ex)
                        {
                            Debug.WriteLine($"Parse eml failed, Ignore. {ex.Message}");
                        }
                        break;

                    case SourceType.BodyText:
                        var ps = new PlainContentService();
                        ctrl._content = ps.ParseData(ctrl.Body);
                        ctrl.SourceUri = ctrl._content.HtmlUri;
                        break;

                    case SourceType.BodyHtml:
                        var hs = new HtmlContentService();
                        ctrl._content = hs.ParseData(ctrl.Body);
                        ctrl.SourceUri = ctrl._content.HtmlUri;
                        break;

                    default:
                        throw new InvalidOperationException("Unknown SourceType");
                }
            }
        }

        /// <summary>
        /// LinkPattern変更イベント
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="e"></param>
        private static void OnPatternChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            HtmlMailControl ctrl = obj as HtmlMailControl;

            if (ctrl != null && ctrl.webView.CoreWebView2 != null)
            {
                // CoreWebView初期化済みなら埋め込みスクリプトを更新してリロード
                ctrl.updateEmbedScript();
                ctrl.webView.Reload();
            }
        }

        /// <summary>
        /// その他設定変更イベント
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="e"></param>
        private static void OnSettingChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            HtmlMailControl ctrl = obj as HtmlMailControl;

            if (ctrl != null && ctrl.webView.CoreWebView2 != null)
            {
                // コンテキストメニュー設定変更
                ctrl.webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = ctrl.EnableContextMenu;
                ctrl.updateEmbedScript();
                ctrl.webView.Reload();
            }
        }

    }
}
