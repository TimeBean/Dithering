namespace Dither.Quantizers.Palette;

public sealed class Cie76PaletteQuantizer : PaletteQuantizer
{
    public Cie76PaletteQuantizer(string paletteName, float[,] palette)
        : base(paletteName, palette) { }

    protected override int GetNearestColorIndex(float red, float green, float blue)
    {
        ColorCaster.RgBtoLab(red, green, blue, out var l1, out var a1, out var b1);

        var nearestIndex = 0;
        var minDistance = float.MaxValue;
        var paletteSize = Palette.GetLength(0);

        for (var j = 0; j < paletteSize; j++)
        {
            ColorCaster.RgBtoLab(Palette[j, 0], Palette[j, 1], Palette[j, 2], out var luminance, out var a2, out var b2);
            var dL = l1 - luminance;
            var da = a1 - a2;
            var db = b1 - b2;
            var distance = dL * dL + da * da + db * db; // deltaE76²

            if (distance < minDistance)
            {
                minDistance = distance;
                nearestIndex = j;
            }
        }

        return nearestIndex;
    }
    
    public override string ToString()
        => $"PaletteQuantizer.{PaletteName}.Cie76";
}