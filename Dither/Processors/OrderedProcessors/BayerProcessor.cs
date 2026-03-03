using System.Diagnostics;

namespace Dither.Processors.OrderedProcessors;

public class BayerProcessor : OrderedProcessor
{
    public BayerProcessor(int width, int height, int rowBytes, int bytesPerPixel, int size) 
        : base(width, height, rowBytes, bytesPerPixel, size) { }

    protected override int[,] GetMatrix(int size)
    {
        if (size < 2 || (size & (size - 1)) != 0)
        {
            throw new ArgumentException("Size must be a power of 2 and at least 2", nameof(size));
        }

        if (size == 2)
        {
            return new int[,]
            {
                { 0, 2 },
                { 3, 1 }
            };
        }

        var halfSize = size / 2;
        var smaller = GetMatrix(halfSize);
        var matrix = new int[size, size];

        for (var y = 0; y < halfSize; y++)
        {
            for (var x = 0; x < halfSize; x++)
            {
                var val = smaller[y, x] * 4;

                matrix[y, x] = val;
                matrix[y, x + halfSize] = val + 2;
                matrix[y + halfSize, x] = val + 3;
                matrix[y + halfSize, x + halfSize] = val + 1;
            }
        }

        return matrix;
    }
}