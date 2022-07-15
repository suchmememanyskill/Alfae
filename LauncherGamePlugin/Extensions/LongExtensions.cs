namespace LauncherGamePlugin.Extensions;

public static class LongExtensions
{
    private static readonly string[] gameSizes = { "B", "KB", "MB", "GB" };

    public static string ReadableSize(this long size)
    {
        int type = 0;
        double bytesLeft = size;
        while (bytesLeft >= 1024)
        {
            type++;
            bytesLeft /= 1024;
        }

        return $"{bytesLeft:0.00} {gameSizes[type]}";
    }
}