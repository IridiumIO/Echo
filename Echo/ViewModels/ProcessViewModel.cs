using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Echo.Messages;
using Echo.Models;
using Echo.Services;
using Echo.Views;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace Echo.ViewModels;

public partial class ProcessViewModel : ObservableRecipient
{

    [ObservableProperty]
    private ProcessTrigger _processTrigger;
    private readonly IContentDialogService _contentDialogService;

    [ObservableProperty]
    private bool isEditingName = false;

    public ProcessViewModel(ProcessTrigger processTrigger, IContentDialogService contentDialogService)
    {
        _processTrigger = processTrigger;
        _contentDialogService = contentDialogService;
        _processTrigger.PropertyChanged += (s, e) => WeakReferenceMessenger.Default.Send<ProcessModifiedMessage>();

    }


    [RelayCommand]
    public async Task AddRipple()
    {
        //var filedialog = new Microsoft.Win32.OpenFileDialog
        //{
        //    Filter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*",
        //    Title = "Select a Process to Ripple"
        //};
        //if (filedialog.ShowDialog() == true)
        //{
        //    var ripple = new ProcessRipple(filedialog.FileName, new ProcessTriggerOptions());
        //    _processTrigger.RipplePrograms.Add(ripple);
        //    _processTrigger.RipplePrograms.Add(new ProcessRipple(@"https://google.com", new ProcessTriggerOptions()));
        //}

        var addRippleDialog = new AddRippleDialog(_contentDialogService.GetDialogHost()!);

        ContentDialogResult result = await addRippleDialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {

            if (addRippleDialog.RippleProcessPath == null) return;

            var ripple = new ProcessRipple(addRippleDialog.RippleProcessPath, addRippleDialog.DisplayName, new ProcessTriggerOptions());
            _processTrigger.RipplePrograms.Add(ripple);
        }

        OnPropertyChanged(nameof(ProcessTrigger));
    }


    [RelayCommand]
    public void RemoveRipple(ProcessRipple ripple)
    {
        if (ripple != null && _processTrigger.RipplePrograms.Contains(ripple))
        {
            _processTrigger.RipplePrograms.Remove(ripple);
        }
    }

    [RelayCommand]
    public void RemoveProcessPath(string processPath)
    {
        if (!string.IsNullOrEmpty(processPath) && _processTrigger.TargetProcessPaths.Contains(processPath) && !(_processTrigger.TargetProcessPaths.Count<2))
        {
            _processTrigger.TargetProcessPaths.Remove(processPath);
        }
    }

    [RelayCommand]
    public void AddProcessPath()
    {
        var filedialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*",
            Title = "Select another process to monitor"
        };
        if (filedialog.ShowDialog() == true)
        {
            if (_processTrigger.RipplePrograms.Any(r => r.RippleProcessPath.Equals(filedialog.FileName, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }
            if (!_processTrigger.TargetProcessPaths.Contains(filedialog.FileName))
            {
                _processTrigger.TargetProcessPaths.Add(filedialog.FileName);
            }
        }
        OnPropertyChanged(nameof(ProcessTrigger.TargetProcessPaths));
    }


    [RelayCommand]
    public void DeleteProcess()
    {
        WeakReferenceMessenger.Default.Send<ProcessDeletedMessage>(new ProcessDeletedMessage(this));
    }

}
