namespace Dither.Quantizers.Palette;

public sealed class ManhattanPaletteQuantizer : PaletteQuantizer
{
    public ManhattanPaletteQuantizer(string paletteName, float[,] palette) 
        : base(paletteName, palette) { }

    protected override int GetNearestColorIndex(float red, float green, float blue)
    {
        var nearestIndex = 0;
        var minDistance = float.MaxValue;
        var paletteSize = Palette.GetLength(0);

        for (var j = 0; j < paletteSize; j++)
        {
            var dr = Math.Abs(red - Palette[j, 0]);
            var dg = Math.Abs(green - Palette[j, 1]);
            var db = Math.Abs(blue - Palette[j, 2]);

            var distance = dr + dg + db;

            if (distance < minDistance)
            {
                minDistance = distance;
                nearestIndex = j;
            }
        }

        return nearestIndex;
    }
    
    public override string ToString()
        => $"PaletteQuantizer.{PaletteName}.Manhattan";
}