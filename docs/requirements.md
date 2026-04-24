# Unity Grid Asset Slicer 要件定義書

## 1. 概要

Unity Grid Asset Slicer は、グリッド状に配置された1枚画像を、Unity で利用可能な個別 PNG アセットへ切り出す Unity Editor 拡張である。

主な利用対象は、AI 画像生成ツールで生成したアイテム画像、スキルアイコン、UI アイコン、顔アイコンなどの制作支援である。

## 2. 前提

- 配布形態は UPM Git package とする
- 対応 Unity は Unity 6000.0 以降とする
- 初期対応 OS は Windows を優先する
- 入力形式は PNG を主対象とする
- 出力形式は PNG とする
- 処理はオフラインで完結する
- OpenCV は別 UPM package 依存として扱う

## 3. 機能要件

### RQ-001 入力画像選択

ユーザーは Unity Editor 上で source PNG を選択できること。

### RQ-002 手動グリッド指定

ユーザーは以下を数値指定できること。

- 行数
- 列数
- 外枠 margin
- セル間 gutter
- セル幅
- セル高さ

### RQ-003 自動検出

OpenCV backend を利用して、source PNG から余白および境界を推定できること。

自動検出結果は直接確定せず、手動グリッド指定値へ反映し、ユーザーが確認・調整できること。

### RQ-004 Preview

切り出し予定のセル一覧を preview できること。

### RQ-005 採用 / 除外

ユーザーはセル単位で出力対象から除外できること。

### RQ-006 出力サイズ

出力画像は元セルサイズを維持すること。

### RQ-007 命名規則

出力ファイル名は prefix と桁数固定の連番で生成すること。

例:

```text
item_001.png
item_002.png
item_003.png
```

### RQ-008 ファイル衝突処理

既存ファイルと衝突した場合、以下から選択できること。

- overwrite
- skip
- duplicate

### RQ-009 Metadata

slice session を JSON として保存できること。

JSON には最低限以下を含める。

- 生成日
- source画像パス
- grid設定
- 出力先
- prefix
- 連番桁数
- 除外セル
- 品質チェック結果
- 出力ファイル一覧

### RQ-010 再現性

保存済み JSON metadata から同じ条件で再実行できること。

### RQ-011 品質チェック

出力前または出力時に以下を検査できること。

- 空白セル
- 低解像度
- ほぼ単色
- 透明領域不足
- 余白過多
- 重複・類似セル
- 出力失敗

### RQ-012 オフライン動作

画像処理、preview、出力、metadata 保存はすべてローカルで完結すること。

外部 API やクラウドサービスへの送信を行わないこと。

## 4. 非機能要件

### NFR-001 配布

UPM Git package として導入できること。

### NFR-002 依存管理

OpenCV は package 本体へ直接埋め込まず、別 UPM package 依存として扱うこと。

### NFR-003 拡張性

初期用途は汎用画像切り出しとし、特定ゲームや特定 Item Catalog には依存しないこと。

### NFR-004 UI

Unity Editor Window として提供し、初期入口は以下とする。

```text
Tools > Grid Asset Slicer
```

## 5. 初回 MVP 範囲

- Editor Window
- PNG 入力
- 数値指定グリッド
- OpenCV による余白・境界推定
- preview
- 採用 / 除外
- PNG 出力
- 桁数固定連番
- 衝突処理選択
- JSON metadata 保存
- JSON metadata からの再実行
- 品質チェック

## 6. 将来範囲

- GUI 上のドラッグ補正
- 自動セル検出の精度向上
- CSV / JSON による名前マッピング
- SpriteAtlas 連携
- Addressables 連携
- プロジェクト別 adapter
- アイテム、スキル、UI、portrait 向け preset