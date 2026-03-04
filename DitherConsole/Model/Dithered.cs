namespace DitherConsole.Model;

public class Dithered
{
    public Dithered(string ditherName, string quantizerName, byte[] data)
    {
        DitherName = ditherName;
        QuantizerName = quantizerName;
        Data = data;
    }
    
    public string DitherName;
    public string QuantizerName;
    public byte[] Data;
}   