using Dither.Quantizers;

namespace Dither.Processors.OrderedProcessors;

public abstract class OrderedProcessor : IProcessor
{
    protected abstract int[,] GetMatrix(int size);
    
    protected OrderedProcessor(int width, int height, int rowBytes, int bytesPerPixel, int size)
    {
        Width = width;
        Height = height;
        RowBytes = rowBytes;
        BytesPerPixel = bytesPerPixel;
        Size = size;
    }

    public int Width { get; }
    public int Height { get; }
    public int RowBytes { get; }
    public int BytesPerPixel { get; }
    private int Size { get; }
    private int[,] Matrix => GetMatrix(Size);
    private int MatrixCells => MatrixSize * MatrixSize;
    private int MatrixSize => Matrix.GetLength(0);

    public void Process(ref Span<byte> pixels, IQuantizer quantizer)
    {
        var rgb = new float[3];

        for (var y = 0; y < Height; y++)
        {
            var rowStart = y * RowBytes;
            var my = y % MatrixSize;

            for (var x = 0; x < Width; x++)
            {
                var px = rowStart + x * BytesPerPixel;
                var mx = x % MatrixSize;

                var m = Matrix[my, mx];

                var offset = (2 * m + 1) * 255 / (2 * MatrixCells) - 127;

                var r = pixels[px + 0] + offset;
                var g = pixels[px + 1] + offset;
                var b = pixels[px + 2] + offset;

                r = r switch
                {
                    < 0 => 0,
                    > 255 => 255,
                    _ => r
                };

                g = g switch
                {
                    < 0 => 0,
                    > 255 => 255,
                    _ => g
                };

                b = b switch
                {
                    < 0 => 0,
                    > 255 => 255,
                    _ => b
                };

                rgb[0] = r;
                rgb[1] = g;
                rgb[2] = b;

                var quantized = quantizer.Quantize(rgb);

                pixels[px + 0] = (byte)quantized[0];
                pixels[px + 1] = (byte)quantized[1];
                pixels[px + 2] = (byte)quantized[2];
            }
        }
    }
}