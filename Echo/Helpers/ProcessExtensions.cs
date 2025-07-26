using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace Echo.Helpers;

public static class ProcessExtensions
{
    public static void KillProcessTree(int pid)
    {
        // Find all child processes
        var searcher = new ManagementObjectSearcher(
            $"Select * From Win32_Process Where ParentProcessID={pid}");
        var moc = searcher.Get();

        foreach (ManagementObject mo in moc)
        {
            int childPid = Convert.ToInt32(mo["ProcessId"]);
            KillProcessTree(childPid); // Recursively kill children
        }

        try
        {
            var process = Process.GetProcessById(pid);
            if (!process.HasExited)
                process.Kill();
        }
        catch
        {
            // Process might have already exited
        }
    }

    public static void KillProcessTree(this Process process)
    {
        if (process == null || process.HasExited)
            return;
        KillProcessTree(process.Id);
    }

}