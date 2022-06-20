using System.Diagnostics;
using System.IO;

namespace LauncherGamePlugin;

public enum LogType
{
    Info,
    Warn,
    Error,
}

public class Logger
{
    private FileStream? _stream = null;
    private StreamWriter? _streamWriter = null;
    private readonly string LOGPATH = "./app.log";

    public Logger()
    {
        try
        {
            _stream = File.Open(LOGPATH, FileMode.Create);
            _streamWriter = new(_stream);
        }
        catch (Exception e)
        {
            Log("Failed to open log file", LogType.Error, "Logger");
        }
        
        AppDomain.CurrentDomain.ProcessExit += (sender, args) =>
        {
            _streamWriter?.Close();
            _stream?.Close();
        };
    }
    
    public void Log(string message, LogType type = LogType.Info, string service = "App")
    {
        string result = $"[{type}] [{service}] {message}";
        Debug.WriteLine(result);
        _streamWriter?.WriteLine(result);
    }
}