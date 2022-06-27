using LauncherGamePlugin;
using LegendaryIntegration.Extensions;
using LegendaryIntegration.Model;
using Newtonsoft.Json;

namespace LegendaryIntegration.Service;

public class LegendaryAuth
{
    public static bool HasNetwork()
    {
        try
        {
            using (var client = new HttpClient())
            {
                client.GetAsync(new Uri("http://www.google.com")).GetAwaiter().GetResult()
                    .EnsureSuccessStatusCode();
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    public static string LegendaryPath { get; set; } = "legendary";
    public LegendaryStatusResponse StatusResponse { get; private set; }
    public bool OfflineLogin { get; private set; }
    public bool LegendaryInstalled { get; private set; }

    public async Task<bool> AttemptLogin()
    {
        Terminal t = new Terminal(LegendaryGameSource.Source.App);
        LegendaryInstalled = true;        

        if (HasNetwork())
        {
            if (!(await t.ExecLegendary("status --json")))
            {
                LegendaryInstalled = false;
                return false;
            }

            if (t.ExitCode == 0)
            {
                StatusResponse = JsonConvert.DeserializeObject<LegendaryStatusResponse>(t.StdOut[0]);
                OfflineLogin = false;

                if (StatusResponse.IsLoggedIn())
                    return true;
            }
        }

        if (!await t.ExecLegendary("status --json --offline"))
        {
            LegendaryInstalled = false;
            return false;
        }

        if (t.ExitCode == 0)
        {
            StatusResponse = JsonConvert.DeserializeObject<LegendaryStatusResponse>(t.StdOut[0]);
            OfflineLogin = true;

            if (StatusResponse.IsLoggedIn())
                return true;
        }

        return false;
    }

    public async Task<bool> Authenticate(string sid)
    {
        Terminal t = new Terminal(LegendaryGameSource.Source.App);
        LegendaryInstalled = true;

        if (!await t.ExecLegendary($"auth --sid {sid}"))
        {
            LegendaryInstalled = false;
            return false;
        }

        return t.ExitCode == 0;
    }

    public async Task<bool> Logout()
    {
        Terminal t = new Terminal(LegendaryGameSource.Source.App);
        LegendaryInstalled = true;

        if (!await t.ExecLegendary($"auth --delete"))
        {
            LegendaryInstalled = false;
            return false;
        }

        return t.ExitCode == 0;
    }
}