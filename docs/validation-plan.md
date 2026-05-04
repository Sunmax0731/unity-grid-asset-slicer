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
- Non-divisible implicit grid calculation
- Variable row / column boundary persistence
- Session JSON round-trip
- File naming and conflict behavior
- Export plan generation
- PNG export with uneven cell sizes

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
2. `Tools > Grid Asset Slicer > メイン画面` を開く。
3. `Packages/com.sunmax0731.grid-asset-slicer/Samples~/BasicGrid/basic-grid-2x2.png` を選択する。
4. rows / columns を `2 / 2` にし、slider と数値 field の両方で値変更できることを確認する。
5. left pane の大きめの margin controller で余白ガイドをドラッグし、`Margin Left / Top / Right / Bottom` が同期して変わることを確認する。
6. `Custom Columns` または `Custom Rows` を有効化し、区切り位置 slider で各列幅 / 各行高を個別に変えられることを確認する。
7. `Preview` を押し、preview を確認する。
8. 少なくとも 1 cell を除外する。
9. `Assets/Generated/GridSlicer/Smoke` に export する。
10. PNG 数が selected cells と一致することを確認する。
11. `slice-session.json` を保存する。
12. window を閉じて再度開く。
13. session JSON を load する。
14. custom boundary 設定も含めて復元されることを確認する。
15. clean folder へ再 export し、file count と names を比較する。
16. `Packages/com.sunmax0731.grid-asset-slicer/Samples~/GuttersAndMargins/gutters-3x2.png` を選択し、`Rows=2`、`Columns=4`、`Cell Width/Height=OFF` の状態で non-divisible implicit grid preview と export が成立することを確認する。

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

