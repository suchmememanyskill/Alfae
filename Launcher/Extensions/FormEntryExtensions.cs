using System;
using Avalonia.Controls;
using Launcher.Forms.FormTemplates;
using LauncherGamePlugin.Forms;
using TextBox = Launcher.Forms.FormTemplates.TextBox;
using Separator = Launcher.Forms.FormTemplates.Separator;

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
                return new TextBox(formEntry);
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
            case FormEntryType.Separator:
                return new Separator(formEntry);
            case FormEntryType.Image:
                return new ImageView(formEntry);
            case FormEntryType.HorizontalPanel:
                return new HorizontalPanel(formEntry);
            default:
                throw new NotImplementedException();
        }
    }
}