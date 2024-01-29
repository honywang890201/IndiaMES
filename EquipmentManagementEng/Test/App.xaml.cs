using Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;

namespace Test
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        internal const string PRIVATE_KEY = "<RSAKeyValue><Modulus>5wlm8TBFktDz/dYiPbYkm5cmhtzC9c0jaXtrL4lyS7cfDsQI1wlSvNu/GdC5Y+HfIxTyKniXOjJyDgBdBu2rzJwiY6wmViGOX5nGzKIF3sKTVJu0fktSbLLwgtwMuAcZikS1/Ddi8oxC3aWaVWemA4AfmdbvO6Vaj7LVDXtYXnU=</Modulus><Exponent>AQAB</Exponent><P>86hiwf9ehwzBwFw6zodrxYLMyaOBJhnTCJhg7/TuDcod2fMSjtzJeTspvoAyaSXqrp9dU4kMevy/fp+BuglE4w==</P><Q>8r1ao5PWPINE9MweIJ6ptir1r1/hAY41Fq4mV9RlxcpNQNeU6nV1XXDQ6BEcJvqa8D+t3gka9yy+BOry3KeGxw==</Q><DP>ok+UYhkEVkBoPQTzY6sQXsUwOE5D9SaUzw/62z406liupAZpYWOwjqKbvzxU2HiaqfKdT81m0/LUebw1xcDw8w==</DP><DQ>Vd+5Ph9h7jx+W6AOlHmtDn46NpXT0yoNC/4GIJJKguOj2umpjByLrcfokADllcCYqZ/Nkbxk5sbUXocD7h3yJw==</DQ><InverseQ>LbY/mgkYSkkBtCOSgyn474gOR1BVzQVCA6+7zeUFq/QGl0H4jR1HLPtO9PfH66lA8q+JrHnWwBbtH5cWPKBRew==</InverseQ><D>bsO3MtLtyCGdmIjdCbEVg/LHacjVP3sGC0A8dyHyRhKZNT8O7eluXVUNHZdQCm7zx6H3KB2Ag0pEHCpYb0XrIthWhXE4gLrLglq5DMcpOxpeXd18A/2lZ2rUB+VhGd0JaZNzW8FzyHS6zkFvVnRlb6BajwkAwvp4ADvkC8E0BdE=</D></RSAKeyValue>";
        internal const string PUBLIC_KEY = "<RSAKeyValue><Modulus>5wlm8TBFktDz/dYiPbYkm5cmhtzC9c0jaXtrL4lyS7cfDsQI1wlSvNu/GdC5Y+HfIxTyKniXOjJyDgBdBu2rzJwiY6wmViGOX5nGzKIF3sKTVJu0fktSbLLwgtwMuAcZikS1/Ddi8oxC3aWaVWemA4AfmdbvO6Vaj7LVDXtYXnU=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

        public App()
        {
            //UI线程的未处理异常
            //Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            //非UI线程抛出的未处理异常
            //AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            /*
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Framework.SysVar.SETTING_FILE_NAME);

            WinAPI.File.INIFileHelper.Write("LogSetting", "IsLog", "True", path);
            WinAPI.File.INIFileHelper.Write("LogSetting", "IsUserQueue", "True", path);
            WinAPI.File.INIFileHelper.Write("LogSetting", "LogPath", "", path);
            WinAPI.File.INIFileHelper.Write("LoginInfo", "ServerUrl", "", path);
            WinAPI.File.INIFileHelper.Write("LoginInfo", "ServerUrlHistory", "http://localhost/Service/Service.svc", path);
            WinAPI.File.INIFileHelper.Write("LoginInfo", "UserCode", "http://localhost/Service/Service.svc", path);
            WinAPI.File.INIFileHelper.Write("LoginInfo", "LoginDateTime", "", path);
            WinAPI.File.INIFileHelper.Write("LoginInfo", "IsDirection", "", path);
            WinAPI.File.INIFileHelper.Write("LoginInfo", "IsSingleton", "", path);
             * */
        }

        #region 未捕获异常处理

        /*
         * 在WPF这种应用程序中，会有两大类未处理异常：
         *      一类是在UI线程抛出来的，例如点击了用户界面上面的某个控件，然后执行某个代码的时候，遇到了异常；
         *      另一类是非UI线程跑出来的，例如在一个多线程的程序里面，工作线程的代码遇到了异常。
         * */
        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            string message = "我们很抱歉，当前应用程序遇到一些问题，请进行重试，如果问题继续存在，请联系管理员！";
            Log.Helper.Write(message + "\r\n" + WinAPI.SysFunction.GetExceptoinMessage(e.Exception), LogType.Fatal);
            Component.MessageBox.MyMessageBox.Show(message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }


        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            /*
             * 这个事件中没有和前面那个事件一样的e.Handled参数，就是说，虽然这样是可以捕捉到非UI线程的异常，而且也可以进行相应的处理，
             * 但是应用程序还是会退出，也就是说这个异常还是被当作是未处理异常继续汇报给Runtime。
             * 
             * 
             * 为了改进这一点，我们可以通过修改配置文件来实现。
             * <?xml version="1.0" encoding="utf-8" ?>
                <configuration>
                  <runtime>
                    <legacyUnhandledExceptionPolicy enabled="1"/>
                  </runtime>
                  <startup>
                    <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.5" />
                  </startup>
                </configuration>
             * */
            string message = "我们很抱歉，当前应用程序遇到一些问题，程序被迫关闭，请重新登录操作，如果问题继续存在，请联系管理员！";
            Log.Helper.Write(message + "\r\n" + e.ExceptionObject == null ? "" : e.ExceptionObject.ToString(), LogType.Fatal);
            Component.MessageBox.MyMessageBox.Show(message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        #endregion

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);


            //获取Devexpress所有主题
            // System.Collections.ObjectModel.ReadOnlyCollection<DevExpress.Xpf.Core.Theme> themes = DevExpress.Xpf.Core.Theme.Themes;
            //设置主题
            //  DevExpress.Xpf.Core.ThemeManager.ApplicationThemeName = themes[21].Name; 
            // this.UpdateLayout();

            DevExpress.Xpf.Core.ThemeManager.ApplicationThemeName = "Office2010Silver";
            Framework.SysVar.SetServiceInfo(PRIVATE_KEY, PUBLIC_KEY);
        }
        /// <summary>
        ///  互拆体，必须定义为类变量，如果在方法里面定义，方法执行完后Mutex会回收，导致没有效果
        /// </summary>
        Mutex mutex;
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            int IsUpdateRun = 0;
            if (e.Args != null && e.Args.Count() == 1)  //有参数，有可能是更新处理
            {
                int.TryParse(e.Args[0], out IsUpdateRun);
            }
            ReadDefaultMessage();


            #region 单例模式处理
            if (IsUpdateRun == 1 || Framework.App.IsClientSingletonPattern)
            {
                bool createdNew = false;
                mutex = new Mutex(true, "SingletonWinAppMutex", out createdNew);
                if (!createdNew)
                {
                    Process instance = GetExistProcess();
                    if (instance != null)
                    {
                        WinAPI.SysFunction.SetForegroud(instance);
                    }
                    else
                    {
                        string message = "没有发现运行的程序...";
                        Log.Helper.Write(message, LogType.Error);
                        Component.MessageBox.MyMessageBox.Show(message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    DB.ConnectionManager.Clear();
                    System.Windows.Application.Current.Shutdown();
                    return;
                }
            }

            #endregion
        }

        private void ReadDefaultMessage()
        {
            string info = null;
            bool b;

            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Framework.SysVar.SETTING_FILE_NAME);

            if (System.IO.File.Exists(path))
            {
                try
                {
                    info = WinAPI.File.INIFileHelper.Read("LoginInfo", "IsDirection", path, "");
                    if (bool.TryParse(info, out b))
                    {
                        if (b)
                        {
                            DB.DBHelper.IsDirectionDB = true;
                        }
                        else
                        {
                            DB.DBHelper.IsDirectionDB = false;
                        }
                    }
                }
                catch { }

                try
                {
                    info = WinAPI.File.INIFileHelper.Read("LoginInfo", "IsSingleton", path, "");
                    if (bool.TryParse(info, out b))
                    {
                        Framework.App.IsClientSingletonPattern = b;
                    }
                }
                catch { }
            }

        }

        /// <summary>       
        /// /// 查看应用程序是否已经运行    
        /// /// </summary>        
        /// <returns></returns>     
        public static Process GetExistProcess()
        {
            Process pro = Process.GetCurrentProcess();
            foreach (Process process in Process.GetProcessesByName(pro.ProcessName))
            {
                if ((process.Id != pro.Id) &&
                    (Assembly.GetExecutingAssembly().Location == pro.MainModule.FileName))
                {
                    return process;
                }
            }
            return null;
        }

        public static void SysExit()
        {
            DB.ConnectionManager.Clear();
            System.Windows.Application.Current.Shutdown();
        }
    }
}
