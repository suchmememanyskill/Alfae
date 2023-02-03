using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Launcher.Extensions;
using LauncherGamePlugin.Commands;

namespace Launcher.Views;

public partial class MenuButton : UserControl
{
    private List<TemplatedControl> _items = new();

    public MenuButton()
    {
        InitializeComponent();
    }
        
    public MenuButton(string header)
        : this()
    {
        Menu.Header = header;
        Menu.Items = _items = new List<TemplatedControl>();
    }

    public MenuButton(IEnumerable<TemplatedControl> items, string header)
        : this(header)
    {
        Menu.Items = _items = items.ToList();
    }

    public MenuButton(IEnumerable<Command> items, string header)
        : this(items.Select(x => x.ToTemplatedControl()), header)
    {
    }

    public void Add(Command command)
    {
        _items.Add(command.ToTemplatedControl());
        Menu.Items = null;
        Menu.Items = _items;
    }

    public void SetFontSize(double fontSize) => Menu.FontSize = fontSize;
}