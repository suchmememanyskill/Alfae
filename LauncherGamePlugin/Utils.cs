using System.Diagnostics;
using System.Runtime.InteropServices;

namespace LauncherGamePlugin;

public static class Utils
{
    public static void OpenUrl(string url)
    {
        // hack because of this: https://github.com/dotnet/corefx/issues/10361
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            url = url.Replace("&", "^&");
            Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Process.Start("xdg-open", url);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Process.Start("open", url);
        }
        else throw new Exception("No url 4 u");
    }

    public static void OpenFolder(string path)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            Process.Start("explorer.exe", "\"" + path.Replace("/", "\\") + "\""); // I love windows hacks
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            Process.Start("xdg-open", path);
    }

    public static void OpenFolderWithHighlightedFile(string path)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            Process.Start("explorer.exe", "/select,\"" + path.Replace("/", "\\") + "\""); // I love windows hacks

        else OpenFolder(path);
    }
}