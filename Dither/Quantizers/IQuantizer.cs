namespace Dither.Quantizers;

public interface IQuantizer
{
    public Span<byte> Quantize(Span<byte> pixels);
}