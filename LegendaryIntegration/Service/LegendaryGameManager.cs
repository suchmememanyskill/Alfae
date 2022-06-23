using LegendaryIntegration.Model;
using LegendaryMapperV2.Model;
using Newtonsoft.Json;

namespace LegendaryIntegration.Service;

public class LegendaryGameManager
{
    public LegendaryAuth Auth { get; private set; }
    public Config Config { get; private set; } = new();
    private string _configPath;

    public LegendaryGameManager(LegendaryAuth auth)
    {
        Auth = auth;

        _configPath = Path.Join(LegendaryGameSource.Source.App.ConfigDir, "legendary.json");
        if (File.Exists(_configPath))
            Config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(_configPath));
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
            _downloads.ForEach(x => games.Find(y => y.InternalName == x.Game.InternalName)?.ReattachDownload(x));
            return games;
        }

    private List<LegendaryDownload> _downloads = new();

    public void AddDownload(LegendaryDownload download)
    {
        _downloads.ForEach(x => x.Pause());
        _downloads.Add(download);
        download.Start();
        download.OnCompletionOrCancel += RemoveDownload;
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

    public void SaveConfig() => File.WriteAllText(_configPath, JsonConvert.SerializeObject(Config));
}