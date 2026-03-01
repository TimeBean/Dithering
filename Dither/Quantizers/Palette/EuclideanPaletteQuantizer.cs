namespace Dither.Quantizers.Palette;

public class EuclideanPaletteQuantizer : PaletteQuantizer
{
    public EuclideanPaletteQuantizer(float[,] palette) : base(palette) { }

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

            var distance = dr * dr + dg * dg + db * db;

            if (distance < minDistance)
            {
                minDistance = distance;
                nearestIndex = j;
            }
        }

        return nearestIndex;
    }
}