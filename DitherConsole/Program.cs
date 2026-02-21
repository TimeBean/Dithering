using Dither;
using Dither.Quantizers;
using SkiaSharp;

namespace DitherConsole
{
    internal class Program
    {
        static void Main()
        {
            using var input = File.OpenRead(@"Examples/Mirana.png");
            using var bitmap = SKBitmap.Decode(input);

            var pixelSpan = bitmap.GetPixelSpan();
            pixelSpan.Dither(new LinearQuantizer(4));
            //pixelSpan.Dither(new OneBitQuantizer(false));
            
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);

            using var output = File.OpenWrite("out.png");
            data.SaveTo(output);
        }
    }
}