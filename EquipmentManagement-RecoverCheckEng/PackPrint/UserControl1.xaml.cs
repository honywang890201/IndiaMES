using Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WinAPI;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Interop;
using System.Windows.Forms.Integration;
using System.Threading;
using RecoverCheck;

namespace PackPrint
{
     /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class UserControl1 : Component.Controls.User.UserVendor
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int SetParent(IntPtr hWndChild, IntPtr hWndNewParent);


        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool MoveWindow(IntPtr hwnd, int x, int y, int cx, int cy, bool repaint);

        [DllImport("user32.dll ", EntryPoint = "ShowWindow")]
        public static extern int ShowWindow(IntPtr hwnd, int nCmdShow);

        private string setting_path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Framework.SysVar.SETTING_FILE_NAME);
        private DataRowView SelectRow = null;

        private Dictionary<long, Printer.Instance> TemplatePath = new Dictionary<long, Printer.Instance>();
        private List<ComboBox> PrinterComboBoxs = new List<ComboBox>();

        private bool _IsLoad = false;
        string MESServerIp = null, FactoryServerIp = null;


        DataTable table = null;
        int DataCount = 0;
        /////////////////////////////////////////////////////////////////////////////////////////////////////
        MySqlDataReader reader = null;
        String mysqlStr = null; //"Database=ott_package;Data Source=192.168.0.119;User Id=root;Password=123456;pooling=false;CharSet=utf8;port=3306";
        MySqlConnection mysql = null;// new MySqlConnection(mysqlStr);
        MySqlCommand mySqlCommand = null;// new MySqlCommand("SELECT COUNT(*)FROM restore_order_all_t", mysql);
                                         /////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 将外进程嵌入到当前程序
        /// </summary>
        /// <param name="process"></param>
        private bool EmbedApp(Process process)
        {
            //是否嵌入成功标志，用作返回值
            bool isEmbedSuccess = false;
            //外进程句柄
            IntPtr processHwnd = process.MainWindowHandle;
            //容器句柄
            IntPtr panelHwnd = new System.Windows.Forms.Panel().Handle;

            if (processHwnd != (IntPtr)0 && panelHwnd != (IntPtr)0)
            {
                //把本窗口句柄与目标窗口句柄关联起来
                int setTime = 0;
                //while (!isEmbedSuccess && setTime < 10)
                //{
                //    isEmbedSuccess = SetParent(processHwnd, panelHwnd) != 0);
                //    Thread.Sleep(100);
                //    setTime++;
                //}
                //设置初始尺寸和位置
               // Win32Api.MoveWindow(_process.MainWindowHandle, 0, 0, (int)ActualWidth, (int)ActualHeight, true);
            }

            //if (isEmbedSuccess)
            //{
            //    _embededWindowHandle = _process.MainWindowHandle;
            //}

            return isEmbedSuccess;
        }
        public UserControl1(Framework.SystemAuthority authority) :
            base(authority)
        {
            InitializeComponent();

            root.Background = new ImageBrush(WinAPI.File.ImageHelper.ConvertToImageSource(Component.App.BackgroudImage));


        }


        private void UserVendor_Loaded(object sender, RoutedEventArgs e)
        {
           
            if (_IsLoad)
                return;
            _IsLoad = true;
            Match m = Regex.Match(Framework.App.ServiceUrl, @"\d{1,3}.\d{1,3}.\d{1,3}.\d{1,3}");
            if (m.Success)
            {
                MESServerIp = m.Value;
            }
            string[] arr = MESServerIp.Split('.');
            if (arr[2] == "0")
            {
                FactoryServerIp = "192.168.0.24";
            }
            if (arr[2] == "2")
            {
                FactoryServerIp = "192.168.2.11";
            }
            table = new DataTable();
            table.Columns.Add("Serial", typeof(string));
            table.Columns.Add("SN", typeof(string));
            table.Columns.Add("DateTime", typeof(string));
            table.Columns.Add("Worker", typeof(string));
            serial.Text = string.Empty;
            serial.Focus();
            mysqlStr = "Database=ott_package;Data Source=" + FactoryServerIp + ";User Id=root;Password=123456;pooling=false;CharSet=utf8;port=3306";
            mysql = new MySqlConnection(mysqlStr);


        }
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //String path = @"C:\Users\wang\Desktop\serialport_demo\Data\Programming\Microsoft\Visual C++\CommTest\Debug\commtest.exe";
                //EmbeddedApp ea = new EmbeddedApp(wa, 100, 100, path, "Communicationport stress test");
                //wa.Child = ea;
                //return;
                int Count = 0;
                //Component.Windows.AuthorityLogin login = new Component.Windows.AuthorityLogin(MenuId, '4');
                //login.Owner = Component.App.Portal;
                //if (!login.ShowDialog().Value)
                //{
                //    return;
                //}
                mySqlCommand = new MySqlCommand("SELECT COUNT(*)FROM restore_order_all_t where SerialNum='" + serial.Text + "'", mysql);
                try
                {
                    mysql.Open();
                    Count = Convert.ToInt32(mySqlCommand.ExecuteScalar().ToString());
                }catch(Exception)
                {
                    ;
                }
                if (Count>0)
                {
                    MessageBox.Show("已经确认过！！！");
                    serial.Text = string.Empty;
                    serial.Focus();
                    mysql.Close();
                    return;
                }
                mySqlCommand = new MySqlCommand("INSERT restore_order_all_t(SerialNum, WorkerNumber, CreateDate)VALUES('"+ serial.Text+"','" + Framework.App.User.UserCode+ "','"+ DateTime.Now.ToLocalTime().ToString() + "')", mysql);
                try
                {
                    mySqlCommand.ExecuteNonQuery();
                }
                catch(Exception)
                {
                    MessageBox.Show("插入失败!!!");
                    serial.Text = string.Empty;
                    serial.Focus();
                    mysql.Close();

                }
                mysql.Close();
                DataRow newRow;
                newRow = table.NewRow();
                newRow["Serial"] = DataCount.ToString();
                newRow["SN"] = serial.Text;
                newRow["DateTime"] = DateTime.Now.ToLocalTime().ToString();
                newRow["Worker"] = Framework.App.User.UserCode;
                table.Rows.Add(newRow);
                DataCount++;
                Result.ItemsSource = table.DefaultView;
                serial.Text = string.Empty;
                serial.Focus();

            }
        }

       
    }
}

       
      

       
