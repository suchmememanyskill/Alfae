namespace LauncherGamePlugin;

public class ProgressStatus
{
    public virtual string Line1 { get; set; }
    public virtual string Line2 { get; set; }
    public virtual double Percentage { get; set; }

    public event Action OnUpdate;
    public void InvokeOnUpdate() => OnUpdate?.Invoke();
}