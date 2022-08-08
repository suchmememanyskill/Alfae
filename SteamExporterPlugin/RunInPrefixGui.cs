using LauncherGamePlugin.Forms;
using LauncherGamePlugin.Interfaces;
using LauncherGamePlugin.Launcher;

namespace SteamExporterPlugin;

public class RunInPrefixGui
{
    public ProtonWrapper Proton { get; private set; }
    public IGame Game { get; private set; }

    public RunInPrefixGui(ProtonWrapper proton, IGame game)
    {
        Proton = proton;
        Game = game;
    }

    public void Show(IApp app, string errMessage = "")
    {
        var items = new List<FormEntry>()
        {
            Form.TextBox("Run executable in prefix", fontWeight: "Bold"),
            Form.FilePicker("Executable"),
            Form.Button("Close", _ => app.HideForm(), "Run", x =>
            {
                string path = x.GetValue("Executable")!;
                if (!File.Exists(path))
                {
                    Show(app, "Invalid path!");
                    return;
                }

                LaunchParams launch = new(path, "", Path.GetDirectoryName(path)!, Game);
                launch.ForceBootProfile = Proton;
                app.HideForm();
                app.Launch(launch);
            })
        };
        
        if (errMessage != "")
            items.Add(Form.TextBox(errMessage, FormAlignment.Center));
        
        app.ShowForm(items);
    }
}