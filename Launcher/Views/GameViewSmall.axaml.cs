using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Launcher.Extensions;
using Launcher.Utils;
using LauncherGamePlugin.Commands;
using LauncherGamePlugin.Extensions;
using LauncherGamePlugin.Forms;
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
    public string GameSize => $"{Game.ReadableSize()} | {Game.Source.ShortServiceName}";

    [Binding(nameof(ButtonPanel), "IsVisible")]
    public bool IsSelected => _isSelected;

    [Binding(nameof(TopPanel), "IsVisible")]
    public bool HasProgress => Game.ProgressStatus != null;
    
    private bool _menuSet = false;
    private bool _isSelected = false;
    private bool _eventSpamPrevention = false;

    public GameViewSmall()
    {
        InitializeComponent();
    }

    public GameViewSmall(IGame game) : this()
    {
        Game = game;
        SetControls();
        OnUpdate();
        game.OnUpdate += OnUpdateWrapper;
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
        
        UpdateView();
        Dispatcher.UIThread.Post(GetCoverImage, DispatcherPriority.Background);

        if (HasProgress)
        {
            Game.ProgressStatus.OnUpdate -= OnProgressUpdateWrapper;
            Game.ProgressStatus.OnUpdate += OnProgressUpdateWrapper;
            
            OnProgressUpdate();
        }
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
            byte[]? img = await Game.CoverImage();

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
    
    // TODO: integrate boot profile switching in this menu
    private void SetMenu()
    {
        List<Command> commands = Game.GetCommands();
        List<Command> functions = commands.Where(x => x.Type == CommandType.Function).ToList();

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
        
        // I love hacky fixes for shit that doesn't work in avalonia
        commands.ForEach(x =>
        {
            if (x.Type == CommandType.Function)
            {
                Action originalAction = x.Action;
                x.Action = () =>
                {
                    if (!_eventSpamPrevention)
                        originalAction?.Invoke();

                    _eventSpamPrevention = !_eventSpamPrevention;
                    Menu.Close();
                };
            }
        });
        MoreMenu.Items = commands.Select(x => x.ToTemplatedControl());
        Menu.Close();
    }

    public void Destroy()
    {
        Game.OnUpdate -= OnUpdate;
        if (Game.ProgressStatus != null)
            Game.ProgressStatus.OnUpdate -= OnProgressUpdate;
    }
}