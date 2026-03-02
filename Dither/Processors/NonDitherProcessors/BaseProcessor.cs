using Dither.Quantizers;

namespace Dither.Processors.NonDitherProcessors;

public class BaseProcessor : IProcessor
{
    public BaseProcessor(int width, int height, int rowBytes, int bytesPerPixel)
    {
        Width = width;
        Height = height;
        RowBytes = rowBytes;
        BytesPerPixel = bytesPerPixel;
    }

    public int Width { get; }
    public int Height { get; }
    public int RowBytes { get; }
    public int BytesPerPixel { get; }

    public Span<byte> Process(Span<byte> pixels, IQuantizer quantizer)
    {
        for (var y = 0; y < Height; y++)
        {
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
                    var newValue = quantizedColors[c];

                    pixels[index] = (byte)Math.Round(newValue);
                }
            }
        }
        
        return pixels;
    }
}