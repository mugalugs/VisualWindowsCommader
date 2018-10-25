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

        public const uint WS_EX_ACCEPTFILES = 0x00000010;
        public const uint WS_EX_APPWINDOW = 0x00040000;
        public const uint WS_EX_CLIENTEDGE = 0x00000200;
        public const uint WS_EX_COMPOSITED = 0x02000000;
        public const uint WS_EX_CONTEXTHELP = 0x00000400;
        public const uint WS_EX_CONTROLPARENT = 0x00010000;
        public const uint WS_EX_DLGMODALFRAME = 0x00000001;
        public const uint WS_EX_LAYERED = 0x00080000;
        public const uint WS_EX_LAYOUTRTL = 0x00400000;
        public const uint WS_EX_LEFT = 0x00000000;
        public const uint WS_EX_LEFTSCROLLBAR = 0x00004000;
        public const uint WS_EX_MDICHILD = 0x00000040;
        public const uint WS_EX_NOACTIVATE = 0x08000000;
        public const uint WS_EX_NOINHERITLAYOUT = 0x00100000;
        public const uint WS_EX_NOPARENTNOTIFY = 0x00000004;
        public const uint WS_EX_NOREDIRECTIONBITMAP = 0x00200000;
        public const uint WS_EX_RIGHT = 0x00001000;
        public const uint WS_EX_RTLREADING = 0x00002000;
        public const uint WS_EX_STATICEDGE = 0x00020000;
        public const uint WS_EX_TOOLWINDOW = 0x00000080;
        public const uint WS_EX_TOPMOST = 0x00000008;
        public const uint WS_EX_TRANSPARENT = 0x00000020;
        public const uint WS_EX_WINDOWEDGE = 0x00000100;

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

        public struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public int showCmd;
            public System.Drawing.Point ptMinPosition;
            public System.Drawing.Point ptMaxPosition;
            public System.Drawing.Rectangle rcNormalPosition;
        }

        public const UInt32 SW_HIDE = 0;
        public const UInt32 SW_NORMAL = 1;
        public const UInt32 SW_SHOWMINIMIZED = 2;
        public const UInt32 SW_MAXIMIZE = 3;
        public const UInt32 SW_SHOWNOACTIVATE = 4;
        public const UInt32 SW_SHOW = 5;
        public const UInt32 SW_MINIMIZE = 6;
        public const UInt32 SW_SHOWMINNOACTIVE = 7;
        public const UInt32 SW_SHOWNA = 8;
        public const UInt32 SW_RESTORE = 9;

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        public static string WindowState(int cmd)
        {
            List<string> properties = new List<string>();

            if (cmd == SW_HIDE)
                properties.Add("Hide");

            if (cmd == SW_NORMAL)
                properties.Add("Normal");

            if (cmd == SW_SHOWMINIMIZED)
                properties.Add("Show Minimised");

            if (cmd == SW_SHOWNOACTIVATE)
                properties.Add("Show No Activate");

            if (cmd == SW_SHOW)
                properties.Add("Show");

            if (cmd == SW_MAXIMIZE)
                properties.Add("Maximise");

            if (cmd == SW_MINIMIZE)
                properties.Add("Minimise");

            if (cmd == SW_SHOWMINNOACTIVE)
                properties.Add("Show Min No Active");

            if (cmd == SW_SHOWNA)
                properties.Add("Show NA");

            if (cmd == SW_RESTORE)
                properties.Add("Restore");

            return String.Join(", ", properties);
        }

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

        public static string WindowExStyles(uint styles)
        {
            List<string> properties = new List<string>();

            if ((styles & WS_EX_ACCEPTFILES) == WS_EX_ACCEPTFILES)
                properties.Add("Accept Files");

            if ((styles & WS_EX_APPWINDOW) == WS_EX_APPWINDOW)
                properties.Add("App Window");

            if ((styles & WS_EX_CLIENTEDGE) == WS_EX_CLIENTEDGE)
                properties.Add("Client Edge");

            if ((styles & WS_EX_COMPOSITED) == WS_EX_COMPOSITED)
                properties.Add("Composited");

            if ((styles & WS_EX_CONTEXTHELP) == WS_EX_CONTEXTHELP)
                properties.Add("Context Help");

            if ((styles & WS_EX_CONTROLPARENT) == WS_EX_CONTROLPARENT)
                properties.Add("Control Parent");

            if ((styles & WS_EX_DLGMODALFRAME) == WS_EX_DLGMODALFRAME)
                properties.Add("Dialog Modal Frame");

            if ((styles & WS_EX_LAYERED) == WS_EX_LAYERED)
                properties.Add("Layered");

            if ((styles & WS_EX_LAYOUTRTL) == WS_EX_LAYOUTRTL)
                properties.Add("Layout RTL");

            if ((styles & WS_EX_LEFT) == WS_EX_LEFT)
                properties.Add("Left");

            if ((styles & WS_EX_LEFTSCROLLBAR) == WS_EX_LEFTSCROLLBAR)
                properties.Add("Left Scrollbar");

            if ((styles & WS_EX_MDICHILD) == WS_EX_MDICHILD)
                properties.Add("MDI Child");

            if ((styles & WS_EX_NOACTIVATE) == WS_EX_NOACTIVATE)
                properties.Add("No Activate");

            if ((styles & WS_EX_NOINHERITLAYOUT) == WS_EX_NOINHERITLAYOUT)
                properties.Add("No Inherit Layout");

            if ((styles & WS_EX_NOPARENTNOTIFY) == WS_EX_NOPARENTNOTIFY)
                properties.Add("No Parent Notify");

            if ((styles & WS_EX_NOREDIRECTIONBITMAP) == WS_EX_NOREDIRECTIONBITMAP)
                properties.Add("No Redirection Bitmap");

            if ((styles & WS_EX_RIGHT) == WS_EX_RIGHT)
                properties.Add("Right");

            if ((styles & WS_EX_RTLREADING) == WS_EX_RTLREADING)
                properties.Add("RTL Reading");

            if ((styles & WS_EX_STATICEDGE) == WS_EX_STATICEDGE)
                properties.Add("Static Edge");

            if ((styles & WS_EX_TOOLWINDOW) == WS_EX_TOOLWINDOW)
                properties.Add("Tool Window");

            if ((styles & WS_EX_TOPMOST) == WS_EX_TOPMOST)
                properties.Add("Topmost");

            if ((styles & WS_EX_TRANSPARENT) == WS_EX_TRANSPARENT)
                properties.Add("Transparent");

            if ((styles & WS_EX_WINDOWEDGE) == WS_EX_WINDOWEDGE)
                properties.Add("Window Edge");

            return String.Join(", ", properties);
        }
    }
}
