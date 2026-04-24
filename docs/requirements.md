# Unity Grid Asset Slicer 要件定義書

## 1. 目的

Unity Grid Asset Slicer は、グリッド状に配置された1枚の画像を Unity で利用可能な個別 PNG アセットへ切り出す Unity Editor 拡張である。

初期利用者は、画像生成ツールなどで作成したアイコンシートを Unity プロジェクトへ取り込む制作者を想定する。繰り返し作業を再現可能にし、出力結果を確認しながら安全に PNG 化できることを重視する。

## 2. 前提

- Unity Editor 内で動作する。
- 初期検証対象は Unity 6000.3.13f1。
- Unity 6000.x 系での利用を目標にする。
- 開発と初期検証は Windows を優先する。
- MVP の入力形式は PNG。
- MVP の出力形式は PNG。
- 画像処理はオフラインで完結する。
- OpenCV は任意の検出 backend とし、UI や core logic に直接結合しない。

## 3. 機能要件

### RQ-001 Editor 起動

ユーザーは次のメニューからツールを起動できる。

```text
Tools > Grid Asset Slicer > Open
```

### RQ-002 ソース画像選択

ユーザーは Unity プロジェクト内の PNG texture asset を選択できる。

表示する情報:

- Asset path
- Pixel width / height
- 読み取り可否、取得できる場合

### RQ-003 手動グリッド指定

ユーザーは以下を数値指定できる。

- 行数
- 列数
- 上下左右 margin
- 水平 / 垂直 gutter
- 任意の cell width / cell height

明示的な cell size がない場合は、画像サイズと grid 設定から計算する。

### RQ-004 Preview

切り出し予定セルを export 前に preview できる。

preview では、採用セルと除外セルの状態が分かること。

### RQ-005 セル採用 / 除外

ユーザーはセル単位で export 対象から除外できる。

除外状態は slice session JSON に保存する。

### RQ-006 Export 設定

ユーザーは以下を設定できる。

- 出力フォルダ
- ファイル prefix
- 開始番号
- 連番桁数
- 競合時の処理

競合時の処理:

- `overwrite`
- `skip`
- `duplicate`

### RQ-007 PNG Export

選択セルを個別 PNG として出力できる。

MVP では元セルの pixel size を維持する。拡大縮小、padding、trim は将来拡張とする。

### RQ-008 Slice Session JSON

ユーザーは切り出し操作を JSON metadata として保存できる。

JSON は同じソース画像から同じ出力を再現するために必要な情報を含む。

### RQ-009 Replay

保存済み JSON を読み込み、同じ条件で再実行できる。

ソース画像が存在しない、または条件が一致しない場合は明確な診断を表示する。

### RQ-010 Quality Check

出力前または出力時に以下を検出し、警告または error として扱う。

- 空または透明セル
- grid bounds が画像外
- 不正な行数 / 列数
- 出力フォルダが存在しない、または書き込み不可
- ファイル名競合
- PNG 出力失敗

### RQ-011 Auto Detection

手動 grid と永続化が安定した後、margin、gutter、grid bounds の自動推定を追加できる。

自動検出結果は即時確定せず、ユーザーが確認・調整してから適用する。

## 4. 非機能要件

### NFR-001 再現性

同じソース画像、session JSON、競合設定で実行した場合、出力結果を再現できること。

### NFR-002 オフライン動作

画像、metadata、diagnostic を外部サービスへ送信しないこと。

### NFR-003 Package 境界

Runtime、Editor UI、永続化、任意の画像解析 backend を分離し、UPM Git package 化しやすい構造にする。

### NFR-004 安全性

ユーザーが `overwrite` を選択しない限り、既存ファイルを上書きしないこと。

### NFR-005 診断性

失敗時は、対象 file、setting、cell を可能な限り特定して表示する。

## 5. MVP 受け入れ条件

- `Tools > Grid Asset Slicer > Open` から Editor window が開く。
- PNG texture を選択して preview できる。
- 手動行列指定で selected cell を PNG 出力できる。
- 除外セルは出力されない。
- `overwrite` / `skip` / `duplicate` が動作する。
- session JSON を保存できる。
- session JSON 読み込みで source path、grid settings、export settings、除外セルが復元される。
- Grid 計算、file naming、session serialization、競合解決に EditMode test がある。

## 6. 将来スコープ

- OpenCV による margin / boundary 検出
- preview 上での drag 補正
- CSV / JSON による出力名 mapping
- item / skill / UI / portrait / tile 向け preset
- SpriteAtlas 連携
- Addressables 連携
- 複数画像の batch 処理

