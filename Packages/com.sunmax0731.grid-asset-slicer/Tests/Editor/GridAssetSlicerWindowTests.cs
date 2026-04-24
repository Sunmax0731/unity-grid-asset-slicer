using NUnit.Framework;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace Sunmax.GridAssetSlicer.Editor.Tests
{
    public sealed class GridAssetSlicerWindowTests
    {
        [Test]
        public void Open_IsRegisteredInToolsMenu()
        {
            var method = typeof(GridAssetSlicerWindow).GetMethod(
                "Open",
                BindingFlags.Public | BindingFlags.Static);

            Assert.That(method, Is.Not.Null);

            var menuItem = method.GetCustomAttributes<MenuItem>()
                .SingleOrDefault(attribute => attribute.menuItem == "Tools/Grid Asset Slicer");

            Assert.That(menuItem, Is.Not.Null);
        }
    }
}
