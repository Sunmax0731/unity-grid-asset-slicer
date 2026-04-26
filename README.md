# Unity Grid Asset Slicer

Unity Grid Asset Slicer は、1枚のグリッド画像から複数の PNG アセットを切り出す Unity Editor 拡張です。

主な対象は、画像生成ツールなどで作成したアイコンシート、スキル画像、UI アイコン、ポートレートなどです。ユーザーはグリッドを手動指定または将来的には自動検出し、セル単位でプレビュー、採用・除外、PNG 出力、再現用 JSON 保存を行えます。

## 現状

- Unity project: Unity 6000.4.0f1
- Repository: `D:\Claude\UnityEditor-Dev\unity-grid-asset-slicer`
- Product name: Unity Grid Asset Slicer
- Version: 0.1.1
- Current implementation state: Unity テンプレート + 設計ドキュメント
- Delivery target: UPM Git package として導入できる Unity Editor 拡張

## ドキュメント

- [要件定義](docs/requirements.md)
- [設計書](docs/design.md)
- [Slice Session JSON 仕様](docs/slice-session-schema.md)
- [実装バックログ](docs/implementation-backlog.md)
- [検証計画](docs/validation-plan.md)
- [リリース計画](docs/release-plan.md)
- [画面イメージ](docs/Image.png)
- [AI Agents Guide](Agents.md)
- [Project Skill](Skill.md)

## MVP

1. `Tools > Grid Asset Slicer > メイン画面` から Editor Window を開く。
2. PNG ソース画像を選択する。
3. 行数、列数、margin、gutter、出力設定を指定する。
4. 切り出し予定セルを preview する。
5. セル単位で採用・除外を切り替える。
6. 選択セルを個別 PNG として出力する。
7. slice session JSON を保存する。
8. JSON を読み込み、同条件で再実行できる。

OpenCV による自動検出は、手動グリッド、永続化、PNG 出力が安定した後の追加機能として扱います。

## メニュー

- `Tools > Grid Asset Slicer > メイン画面`
- `Tools > Grid Asset Slicer > ライセンス`
- `Tools > Grid Asset Slicer > バージョン情報`

## ライセンス

Unity Grid Asset Slicer は MIT License で提供します。詳細は [LICENSE.md](LICENSE.md) を確認してください。
