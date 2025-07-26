using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Echo.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Echo.Models;

public partial class ProcessRipple : ObservableObject
{

    [ObservableProperty] string rippleProcessName;
    [ObservableProperty] string rippleProcessPath;
    [ObservableProperty] ProcessTriggerOptions processTriggerOptions = new();

    public Process Process;

    public ProcessRipple() { }

    public ProcessRipple(string rippleProcessPath, ProcessTriggerOptions processTriggerOptions) : this(rippleProcessPath, null, processTriggerOptions) { }

    public ProcessRipple(string rippleProcessPath, string? rippleProcessName, ProcessTriggerOptions processTriggerOptions) 
    {
        RippleProcessName = rippleProcessName ?? Path.GetFileName(rippleProcessPath);
        RippleProcessPath = rippleProcessPath;
        ProcessTriggerOptions = processTriggerOptions;
        ProcessTriggerOptions.PropertyChanged += (s, e) => WeakReferenceMessenger.Default.Send<ProcessModifiedMessage>();
    }



}
