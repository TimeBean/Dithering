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
        private static readonly SKPaint TextPaint = new()
        {
            Color = SKColors.GhostWhite,
            IsAntialias = true
        };

        private static readonly SKPaint ShadowPaint = new()
        {
            Color = SKColors.Black,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 4f,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 2f)
        };

        private static readonly SKFont Font = new(SKTypeface.Default, 14f);

        private static void Main()
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
                ("Floyd-Steinberg", () => new FloydSteinbergProcessor(width, height, rowBytes, bpp)),
                ("Bayer_2x2", () => new BayerProcessor(width, height, rowBytes, bpp, Constants.Bayer2)),
                ("Bayer_4x4", () => new BayerProcessor(width, height, rowBytes, bpp, Constants.Bayer4)),
                ("Bayer_8x8", () => new BayerProcessor(width, height, rowBytes, bpp, Constants.Bayer8)),
            };

            var palettes = new PaletteCaster().ParsePalettes();

            var quantizers = new (string Name, IQuantizer Quantizer)[]
            {
                (Name: "Linear_2x", Quantizer: new LinearQuantizer(2)),
                (Name: "Linear_4x", Quantizer: new LinearQuantizer(4)),
                (Name: "Linear_8x", Quantizer: new LinearQuantizer(8)),
                (Name: "Grayscale_One_Bit", Quantizer: new OneBitQuantizer(true)),
                (Name: "Colored_One_Bit", Quantizer: new OneBitQuantizer(false)),
            };

            quantizers = quantizers
                .Concat(palettes.Select(p => (Name: $"{p.Name} ({p.ColorCount})", Quantizer: (IQuantizer)new PaletteQuantizer(p.Data))))
                .ToArray();

            using var compilationBitmap = new SKBitmap(width * quantizers.Length, height * processors.Length);
            using var canvas = new SKCanvas(compilationBitmap);
            canvas.Clear(SKColors.Black);

            var stopWatch = new Stopwatch();

            var fontSize = Convert.ToInt32(Font.Size);

            for (var pIndex = 0; pIndex < processors.Length; pIndex++)
            {
                stopWatch.Start();

                var processorDef = processors[pIndex];
                var yPosition = pIndex * height;

                for (var qIndex = 0; qIndex < quantizers.Length; qIndex++)
                {
                    var quantizerDef = quantizers[qIndex];
                    var xPosition = qIndex * width;

                    using var currentBitmap = originalBitmap.Copy();
                    var pixelSpan = currentBitmap.GetPixelSpan();

                    var processor = processorDef.Create();
                    var dithered = processor.Process(pixelSpan, quantizerDef.Quantizer);

                    dithered.CopyTo(pixelSpan);

                    stopWatch.Stop();
                    canvas.DrawBitmap(currentBitmap, xPosition, yPosition);

                    var processorText = processorDef.Name.Replace("_", " ");
                    var quantizerText = quantizerDef.Name.Replace("_", " ");

                    const int topOffset = 2;
                    const int leftOffset = 4;

                    DrawText(canvas, $"p: {processorText}", xPosition + leftOffset, yPosition + topOffset + fontSize);
                    DrawText(canvas, $"q: {quantizerText}", xPosition + leftOffset,
                        yPosition + topOffset + fontSize + 4 + fontSize);
                    DrawText(canvas, $"c: {dithered.GetColorCount()}", xPosition + leftOffset,
                        yPosition + topOffset + fontSize + 4 + fontSize+ 4 + fontSize);
                    
                    stopWatch.Start();

                    var fileName = $"Out/{processorDef.Name}_{quantizerDef.Name}.png";
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

        private static void DrawText(SKCanvas canvas, string text, int xPosition, int yPosition)
        {
            canvas.DrawText(text, xPosition + 2, yPosition + 1, Font, ShadowPaint);
            canvas.DrawText(text, xPosition, yPosition, Font, TextPaint);
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