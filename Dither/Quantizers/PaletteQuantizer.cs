namespace Dither.Quantizers;

public class PaletteQuantizer : IQuantizer
{
    public float[,] Palette { get; }
    
    public PaletteQuantizer(float[,] palette)
    {
        Palette = palette;
    }
    
    public float[] Quantize(float[] pixels)
    {
        const int colorCount = 3;
        var result = new float[pixels.Length];

        for (var i = 0; i < pixels.Length; i += colorCount)
        {
            var r = pixels[i];
            var g = pixels[i + 1];
            var b = pixels[i + 2];

            var nearestIndex = 0;
            var minDistance = float.MaxValue;

            for (var j = 0; j < Palette.GetLength(0); j++)
            {
                var dr = r - Palette[j, 0];
                var dg = g - Palette[j, 1];
                var db = b - Palette[j, 2];

                var distance = dr * dr + dg * dg + db * db;
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestIndex = j;
                }
            }

            result[i] = Palette[nearestIndex, 0];
            result[i + 1] = Palette[nearestIndex, 1];
            result[i + 2] = Palette[nearestIndex, 2];
        }

        return result;
    }
}