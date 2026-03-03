using System.Diagnostics;
using Dither.Processors;
using Dither.Processors.ErrorDiffusionProcessors;
using Dither.Processors.NonDitherProcessors;
using Dither.Processors.OrderedProcessors;
using Dither.Quantizers;
using Dither.Quantizers.Palette;
using SkiaSharp;

namespace DitherConsole
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class Program
    {
        private static SKPaint _textPaint = null!;
        private static SKPaint _shadowPaint = null!;

        private static readonly SKFont Font = new(SKTypeface.Default, 14f);
        private static readonly int FontSize = Convert.ToInt32(Font.Size);

        private const string Path = "Out";

        private static bool _isDebug = true;

        private static void Main(string[] args)
        {
            var totalProcessingTime = Stopwatch.StartNew();
            long algorithmProcessingTime = 0;
            var algorithmCount = 0;

            foreach (var arg in args)
            {
                if (arg.Contains("--debug"))
                {
                    Log("Dither", "Debug enable", ConsoleColor.Yellow);

                    _isDebug = true;
                }
            }

            if (_isDebug)
            {
                _textPaint = new SKPaint
                {
                    Color = SKColors.GhostWhite,
                    IsAntialias = true
                };

                _shadowPaint = new SKPaint
                {
                    Color = SKColors.Black,
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 4f,
                    MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 2f)
                };
            }

            using var input = File.OpenRead(@"Examples/Mirana.png");

            using var originalBitmap = SKBitmap.Decode(input);

            var width = originalBitmap.Width;
            var height = originalBitmap.Height;
            var rowBytes = originalBitmap.RowBytes;
            var bpp = originalBitmap.BytesPerPixel;

            if (!Directory.Exists(Path))
            {
                Log("Dither", $"{Path}/ doesn't exists, creating...");
                Directory.CreateDirectory("Out");
            }
            else
            {
                Log("Dither", $"{Path}/ exists, deleting and creating...");
                Directory.Delete($"{Path}", true);
                Directory.CreateDirectory($"{Path}");
            }

            var processors = new (string Name, Func<IProcessor> Processor)[]
            {
                ("Original", () => new OriginalProcessor(width, height, rowBytes, bpp)),
                ("Base", () => new BaseProcessor(width, height, rowBytes, bpp)),
                ("Atkinson", () => new AtkinsonProcessor(width, height, rowBytes, bpp)),
                ("Burkes", () => new BurkesProcessor(width, height, rowBytes, bpp)),
                ("Floyd-Steinberg", () => new FloydSteinbergProcessor(width, height, rowBytes, bpp)),
                ("Jarvis-Judice-Ninke", () => new JarvisJudiceNinkeProcessor(width, height, rowBytes, bpp)),
                ("Sierra_3_Row", () => new Sierra3Processor(width, height, rowBytes, bpp)),
                ("Stucki", () => new StuckiProcessor(width, height, rowBytes, bpp)),
                ("Bayer_2x2", () => new BayerProcessor(width, height, rowBytes, bpp, 2)),
                ("Bayer_4x4", () => new BayerProcessor(width, height, rowBytes, bpp, 4)),
                ("Bayer_8x8", () => new BayerProcessor(width, height, rowBytes, bpp, 8)),
                ("Cluster_2x2", () => new ClusterProcessor(width, height, rowBytes, bpp, 2)),
                ("Cluster_4x4", () => new ClusterProcessor(width, height, rowBytes, bpp, 4)),
                ("Cluster_8x8", () => new ClusterProcessor(width, height, rowBytes, bpp, 8)),
                ("Halftone_2x2", () => new HalftoneProcessor(width, height, rowBytes, bpp, 2)),
                ("Halftone_4x4", () => new HalftoneProcessor(width, height, rowBytes, bpp, 4)),
                ("Halftone_8x8", () => new HalftoneProcessor(width, height, rowBytes, bpp, 8)),
                ("BlueNoise_2x2", () => new BlueNoiseProcessor(width, height, rowBytes, bpp, 2)),
                ("BlueNoise_4x4", () => new BlueNoiseProcessor(width, height, rowBytes, bpp, 4)),
            };

            var palettes = PaletteCaster.ParsePalettes();

            var quantizers = new (string Name, IQuantizer Quantizer)[]
                {
                    /*(Name: "Linear_2x", Quantizer: new LinearQuantizer(2)),
                    (Name: "Linear_8x", Quantizer: new LinearQuantizer(8))*/
                }
                .Concat(palettes.SelectMany(p => new (string, IQuantizer)[]
                {
                    /*($"Euclidean {p.Name} ({p.ColorCount})", new EuclideanPaletteQuantizer(p.Data)),
                    ($"Manhattan {p.Name} ({p.ColorCount})", new ManhattanPaletteQuantizer(p.Data)),
                    */($"Linear Euclidean {p.Name} ({p.ColorCount})", new LinearEuclideanPaletteQuantizer(p.Data)),/*
                    ($"Weighted {p.Name} ({p.ColorCount})", new WeightedPaletteQuantizer(p.Data)),
                    ($"Cie76 palette {p.Name} ({p.ColorCount})", new Cie76PaletteQuantizer(p.Data)),
                    ($"Oklab palette {p.Name} ({p.ColorCount})", new OklabPaletteQuantizer(p.Data)),
                    ($"Ciede2000 palette {p.Name} ({p.ColorCount})", new Ciede2000PaletteQuantizer(p.Data))*/
                }))
                .ToArray();

            using var compilationBitmap = new SKBitmap(width * quantizers.Length, height * processors.Length);
            using var canvas = new SKCanvas(compilationBitmap);
            canvas.Clear(SKColors.Black);

            for (var pIndex = 0; pIndex < processors.Length; pIndex++)
            {
                var processorDef = processors[pIndex];
                var yPosition = pIndex * height;

                for (var qIndex = 0; qIndex < quantizers.Length; qIndex++)
                {
                    var quantizerDef = quantizers[qIndex];
                    var xPosition = qIndex * width;

                    using var currentBitmap = originalBitmap.Copy();
                    var pixelSpan = currentBitmap.GetPixelSpan();

                    var processor = processorDef.Processor();

                    Log("Processor", $"Processing dither...");
                    var processorStopwatch = Stopwatch.StartNew();
                    processor.Process(ref pixelSpan, quantizerDef.Item2);
                    processorStopwatch.Stop();
                    algorithmProcessingTime += processorStopwatch.ElapsedMilliseconds;
                    algorithmCount++;
                    Log("Processor", $"Processed dither in {processorStopwatch.ElapsedMilliseconds} ms",
                        ConsoleColor.Green);

                    Log("Canvas", $"Drawing dithered...");
                    canvas.DrawBitmap(currentBitmap, xPosition, yPosition);
                    Log("Canvas", $"Dithered added to canvas", ConsoleColor.Green);

                    var processorText = processorDef.Name.Replace("_", " ");
                    var quantizerText = quantizerDef.Item1.Replace("_", " ");

                    const int topOffset = 2;
                    const int leftOffset = 4;

                    if (_isDebug)
                    {
                        Log("Canvas", $"Add debug info to image", ConsoleColor.Yellow);

                        if (processors[pIndex].Name == "Original")
                        {
                            DrawText(canvas, $"colors: {pixelSpan.GetUniqueColorCount()}", xPosition + leftOffset,
                                yPosition + topOffset + FontSize + 4 + FontSize + 4 + FontSize);
                        }
                        else
                        {
                            DrawText(canvas, $"processor: {processorText}", xPosition + leftOffset,
                                yPosition + topOffset + FontSize);
                            DrawText(canvas, $"quantizer: {quantizerText}", xPosition + leftOffset,
                                yPosition + topOffset + FontSize + 4 + FontSize);
                            DrawText(canvas, $"output colors: {pixelSpan.GetUniqueColorCount()}",
                                xPosition + leftOffset,
                                yPosition + topOffset + FontSize + 4 + FontSize + 4 + FontSize);

                            if (quantizers[qIndex].Item2.GetType() == typeof(PaletteQuantizer))
                            {
                                DrawPalette(canvas, ((PaletteQuantizer)quantizers[qIndex].Item2).Palette,
                                    xPosition + leftOffset,
                                    yPosition + topOffset + FontSize + 4 + FontSize + 4 + FontSize + FontSize, width);
                            }

                            DrawPalette(canvas, pixelSpan.GetPalette(), xPosition + leftOffset,
                                yPosition + topOffset + FontSize + 4 + FontSize + 4 + FontSize + FontSize + 12, width);
                        }
                    }

                    var fileName = $"Out/{processorDef.Name}_{quantizerDef.Item1}.png".Replace(" ", "_")
                        .Replace("_", "")
                        .Replace("(", "-")
                        .Replace(")", "");

                    SaveBitmap(currentBitmap, fileName);

                    Log("Algorithm", $"Saved {fileName}");
                }
            }

            Log("Grid", "Saving bitmap...");

            const string compilationFileName = "Out/!CompilationGrid.png";
            SaveBitmap(compilationBitmap, compilationFileName);

            totalProcessingTime.Stop();
            Log("Dither", $"{algorithmCount} algorithms done in {algorithmProcessingTime} ms ({algorithmProcessingTime / (float)algorithmCount} ms)", ConsoleColor.Green);
            Log("Dither", $"Done in {totalProcessingTime.ElapsedMilliseconds} ms", ConsoleColor.Green);
        }

        private static void Log(string point, string message, ConsoleColor? color = null)
        {
            if (color.HasValue)
            {
                Console.ForegroundColor = (ConsoleColor)color;
            }

            Console.WriteLine($"[{DateTime.Now}] [{point}] {message}");

            if (color.HasValue)
            {
                Console.ResetColor();
            }
        }

        private static void DrawText(SKCanvas canvas, string text, int xPosition, int yPosition)
        {
            canvas.DrawText(text, xPosition + 2, yPosition + 1, Font, _shadowPaint);
            canvas.DrawText(text, xPosition, yPosition, Font, _textPaint);
        }

        private static void DrawPalette(SKCanvas canvas, float[,] palette, int xPosition, int yPosition, int width)
        {
            var colorCount = palette.GetUniqueColorCount();
            if (colorCount <= 0)
            {
                return;
            }

            var colors = palette.ToSkColors();

            const int cellWidth = 8;
            const int cellHeight = 8;
            const int border = 1;

            var availableForCells = Math.Max(1, width - border * 2);

            var maxPerRow = Math.Max(1, availableForCells / cellWidth);

            var colsInFirstRow = Math.Min(colorCount, maxPerRow);

            var rows = (colorCount + maxPerRow - 1) / maxPerRow;

            var totalWidth = border * 2 + colsInFirstRow * cellWidth;
            var totalHeight = border * 2 + rows * cellHeight;

            using var bgPaint = new SKPaint();
            bgPaint.Style = SKPaintStyle.Fill;
            bgPaint.Color = SKColors.Purple;

            canvas.DrawRect(xPosition, yPosition, totalWidth, totalHeight, bgPaint);

            using var fillPaint = new SKPaint();
            fillPaint.Style = SKPaintStyle.Fill;
            fillPaint.IsAntialias = true;

            for (var i = 0; i < colorCount; i++)
            {
                var row = i / maxPerRow;
                var col = i % maxPerRow;

                fillPaint.Color = colors[i];

                var x = xPosition + border + col * cellWidth;
                var y = yPosition + border + row * cellHeight;

                canvas.DrawRect(x, y, cellWidth, cellHeight, fillPaint);
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