using System.Globalization;
using LauncherGamePlugin;
using LauncherGamePlugin.Forms;
using LegendaryIntegration.Extensions;

namespace LegendaryIntegration.Service;

public enum LegendaryStatusType
{
    Download,
    Repair,
    Move
}

public class LegendaryDownload : ProgressStatus
{
    public event Action<LegendaryDownload> OnCompletionOrCancel;
    public event Action<LegendaryDownload> OnPauseOrContinue;
    public LegendaryGame Game { get; set; }
    public LegendaryStatusType Type { get; set; }
    public bool Active => _terminal.IsActive;
    private Terminal _terminal = new(LegendaryGameSource.Source.App);
    private string _path;
    private string _line1 = "";
    private string _line2 = "";

    public override string Line1 => _line1;
    public override string Line2 => _line2;

    public LegendaryDownload(LegendaryGame game, LegendaryStatusType type, string? path = null)
    {
        Game = game;
        Type = type;
        if (Game.Download != null)
            throw new Exception("Game already has a download active");

        if (type == LegendaryStatusType.Download)
        {
            if (Game.IsInstalled && !Game.UpdateAvailable)
                throw new Exception("Game is installed and up to date?");

            _path = Path.Join(LegendaryGameSource.Source.App.GameDir, "legendary", Game.InternalName);
            if (!Directory.Exists(_path))
                Directory.CreateDirectory(_path);
        }

        if (type == LegendaryStatusType.Move)
        {
            _path = path ?? throw new Exception("Path is null");
        }
        
        _terminal.OnNewErrLine += DownloadTracker;
        _terminal.OnNewLine += VerifyTracker;
    }
    
    public void DownloadTracker(Terminal t, string last)
    {
        if (last.StartsWith("[cli] INFO: Download size: "))
            _line1 = $"Download: {last.Substring(27).Split('(')[0].Trim()}";

        else if (last.StartsWith("[DLManager] INFO: = Progress: "))
        {
            last = last.Substring(30);
            string[] temp = last.Split('%');
            double a = 0;
            double.TryParse(temp[0].Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out a);
            Percentage = a;
            temp = last.Split(',');
            _line2 = $"Remaining: {temp[2].Substring(6)}";
        }
        else return;

        InvokeOnUpdate();
    }

    public void VerifyTracker(Terminal t, string last)
    {
        if (last.StartsWith("Verification progress:"))
        {
            string sub = last.Substring(23);
            string[] split = sub.Split(' ');
            _line1 = $"Verifying: {split[0]} files";
            string percentageNumber = split[1].Substring(1, split[1].Length - 3);
            double a = 0;
            double.TryParse(percentageNumber.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out a);
            Percentage = a;
            _line2 = $"@ {split[2].Substring(1)} MiB/s";
        }
        else return;
        
        InvokeOnUpdate();
    }

    private bool _ignoreExceptionHack;
    public async void Start()
    {
        InvokeOnUpdate();
        _ignoreExceptionHack = true;
        try
        {
            Game.Parser.PauseAllDownloads();
        }
        catch
        {
            return;
        }
        
        _ignoreExceptionHack = false;
        OnPauseOrContinue?.Invoke(this);
        
        if (Type == LegendaryStatusType.Move)
        {
            try
            {
                _line1 = "Moving...";
                await Utils.MoveDirectoryAsync(Game.InstallPath, _path, new Progress<float>(x =>
                {
                    Percentage = x * 100;
                    InvokeOnUpdate();
                }));
                await _terminal.ExecLegendary($"move {Game.InternalName} \"{_path}\" --skip-move");
                OnCompletionOrCancel?.Invoke(this);
            }
            catch (Exception e)
            {
                /* I'm not sure if deleting here is a good idea
                try
                {
                    Directory.Delete(Path.Join(_path, Game.InternalName), true);
                }
                catch {}
                */

                LegendaryGameSource.Source.App.ShowDismissibleTextPrompt($"Epic Games move failed: {e.Message}");
                OnCompletionOrCancel?.Invoke(this);
            }
            
            return;
        }
        
        
        if (Type == LegendaryStatusType.Repair)
            await _terminal.ExecLegendary($"-y repair {Game.InternalName}");
        else
            await _terminal.ExecLegendary($"-y install {Game.InternalName} --skip-sdl --game-folder \"{_path}\"");
        if (!_terminal.Killed)
            OnCompletionOrCancel?.Invoke(this);
    }

    public void Pause()
    {
        if (Type == LegendaryStatusType.Move && !_ignoreExceptionHack)
            throw new Exception("Cannot pause a move");
        
        if (Active)
            _terminal.Kill();

        OnPauseOrContinue?.Invoke(this);
    }
    
    public async void Stop()
    {
        if (Type == LegendaryStatusType.Move && !_ignoreExceptionHack)
            throw new Exception("Cannot stop a move");
        
        Pause();
        OnCompletionOrCancel?.Invoke(this);
        if (!Game.IsInstalled)
            await Task.Run(() => Directory.Delete(_path, true));
    }
}