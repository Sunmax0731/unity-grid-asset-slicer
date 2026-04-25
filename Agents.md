## Unity エディタ拡張の共通実務

- GitHub Issue、Issue コメント、検証サマリー、リリースノートは日本語で記載する。コード識別子、パス、コマンド、ブランチ名、validation marker、エラー文は原文を維持する。
- `EditorWindow` は操作に集中させる。grid 計算、slice plan、読み取り可能 Texture 生成、batch 処理、export 命名、競合処理、release 検査は service に分離する。
- UI や workflow を追加したら、影響する README、manual/specification、validation checklist、release notes を同じ Issue で更新する。
- Issue 単位の検証では、可能な限り `ISSUE<number>_<TOPIC>_VALIDATION=PASS` のような明示 marker を出し、validation script 側でも marker を確認する。
- ユーザーが追加した QA 画像や生成済み `Assets/` 出力は、Issue でサンプル採用すると明記された場合だけコミットする。
- Release artifact は tracked files から生成する。配布 ZIP に `Assets/`、`Library/`、`Logs/`、`Temp/`、`Validation/`、`ReleaseBuilds/` が含まれないことを検査する。
- UI Toolkit への全面移行は高リスクな移行作業として扱う。安定した editor window を置き換える前に、主要 workflow との同等性を試作で確認する。

# Unity Grid Asset Slicer Agents Guide

この文書は、この repository で作業する AI agent / automation 向けの作業規約である。

## Project Identity

- Repository: `D:\Claude\UnityEditor-Dev\unity-grid-asset-slicer`
- Product: Unity Grid Asset Slicer
- Unity version: 6000.4.0f1
- Delivery target: UPM Git package
- Main menu path: `Tools > Grid Asset Slicer > Open`

## Workspace Rule

`unity-grid-asset-slicer` を独立した Git repository として扱う。

`D:\Claude\UnityEditor-Dev` 全体を 1 repo として扱わない。`SoundCreater`、`ImageGridSlicer`、その他 sibling folders の変更を混ぜない。

## Before Editing

1. `git status --short --branch` をこの repository で実行する。
2. 次の順で読む。
   - `README.md`
   - `docs/requirements.md`
   - `docs/design.md`
   - `docs/slice-session-schema.md`
   - `docs/implementation-backlog.md`
   - `Skill.md`
3. unrelated user changes を戻さない。
4. public behavior、schema、menu path、package name を変える場合は docs も同時に更新する。

## Issue Branch Workflow

この repository の実装作業は Issue 単位で進める。

1. 着手する GitHub Issue を 1 つ選ぶ。
2. `main` が最新であることを確認する。
3. Issue 番号が分かる branch を切る。

```text
task/issue-<number>-<short-topic>
```

例:

```text
task/issue-2-package-scaffold
```

4. その branch で実装、テスト、docs 更新を行う。
5. 作業後に validation を実行し、結果を Issue へ記録する。
6. branch を push し、必要に応じて PR を作る。
7. 確認後、`main` へ merge する。
8. `main` へ戻り、最新状態と clean status を確認してから次の Issue に進む。

複数 Issue の変更を 1 branch に混ぜない。やむを得ず共通 docs や release 設定を触る場合も、現在の Issue の受け入れ条件に必要な範囲へ限定する。

## Agents And Skill Update Rule

各工程の完了前に、次を確認する。

- 作業で新しい恒久ルール、validation 手順、release 手順、branch 運用、package 命名、schema 方針が増えたか。
- 増えた場合は `Agents.md` または `Skill.md` を同じ branch で更新する。
- 一時的な作業メモや今回限りのログは `Agents.md` / `Skill.md` に入れない。
- Issue 完了コメントには、`Agents.md` / `Skill.md` を更新したか、更新不要だったかを明記する。

## Implementation Principles

- Grid math と file naming は pure C# service に置く。
- Editor window は service 呼び出しに徹する。
- Session metadata は versioned JSON とする。
- OpenCV auto-detection は optional backend として隔離する。
- 画像や metadata を network service へ送信しない。
- 既存ファイルの上書きは user が `overwrite` を選んだ場合だけ許可する。

## Naming

Package:

```text
com.sunmax0731.grid-asset-slicer
```

Namespaces:

```text
Sunmax.GridAssetSlicer
Sunmax.GridAssetSlicer.Editor
```

## Validation

EditMode tests:

- Grid calculation
- Session JSON serialization
- Export file naming
- Conflict behavior
- Export planning

Manual Unity validation:

- Editor window opens
- Preview renders
- Cell selection works
- Real PNG export works
- AssetDatabase refresh works

## Stop Boundaries

次の場合は明確に止めて報告する。

- Unity executable が見つからない。
- 作業前から project が compile できない。
- 既存 local changes が依頼内容と衝突する。
- GitHub operation に認証が必要で実行できない。
