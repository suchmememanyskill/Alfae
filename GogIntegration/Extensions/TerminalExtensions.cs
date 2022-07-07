using LauncherGamePlugin;

namespace GogIntegration.Extensions;

public static class TerminalExtensions
{
    public static Task<bool> ExecGog(this Terminal t, string args) => t.Exec("gogdl", args);
}