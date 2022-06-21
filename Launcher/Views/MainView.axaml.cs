using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Launcher.Extensions;
using Launcher.Utils;
using LauncherGamePlugin.Commands;
using LauncherGamePlugin.Extensions;

namespace Launcher.Views;

public partial class MainView : UserControlExt<MainView>
{
    [Binding(nameof(PluginMenu), "Items")] public List<TemplatedControl> MenuItems => GenerateMenuItems();

    private GameViewSmall _currentSelection;
    
    public MainView()
    {
        InitializeComponent();
        SetControls();
        UpdateView();
        ListBox.SelectionChanged += (_, _) =>
        {
            ListBoxItem? item = ListBox.SelectedItem as ListBoxItem;
            if (item?.Content is not GameViewSmall gameViewSmall)
                return;
            if (_currentSelection != null)
            {
                if (Equals(_currentSelection, gameViewSmall))
                    return;
                
                _currentSelection.Deselected();
            }

            _currentSelection = gameViewSmall;
            _currentSelection.Selected();
        };
    }

    private List<TemplatedControl> GenerateMenuItems()
    {
        Loader.App app = Loader.App.GetInstance();
        List<TemplatedControl> items = new();
        
        app.GameSources.ForEach(x =>
        {
            MenuItem item = new MenuItem();
            item.IsEnabled = false;
            item.Header = $"{x.ServiceName} - {x.Version}";
            items.Add(item);
        });
        
        items.Add(new Separator());
        
        app.GameSources.ForEach(x =>
        {
            if (x.GlobalCommands.Count > 0)
            {
                MenuItem root = new()
                {
                    Header = x.ServiceName
                };
                List<TemplatedControl> controls = x.GlobalCommands.Select(x => x.ToTemplatedControl()).ToList();
                root.Items = controls;
                items.Add(root);
            }
        });

        return items;
    }
}