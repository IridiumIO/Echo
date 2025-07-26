using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Echo.Services;

public class ShellExecuteHelper
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct SHELLEXECUTEINFO
    {
        public int cbSize;
        public uint fMask;
        public IntPtr hwnd;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string lpVerb;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string lpFile;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string lpParameters;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string lpDirectory;
        public int nShow;
        public IntPtr hInstApp;
        public IntPtr lpIDList;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string lpClass;
        public IntPtr hkeyClass;
        public uint dwHotKey;
        public IntPtr hIconOrMonitor;
        public IntPtr hProcess;
    }

    private const uint SEE_MASK_NOCLOSEPROCESS = 0x00000040;

    [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern bool ShellExecuteEx(ref SHELLEXECUTEINFO lpExecInfo);

    public static Process ShellExecuteAndGetProcess(string file, string parameters = null, string verb = null, int nshow = 1)
    {
        var info = new SHELLEXECUTEINFO();
        info.cbSize = Marshal.SizeOf(info);
        info.fMask = SEE_MASK_NOCLOSEPROCESS;
        info.hwnd = IntPtr.Zero;
        info.lpVerb = verb;
        info.lpFile = file;
        info.lpParameters = parameters;
        info.lpDirectory = null;
        info.nShow = nshow; // SW_HIDE : SW_SHOWNORMAL : SW_SHOWMINIMISED
        info.hInstApp = IntPtr.Zero;
        info.hProcess = IntPtr.Zero;

        if (!ShellExecuteEx(ref info))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        if (info.hProcess != IntPtr.Zero)
        {
            return Process.GetProcessById(GetProcessId(info.hProcess));
        }
        return null;
    }

    [DllImport("kernel32.dll")]
    private static extern int GetProcessId(IntPtr handle);
}
