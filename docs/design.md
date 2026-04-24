# Unity Grid Asset Slicer 設計書

## 1. Architecture

Editor UI に処理を寄せず、テスト可能な core service を中心に構成する。

```text
Editor UI
  -> Application Services
    -> Core Grid / Export Services
    -> Persistence
    -> Optional Detection Backends
```

## 2. Package Layout

実装開始時は Unity project 内に UPM package を作成する。

```text
Packages/com.sunmax0731.grid-asset-slicer/
  package.json
  README.md
  Runtime/
    Sunmax.GridAssetSlicer.Runtime.asmdef
    Models/
  Editor/
    Sunmax.GridAssetSlicer.Editor.asmdef
    Windows/
    Services/
    Persistence/
    Export/
  Tests/
    Editor/
```

推奨 package metadata:

```json
{
  "name": "com.sunmax0731.grid-asset-slicer",
  "displayName": "Unity Grid Asset Slicer"
}
```

推奨 namespace:

```text
Sunmax.GridAssetSlicer
Sunmax.GridAssetSlicer.Editor
```

## 3. Core Models

### GridSliceSession

保存される最上位 session。

- Source image reference
- Grid settings
- Export settings
- Excluded cells
- Last quality check result
- Format version

### GridSettings

Grid 計算に必要な設定。

- `rows`
- `columns`
- `marginLeft`
- `marginTop`
- `marginRight`
- `marginBottom`
- `gutterX`
- `gutterY`
- `cellWidth`
- `cellHeight`

### CellCoordinate

0-based の row / column で cell を示す。

### ExportSettings

出力先と命名規則を保持する。

- `outputFolder`
- `filePrefix`
- `startIndex`
- `numberPadding`
- `conflictBehavior`

## 4. Services

### GridCalculator

`GridSettings` と source image dimensions から cell rectangles を計算する pure C# service。

検証項目:

- rows / columns は正の整数。
- margin / gutter は 0 以上。
- cell width / height は正の整数。
- cell rectangle は image bounds 内。

### TextureCellExtractor

Unity texture から cell pixel を抽出する。

Unity 固有 API はこの adapter 層へ閉じ込める。

### ExportFileNameResolver

出力 file path を決定する。

競合処理:

- `overwrite`: 既存 path を返す。
- `skip`: 既存 path を skipped として扱う。
- `duplicate`: `_copy01` のような suffix を付ける。

### PngExporter

選択 cell を PNG として書き出す。

成功 / 警告 / 失敗 / exported files / skipped files を structured result として返す。

### SliceSessionSerializer

session JSON を読み書きする。

format version を持ち、未対応 version は明確に失敗させる。

### QualityCheckService

export 前の警告と error を集める。

初期対象:

- Empty selection
- Invalid bounds
- Missing source
- File conflicts
- Transparent or near-empty cells

### DetectionBackend

OpenCV などの任意検出機能は interface の裏側に置く。

```text
IGridDetectionBackend
  Detect(image data) -> GridDetectionResult
```

## 5. Editor UI

初期実装は IMGUI または UI Toolkit のどちらでもよい。既存コードがないため、MVP では実装速度と安定性を優先する。

UI 実装時は、画面イメージとして `docs/Image.png` を参照する。

この画面イメージは以下の構成を示す。

- 上部 toolbar: source image path、auto detect、preview、export、save session。
- 左 pane: grid settings、output settings、display settings。
- 中央 pane: indexed cell grid preview、excluded cell 表示、selected cell highlight、zoom controls。
- 右 pane: cell inspector、metadata、selected cell preview、session info。
- 下部 pane: quality check report table、warning filters、status。

Window sections:

- Source image
- Grid settings
- Preview
- Cell selection
- Export settings
- Quality warnings
- Session save / load
- Export result

UI は service 呼び出しに徹し、grid math や file naming を直接持たない。

## 6. Persistence Rules

- JSON を replay metadata の source of truth とする。
- path は可能な限り Unity project relative path で保存する。
- 壊れた JSON は自動補正せず load failure として扱う。
- schema 変更時は migration 方針と fixture test を追加する。

## 7. Implementation Order

1. UPM package scaffold と asmdef
2. Core models
3. Grid calculation と tests
4. File naming と conflict resolution
5. Session JSON save / load
6. PNG export service
7. Editor window shell
8. Preview と cell selection
9. Manual export end-to-end
10. Optional auto-detection backend
