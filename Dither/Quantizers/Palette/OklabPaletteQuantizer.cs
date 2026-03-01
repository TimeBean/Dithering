namespace Dither.Quantizers.Palette;

public class OklabPaletteQuantizer : PaletteQuantizer
{
    public OklabPaletteQuantizer(float[,] palette) : base(palette) { }

    private static void RgBtoOklab(float r, float g, float b, out float L, out float a, out float bb)
    {
        r = Linear(r); g = Linear(g); b = Linear(b);

        var l = 0.4122214708f*r + 0.5363325363f*g + 0.0514459929f*b;
        var m = 0.2119034982f*r + 0.6806995451f*g + 0.1073969566f*b;
        var s = 0.0883024619f*r + 0.2817188376f*g + 0.6299787005f*b;

        l = (float)Math.Pow(l, 1.0/3);
        m = (float)Math.Pow(m, 1.0/3);
        s = (float)Math.Pow(s, 1.0/3);

        L  = 0.2104542553f*l + 0.7936177850f*m - 0.0040720468f*s;
        a  = 1.9779984951f*l - 2.4285922050f*m + 0.4505937099f*s;
        bb = 0.0259040371f*l + 0.7827717662f*m - 0.8086757660f*s;
        return;

        float Linear(float c) => c <= 0.04045f ? c / 12.92f : (float)Math.Pow((c + 0.055f) / 1.055f, 2.4f);
    }

    protected override int GetNearestColorIndex(float r, float g, float b)
    {
        RgBtoOklab(r, g, b, out var L1, out var a1, out var b1);

        var nearestIndex = 0;
        var minDistance = float.MaxValue;
        var paletteSize = Palette.GetLength(0);

        for (var j = 0; j < paletteSize; j++)
        {
            RgBtoOklab(Palette[j,0], Palette[j,1], Palette[j,2], out var l2, out var a2, out var b2);
            var dL = L1 - l2;
            var da = a1 - a2;
            var db = b1 - b2;
            var distance = dL*dL + da*da + db*db;
            
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestIndex = j;
            }
        }

        return nearestIndex;
    }
}