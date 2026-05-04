# Validation Checklist

Use this checklist before release packaging and before BOOTH upload.

## Automated Gate

Run from the repository root:

```powershell
.\tools\validation\run-editmode-tests.ps1
```

The gate must fail when Unity returns a non-zero exit code, the result XML is missing or empty, zero tests are executed, or any EditMode test fails.

## Manual Smoke

1. Open the project in Unity.
2. Open `Tools > Grid Asset Slicer > メイン画面`.
3. Select `Packages/com.sunmax0731.grid-asset-slicer/Samples~/BasicGrid/basic-grid-2x2.png` as the source image.
4. Set rows to `2`, columns to `2`, cell width to `32`, and cell height to `32`, then change rows and columns once via the sliders and return them to `2`.
5. Drag the margin guides in the larger `Margin Controller` and confirm the four margin fields update immediately.
6. Enable `Custom Columns` or `Custom Rows`, move at least one boundary slider, and confirm the values change independently.
7. Collapse and expand multiple left-pane foldouts and confirm the layout remains readable without overlapping controls.
8. Clear the source image once and confirm the center workspace shows an empty-state message that explains the next step.
9. Confirm the preview shows four cells matching the source image.
10. Right-click one cell to exclude it and confirm the cell is visually marked as excluded.
11. Enable `Output Width` and `Output Height`, set a new size such as `64 x 64`, export once, and confirm every written PNG uses the configured dimensions.
12. Export to `Assets/Generated/GridSlicer/Smoke`.
13. Confirm the exported PNG count matches the included cell count.
14. Save a session JSON from the window.
15. Close and reopen the window.
16. Load the saved session JSON.
17. Confirm the source path, grid settings, custom boundary settings, export settings, selected/excluded cell state, output names, and export resize settings are restored.
18. Export again to a clean folder and confirm the file count and names match the replayed session.
19. Select `Packages/com.sunmax0731.grid-asset-slicer/Samples~/GuttersAndMargins/gutters-3x2.png`, set `Rows=2`, `Columns=4`, leave `Cell Width` and `Cell Height` off, and confirm non-divisible implicit preview/export still succeed.
20. Repeat with the margin and transparent-cell samples under `Samples~/`.

## Release Gate Notes

- Keep the generated result XML and log path in the release notes or Issue comment.
- Revert unrelated Unity-generated project setting changes before committing.
- The release packaging script should call `tools/validation/run-editmode-tests.ps1` before creating the release zip or unitypackage.
