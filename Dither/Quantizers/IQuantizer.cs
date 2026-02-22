namespace Dither.Quantizers;

public interface IQuantizer
{
    public float[] Quantize(float[] pixels);
}