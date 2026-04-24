# Unity Grid Asset Slicer マニュアル

English version: [Manual.md](Manual.md)

関連文書:

- [TermsOfUse.md](TermsOfUse.md)
- [ReleaseNotes.md](ReleaseNotes.md)
- [ValidationChecklist.md](ValidationChecklist.md)

## 概要

Unity Grid Asset Slicer は、格子状に並んだ PNG 画像を Unity Editor 上で個別の PNG ファイルへ切り出すための Editor 拡張です。
スライス設定は session JSON として保存できるため、同じ素材を同じ設定で再出力できます。

## 対応環境

- Unity `6000.0` 以降
- Windows 上の Unity Editor を優先サポート
- オフライン利用可能

## インストール

この package を Unity project の `Packages/` 配下に配置します。

```text
Packages/com.sunmax0731.grid-asset-slicer/
```

Package Manager から Git URL または local package として追加する運用も想定しています。

## 起動

Unity Editor のメニューから起動します。

```text
Tools > Grid Asset Slicer
```

## 基本ワークフロー

1. `Source Image` に PNG テクスチャを指定します。
2. `Rows` と `Columns` を入力します。
3. 必要に応じて margin、gutter、cell width、cell height を設定します。
4. `Preview` を押し、別ウィンドウの preview で cell 分割を確認します。
5. 出力しない cell は右クリックで excluded にします。
6. `Output Folder`、`File Prefix`、`Start Index`、`Number Padding`、`Conflict` を設定します。
7. `Export PNGs` を実行します。
8. 必要に応じて `Save Session` で設定を JSON に保存します。

## Grid 設定

- `Display` セクションの `Parameter Help` を有効にすると、操作を続けたまま各入力項目の説明を表示できます。
- `Rows`: 縦方向の cell 数です。
- `Columns`: 横方向の cell 数です。
- `Margin Left / Top / Right / Bottom`: 画像端から grid までの余白です。
- `Gutter X / Y`: cell 間の隙間です。
- `Cell Width / Height`: cell サイズを明示する場合に使います。未指定の場合は画像サイズ、margin、gutter、rows、columns から計算します。

grid が画像範囲外になる場合は validation warning を表示しますが、cell 矩形を計算できる場合は preview を表示します。rows / columns が無効な場合や、暗黙 cell サイズを計算できない場合など、cell 矩形を作れない設定のときだけ preview は表示されません。

## Preview と Cell 除外

preview は export と同じ grid calculation を使います。
`Preview` を押すと、preview は別の可変サイズウィンドウに表示されます。メインツール内には grid を直接描画せず、別ウィンドウを開いたままでもメインツール側の設定を変更できます。

- 左クリック: cell を選択します。
- 右クリック: cell の include / exclude を切り替えます。

excluded cell は export 対象から外れ、session JSON に保存されます。

## Export 設定

- `Output Folder`: PNG の出力先です。
- `File Prefix`: 出力ファイル名の prefix です。
- `Start Index`: 連番の開始番号です。
- `Number Padding`: 連番のゼロ埋め桁数です。
- `Conflict`: 既存ファイルがある場合の動作です。

`Conflict` は次の値を選べます。

- `Overwrite`: 既存ファイルを上書きします。
- `Skip`: 既存ファイルを残し、該当 cell を skip します。
- `Duplicate`: `_copy01` のような別名で出力します。

## Session JSON

`Save Session` で source path、grid settings、export settings、excluded cells を保存できます。
`Load Session` で保存済み JSON を読み込むと、同じ設定で再度 export できます。

session JSON の format version は `1` です。

## Samples

`Samples~/` には検証用の PNG と session JSON が含まれています。

- `BasicGrid`: 2x2 の基本サンプル
- `GuttersAndMargins`: gutter と margin のサンプル
- `TransparentCells`: transparent cell を含むサンプル

## 既知の制限

- MVP では PNG の grid slicing に焦点を当てています。
- 不規則な atlas、polygon packing、sprite metadata 生成は対象外です。
- 大きな画像や大量 cell の UX 最適化は今後の改善対象です。
