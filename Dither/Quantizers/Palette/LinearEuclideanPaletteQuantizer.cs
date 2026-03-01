namespace Dither.Quantizers.Palette;

public class LinearEuclideanPaletteQuantizer : PaletteQuantizer
{
    public LinearEuclideanPaletteQuantizer(float[,] palette) : base(palette) { }

    private static float ToLinear(float c)
    {
        if (c <= 0.04045f)
            return c / 12.92f;
        else
            return (float)Math.Pow((c + 0.055f) / 1.055f, 2.4f);
    }

    protected override int GetNearestColorIndex(float r, float g, float b)
    {
        var lr = ToLinear(r);
        var lg = ToLinear(g);
        var lb = ToLinear(b);

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
}