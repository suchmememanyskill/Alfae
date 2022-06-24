using System;
using System.Linq;
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
                MenuItem item = new()
                {
                    IsEnabled = false,
                    Header = command.Text
                };
                return item;
            case CommandType.Separator:
                return new Separator();
            case CommandType.Function:
                MenuItem item2 = new()
                {
                    Command = new LambdaCommand(_ => command.Action()),
                    Header = command.Text
                };
                return item2;
            case CommandType.SubMenu:
                MenuItem root = new()
                {
                    Header = command.Text,
                    Items = command.SubCommands.Select(x => x.ToTemplatedControl()).ToList()
                };
                return root;
            default:
                throw new NotImplementedException();
        }
    }
}