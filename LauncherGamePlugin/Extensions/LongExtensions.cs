namespace LauncherGamePlugin.Extensions;

public static class LongExtensions
{
    private static readonly string[] gameSizes = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

    public static string ReadableSize(this long size)
    {
        if (size <= 0)
        {
            return "0 B";
        }

        var i = (int)Math.Floor(Math.Log(size, 1024));
        var p = Math.Pow(1024, i);
        var s = Math.Round(size / p, 2);
        return $"{s} {gameSizes[i]}";
    }
}