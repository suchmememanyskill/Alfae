using System;
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

    [Binding(nameof(ProfileMenu), "Items")]
    public List<TemplatedControl> BootProfileItems =>
        _app.Launcher.BuildCommands().Select(x => x.ToTemplatedControl()).ToList();

    [Binding(nameof(DownloadLocationButton), "Content")]
    public string DlText => $"Current download location: {_app.GameDir}";
    
    private GameViewSmall _currentSelection;
    private Loader.App _app = Loader.App.GetInstance();
    
    public MainView()
    {
        InitializeComponent();
        SetControls();
        UpdateView();
        InstalledListBox.SelectionChanged += (_, _) => MonitorListBox(InstalledListBox);
        NotInstalledListBox.SelectionChanged += (_, _) => MonitorListBox(NotInstalledListBox);
        SearchBox.KeyUp += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
                _app.GameViews.ForEach(x => x.IsVisible = true);
            else
                _app.GameViews.ForEach(x =>
                    x.IsVisible = x.GameName.Contains(SearchBox.Text, StringComparison.OrdinalIgnoreCase) || 
                                  x.Game.Source.ShortServiceName.Contains(SearchBox.Text, StringComparison.OrdinalIgnoreCase));
        };
    }

    private void MonitorListBox(ListBox box)
    {
        GameViewSmall? gameViewSmall = box.SelectedItem as GameViewSmall;
        if (gameViewSmall == null)
            return;
            
        if (_currentSelection != null)
        {
            if (Equals(_currentSelection, gameViewSmall))
                return;
                
            _currentSelection.Deselected();
        }

        _currentSelection = gameViewSmall;
        _currentSelection.Selected();
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
            List<Command> commands = x.GetGlobalCommands();
            
            if (commands.Count > 0)
            {
                MenuItem root = new()
                {
                    Header = x.ServiceName
                };
                List<TemplatedControl> controls = commands.Select(x => x.ToTemplatedControl()).ToList();
                root.Items = controls;
                items.Add(root);
            }
        });

        return items;
    }

    [Command(nameof(DownloadLocationButton))]
    public async void OnDownloadLocationButton()
    {
        OpenFolderDialog dialog = new();
        string? result = await dialog.ShowAsync(Loader.App.GetInstance().MainWindow);
        if (!string.IsNullOrWhiteSpace(result))
        {
            _app.GameDir = result;
            UpdateView();
        }
    }
}