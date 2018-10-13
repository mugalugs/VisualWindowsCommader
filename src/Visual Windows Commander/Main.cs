using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Text;
using System.Windows.Forms;

namespace VisualWindowsManager
{
    public partial class Main : Form
    {
        List<WindowInfo> windowData = new List<WindowInfo>();
        Rectangle desktopBounds = new Rectangle();
        Rectangle windowBounds = new Rectangle();
        Rectangle maxBounds = new Rectangle();
        Timer repaint = new Timer();
        Timer refresh = new Timer();
        bool mouseDown = false;
        Point grabPointOffset = Point.Empty;
        WindowInfo closest = null;

        volatile bool windowRefresh = false;
        
        // could move all of these to a settings class etc.
        Font smallWindowFont = new Font(FontFamily.GenericSansSerif, 8, FontStyle.Regular);

        Color backgroundColor = Color.FromArgb(40, 40, 50);
        Pen virtualScreenOutlinePen = Pens.Blue;

        Pen windowNormalOutlinePen = Pens.White;
        Brush windowNormalNameBrush = Brushes.White;
        Pen windowCloestOutlinePen = Pens.LightGreen;
        Brush windowClosestNameBrush = Brushes.LightGreen;
        Pen windowMovingOutlinePen = Pens.Orange;
        Brush windowMovingNameBrush = Brushes.Orange;
        
        public Main()
        {
            InitializeComponent();

            renderPanel.BackColor = backgroundColor;

            //yay for protected members
            renderPanel.GetType().GetMethod("SetStyle", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.InvokeMethod)
                .Invoke(renderPanel, new object[] { ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true });

            repaint.Interval = 1000 / 15;
            repaint.Tick += Repaint_Tick;
            repaint.Start();

            refresh.Interval = 1000;
            refresh.Tick += Refresh_Tick;
            refresh.Start();

            desktopBounds.Width = SystemInformation.VirtualScreen.Width;
            desktopBounds.Height = SystemInformation.VirtualScreen.Height;

            Refresh_Tick(null, null);
        }

        private void Refresh_Tick(object sender, EventArgs e)
        {
            // don't refresh window state if were moving some around, prevent jarring experience
            if (mouseDown)
                return;

            windowRefresh = true;

            // mark all of the existing windows that they need to be checked
            foreach (WindowInfo window in windowData)
            {
                window.exists = false;
            }

            Win32Interop.EnumDesktopWindows(IntPtr.Zero, new Win32Interop.EnumDelegate(EnumWindowsProc), IntPtr.Zero);

            // remove all the windows that don't exist anymore
            for (int i = windowData.Count - 1; i > 0; i--) // remove them in reverse to avoid creating a black hole and killing humanity
            {
                if (!windowData[i].exists)
                {
                    windowData.RemoveAt(i);
                }
            }

            UpdateWindowBounds();

            windowRefresh = false;
        }

        private void UpdateWindowBounds()
        {
            windowBounds = new Rectangle();

            //update the window and max bounds
            foreach (WindowInfo window in windowData)
            {
                //ignore if minimised
                if (window.screenPosition.X == -32000 && window.screenPosition.Y == -32000)
                    continue;

                windowBounds = Rectangle.Union(windowBounds, window.screenPosition);
            }

            maxBounds = Rectangle.Union(desktopBounds, windowBounds);

            //push instead so everything is on the panel
            if (maxBounds.X < 0)
            {
                maxBounds.X = Math.Abs(maxBounds.X);
            }

            if (maxBounds.Y < 0)
            {
                maxBounds.Y = Math.Abs(maxBounds.Y);
            }
        }
        
        private bool EnumWindowsProc(IntPtr hWnd, int lParam)
        {
            string windowText = "";
            string className = "";
            Rectangle rect;
            Win32Interop.WINDOWINFO info = new Win32Interop.WINDOWINFO();
            info.cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(info);
            
            // if we already have it simply skip
            foreach (WindowInfo window in windowData)
            {
                if (window.hwnd == hWnd)
                {
                    window.exists = true;
                    return true;
                }
            }

            StringBuilder sb = new StringBuilder(254);

            Win32Interop.GetWindowText(hWnd, sb, 255);

            if (sb.Length > 0)
                windowText = sb.ToString();

            sb.Clear();
            
            if (Win32Interop.GetWindowInfo(hWnd, ref info) && windowText.Length > 0)
            {
                Win32Interop.GetClassName(hWnd, sb, 255);

                if (sb.Length > 0)
                    className = sb.ToString();

                sb.Clear();

                // "Real" windows
                if (HasOpt(info.dwStyle, Win32Interop.WS_VISIBLE) &&
                    HasOpt(info.dwStyle, Win32Interop.WS_THICKFRAME) &&
                    HasOpt(info.dwStyle, Win32Interop.WS_SYSMENU) &&
                    HasOpt(info.dwStyle, Win32Interop.WS_OVERLAPPED) &&
                    HasOpt(info.dwStyle, Win32Interop.WS_BORDER))
                {
                    WindowInfo window = new WindowInfo();
                    window.hwnd = hWnd;
                    window.windowName = windowText;
                    window.className = className;
                    window.windowInfo = info;
                    window.SetRect(info.rcWindow);

                    windowData.Add(window);
                }

                // "Popup" dialogs, like find, open etc.
                if (HasOpt(info.dwStyle, Win32Interop.WS_POPUP) &&
                    HasOpt(info.dwStyle, Win32Interop.WS_SYSMENU) &&
                    HasOpt(info.dwStyle, Win32Interop.WS_OVERLAPPED) &&
                    HasOpt(info.dwStyle, Win32Interop.WS_BORDER) &&
                    !HasOpt(info.dwStyle, Win32Interop.WS_THICKFRAME) && // removes hidden weird things
                    info.rcWindow.X != 0 &&
                    info.rcWindow.Y != 0)
                {
                    WindowInfo window = new WindowInfo();
                    window.hwnd = hWnd;
                    window.windowName = windowText;
                    window.className = className;
                    window.windowInfo = info;
                    window.SetRect(info.rcWindow);

                    windowData.Add(window);
                }
            }
            
            return true;
        }

