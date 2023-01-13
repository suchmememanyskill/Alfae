using LauncherGamePlugin.Enums;
using LauncherGamePlugin.Interfaces;
using Newtonsoft.Json;

namespace LauncherGamePlugin;

public static class Storage
{
    public async static Task<byte[]?> Cache(string filename, Func<Task<byte[]?>> data)
    {
        if (string.IsNullOrWhiteSpace(filename))
            return null;
        
        foreach (char x in Path.GetInvalidFileNameChars())
        {
            if (filename.Contains(x))
                throw new Exception("Invalid file name detected!");
        }

        string path = Path.Join(GetCachePath(), filename);
        if (File.Exists(path))
            return await File.ReadAllBytesAsync(path);

        byte[]? generatedData = await data();

        if (generatedData == null)
            return null;
        
        await File.WriteAllBytesAsync(path, generatedData);
        return generatedData;
    }

    public static Task<byte[]?> ImageDownload(string? url) => ImageDownload((url == null) ? null : new Uri(url));
    public static async Task<byte[]?> ImageDownload(Uri? uri)
    {
        if (uri == null)
            return null;
        
        using HttpClient client = new();
        try
        {
            HttpResponseMessage response = await client.GetAsync(uri);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsByteArrayAsync();
        }
        catch
        {
            return null;
        }
    }

    public static string GetCachePath()
    {
        string path;
        if (PlatformExtensions.CurrentPlatform == Platform.Windows)
        {
            path = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache), "Alfae");
        }
        else if (PlatformExtensions.CurrentPlatform == Platform.Linux)
        {
            string? cacheDir = Environment.GetEnvironmentVariable("XDG_CACHE_HOME");
            cacheDir ??= Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cache");
            path = Path.Join(cacheDir, "Alfae");
        }
        else
        {
            throw new PlatformNotSupportedException();
        }

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        return path;
    }
}

public class Storage<T> where T : new()
{
    public T Data { get; set; }
    public string Path { get; private set; }
    private IApp _app;

    public Storage(IApp app, string fileName)
    {
        Path = System.IO.Path.Join(app.ConfigDir, fileName);
        _app = app;
        Data = new();
        Load();
    }

    private void Load()
    {
        if (!File.Exists(Path))
            return;

        Data = JsonConvert.DeserializeObject<T>(File.ReadAllText(Path))!;
    }

    public void Save()
    {
        File.WriteAllText(Path, JsonConvert.SerializeObject(Data));
    }
}