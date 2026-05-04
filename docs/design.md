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
- `columnWidths` optional
- `rowHeights` optional

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
- implicit cell size が割り切れない場合でも、各 axis の境界を整数補間して cell rect を返す。

implicit size の境界規則:

- 利用可能幅 / 高さから gutter を引いた残りを axis ごとに扱う。
- `boundary[i] = round(i * available / count)` の形で整数境界を作る。
- 各 cell size は隣接 boundary の差分とする。
- これにより preview と export は同じ可変幅 / 可変高さの rect 一覧を共有する。
- `available < count` で各 cell に 1 pixel 以上を配れない場合だけ invalid とする。
- `columnWidths` / `rowHeights` が指定されている場合は、その explicit 配列を優先する。

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
- 左 pane: unified settings form を Foldout 単位で整理し、grid settings、margin controller、variable boundary settings、output settings、cell inspector、quality toggles を 1 列で扱う。
- 中央 pane: source / grid summary、empty state、detached preview guidance、next actions、warnings、latest export summary を表示する。
- 下部 pane: quality check report table、warning filters、status。

Window sections:

- Source image
- Grid settings
- Margin controller
- Preview
- Cell selection
- Export settings
- Quality warnings
- Session save / load
- Export result

UI は service 呼び出しに徹し、grid math や file naming を直接持たない。

Grid settings UI rules:

- rows / columns / margins / gutters / explicit cell size は数値 field を残しつつ slider でも調整できる。
- margin controller は detached preview とは別の source-image view とし、left / top / right / bottom guide をドラッグして `GridSettings` を直接更新する。
- variable boundary settings は `columnWidths` / `rowHeights` を編集し、各 separator の位置を個別に変えられるようにする。
- detached preview は grid slice の確認と cell include / exclude 操作に専念させる。
- main workspace は元画像未選択時の empty state、現在の概要、Preview / Export の主要操作、警告、最新結果を分離して表示する。
- export settings は output folder、naming、conflict behavior に加えて、書き出し先の統一 `outputWidth` / `outputHeight` を持つ。両方が指定された場合のみ各セル PNG を同一サイズへリサイズして保存する。

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
