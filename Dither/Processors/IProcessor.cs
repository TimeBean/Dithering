using Dither.Quantizers;

namespace Dither.Processors;

public interface IProcessor
{
    int Width { get; }
    int Height { get; }
    int RowBytes { get; }
    int BytesPerPixel { get; }

    void Process(ref Span<byte> pixels, IQuantizer quantizer);
}