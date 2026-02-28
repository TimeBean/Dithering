using Dither.Quantizers;

namespace Dither.Processors;

public class BayerProcessor : IProcessor
{
    public int Width { get; }
    public int Height { get; }
    public int RowBytes { get; }
    public int BytesPerPixel { get; }

    private readonly int[,] _matrix;
    private readonly int _matrixSize;
    private readonly int _matrixCells;

    public BayerProcessor(int width, int height, int rowBytes, int bytesPerPixel, int[,] matrix)
    {
        Width = width;
        Height = height;
        RowBytes = rowBytes;
        BytesPerPixel = bytesPerPixel;

        _matrix = matrix;
        _matrixSize = matrix.GetLength(0);
        _matrixCells = _matrixSize * _matrixSize;
    }

    public Span<byte> Process(Span<byte> pixels, IQuantizer quantizer)
    {
        var rgb = new float[3];

        for (var y = 0; y < Height; y++)
        {
            var rowStart = y * RowBytes;
            var my = y % _matrixSize;

            for (var x = 0; x < Width; x++)
            {
                var px = rowStart + x * BytesPerPixel;
                var mx = x % _matrixSize;

                var m = _matrix[my, mx];

                var offset = ((2 * m + 1) * 255) / (2 * _matrixCells) - 127;

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

        return pixels;
    }
}