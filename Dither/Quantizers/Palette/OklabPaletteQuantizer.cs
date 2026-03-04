namespace Dither.Quantizers.Palette;

public sealed class OklabPaletteQuantizer : PaletteQuantizer
{
    public OklabPaletteQuantizer(string paletteName, float[,] palette) 
        : base(paletteName, palette) { }

    private static void RgBtoOklab(float red, float green, float blue, out float luminance, out float alpha,
        out float bb)
    {
        red = Linear(red);
        green = Linear(green);
        blue = Linear(blue);

        var l = 0.4122214708f * red + 0.5363325363f * green + 0.0514459929f * blue;
        var m = 0.2119034982f * red + 0.6806995451f * green + 0.1073969566f * blue;
        var s = 0.0883024619f * red + 0.2817188376f * green + 0.6299787005f * blue;

        l = (float)Math.Pow(l, 1.0 / 3);
        m = (float)Math.Pow(m, 1.0 / 3);
        s = (float)Math.Pow(s, 1.0 / 3);

        luminance = 0.2104542553f * l + 0.7936177850f * m - 0.0040720468f * s;
        alpha = 1.9779984951f * l - 2.4285922050f * m + 0.4505937099f * s;
        bb = 0.0259040371f * l + 0.7827717662f * m - 0.8086757660f * s;
        return;

        float Linear(float c)
        {
            return c <= 0.04045f ? c / 12.92f : (float)Math.Pow((c + 0.055f) / 1.055f, 2.4f);
        }
    }

    protected override int GetNearestColorIndex(float red, float green, float blue)
    {
        RgBtoOklab(red, green, blue, out var luminance, out var alpha, out var bb);

        var nearestIndex = 0;
        var minDistance = float.MaxValue;
        var paletteSize = Palette.GetLength(0);

        for (var j = 0; j < paletteSize; j++)
        {
            RgBtoOklab(Palette[j, 0], Palette[j, 1], Palette[j, 2], out var l2, out var a2, out var b2);
            var dL = luminance - l2;
            var da = alpha - a2;
            var db = bb - b2;
            var distance = dL * dL + da * da + db * db;

            if (distance < minDistance)
            {
                minDistance = distance;
                nearestIndex = j;
            }
        }

        return nearestIndex;
    }
    
    public override string ToString()
        => $"PaletteQuantizer.{PaletteName}.Oklab";
}