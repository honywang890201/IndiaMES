using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace CartonQRPrint
{
    
    class PrintToPDF
    {
        [DllImport("user32.dll", EntryPoint = "PostMessage")]
        public static extern int PostMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);
        public const int WM_SYSCOMMAND = 0x112;
        public const int SC_MINIMIZE = 0xF020;
        public const int SC_MAXIMIZE = 0xF030;
        public const int WM_CLOSE = 0x0010;
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, string lParam);
        [DllImport("User32.dll ")]
        public static extern IntPtr FindWindowEx(IntPtr parent, IntPtr childe, string strclass, string FrmText);
        [DllImport("user32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);

        public void PrintPDF(string FilePath,int num)
        {
            IntPtr hwnd = IntPtr.Zero;
            while (FindWindow(null, "打印成PDF文件-福昕PDF打印机")== IntPtr.Zero) ;
            System.Threading.Thread.Sleep(1000);
            WinAPI.SysFunction.KillProcessByName("FoxitReader");
                hwnd = FindWindow(null, "打印成PDF文件-福昕PDF打印机");
                IntPtr btn = FindWindowEx(hwnd, (IntPtr)0, "Button", "保存(&S)");
                IntPtr hwnd1 = FindWindowEx(hwnd, (IntPtr)0, "ComboBoxEx32", null);
                IntPtr hwnd2 = FindWindowEx(hwnd1, (IntPtr)0, "ComboBox", null);
                IntPtr hwnd3 = FindWindowEx(hwnd2, (IntPtr)0, "Edit", null);
                SendMessage(hwnd3, 0x000C, (IntPtr)4096, FilePath.Replace("/", "\\"));
                System.Threading.Thread.Sleep(50);
                SendMessage(btn, 0x0201, (IntPtr)0, null);// {按下鼠标左键}
                SendMessage(btn, 0x0202, (IntPtr)0, null);// {抬起鼠标左键}
                System.Threading.Thread.Sleep(50);
                SendMessage(hwnd3, 0x000C, (IntPtr)4096, num.ToString());
                System.Threading.Thread.Sleep(50);
                SendMessage(btn, 0x0201, (IntPtr)0, null);// {按下鼠标左键}
                SendMessage(btn, 0x0202, (IntPtr)0, null);// {抬起鼠标左键}
            
        }



    }
}
