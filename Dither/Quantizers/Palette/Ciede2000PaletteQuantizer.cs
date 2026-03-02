namespace Dither.Quantizers.Palette;

public sealed class Ciede2000PaletteQuantizer : PaletteQuantizer
{
    public Ciede2000PaletteQuantizer(float[,] palette) : base(palette)
    {
    }

    private static void RgBtoLab(float red, float green, float blue, out float luminance, out float alpha, out float bb)
    {
        red = Linear(red);
        green = Linear(green);
        blue = Linear(blue);

        var x = red * 0.4124f + green * 0.3576f + blue * 0.1805f;
        var y = red * 0.2126f + green * 0.7152f + blue * 0.0722f;
        var z = red * 0.0193f + green * 0.1192f + blue * 0.9505f;

        const float xn = 0.95047f;
        const float yn = 1f;
        const float zn = 1.08883f;

        var fx = F(x / xn);
        var fy = F(y / yn);
        var fz = F(z / zn);

        luminance = 116 * fy - 16;
        alpha = 500 * (fx - fy);
        bb = 200 * (fy - fz);
        return;

        float F(float t)
        {
            return t > 0.008856f ? (float)Math.Pow(t, 1.0 / 3) : (903.3f * t + 16f) / 116f;
        }

        float Linear(float c)
        {
            return c <= 0.04045f ? c / 12.92f : (float)Math.Pow((c + 0.055f) / 1.055f, 2.4f);
        }
    }

    private static float DeltaE2000(float luminance1, float alpha1, float blue, float luminance2, float alpha2,
        float blue2)
    {
        var dL = luminance2 - luminance1;
        var da = alpha2 - alpha1;
        var db = blue2 - blue;

        return (float)Math.Sqrt(dL * dL + da * da + db * db);
    }

    protected override int GetNearestColorIndex(float red, float green, float blue)
    {
        RgBtoLab(red, green, blue, out var l1, out var a1, out var b1);

        var nearestIndex = 0;
        var minDistance = float.MaxValue;
        var paletteSize = Palette.GetLength(0);

        for (var j = 0; j < paletteSize; j++)
        {
            RgBtoLab(Palette[j, 0], Palette[j, 1], Palette[j, 2], out var l2, out var a2, out var b2);
            var distance = DeltaE2000(l1, a1, b1, l2, a2, b2);

            if (distance < minDistance)
            {
                minDistance = distance;
                nearestIndex = j;
            }
        }

        return nearestIndex;
    }
}