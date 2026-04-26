# Release Notes

## v0.1.1

- Unity Editor menu entries are now standardized as `Tools > Grid Asset Slicer > メイン画面`, `ライセンス`, and `バージョン情報`.
- Added a Unity Editor information window for MIT License and version details.
- Updated README, package README, manuals, validation checklist, release notes, BOOTH copy, and GitHub release copy to match the current implementation.
- Release validation target: Unity `6000.4.0f1`, EditMode tests.


## v0.1.0

Initial BOOTH and GitHub release candidate scope.

### Included

- Unity package identity: `com.sunmax0731.grid-asset-slicer`.
- Editor menu: `Tools > Grid Asset Slicer > メイン画面`.
- Grid settings for rows, columns, margins, gutters, and explicit cell size.
- Preview grid based on the same calculation used for export.
- Cell selection and include / exclude toggling.
- PNG export service.
- Export file naming with overwrite, skip, and duplicate conflict behavior.
- Session JSON save and load.
- Section dividers, wider pane spacing, and toolbar Help access for clearer editor navigation.
- Deterministic sample PNGs and replay fixtures under `Samples~/`.
- EditMode validation gate: `tools/validation/run-editmode-tests.ps1`.
- Manual validation checklist.

### Known Limitations

- MVP supports grid-based PNG slicing only.
- Irregular atlas layouts are not supported.
- Sprite metadata and Unity `Sprite` asset generation are not included.
- Batch processing multiple source images is not included.
- Very large source images and high cell counts may need future UX and performance tuning.

### Validation

Before packaging, run:

```powershell
.\tools\validation\run-editmode-tests.ps1
```

Then complete [ValidationChecklist.md](ValidationChecklist.md).
