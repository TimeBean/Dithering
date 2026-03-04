namespace Dither.Quantizers.Palette;

public sealed class LinearEuclideanPaletteQuantizer : PaletteQuantizer
{
    public LinearEuclideanPaletteQuantizer(string paletteName, float[,] palette) 
        : base(paletteName, palette) { }

    private static float ToLinear(float c)
    {
        if (c <= 0.04045f)
            return c / 12.92f;
        return (float)Math.Pow((c + 0.055f) / 1.055f, 2.4f);
    }

    protected override int GetNearestColorIndex(float red, float green, float blue)
    {
        var lr = ToLinear(red);
        var lg = ToLinear(green);
        var lb = ToLinear(blue);

        var nearestIndex = 0;
        var minDistance = float.MaxValue;
        var paletteSize = Palette.GetLength(0);

        for (var j = 0; j < paletteSize; j++)
        {
            var pr = ToLinear(Palette[j, 0]);
            var pg = ToLinear(Palette[j, 1]);
            var pb = ToLinear(Palette[j, 2]);

            var dr = lr - pr;
            var dg = lg - pg;
            var db = lb - pb;

            var distance = dr * dr + dg * dg + db * db;

            if (distance < minDistance)
            {
                minDistance = distance;
                nearestIndex = j;
            }
        }

        return nearestIndex;
    }
    
    public override string ToString()
        => $"PaletteQuantizer.{PaletteName}.LinearEuclidean";
}