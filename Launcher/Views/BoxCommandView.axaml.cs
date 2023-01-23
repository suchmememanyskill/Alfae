using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Launcher.Utils;
using LauncherGamePlugin.Commands;

namespace Launcher.Views;

public partial class BoxCommandView : UserControl
{
    public event Action<string>? OnButtonPress;
    private List<Control> _items = new();
    
    public BoxCommandView()
    {
        InitializeComponent();
    }

    public BoxCommandView(string header) : this()
    {
        Header.Content = header;
    }

    public BoxCommandView(string header, IEnumerable<Control> items) : this(header)
    {
        _items = items.ToList();
        Items.Children.AddRange(_items);
    }
    
    public BoxCommandView(string header, IEnumerable<Command> items) : this(header)
    {
        _items = items.Select(CommandToControl).ToList();
        Items.Children.AddRange(_items);
    }
    
    private Control CommandToControl(Command c)
    {
        switch (c.Type)
        {
            case CommandType.Separator:
                return new Rectangle()
                {
                    Fill = Brushes.Black,
                    Margin = new(5, 4),
                    Height = 1,
                };
            case CommandType.SubMenu:
                MenuButton button = new MenuButton(c.SubCommands, c.Text!)
                {
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                    TopMenu =
                    {
                        Background = Brushes.Transparent,
                        Height = 27,
                    },
                    Menu =
                    {
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                        FontSize = 14,
                        Padding = new(3)
                    },
                };

                return button;
            case CommandType.Text:
                return new Label()
                {
                    Content = c.Text,
                    Foreground = SolidColorBrush.Parse("#777")
                };
            case CommandType.Function:
                return new Button()
                {
                    Content = c.Text,
                    Command = new LambdaCommand(_ =>
                    {
                        c.Action?.Invoke();
                        OnButtonPress?.Invoke(c.Text!);
                    }),
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                    Background = Brushes.Transparent,
                    FontSize = 14,
                    Padding = new(3),
                };
            default:
                throw new NotImplementedException();
        }
    }
}