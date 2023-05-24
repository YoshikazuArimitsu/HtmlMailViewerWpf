# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.0.17] - 2023-05-24

- コンテキストメニューの有効/無効切り替え設定に対応

## [0.0.16] - 2022-12-30

- 初期化時の `Cannot convert '<null>' from type '<null>' to type 'System.Uri' for...` 警告抑止

## [0.0.15] - 2022-11-11

### Changed

- <meta> タグの Content-Type での指定Encodingに対応Cannot convert '<null>' from type '<null>' to type 'System.Uri' for
	- unicode
	- iso-2022-jp
	- shift_jis
	- euc-jp
	- utf-8

## [0.0.14] - 2022-11-02

### Added

- HTMLの表示対応(BodyHtmlモード)

### Changed

- HTML置換機能でボタンではなくリンクに置換

## [0.0.13] - 2022-10-30

### Changed

- 不正なパスが指定された時は無視する。

## [0.0.12] - 2022-10-30

### Added

- プレーンテキストの表示対応(BodyTextモード)

### Changed

- アンカーをボタンに変更？

## [0.0.11] - 2022-10-27

### Changed

- リンクを開いた時、OS機能で開くよう修正

### Fixed

- Patterns = null の時、置換パターンに空配列が渡るよう修正

## [0.0.9] - 2022-10-22

### Changed

- 対応プラットフォームを .NET Core 3.1 以降に変更

## [0.0.8] - 2022-10-21

### Added

- Nuget に登録

## [0.0.2] - 2022-10-21

### Added

- README.md, CHANGELOG.md 追加

## 0.0.1 - 2022-10-21

### Added

- 初版

[unreleased]: https://github.com/YoshikazuArimitsu/HtmlMailViewerWpf/compare/v0.0.14...HEAD
[0.0.2]: https://github.com/YoshikazuArimitsu/HtmlMailViewerWpf/releases/tag/v0.0.2
[0.0.8]: https://github.com/YoshikazuArimitsu/HtmlMailViewerWpf/releases/tag/v0.0.8
[0.0.9]: https://github.com/YoshikazuArimitsu/HtmlMailViewerWpf/releases/tag/v0.0.9
[0.0.11]: https://github.com/YoshikazuArimitsu/HtmlMailViewerWpf/releases/tag/v0.0.11
[0.0.12]: https://github.com/YoshikazuArimitsu/HtmlMailViewerWpf/releases/tag/v0.0.12
[0.0.13]: https://github.com/YoshikazuArimitsu/HtmlMailViewerWpf/releases/tag/v0.0.13
[0.0.14]: https://github.com/YoshikazuArimitsu/HtmlMailViewerWpf/releases/tag/v0.0.14
[0.0.15]: https://github.com/YoshikazuArimitsu/HtmlMailViewerWpf/releases/tag/v0.0.15
