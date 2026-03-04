namespace DitherConsole.Model;

public class Palette
{
    public string Name { get; private set; }
    public float[,] Data { get; private set; }
    public int ColorCount =>  Data.GetLength(0);
    
    public Palette(string name, float[,] data)
    {
        Name = name;
        Data = data ?? throw new ArgumentNullException(nameof(data));
    }
}