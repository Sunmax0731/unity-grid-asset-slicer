# Unity Grid Asset Slicer Release Plan

## 1. Release Goal

Unity Grid Asset Slicer を Unity Editor 拡張として実装し、GitHub Release と BOOTH で同一成果物を配布できる状態にする。

現在のリリース対象は `v0.1.2` とする。

## 2. Reference Project

SoundCreater / Torus Edison の release flow を参考にする。

継承する方針:

- UPM package を Unity project 内に置く。
- package version を release version の source of truth にする。
- EditMode tests を release gate に含める。
- manual validation checklist を同梱する。
- GitHub Release body と BOOTH description を repo 内の `tools/release/` に置く。
- GitHub Release artifact と BOOTH artifact は同一内容にする。
- release zip と unitypackage の両方を作る。
- temp copy または release staging folder で packaging を確認する。

## 3. Target Artifact Names

初期案:

```text
UnityGridAssetSlicer-<version>.unitypackage
UnityGridAssetSlicer-<version>-release.zip
ReleaseBuilds/UnityGridAssetSlicer-<version>/
```

## 4. Release Zip Contents

初回 release zip は以下を含める。

- `README.md`
- `Manual.ja.md`
- `Manual.md`
- `TermsOfUse.md`
- `ReleaseNotes.md`
- `ValidationChecklist.md`
- `CHANGELOG.md`
- `LICENSE.md`
- `Samples/`
- `UnityGridAssetSlicer-<version>.unitypackage`

## 5. Required Package Docs

Package 内に次を用意する。

```text
Packages/com.sunmax0731.grid-asset-slicer/
  Documentation~/
    Manual.ja.md
    Manual.md
    TermsOfUse.md
    ReleaseNotes.md
    ValidationChecklist.md
  Samples~/
    README.md
    BasicGrid/
    GuttersAndMargins/
```

## 6. BOOTH Preparation

`tools/release/BOOTHDescription.ja.md` を販売文面の source とする。

BOOTH 文面に含める項目:

- 何をする Unity Editor 拡張か
- 想定利用者
- 主な機能
- 同梱内容
- 対応環境
- 現在の対象外機能
- 購入前の注意
- サポート時に必要な情報
- GitHub Release と BOOTH 配布物が同一内容であること

## 7. Release Gate

初回 release-ready 条件:

- UPM package scaffold が clean Unity project で import できる。
- Grid calculation、session JSON、file naming、conflict behavior、export planning の EditMode tests が通る。
- sample grid image で manual export smoke が通る。
- saved session JSON から同条件で replay できる。
- release packaging script が validation gate を通して unitypackage と zip を作れる。
- release zip に expected files が含まれる。
- README、manual、terms、release notes、validation checklist、BOOTH description が current implementation と一致している。

## 8. GitHub Issue Roadmap

Release までの作業は以下の順で進める。

1. Foundation package scaffold
2. Core grid model and calculation
3. Slice session JSON persistence
4. Export naming and conflict behavior
5. PNG export service
6. Editor window source, grid, preview, selection workflow
7. Sample assets and replay fixtures
8. Validation scripts and release gate
9. User documentation, terms, and BOOTH copy
10. Release packaging script and artifact verification
11. BOOTH / GitHub release final checklist

## 9. Issue Execution Policy

各工程は GitHub Issue 単位で進める。

- 1 Issue につき 1 branch を切る。
- branch 名は `task/issue-<number>-<short-topic>` を使う。
- 作業後は validation 結果を Issue に記録する。
- 確認後、branch を `main` に merge する。
- merge 後、次の Issue に進む前に `main` の clean status を確認する。
- 工程中に恒久的な運用知識が増えた場合は、同じ branch で `Agents.md` または `Skill.md` を更新する。
- Issue 完了時に、`Agents.md` / `Skill.md` の更新有無を明記する。
