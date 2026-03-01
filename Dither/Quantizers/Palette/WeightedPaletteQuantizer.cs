namespace Dither.Quantizers.Palette;

public class WeightedPaletteQuantizer : PaletteQuantizer
{
    public WeightedPaletteQuantizer(float[,] palette) : base(palette) { }

    protected override int GetNearestColorIndex(float r, float g, float b)
    {
        var nearestIndex = 0;
        var minDistance = float.MaxValue;
        var paletteSize = Palette.GetLength(0);

        for (var j = 0; j < paletteSize; j++)
        {
            var dr = r - Palette[j, 0];
            var dg = g - Palette[j, 1];
            var db = b - Palette[j, 2];

            // Весовые коэффициенты по стандарту Y = 0.299 R + 0.587 G + 0.114 B
            var distance = 0.299f * dr * dr + 0.587f * dg * dg + 0.114f * db * db;

            if (distance < minDistance)
            {
                minDistance = distance;
                nearestIndex = j;
            }
        }

        return nearestIndex;
    }
}