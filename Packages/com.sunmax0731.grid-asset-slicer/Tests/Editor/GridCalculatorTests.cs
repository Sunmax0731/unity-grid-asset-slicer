using System.Linq;
using NUnit.Framework;

namespace Sunmax.GridAssetSlicer.Editor.Tests
{
    public sealed class GridCalculatorTests
    {
        [Test]
        public void Calculate_ReturnsCellsForValidGrid()
        {
            var settings = new GridSettings
            {
                Rows = 2,
                Columns = 3,
                MarginLeft = 10,
                MarginTop = 20,
                MarginRight = 10,
                MarginBottom = 20,
                GutterX = 5,
                GutterY = 10
            };

            var result = GridCalculator.Calculate(348, 250, settings);

            Assert.That(result.IsValid, Is.True, string.Join("\n", result.Errors));
            Assert.That(result.Cells, Has.Count.EqualTo(6));
            Assert.That(result.Cells[0], Is.EqualTo(new CellRect(new CellCoordinate(0, 0), 10, 20, 106, 100)));
            Assert.That(result.Cells[1], Is.EqualTo(new CellRect(new CellCoordinate(0, 1), 121, 20, 106, 100)));
            Assert.That(result.Cells[5], Is.EqualTo(new CellRect(new CellCoordinate(1, 2), 232, 130, 106, 100)));
        }

        [Test]
        public void Calculate_UsesExplicitCellSizeWhenProvided()
        {
            var settings = new GridSettings
            {
                Rows = 2,
                Columns = 2,
                MarginLeft = 4,
                MarginTop = 6,
                MarginRight = 4,
                MarginBottom = 6,
                GutterX = 2,
                GutterY = 3,
                CellWidth = 16,
                CellHeight = 20
            };

            var result = GridCalculator.Calculate(128, 128, settings);

            Assert.That(result.IsValid, Is.True, string.Join("\n", result.Errors));
            Assert.That(result.Cells.Select(cell => cell.Width), Is.All.EqualTo(16));
            Assert.That(result.Cells.Select(cell => cell.Height), Is.All.EqualTo(20));
            Assert.That(result.Cells[3], Is.EqualTo(new CellRect(new CellCoordinate(1, 1), 22, 29, 16, 20)));
        }

        [Test]
        public void Calculate_UsesCustomColumnWidthsAndRowHeightsWhenProvided()
        {
            var settings = new GridSettings
            {
                Rows = 2,
                Columns = 3,
                MarginLeft = 2,
                MarginTop = 4,
                GutterX = 1,
                GutterY = 2,
                ColumnWidths = new[] { 10, 14, 8 },
                RowHeights = new[] { 12, 18 }
            };

            var result = GridCalculator.Calculate(40, 40, settings);

            Assert.That(result.IsValid, Is.True, string.Join("\n", result.Errors));
            Assert.That(result.Cells[0], Is.EqualTo(new CellRect(new CellCoordinate(0, 0), 2, 4, 10, 12)));
            Assert.That(result.Cells[1], Is.EqualTo(new CellRect(new CellCoordinate(0, 1), 13, 4, 14, 12)));
            Assert.That(result.Cells[5], Is.EqualTo(new CellRect(new CellCoordinate(1, 2), 28, 18, 8, 18)));
        }

        [TestCase(0, 1, "Rows must be greater than zero.")]
        [TestCase(1, 0, "Columns must be greater than zero.")]
        public void Calculate_RejectsInvalidRowsOrColumns(int rows, int columns, string expectedError)
        {
            var settings = CreateBasicSettings();
            settings.Rows = rows;
            settings.Columns = columns;

            var result = GridCalculator.Calculate(64, 64, settings);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Cells, Is.Empty);
            Assert.That(result.Errors, Does.Contain(expectedError));
        }

        [Test]
        public void Calculate_RejectsNegativeMarginsAndGutters()
        {
            var settings = CreateBasicSettings();
            settings.MarginLeft = -1;
            settings.MarginTop = -2;
            settings.GutterX = -3;
            settings.GutterY = -4;

            var result = GridCalculator.Calculate(64, 64, settings);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors, Does.Contain("MarginLeft must be zero or greater."));
            Assert.That(result.Errors, Does.Contain("MarginTop must be zero or greater."));
            Assert.That(result.Errors, Does.Contain("GutterX must be zero or greater."));
            Assert.That(result.Errors, Does.Contain("GutterY must be zero or greater."));
        }

        [Test]
        public void Calculate_RejectsCustomSizeArraysWhenLengthDoesNotMatchCounts()
        {
            var settings = CreateBasicSettings();
            settings.ColumnWidths = new[] { 10 };
            settings.RowHeights = new[] { 20, 20, 20 };

            var result = GridCalculator.Calculate(64, 64, settings);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors, Does.Contain("ColumnWidths length must match the configured cell count."));
            Assert.That(result.Errors, Does.Contain("RowHeights length must match the configured cell count."));
        }

        [Test]
        public void Calculate_RejectsExplicitGridOutsideImageBounds()
        {
            var settings = CreateBasicSettings();
            settings.Columns = 3;
            settings.CellWidth = 30;
            settings.CellHeight = 16;
            settings.GutterX = 5;
            settings.MarginLeft = 10;
            settings.MarginRight = 10;

            var result = GridCalculator.Calculate(100, 64, settings);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Cells, Has.Count.EqualTo(6));
            Assert.That(result.Cells[2], Is.EqualTo(new CellRect(new CellCoordinate(0, 2), 80, 0, 30, 16)));
            Assert.That(result.Errors.Any(error => error.Contains("Grid width exceeds image bounds")), Is.True);
        }

        [Test]
        public void Calculate_AllowsNonDivisibleImplicitGrid()
        {
            var settings = CreateBasicSettings();
            settings.Columns = 3;

            var result = GridCalculator.Calculate(100, 64, settings);

            Assert.That(result.IsValid, Is.True, string.Join("\n", result.Errors));
            Assert.That(result.Cells, Has.Count.EqualTo(6));
            Assert.That(result.Cells.Where(cell => cell.Coordinate.Row == 0).Select(cell => cell.Width), Is.EqualTo(new[] { 33, 34, 33 }));
            Assert.That(result.Cells.Where(cell => cell.Coordinate.Column == 0).Select(cell => cell.Height), Is.All.EqualTo(32));
            Assert.That(result.Cells[2], Is.EqualTo(new CellRect(new CellCoordinate(0, 2), 67, 0, 33, 32)));
            Assert.That(result.Cells[5], Is.EqualTo(new CellRect(new CellCoordinate(1, 2), 67, 32, 33, 32)));
        }

        [Test]
        public void Calculate_RejectsImplicitGridWhenAvailablePixelsAreFewerThanCells()
        {
            var settings = CreateBasicSettings();
            settings.Rows = 1;
            settings.Columns = 3;

            var result = GridCalculator.Calculate(2, 8, settings);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors, Does.Contain("Available grid width must allocate at least one pixel to every cell."));
        }

        private static GridSettings CreateBasicSettings()
        {
            return new GridSettings
            {
                Rows = 2,
                Columns = 2,
                MarginLeft = 0,
                MarginTop = 0,
                MarginRight = 0,
                MarginBottom = 0,
                GutterX = 0,
                GutterY = 0
            };
        }
    }
}
