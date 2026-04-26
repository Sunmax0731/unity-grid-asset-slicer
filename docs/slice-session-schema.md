# Slice Session JSON 仕様

## 1. 目的

slice session JSON は、切り出し操作を再現可能かつ review 可能にするための metadata である。

## 2. 初期 Version

```text
1
```

## 3. Example

```json
{
  "formatVersion": 1,
  "createdUtc": "2026-04-24T00:00:00Z",
  "toolVersion": "0.1.1",
  "source": {
    "assetPath": "Assets/SourceSheets/items.png",
    "width": 1024,
    "height": 1024,
    "contentHash": "optional-sha256"
  },
  "grid": {
    "rows": 8,
    "columns": 8,
    "marginLeft": 0,
    "marginTop": 0,
    "marginRight": 0,
    "marginBottom": 0,
    "gutterX": 0,
    "gutterY": 0,
    "cellWidth": 128,
    "cellHeight": 128
  },
  "selection": {
    "excludedCells": [
      { "row": 0, "column": 7 },
      { "row": 7, "column": 7 }
    ]
  },
  "export": {
    "outputFolder": "Assets/Generated/GridSlicer/items",
    "filePrefix": "item_",
    "startIndex": 1,
    "numberPadding": 3,
    "conflictBehavior": "duplicate"
  },
  "quality": {
    "lastCheckedUtc": "2026-04-24T00:00:00Z",
    "warnings": []
  }
}
```

## 4. Field Rules

- `source.assetPath` は Unity project relative path を優先する。
- `source.contentHash` は MVP では optional。
- `grid` の数値は pixel 単位。
- `selection.excludedCells` は 0-based row / column。
- default は include とし、除外 cell だけを保存する。
- `export.conflictBehavior` は `overwrite` / `skip` / `duplicate` のみ許可する。

## 5. Migration

loader は未対応の future `formatVersion` を明確な error として拒否する。

schema を変更する場合は、この文書と fixture test を同時に更新する。

