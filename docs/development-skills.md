# 開発に必要なスキル整理

この文書は `D:\Claude\UnityEditor-Dev\workspace-guides` を確認したうえで、Unity Grid Asset Slicer の開発で利用するスキルと順序を整理する。

## 参照した workspace guide

- `workspace-guides/Agents.md`
- `workspace-guides/skills/unity-editor-workspace-common/SKILL.md`
- `workspace-guides/skills/unity-editor-intake-and-planning/SKILL.md`
- `workspace-guides/skills/unity-editor-foundation-and-persistence/SKILL.md`
- `workspace-guides/skills/unity-editor-core-services/SKILL.md`
- `workspace-guides/skills/unity-editor-ui-and-workflow/SKILL.md`
- `workspace-guides/skills/unity-editor-validation-and-release/SKILL.md`

## 今回必要なスキル

### 1. unity-editor-workspace-common

UnityEditor-Dev 配下の共通作法を扱う。

このリポジトリでは、workspace root を単一 repo として扱わず、`unity-grid-asset-slicer` を独立した sibling repo として扱う。

### 2. unity-editor-intake-and-planning

要件整理、Issue 分割、MVP と将来拡張の切り分けに使う。

初期 Issue は少なくとも以下へ分割する。

- repo / UPM package scaffold
- slice session JSON と永続化
- grid slicing core service
- OpenCV auto-detect backend
- quality check service
- Editor Window preview / export workflow
- validation and release preparation

### 3. unity-editor-foundation-and-persistence

UPM package 構成、asmdef、namespace、保存形式、format version、migration 方針を固めるために使う。

本ツールでは UI より先に、slice session JSON の schema と save / load / replay の責務を決める。

### 4. unity-editor-core-services

UI から呼び出す非 UI ロジックを実装するために使う。

本ツールの core service は以下を含む。

- source image loader
- manual grid calculator
- OpenCV margin / boundary detector
- cell cropper
- duplicate filename resolver
- PNG exporter
- metadata writer
- quality checker

### 5. unity-editor-ui-and-workflow

EditorWindow、preview、selection、採用 / 除外、衝突処理選択、Auto Detect 実行導線を実装するために使う。

UI は処理ロジックを直接抱えず、core service と persistence layer を呼び出す構成にする。

### 6. unity-editor-validation-and-release

Unity 6000 での compile / test、sample project、manual verification、UPM package と release artifact の確認に使う。

Windows 優先のため、Unity executable は `C:\Program Files\Unity` 配下から確認する。

## 推奨開発順序

1. repo / package scaffold を作る
2. 要件定義を Issue に分割する
3. slice session JSON schema を固定する
4. 手動グリッド計算と PNG 出力 core を実装する
5. 品質チェック service を追加する
6. EditorWindow preview / export workflow を接続する
7. OpenCV backend による余白・境界推定を追加する
8. Unity 6000 の compile / manual verification を整える
9. UPM Git package として導入可能な状態にする

## 初期設計上の注意

- OpenCV は別 UPM package 依存とし、本 package に native binary を直接同梱しない
- オフライン動作を前提とし、画像や metadata を外部 API に送信しない
- preview と export の計算結果がずれないように、同じ grid model を共有する
- JSON metadata は format version を持つ
- 破損した metadata は読み込み失敗として扱い、勝手に補正して保存しない
- 自動検出結果は即時確定せず、手動設定値へ反映してユーザーが確認する
- 出力衝突処理は overwrite / skip / duplicate を明示的に選択させる