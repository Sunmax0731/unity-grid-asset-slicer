---
name: unity-grid-asset-slicer
description: Use this when implementing, testing, documenting, or releasing the Unity Grid Asset Slicer Unity Editor extension.
---

# Unity Grid Asset Slicer Skill

## When To Use

Use this skill for work in:

```text
D:\Claude\UnityEditor-Dev\unity-grid-asset-slicer
```

Primary tasks:

- Requirements refinement
- UPM package scaffold
- Grid slicing core logic
- Session JSON persistence
- PNG export
- Editor window workflow
- Validation and release preparation

## Required Context

Read these files before implementation:

1. `Agents.md`
2. `docs/requirements.md`
3. `docs/design.md`
4. `docs/slice-session-schema.md`
5. `docs/implementation-backlog.md`
6. `docs/Image.png` when implementing Editor UI, preview, inspector, quality report, or workflow layout

## Recommended Work Order

1. Confirm repository status.
2. Choose exactly one GitHub issue or one backlog item from `docs/implementation-backlog.md`.
3. Start from updated `main`.
4. Create an issue branch named `task/issue-<number>-<short-topic>`.
5. Implement the smallest complete slice.
6. Add or update tests.
7. Run available validation.
8. Update docs if behavior changed.
9. Check whether `Agents.md` or `Skill.md` needs a process update.
10. Push the branch, merge to `main` after review or confirmation, then return to clean `main`.
11. Report changed files, validation result, merge status, and remaining risks.

## Issue Branch And Merge Rule

Do not implement directly on `main` except for explicit emergency documentation fixes requested by the user.

For normal work:

```text
git switch main
git pull --ff-only
git switch -c task/issue-<number>-<short-topic>
```

After the issue is complete:

```text
git switch main
git pull --ff-only
git merge --ff-only task/issue-<number>-<short-topic>
git push origin main
```

If fast-forward merge is not possible, stop and inspect rather than forcing history.

Each issue completion should include an Issue comment or final report with:

- validation result
- branch name
- merge result
- whether `Agents.md` / `Skill.md` was updated or intentionally left unchanged

## GitHub Issue Language

Post GitHub Issues and Issue comments in Japanese.

Use English only where it is clearer as a literal identifier, such as code symbols, file paths, command names, package names, enum values, branch names, validation output, release artifact names, or quoted error text.

## Architecture Rules

- Put reusable logic in services, not in the Editor window.
- Keep Unity-specific texture and AssetDatabase operations behind adapters.
- Use versioned JSON for session persistence.
- Keep OpenCV and other image-analysis dependencies optional.
- Avoid project-specific item, skill, or game catalog dependencies.
- Use `docs/Image.png` as the visual reference for the Editor UI layout and interaction model. Match the workflow structure, but keep implementation scoped to each Issue.
- For UI help, prefer inline or docked explanations that do not block parameter editing, preview interaction, or export setup.

## First Implementation Target

Start with:

```text
Packages/com.sunmax0731.grid-asset-slicer/
```

Create:

- `package.json`
- Runtime asmdef
- Editor asmdef
- Core model files
- Grid calculator
- EditMode tests for grid calculation

Do not start with preview UI before the data model and grid calculator are stable.

## Validation Command

After tests exist:

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.3.13f1\Editor\Unity.exe" `
  -batchmode `
  -projectPath "D:\Claude\UnityEditor-Dev\unity-grid-asset-slicer" `
  -runTests `
  -testPlatform EditMode `
  -testResults ".tmp\unity-grid-asset-slicer-EditMode.xml" `
  -logFile ".tmp\unity-grid-asset-slicer-EditMode.log" `
  -quit
```

If the exact Unity path differs, search under `C:\Program Files\Unity`.

## Completion Report

Always include:

- What changed
- Files touched
- Validation run
- Any validation not run and why
- Remaining next step
