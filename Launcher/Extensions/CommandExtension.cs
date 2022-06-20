using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Launcher.Utils;
using LauncherGamePlugin.Commands;

namespace Launcher.Extensions;

public static class CommandExtension
{
    public static TemplatedControl ToTemplatedControl(this Command command)
    {
        switch (command.Type)
        {
            case CommandType.Text:
                MenuItem item = new();
                item.IsEnabled = false;
                item.Header = command.Text;
                return item;
            case CommandType.Separator:
                return new Separator();
            case CommandType.Function:
                MenuItem item2 = new();
                item2.Command = new LambdaCommand(_ => command.Action());
                item2.Header = command.Text;
                return item2;
            default:
                throw new NotImplementedException();
        }
    }
}