# Unity Grid Asset Slicer

Unity Grid Asset Slicer は、1枚のグリッド画像から複数の画像アセットを切り出す Unity Editor 拡張です。

ChatGPT Image2 などの画像生成ツールで作成した、アイコンやアイテム画像が格子状に並んだ画像を、Unity プロジェクト内で使いやすい個別 PNG アセットへ変換することを目的とします。

## Target

- Unity 6000.0 以降
- Windows 優先対応
- UPM Git package 配布
- オフライン動作
- PNG 入力、PNG 出力

## Features

- グリッド画像からセル単位で個別 PNG を出力
- 行数、列数、余白、境界幅を数値指定
- OpenCV backend による余白・境界の自動推定
- セル preview
- セル単位の採用 / 除外
- 元セルサイズを維持した出力
- 桁数固定の連番ファイル名
- overwrite / skip / duplicate の衝突処理
- JSON metadata による再現可能な slice session 保存
- 品質チェック
  - 空白セル
  - 低解像度
  - ほぼ単色
  - 透明領域不足
  - 余白過多
  - 重複・類似セル
  - 出力失敗

## Package Name

```json
{
  "name": "com.sunmax0731.grid-asset-slicer"
}
```

## OpenCV Dependency

画像処理は別 UPM package として導入する OpenCV backend を利用します。

この package 自体は外部 API やクラウド処理に依存せず、Unity Editor 内でオフライン処理を完結させます。

## Basic Workflow

1. `Tools > Grid Asset Slicer` を開く
2. source PNG を選択する
3. 行数、列数、margin、gutter を指定する
4. 必要に応じて `Auto Detect` で余白・境界を推定する
5. preview でセル単位の結果を確認する
6. 採用しないセルを除外する
7. 出力先、prefix、連番桁数、衝突処理を設定する
8. `Export` で個別 PNG を生成する
9. session JSON を保存し、同条件で再実行できるようにする

## Output Example

```text
Assets/Generated/GridSlicer/items/
  item_001.png
  item_002.png
  item_003.png
  slice-session.json
```

## Development Guides

このリポジトリは `D:\Claude\UnityEditor-Dev\workspace-guides` の Unity Editor 拡張向け共通ガイドに従って開発します。

今回の初期開発で参照するスキルは `docs/development-skills.md` にまとめています。

## Future Scope

- GUI 上でのドラッグ補正
- CSV / JSON による出力名マッピング
- SpriteAtlas 連携
- Unity Addressables 連携
- プロジェクト別 adapter
- item / skill / UI / portrait などの preset