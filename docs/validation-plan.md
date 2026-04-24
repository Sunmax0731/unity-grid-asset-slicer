# 検証計画

## 1. Environment

- Unity: 6000.4.0f1
- OS priority: Windows
- Project path: `D:\Claude\UnityEditor-Dev\unity-grid-asset-slicer`

## 2. Automated Checks

package scaffold 後、EditMode test を追加する。

初期対象:

- Grid calculation
- Validation errors
- Session JSON round-trip
- File naming and conflict behavior
- Export plan generation

## 3. Unity Batch Check

tests 作成後は次の形で EditMode tests を実行する。

```powershell
& "C:\Program Files\Unity\6000.4.0f1\Editor\Unity.exe" `
  -batchmode `
  -projectPath "D:\Claude\UnityEditor-Dev\unity-grid-asset-slicer" `
  -runTests `
  -testPlatform EditMode `
  -testResults ".tmp\unity-grid-asset-slicer-EditMode.xml" `
  -logFile ".tmp\unity-grid-asset-slicer-EditMode.log" `
  -quit
```

Unity executable が異なる場合は `C:\Program Files\Unity` 配下から探す。

## 4. Manual Smoke Path

Editor window 実装後の smoke path:

1. Unity で project を開く。
2. `Tools > Grid Asset Slicer > Open` を開く。
3. square PNG test sheet を選択する。
4. rows / columns を指定する。
5. preview を確認する。
6. 少なくとも 1 cell を除外する。
7. `Assets/Generated/GridSlicer/Smoke` に export する。
8. PNG 数が selected cells と一致することを確認する。
9. `slice-session.json` を保存する。
10. window を閉じて再度開く。
11. session JSON を load する。
12. clean folder へ再 export し、file count と names を比較する。

## 5. Test Fixtures

小さく deterministic な fixture を用意する。

- 2 x 2 grid with four distinct colors
- 3 x 2 grid with gutters
- grid with one transparent cell
- grid with margins

## 6. Completion Boundary

task 完了前に確認すること:

- docs 更新済み。
- tests または manual validation 実行済み、または未実行理由が明確。
- `git status --short --branch` で unrelated changes を確認済み。

