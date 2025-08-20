using Echo.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Echo.Services;

public class ProcessLauncherService
{


    public void LaunchProcesses(IEnumerable<ProcessRipple> ripplePrograms)
    {
        foreach (var ripple in ripplePrograms)
        {
          if (
                (!ripple.ProcessTriggerOptions.Enabled) ||
                (!ripple.ProcessTriggerOptions.AllowMultiple && ripple.Process is not null && !ripple.Process.HasExited))
            {
                continue;
            }
          
            // Launch the process
            LaunchProcess(ripple);
        }
    }




    public async void LaunchProcess(ProcessRipple ripple)
    {
        try
        {
            ripple.Process = ShellExecuteHelper.ShellExecuteAndGetProcess(
                 ripple.RippleProcessPath,
                 ripple.ProcessTriggerOptions.Arguments,
                 null,
                 nshow: ripple.ProcessTriggerOptions.LaunchHidden ? 2 : 1 // SW_HIDE : SW_SHOWNORMAL : SW_SHOWMINIMISED
             );

        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error launching process: {ex.Message}");
        }
    }


}
