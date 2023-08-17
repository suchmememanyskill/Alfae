using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Skia;
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

    [Binding(nameof(TopPanel), "IsVisible")]
    public bool HasProgress => Game.ProgressStatus != null;
    
    private bool _downloadedImage = false;
    private Loader.App _app;
    private ContextMenu _contextMenu = new();
    private int _clickCount = 0;

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

        _contextMenu.Opening += ((sender, args) =>
        {
            _contextMenu.ItemsSource = GetCommands().Select(x => x.ToTemplatedControl()).ToList();
        });

        Control.ContextMenu = _contextMenu;

        OnUpdateView += () =>
        {
            if (_downloadedImage)
                SetIcons();
        };
        
        PointerPressed += (sender, args) =>
        {
            _clickCount = args.ClickCount;
        };

        PointerReleased += (sender, args) =>
        {
            if (_clickCount == 2 && args.InitialPressMouseButton == MouseButton.Left && PlayButton.IsVisible && EmptySpace.Bounds.Contains(args.GetPosition(EmptySpace)))
            {
                PlayButton.Command?.Execute(null);
            }
        };
    }

    public void Selected()
    {
        UpdateView();
    }

    public void Deselected()
    {
        UpdateView();
    }

    private void OnUpdateWrapper() => Dispatcher.UIThread.Post(OnUpdate);
    private void OnUpdate()
    {
        if (!TopPanel.IsVisible && HasProgress)
            Loader.App.GetInstance().MainView.SetNewSelection(this);
        
        UpdateView();

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
        SetIcons();
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
        
        SetIcons();
    }

    public async void GetCoverImage()
    {
        try
        {
            byte[]? img = (Game.HasImage(ImageType.VerticalCover)) ? await Game.GetImage(ImageType.VerticalCover) : null;

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

    public void SetIcons()
    {
        List<Command> commands = GetCommands();
        
        MenuFlyout flyout = new MenuFlyout();
        flyout.ItemsSource = commands.Select(x => x.ToTemplatedControl()).ToList();
        MenuButton.Flyout = flyout;

        SetIconButton(PlayButton, "Launch", commands);
        SetIconButton(RunningButton, "Running", commands);
        SetIconButton(InstallButton, "Install", commands);
        SetIconButton(UpdateButton, "Update", commands);
        SetIconButton(PauseButton, "Pause", commands);
        SetIconButton(StopButton, "Stop", commands);
        SetIconButton(SettingsButton, "Config/Info", commands);
        SetIconButton(ContinueButton, "Continue", commands);
    }

    private void SetIconButton(Button button, string target, List<Command> commands)
    {
        Command? found = commands.Find(x => x.Text == target);
        button.IsVisible = found != null;
        button.Command = found != null ? new LambdaCommand(_ => found.Action?.Invoke()) : null;
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
        Debug.WriteLine($"{args.EffectiveViewport.Width} {args.EffectiveViewport.Height}");
        if (args.EffectiveViewport.Width == 0 && args.EffectiveViewport.Height == 0)
            return;

        EffectiveViewportChanged -= EffectiveViewportChangedReact;
        UpdateCoverImage();
    }
}