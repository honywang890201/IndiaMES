using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
namespace RecoverCheck
{

    class EmbeddedApp : HwndHost, IKeyboardInputSink
        {
            [DllImport("user32.dll", EntryPoint = "FindWindow")]
            private extern static IntPtr FindWindow(string lpClassName, string lpWindowName);
            [DllImport("user32.dll")]
            private static extern int SetParent(IntPtr hWndChild, IntPtr hWndParent);
            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            private static extern uint SetWindowLong(IntPtr hwnd, int nIndex, uint newLong);
            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            private static extern uint GetWindowLong(IntPtr hwnd, int nIndex);
            [DllImport("user32.dll")]
            private static extern int EnumWindows(CallBackPtr callPtr, ref WindowInfo WndInfoRef);
            [DllImport("User32.dll")]
            static extern int GetWindowText(IntPtr handle, StringBuilder text, int MaxLen);
            [DllImport("user32.dll")]
            public static extern int GetWindowRect(IntPtr hwnd, ref RECT rc);
            [DllImport("user32.dll", EntryPoint = "SendMessage", SetLastError = true, CharSet = CharSet.Auto)]
            private static extern int SendMessage(IntPtr hwnd, uint wMsg, int wParam, int lParam); //对外部软件窗口发送一些消息(如 窗口最大化、最小化等)
            internal const int
                GWL_WNDPROC = (-4),
                GWL_HINSTANCE = (-6),
                GWL_HWNDPARENT = (-8),
                GWL_STYLE = (-16),
                GWL_EXSTYLE = (-20),
                GWL_USERDATA = (-21),
                GWL_ID = (-12);
            internal const uint
                  WS_CHILD = 0x40000000,
                  WS_VISIBLE = 0x10000000,
                  LBS_NOTIFY = 0x00000001,
                  HOST_ID = 0x00000002,
                  LISTBOX_ID = 0x00000001,
                  WS_VSCROLL = 0x00200000,
                  WS_BORDER = 0x00800000,
                  WS_POPUP = 0x80000000;
            private const int HWND_TOP = 0x0;
            private const int WM_COMMAND = 0x0112;
            private const int WM_QT_PAINT = 0xC2DC;
            private const int WM_PAINT = 0x000F;
            private const int WM_SIZE = 0x0005;
            private const int SWP_FRAMECHANGED = 0x0020;
            private const int WM_SYSCOMMAND = 0x0112;
            private const int SC_CLOSE = 0xF060;
            private const int SC_MINIMIZE = 0xF020;
            private const int SC_MAXIMIZE = 0xF030;
            private const uint WM_LBUTTONDOWN = 0x0201;
            private const uint WM_LBUTTONUP = 0x0202;
            private const int BM_CLICK = 0xF5;

            private Border WndHoster;
            private double screenW, screenH;
            private System.Diagnostics.Process appProc;
            private uint oldStyle;
            public IntPtr hwndHost;
            private String appPath;
            public EmbeddedApp(Border b, double sW, double sH, String p, String f)
            {
                WndHoster = b;
                screenH = sH;
                screenW = sW;
                appPath = p;
                WinInfo = new WindowInfo();
                WinInfo.winTitle = f;
            }
            protected override HandleRef BuildWindowCore(HandleRef hwndParent)
            {
                hwndHost = FindTheWindow();
                //if (hwndHost == null)   
                //{   
                appProc = new System.Diagnostics.Process();
                appProc.StartInfo.FileName = appPath;
                appProc.Start();
                Thread.Sleep(100);
                hwndHost = FindTheWindow();
                SendMessage(hwndHost, WM_SYSCOMMAND, SC_MINIMIZE, 0);
                //}   
                // 嵌入在HwnHost中的窗口必须要 设置为WS_CHILD风格    
                oldStyle = GetWindowLong(hwndHost, GWL_STYLE);
                uint newStyle = oldStyle;
                //WS_CHILD和WS_POPUP不能同时存在。有些Win32窗口，比如QQ的窗口，有WS_POPUP属性，这样嵌入的时候会导致程序错误   
                newStyle |= WS_CHILD;
                newStyle &= ~WS_POPUP;
                newStyle &= ~WS_BORDER;
                SetWindowLong(hwndHost, GWL_STYLE, newStyle);
                //将窗口居中，实际上是将窗口的容器居中   
                RePosWindow(WndHoster, WndHoster.Width, WndHoster.Height);

                //将netterm的父窗口设置为HwndHost    
                SetParent(hwndHost, hwndParent.Handle);
                return new HandleRef(this, hwndHost);
            }
            protected override void DestroyWindowCore(System.Runtime.InteropServices.HandleRef hwnd)
            {
                SetWindowLong(hwndHost, GWL_STYLE, oldStyle);
                SetParent(hwndHost, (IntPtr)0);
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct RECT
            {
                public int left;
                public int top;
                public int right;
                public int bottom;
            }
            [StructLayout(LayoutKind.Sequential)]
            public struct WindowInfo
            {
                public String winTitle;
                public RECT r;
                public IntPtr hwnd;
            }
            public delegate bool CallBackPtr(IntPtr hwnd, ref WindowInfo WndInfoRef);
            private static CallBackPtr callBackPtr;
            private WindowInfo WinInfo;
            public static bool CallBackProc(IntPtr hwnd, ref WindowInfo WndInfoRef)
            {
                StringBuilder str = new StringBuilder(512);
                GetWindowText(hwnd, str, str.Capacity);
                Console.WriteLine(str.ToString());
                if (str.ToString().IndexOf(WndInfoRef.winTitle, 0) >= 0)
                {
                    WndInfoRef.hwnd = hwnd;
                    GetWindowRect(hwnd, ref (WndInfoRef.r));
                }

                return true;
            }
            public IntPtr FindTheWindow()
            {
                callBackPtr = new CallBackPtr(CallBackProc);
            //EnumWindows(callBackPtr, ref WinInfo);
                IntPtr ParenthWnd = new IntPtr(0);
                ParenthWnd = FindWindow(null, "Communicationport stress test");
                WinInfo.hwnd = ParenthWnd;
                return WinInfo.hwnd;
            }
            public void RePosWindow(Border b, double screenW, double screenH)
            {
                double width = WinInfo.r.right - WinInfo.r.left;
                double height = WinInfo.r.bottom - WinInfo.r.top;

                double left = (screenW - width) / 2;
                double right = (screenW - width) / 2;
                double top = (screenH - height) / 2;
                double bottom = (screenH - height) / 2;

                //b.Margin = new Thickness(left, top, right, bottom);
            }
        }
    }

