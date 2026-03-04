using Dither.Processors;
using Dither.Quantizers;

namespace DitherConsole.Model;

public record Dither(IQuantizer Quantizer, IProcessor Processor);