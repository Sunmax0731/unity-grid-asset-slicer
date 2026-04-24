# Unity Grid Asset Slicer v0.1.0

Unity Editor 上で grid-based PNG source image を個別 PNG に切り出すためのエディタ拡張です。

## 含まれるもの

- `UnityGridAssetSlicer-0.1.0.unitypackage`
- `UnityGridAssetSlicer-0.1.0-release.zip`
- `README.md`
- `Manual.ja.md`
- `Manual.md`
- `TermsOfUse.md`
- `ReleaseNotes.md`
- `ValidationChecklist.md`
- `CHANGELOG.md`
- `LICENSE.md`
- `Samples/`

GitHub Release と BOOTH 配布物は同一内容です。

## v0.1.0 の主な機能

- `Tools > Grid Asset Slicer > Open` から起動する Editor window
- source PNG selection
- rows / columns / margins / gutters / explicit cell size の grid setup
- export と同じ cell rectangles を使う別ウィンドウ preview
- cell selection
- 右クリックによる include / exclude 切り替え
- PNG export
- export file prefix / start index / number padding の指定
- 既存ファイル衝突時の `Overwrite` / `Skip` / `Duplicate`
- 品質チェック項目の ON / OFF と quality report
- 日本語 / 英語の表示言語切り替え
- 別ウィンドウの parameter help
- Read/Write 無効な source texture の一時読み取り可能コピー処理
- session JSON save / load
- sample PNGs and replay fixtures
- validation script: `tools/validation/run-editmode-tests.ps1`

## Samples

同梱 sample:

- `BasicGrid`: simple 2x2 colored grid
- `GuttersAndMargins`: gutters 付き grid と margins 付き grid
- `TransparentCells`: transparent cell を含む grid

各 sample には replay 用 `.session.json` が含まれています。

## 検証

検証コマンド:

```powershell
.\tools\validation\run-editmode-tests.ps1
```

検証内容:

- Grid calculation
- Session JSON persistence
- Export naming and conflict behavior
- PNG export service
- Editor menu registration
- Sample replay fixtures

## 対応環境

- Unity `6000.0` 以降
- Windows 上の Unity Editor を優先サポート
- オフライン利用可能

検証環境:

- Unity `6000.4.0f1`
- Windows
- EditMode tests

## 既知の制限

- MVP supports grid-based PNG slicing only.
- 不規則な atlas layout は未対応です。
- OpenCV auto-detection などの自動セル検出は未実装です。
- Sprite metadata / Unity `Sprite` asset generation は未実装です。
- 複数 source image の batch processing は未実装です。
- very large source images / high cell counts は今後の UX / performance tuning 対象です。

## ドキュメント

- 日本語マニュアル: `Manual.ja.md`
- English manual: `Manual.md`
- 利用規約: `TermsOfUse.md`
- リリースノート: `ReleaseNotes.md`
- 検証チェックリスト: `ValidationChecklist.md`

## 備考

BOOTH 配布物もこの GitHub Release と同じ成果物を前提としています。
