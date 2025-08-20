using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Echo.Helpers;
using Echo.Messages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Echo.Models;


[ObservableRecipient]
public partial class ProcessTrigger : ObservableValidator
{

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
    [MinLength(2)]
    string displayName;

    [ObservableProperty] ObservableCollection<string> targetProcessPaths = new();
    [ObservableProperty] ObservableCollection<ProcessRipple> ripplePrograms = new();

    public ProcessTrigger() { 
        TargetProcessPaths.CollectionChanged += (s, e) => WeakReferenceMessenger.Default.Send<ProcessModifiedMessage>();
        RipplePrograms.CollectionChanged += (s, e) => WeakReferenceMessenger.Default.Send<ProcessModifiedMessage>();
    }

    public ProcessTrigger(string targetProcessPath, string targetName) : this()
    {
        TargetProcessPaths.Add(targetProcessPath);
        DisplayName = targetName;
    }


    public ProcessTrigger(string targetProcessPath) : this(targetProcessPath, Path.GetFileNameWithoutExtension(targetProcessPath)) { }


   
    public ProcessTrigger(string targetProcessPath, List<ProcessRipple>? ripplePrograms): this(targetProcessPath, Path.GetFileNameWithoutExtension(targetProcessPath))
    {
        
        foreach (var ripple in ripplePrograms ?? new List<ProcessRipple>())
        {
            RipplePrograms.Add(ripple);
        }
       
    }


}


public partial class ProcessTriggerOptions : ObservableObject
{
    [ObservableProperty] private bool enabled = true;

    [ObservableProperty] private bool launchHidden = false; // Start helpers minimised (screw hidden, you lose so many programs)
    [ObservableProperty] private bool allowMultiple  = false; // Only launch helpers if they are not already running
    [ObservableProperty] private bool terminateHelpersOnTargetExit  = true; // Terminate helpers when the target process exits
    [ObservableProperty] private string arguments = string.Empty;

    //public TimeSpan LaunchDelay { get; set; } = TimeSpan.Zero; 
    //public bool RunAsAdministrator { get; set; } = false; 
    //public bool MonitorProcessExit { get; set; } = false; // Optionally launch helpers when the target process exits
    //public List<string> Arguments { get; set; } = new(); // Custom arguments 
    //public bool NotifyOnLaunch { get; set; } = false; // Show notification when helpers are launched
    //public bool RequireUserConfirmation { get; set; } = false; // Ask user before launching

}