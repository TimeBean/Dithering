using System.Diagnostics;
using Dither.Processors;
using Dither.Processors.ErrorDiffusionProcessors;
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

        private const bool IsDebug = true;

        private static void Main()
        {
            using var input = File.OpenRead(@"Examples/Mirana.png");
            //using var input = File.OpenRead(@"Examples/Vanko.jpg");

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
                ("Atkinson", () => new AtkinsonProcessor(width, height, rowBytes, bpp)),
                ("Burkes", () => new BurkesProcessor(width, height, rowBytes, bpp)),
                ("Floyd-Steinberg", () => new FloydSteinbergProcessor(width, height, rowBytes, bpp)),
                ("Jarvis-Judice-Ninke", () => new JarvisJudiceNinkeProcessor(width, height, rowBytes, bpp)),
                ("Sierra_3_Row", () => new Sierra3Processor(width, height, rowBytes, bpp)),
                ("Stucki", () => new StuckiProcessor(width, height, rowBytes, bpp)),
                ("Bayer_2x2", () => new BayerProcessor(width, height, rowBytes, bpp, Constants.Bayer2)),
                ("Bayer_4x4", () => new BayerProcessor(width, height, rowBytes, bpp, Constants.Bayer4)),
                ("Bayer_8x8", () => new BayerProcessor(width, height, rowBytes, bpp, Constants.Bayer8)),
            };

            var palettes = new PaletteCaster().ParsePalettes();

            var quantizers = new (string Name, IQuantizer Quantizer)[]
            {
                (Name: "Linear_2x", Quantizer: new LinearQuantizer(2)),
                (Name: "Linear_8x", Quantizer: new LinearQuantizer(8))
            };

            quantizers = quantizers
                .Concat(palettes.Select(p =>
                    (Name: $"Palette {p.Name} ({p.ColorCount})", Quantizer: (IQuantizer)new PaletteQuantizer(p.Data))))
                .ToArray();

            using var compilationBitmap = new SKBitmap(width * quantizers.Length, height * processors.Length);
            using var canvas = new SKCanvas(compilationBitmap);
            canvas.Clear(SKColors.Black);

            var fontSize = Convert.ToInt32(Font.Size);

            var millis = 0L;
            var imagesCount = 0;

            for (var pIndex = 0; pIndex < processors.Length; pIndex++)
            {
                var perAlgorithmStopwatch = new Stopwatch();
                perAlgorithmStopwatch.Start();

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

                    perAlgorithmStopwatch.Stop();

                    canvas.DrawBitmap(currentBitmap, xPosition, yPosition);

                    var processorText = processorDef.Name.Replace("_", " ");
                    var quantizerText = quantizerDef.Name.Replace("_", " ");

                    const int topOffset = 2;
                    const int leftOffset = 4;

                    if (IsDebug)
                    {
                        DrawText(canvas, $"processor: {processorText}", xPosition + leftOffset,
                            yPosition + topOffset + fontSize);
                        DrawText(canvas, $"quantizer: {quantizerText}", xPosition + leftOffset,
                            yPosition + topOffset + fontSize + 4 + fontSize);
                        DrawText(canvas, $"output colors: {dithered.GetColorCount()}", xPosition + leftOffset,
                            yPosition + topOffset + fontSize + 4 + fontSize + 4 + fontSize);

                        /*if (quantizers[qIndex].Quantizer.GetType() == typeof(PaletteQuantizer))
                        {
                            DrawPalette(canvas, ((PaletteQuantizer)quantizers[qIndex].Quantizer).Palette, xPosition + leftOffset,
                                yPosition + topOffset + fontSize + 4 + fontSize + 4 + fontSize + fontSize);
                        }
                        
                        DrawPalette(canvas, dithered.GetUniqueColors(), xPosition + leftOffset,
                            yPosition + topOffset + fontSize + 4 + fontSize + 4 + fontSize + fontSize + 12);*/
                    }

                    perAlgorithmStopwatch.Start();

                    var fileName = $"Out/{processorDef.Name}_{quantizerDef.Name}.png";
                    SaveBitmap(currentBitmap, fileName);

                    perAlgorithmStopwatch.Stop();

                    Console.WriteLine($"Generated: {fileName} ({perAlgorithmStopwatch.ElapsedMilliseconds} ms)");
                    millis += perAlgorithmStopwatch.ElapsedMilliseconds;
                    perAlgorithmStopwatch.Reset();
                    imagesCount++;
                }
            }

            const string compilationFileName = "Out/!CompilationGrid.png";
            SaveBitmap(compilationBitmap, compilationFileName);
            Console.WriteLine(
                $"\nCompilation generated: {compilationFileName} ({millis} ms for {imagesCount} images i.e. {Math.Round(millis / (float)imagesCount, 2)} ms for image)");
        }
        
        private static void DrawText(SKCanvas canvas, string text, int xPosition, int yPosition)
        {
            canvas.DrawText(text, xPosition + 2, yPosition + 1, Font, ShadowPaint);
            canvas.DrawText(text, xPosition, yPosition, Font, TextPaint);
        }
        
        private static void DrawPalette(SKCanvas canvas, float[,] palette, int xPosition, int yPosition)
        {
            var colorCount = palette.GetColorCount();
            
            if (colorCount <= 0) 
                return;

            var colors = palette.ToSkColors();

            const int cellWidth = 8;
            const int cellHeight = 8;
            const int border = 1; 

            var totalWidth = border * 2 + colorCount * cellWidth;
            const int totalHeight = border * 2 + cellHeight;

            using var bgPaint = new SKPaint();
            bgPaint.Style = SKPaintStyle.Fill;
            bgPaint.Color = SKColors.Purple;
            
            canvas.DrawRect(xPosition, yPosition, totalWidth, totalHeight, bgPaint);

            using var fillPaint = new SKPaint();
            fillPaint.Style = SKPaintStyle.Fill;
            fillPaint.IsAntialias = true;
            
            for (var i = 0; i < colorCount; i++)
            {
                fillPaint.Color = colors[i];
                canvas.DrawRect(
                    xPosition + border + i * cellWidth,
                    yPosition + border,
                    cellWidth,
                    cellHeight,
                    fillPaint
                );
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