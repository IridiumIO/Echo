using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Echo.Helpers;
using Echo.Messages;
using Echo.Models;
using Echo.Services;
using Echo.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;


//TODO: Rename to Ripple

namespace Echo.ViewModels;

public partial class MainViewModel : ObservableRecipient, IRecipient<ProcessDeletedMessage>, IRecipient<ProcessModifiedMessage>
{
    private readonly ProcessMonitorService _processMonitorService;
    private readonly ProcessLauncherService _processLauncherService;
    private readonly FileSaveService _fileSaveService;
    private readonly IContentDialogService _contentDialogService;

    public ObservableCollection<ProcessViewModel> Triggers { get; } = new();
   
    
    [ObservableProperty] private ProcessViewModel? selectedProcessVM;
    [ObservableProperty] bool isMonitoring;
    [ObservableProperty] bool runOnWindowsStartup = false;
    [ObservableProperty] bool addToStartMenu = false;





    public MainViewModel(ProcessMonitorService processMonitorService, ProcessLauncherService processLauncherService, FileSaveService fileSaveService, IContentDialogService contentDialogService)
    {
        _processMonitorService = processMonitorService;
        _processLauncherService = processLauncherService;
        _fileSaveService = fileSaveService;
        _contentDialogService = contentDialogService;

        var monitoredProcesses = _fileSaveService.LoadProcessTriggersFromDisk();

        foreach (var proc in monitoredProcesses)
        {
            Triggers.Add(new ProcessViewModel(proc, _contentDialogService));
        }

        IsActive = true;

        Triggers.CollectionChanged += (s, e) =>
        {
            if (IsMonitoring)
            {
                _processMonitorService.StopMonitoring();
                _processMonitorService.StartMonitoring(Triggers.Select(vm => vm.ProcessTrigger), OnProcessDetected, OnProcessExited);
            }
            _fileSaveService.SaveProcessTriggersToDisk(Triggers.Select(vm => vm.ProcessTrigger).ToList());
        };

        RunOnWindowsStartup = DoesStartupEntryExist();
        AddToStartMenu = DoesStartMenuEntryExist();

    }

    private bool DoesStartupEntryExist()
    {
        var startupRegKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
        var existingValue = startupRegKey?.GetValue("Echo");
        return existingValue != null;
    }

    private bool DoesStartMenuEntryExist()
    {
        var startMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
        var startupFile = System.IO.Path.Combine(startMenuPath, "Echo.lnk");
        return System.IO.File.Exists(startupFile);
    }

    partial void OnAddToStartMenuChanged(bool oldValue, bool newValue)
    {
        var startMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
        var startupFile = System.IO.Path.Combine(startMenuPath, "Echo.lnk");
        if (newValue)
        {
            var exePath = Environment.ProcessPath;
            ShortcutCreator.CreateShortcut(startupFile, exePath!, "Echo", System.IO.Path.GetDirectoryName(exePath)!, exePath!);
        }
        else
        {
            if (System.IO.File.Exists(startupFile))
            {
                System.IO.File.Delete(startupFile);
            }
        }

    }

    partial void OnRunOnWindowsStartupChanged(bool oldValue, bool newValue)
    {
        var startupRegKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
        var existingValue = startupRegKey?.GetValue("Echo");

        if (newValue)
        {
            if (existingValue == null)
            {
                startupRegKey?.SetValue("Echo", Environment.ProcessPath + " -tray");
            }
        }
        else
        {
            if (existingValue != null)
            {
                startupRegKey?.DeleteValue("Echo", false);
            }
        }


    }


    [RelayCommand]
    public void StartMonitoring()
    {
        var triggerModels = Triggers.Select(vm => vm.ProcessTrigger).ToList();
        _processMonitorService.StartMonitoring(triggerModels, OnProcessDetected, OnProcessExited);
        IsMonitoring = true;
    }


    [RelayCommand]
    private void StopMonitoring()
    {
        _processMonitorService.StopMonitoring();
        IsMonitoring = false;

    }

    private void OnProcessDetected(ProcessTrigger trigger, int processID)
    {

        if (trigger != null)
        {
            _processLauncherService.LaunchProcesses(trigger.RipplePrograms);
        }
    }

    private void OnProcessExited(ProcessTrigger trigger, int processID)
    {

        if (trigger != null)
        {
            foreach (var ripple in trigger.RipplePrograms)
            {
                if (ripple.Process != null && !ripple.Process.HasExited && ripple.ProcessTriggerOptions.TerminateHelpersOnTargetExit)
                {
                    ripple.Process.KillProcessTree();
                }
            }
        }
    }

    [RelayCommand]
    private void AddProgram()
    {
        var fileselect = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Select Program",
            Filter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*",
            Multiselect = false
        };
        if (fileselect.ShowDialog() == true)
        {
            var trigger = new ProcessTrigger(fileselect.FileName);
            Triggers.Add(new ProcessViewModel(trigger, _contentDialogService));

            if (IsMonitoring)
            {
                _processMonitorService.StopMonitoring();
                _processMonitorService.StartMonitoring(Triggers.Select(vm => vm.ProcessTrigger), OnProcessDetected, OnProcessExited);
            }
        }

    }


    [RelayCommand]
    private async Task AddSteamGame(Object content)
    {
        var steamDialog = new SelectSteamGameDialog(_contentDialogService.GetDialogHost()!);
        var isSteamFound = steamDialog.Initialise();

        if (!isSteamFound)
        {
            var uiMessageBox = new Wpf.Ui.Controls.MessageBox
            {
                Title = "Steam Library Unable to be Located",
                Content = "Please use the standard 'Add Program' option instead"
            };
            _ = uiMessageBox.ShowDialogAsync();
            return;
        }

        ContentDialogResult result = await steamDialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {

            if (steamDialog.SelectedSteamGame == null) return;

            var trigger = new ProcessTrigger(steamDialog.SelectedSteamGame.ExecutablePath, steamDialog.SelectedSteamGame.Name);
            Triggers.Add(new ProcessViewModel(trigger, _contentDialogService));
            
            if (IsMonitoring)
            {
                _processMonitorService.StopMonitoring();
                _processMonitorService.StartMonitoring(Triggers.Select(vm => vm.ProcessTrigger), OnProcessDetected, OnProcessExited);
            }

        }

    }

    public void Receive(ProcessDeletedMessage message)
    {
        Triggers.Remove((ProcessViewModel)message.Value);
    }

    public void Receive(ProcessModifiedMessage message)
    {
        _fileSaveService.SaveProcessTriggersToDisk(Triggers.Select(vm => vm.ProcessTrigger).ToList());
    }


    [RelayCommand]
    public void NotifyIconOpen()
    {
        var mainWindow = Application.Current.MainWindow;
        if (mainWindow != null)
        {
            mainWindow.Show();
            mainWindow.Activate();
            mainWindow.WindowState = WindowState.Normal;
            mainWindow.BringIntoView();
            mainWindow.Focus();
        }
    }

    [RelayCommand]
    public void NotifyIconExit()
    {
        Application.Current.Shutdown();
    }

}
