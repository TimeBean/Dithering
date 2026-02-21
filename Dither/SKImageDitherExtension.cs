using Dither.Quantizers;

namespace Dither;

public static class SkImageDitherExtension
{
    extension(Span<byte> pixels)
    {
        public void Dither(IQuantizer quantizer)
        {
            //BGRA8888
            quantizer.Quantize(pixels);
        }
    }
}