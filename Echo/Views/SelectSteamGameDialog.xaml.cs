using CommunityToolkit.Mvvm.ComponentModel;
using Echo.Helpers;
using Gameloop.Vdf;
using Gameloop.Vdf.Linq;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using ValveKeyValue;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace Echo.Views;

[ObservableObject]
public partial class SelectSteamGameDialog : ContentDialog
{

    private DirectoryInfo? _steamInstallDir;

    public static List<SteamGame> SteamGames = new List<SteamGame>();

    [ObservableProperty]
    private SteamGame selectedSteamGame = null!; // Will be set by the view model

    public SelectSteamGameDialog(ContentPresenter contentPresenter) : base(contentPresenter)
    {
        InitializeComponent();
        DataContext = this;
    }


    public bool Initialise()
    {
        if (SteamGames.Count > 0) return true; // Already initialised

        _steamInstallDir = GetSteamInstallDir();
        if (_steamInstallDir == null) return false;

        var AllAppInfos = GetAppInfos(_steamInstallDir);
        if (AllAppInfos == null || AllAppInfos.Count == 0) return false;

        var steamLibraries = GetSteamLibraries(_steamInstallDir);
        if (steamLibraries == null || steamLibraries.Count == 0) return false;

        var steamGames = GetSteamGames(steamLibraries, AllAppInfos);
        if (steamGames.Count == 0) return false;
        steamGames.Sort();

        SteamGames.AddRange(steamGames);
        OnPropertyChanged(nameof(SteamGames));

        return _steamInstallDir != null;   
    }


    private DirectoryInfo? GetSteamInstallDir()
    {

        // 1. Check Steam Registry Key for installation path
        var key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Valve\\Steam") ??
                      RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
                          .OpenSubKey("SOFTWARE\\Valve\\Steam");
        if (key != null)
        {
            string? installPath = key.GetValue("SteamPath") as string;
            if (!string.IsNullOrEmpty(installPath) && Directory.Exists(installPath))
            {
                return new DirectoryInfo(installPath);
            }
        }

        // 2. Check custom Echo registry key for Steam path
        var customKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Iridium\\Echo\\");
        if (customKey != null)
        {
            string? customPath = customKey.GetValue("SteamInstallPath") as string;
            if (!string.IsNullOrEmpty(customPath) && Directory.Exists(customPath))
            {
                if (File.Exists(Path.Combine(customPath, "appcache", "appinfo.vdf")))
                {
                    return new DirectoryInfo(customPath);

                }
            }
        }

        // 3. If no path found, show message box and allow user to browse for steam.exe
        var uiMessageBox = new Wpf.Ui.Controls.MessageBox
        {
            Title = "Unable to automatically locate Steam",
            Content = "Please select the Steam executable (steam.exe) manually.",
            CloseButtonText = "Browse...",
        };
        _ = uiMessageBox.ShowDialogAsync();



        var fileSelectionDialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Select Steam Executable",
            Filter = "Steam Executable|steam.exe",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
        };

     

        if (fileSelectionDialog.ShowDialog() == true)
        {
            var steamExePath = fileSelectionDialog.FileName;
            var steamDir = Path.GetDirectoryName(steamExePath);
            if (steamDir != null && Directory.Exists(steamDir))
            {
                // Save the custom path to the registry for future use
                using (var customKeyToWrite = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Iridium\\Echo\\"))
                {
                    customKeyToWrite?.SetValue("SteamInstallPath", steamDir);
                }
                return new DirectoryInfo(steamDir);
            }
        }

        return null;
    }




    private static Dictionary<uint, KVObject>? GetAppInfos(DirectoryInfo steamLibrary)
    {
        var libraryFoldersFile = Path.Combine(steamLibrary.FullName, "appcache", "appinfo.vdf");
        if (!File.Exists(libraryFoldersFile)) return null;
        
        var stream = File.OpenRead(libraryFoldersFile);

        var appInfo = new AppInfo();
        appInfo.Read(stream);

        var dict = new Dictionary<uint, KVObject>();
        foreach (var app in appInfo.Apps)
        {
            dict[app.AppID] = app.Data;
        }
        return dict;

    }


