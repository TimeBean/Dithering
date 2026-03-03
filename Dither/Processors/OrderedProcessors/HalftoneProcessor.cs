namespace Dither.Processors.OrderedProcessors;

public class HalftoneProcessor : OrderedProcessor
{
    public HalftoneProcessor(int width, int height, int rowBytes, int bytesPerPixel, int size)
        : base(width, height, rowBytes, bytesPerPixel, size) { }

    protected override int[,] GetMatrix(int size)
    {
        var matrix = new int[size, size];
        double cx = (size - 1) / 2.0;
        double cy = (size - 1) / 2.0;

        var cells = new List<(int row, int col, double dist)>();

        for (var row = 0; row < size; row++)
        {
            for (var col = 0; col < size; col++)
            {
                double dx = col - cx;
                double dy = row - cy;
                double dist = Math.Sqrt(dx * dx + dy * dy);
                cells.Add((row, col, dist));
            }
        }

        var sorted = cells.OrderBy(c => c.dist)
            .ThenBy(c => c.row)
            .ThenBy(c => c.col);

        var value = 0;
        foreach (var (row, col, _) in sorted)
        {
            matrix[row, col] = value++;
        }

        return matrix;
    }
}