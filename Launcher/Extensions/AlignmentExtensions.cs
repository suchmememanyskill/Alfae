using Avalonia.Layout;
using LauncherGamePlugin.Forms;

namespace Launcher.Extensions;

public static class AlignmentExtensions
{
    public static HorizontalAlignment ToAvaloniaAlignment(this FormAlignment a)
    {
        switch (a)
        {
            case FormAlignment.Center:
                return HorizontalAlignment.Center;
            case FormAlignment.Right:
                return HorizontalAlignment.Right;
            default:
                return HorizontalAlignment.Left;
        }
    }
}