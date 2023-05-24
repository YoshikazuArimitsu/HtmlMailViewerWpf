# HtmlMailViewerWpf

## 概要

![image](./images/image.png)

HTML 形式のデータを持つ Email ファイル(.eml) を内部で展開し、WebView2 に表示する WPF 用コントロールです。

また、文字列パターンを指定する事で本文中のマッチした部分をリンクに差し替え、リンクをクリックした時にアプリケーション側でイベントを受け取る事が可能です。

## 利用方法

[SampleApp](./SampleApp/) が実際の使用例です。

```xml
<Window x:Class="SampleApp.MainWindow"
        ...
        xmlns:hmv="clr-namespace:HtmlMailControlWpf;assembly=HtmlMailControlWpf">
    <Grid>
        <hmv:HtmlMailControl
            SourceType={"Binding SourceType}"}
            EmlFile="{Binding EmlFile}"
            Patterns="{Binding LinkPatterns}"
            EnableContextMenu="{Binding EnableContextMenu}"
            LinkClicked="EmlView_LinkClicked"
            />
    </Grid>
</Window>
```

| プロパティ名 | 説明                                       |
| :----------- | :----------------------------------------- |
| SourceType   | 表示データ(EmlFile/BodyText/BodyHtml)      |
| EmlFile      | eml ファイルのパス                         |
| Body         | テキストデータ(SourceType=BodyText/BodyHtml) |
| Patterns     | リンクに差し替える文字列パターン(string[]) |
| EnableContextMenu  | 右クリックメニュー有効かどうか(bool) |
| LinkClicked  | リンククリック時のコールバック             |
