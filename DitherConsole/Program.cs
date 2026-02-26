using Dither.DitherStrategy;
using Dither.Quantizers;
using SkiaSharp;

namespace DitherConsole
{
    internal class Program
    {
        // BGR
        private static readonly float[,] Palette0 = new[,]
        {
            { 216f, 245f, 250f }, // #faf5d8
            { 139f, 174f, 216f }, // #d8ae8b
            { 27f, 24f, 33f },    // #21181b
            { 42f, 95f, 205f },   // #cd5f2a
            { 55f, 171f, 242f }   // #f2ab37
        };
            
        // BGR
        private static readonly float[,] Palette1 = new[,]
        {
            { 173f, 187f, 251f }, // #fbbbad
            { 150f, 122f, 74f },  // #4a7a96
            { 88f, 63f, 51f },    // #333f58
            { 49f, 40f, 41f }     // #292831
        };
            
        private static readonly float[,] Palette2 = new[,]
        {
            { 35f, 35f, 34f },    // #222323
            { 240f, 246f, 240f }  // #f0f6f0
        };
            
        private static readonly float[,] Palette3 = new[,]
        {
            { 255f, 255f, 255f }, // #ffffff
            { 242f, 230f, 12f },  // #0ce6f2
            { 219f, 152f, 0f },   // #0098db
            { 156f, 87f, 30f },   // #1e579c
            { 98f, 53f, 32f },    // #203562
            { 70f, 36f, 37f },    // #252446
            { 51f, 21f, 32f }     // #201533
        };
        
        static void Main()
        {
            using var input = File.OpenRead(@"Examples/Mirana.png");
            using var originalBitmap = SKBitmap.Decode(input);

            var width = originalBitmap.Width;
            var height = originalBitmap.Height;
            var rowBytes = originalBitmap.RowBytes;
            var bpp = originalBitmap.BytesPerPixel;
            
            if (!Directory.Exists("Out"))
            {
                Directory.CreateDirectory("Out");
            }
            else
            {
                Directory.Delete("Out", true);
                Directory.CreateDirectory("Out");
            }
            
            var processors = new (string Name, Func<IProcessor> Create)[]
            {
                ("FloydSteinberg", () => new FloydSteinbergProcessor(width, height, rowBytes, bpp)),
                ("Base", () => new BaseProcessor(width, height, rowBytes, bpp))
            };

            var quantizers = new (string Name, IQuantizer Quantizer)[]
            {
                ("Linear2", new LinearQuantizer(2)),
                ("Linear4", new LinearQuantizer(4)),
                ("Linear8", new LinearQuantizer(8)),
                ("OneBitGrayscale", new OneBitQuantizer(true)),
                ("OneBitColored", new OneBitQuantizer(false)),
                ("Palette0", new PaletteQuantizer(Palette0)),
                ("Palette1", new PaletteQuantizer(Palette1)),
                ("Palette2", new PaletteQuantizer(Palette2)),
                ("Palette3", new PaletteQuantizer(Palette3)),
            };

            foreach (var procDef in processors)
            {
                foreach (var quantDef in quantizers)
                {
                    using var currentBitmap = originalBitmap.Copy();
                    var pixelSpan = currentBitmap.GetPixelSpan();

                    var processor = procDef.Create();
                    var dithered = processor.Process(pixelSpan, quantDef.Quantizer);

                    dithered.CopyTo(pixelSpan);

                    var fileName = $"Out/{procDef.Name}_{quantDef.Name}.png";
                    SaveBitmap(currentBitmap, fileName);
            
                    Console.WriteLine($"Generated: {fileName}");
                }
            }
        }

        private static void SaveBitmap(SKBitmap bitmap, string path)
        {
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            using var output = File.OpenWrite(path);
            data.SaveTo(output);
        }
    }
}