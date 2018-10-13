using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace VisualWindowsManager
{
    public class WindowInfo
    {
        public IntPtr hwnd;
        public Win32Interop.WINDOWINFO windowInfo;
        public string windowName;
        public string className;
        public Rectangle screenPosition;

        public bool exists = true;

        //fix the rcWindow rect by changing l, t, r, b to x, y, w, h
        public void SetRect(Rectangle window)
        {
            screenPosition.X = window.X;
            screenPosition.Y = window.Y;
            screenPosition.Width = window.Width - window.X;
            screenPosition.Height = window.Height - window.Y;
        }

        public void UpdateLocation()
        {
            Rectangle rect;
            Win32Interop.GetWindowRect(hwnd, out rect);
            SetRect(rect);
        }
    }
}
