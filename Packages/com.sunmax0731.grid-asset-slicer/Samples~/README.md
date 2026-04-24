# Unity Grid Asset Slicer Samples

These samples are small deterministic fixtures for manual validation, release review, and replay tests.

## Contents

- `BasicGrid/basic-grid-2x2.png`: 64 x 64 image with four 32 x 32 colored cells.
- `GuttersAndMargins/gutters-3x2.png`: 52 x 34 image with six 16 x 16 cells and 2 px gutters.
- `GuttersAndMargins/margins-2x2.png`: 46 x 46 image with 4/6/4/2 px margins, 2 px gutters, and one excluded cell in the session.
- `TransparentCells/transparent-2x2.png`: 64 x 64 image with one transparent cell.

Each `.session.json` file can be loaded from the editor window and replayed into `Assets/Generated/GridSlicer/Samples/...`.
