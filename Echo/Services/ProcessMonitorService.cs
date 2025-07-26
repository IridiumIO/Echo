using Echo.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace Echo.Services;

public class ProcessMonitorService
{

    private ManagementEventWatcher? _creationWatcher;
    private ManagementEventWatcher? _deletionWatcher;
    private readonly Dictionary<string, int> _runningProcessIds = new();

    public void StartMonitoring(IEnumerable<ProcessTrigger> triggers, Action<string, int> onProcessStarted, Action<string, int> onProcessExited)
    {
        // Listen for process creation
        string creationQuery = "SELECT * FROM __InstanceCreationEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_Process'";
        _creationWatcher = new ManagementEventWatcher(new WqlEventQuery(creationQuery));
        _creationWatcher.EventArrived += (sender, e) =>
        {
            var targetInstance = (ManagementBaseObject)e.NewEvent["TargetInstance"];
            string processName = (string)targetInstance["Name"];
            int processId = Convert.ToInt32(targetInstance["ProcessId"]);
            string? executablePath = targetInstance["ExecutablePath"] as string;
            foreach (var trigger in triggers)
            {

                if (executablePath is not null &&
                    !trigger.TargetProcessPaths.Any(path => executablePath.EndsWith(path, StringComparison.OrdinalIgnoreCase)))
                {
                    continue; // Skip if the executable path does not match the trigger
                }


                // Check if processName matches any file name in TargetProcessPaths
                if (trigger.TargetProcessPaths.Any(path => processName.Equals(Path.GetFileName(path), StringComparison.OrdinalIgnoreCase)))
                {
                    _runningProcessIds[processName] = processId;
                    onProcessStarted?.Invoke(processName, processId);
                }
            }
        };
        _creationWatcher.Start();

        // Listen for process exti
        string deletionQuery = "SELECT * FROM __InstanceDeletionEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_Process'";
        _deletionWatcher = new ManagementEventWatcher(new WqlEventQuery(deletionQuery));
        _deletionWatcher.EventArrived += (sender, e) =>
        {
            var targetInstance = (ManagementBaseObject)e.NewEvent["TargetInstance"];
            string processName = (string)targetInstance["Name"];
            int processId = Convert.ToInt32(targetInstance["ProcessId"]);
            if (_runningProcessIds.TryGetValue(processName, out int trackedId) && trackedId == processId)
            {
                _runningProcessIds.Remove(processName);
                onProcessExited?.Invoke(processName, processId);
            }
        };
        _deletionWatcher.Start();
    }

    public void StopMonitoring()
    {
        _creationWatcher?.Stop();
        _creationWatcher?.Dispose();
        _creationWatcher = null;

        _deletionWatcher?.Stop();
        _deletionWatcher?.Dispose();
        _deletionWatcher = null;

        _runningProcessIds.Clear();
    }


    public bool IsProcessRunning(string processName)
    {
        var processes = System.Diagnostics.Process.GetProcessesByName(processName);
        return processes.Length > 0;
    }


}
