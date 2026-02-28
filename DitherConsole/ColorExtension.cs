using System.Runtime.InteropServices;
using SkiaSharp;

namespace DitherConsole;

public static class ColorExtension
{
    extension(Span<byte> pixels)
    {
        public int GetColorCount()
        {
            var colors = MemoryMarshal.Cast<byte, SKColor>(pixels);

            var set = new HashSet<SKColor>();
            foreach (var color in colors)
                set.Add(color);

            return set.Count;
        }
        
        public float[,] GetUniqueColors()
        {
            if (pixels.Length % 4 != 0)
                throw new ArgumentException("Pixel span length must be divisible by 4 (BGRA).");

            var set = new HashSet<(byte B, byte G, byte R, byte A)>();

            for (var i = 0; i < pixels.Length; i += 4)
            {
                var b = pixels[i + 0];
                var g = pixels[i + 1];
                var r = pixels[i + 2];
                var a = pixels[i + 3];

                set.Add((b, g, r, a));
            }

            var result = new float[set.Count, 4];
            var index = 0;
            
            foreach (var (b, g, r, a) in set)
            {
                result[index, 0] = b;
                result[index, 1] = g;
                result[index, 2] = r;
                result[index, 3] = a;
                index++;
            }

            return result;
        }
    }

    extension(float[,] palette)
    {
        public int GetColorCount()
        {
            var rows = palette.GetLength(0);
            var cols = palette.GetLength(1);

            if (cols != 3 && cols != 4)
                throw new ArgumentException("Ожидаются 3 колонки (B,G,R) или 4 колонки (B,G,R,A).");

            var set = new HashSet<(int B, int G, int R, int A)>();

            for (var i = 0; i < rows; i++)
            {
                var b = (int)Math.Round(palette[i, 0]);
                var g = (int)Math.Round(palette[i, 1]);
                var r = (int)Math.Round(palette[i, 2]);
                var a = cols == 4 ? (int)Math.Round(palette[i, 3]) : 255; 

                set.Add((b, g, r, a));
            }

            return set.Count;
        }
    }

    extension(float[,] colors)
    {
        public SKColor[] ToSkColors()
        {
            var rows = colors.GetLength(0);
            var cols = colors.GetLength(1);

            if (cols != 3 && cols != 4) 
                throw new ArgumentException("float[,] must contain BGR (3 columns) or BGRA (4 columns).", nameof(colors));

            var result = new SKColor[rows];

            for (var i = 0; i < rows; i++)
            {
                var b = (byte)colors[i, 0];
                var g = (byte)colors[i, 1];
                var r = (byte)colors[i, 2];
                var a = cols == 4 ? (byte)colors[i, 3] : (byte)255;

                result[i] = new SKColor(r, g, b, a); 
            }

            return result;
        }
    }
}