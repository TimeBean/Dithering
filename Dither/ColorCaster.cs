namespace Dither;

public class ColorCaster
{
    public static void RgBtoLab(float red, float green, float blue, out float luminance, out float alpha, out float bb)
    {
        red = Linear(red);
        green = Linear(green);
        blue = Linear(blue);

        var x = red * 0.4124f + green * 0.3576f + blue * 0.1805f;
        var y = red * 0.2126f + green * 0.7152f + blue * 0.0722f;
        var z = red * 0.0193f + green * 0.1192f + blue * 0.9505f;

        const float xn = 0.95047f;
        const float yn = 1f;
        const float zn = 1.08883f;

        var fx = F(x / xn);
        var fy = F(y / yn);
        var fz = F(z / zn);

        luminance = 116 * fy - 16;
        alpha = 500 * (fx - fy);
        bb = 200 * (fy - fz);
        return;

        float F(float t)
        {
            return t > 0.008856f ? (float)Math.Pow(t, 1.0 / 3) : (903.3f * t + 16f) / 116f;
        }

        float Linear(float c)
        {
            return c <= 0.04045f ? c / 12.92f : (float)Math.Pow((c + 0.055f) / 1.055f, 2.4f);
        }
    }
}