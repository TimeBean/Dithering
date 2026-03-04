namespace DitherConsole;

public enum LogLevel
{
    Information,
    Warning,
    Error,
    Success
}

public class Logger
{
    public static void Log(string point, string message, LogLevel logLevel = LogLevel.Information)
    {
        if (logLevel == LogLevel.Warning)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
        }
        else if (logLevel == LogLevel.Error)
        {
            Console.ForegroundColor = ConsoleColor.Red;
        }
        else if (logLevel == LogLevel.Success)
        {
            Console.ForegroundColor = ConsoleColor.Green;
        }
            
        Console.WriteLine($"[{DateTime.Now}] [{point}] {message}");
        
        Console.ResetColor();
    }
}