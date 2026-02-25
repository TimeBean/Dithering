using System.Drawing;
using Dither.Quantizers;

namespace Dither.DitherStrategy;

public class FloydSteinbergProcessor : IProcessor
{
    public int Width { get; }
    public int Height { get; }
    public int RowBytes { get; }
    public int BytesPerPixel { get; }

    public FloydSteinbergProcessor(int width, int height, int rowBytes, int bytesPerPixel)
    {
        Width = width;
        Height = height;
        RowBytes = rowBytes;
        BytesPerPixel = bytesPerPixel;
    }

    public Span<byte> Process(Span<byte> pixels, IQuantizer quantizer)
    {
        for (var y = 0; y < Height; y++)
        for (var x = 0; x < Width; x++)
        {
            var baseIndex = y * RowBytes + x * BytesPerPixel;

            var newValues = new List<float>();

            for (var c = 0; c < 3; c++)
            {
                newValues.Add(pixels[baseIndex + c]);
            }

            var quantizedColors = quantizer.Quantize(newValues.ToArray());
            
            for (var c = 0; c < 3; c++)
            {
                var index = baseIndex + c;

                var oldValue = pixels[index];
                var newValue = quantizedColors[c];

                var error = oldValue - newValue;
                pixels[index] = (byte)Math.Round(newValue);

                DistributeError(pixels, x, y, c, error);
            }
        }

        return pixels;
    }

    private void DistributeError(Span<byte> pixels, int x, int y, int channel, double error)
    {
        Add(x + 1, y, 7.0 / 16.0, pixels);
        Add(x - 1, y + 1, 3.0 / 16.0, pixels);
        Add(x, y + 1, 5.0 / 16.0, pixels);
        Add(x + 1, y + 1, 1.0 / 16.0, pixels);
        return;

        void Add(int nx, int ny, double factor, Span<byte> pixelsAdd)
        {
            if (nx < 0 || nx >= Width || ny < 0 || ny >= Height)
                return;

            var idx = ny * RowBytes + nx * BytesPerPixel + channel;

            var value = pixelsAdd[idx] + error * factor;

            if (value > 255)
                value = 255;
            if (value < 0)
                value = 0;

            pixelsAdd[idx] = (byte)Math.Round(value);
        }
    }
}