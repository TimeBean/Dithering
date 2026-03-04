namespace Dither.Quantizers.Palette;

public sealed class Ciede2000PaletteQuantizer : PaletteQuantizer
{
    public Ciede2000PaletteQuantizer(string paletteName, float[,] palette) 
        : base(paletteName, palette) { }

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
        ColorCaster.RgBtoLab(red, green, blue, out var l1, out var a1, out var b1);

        var nearestIndex = 0;
        var minDistance = float.MaxValue;
        var paletteSize = Palette.GetLength(0);

        for (var j = 0; j < paletteSize; j++)
        {
            ColorCaster.RgBtoLab(Palette[j, 0], Palette[j, 1], Palette[j, 2], out var l2, out var a2, out var b2);
            var distance = DeltaE2000(l1, a1, b1, l2, a2, b2);

            if (distance < minDistance)
            {
                minDistance = distance;
                nearestIndex = j;
            }
        }

        return nearestIndex;
    }
    
    public override string ToString()
        => $"PaletteQuantizer.{PaletteName}.Ciede2000";
}