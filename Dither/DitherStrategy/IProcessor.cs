using Dither.Quantizers;

namespace Dither.DitherStrategy;

public interface IProcessor
{
    public int Width { get; }
    public int Height { get; }
    public int RowBytes { get; }
    public int BytesPerPixel { get; }
    
    public Span<byte> Process(Span<byte> pixels, IQuantizer quantizer);
}