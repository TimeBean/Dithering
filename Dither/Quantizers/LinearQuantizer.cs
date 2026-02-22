using Dither.Exceptions;

namespace Dither.Quantizers;

public class LinearQuantizer : IQuantizer
{
    private int Levels { get; set; }
    
    public LinearQuantizer(int levels)
    {
        Levels = levels;
    }
    
    public float[] Quantize(float[] pixels)
    {
        if (Levels is <= 1 or >= 256)
            throw new WrongLevelQuantityException($"Level number must be between 2 and 255, inclusive: {Levels}");

        var newPixels = new List<float>();

        foreach (var pixel in pixels)
        {
            var levelsMinusOne = Levels - 1;
            var index = (int)Math.Round(pixel * (levelsMinusOne / 255.0));
            if (index < 0) index = 0;
            if (index > levelsMinusOne) index = levelsMinusOne;

            var value = index * (255.0 / levelsMinusOne);
            
            newPixels.Add((float)value);
        }
        
        return newPixels.ToArray();
    }
}