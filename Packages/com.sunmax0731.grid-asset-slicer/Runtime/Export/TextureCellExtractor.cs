using System;
using UnityEngine;

namespace Sunmax.GridAssetSlicer
{
    public static class TextureCellExtractor
    {
        public static Texture2D ExtractTexture(Texture2D sourceTexture, CellRect rect)
        {
            if (sourceTexture == null)
            {
                throw new ArgumentNullException(nameof(sourceTexture));
            }

            ValidateBounds(sourceTexture, rect);

            var unityY = sourceTexture.height - rect.Y - rect.Height;
            var pixels = sourceTexture.GetPixels(rect.X, unityY, rect.Width, rect.Height);
            var cellTexture = new Texture2D(rect.Width, rect.Height, TextureFormat.RGBA32, false);
            cellTexture.SetPixels(pixels);
            cellTexture.Apply(false, false);
            return cellTexture;
        }

        public static byte[] ExtractPng(Texture2D sourceTexture, CellRect rect)
        {
            var cellTexture = ExtractTexture(sourceTexture, rect);
            try
            {
                return cellTexture.EncodeToPNG();
            }
            finally
            {
                if (Application.isPlaying)
                {
                    UnityEngine.Object.Destroy(cellTexture);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(cellTexture);
                }
            }
        }

        private static void ValidateBounds(Texture2D sourceTexture, CellRect rect)
        {
            if (rect.Width <= 0 || rect.Height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(rect), "Cell width and height must be greater than zero.");
            }

            if (rect.X < 0 || rect.Y < 0 || rect.Right > sourceTexture.width || rect.Bottom > sourceTexture.height)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(rect),
                    $"Cell bounds {rect} exceed source texture bounds {sourceTexture.width}x{sourceTexture.height}.");
            }
        }
    }
}
