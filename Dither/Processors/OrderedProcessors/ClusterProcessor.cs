namespace Dither.Processors.OrderedProcessors;

public class ClusterProcessor : OrderedProcessor
{
    public ClusterProcessor(int width, int height, int rowBytes, int bytesPerPixel, int size) 
        : base(width, height, rowBytes, bytesPerPixel, size) { }

    protected override int[,] GetMatrix(int size)
    {
        var baseKernel = new int[4, 4]
        {
            { 12,  5,  6, 13 },
            {  4,  0,  1,  7 },
            { 11,  3,  2,  8 },
            { 15, 10,  9, 14 }
        };

        const int baseSize = 4;
        var matrix = new int[size, size];

        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var tileX = x % baseSize;
                var tileY = y % baseSize;

                var baseValue = baseKernel[tileY, tileX];
                
                var totalCells = size * size;
                matrix[y, x] = (int)Math.Round((double)baseValue / (baseSize * baseSize - 1) * (totalCells - 1));
            }
        }

        return matrix;
    }
}