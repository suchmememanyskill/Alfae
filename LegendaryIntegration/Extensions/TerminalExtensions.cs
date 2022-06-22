using LegendaryIntegration.Service;

namespace LegendaryIntegration.Extensions;

public static class TerminalExtensions
{
    public static Task<bool> ExecLegendary(this Terminal t, string args) => t.Exec(LegendaryAuth.LegendaryPath, args);
}