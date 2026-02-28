using System.Runtime.CompilerServices;

namespace BasicFaceitServer.Infrastructure;

public static class PluginLogger
{
    public static void Info(
        string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "")
    {
        Log(message, ConsoleColor.Green, "Info", memberName, filePath);
    }

    public static void Warn(
        string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "")
    {
        Log(message, ConsoleColor.Yellow, "Warn", memberName, filePath);
    }

    public static void Error(
        string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "")
    {
        Log(message, ConsoleColor.Red, "Error", memberName, filePath);
    }

    public static void Debug(
        string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "")
    {
        Log(message, ConsoleColor.Cyan, "Debug", memberName, filePath);
    }

    private static void Log(
        string message,
        ConsoleColor color,
        string level,
        string memberName,
        string filePath)
    {
        var className = Path.GetFileNameWithoutExtension(filePath);
        Console.ForegroundColor = color;
        Console.WriteLine($"[{level}] [{className}.{memberName}] {message}");
        Console.ResetColor();
    }
}