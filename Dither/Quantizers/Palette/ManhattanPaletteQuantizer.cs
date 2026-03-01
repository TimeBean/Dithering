namespace Dither.Quantizers.Palette;

public class ManhattanPaletteQuantizer : PaletteQuantizer
{
    public ManhattanPaletteQuantizer(float[,] palette) : base(palette) { }

    protected override int GetNearestColorIndex(float r, float g, float b)
    {
        var nearestIndex = 0;
        var minDistance = float.MaxValue;
        var paletteSize = Palette.GetLength(0);

        for (var j = 0; j < paletteSize; j++)
        {
            var dr = Math.Abs(r - Palette[j, 0]);
            var dg = Math.Abs(g - Palette[j, 1]);
            var db = Math.Abs(b - Palette[j, 2]);

            var distance = dr + dg + db;

            if (distance < minDistance)
            {
                minDistance = distance;
                nearestIndex = j;
            }
        }

        return nearestIndex;
    }
}