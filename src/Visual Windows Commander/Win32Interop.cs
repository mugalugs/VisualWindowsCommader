using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;

namespace VisualWindowsManager
{
    public class Win32Interop
    {
        public delegate bool EnumDelegate(IntPtr hWnd, int lParam);

        public const uint WS_VISIBLE = 0x10000000;
        public const uint WS_MINIMIZE = 0x20000000;
        public const uint WS_POPUP = 0x80000000;
        public const uint WS_THICKFRAME = 0x00040000;
        public const uint WS_SYSMENU = 0x00080000;
        public const uint WS_BORDER = 0x00800000;
        public const uint WS_CHILD = 0x40000000;
        public const uint WS_DISABLED = 0x08000000;
        public const uint WS_MAXIMIZE = 0x01000000;
        public const uint WS_OVERLAPPED = 0x00000000;

        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWINFO
        {
            public uint cbSize;
            public Rectangle rcWindow;
            public Rectangle rcClient;
            public uint dwStyle;
            public uint dwExStyle;
            public uint dwWindowStatus;
            public uint cxWindowBorders;
            public uint cyWindowBorders;
            public ushort atomWindowType;
            public ushort wCreatorVersion;

            public WINDOWINFO(Boolean? filler) : this()   // Allows automatic initialization of "cbSize" with "new WINDOWINFO(null/true/false)".
            {
                cbSize = (UInt32)(Marshal.SizeOf(typeof(WINDOWINFO)));
            }

        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetWindowInfo(IntPtr hwnd, ref WINDOWINFO pwi);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetLayeredWindowAttributes(IntPtr hwnd, out uint crKey, out byte bAlpha, out uint dwFlags);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetWindowRect(IntPtr hWnd, out Rectangle rect);

        public const uint SWP_NOACTIVATE = 0x0010;
        public const uint SWP_NOMOVE = 0x0002;
        public const uint SWP_NOSIZE = 0x0001;
        public const uint SWP_NOZORDER = 0x0004;
        public const uint SWP_SHOWWINDOW = 0x0040;
        public const uint SWP_NOOWNERZORDER = 0x0200;
        public const uint SWP_DEFERERASE = 0x2000;

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, uint wFlags);
        
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr WindowFromPoint([MarshalAs(UnmanagedType.Struct)] Point pt);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetClassName(IntPtr hWnd, [Out, MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int RealGetWindowClass(IntPtr hWnd, [Out, MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowText(IntPtr hWnd, [Out, MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpString, int nMaxCount);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern int GetModuleFileName(IntPtr hWnd, [Out, MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", EntryPoint = "EnumDesktopWindows", ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool EnumDesktopWindows(IntPtr hDesktop, EnumDelegate lpEnumCallbackFunction, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        //only use this version for passing null
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, object lpdwProcessId);

        public enum UMS_THREAD_INFO_CLASS
        {
            UmsThreadInvalidInfoClass,
            UmsThreadUserContext,
            UmsThreadPriority,
            UmsThreadAffinity,
            UmsThreadTeb,
            UmsThreadIsSuspended,
            UmsThreadIsTerminated,
            UmsThreadMaxInfoClass
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool QueryUmsThreadInformation(IntPtr hThread, UMS_THREAD_INFO_CLASS threadInfoClass, out uint buffer, uint bufferLength, out uint length);

        public static string WindowStyles(uint styles)
        {
            List<string> properties = new List<string>();

            if ((styles & WS_VISIBLE) == WS_VISIBLE)
                properties.Add("Visible");

            if ((styles & WS_CHILD) == WS_CHILD)
                properties.Add("Child");

            if ((styles & WS_BORDER) == WS_BORDER)
                properties.Add("Border");

            if ((styles & WS_THICKFRAME) == WS_THICKFRAME)
                properties.Add("Thickframe");

            if ((styles & WS_POPUP) == WS_POPUP)
                properties.Add("Popup");

            if ((styles & WS_SYSMENU) == WS_SYSMENU)
                properties.Add("Sysmenu");

            if ((styles & WS_DISABLED) == WS_DISABLED)
                properties.Add("Disabled");

            if ((styles & WS_MAXIMIZE) == WS_MAXIMIZE)
                properties.Add("Maximize");

            if ((styles & WS_MINIMIZE) == WS_MINIMIZE)
                properties.Add("Minimize");

            if ((styles & WS_OVERLAPPED) == WS_OVERLAPPED)
                properties.Add("Overlapped");

            return String.Join(", ", properties);
        }
    }
}
