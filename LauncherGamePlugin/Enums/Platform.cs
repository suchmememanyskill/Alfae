﻿using System.Runtime.InteropServices;

namespace LauncherGamePlugin.Enums;

public enum Platform
{
    Windows,
    Linux,
    Unknown,
}

public static class PlatformExtensions
{
    public static Platform CurrentPlatform =>
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? Platform.Windows : Platform.Linux;
}