    private List<SteamLibrary>? GetSteamLibraries(DirectoryInfo steamLibrary)
    {

        var libraryFoldersFile = Path.Combine(steamLibrary.FullName, "steamapps", "libraryfolders.vdf");
        if (!File.Exists(libraryFoldersFile)) return null;

        var steamLibraries = new List<SteamLibrary>();

        var libraryVDF = VdfConvert.Deserialize(File.ReadAllText(libraryFoldersFile));

        foreach (VProperty child in libraryVDF.Value.Children())
        {
            var pathProp = child.Value["path"];
            if (pathProp is null) continue;

            var libPath = pathProp.Value<string>();

            List<uint> appIDs = new List<uint>();

            foreach (VProperty app in child.Value["apps"].Children())
            {
                appIDs.Add(uint.Parse(app.Key));
            }
            if (Directory.Exists(libPath))
            {
                steamLibraries.Add(new SteamLibrary(libPath, appIDs));
            }
        }


        return steamLibraries;

    }


    private List<SteamGame> GetSteamGames(List<SteamLibrary> libraries, Dictionary<uint, KVObject> appinfos)
    {
        var steamGames = new List<SteamGame>();
        foreach (var library in libraries)
        {
            foreach (var appId in library.AppIDs)
            {
                var appManifestPath = Path.Combine(library.Path, "steamapps", $"appmanifest_{appId}.acf");
                if (File.Exists(appManifestPath))
                {
                    var appManifest = VdfConvert.Deserialize(File.ReadAllText(appManifestPath));
                    var name = appManifest.Value["name"]?.Value<string>() ?? "Unknown Game";
                    var installDir = appManifest.Value["installdir"]?.Value<string>();
                    var GameExecutablePath = GetGameExecutablePath(appId, appinfos);
                    if (string.IsNullOrEmpty(installDir) || string.IsNullOrEmpty(GameExecutablePath)) continue;
                    var executablePath = Path.Combine(library.Path, "steamapps", "common", installDir, GameExecutablePath).Replace('/', '\\');

                    if (!File.Exists(executablePath)) continue;


                    steamGames.Add(new SteamGame(appId, name, executablePath));
                }
            }
        }
        return steamGames;
    }


    private string? GetGameExecutablePath(uint appId, Dictionary<uint, KVObject> appinfos)
    {
        var appInfo = appinfos.GetValueOrDefault(appId);
        if (appInfo is null) return null;

        // Find the "config" entry
        var config = appInfo.Children.FirstOrDefault(f => f.Name == "config");
        if (config == null) return null;

        // Find the "launch" entry
        var launch = config.Children.FirstOrDefault(f => f.Name == "launch");
        if (launch == null) return null;

        // Iterate over all launch entries ("0", "1", ...)
        foreach (var launchEntry in launch.Children)
        {
            // Each launchEntry is a KVObject with children
            var configEntry = launchEntry.Children.FirstOrDefault(f => f.Name == "config");
            if (configEntry == null) continue;

            var oslist = configEntry.Children.FirstOrDefault(f => f.Name == "oslist")?.Value.ToString();
            var betaKey = configEntry.Children.FirstOrDefault(f => f.Name == "BetaKey");

            if (oslist == "windows" && betaKey == null)
            {
                // Get the executable path
                var executable = launchEntry.Children.FirstOrDefault(f => f.Name == "executable")?.Value.ToString();
                if (!string.IsNullOrEmpty(executable))
                    return executable;
            }
        }

        return null;

    }



}

public record SteamLibrary(string Path, List<uint> AppIDs);
public record SteamGame(uint AppID, string Name, string ExecutablePath) : IComparable<SteamGame>
{
    public int CompareTo(SteamGame? other)
    {
        if (other is null) return 1;
        return string.Compare(Name, other.Name, StringComparison.OrdinalIgnoreCase);
    }
}