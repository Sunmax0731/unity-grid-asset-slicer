using System.Linq;
using NUnit.Framework;

namespace Sunmax.GridAssetSlicer.Editor.Tests
{
    public sealed class SliceSessionSerializerTests
    {
        [Test]
        public void FromJson_LoadsSampleSession()
        {
            var result = SliceSessionSerializer.FromJson(CreateSampleJson());

            Assert.That(result.IsValid, Is.True, string.Join("\n", result.Errors));
            Assert.That(result.Session.Source.AssetPath, Is.EqualTo("Assets/SourceSheets/items.png"));
            Assert.That(result.Session.Grid.Rows, Is.EqualTo(2));
            Assert.That(result.Session.Grid.Columns, Is.EqualTo(2));
            Assert.That(result.Session.Export.ConflictBehavior, Is.EqualTo(ExportConflictBehavior.Duplicate));
            Assert.That(result.Session.Selection.ExcludedCells, Has.Count.EqualTo(2));
            Assert.That(result.Session.Selection.ExcludedCells[1], Is.EqualTo(new CellCoordinate(1, 1)));
        }

        [Test]
        public void ToJson_RoundTripsSession()
        {
            var session = SliceSessionSerializer.FromJson(CreateSampleJson()).Session;

            var json = SliceSessionSerializer.ToJson(session);
            var result = SliceSessionSerializer.FromJson(json);

            Assert.That(result.IsValid, Is.True, string.Join("\n", result.Errors));
            Assert.That(result.Session.FormatVersion, Is.EqualTo(GridSliceSession.CurrentFormatVersion));
            Assert.That(result.Session.Source.Width, Is.EqualTo(64));
            Assert.That(result.Session.Grid.CellWidth, Is.EqualTo(32));
            Assert.That(result.Session.Export.OutputFolder, Is.EqualTo("Assets/Generated/GridSlicer/items"));
            Assert.That(result.Session.Selection.ExcludedCells.Any(cell => cell.Equals(new CellCoordinate(0, 1))), Is.True);
        }

        [Test]
        public void FromJson_RejectsMissingSource()
        {
            var json = @"{
  ""formatVersion"": 1,
  ""grid"": { ""rows"": 1, ""columns"": 1, ""cellWidth"": 32, ""cellHeight"": 32 },
  ""export"": { ""outputFolder"": ""Assets/Generated"", ""filePrefix"": ""item_"", ""startIndex"": 1, ""numberPadding"": 3, ""conflictBehavior"": ""skip"" }
}";

            var result = SliceSessionSerializer.FromJson(json);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors, Does.Contain("source is required."));
        }

        [Test]
        public void FromJson_RejectsInvalidConflictBehavior()
        {
            var json = CreateSampleJson().Replace(@"""conflictBehavior"": ""duplicate""", @"""conflictBehavior"": ""replace""");

            var result = SliceSessionSerializer.FromJson(json);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors, Does.Contain("export.conflictBehavior is invalid: replace."));
        }

        [Test]
        public void FromJson_RejectsUnsupportedFutureFormatVersion()
        {
            var json = CreateSampleJson().Replace(@"""formatVersion"": 1", @"""formatVersion"": 99");

            var result = SliceSessionSerializer.FromJson(json);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors, Does.Contain("Unsupported formatVersion: 99."));
        }

        private static string CreateSampleJson()
        {
            return @"{
  ""formatVersion"": 1,
  ""createdUtc"": ""2026-04-24T00:00:00Z"",
  ""toolVersion"": ""0.1.0"",
  ""source"": {
    ""assetPath"": ""Assets/SourceSheets/items.png"",
    ""width"": 64,
    ""height"": 64,
    ""contentHash"": ""sample""
  },
  ""grid"": {
    ""rows"": 2,
    ""columns"": 2,
    ""marginLeft"": 0,
    ""marginTop"": 0,
    ""marginRight"": 0,
    ""marginBottom"": 0,
    ""gutterX"": 0,
    ""gutterY"": 0,
    ""cellWidth"": 32,
    ""cellHeight"": 32
  },
  ""selection"": {
    ""excludedCells"": [
      { ""row"": 0, ""column"": 1 },
      { ""row"": 1, ""column"": 1 }
    ]
  },
  ""export"": {
    ""outputFolder"": ""Assets/Generated/GridSlicer/items"",
    ""filePrefix"": ""item_"",
    ""startIndex"": 1,
    ""numberPadding"": 3,
    ""conflictBehavior"": ""duplicate""
  }
}";
        }
    }
}
