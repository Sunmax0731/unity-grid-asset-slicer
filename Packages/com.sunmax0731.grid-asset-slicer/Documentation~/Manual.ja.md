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
Tools > Grid Asset Slicer > メイン画面
```

ライセンスとバージョン情報は `Tools > Grid Asset Slicer > ライセンス` と `Tools > Grid Asset Slicer > バージョン情報` から確認できます。

## 基本ワークフロー

1. `Source Image` に PNG テクスチャを指定します。
2. `Rows` と `Columns` を入力します。
3. 必要に応じて margin、gutter、cell width、cell height を設定します。
4. 左ペインの大きめの `Margin Controller` で source image 上のガイドをドラッグし、上下左右の余白を直接調整します。
5. 必要に応じて `Custom Columns` / `Custom Rows` を有効化し、各列・各行の区切り位置を個別に調整します。
6. `Preview` を押し、別ウィンドウの preview で cell 分割を確認します。
7. 出力しない cell は右クリックで excluded にします。
8. `Output Folder`、`File Prefix`、`Start Index`、`Number Padding`、必要に応じて `Output Width / Height`、`Conflict` を設定します。
9. `Export...` を実行します。
10. 必要に応じて `Save Session` で設定を JSON に保存します。

## Grid 設定

- ツールバーの `Help` ボタンを使うと、操作を続けたまま各入力項目の説明を別ウィンドウで表示できます。
- `Rows`: 縦方向の cell 数です。数値 field と slider の両方で調整できます。
- `Columns`: 横方向の cell 数です。数値 field と slider の両方で調整できます。
- `Margin Left / Top / Right / Bottom`: 画像端から grid までの余白です。数値 field / slider / margin controller の 3 つが同期します。
- `Gutter X / Y`: cell 間の隙間です。数値 field と slider の両方で調整できます。
- `Cell Width / Height`: cell サイズを明示する場合に使います。未指定の場合は画像サイズ、margin、gutter、rows、columns から計算します。ON 中は slider でも調整できます。
- `Custom Columns / Custom Rows`: 各列幅 / 各行高を個別管理したいときに使います。区切り位置 slider を動かすと、隣接する幅が再配分されます。同じセクションに、その軸の余白 slider も表示されます。

grid が画像範囲外になる場合は validation warning を表示しますが、cell 矩形を計算できる場合は preview を表示します。rows / columns が無効な場合や、各 cell に 1 pixel 以上を割り当てられない場合など、cell 矩形を作れない設定のときだけ preview は表示されません。

implicit cell size が割り切れない場合でも、tool は整数境界を補間して cell rect を作り、preview と export の両方に同じ rect 一覧を使います。

メインツールの設定フォームは 1 つの左ペインに集約されています。grid、可変境界、output、cell inspector、quality checks を同じ場所で調整でき、各グループは Foldout で開閉できます。

中央の workspace は、元画像未選択時の empty state、現在の概要、Preview / Export の主要操作、警告、最新の書き出し結果が順に分かる構成です。余計な右ペインを増やさず、今の状態と次の操作を追いやすくしています。

## Preview と Cell 除外

preview は export と同じ grid calculation を使います。
`Preview` を押すと、preview は別の可変サイズウィンドウに表示されます。メインツール内には grid を直接描画せず、別ウィンドウを開いたままでもメインツール側の設定を変更できます。

- 左クリック: cell を選択します。
- 右クリック: cell の include / exclude を切り替えます。

excluded cell は export 対象から外れ、session JSON に保存されます。

Cell Inspector には選択 cell の preview が表示されます。`Show Outline`、`Background Color`、`Outline Color` で表示を調整できます。

## Export 設定

Export 設定は左の設定ペイン内の `Output` Foldout に表示されます。

- `Output Folder`: PNG の出力先です。
- `File Prefix`: 出力ファイル名の prefix です。
- `Start Index`: 連番の開始番号です。
- `Number Padding`: 連番のゼロ埋め桁数です。
- `Output Width / Height`: すべての書き出し PNG を統一サイズにしたい場合の設定です。両方を有効にすると、各セルは書き出し前にそのピクセルサイズへリサイズされます。
- `Conflict`: 既存ファイルがある場合の動作です。

`Conflict` は次の値を選べます。

- `Overwrite`: 既存ファイルを上書きします。
- `Skip`: 既存ファイルを残し、該当 cell を skip します。
- `Duplicate`: `_copy01` のような別名で出力します。

cell の書き出しに失敗した場合、quality report に対象 cell、出力先、失敗原因が表示されます。
元画像の Read/Write が無効な場合、書き出し時に一時的な読み取り可能コピーを作成し、そのコピーを使って slice します。元アセットの importer 設定は変更し続けません。

quality report 上部の `Report Height` スライダーで、レポート表示領域の高さを調整できます。

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
