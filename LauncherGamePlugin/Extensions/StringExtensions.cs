using LauncherGamePlugin.Enums;

namespace LauncherGamePlugin.Extensions;

public static class StringExtensions
{
    public static string Curse(this string s)
    {
        if (PlatformExtensions.CurrentPlatform == Platform.Windows)
            return s;

        List<char> newString = new();
        string illegals = "&|>\"- ";

        for (int i = 0; i < s.Length; i++)
        {
            if (illegals.Contains(s[i]))
                newString.AddRange($"\\{s[i]}");
            else
                newString.Add(s[i]);
        }

        return new(newString.ToArray());
    }

    public static string StripIllegalFsChars(this string s)
    {
        return string.Join("_", s.Split(Path.GetInvalidFileNameChars()));
    }
}