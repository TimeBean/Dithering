namespace Dither.Quantizers.Palette;

public abstract class PaletteQuantizer : IQuantizer
{
    protected abstract int GetNearestColorIndex(float red, float green, float blue);
    
    protected PaletteQuantizer(float[,] palette)
    {
        Palette = palette;
    }

    public float[,] Palette { get; }

    public float[] Quantize(float[] pixels)
    {
        const int colorCount = 3;
        var result = new float[pixels.Length];

        for (var i = 0; i < pixels.Length; i += colorCount)
        {
            var nearestIndex = GetNearestColorIndex(
                pixels[i],
                pixels[i + 1],
                pixels[i + 2]);

            result[i] = Palette[nearestIndex, 0];
            result[i + 1] = Palette[nearestIndex, 1];
            result[i + 2] = Palette[nearestIndex, 2];
        }

        return result;
    }
}