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
                ("Base", () => new BaseProcessor(width, height, rowBytes, bpp)),
                ("FloydSteinberg", () => new FloydSteinbergProcessor(width, height, rowBytes, bpp)),
                ("Bayer 2x2", () => new BayerProcessor(width, height, rowBytes, bpp, Constants.Bayer2)),
                ("Bayer 4x4", () => new BayerProcessor(width, height, rowBytes, bpp, Constants.Bayer4)),
                ("Bayer 8x8", () => new BayerProcessor(width, height, rowBytes, bpp, Constants.Bayer8)),
            };

            var quantizers = new (string Name, IQuantizer Quantizer)[]
            {
                ("Linear2", new LinearQuantizer(2)),
                ("Linear4", new LinearQuantizer(4)),
                ("Linear8", new LinearQuantizer(8)),
                ("OneBitGrayscale", new OneBitQuantizer(true)),
                ("OneBitColored", new OneBitQuantizer(false)),
                ("Palette0", new PaletteQuantizer(Constants.Palette0)),
                ("Palette1", new PaletteQuantizer(Constants.Palette1)),
                ("Palette2", new PaletteQuantizer(Constants.Palette2)),
                ("Palette3", new PaletteQuantizer(Constants.Palette3)),
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