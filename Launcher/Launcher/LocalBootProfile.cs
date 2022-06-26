using System.Collections.Generic;
using LauncherGamePlugin.Commands;
using LauncherGamePlugin.Launcher;

namespace Launcher.Launcher;

public class LocalBootProfile : CustomBootProfile
{
    public override List<Command> CustomCommands()
    {
        return new()
        {
            new("Edit", () => new CustomBootProfileGUI(Loader.App.GetInstance(), this).CreateProfileForm()),
            new ("Delete")
        };
    }
}