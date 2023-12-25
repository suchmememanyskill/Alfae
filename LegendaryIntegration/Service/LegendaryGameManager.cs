using LauncherGamePlugin;
using LauncherGamePlugin.Interfaces;
using LegendaryIntegration.Model;
using Newtonsoft.Json;

namespace LegendaryIntegration.Service;

public class LegendaryGameManager
{
    public LegendaryAuth Auth { get; private set; }
    public Config Config => _storage.Data;
    private readonly Storage<Config> _storage;
    public int LastGameCount { get; private set; }

    public LegendaryGameManager(LegendaryAuth auth, IApp app)
    {
        Auth = auth;
        _storage = new(app, "legendary.json");
    }
    
    public async Task<List<LegendaryGame>> GetGames()
        {
            string configDir = Auth.StatusResponse.ConfigDirectory;
            List<string> files = Directory.GetFiles(Path.Combine(configDir, "metadata")).ToList();
            List<string> fileContents = (await Task.WhenAll(files.Select(x => File.ReadAllTextAsync(x)))).ToList();
            List<LegendaryGame> games = fileContents.Select(x => new LegendaryGame(JsonConvert.DeserializeObject<GameMetadata>(x), this)).ToList();

            if (File.Exists(Path.Combine(configDir, "installed.json")))
            {
                InstalledGameList list = new();

                list.Games = JsonConvert.DeserializeObject<Dictionary<string, InstalledGame>>(await File.ReadAllTextAsync(Path.Combine(configDir, "installed.json")));

                list.GetGamesAsList()
                    .ForEach(x => games.Find(y => y.Metadata.AppName == x.AppName)?.SetInstalledData(x));
            }

            // Filter out dlc
            List<LegendaryGame> dlc = new();
            games.ForEach(currentGame =>
            {
                if (currentGame.Metadata != null && currentGame.Metadata.Metadata != null && currentGame.Metadata.Metadata.DlcItemList != null)
                {
                    List<LegendaryGame> currentGameDlc = games.Where(possibleDlc =>
                        currentGame.Metadata.Metadata.DlcItemList.Any(currentGameDlc =>
                        {
                            if (currentGameDlc.ReleaseDetails == null)
                                return false;

                            return currentGameDlc.ReleaseDetails.Any(currentGameDlcRelease =>
                                currentGameDlcRelease.AppId == possibleDlc.InternalName
                            );
                        })
                    ).ToList();
                    currentGame.Dlc.AddRange(currentGameDlc);
                    currentGameDlc.ForEach(x => x.IsDlc = true);
                    dlc.AddRange(currentGameDlc);
                }
            });

            games.RemoveAll(x => dlc.Contains(x));

            // Filter out UE stuff
            games.RemoveAll(x =>
            {
                if (x.Metadata != null && x.Metadata.Metadata != null && x.Metadata.Metadata.Categories != null)
                    return !x.Metadata.Metadata.Categories.Any(y => y["path"] == "games");

                return false;
            });

            // Filter out not installed games when offline
            if (Auth.OfflineLogin)
                games.RemoveAll(x => !x.IsInstalled);
            
            games = games.OrderBy(x => x.Name).ToList();
            LastGameCount = games.Count;
            _downloads.ForEach(x => games.Find(y => y.InternalName == x.Game.InternalName)?.ReattachDownload(x));
            return games;
        }

    private List<LegendaryDownload> _downloads = new();

    public void AddDownload(LegendaryDownload download)
    {
        _downloads.Add(download);
        download.OnCompletionOrCancel += RemoveDownload;
        download.Start();
    }

    public void RemoveDownload(LegendaryDownload download)
    {
        _downloads.Remove(download);
        if (!_downloads.Any(x => x.Active) && _downloads.Count > 0)
            _downloads.First().Start();
    }

    public void PauseAllDownloads() => _downloads.ForEach(x => x.Pause());
    
    public void StopAllDownloads()
    {
        _downloads.ForEach(x => x.Stop());
        _downloads = new();
    }

    public void SaveConfig() => _storage.Save();
}