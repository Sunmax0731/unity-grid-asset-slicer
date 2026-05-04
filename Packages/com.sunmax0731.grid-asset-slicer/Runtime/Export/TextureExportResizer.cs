using System;
using UnityEngine;

namespace Sunmax.GridAssetSlicer
{
    public static class TextureExportResizer
    {
        public static Texture2D Resize(Texture2D sourceTexture, int width, int height)
        {
            if (sourceTexture == null)
            {
                throw new ArgumentNullException(nameof(sourceTexture));
            }

            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width), "Output width must be greater than zero.");
            }

            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height), "Output height must be greater than zero.");
            }

            if (sourceTexture.width == width && sourceTexture.height == height)
            {
                var copy = new Texture2D(width, height, TextureFormat.RGBA32, false);
                copy.SetPixels(sourceTexture.GetPixels());
                copy.Apply(false, false);
                return copy;
            }

            var resized = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var pixels = new Color[width * height];
            var invWidth = 1f / width;
            var invHeight = 1f / height;

            for (var y = 0; y < height; y++)
            {
                var v = (y + 0.5f) * invHeight;
                for (var x = 0; x < width; x++)
                {
                    var u = (x + 0.5f) * invWidth;
                    pixels[(y * width) + x] = sourceTexture.GetPixelBilinear(u, v);
                }
            }

            resized.SetPixels(pixels);
            resized.Apply(false, false);
            return resized;
        }
    }
}
