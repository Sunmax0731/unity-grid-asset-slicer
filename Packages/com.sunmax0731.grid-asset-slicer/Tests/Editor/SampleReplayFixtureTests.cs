using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Sunmax.GridAssetSlicer.Editor.Tests
{
    public sealed class SampleReplayFixtureTests
    {
        private static readonly IReadOnlyDictionary<string, string[]> ExpectedOutputs =
            new Dictionary<string, string[]>
            {
                ["BasicGrid/basic-grid-2x2.session.json"] = new[]
                {
                    "Assets/Generated/GridSlicer/Samples/BasicGrid/basic_001.png",
                    "Assets/Generated/GridSlicer/Samples/BasicGrid/basic_002.png",
                    "Assets/Generated/GridSlicer/Samples/BasicGrid/basic_003.png",
                    "Assets/Generated/GridSlicer/Samples/BasicGrid/basic_004.png"
                },
                ["GuttersAndMargins/gutters-3x2.session.json"] = new[]
                {
                    "Assets/Generated/GridSlicer/Samples/Gutters/gutter_00.png",
                    "Assets/Generated/GridSlicer/Samples/Gutters/gutter_01.png",
                    "Assets/Generated/GridSlicer/Samples/Gutters/gutter_02.png",
                    "Assets/Generated/GridSlicer/Samples/Gutters/gutter_03.png",
                    "Assets/Generated/GridSlicer/Samples/Gutters/gutter_04.png",
                    "Assets/Generated/GridSlicer/Samples/Gutters/gutter_05.png"
                },
                ["GuttersAndMargins/margins-2x2.session.json"] = new[]
                {
                    "Assets/Generated/GridSlicer/Samples/Margins/margin_01.png",
                    "Assets/Generated/GridSlicer/Samples/Margins/margin_02.png",
                    "Assets/Generated/GridSlicer/Samples/Margins/margin_03.png"
                },
                ["TransparentCells/transparent-2x2.session.json"] = new[]
                {
                    "Assets/Generated/GridSlicer/Samples/Transparent/alpha_001.png",
                    "Assets/Generated/GridSlicer/Samples/Transparent/alpha_002.png",
                    "Assets/Generated/GridSlicer/Samples/Transparent/alpha_003.png",
                    "Assets/Generated/GridSlicer/Samples/Transparent/alpha_004.png"
                }
            };

        [Test]
        public void SampleSessions_ReplayExpectedExportNames()
        {
            var projectRoot = Directory.GetCurrentDirectory();
            var samplesRoot = Path.Combine(
                projectRoot,
                "Packages",
                "com.sunmax0731.grid-asset-slicer",
                "Samples~");

            foreach (var pair in ExpectedOutputs)
            {
                var sessionPath = Path.Combine(samplesRoot, pair.Key.Replace('/', Path.DirectorySeparatorChar));
                Assert.That(File.Exists(sessionPath), Is.True, sessionPath);

                var loadResult = SliceSessionSerializer.FromJson(File.ReadAllText(sessionPath));
                Assert.That(loadResult.IsValid, Is.True, string.Join("\n", loadResult.Errors));

                var session = loadResult.Session;
                AssertSourceImageMatchesSession(projectRoot, session);

                var grid = GridCalculator.Calculate(session.Source.Width, session.Source.Height, session.Grid);
                Assert.That(grid.IsValid, Is.True, string.Join("\n", grid.Errors));

                var includedCells = grid.Cells
                    .Select(cell => cell.Coordinate)
                    .Where(coordinate => !session.Selection.ExcludedCells.Contains(coordinate))
                    .ToArray();

                var plan = ExportFileNameResolver.BuildPlan(session.Export, includedCells, _ => false);
                Assert.That(plan.IsValid, Is.True, string.Join("\n", plan.Errors));
                Assert.That(plan.Items.Select(item => item.OutputPath), Is.EqualTo(pair.Value));
            }
        }

        private static void AssertSourceImageMatchesSession(string projectRoot, GridSliceSession session)
        {
            var imagePath = Path.Combine(projectRoot, session.Source.AssetPath.Replace('/', Path.DirectorySeparatorChar));
            Assert.That(File.Exists(imagePath), Is.True, imagePath);

            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            try
            {
                Assert.That(ImageConversion.LoadImage(texture, File.ReadAllBytes(imagePath)), Is.True);
                Assert.That(texture.width, Is.EqualTo(session.Source.Width));
                Assert.That(texture.height, Is.EqualTo(session.Source.Height));
            }
            finally
            {
                Object.DestroyImmediate(texture);
            }
        }
    }
}
