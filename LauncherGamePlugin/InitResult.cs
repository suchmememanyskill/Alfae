using LauncherGamePlugin.Interfaces;

namespace LauncherGamePlugin;

public class InitResult
{
    public List<IServiceMiddleware> Middlewares { get; set; } = new();
}