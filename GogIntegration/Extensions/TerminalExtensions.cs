using LauncherGamePlugin;

namespace GogIntegration.Extensions;

public static class TerminalExtensions
{
    public static Task<bool> ExecGog(this Terminal t, string args, string authToken)
    {
        if (!t.NoLog.Contains(authToken))
            t.NoLog.Add(authToken);
        return t.Exec("gogdl", args);
    }
}