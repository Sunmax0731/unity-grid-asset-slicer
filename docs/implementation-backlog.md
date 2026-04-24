# 実装バックログ

1 Issue 1 目的で進める。UI より先に data model、grid calculation、persistence を固定する。

## P0 Foundation

### 1. Package Scaffold

Goal: `Packages/com.sunmax0731.grid-asset-slicer` を作成する。

Scope:

- `package.json`
- Runtime / Editor asmdef
- package README
- namespace convention

Acceptance:

- Unity 6000.4.0f1 で compile error がない。
- Editor window 実装はまだ不要。

### 2. Core Models and Grid Calculator

Goal: pure C# の model と grid rectangle 計算を追加する。

Scope:

- `GridSettings`
- `CellCoordinate`
- `CellRect`
- `GridCalculator`
- validation result types

Acceptance:

- valid grid、invalid rows / columns、negative margin、gutter、out-of-bounds の EditMode test がある。

### 3. Session JSON Persistence

Goal: slice session JSON を save / load できる。

Scope:

- `GridSliceSession`
- serializer
- required fields validation
- enum validation
- sample JSON fixture

Acceptance:

- sample JSON を round-trip できる。
- unsupported `formatVersion` が明確に失敗する。

## P1 Export

### 4. File Naming and Conflict Resolution

Goal: export 前に output path を deterministic に解決する。

Scope:

- prefix
- start index
- number padding
- `overwrite` / `skip` / `duplicate`
- export action plan

Acceptance:

- 3 種類の conflict behavior の test がある。
- duplicate name が安定している。

### 5. PNG Export Service

Goal: selected cells を PNG 出力する。

Scope:

- texture reading adapter
- cell extraction
- PNG write result
- AssetDatabase refresh

Acceptance:

- expected count の PNG が出力される。
- excluded cells は出力されない。
- exported / skipped files が result に含まれる。

## P2 Editor Workflow

### 6. Editor Window Shell

Goal: main tool entry point を追加する。

Scope:

- `Tools > Grid Asset Slicer > Open`
- source texture field
- grid settings fields
- export settings fields
- save / load / preview / export buttons

Acceptance:

- clean Unity project で window が開く。
- UI 内に grid math を重複実装しない。

### 7. Preview and Cell Selection

Goal: calculated cells を表示し、採用 / 除外を切り替える。

Acceptance:

- 除外 cell は export されない。
- save / load で excluded cells が復元される。

### 8. Quality Checks

Goal: export risk を出力前に表示する。

Acceptance:

- warning は安全な export を阻害しない。
- error は export を止め、問題の setting または cell を示す。

## P3 Detection and Release

### 9. Auto Detection Backend

Goal: optional な grid detection を追加する。

Acceptance:

- detection result は user action なしに manual settings を上書きしない。
- backend がない場合も tool は壊れない。

### 10. Release Preparation

Goal: UPM Git package として導入できる状態にする。

Acceptance:

- fresh Unity project から Git package として導入できる。
- manual smoke path が文書化され、通る。
