namespace Dither.Quantizers.Palette;

public sealed class WeightedPaletteQuantizer : PaletteQuantizer
{
    public WeightedPaletteQuantizer(float[,] palette) : base(palette)
    {
    }

    protected override int GetNearestColorIndex(float red, float green, float blue)
    {
        var nearestIndex = 0;
        var minDistance = float.MaxValue;
        var paletteSize = Palette.GetLength(0);

        for (var j = 0; j < paletteSize; j++)
        {
            var dr = red - Palette[j, 0];
            var dg = green - Palette[j, 1];
            var db = blue - Palette[j, 2];

            // Rec. 601
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