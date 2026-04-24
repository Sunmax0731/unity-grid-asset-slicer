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
2. Open `Tools > Grid Asset Slicer`.
3. Select `Packages/com.sunmax0731.grid-asset-slicer/Samples~/BasicGrid/basic-grid-2x2.png` as the source image.
4. Set rows to `2`, columns to `2`, cell width to `32`, and cell height to `32`.
5. Confirm the preview shows four cells matching the source image.
6. Right-click one cell to exclude it and confirm the cell is visually marked as excluded.
7. Export to `Assets/Generated/GridSlicer/Smoke`.
8. Confirm the exported PNG count matches the included cell count.
9. Save a session JSON from the window.
10. Close and reopen the window.
11. Load the saved session JSON.
12. Confirm the source path, grid settings, export settings, selected/excluded cell state, and output names are restored.
13. Export again to a clean folder and confirm the file count and names match the replayed session.
14. Repeat with the gutter, margin, and transparent-cell samples under `Samples~/`.

## Release Gate Notes

- Keep the generated result XML and log path in the release notes or Issue comment.
- Revert unrelated Unity-generated project setting changes before committing.
- The release packaging script should call `tools/validation/run-editmode-tests.ps1` before creating the release zip or unitypackage.
