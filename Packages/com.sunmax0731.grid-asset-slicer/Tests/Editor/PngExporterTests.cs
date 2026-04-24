using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Sunmax.GridAssetSlicer.Editor.Tests
{
    public sealed class PngExporterTests
    {
        private string _tempRoot;

        [SetUp]
        public void SetUp()
        {
            _tempRoot = Path.Combine(Path.GetTempPath(), "UnityGridAssetSlicerTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempRoot);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_tempRoot))
            {
                Directory.Delete(_tempRoot, true);
            }
        }

        [Test]
        public void Export_WritesSelectedCellsAsPngFiles()
        {
            var source = CreateFixtureTexture();
            try
            {
                var request = CreateRequest(source, new SliceSelection());

                var result = PngExporter.Export(request);

                Assert.That(result.IsSuccess, Is.True, string.Join("\n", result.Errors));
                Assert.That(result.ExportedFiles, Has.Count.EqualTo(4));
                Assert.That(result.SkippedFiles, Is.Empty);
                Assert.That(result.ExportedPaths.All(File.Exists), Is.True);
                AssertPngColor(result.ExportedPaths[0], Color.red);
                AssertPngColor(result.ExportedPaths[3], Color.yellow);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(source);
            }
        }

        [Test]
        public void Export_DoesNotWriteExcludedCells()
        {
            var source = CreateFixtureTexture();
            try
            {
                var selection = new SliceSelection();
                selection.ExcludedCells.Add(new CellCoordinate(0, 1));
                selection.ExcludedCells.Add(new CellCoordinate(1, 0));
                var request = CreateRequest(source, selection);

                var result = PngExporter.Export(request);

                Assert.That(result.IsSuccess, Is.True, string.Join("\n", result.Errors));
                Assert.That(result.ExportedFiles, Has.Count.EqualTo(2));
                Assert.That(result.ExportedFiles.Select(file => file.Cell), Is.EquivalentTo(new[]
                {
                    new CellCoordinate(0, 0),
                    new CellCoordinate(1, 1)
                }));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(source);
            }
        }

        [Test]
        public void Export_ReturnsSkippedFilesFromPlan()
        {
            var source = CreateFixtureTexture();
            try
            {
                var existingPath = Path.Combine(_tempRoot, "cell_001.png").Replace('\\', '/');
                File.WriteAllBytes(existingPath, new byte[] { 1, 2, 3 });
                var request = CreateRequest(source, new SliceSelection());

                var result = PngExporter.Export(request);

                Assert.That(result.IsSuccess, Is.True, string.Join("\n", result.Errors));
                Assert.That(result.SkippedFiles, Has.Count.EqualTo(1));
                Assert.That(result.SkippedFiles[0].OutputPath, Is.EqualTo(existingPath));
                Assert.That(result.ExportedFiles, Has.Count.EqualTo(3));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(source);
            }
        }

        [Test]
        public void Export_ReturnsFailureWithCellAndPathForInvalidBounds()
        {
            var source = CreateFixtureTexture();
            try
            {
                var cells = new[]
                {
                    new CellRect(new CellCoordinate(0, 0), 0, 0, 99, 99)
                };
                var request = new PngExportRequest(source, cells, new SliceSelection(), CreateExportSettings());

                var result = PngExporter.Export(request);

                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Errors[0], Does.Contain("(0, 0)"));
                Assert.That(result.Errors[0], Does.Contain("cell_001.png"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(source);
            }
        }

        private PngExportRequest CreateRequest(Texture2D source, SliceSelection selection)
        {
            var grid = new GridSettings
            {
                Rows = 2,
                Columns = 2,
                CellWidth = 2,
                CellHeight = 2
            };
            var cells = GridCalculator.Calculate(source.width, source.height, grid).Cells;
            return new PngExportRequest(source, cells, selection, CreateExportSettings());
        }

        private ExportSettings CreateExportSettings()
        {
            return new ExportSettings
            {
                OutputFolder = _tempRoot.Replace('\\', '/'),
                FilePrefix = "cell_",
                StartIndex = 1,
                NumberPadding = 3,
                ConflictBehavior = ExportConflictBehavior.Skip
            };
        }

        private static Texture2D CreateFixtureTexture()
        {
            var texture = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            Fill(texture, 0, 2, Color.red);
            Fill(texture, 2, 2, Color.green);
            Fill(texture, 0, 0, Color.blue);
            Fill(texture, 2, 0, Color.yellow);
            texture.Apply(false, false);
            return texture;
        }

        private static void Fill(Texture2D texture, int x, int y, Color color)
        {
            for (var row = 0; row < 2; row++)
            {
                for (var column = 0; column < 2; column++)
                {
                    texture.SetPixel(x + column, y + row, color);
                }
            }
        }

        private static void AssertPngColor(string path, Color expected)
        {
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            try
            {
                Assert.That(ImageConversion.LoadImage(texture, File.ReadAllBytes(path)), Is.True);
                Assert.That(texture.width, Is.EqualTo(2));
                Assert.That(texture.height, Is.EqualTo(2));
                Assert.That(texture.GetPixel(0, 0), Is.EqualTo(expected));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }
    }
}
