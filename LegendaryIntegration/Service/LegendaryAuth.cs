using LauncherGamePlugin;
using LegendaryIntegration.Extensions;
using LegendaryIntegration.Model;
using Newtonsoft.Json;

namespace LegendaryIntegration.Service;

public class LegendaryAuth
{
    public static string LegendaryPath { get; set; } = "legendary";
    public LegendaryStatusResponse StatusResponse { get; private set; }
    public bool OfflineLogin { get; private set; }
    public bool LegendaryInstalled { get; private set; }

    public async Task<bool> AttemptLogin()
    {
        Terminal t = new Terminal(LegendaryGameSource.Source.App);
        LegendaryInstalled = true;        

        if (await Utils.HasNetworkAsync())
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

    public async Task Authenticate(string authCode)
    {
        Terminal t = new Terminal(LegendaryGameSource.Source.App);
        LegendaryInstalled = true;

        if (!t.NoLog.Contains(authCode))
            t.NoLog.Add(authCode);
        
        if (!await t.ExecLegendary($"auth --code {authCode}"))
        {
            LegendaryInstalled = false;
            throw new Exception("Legendary is not installed");
        }

        if (t.ExitCode != 0)
            throw new Exception("Auth command failed");
    }

    public async Task AuthenticateUsingWebview()
    {
        Terminal t = new Terminal(LegendaryGameSource.Source.App);
        LegendaryInstalled = true;
        
        if (!await t.ExecLegendary($"auth"))
        {
            LegendaryInstalled = false;
            throw new Exception("Legendary is not installed");
        }

        if (t.ExitCode != 0)
            throw new Exception("Auth command failed");

        if (t.StdErr.Contains("[WebViewHelper] ERROR: Login aborted by user."))
            throw new Exception("Login aborted by user");

        if (!t.StdErr.Last().StartsWith("[cli] INFO: Successfully logged in as"))
            throw new Exception("Login failed");
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