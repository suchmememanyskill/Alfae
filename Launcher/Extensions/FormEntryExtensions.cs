using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Launcher.Forms.FormTemplates;
using LauncherGamePlugin.Forms;

namespace Launcher.Extensions;

public static class FormEntryExtensions
{
    public static UserControl ToTemplatedControl(this FormEntry formEntry)
    {
        switch (formEntry.Type)
        {
            case FormEntryType.TextInput:
                return new TextInput(formEntry);
            case FormEntryType.TextBox:
                return new Forms.FormTemplates.TextBox(formEntry);
            case FormEntryType.Toggle:
                return new Toggle(formEntry);
            case FormEntryType.FilePicker:
            case FormEntryType.FolderPicker:
                return new FilePicker(formEntry);
            case FormEntryType.ClickableLinkBox:
                return new ClickableLinkBox(formEntry);
            case FormEntryType.ButtonList:
                return new ButtonList(formEntry);
            case FormEntryType.Dropdown:
                return new Dropdown(formEntry);
            default:
                throw new NotImplementedException();
        }
    }
}