        private bool HasOpt(uint num, uint opt)
        {
            return (num & opt) == opt;
        }

        private void Repaint_Tick(object sender, EventArgs e)
        {
            if (!windowRefresh) // skip repainting if we're refreshing window state
                renderPanel.Invalidate();
        }

        private double Distance(Point p, float x, float y)
        {
            return Math.Sqrt(((p.X - x) * (p.X - x)) + ((p.Y - y) * (p.Y - y)));
        }

        private void renderPanel_Paint(object sender, PaintEventArgs e)
        {
            Brush textColor;
            Pen outlineColor;
            float x;
            float y;
            float width;
            float height;
            Point cursor = renderPanel.PointToClient(Cursor.Position); // make the cursor panel space
            double distance = 0;
            double closestDistance = double.MaxValue;
            WindowInfo nearest = null;
            
            float scaleX = renderPanel.DisplayRectangle.Width / (float)(maxBounds.Width);
            float scaleY = renderPanel.DisplayRectangle.Height / (float)(maxBounds.Height);
            
            if (!mouseDown)
            {
                //calc closest
                foreach (WindowInfo window in windowData)
                {
                    //ignore if minimised
                    if (window.screenPosition.X == -32000 && window.screenPosition.Y == -32000)
                        continue;

                    //prevent hilarous bug
                    if (window.hwnd == Handle)
                        continue;

                    x = (maxBounds.X + window.screenPosition.X) * scaleX;
                    y = (maxBounds.Y + window.screenPosition.Y) * scaleY;

                    distance = Distance(cursor, x, y);

                    if (distance < closestDistance)
                    {
                        nearest = window;
                        closestDistance = distance;
                    }
                }

                closest = nearest; // ya ya
            }

            foreach (WindowInfo window in windowData)
            {
                window.UpdateLocation();

                //ignore if minimised
                if (window.screenPosition.X == -32000 && window.screenPosition.Y == -32000)
                    continue;

                outlineColor = windowNormalOutlinePen;
                textColor = windowNormalNameBrush;

                if (window.hwnd == Handle)
                {
                    outlineColor = Pens.Gray;
                    textColor = Brushes.Gray;
                }

                x = (maxBounds.X + window.screenPosition.X) * scaleX;
                y = (maxBounds.Y + window.screenPosition.Y) * scaleY;
                width = window.screenPosition.Width * scaleX;
                height = window.screenPosition.Height * scaleY;
                
                if (window == closest)
                {
                    outlineColor = windowCloestOutlinePen;
                    textColor = windowClosestNameBrush;

                    if (mouseDown)
                    {
                        //cursor which is in panel space
                        //but scale is based on the panel and max bounds not screen
                        //ie we need to scale back up
                        
                        if (grabPointOffset == Point.Empty)
                        {
                            grabPointOffset.X = (int)( cursor.X - x );
                            grabPointOffset.Y = (int)( cursor.Y - y );
                        }
                        
                        int newPosX = (int)( (cursor.X - grabPointOffset.X) / scaleX);
                        int newPosY = (int)( (cursor.Y - grabPointOffset.Y) / scaleY);

                        //currently in max bound space but SetWindowPos uses screen space
                        //since maxBounds is a superset of screenspace we just need to adjust it with an offset

                        Win32Interop.SetWindowPos(window.hwnd, 0, newPosX - maxBounds.X, newPosY - maxBounds.Y, 0, 0, 
                            Win32Interop.SWP_NOACTIVATE | Win32Interop.SWP_NOSIZE | Win32Interop.SWP_NOZORDER | Win32Interop.SWP_NOOWNERZORDER | Win32Interop.SWP_DEFERERASE);

                        outlineColor = windowMovingOutlinePen;
                        textColor = windowMovingNameBrush;
                    }
                }

                e.Graphics.DrawRectangle(outlineColor,
                    x,
                    y,
                    width,
                    height);

                e.Graphics.DrawString(window.windowName, smallWindowFont, textColor, 
                    x, 
                    y);


                distance = Distance(cursor, x, y);
                e.Graphics.DrawString(Math.Round(distance).ToString(), smallWindowFont, textColor,
                    x,
                    y + 15);
            }

            if (maxBounds.Size != desktopBounds.Size)
            {
                e.Graphics.DrawRectangle(virtualScreenOutlinePen,
                        (maxBounds.X + desktopBounds.X) * scaleX,
                        (maxBounds.Y + desktopBounds.Y) * scaleY,
                        desktopBounds.Width * scaleX,
                        desktopBounds.Height * scaleY);
            }
        }

        private void renderPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (closest == null) return;

            grabPointOffset = Point.Empty;
            mouseDown = true;
            TopMost = true;
            Opacity = 0.7;
        }

        private void renderPanel_MouseUp(object sender, MouseEventArgs e)
        {
            grabPointOffset = Point.Empty;
            mouseDown = false;
            TopMost = false;
            Opacity = 1;
            closest = null;
        }
    }
}
