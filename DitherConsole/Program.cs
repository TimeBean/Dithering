using Dither.DitherStrategy;
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

            var width = bitmap.Width;
            var height = bitmap.Height;

            var pixelSpan = bitmap.GetPixelSpan();

            var dither = new FloydSteinbergProcessor(width, height, bitmap.RowBytes, bitmap.BytesPerPixel);
            //var dither = new BaseProcessor(width, height, bitmap.RowBytes, bitmap.BytesPerPixel);

            var palette = new[,]
            {
                { 216f, 245f, 250f }, // #faf5d8 → BGR
                { 139f, 174f, 216f }, // #d8ae8b → BGR
                { 27f, 24f, 33f },    // #21181b → BGR
                { 42f, 95f, 205f },   // #cd5f2a → BGR
                { 55f, 171f, 242f }   // #f2ab37 → BGR
            };

            //var dithered = dither.Process(pixelSpan, new LinearQuantizer(16));
            //var dithered = dither.Process(pixelSpan, new OneBitQuantizer());
            var dithered = dither.Process(pixelSpan, new PaletteQuantizer(palette));

            dithered.CopyTo(pixelSpan);

            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);

            using var output = File.OpenWrite("out.png");
            data.SaveTo(output);
        }
    }
}