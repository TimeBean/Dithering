namespace Dither;

public static class SkImageDitherExtension
{
    extension(Span<byte> pixels)
    {
        public void Dither()
        {
            //BGRA8888
            for (var i = 0; i < pixels.Length; i += 4)
            {
                pixels[i + 0] = pixels[i + 0] > (255 / 2) ? (byte)255 : (byte)0;
                pixels[i + 1] = pixels[i + 1] > (255 / 2) ? (byte)255 : (byte)0;
                pixels[i + 2] = pixels[i + 2] > (255 / 2) ? (byte)255 : (byte)0;
            }
        }
    }
}