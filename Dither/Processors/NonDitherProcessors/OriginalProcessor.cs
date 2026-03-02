using Dither.Quantizers;

namespace Dither.Processors.NonDitherProcessors;

public class OriginalProcessor : IProcessor
{
    public OriginalProcessor(int width, int height, int rowBytes, int bytesPerPixel)
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
        => pixels;
}