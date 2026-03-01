namespace Dither.Quantizers.Palette;

public class Cie76PaletteQuantizer : PaletteQuantizer
{
    public Cie76PaletteQuantizer(float[,] palette) : base(palette) { }

    private static void RgBtoLab(float r, float g, float b, out float L, out float a, out float bb)
    {
        r = Linear(r); g = Linear(g); b = Linear(b);

        var x = r * 0.4124f + g * 0.3576f + b * 0.1805f;
        var y = r * 0.2126f + g * 0.7152f + b * 0.0722f;
        var z = r * 0.0193f + g * 0.1192f + b * 0.9505f;

        const float xn = 0.95047f;
        const float yn = 1f;
        const float zn = 1.08883f;

        var fx = F(x/xn);
        var fy = F(y/yn);
        var fz = F(z/zn);

        L = 116f * fy - 16f;
        a = 500f * (fx - fy);
        bb = 200f * (fy - fz);
        return;

        float F(float t) => t > 0.008856f ? (float)Math.Pow(t, 1.0/3) : (903.3f * t + 16f)/116f;

        float Linear(float c) => c <= 0.04045f ? c / 12.92f : (float)Math.Pow((c + 0.055f)/1.055f, 2.4f);
    }

    protected override int GetNearestColorIndex(float r, float g, float b)
    {
        RgBtoLab(r, g, b, out var l1, out var a1, out var b1);

        var nearestIndex = 0;
        var minDistance = float.MaxValue;
        var paletteSize = Palette.GetLength(0);

        for (var j = 0; j < paletteSize; j++)
        {
            RgBtoLab(Palette[j,0], Palette[j,1], Palette[j,2], out var L2, out var a2, out var b2);
            var dL = l1 - L2;
            var da = a1 - a2;
            var db = b1 - b2;
            var distance = dL*dL + da*da + db*db; // deltaE76²
            
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestIndex = j;
            }
        }

        return nearestIndex;
    }
}