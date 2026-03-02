using System.Globalization;

namespace DitherConsole;

public static class PaletteCaster
{
    public static Palette[] ParsePalettes()
    {
        const string path = @"Palettes/";

        if (!Directory.Exists(path))
        {
            throw new IOException($"The directory '{path}' doesn't exist.");
        }

        var paletteFiles = Directory.GetFiles(path, "*.hex");

        if (paletteFiles.Length == 0)
        {
            throw new IOException($"No palette files found in '{path}'.");
        }

        var palettes = new List<Palette>(paletteFiles.Length);

        foreach (var file in paletteFiles)
        {
            var data = ParsePaletteFile(file); 
            var name = Path.GetFileNameWithoutExtension(file);
            palettes.Add(new Palette(name, data));
        }

        return palettes.ToArray();
    }

    private static float[,] ParsePaletteFile(string filePath)
    {
        var lines = File.ReadAllLines(filePath)
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrEmpty(l) && !l.StartsWith("#"))
            .ToArray();

        if (lines.Length == 0)
            return new float[0, 3];

        var result = new float[lines.Length, 3];

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            if (line.Length != 6)
                throw new FormatException(
                    $"Invalid color at line {i + 1} in '{filePath}': '{line}' (expected 6 hex chars).");

            var r = byte.Parse(line.AsSpan(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            var g = byte.Parse(line.AsSpan(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            var b = byte.Parse(line.AsSpan(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);

            result[i, 0] = b; 
            result[i, 1] = g; 
            result[i, 2] = r; 
        }

        return result;
    }
}