# Unity Grid Asset Slicer Package

Unity Grid Asset Slicer is a Unity Editor extension package for slicing a grid-based source image into individual PNG assets.

## Status

This package is currently released as `0.1.2`. Grid calculation, uneven implicit cell support, optional per-column and per-row boundaries, session JSON persistence, PNG export, optional uniform export resize, replay samples, validation, the Editor window workflow, detached preview, a foldout-based single settings pane, slider-assisted grid editing, and a draggable margin controller are in place.

## Package Name

```text
com.sunmax0731.grid-asset-slicer
```

## Public Menu Path

Menu path:

```text
Tools > Grid Asset Slicer > メイン画面
Tools > Grid Asset Slicer > ライセンス
Tools > Grid Asset Slicer > バージョン情報
```

## License

Unity Grid Asset Slicer is distributed under the MIT License. See `LICENSE.md`.

## Development Order

1. Core grid model and calculator
2. Slice session JSON persistence
3. Export naming and conflict behavior
4. PNG cell extraction and export
5. Editor window workflow
6. Validation and release packaging

See `Documentation~/` for user-facing manuals and validation notes. See the repository-level `docs/` folder for requirements, design, validation, and release planning.

