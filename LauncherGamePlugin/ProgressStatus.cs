namespace LauncherGamePlugin;

public class ProgressStatus
{
    public string Line1 { get; set; }
    public string Line2 { get; set; }
    public double Percentage { get; set; }

    public event Action OnUpdate;
    public void InvokeOnUpdate() => OnUpdate?.Invoke();
}