namespace Dither.Exceptions;

public class WrongLevelQuantityException : Exception
{
    public WrongLevelQuantityException() { }

    public WrongLevelQuantityException(string message) : base(message) { }
}