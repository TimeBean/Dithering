namespace Dither.Quantizers;

public class OneBitQuantizer : IQuantizer
{
    private bool IsGrayscale { get; set; }

    public OneBitQuantizer(bool isGrayscale)
    {
        IsGrayscale = isGrayscale;
    }

    public Span<byte> Quantize(Span<byte> pixels)
    {
        if (IsGrayscale)
        {
            for (var i = 0; i < pixels.Length; i += 4)
            {
                var average = pixels[i + 0] + pixels[i + 1] + pixels[i + 2] / 3 < 128 ? (byte)0 : (byte)255;

                pixels[i + 0] = average;
                pixels[i + 1] = average;
                pixels[i + 2] = average;
            }
        }
        else
        {
            for (var i = 0; i < pixels.Length; i += 4)
            {
                pixels[i + 0] = (byte)(pixels[i + 0] < 128 ? 0 : 255);
                pixels[i + 1] = (byte)(pixels[i + 1] < 128 ? 0 : 255);
                pixels[i + 2] = (byte)(pixels[i + 2] < 128 ? 0 : 255);
            }
        }

        return pixels;
    }
}