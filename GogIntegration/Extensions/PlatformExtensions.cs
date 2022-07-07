using LauncherGamePlugin.Enums;

namespace GogIntegration.Extensions;

public static class PlatformExtensions
{
    public static string? GetGogDlString(this Platform platform)
    {
        if (platform == Platform.Windows)
            return "windows";

        if (platform == Platform.Linux)
            return "linux";

        return null;
    }
}