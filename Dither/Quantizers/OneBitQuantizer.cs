namespace Dither.Quantizers;

public class OneBitQuantizer : IQuantizer
{
    public OneBitQuantizer(bool isGrayscale)
    {
        IsGrayscale = isGrayscale;
    }

    private bool IsGrayscale { get; }

    public float[] Quantize(float[] pixels)
    {
        if (IsGrayscale)
        {
            var average = pixels.Average();

            for (var i = 0; i < pixels.Length; i++) pixels[i] = average < 128 ? 0 : 255;
        }
        else
        {
            for (var i = 0; i < pixels.Length; i++) pixels[i] = pixels[i] < 128 ? 0 : 255;
        }

        return pixels;
    }
}