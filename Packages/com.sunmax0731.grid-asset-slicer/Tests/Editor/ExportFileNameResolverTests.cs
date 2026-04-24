using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Sunmax.GridAssetSlicer.Editor.Tests
{
    public sealed class ExportFileNameResolverTests
    {
        [Test]
        public void BuildPlan_CreatesPaddedSequentialPngNames()
        {
            var plan = ExportFileNameResolver.BuildPlan(
                CreateSettings(ExportConflictBehavior.Skip),
                CreateCells(3),
                _ => false);

            Assert.That(plan.IsValid, Is.True, string.Join("\n", plan.Errors));
            Assert.That(plan.Items.Select(item => item.OutputPath), Is.EqualTo(new[]
            {
                ExpectedPath("item_001.png"),
                ExpectedPath("item_002.png"),
                ExpectedPath("item_003.png")
            }));
        }

        [Test]
        public void BuildPlan_SkipsExistingFileWhenConflictBehaviorIsSkip()
        {
            var existingPath = ExpectedPath("item_001.png");

            var plan = ExportFileNameResolver.BuildPlan(
                CreateSettings(ExportConflictBehavior.Skip),
                CreateCells(2),
                path => path == existingPath);

            Assert.That(plan.IsValid, Is.True, string.Join("\n", plan.Errors));
            Assert.That(plan.Items[0].Action, Is.EqualTo(ExportActionType.Skip));
            Assert.That(plan.Items[0].TargetExisted, Is.True);
            Assert.That(plan.Items[1].Action, Is.EqualTo(ExportActionType.Write));
        }

        [Test]
        public void BuildPlan_OverwritesExistingFileOnlyWhenConflictBehaviorIsOverwrite()
        {
            var existingPath = ExpectedPath("item_001.png");

            var overwritePlan = ExportFileNameResolver.BuildPlan(
                CreateSettings(ExportConflictBehavior.Overwrite),
                CreateCells(1),
                path => path == existingPath);

            var skipPlan = ExportFileNameResolver.BuildPlan(
                CreateSettings(ExportConflictBehavior.Skip),
                CreateCells(1),
                path => path == existingPath);

            Assert.That(overwritePlan.Items[0].Action, Is.EqualTo(ExportActionType.Write));
            Assert.That(overwritePlan.Items[0].TargetExisted, Is.True);
            Assert.That(skipPlan.Items[0].Action, Is.EqualTo(ExportActionType.Skip));
        }

        [Test]
        public void BuildPlan_ResolvesDuplicateNamesDeterministically()
        {
            var existing = new HashSet<string>
            {
                ExpectedPath("item_001.png"),
                ExpectedPath("item_001_copy01.png")
            };

            var plan = ExportFileNameResolver.BuildPlan(
                CreateSettings(ExportConflictBehavior.Duplicate),
                CreateCells(1),
                existing.Contains);

            Assert.That(plan.IsValid, Is.True, string.Join("\n", plan.Errors));
            Assert.That(plan.Items[0].Action, Is.EqualTo(ExportActionType.Write));
            Assert.That(plan.Items[0].OutputPath, Is.EqualTo(ExpectedPath("item_001_copy02.png")));
        }

        [Test]
        public void BuildPlan_ReservesDuplicatePathsWithinPlan()
        {
            var settings = CreateSettings(ExportConflictBehavior.Duplicate);
            settings.StartIndex = 1;
            settings.NumberPadding = 0;

            var plan = ExportFileNameResolver.BuildPlan(
                settings,
                CreateCells(2),
                path => path == ExpectedPath("item_1.png"));

            Assert.That(plan.Items[0].OutputPath, Is.EqualTo(ExpectedPath("item_1_copy01.png")));
            Assert.That(plan.Items[1].OutputPath, Is.EqualTo(ExpectedPath("item_2.png")));
        }

        [Test]
        public void BuildPlan_ReportsInvalidSettingsBeforeTextureExtraction()
        {
            var settings = CreateSettings(ExportConflictBehavior.Skip);
            settings.OutputFolder = "";

            var plan = ExportFileNameResolver.BuildPlan(settings, CreateCells(1), _ => false);

            Assert.That(plan.IsValid, Is.False);
            Assert.That(plan.Items, Is.Empty);
            Assert.That(plan.Errors, Does.Contain("OutputFolder is required."));
        }

        private static ExportSettings CreateSettings(ExportConflictBehavior behavior)
        {
            return new ExportSettings
            {
                OutputFolder = "Assets/Generated/GridSlicer/items",
                FilePrefix = "item_",
                StartIndex = 1,
                NumberPadding = 3,
                ConflictBehavior = behavior
            };
        }

        private static IReadOnlyList<CellCoordinate> CreateCells(int count)
        {
            return Enumerable.Range(0, count)
                .Select(index => new CellCoordinate(index / 8, index % 8))
                .ToArray();
        }

        private static string ExpectedPath(string fileName)
        {
            return $"Assets/Generated/GridSlicer/items/{fileName}";
        }
    }
}
