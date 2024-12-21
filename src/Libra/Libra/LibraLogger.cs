using Libra;

public class LibraLogger
{
    public static void Debug(string msg, int nivel = 0)
    {
        if(nivel > Interpretador.NivelDebug)
            return;
        
        Msg(msg, "DEBUG", ConsoleColor.Blue);
    }

    public static void Log(string msg)
    {
        Msg(msg);
    }

    private static void Msg(string msg, string prefix = "LOG", ConsoleColor cor = ConsoleColor.White)
    {
        Console.ForegroundColor = cor;
        msg = msg.Replace("\n", $"{Environment.NewLine}[{prefix}] ");
        Console.WriteLine($"[{prefix}] {msg}");
        Console.ResetColor();
    }
}