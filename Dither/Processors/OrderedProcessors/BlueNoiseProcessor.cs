namespace Dither.Processors.OrderedProcessors;

public class BlueNoiseProcessor : OrderedProcessor
{
    public BlueNoiseProcessor(int width, int height, int rowBytes, int bytesPerPixel, int size)
        : base(width, height, rowBytes, bytesPerPixel, size) { }

    protected override int[,] GetMatrix(int size)
    {
        return GenerateBlueNoiseMatrix(size);
    }

    private static int[,] GenerateBlueNoiseMatrix(int size)
    {
        int total = size * size;
        bool[,] binary = InitialBinaryPattern(size);

        StabilisePattern(binary, size);

        int[,] rank = new int[size, size];

        var ones = CountOnes(binary, size);
        for (var r = ones - 1; r >= 0; r--)
        {
            (int row, int col) = FindTightestCluster(binary, size);
            binary[row, col] = false;
            rank[row, col] = r;
        }

        for (var r = ones; r < total; r++)
        {
            (var row, var col) = FindLargestVoid(binary, size);
            binary[row, col] = true;
            rank[row, col] = r;
        }

        return rank;
    }

    private static bool[,] InitialBinaryPattern(int size)
    {
        int total = size * size;
        int ones = total / 2;
        bool[,] binary = new bool[size, size];
        var rng = new Random(42);

        int[] indices = new int[total];
        for (var i = 0; i < total; i++)
        {
            indices[i] = i;
        }
        
        for (var i = total - 1; i > 0; i--)
        {
            var j = rng.Next(i + 1);
            (indices[i], indices[j]) = (indices[j], indices[i]);
        }

        for (var i = 0; i < ones; i++)
        {
            binary[indices[i] / size, indices[i] % size] = true;
        }

        return binary;
    }

    private static void StabilisePattern(bool[,] binary, int size)
    {
        while (true)
        {
            (int cr, int cc) = FindTightestCluster(binary, size);
            binary[cr, cc] = false;

            (int vr, int vc) = FindLargestVoid(binary, size);
            binary[vr, vc] = true;

            if (cr == vr && cc == vc) break;
        }
    }

    private const double Sigma = 1.5;
    private const double Sigma2 = 2.0 * Sigma * Sigma;

    private static double[,] ComputeEnergyField(bool[,] binary, int size)
    {
        double[,] energy = new double[size, size];

        int half = size / 2;
        double[] gauss = new double[size];
        for (int d = 0; d < size; d++)
        {
            int dist = d <= half ? d : d - size;
            gauss[d] = Math.Exp(-(dist * dist) / Sigma2);
        }

        for (int r = 0; r < size; r++)
        {
            for (int c = 0; c < size; c++)
            {
                if (!binary[r, c])
                {
                    continue;
                }

                for (var dr = 0; dr < size; dr++)
                {
                    for (var dc = 0; dc < size; dc++)
                    {
                        energy[(r + dr) % size, (c + dc) % size] += gauss[dr] * gauss[dc];
                    }
                }
            }
        }

        return energy;
    }

    private static (int row, int col) FindTightestCluster(bool[,] binary, int size)
    {
        double[,] energy = ComputeEnergyField(binary, size);
        
        var br = 0;
        var bc = 0;
        var best = double.NegativeInfinity;

        for (int r = 0; r < size; r++)
        for (int c = 0; c < size; c++)
            if (binary[r, c] && energy[r, c] > best)
            {
                best = energy[r, c];
                br = r;
                bc = c;
            }

        return (br, bc);
    }

    private static (int row, int col) FindLargestVoid(bool[,] binary, int size)
    {
        double[,] energy = ComputeEnergyField(binary, size);
        
        var br = 0;
        var bc = 0;
        var best = double.PositiveInfinity;

        for (var r = 0; r < size; r++)
        {
            for (var c = 0; c < size; c++)
            {
                if (!binary[r, c] && energy[r, c] < best)
                {
                    best = energy[r, c];
                    br = r;
                    bc = c;
                }
            }
        }

        return (br, bc);
    }

    private static int CountOnes(bool[,] binary, int size)
    {
        var count = 0;
        for (var r = 0; r < size; r++)
        {
            for (var c = 0; c < size; c++)
            {
                if (binary[r, c])
                {
                    count++;
                }
            }
        }

        return count;
    }
}