# Dithering

.NET implementation of dithring

![preview](https://raw.githubusercontent.com/TimeBean/Dithering/refs/heads/main/Preview.png)

## Usage

```c#
// load file
using var input = File.OpenRead(@"Examples/Mirana.png");

// get bitmap from file stream
using var originalBitmap = SKBitmap.Decode(input);

            
// stats
var width = originalBitmap.Width;
var height = originalBitmap.Height;
var rowBytes = originalBitmap.RowBytes;
var bytesPerPixel = originalBitmap.BytesPerPixel;

// pixelSpan that will be modified
var pixelSpan = originalBitmap.GetPixelSpan();

// reduces the number of colors to a fixed palette (e.g. 4 colors)
var quantizer = new LinearQuantizer(4);

// applies error diffusion dithering using Floyd–Steinberg
var processor = new FloydSteinbergProcessor(width, height, rowBytes, bytesPerPixel);

// dithring itself
var dithered = processor.Process(pixelSpan, quantizer);

// copy dithered span to original span
dithered.CopyTo(pixelSpan);

// save bitmap to file
var fileName = $"Out/dithered.png";
SaveBitmap(originalBitmap, fileName);
```

## Contributing

Pull requests are welcome. For major changes, please open an issue first
to discuss what you would like to change.

Please make sure to update tests as appropriate.

## License

[GNU GPL v3](https://raw.githubusercontent.com/TimeBean/Dithering/refs/heads/main/LICENSE)
