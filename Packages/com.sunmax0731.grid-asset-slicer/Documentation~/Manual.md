# Unity Grid Asset Slicer Manual

Japanese version: [Manual.ja.md](Manual.ja.md)

Related documents:

- [TermsOfUse.md](TermsOfUse.md)
- [ReleaseNotes.md](ReleaseNotes.md)
- [ValidationChecklist.md](ValidationChecklist.md)

## Overview

Unity Grid Asset Slicer is a Unity Editor extension for slicing grid-based PNG source images into individual PNG files.
Slice settings can be saved as session JSON so the same source can be replayed with the same settings.

## Requirements

- Unity `6000.0` or later
- Windows Unity Editor is the primary supported environment
- Offline use is supported

## Installation

Place this package under the Unity project `Packages/` directory.

```text
Packages/com.sunmax0731.grid-asset-slicer/
```

The package is also intended to work as a local package or Git URL package through Package Manager.

## Launch

Open the editor window from:

```text
Tools > Grid Asset Slicer > Open
```

## Basic Workflow

1. Assign a PNG texture to `Source Image`.
2. Enter `Rows` and `Columns`.
3. Set margins, gutters, cell width, and cell height as needed.
4. Click `Preview` and check the generated cells in the separate preview window.
5. Right-click cells that should be excluded.
6. Set `Output Folder`, `File Prefix`, `Start Index`, `Number Padding`, and `Conflict`.
7. Run `Export PNGs`.
8. Use `Save Session` when the setup should be replayed later.

## Grid Settings

- Use `Open Help Window` in the `Display` section to show parameter explanations in a separate non-modal window while keeping the tool editable.
- `Rows`: vertical cell count.
- `Columns`: horizontal cell count.
- `Margin Left / Top / Right / Bottom`: pixel margins around the grid.
- `Gutter X / Y`: pixel spacing between cells.
- `Cell Width / Height`: explicit cell size. If omitted, the size is calculated from the source image, margins, gutters, rows, and columns.

Validation warnings are shown when the grid is outside the source image, but preview remains visible when cell rectangles can still be calculated. Preview is unavailable only when the settings cannot produce any cell rectangles, such as invalid row or column counts or unresolved implicit cell sizes.

## Preview and Exclusion

The preview uses the same grid calculation as export.
Click `Preview` to open the preview in a separate resizable window. The main tool window does not draw the grid directly and remains editable while the preview window is open.

- Left-click: select a cell.
- Right-click: toggle include / exclude.

Excluded cells are skipped during export and saved into session JSON.

The Cell Inspector includes a selected-cell preview. Use `Show Outline`, `Background Color`, and `Outline Color` to adjust the inspector preview display.

## Export Settings

Export settings are shown under the `Workspace` section in the main window.

- `Output Folder`: target folder for generated PNG files.
- `File Prefix`: prefix for generated file names.
- `Start Index`: first sequence number.
- `Number Padding`: zero-padding digits for sequence numbers.
- `Conflict`: behavior when a target file already exists.

Conflict behavior:

- `Overwrite`: write over existing files.
- `Skip`: leave existing files and skip those cells.
- `Duplicate`: write to a new `_copy01` style name.

If export fails for a cell, the quality report includes the affected cell, output path, and reported failure reason.
When the source texture has Read/Write disabled, export creates a temporary readable copy and uses that copy for slicing without changing the original asset importer setting.

Use the `Report Height` slider above the quality report to adjust the visible report area.

## Session JSON

`Save Session` stores source path, grid settings, export settings, and excluded cells.
`Load Session` restores those values for replay export.

The current session JSON format version is `1`.

## Samples

`Samples~/` contains validation PNGs and session JSON files.

- `BasicGrid`: basic 2x2 grid sample.
- `GuttersAndMargins`: gutter and margin samples.
- `TransparentCells`: sample with a transparent cell.

## Known Limitations

- The MVP focuses on PNG grid slicing.
- Irregular atlases, polygon packing, and sprite metadata generation are out of scope.
- UX tuning for very large images or very large cell counts is future work.
