using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Launcher.Configuration;
using Launcher.Extensions;
using Launcher.Launcher;
using Launcher.Utils;
using LauncherGamePlugin.Commands;
using LauncherGamePlugin.Enums;
using LauncherGamePlugin.Extensions;
using LauncherGamePlugin.Interfaces;

namespace Launcher.Views;

public partial class GameViewSmall : UserControlExt<GameViewSmall>
{
    public IGame Game { get; private set; }

    [Binding(nameof(BottomPanel), "Background")]
    [Binding(nameof(TopPanel), "Background")]
    public IBrush HalfTransparency => new SolidColorBrush(new Color(128, 0, 0, 0));

    [Binding(nameof(GameLabel), "Content")]
    public string GameName => Game.Name;

    [Binding(nameof(SizeLabel), "Content")]
    public string GameSize => (Game.Size == 0) ? Game.Source.ShortServiceName : $"{Game.ReadableSize()} | {Game.Source.ShortServiceName}";

    [Binding(nameof(ButtonPanel), "IsVisible")]
    public bool IsSelected => _isSelected;

    [Binding(nameof(TopPanel), "IsVisible")]
    public bool HasProgress => Game.ProgressStatus != null;
    
    private bool _isSelected = false;
    private bool _downloadedImage = false;
    private Loader.App _app;
    private ContextMenu _contextMenu = new();

    public GameViewSmall()
    {
        InitializeComponent();
    }

    public GameViewSmall(IGame game, Loader.App app) : this()
    {
        _app = app;
        Game = game;
        SetControls();
        OnUpdate();
        game.OnUpdate += OnUpdateWrapper;
        EffectiveViewportChanged += EffectiveViewportChangedReact;

        _contextMenu.ContextMenuOpening += ((sender, args) =>
        {
            _contextMenu.Items = GetCommands().Select(x => x.ToTemplatedControl()).ToList();
        });

        Control.ContextMenu = _contextMenu;
    }

    public void Selected()
    {
        _isSelected = true;
        UpdateView();
        SetMenu();
    }

    public void Deselected()
    {
        _isSelected = false;
        UpdateView();
    }

    private void OnUpdateWrapper() => Dispatcher.UIThread.Post(OnUpdate);
    private void OnUpdate()
    {
        if (_isSelected)
            SetMenu();
        
        if (!TopPanel.IsVisible && HasProgress)
            Loader.App.GetInstance().MainView.SetNewSelection(this);
        
        UpdateView();
        _downloadedImage = false;

        if (HasProgress)
        {
            Game.ProgressStatus.OnUpdate -= OnProgressUpdateWrapper;
            Game.ProgressStatus.OnUpdate += OnProgressUpdateWrapper;
            
            OnProgressUpdate();
        }
    }
    
    public void UpdateCoverImage()
    {
        if (_downloadedImage)
            return;

        _downloadedImage = true;
        Dispatcher.UIThread.Post(GetCoverImage, DispatcherPriority.Background);
    }

    private void OnProgressUpdateWrapper() => Dispatcher.UIThread.Post(OnProgressUpdate);
    private void OnProgressUpdate()
    {
        if (Game.ProgressStatus == null)
            return;
        
        ProgressBar.Value = Game.ProgressStatus.Percentage;
        TopLabel1.IsVisible = !string.IsNullOrWhiteSpace(Game.ProgressStatus.Line1);
        if (TopLabel1.IsVisible)
            TopLabel1.Content = Game.ProgressStatus.Line1;
        
        TopLabel2.IsVisible = !string.IsNullOrWhiteSpace(Game.ProgressStatus.Line2);
        if (TopLabel2.IsVisible)
            TopLabel2.Content = Game.ProgressStatus.Line2;
        
        SetMenu();
    }

    public async void GetCoverImage()
    {
        try
        {
            byte[]? img = (Game.HasCoverImage) ? await Game.CoverImage() : null;

            if (img == null)
                throw new InvalidDataException();

            Stream stream = new MemoryStream(img);
            CoverImage.Source = Bitmap.DecodeToHeight(stream, 300);
        }
        catch
        {
            Loader.App.GetInstance().Logger.Log($"Failed to get cover of {Game.Name}");
        }
    }
    
    private void SetMenu()
    {
        List<Command> commands = GetCommands();
        List<Command> functions = commands.Where(x => x.Type == CommandType.Function && x.Text != "Set Boot Profile").ToList();

        PrimaryButton.IsVisible = false;
        SecondaryButton.IsVisible = false;

        if (functions.Count >= 1)
        {
            PrimaryButton.IsVisible = true;
            Action actionOne = functions[0].Action;
            PrimaryButton.Command = new LambdaCommand(x => actionOne());
            PrimaryButtonLabel.Content = functions[0].Text;
        }
        
        Menu.IsVisible = !(commands.Count == functions.Count && commands.Count <= 2);

        if (functions.Count >= 2 && !Menu.IsVisible)
        {
            SecondaryButton.IsVisible = true;
            Action actionTwo = functions[1].Action;
            SecondaryButton.Command = new LambdaCommand(x => actionTwo());
            SecondaryButtonLabel.Content = functions[1].Text;
        }

        commands.ForEach(x =>
        {
            if (x.Type == CommandType.Function)
            {
                Action originalAction = x.Action;
                x.Action = () =>
                {
                    originalAction?.Invoke();
                    Menu.Close();
                };
            }
        });
        MoreMenu.Items = commands.Select(x => x.ToTemplatedControl());
        Menu.Close();
    }
    
    public List<Command> GetCommands()
    {
        List<Command> commands = _app.Middleware.GetGameCommands(Game.Original, Game.Source);
        
        if (Game.InstalledStatus == InstalledStatus.Installed && Game.EstimatedGamePlatform != Platform.None)
        {
            commands.Add(new Command());
            commands.Add(new("Set Boot Profile", () => new BootProfileSelectGUI(Loader.App.GetInstance(), Game).ShowGUI()));
            
            GameConfig? config = _app.Config.GetGameConfigOptional(Game);
            if (config != null)
            {
                GameSession total = config.GetTotalTime();
                if (total.TimeSpent.TotalSeconds > 0)
                {
                    string time = total.TimeSpent.ToString(@"hh\:mm");
                    commands.Add(new($"Played for {time}"));
                }
            }
        }

        return commands;
    }

    public void Destroy()
    {
        Game.OnUpdate -= OnUpdate;
        EffectiveViewportChanged -= EffectiveViewportChangedReact;
        if (Game.ProgressStatus != null)
            Game.ProgressStatus.OnUpdate -= OnProgressUpdate;
    }

    public void SetVisibility(bool visible)
    {
        IsVisible = visible;
    }

    private void EffectiveViewportChangedReact(object? obj, EffectiveViewportChangedEventArgs args)
    {
        if (args.EffectiveViewport.IsEmpty)
            return;
        
        EffectiveViewportChanged -= EffectiveViewportChangedReact;
        UpdateCoverImage();
    }
}