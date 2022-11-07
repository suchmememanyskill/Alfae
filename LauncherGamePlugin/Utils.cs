using System.Diagnostics;
using System.Runtime.InteropServices;
using LauncherGamePlugin.Enums;

namespace LauncherGamePlugin;

public static class Utils
{
    public static void OpenUrl(string url)
    {
        // hack because of this: https://github.com/dotnet/corefx/issues/10361
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            url = url.Replace("&", "^&");
            Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Process.Start("xdg-open", url);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Process.Start("open", url);
        }
        else throw new Exception("No url 4 u");
    }

    public static void OpenFolder(string path)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            Process.Start("explorer.exe", "\"" + path.Replace("/", "\\") + "\""); // I love windows hacks
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            Process.Start("xdg-open", $"\"{path}\"");
    }

    public static void OpenFolderWithHighlightedFile(string path)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            Process.Start("explorer.exe", "/select,\"" + path.Replace("/", "\\") + "\""); // I love windows hacks

        else OpenFolder(path);
    }
    
    // Thanks stackoverflow https://stackoverflow.com/questions/468119/whats-the-best-way-to-calculate-the-size-of-a-directory-in-net
    public static long DirSize(DirectoryInfo d) 
    {    
        long size = 0;    
        // Add file sizes.
        FileInfo[] fis = d.GetFiles();
        foreach (FileInfo fi in fis) 
        {      
            size += fi.Length;    
        }
        // Add subdirectory sizes.
        DirectoryInfo[] dis = d.GetDirectories();
        foreach (DirectoryInfo di in dis) 
        {
            size += DirSize(di);   
        }
        return size;  
    }

    public record DirRepresentation(Dictionary<string, FileInfo> Files, List<string> Folders);
    public static DirRepresentation GetDirRepresentation(string src) => GetDirRepresentation(new DirectoryInfo(src));

    public static DirRepresentation GetDirRepresentation(DirectoryInfo info)
    {
        Dictionary<string, FileInfo> files = new();
        List<string> folders = new();
        folders.Add(info.FullName);
        
        foreach (DirectoryInfo di in info.GetDirectories())
        {
            folders.Add(di.FullName);
            
            DirRepresentation subRepresentation = GetDirRepresentation(di);
            
            foreach (var file in subRepresentation.Files)
                files.Add(file.Key, file.Value);
            
            folders.AddRange(subRepresentation.Folders);
        }

        foreach (var file in info.GetFiles())
            files.Add(file.FullName, file);

        return new DirRepresentation(files, folders);
    }

    public static async Task MoveDirectoryAsync(string src, string dstDir, IProgress<float>? progress = null)
    {
        try
        {
            Directory.Move(src, Path.Join(dstDir, Path.GetFileName(src)));
            progress?.Report(1);
        }
        catch (IOException e)
        {
            if (e.Message != "Source and destination path must have identical roots. Move will not work across volumes.")
                throw;

            await CopyDirectoryAsync(src, dstDir, progress);
            Directory.Delete(src, true);
        }
    }

    public static async Task CopyDirectoryAsync(string src, string dstDir, IProgress<float>? progress = null)
    {
        progress?.Report(0);
        DirRepresentation representation = GetDirRepresentation(src);
        
        // Create folders
        string srcRoot = Path.GetDirectoryName(src)!;
        foreach (string srcPath in representation.Folders)
        {
            string relPath = Path.GetRelativePath(srcRoot, srcPath);
            string path = Path.Join(dstDir, relPath);
            Directory.CreateDirectory(path);
        }

        long totalSize = 0;
        long currentSize = 0;

        // Calculate total size
        foreach (var file in representation.Files)
            totalSize += file.Value.Length;

        // Copy files
        foreach (var file in representation.Files)
        {
            string relPath = Path.GetRelativePath(srcRoot, file.Key);
            string path = Path.Join(dstDir, relPath);
            await CopyFileAsync(file.Key, path);
            currentSize += file.Value.Length;
            progress?.Report((float)currentSize/totalSize);
        }
        
        progress?.Report(1);
    }

    public static async Task CopyFileAsync(string srcPath, string dstPath)
    {
        using Stream src = File.Open(srcPath, FileMode.Open);
        using Stream dst = File.Create(dstPath);
        await src.CopyToAsync(dst);
    }
    
    public static string GetExecutablePath()
    {
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            return Path.Join(AppContext.BaseDirectory, $"{AppDomain.CurrentDomain.FriendlyName}.exe");
        else
            return Path.Join(AppContext.BaseDirectory, AppDomain.CurrentDomain.FriendlyName);
    }

    public static bool HasNetwork() => HasNetworkAsync().GetAwaiter().GetResult();
    
    public static async Task<bool> HasNetworkAsync()
    {
        try
        {
            using var client = new HttpClient();
            (await client.GetAsync(new Uri("http://www.google.com")))
                .EnsureSuccessStatusCode();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static string? WhereSearch(string filename)
    {
        if (PlatformExtensions.CurrentPlatform == Platform.Windows)
        {
            var paths = new[]{ Environment.CurrentDirectory }
                .Concat(Environment.GetEnvironmentVariable("PATH")!.Split(';'));
            var extensions = new[]{ String.Empty }
                .Concat(Environment.GetEnvironmentVariable("PATHEXT")!.Split(';')
                    .Where(e => e.StartsWith(".")));
            var combinations = paths.SelectMany(x => extensions,
                (path, extension) => Path.Combine(path, filename + extension));
            return combinations.FirstOrDefault(File.Exists);
        }
        else
        {
            var paths = new[]{ Environment.CurrentDirectory }
                .Concat(Environment.GetEnvironmentVariable("PATH")!.Split(':'));
            var combinations = paths.Select(x => Path.Combine(x, filename));
            return combinations.FirstOrDefault(File.Exists);
        }
    }
}