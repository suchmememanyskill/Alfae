using Avalonia.Controls;
using MessageBox.Avalonia;
using MessageBox.Avalonia.BaseWindows.Base;
using MessageBox.Avalonia.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Launcher.Utils
{
    public static class Utils
    {
        public static IMsBoxWindow<ButtonResult> CreateMessageBox(string title, string message,
            ButtonEnum buttons = ButtonEnum.Ok) =>
            MessageBoxManager.GetMessageBoxStandardWindow(new MessageBox.Avalonia.DTO.MessageBoxStandardParams
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
