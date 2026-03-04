using System.Collections.Concurrent;
using Dither.Processors;
using Dither.Processors.ErrorDiffusionProcessors;
using Dither.Processors.NonDitherProcessors;
using Dither.Quantizers;
using Dither.Quantizers.Palette;
using DitherConsole.Model;
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

        private static bool _isDebug;

        private static void Main(string[] args)
        {
            ParseArguments(args);
            ClearOutDirectory();

            using var input = File.OpenRead(@"Examples/Mirana.png");
            using var bitmap = SKBitmap.Decode(input);

            var processors = ConstructProcessors(bitmap.Width, bitmap.Height, bitmap.RowBytes, bitmap.BytesPerPixel);

            var palettes = GetPalettes();
            var quantizers = ConstructQuantizers(palettes);

            var dithers = ConstructDithers(quantizers, processors);
            var dithereds = ProcessAll(bitmap, dithers);

            for (var i = 0; i < dithereds.Count; i++)
            {
                Logger.Log("Saver", $"Saving {dithereds[i].DitherName}{dithereds[i].QuantizerName}");
                
                var dithered = dithereds[i];
                SaveBitmap(FromBgra(dithered.Data, bitmap.Width, bitmap.Height), $"{Path}/{dithered.DitherName}_{dithered.QuantizerName}_{i}.png");
                
                Logger.Log("Saver", $"Saved {dithereds[i].DitherName} {dithereds[i].QuantizerName}", LogLevel.Success);
            }

            var canvasBitmap = new SKBitmap(bitmap.Width * quantizers.Count, bitmap.Height * processors.Count);
            var canvas = new SKCanvas(canvasBitmap);
            canvas.Clear(SKColors.Purple);
            
            var currentDither = dithereds.First().DitherName;
            var row = 0;
            var column = 0;
            foreach (var dithered in dithereds)
            {
                if (currentDither != dithered.DitherName)
                {
                    currentDither = dithered.DitherName;
                    column++;
                    row = 0;
                }

                canvas.DrawBitmap(
                    FromBgra(dithered.Data, bitmap.Width, bitmap.Height),
                    row * bitmap.Width,
                    column * bitmap.Height
                );

                row++;
            }
            
            SaveBitmap(canvasBitmap, $"{Path}/!grid.png");
            
            Logger.Log("Dither", "Done", LogLevel.Success);
        }

        private static void ClearOutDirectory()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, true);
            }

            Directory.CreateDirectory(Path);
        }

        private static SKBitmap FromBgra(byte[] bgra, int width, int height)
        {
            var info = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);

            if (bgra.Length != width * height * 4)
                throw new ArgumentException("Invalid buffer size");

            var bitmap = new SKBitmap(info);
            bgra.CopyTo(bitmap.GetPixelSpan());

            return bitmap;
        }

        private static List<Dithered> ProcessAll(SKBitmap bitmap, List<Model.Dither> dithers)
        {
            var dithereds = new ConcurrentBag<Dithered>();

            Parallel.ForEach(dithers, dither =>
            {
                var dithered = Process(bitmap, dither);

                dithereds.Add(dithered);
            });

            var result = dithereds
                .OrderBy(x => x.DitherName.Contains("OriginalProcessor") ? 0 : 1)
                .ThenBy(x => x.DitherName.Contains("BaseProcessor") ? 0 : 1)
                .ThenBy(x => x.DitherName, StringComparer.Ordinal)
                .ThenBy(x => x.QuantizerName, StringComparer.Ordinal)
                .ToList();

            return result;
        }

        private static Dithered Process(SKBitmap bitmap, Model.Dither dither)
        {
            Logger.Log("Algorithm",
                $"{dither.Processor.GetType().Name} via {dither.Quantizer} dithering...");

            using var currentBitmap = bitmap.Copy();
            var pixelSpan = currentBitmap.GetPixelSpan();

            dither.Processor.Process(ref pixelSpan, dither.Quantizer);

            var fileName = $"{Path}/{dither.Processor}_{dither.Quantizer}.png"
                .Replace(" ", "")
                .Replace("(", "-")
                .Replace(")", "");

            if (_isDebug)
            {
                const int topOffset = 2;
                const int leftOffset = 4;

                var canvas = new SKCanvas(currentBitmap);

                var palette = pixelSpan.GetPalette();
                var uniqueColors = pixelSpan.GetUniqueColorCount();
                
                DrawText(canvas, $"processor: {dither.Processor.ToString().Replace("Dither.Processors.", "")}", leftOffset, topOffset + FontSize);
                DrawText(canvas, $"quantizer: {dither.Quantizer}", leftOffset, topOffset + FontSize + 4 + FontSize);
                DrawText(canvas, $"output colors: {uniqueColors}", leftOffset, topOffset + FontSize + 4 + FontSize + 4 + FontSize);

                if (dither.Processor is not OriginalProcessor)
                {
                    DrawPalette(canvas, palette, leftOffset,
                        topOffset + FontSize + 4 + FontSize + 4 + FontSize + FontSize + 12, currentBitmap.Width);
                }
            }

            //SaveBitmap(currentBitmap, fileName);

            Logger.Log("Algorithm", $"{dither.Processor.GetType().Name} via {dither.Quantizer} dithered",
                LogLevel.Success);

            var dithered = new byte[pixelSpan.Length];
            pixelSpan.CopyTo(dithered);

            return new Dithered(dither.Processor.GetType().Name, dither.Quantizer.ToString(), dithered);
        }

        private static List<Model.Dither> ConstructDithers(List<IQuantizer> quantizers, List<IProcessor> processors)
        {
            var dithers = new List<Model.Dither>();

            foreach (var quantizer in quantizers)
            {
                foreach (var processor in processors)
                {
                    dithers.Add(new Model.Dither(quantizer, processor));
                }
            }

            return dithers;
        }

        private static List<IQuantizer> ConstructQuantizers(Palette[]? palettes = null)
        {
            var quantizers = new List<IQuantizer>
            {
                new LinearQuantizer(2),
                new LinearQuantizer(4),
                new LinearQuantizer(8),
                new OneBitQuantizer(false),
                new OneBitQuantizer(true)
            };
            
            if (palettes != null)
            {
                foreach (var palette in palettes)
                {
                    var name = palette.Name;
                    var data = palette.Data;

                    quantizers.Add(new EuclideanPaletteQuantizer(name, data));
                    quantizers.Add(new LinearEuclideanPaletteQuantizer(name, data));
                    quantizers.Add(new Cie76PaletteQuantizer(name, data));
                    quantizers.Add(new Ciede2000PaletteQuantizer(name, data));
                    quantizers.Add(new ManhattanPaletteQuantizer(name, data));
                    quantizers.Add(new OklabPaletteQuantizer(name, data));
                    quantizers.Add(new WeightedPaletteQuantizer(name, data));
                }
            }

            return quantizers;
        }

        private static List<IProcessor> ConstructProcessors(int width, int height, int rowBytes, int bytesPerPixel)
        {
            return new List<IProcessor>()
            {
                new OriginalProcessor(width, height, rowBytes, bytesPerPixel),
                new BaseProcessor(width, height, rowBytes, bytesPerPixel),
                new AtkinsonProcessor(width, height, rowBytes, bytesPerPixel),
                new BurkesProcessor(width, height, rowBytes, bytesPerPixel),
                new FloydSteinbergProcessor(width, height, rowBytes, bytesPerPixel),
                new JarvisJudiceNinkeProcessor(width, height, rowBytes, bytesPerPixel),
                new Sierra3Processor(width, height, rowBytes, bytesPerPixel),
                new StuckiProcessor(width, height, rowBytes, bytesPerPixel),
            };
        }

        private static Palette[] GetPalettes()
        {
            try
            {
                var palettes = PaletteParser.Parse();
                Logger.Log("Dither", "Palettes parsed", LogLevel.Success);

                return palettes;
            }
            catch (IOException e)
            {
                Logger.Log("Dither", $"Palettes not parsed\n{e}", LogLevel.Error);
                throw;
            }
        }

        private static void ParseArguments(string[] args)
        {
            for (var i = 0; i < args.Length; i++)
            {
                if (args[i].Contains("--debug"))
                {
                    InitializeDebug();
                }
            }
        }

        private static void InitializeDebug()
        {
            _isDebug = true;

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

            Logger.Log("Dither", "Debug enable", LogLevel.Warning);
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