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
        List<WindowInfo> newWindows = new List<WindowInfo>();
        Rectangle desktopBounds = new Rectangle();
        Rectangle windowBounds = new Rectangle();
        Rectangle maxBounds = new Rectangle();
        Point initialOffset = new Point();
        Timer repaint = new Timer();
        Timer refresh = new Timer();
        bool mouseDown = false;
        Point grabPointOffset = Point.Empty;
        WindowInfo closest = null;
        Screen[] ScreenCache = Screen.AllScreens;
        Rectangle[] screens = new Rectangle[Screen.AllScreens.Length];

        volatile bool windowRefresh = false;

        // could move all of these to a settings class etc.
        float maxDistanceToSelectClosest = 200;
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

            repaint.Interval = 1000 / 30;
            repaint.Tick += Repaint_Tick;
            repaint.Start();

            refresh.Interval = 1000;
            refresh.Tick += Refresh_Tick;
            refresh.Start();
            
            // doesnt account for multiple displays when out of order
            desktopBounds.Width = SystemInformation.VirtualScreen.Width;
            desktopBounds.Height = SystemInformation.VirtualScreen.Height;

            Refresh_Tick(null, null);
        }

        private void Refresh_Tick(object sender, EventArgs e)
        {
            // don't refresh window state if were moving some around, prevent jarring experience
            if (mouseDown)
                return;
            
            // mark all of the existing windows that they need to be checked
            foreach (WindowInfo window in windowData)
            {
                window.exists = false;
            }

            Win32Interop.EnumDesktopWindows(IntPtr.Zero, new Win32Interop.EnumDelegate(EnumWindowsProc), IntPtr.Zero);

            windowRefresh = true;

            // remove all the windows that don't exist anymore
            for (int i = windowData.Count - 1; i > 0; i--) // remove them in reverse to avoid creating a black hole and killing humanity
            {
                if (!windowData[i].exists)
                {
                    windowData.RemoveAt(i);
                }
            }

            windowData.AddRange(newWindows);
            newWindows.Clear();

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

            //adjust window bounds for screen adjustments
            
            for (var i = 0; i < ScreenCache.Length; i++)
            {
                if (!ScreenCache[i].Primary)
                {
                    if (ScreenCache[i].Bounds.X < 0)
                        windowBounds.X += Math.Abs(ScreenCache[i].Bounds.Width);

                    if (ScreenCache[i].Bounds.Y < 0)
                        windowBounds.Y += Math.Abs(ScreenCache[i].Bounds.Height);
                }

                screens[i] = ScreenCache[i].Bounds;
            }

            maxBounds = Rectangle.Union(desktopBounds, windowBounds);
            
            //push instead so everything is on the panel
            maxBounds.X = Math.Abs(maxBounds.X);
            maxBounds.Y = Math.Abs(maxBounds.Y);
            initialOffset = maxBounds.Location;
            
            // shift all rects to be within virtual screen space (offset)
            for (var i = 0; i < screens.Length; i++)
            {
                if (screens[i].X < 0)
                {
                    for (var j = 0; j < screens.Length; j++)
                    {
                        screens[j].X += screens[i].Width;
                    }

                    maxBounds.X += screens[i].Width;
                }

                if (screens[i].Y < 0)
                {
                    for (var j = 0; j < screens.Length; j++)
                    {
                        screens[j].Y += screens[i].Height;
                    }
                    
                    maxBounds.Y += screens[i].Height;
                }

                screens[i].X += initialOffset.X;
                screens[i].Y += initialOffset.Y;
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
            
            if (windowText.Length > 0 && Win32Interop.GetWindowInfo(hWnd, ref info))
            {
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
                    window.SetRect(info.rcWindow);

                    newWindows.Add(window);
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
                    window.SetRect(info.rcWindow);

                    newWindows.Add(window);
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
            renderPanel.Invalidate();
        }

        private double Distance(Point p, float x, float y)
        {
            return Math.Sqrt(((p.X - x) * (p.X - x)) + ((p.Y - y) * (p.Y - y)));
        }

        private void renderPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics gfx = e.Graphics;

            // skip repainting if we're refreshing window state
            // because we might not be the only callers the check is here
            if (windowRefresh)
                return;
            
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

                    x = (maxBounds.X + window.screenPosition.X + window.screenPosition.Width / 2) * scaleX;
                    y = (maxBounds.Y + window.screenPosition.Y + window.screenPosition.Height / 2) * scaleY;
                    
                    distance = Distance(cursor, x, y);

                    if (distance < closestDistance)
                    {
                        nearest = window;
                        closestDistance = distance;
                    }
                }

                if (closestDistance < maxDistanceToSelectClosest)
                    closest = nearest; // ya ya
                else
                    closest = null;
            }

            foreach (WindowInfo window in windowData)
            {
                window.UpdateLocation();

                //ignore if minimised
                if (window.screenPosition.X == -32000 && window.screenPosition.Y == -32000)
                    continue;
                
                //draw the closest last
                if (window == closest)
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
                
                gfx.DrawRectangle(outlineColor,
                    x,
                    y,
                    width,
                    height);

                //cross
                gfx.DrawLine(outlineColor, x + (width / 2) - 5, y + (height / 2) - 5, x + (width / 2) + 5, y + (height / 2) + 5);
                gfx.DrawLine(outlineColor, x + (width / 2) + 5, y + (height / 2) - 5, x + (width / 2) - 5, y + (height / 2) + 5);

                gfx.DrawString(window.windowName, smallWindowFont, textColor, x, y);
                
                distance = Distance(cursor, x + (window.screenPosition.Width / 2 * scaleX), y + (window.screenPosition.Height / 2 * scaleY));
                gfx.DrawString(Math.Round(distance).ToString(), smallWindowFont, textColor, x, y + 15);
            }
            
            if (closest != null)
            {
                x = (maxBounds.X + closest.screenPosition.X) * scaleX;
                y = (maxBounds.Y + closest.screenPosition.Y) * scaleY;
                width = closest.screenPosition.Width * scaleX;
                height = closest.screenPosition.Height * scaleY;

                outlineColor = windowCloestOutlinePen;
                textColor = windowClosestNameBrush;

                if (mouseDown)
                {
                    //cursor which is in panel space
                    //but scale is based on the panel and max bounds not screen
                    //ie we need to scale back up

                    if (grabPointOffset == Point.Empty)
                    {
                        grabPointOffset.X = (int)(cursor.X - x);
                        grabPointOffset.Y = (int)(cursor.Y - y);
                    }

                    int newPosX = (int)((cursor.X - grabPointOffset.X) / scaleX);
                    int newPosY = (int)((cursor.Y - grabPointOffset.Y) / scaleY);

                    //currently in max bound space but SetWindowPos uses screen space
                    //since maxBounds is a superset of screenspace we just need to adjust it with an offset

                    Win32Interop.SetWindowPos(closest.hwnd, 0, newPosX - maxBounds.X, newPosY - maxBounds.Y, 0, 0,
                        Win32Interop.SWP_NOACTIVATE | Win32Interop.SWP_NOSIZE | Win32Interop.SWP_NOZORDER | Win32Interop.SWP_NOOWNERZORDER | Win32Interop.SWP_DEFERERASE);

                    outlineColor = windowMovingOutlinePen;
                    textColor = windowMovingNameBrush;
                }

                //cross
                gfx.DrawLine(outlineColor, x + (width / 2) - 5, y + (height / 2) - 5, x + (width / 2) + 5, y + (height / 2) + 5);
                gfx.DrawLine(outlineColor, x + (width / 2) + 5, y + (height / 2) - 5, x + (width / 2) - 5, y + (height / 2) + 5);

                SizeF windowNameBackground = gfx.MeasureString(closest.windowName, smallWindowFont);
                gfx.FillRectangle(new SolidBrush(backgroundColor), x - 5, y - 5, windowNameBackground.Width + 10, windowNameBackground.Height + 10);
                gfx.DrawString(closest.windowName, smallWindowFont, textColor,
                    x,
                    y);

                gfx.DrawRectangle(outlineColor,
                    x,
                    y,
                    width,
                    height);

                distance = Distance(cursor, x + (closest.screenPosition.Width / 2 * scaleX), y + (closest.screenPosition.Height / 2 * scaleY));
                gfx.DrawString(Math.Round(distance).ToString(), smallWindowFont, textColor,
                    x,
                    y + 15);

                gfx.DrawLine(windowCloestOutlinePen, cursor.X, cursor.Y, (maxBounds.X + closest.screenPosition.X + closest.screenPosition.Width / 2) * scaleX, (maxBounds.Y + closest.screenPosition.Y + closest.screenPosition.Height / 2) * scaleY);
            }

            //screens
            if (maxBounds.Size != desktopBounds.Size)
            {
                gfx.DrawRectangle(virtualScreenOutlinePen,
                        (initialOffset.X) * scaleX,
                        (initialOffset.Y) * scaleY,
                        desktopBounds.Width * scaleX,
                        desktopBounds.Height * scaleY);
            }

            foreach (Rectangle scr in screens)
            {
                gfx.DrawRectangle(Pens.Pink,
                        (scr.X) * scaleX,
                        (scr.Y) * scaleY,
                        scr.Width * scaleX,
                        scr.Height * scaleY);
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
