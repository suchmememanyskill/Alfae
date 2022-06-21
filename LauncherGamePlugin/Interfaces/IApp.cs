﻿using LauncherGamePlugin;
using LauncherGamePLugin;
using LauncherGamePlugin.Forms;

namespace LauncherGamePlugin.Interfaces;

public interface IApp
{
    public string ConfigDir { get; }
    public Logger Logger { get; }
    void ShowForm(Form form);
    void HideOverlay();
    void ReloadGames();
    void Launch(ExecLaunch launch);
}