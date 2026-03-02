namespace Dither.Exceptions;

public sealed class WrongLevelQuantityException : Exception
{
    public WrongLevelQuantityException()
    {
    }

    public WrongLevelQuantityException(string message) : base(message)
    {
    }
}