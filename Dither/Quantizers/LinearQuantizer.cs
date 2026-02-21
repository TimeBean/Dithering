using Dither.Exceptions;

namespace Dither.Quantizers;

public class LinearQuantizer : IQuantizer
{
    private int Levels { get; set; }
    
    public LinearQuantizer(int levels)
    {
        Levels = levels;
    }
    
    public Span<byte> Quantize(Span<byte> pixels)
    {
        for (var i = 0; i < pixels.Length; i += 4)
        {
            pixels[i + 0] = QuantizeChannel(pixels[i + 0]);
            pixels[i + 1] = QuantizeChannel(pixels[i + 1]);
            pixels[i + 2] = QuantizeChannel(pixels[i + 2]);
        }
        
        return pixels;
    }

    private byte QuantizeChannel(byte pixelChannel)
    {
        if (Levels is <= 1 or >= 256)
            throw new WrongLevelQuantityException($"Level number must be between 1 and 255, inclusive: {Levels}");
        
        var step = (byte)(255 / (Levels - 1));
        
        return (byte)(Math.Round((double)pixelChannel / step) * step);
    }
}