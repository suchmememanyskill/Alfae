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
        switch (formEntry)
        {
            case TextInputElement textInputElement:
                return new TextInput(textInputElement);
            case TextBoxElement textBoxElement:
                return new TextBox(textBoxElement);
            case ToggleElement toggleElement:
                return new Toggle(toggleElement);
            case FilePickerElement:
            case FolderPickerElement:
                return new FilePicker(formEntry);
            case ClickableLinkBoxElement clickableLinkBoxElement:
                return new ClickableLinkBox(clickableLinkBoxElement);
            case ButtonListElement buttonListElement:
                return new ButtonList(buttonListElement);
            case DropdownElement dropdownElement:
                return new Dropdown(dropdownElement);
            case SeperatorElement separatorElement:
                return new Separator(separatorElement);
            case ImageElement imageElement:
                return new ImageView(imageElement);
            case HorizontalPanelElement horizontalPanelElement:
                return new HorizontalPanel(horizontalPanelElement);
            default:
                throw new NotImplementedException();
        }
    }
}