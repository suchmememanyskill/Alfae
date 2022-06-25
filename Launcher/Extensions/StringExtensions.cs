﻿using LauncherGamePlugin;

namespace Launcher.Extensions;

public static class StringExtensions
{
    public static string Curse(this string s)
    {
        if (PlatformExtensions.CurrentPlatform == Platform.Windows)
            return s;
        
        string copy = new(s);
        string illegals = "&|>\"- ";
        for (int i = 0; i < illegals.Length; i++)
        {
            copy = copy.Replace($"{illegals[i]}", $"\\{illegals[i]}");
        }

        return copy;
    }
}