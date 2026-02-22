namespace Dither.Quantizers;

public class OneBitQuantizer : IQuantizer
{
    public float[] Quantize(float[] pixels)
    {
        for (var i = 0; i < pixels.Length; i++)
        {
            pixels[i] = pixels[i] < 128 ? 0 : 255;
        }
        
        return pixels;
    }
}