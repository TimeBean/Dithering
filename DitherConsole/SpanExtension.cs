using System.Runtime.InteropServices;
using SkiaSharp;

namespace DitherConsole;

public static class SpanExtension
{
    extension (Span<byte> pixels)
    {
        public int GetColorCount()
        {
            var colors = MemoryMarshal.Cast<byte, SKColor>(pixels);

            var set = new HashSet<SKColor>();
            foreach (var color in colors)
                set.Add(color);

            return set.Count;
        }
    }
}