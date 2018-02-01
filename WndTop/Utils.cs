using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;

namespace WndTop
{
    class Utils
    {
        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        [DllImport("user32.dll", EntryPoint = "FindWindow", CharSet = CharSet.Auto)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);

        const UInt32 SWP_NOSIZE = 0x0001;
        const UInt32 SWP_NOMOVE = 0x0002;
        const UInt32 SWP_SHOWWINDOW = 0x0040;

        IntPtr beforeHwnd = new IntPtr(-1);



        public static void SetOnTop(IntPtr winHandle)
        {
            SetWindowPos(winHandle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
        }

        public static void CancelOnTop(IntPtr winHandle)
        {
            SetWindowPos(winHandle, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
        }


        private const int WH_MOUSE_LL = 14;

        private enum MouseMessages
        {
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_MOUSEMOVE = 0x0200,
            WM_MOUSEWHEEL = 0x020A,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        static extern IntPtr WindowFromPoint(POINT Point);

        [DllImport("user32.dll")]
        static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);
        enum GetWindow_Cmd : uint
        {
            GW_HWNDFIRST = 0,
            GW_HWNDLAST = 1,
            GW_HWNDNEXT = 2,
            GW_HWNDPREV = 3,
            GW_OWNER = 4,
            GW_CHILD = 5,
            GW_ENABLEDPOPUP = 6
        }
        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern IntPtr GetParent(IntPtr hWnd);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);



        private static LowLevelMouseProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        static IntPtr hHook = IntPtr.Zero;

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && MouseMessages.WM_LBUTTONDOWN == (MouseMessages)wParam)
            {
                //  The application runs to here when you click on the window whose handle you  want to get                
                POINT cusorPoint;
                bool ret = GetCursorPos(out cusorPoint);
                // cusorPoint contains your cusor’s position when you click on the window


                // Then use cusorPoint to get the handle of the window you clicked

                IntPtr winHandle = WindowFromPoint(cusorPoint);

                Debug.WriteLine("handle: " + winHandle.ToString("X"));

                // Get the final parent handle of the window
                while (true)
                {
                    IntPtr temp = GetParent(winHandle);
                    Debug.WriteLine("parent handle: " + temp.ToString("X"));
                    if (temp.Equals(IntPtr.Zero))
                        break;
                    winHandle = temp;
                    Debug.WriteLine("handle: " + winHandle.ToString("X"));

                }
                //IntPtr winHandle = GetActiveWindow();


                // winHandle is the Hanle you need
                // Now you have get the handle, do what you want here
                // …………………………………………………. 
                using (Process curProcess = Process.GetCurrentProcess())
                {
                    using (ProcessModule curModule = curProcess.MainModule)
                    {
                        //IntPtr hwndCur = GetModuleHandle(curModule.ModuleName);

                        IntPtr hwndCur = curProcess.MainWindowHandle;

                        if (mainWindow.btnTop.IsChecked == true)
                        {
                            if (hwndCur != winHandle)
                            {
                                Debug.WriteLine("cur handle: " + winHandle.ToString("X"));
                                SetOnTop(winHandle);
                            }
                            mainWindow.btnTop.IsChecked = false;
                        }

                        if (mainWindow.btnRelease.IsChecked == true)
                        {
                            if (hwndCur != winHandle)
                            {
                                CancelOnTop(winHandle);
                            }
                            mainWindow.btnRelease.IsChecked = false;
                        }
                    }
                }




                // Because the hook may occupy much memory, so remember to uninstall the hook after
                // you finish your work, and that is what the following code does.
                UnhookWindowsHookEx(hHook);
                hHook = IntPtr.Zero;


                // Here I do not use the GetActiveWindow(). Let's call the window you clicked "DesWindow" and explain my reason.
                // I think the hook intercepts the mouse click message before the mouse click message delivered to the DesWindow's 
                // message queue. The application came to this function before the DesWindow became the active window, so the handle 
                // abtained from calling GetActiveWindow() here is not the DesWindow's handle, I did some tests, and What I got is always 
                // the Form's handle, but not the DesWindow's handle. You can do some test too.

                //IntPtr handle = GetActiveWindow();


            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        public static MainWindow mainWindow;
        public static void fun(MainWindow wnd)
        {
            mainWindow = wnd;
            if (IntPtr.Zero == hHook)
            {
                using (Process curProcess = Process.GetCurrentProcess())
                using (ProcessModule curModule = curProcess.MainModule)
                {
                    hHook = SetWindowsHookEx(WH_MOUSE_LL, _proc,
                        GetModuleHandle(curModule.ModuleName), 0);
                }
            }
        }
    }

    class OnTop
    {
        private static Timer timer;
        private static IntPtr intPtr;

        public static void Run(int interval)
        {
            timer = new Timer(interval);

            using (Process curProcess = Process.GetCurrentProcess())
            {
                using (ProcessModule curModule = curProcess.MainModule)
                {
                    //IntPtr hwndCur = GetModuleHandle(curModule.ModuleName);

                    intPtr = curProcess.MainWindowHandle;

                    timer.Elapsed += timer_Elapsed;
                    timer.Start();
                }
            }

        }

        private static void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Utils.SetOnTop(intPtr);
        }
    }
}
