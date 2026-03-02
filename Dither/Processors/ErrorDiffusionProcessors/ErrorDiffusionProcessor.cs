using Dither.Quantizers;

namespace Dither.Processors.ErrorDiffusionProcessors;

public abstract class ErrorDiffusionProcessor : IProcessor
{
    protected abstract void DistributeError(Span<byte> pixels, int x, int y, int channel, double error);
    
    protected ErrorDiffusionProcessor(int width, int height, int rowBytes, int bytesPerPixel)
    {
        Width = width;
        Height = height;
        RowBytes = rowBytes;
        BytesPerPixel = bytesPerPixel;
    }

    public int Width { get; protected init; }
    public int Height { get; protected init; }
    public int RowBytes { get; protected init; }
    public int BytesPerPixel { get; protected init; }

    public Span<byte> Process(Span<byte> pixels, IQuantizer quantizer)
    {
        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                var baseIndex = y * RowBytes + x * BytesPerPixel;

                var newValues = new List<float>();

                for (var c = 0; c < 3; c++) newValues.Add(pixels[baseIndex + c]);

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
        }

        return pixels;
    }
}