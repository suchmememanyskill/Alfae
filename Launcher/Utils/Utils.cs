using Avalonia.Controls;
using MessageBox.Avalonia;
using MessageBox.Avalonia.BaseWindows.Base;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;

namespace Launcher.Utils
{
    public static class Utils
    {
        public static IMsBoxWindow<ButtonResult> CreateMessageBox(string title, string message,
            ButtonEnum buttons = ButtonEnum.Ok) =>
            MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams
            {
                ButtonDefinitions = buttons,
                ContentTitle = title,
                ContentMessage = message,
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = true,
            });
    }
}
