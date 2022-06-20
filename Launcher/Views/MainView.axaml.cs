using System.Collections.Generic;
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
    
    public MainView()
    {
        InitializeComponent();
        SetControls();
        UpdateView();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
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
                List<TemplatedControl> rootItems = new();
                
                x.GlobalCommands.ForEach(y =>
                {
                    if (y.IsSeperatorCommand())
                    {
                        rootItems.Add(new Separator());
                        return;
                    }
                    
                    MenuItem item = new MenuItem();
                    
                    if (y.IsActionCommand())
                    {
                        ActionCommand command = y.GetActionCommand()!;
                        item.Command = new LambdaCommand(_ => command.Action());
                    }
                    else
                    {
                        item.IsEnabled = false;
                    }

                    item.Header = y.Text;
                    rootItems.Add(item);
                });

                root.Items = rootItems;
                items.Add(root);
            }
        });

        return items;
    }
}