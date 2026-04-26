# Unity Grid Asset Slicer

`Unity Grid Asset Slicer` は、格子状に並んだ PNG 画像を Unity Editor 上で個別の PNG ファイルへ切り出すためのエディタ拡張です。
アイコンシート、アイテム画像、UI パーツ、タイル状の検証素材など、同じサイズの画像が grid 配置されている素材を、Unity プロジェクト内で確認しながら分割できます。

切り出し設定は session JSON として保存できるため、同じ素材を同じ条件で再出力できます。

## こんな方におすすめです

- Unity プロジェクト内で grid 画像を手早く PNG 分割したい方
- アイテム画像、UI アイコン、素材シートを個別ファイルに整理したい方
- 切り出し条件を JSON として保存し、後から同じ条件で再出力したい方
- 外部画像編集ソフトではなく、Unity Editor 内で素材確認から export まで行いたい方

## 主な機能

- `Tools > Grid Asset Slicer > メイン画面` から起動する Unity Editor window
- source PNG の選択
- rows / columns / margins / gutters / cell width / cell height の設定
- export と同じ grid calculation を使った preview
- 可変サイズの別ウィンドウ preview
- cell の選択
- 右クリックによる include / exclude 切り替え
- PNG export
- 出力ファイル名の prefix、開始番号、ゼロ埋め桁数の指定
- 既存ファイル衝突時の `Overwrite` / `Skip` / `Duplicate` 対応
- 品質チェック項目の ON / OFF
- quality report と書き出し結果の確認
- 日本語 / 英語の表示言語切り替え
- 別ウィンドウの parameter help
- Read/Write 無効な source texture の一時読み取り可能コピー処理
- session JSON の保存 / 読み込み
- sample PNG と replay 用 session JSON
- EditMode test 用 validation script
- 日本語 / 英語マニュアル

## 同梱内容

- `UnityGridAssetSlicer-0.1.1.unitypackage`
- `UnityGridAssetSlicer-0.1.1-release.zip`
- 日本語マニュアル `Manual.ja.md`
- English manual `Manual.md`
- 利用規約 `TermsOfUse.md`
- リリースノート `ReleaseNotes.md`
- 検証チェックリスト `ValidationChecklist.md`
- `CHANGELOG.md`
- `LICENSE.md`
- sample grid images
- sample session JSON files

GitHub Release と BOOTH で配布する成果物は同一内容を前提としています。

## 対応環境

- Unity `6000.0` 以降
- Windows 上の Unity Editor を優先サポート
- オフライン利用可能

検証環境:

- Unity `6000.4.0f1`
- Windows
- EditMode tests

## 現在の対象外機能

`v0.1.1` は grid-based PNG slicing に焦点を当てた初期版です。
以下は現在の対象外です。

- 不規則な atlas layout の自動検出
- OpenCV などによる自動セル検出
- polygon packing
- Unity `Sprite` metadata の自動生成
- 複数 source image の batch processing
- runtime 向け slicing 機能

## ご購入前にご確認ください

- 本商品は Unity Editor 用のエディタ拡張です。
- source image の利用権限は購入者側でご確認ください。
- 生成された PNG の権利は、元画像の権利条件に従います。
- 詳しい使い方と制限事項は同梱マニュアルをご確認ください。
- BOOTH 配布物と GitHub Release 配布物は同一内容を前提としています。

## サポート時に必要な情報

お問い合わせ時は、次の情報があると確認しやすくなります。

- 使用している Unity バージョン
- Unity Grid Asset Slicer のバージョン
- source image のサイズ
- rows / columns / margins / gutters / cell size の設定
- session JSON
- Unity Console に表示された error / warning
- 再現手順
