using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Launcher.Extensions;
using LauncherGamePlugin.Extensions;
using LauncherGamePlugin.Interfaces;

namespace Launcher.Views;

public partial class GameViewSmall : UserControlExt<GameViewSmall>
{
    private IGame _game;

    [Binding(nameof(BottomPanel), "Background")]
    public IBrush HalfTransparency => new SolidColorBrush(new Color(128, 0, 0, 0));

    [Binding(nameof(GameLabel), "Content")]
    public string GameName => _game.Name;

    [Binding(nameof(SizeLabel), "Content")]
    public string GameSize => _game.ReadableSize();
    
    public GameViewSmall()
    {
        InitializeComponent();
    }

    public GameViewSmall(IGame game) : this()
    {
        _game = game;
        SetControls();
        UpdateView();
    }
}