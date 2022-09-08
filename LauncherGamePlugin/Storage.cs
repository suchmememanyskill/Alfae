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

        string tempPath = Path.Join(Path.GetTempPath(), "Alfae_Cache");
        if (!Directory.Exists(tempPath))
            Directory.CreateDirectory(tempPath);

        string path = Path.Join(tempPath, filename);
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
}

public class Storage<T> where T : new()
{
    public T Data { get; set; }
    private string _path;
    private IApp _app;

    public Storage(IApp app, string fileName)
    {
        _path = Path.Join(app.ConfigDir, fileName);
        _app = app;
        Data = new();
        Load();
    }

    private void Load()
    {
        if (!File.Exists(_path))
            return;

        Data = JsonConvert.DeserializeObject<T>(File.ReadAllText(_path))!;
    }

    public void Save()
    {
        File.WriteAllText(_path, JsonConvert.SerializeObject(Data));
    }
}