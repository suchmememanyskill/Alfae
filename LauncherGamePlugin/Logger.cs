using System.Diagnostics;
using LauncherGamePlugin.Interfaces;

namespace LauncherGamePlugin;

public enum LogType
{
    Debug = 0,
    Info = 1,
    Warn = 2,
    Error = 3,
}

public class Logger
{
    private string LOGPATH = "./app.log";
    private List<string> _logs = new();
    private LogType _logLevel;

    public Logger(IApp app, LogType logLevel)
    {
        _logLevel = logLevel;

        try
        {
            LOGPATH = Path.Join(app.ConfigDir, "app.log");
        }
        catch { }
        
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
        if (type < _logLevel)
            return;

        string result = $"[{type}] [{service}] {message}";
        Debug.WriteLine(result);
        Console.WriteLine(result);
        _logs.Add(result);
    }
}