using System.Diagnostics;
using Dither.DitherStrategy;
using Dither.Processors;
using Dither.Quantizers;
using SkiaSharp;

namespace DitherConsole
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class Program
    {
        private static readonly float[,] Palette0 = new[,]
        {
            { 216f, 245f, 250f }, // #faf5d8
            { 139f, 174f, 216f }, // #d8ae8b
            { 27f, 24f, 33f }, // #21181b
            { 42f, 95f, 205f }, // #cd5f2a
            { 55f, 171f, 242f } // #f2ab37
        };

        private static readonly float[,] Palette1 = new[,]
        {
            { 173f, 187f, 251f }, // #fbbbad
            { 150f, 122f, 74f }, // #4a7a96
            { 88f, 63f, 51f }, // #333f58
            { 49f, 40f, 41f } // #292831
        };

        private static readonly float[,] Palette2 = new[,]
        {
            { 35f, 35f, 34f }, // #222323
            { 240f, 246f, 240f } // #f0f6f0
        };

        private static readonly float[,] Palette3 = new[,]
        {
            { 255f, 255f, 255f }, // #ffffff
            { 242f, 230f, 12f }, // #0ce6f2
            { 219f, 152f, 0f }, // #0098db
            { 156f, 87f, 30f }, // #1e579c
            { 98f, 53f, 32f }, // #203562
            { 70f, 36f, 37f }, // #252446
            { 51f, 21f, 32f } // #201533
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

            int[,] bayer2 =
            {
                { 0, 2 },
                { 3, 1 }
            };
            
            int[,] bayer4 =
            {
                {  0,  8,  2, 10 },
                { 12,  4, 14,  6 },
                {  3, 11,  1,  9 },
                { 15,  7, 13,  5 }
            };

            int[,] bayer8 =
            {
                {  0,32, 8,40, 2,34,10,42 },
                { 48,16,56,24,50,18,58,26 },
                { 12,44, 4,36,14,46, 6,38 },
                { 60,28,52,20,62,30,54,22 },
                {  3,35,11,43, 1,33, 9,41 },
                { 51,19,59,27,49,17,57,25 },
                { 15,47, 7,39,13,45, 5,37 },
                { 63,31,55,23,61,29,53,21 }
            };
            
            var processors = new (string Name, Func<IProcessor> Create)[]
            {
                ("Base", () => new BaseProcessor(width, height, rowBytes, bpp)),
                ("FloydSteinberg", () => new FloydSteinbergProcessor(width, height, rowBytes, bpp)),
                ("Bayer 2x2", () => new BayerProcessor(width, height, rowBytes, bpp, bayer2)),
                ("Bayer 4x4", () => new BayerProcessor(width, height, rowBytes, bpp, bayer4)),
                ("Bayer 8x8", () => new BayerProcessor(width, height, rowBytes, bpp, bayer8)),
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

            using var compilationBitmap = new SKBitmap(width * quantizers.Length, height * processors.Length);
            using var canvas = new SKCanvas(compilationBitmap);
            canvas.Clear(SKColors.Black);

            var stopWatch = new Stopwatch();
            
            for (var pIndex = 0; pIndex < processors.Length; pIndex++)
            {
                stopWatch.Start();
                
                var procDef = processors[pIndex];
                var yPosition = pIndex * height; 

                for (var qIndex = 0; qIndex < quantizers.Length; qIndex++)
                {
                    var quantDef = quantizers[qIndex];
                    var xPosition = qIndex * width;

                    using var currentBitmap = originalBitmap.Copy();
                    var pixelSpan = currentBitmap.GetPixelSpan();

                    var processor = procDef.Create();
                    var dithered = processor.Process(pixelSpan, quantDef.Quantizer);

                    dithered.CopyTo(pixelSpan);

                    stopWatch.Stop();
                    canvas.DrawBitmap(currentBitmap, xPosition, yPosition);
                    stopWatch.Start();
                    
                    var fileName = $"Out/{procDef.Name}_{quantDef.Name}.png";
                    SaveBitmap(currentBitmap, fileName);
                    
                    stopWatch.Stop();
                    
                    Console.WriteLine($"Generated: {fileName} ({stopWatch.ElapsedMilliseconds} ms)");
                    stopWatch.Reset();
                }
            }

            const string compilationFileName = "Out/!CompilationGrid.png";
            SaveBitmap(compilationBitmap, compilationFileName);
            Console.WriteLine($"\nCompilation generated: {compilationFileName}");
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