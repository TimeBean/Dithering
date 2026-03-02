namespace Dither.Processors.ErrorDiffusionProcessors;

public sealed class JarvisJudiceNinkeProcessor : ErrorDiffusionProcessor
{
    public JarvisJudiceNinkeProcessor(int width, int height, int rowBytes, int bytesPerPixel)
        : base(width, height, rowBytes, bytesPerPixel)
    {
    }

    protected override void DistributeError(Span<byte> pixels, int x, int y, int channel, double error)
    {
        Add(pixels, x + 1, y, channel, error, 7.0 / 48.0);
        Add(pixels, x + 2, y, channel, error, 5.0 / 48.0);

        Add(pixels, x - 2, y + 1, channel, error, 3.0 / 48.0);
        Add(pixels, x - 1, y + 1, channel, error, 5.0 / 48.0);
        Add(pixels, x, y + 1, channel, error, 7.0 / 48.0);
        Add(pixels, x + 1, y + 1, channel, error, 5.0 / 48.0);
        Add(pixels, x + 2, y + 1, channel, error, 3.0 / 48.0);

        Add(pixels, x - 2, y + 2, channel, error, 1.0 / 48.0);
        Add(pixels, x - 1, y + 2, channel, error, 3.0 / 48.0);
        Add(pixels, x, y + 2, channel, error, 5.0 / 48.0);
        Add(pixels, x + 1, y + 2, channel, error, 3.0 / 48.0);
        Add(pixels, x + 2, y + 2, channel, error, 1.0 / 48.0);
    }

    private void Add(Span<byte> pixels, int nx, int ny, int channel, double error, double factor)
    {
        if (nx < 0 || nx >= Width || ny < 0 || ny >= Height)
        {
            return;
        }

        var idx = ny * RowBytes + nx * BytesPerPixel + channel;

        var value = pixels[idx] + error * factor;

        value = value switch
        {
            < 0 => 0,
            > 255 => 255,
            _ => value
        };

        pixels[idx] = (byte)Math.Round(value);
    }
}