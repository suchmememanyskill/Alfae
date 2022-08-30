using System.Diagnostics;

namespace LauncherGamePlugin;

public enum LogType
{
    Info,
    Warn,
    Error,
}

public class Logger
{
    private readonly string LOGPATH = "./app.log";
    private List<string> _logs = new();

    public Logger()
    {
        AppDomain.CurrentDomain.ProcessExit += (sender, args) =>
        {
            try
            {
                File.WriteAllText(LOGPATH, string.Join('\n', _logs));
            }
            catch (Exception e)
            {
                Log("Failed to open log file", LogType.Error, "Logger");
            }
        };
    }

    public void Log(string message, LogType type = LogType.Info, string service = "App")
    {
        string result = $"[{type}] [{service}] {message}";
        Debug.WriteLine(result);
        Console.WriteLine(result);
        _logs.Add(result);
    }
}