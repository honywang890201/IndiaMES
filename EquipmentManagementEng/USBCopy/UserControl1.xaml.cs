/*
 软件修改记录
 2020.04.28： txtScan1_KeyDown，修改兼容华电打印机身及彩盒时要扫描二维码。二维码同时带有mac及sn，格式为mac_sn,因为这个sn不在系统内，所以在扫描到之后要丢弃
 
 */



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
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Microsoft.Win32;
using System.Diagnostics;
using System.Threading;
using Com.FirstSolver.USB;
using System.Net;
using Microsoft.VisualBasic.Devices;
using SeasideResearch.LibCurlNet;
namespace USBCopy
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class UserControl1 : Component.Controls.User.UserVendor
    {
        private string setting_path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Framework.SysVar.SETTING_FILE_NAME);
        private DataRowView SelectRow = null;

        private Dictionary<long, Printer.Instance> TemplatePath = new Dictionary<long, Printer.Instance>();
        private List<ComboBox> PrinterComboBoxs = new List<ComboBox>();

        private bool _IsLoad = false;
        private bool IsENOnlineAssembly = false;
        string MESServerIp = null, FactoryServerIp=null;
        string[] Drive = new string[10];
        Int32 DriveCount = 0;
        Thread my = null;
        List<string> driverpath = new List<string>();
        List<string> FixDriveName = new List<string>();
            /////////////////////////////////////////////////////////////////////////////////////////////////////
        MySqlDataReader reader = null;
        String mysqlStr = null; //"Database=ott_package;Data Source=192.168.0.119;User Id=root;Password=123456;pooling=false;CharSet=utf8;port=3306";
        MySqlConnection mysql = null;// new MySqlConnection(mysqlStr);
        MySqlCommand mySqlCommand = null;// new MySqlCommand("SELECT COUNT(*)FROM restore_order_all_t", mysql);
        //华电识别用，true时为华电
        private bool IsHuadian = false;
        private bool IsDrPengBuDing = false;//鹏博士小布丁项目判断标志，用于获取鹏博士烧号数据中的WiFi mac并上传至mes方便后期报盘文件导出
        private long MOId = -1;
        string CopyPath = string.Empty;
        HwndSource hwndSource;
        HostControllerInfo[] p;
        /////////////////////////////////////////////////////////////////////////////////////////////////////
        string panfu = "";
        public const int WM_DEVICECHANGE = 0x219;//U盘插入后，OS的底层会自动检测到，然后向应用程序发送“硬件设备状态改变“的消息
        public const int DBT_DEVICEARRIVAL = 0x8000;  //就是用来表示U盘可用的。一个设备或媒体已被插入一块，现在可用。
        public const int DBT_CONFIGCHANGECANCELED = 0x0019;  //要求更改当前的配置（或取消停靠码头）已被取消。
        public const int DBT_CONFIGCHANGED = 0x0018;  //当前的配置发生了变化，由于码头或取消固定。
        public const int DBT_CUSTOMEVENT = 0x8006; //自定义的事件发生。 的Windows NT 4.0和Windows 95：此值不支持。
        public const int DBT_DEVICEQUERYREMOVE = 0x8001;  //审批要求删除一个设备或媒体作品。任何应用程序也不能否认这一要求，并取消删除。
        public const int DBT_DEVICEQUERYREMOVEFAILED = 0x8002;  //请求删除一个设备或媒体片已被取消。
        public const int DBT_DEVICEREMOVECOMPLETE = 0x8004;  //一个设备或媒体片已被删除。
        public const int DBT_DEVICEREMOVEPENDING = 0x8003;  //一个设备或媒体一块即将被删除。不能否认的。
        public const int DBT_DEVICETYPESPECIFIC = 0x8005;  //一个设备特定事件发生。
        public const int DBT_DEVNODES_CHANGED = 0x0007;  //一种设备已被添加到或从系统中删除。
        public const int DBT_QUERYCHANGECONFIG = 0x0017;  //许可是要求改变目前的配置（码头或取消固定）。
        public const int DBT_USERDEFINED = 0xFFFF;  //此消息的含义是用户定义的
        public const uint GENERIC_READ = 0x80000000;
        public const int GENERIC_WRITE = 0x40000000;
        public const int FILE_SHARE_READ = 0x1;
        public const int FILE_SHARE_WRITE = 0x2;
        public const int IOCTL_STORAGE_EJECT_MEDIA = 0x2d4808;
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr CreateFile(
        string lpFileName,
        uint dwDesireAccess,
        uint dwShareMode,
        IntPtr SecurityAttributes,
        uint dwCreationDisposition,
        uint dwFlagsAndAttributes,
        IntPtr hTemplateFile);

        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool DeviceIoControl(
            IntPtr hDevice,
            uint dwIoControlCode,
            IntPtr lpInBuffer,
            uint nInBufferSize,
            IntPtr lpOutBuffer,
            uint nOutBufferSize,
            out uint lpBytesReturned,
            IntPtr lpOverlapped
        );
      
        public UserControl1(Framework.SystemAuthority authority) :
            base(authority)
        {
            InitializeComponent();

            root.Background = new ImageBrush(WinAPI.File.ImageHelper.ConvertToImageSource(Component.App.BackgroudImage));

            tbProcess.Text = Framework.App.Resource.StationCode;
            if (!string.IsNullOrEmpty(Framework.App.Resource.StationDesc))
                tbProcess.Text = tbProcess.Text + "/" + Framework.App.Resource.StationDesc;

            tbLine.Text = Framework.App.Resource.LineCode;
            if (!string.IsNullOrEmpty(Framework.App.Resource.LineDesc))
                tbLine.Text = tbLine.Text + "/" + Framework.App.Resource.LineDesc;

            tbUser.Text = Framework.App.User.UserCode;
            if (!string.IsNullOrEmpty(Framework.App.User.UserDesc))
                tbUser.Text = tbUser.Text + "/" + Framework.App.User.UserDesc;
        }


        private void UserVendor_Loaded(object sender, RoutedEventArgs e)
        {
            
            ////////////////////////////////////////////////////////////////////////////////////////
            hwndSource = PresentationSource.FromVisual(this) as HwndSource;//窗口过程
            if (hwndSource != null)
            {
                hwndSource.AddHook(new HwndSourceHook(WndProc));//挂钩
            }

           
            //////////////////////////////////////////////////////////////////////////////////////////////////
            //MessageBox.Show("StationId:" + Framework.App.Resource.StationId.ToString() + "ShiftTypeId:" + Framework.App.Resource.ShiftTypeId.ToString() + "LineId:" + Framework.App.Resource.LineId.ToString()
            //   + "ResourceId:" + Framework.App.Resource.ResourceId.ToString() + "UserId:" + Framework.App.User.UserId.ToString());
            if (_IsLoad)
                return;
            _IsLoad = true;
            DeleteFolder1("Template/");
            FixDriveName.Add("C:\\");
            FixDriveName.Add("D:\\");
            FixDriveName.Add("E:\\");
            FixDriveName.Add("F:\\");
            FixDriveName.Add("G:\\");
            FixDriveName.Add("H:\\");
            FixDriveName.Add("I:\\");
            FixDriveName.Add("J:\\");
            FixDriveName.Add("K:\\");
            FixDriveName.Add("L:\\");
            FixDriveName.Add("M:\\");
            FixDriveName.Add("N:\\");
            FixDriveName.Add("O:\\");
            FixDriveName.Add("P:\\");
            FixDriveName.Add("Q:\\");
            FixDriveName.Add("R:\\");
            FixDriveName.Add("S:\\");
            FixDriveName.Add("T:\\");
            FixDriveName.Add("U:\\");
            FixDriveName.Add("V:\\");
            FixDriveName.Add("W:\\");
            FixDriveName.Add("X:\\");
            FixDriveName.Add("Y:\\");
            FixDriveName.Add("Z:\\");
            btnSelectorMO_Click(btnSelectorMO, null);
        }
        private void LoadMO()
        {
            Microsoft.VisualBasic.Devices.Computer MyComputer = new Microsoft.VisualBasic.Devices.Computer();
            btnAddPrint.Visibility = Visibility.Collapsed;
            CloseTemplate();
            TemplatePath.Clear();
            PrinterComboBoxs.Clear();
            if (SelectRow == null)
            {
                tbMOCode.Text = string.Empty;
                tbQty.Text = string.Empty;
                tbItemCode.Text = string.Empty;
                tbItemSpec.Text = string.Empty;
                tbWorkflow.Text = string.Empty;
                tbCustomer.Text = string.Empty;
                IsENOnlineAssembly = false;
                btnAddPrint.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (SelectRow["ENType"].ToString().ToUpper() == "No".ToUpper())
                {
                    
                    IsENOnlineAssembly = false;
                }
                else
                {
                    
                    if (SelectRow["ENType"].ToString().ToUpper() == "OnLine".ToUpper())
                    {

                        IsENOnlineAssembly = true;
                    }
                    else
                    {

                        IsENOnlineAssembly = false;
                    }
                    
                }
                tbMOCode.Text = SelectRow["MOCode"].ToString();
                tbQty.Text = SelectRow["Qty"].ToString();
                tbItemCode.Text = SelectRow["ItemCode"].ToString();
                tbItemSpec.Text = SelectRow["ItemSpecification"].ToString();
                tbWorkflow.Text = SelectRow["WorkflowCode"].ToString();
                tbCustomer.Text = SelectRow["CustomerCode"].ToString();
                if (!string.IsNullOrEmpty(SelectRow["CustomerDesc"].ToString().Trim()))
                    tbCustomer.Text = tbCustomer.Text + "/" + SelectRow["CustomerDesc"].ToString();
                if (!string.IsNullOrEmpty(SelectRow["WorkflowDesc"].ToString().Trim()))
                    tbWorkflow.Text = tbWorkflow.Text + "/" + SelectRow["WorkflowDesc"].ToString();
                if (SelectRow["WorkflowId"] == null || SelectRow["WorkflowId"].ToString().Trim() == string.Empty)
                {
                    txtMessage.AddMessage(string.Format("工单[{0}]未设置工作流！", SelectRow["MOCode"]), true);
                }

                string sql = @"SELECT Bas_MO_Template.MOTemplateId AS TemplateId,
	   Bas_MO_Template.TemplateDesc,
	   Bas_MO_Template.Copies,
	   Set_File.FileId,
	   Set_File.FileServerId,
	   Bas_MO_Template.Sequence
FROM dbo.Bas_MO_Template   WITH(NOLOCK) 
LEFT JOIN dbo.Set_File  WITH(NOLOCK) ON Bas_MO_Template.TemplateId=Set_File.FileId
WHERE MOId=@MOId AND OPId=@OPId  ORDER BY Sequence";
                Parameters parameters = new Parameters();
                parameters.Add("MOId", SelectRow["MOId"]);
                parameters.Add("OPId", Framework.App.Resource.StationId);

                try
                {
                    DataTable source = DB.DBHelper.GetDataTable(sql, parameters, ExecuteType.Text);
                    if (source.Rows.Count < 1)
                    {
                        sql = @"SELECT Bas_Item_Template.ItemTemplateId AS TemplateId,
	   Bas_Item_Template.TemplateDesc,
	   Bas_Item_Template.Copies,
	   Set_File.FileId,
	   Set_File.FileServerId,
	   Bas_Item_Template.Sequence
FROM dbo.Bas_Item_Template   WITH(NOLOCK) 
LEFT JOIN dbo.Set_File  WITH(NOLOCK) ON Bas_Item_Template.TemplateId=Set_File.FileId
WHERE ItemId=@ItemId AND OPId=@OPId  ORDER BY Sequence";
                        parameters = new Parameters();
                        parameters.Add("ItemId", SelectRow["ItemId"]);
                        parameters.Add("OPId", Framework.App.Resource.StationId);
                        source = DB.DBHelper.GetDataTable(sql, parameters, ExecuteType.Text);
                    }

                    if (source.Rows.Count > 0)
                    {
                       // gridTemplate.RowDefinitions.Add(new RowDefinition());
                        TextBlock block = new TextBlock();
                        block.Text = "选择打印机";
                        block.FontSize = 16;
                        block.FontWeight = FontWeights.Bold;
                        block.HorizontalAlignment = HorizontalAlignment.Center;
                        //block.SetValue(Grid.RowProperty, gridTemplate.RowDefinitions.Count - 1);
                        block.SetValue(Grid.ColumnProperty, 1);
                        //gridTemplate.Children.Add(block);

                        //if(Authority!=null&&Authority.IsAddPrint)
                        {
                            btnAddPrint.Visibility = Visibility.Visible;
                        }

                    }
                    else
                    {
                        btnAddPrint.Visibility = Visibility.Collapsed;
                    }
                    WinAPI.SysFunction.KillProcessByName("lppa");
                    foreach (DataRow row in source.Rows)
                    {
                        byte[] buffer = null;
                        Component.Controls.User.FileHelper helper = null;
                        try
                        {
                            helper = new Component.Controls.User.FileHelper();
                            helper.Init((long)row["FileId"]);
                            FtpDownLoad m = new FtpDownLoad();
                            m.mFtpDownLoad(helper.FileServerHelper.Address+ helper.ServerFileName, "Template/", helper.FileName, helper.FileServerHelper.FileServerUser, helper.FileServerHelper.FileServerPwd);
                            //Download(helper.ServerFileName,helper.FileServerHelper.FileServerUser, helper.FileServerHelper.FileServerPwd, helper.FileServerHelper.Address, "Template/");
                            //MyComputer.FileSystem.RenameFile("Template/" + helper.ServerFileName, helper.FileName);
                            DeCompressRar("Template/" + helper.FileName, "Template/");
                            File.Delete("Template/" + helper.FileName);
                            string a = "Template/" + helper.FileName.Substring(0, helper.FileName.IndexOf("."));

                            foreach (string d in Directory.GetFileSystemEntries(a))
                            {
                                Postion.Items.Add(d);
                            }
                            //if (helper.FileServerHelper != null)
                            //{
                            //    buffer = helper.FileServerHelper.WriteFile("1.rar");
                            //}
                        }
                        catch (Exception ex)
                        {
                            txtMessage.AddMessage("文件传输时内存溢出", false);
                            txtMessage.AddMessage(ex.Message, true);
                            continue;
                        }
                        //if (buffer == null)
                        //    continue;
                        //string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Template", helper.FileName);
                        //try
                        //{
                        //    WinAPI.File.FileHelper.Write(buffer, path);
                        //    DeCompressRar("Template/"+ helper.FileName, "Template/");
                        //    File.Delete("Template/" + helper.FileName);
                        //    string a = "Template/" + helper.FileName.Substring(0, helper.FileName.IndexOf("."));
                            
                        //    foreach (string d in Directory.GetFileSystemEntries(a))
                        //    {
                        //        Postion.Items.Add(d);
                        //    }
                        //}
                        //catch (Exception ex)
                        //{
                        //    txtMessage.AddMessage("文件写入时内存溢出", false);
                        //    txtMessage.AddMessage(ex.Message, true);
                        //    continue;
                        //}
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        public class City
        {
            public int ID { get; set; }
            public string Name { get; set; }
        }
        private void btnSelectorMO_Click(object sender, RoutedEventArgs e)
        {
            DeleteFolder1("Template/");
            Postion.Items.Clear();
            Component.Windows.MOSelector selector = new Component.Windows.MOSelector(MenuId);
            selector.Owner = Component.App.Portal;
            if (selector.ShowDialog().Value)
            {
                string sql = @"SELECT Bas_MO.MOId,
	   Bas_MO.ItemId,
	   Bas_MO.WorkflowId,
	   Bas_MO.CustomerId,
	   Bas_MO.MOCode,
	   Bas_MO.Qty,
	   Bas_Item.ItemCode,
	   Bas_Item.ItemSpecification,
	   Bas_Workflow.WorkflowCode,
	   Bas_Workflow.WorkflowDesc,
	   Bas_Customer.CustomerCode,
	   Bas_Customer.CustomerDesc,
	   Bas_Item.ENType
FROM dbo.Bas_MO  WITH(NOLOCK) 
LEFT JOIN dbo.Bas_Workflow  WITH(NOLOCK) ON dbo.Bas_Workflow.WorkflowId = dbo.Bas_MO.WorkflowId
LEFT JOIN dbo.Bas_Item  WITH(NOLOCK) ON dbo.Bas_Item.ItemId = dbo.Bas_MO.ItemId
LEFT JOIN dbo.Bas_Customer  WITH(NOLOCK) ON dbo.Bas_Customer.CustomerId = dbo.Bas_MO.CustomerId
WHERE Bas_MO.MOId=@MOId";

                Parameters parameters = new Parameters();
                parameters.Add("MOId", selector.moId);

                try
                {
                    DataTable table = DB.DBHelper.GetDataTable(sql, parameters, ExecuteType.Text, null, true);
                    if (table.Rows.Count < 1)
                    {
                        MessageBox.Show("工单错误", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    SelectRow = table.DefaultView[0];
                    MOId = selector.moId;
                    LoadMO();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
        }

        private void CloseTemplate()
        {
            WinAPI.SysFunction.KillProcessByName("lppa");
            foreach (Printer.Instance cs in TemplatePath.Values)
            {
                try
                {
                    if (cs != null)
                    {
                        cs.Close();

                        try
                        {
                            System.IO.File.Delete(cs.PrintTemplate);
                        }
                        catch { }

                        if (!string.IsNullOrEmpty(cs.PrintTemplate) && cs.PrintTemplate.ToLower().EndsWith(".lab"))
                        {
                            string path = null;
                            try
                            {
                                path = cs.PrintTemplate.Substring(0, cs.PrintTemplate.Length - 3);
                                path = path + "bak";
                                System.IO.File.Delete(path);
                            }
                            catch { }
                        }
                    }
                }
                catch// (Exception ex)
                {
                    //MessageBox.Show(ex.Message);
                    //txtMessage.AddMessage(ex.Message, true);
                }
            }

            GC.Collect();
        }
        private void SetErrorOrSuccImage(bool IsError, bool IsHidden = false)
        {
            if (IsHidden)
            {
                imgResult.Visibility = Visibility.Collapsed;
                return;
            }
            if (IsError)
                imgResult.Source = WinAPI.File.ImageHelper.ConvertToImageSource(Component.App.ErrorImage);
            else
                imgResult.Source = WinAPI.File.ImageHelper.ConvertToImageSource(Component.App.CorrectImage);
            imgResult.Visibility = Visibility.Visible;

            System.Windows.Media.Animation.DoubleAnimation widthAnimation = null;
            System.Windows.Media.Animation.DoubleAnimation heightAnimation = null;
            imgResult.BeginAnimation(System.Windows.Controls.Image.WidthProperty, widthAnimation);
            imgResult.BeginAnimation(System.Windows.Controls.Image.HeightProperty, heightAnimation);
            if (IsError)
            {
                widthAnimation = new System.Windows.Media.Animation.DoubleAnimation()
                {
                    To = 350,
                    Duration = TimeSpan.FromMilliseconds(500),
                    RepeatBehavior = new System.Windows.Media.Animation.RepeatBehavior(3),
                    AutoReverse = true
                };
                heightAnimation = new System.Windows.Media.Animation.DoubleAnimation()
                {
                    To = 350,
                    Duration = TimeSpan.FromMilliseconds(500),
                    RepeatBehavior = new System.Windows.Media.Animation.RepeatBehavior(3),
                    AutoReverse = true
                };
            }
            imgResult.BeginAnimation(System.Windows.Controls.Image.WidthProperty, widthAnimation);
            imgResult.BeginAnimation(System.Windows.Controls.Image.HeightProperty, heightAnimation);

        }

      



        


    

        private void btnAddPrint_Click(object sender, RoutedEventArgs e)
        {
            if (SelectRow == null)
            {
                MessageBox.Show("请先选择工单！");
                return;
            }
            Component.Windows.AuthorityLogin login = new Component.Windows.AuthorityLogin(MenuId, '4');
            login.Owner = Component.App.Portal;
            if (login.ShowDialog().Value)
            {
                //AddPrint p = new AddPrint(cmbType1.SelectedValue == null ? null : cmbType1.SelectedValue.ToString(), this, (long)SelectRow["MOId"], login.UserId);
               // p.Owner = Component.App.Portal;
                //p.ShowDialog();
            }
        }

        public void AddMessage(string message, bool isError)
        {
            txtMessage.AddMessage(message, isError);

            SetErrorOrSuccImage(isError, false);
        }

  
       

        private  void DeleteFolder1(string dir)
        {
            foreach (string d in Directory.GetFileSystemEntries(dir))
            {
                string LeftTS = dir + "ovtfac_left_soundtrack.ts";
                string RightTS = dir + "ovtfac_right_soundtrack.ts";
                string SYSPATH = dir + "System Volume Information";
                if (d==LeftTS||d== RightTS||d== SYSPATH)
                {
                    ;
                }
                else
                {

                    if (File.Exists(d))
                    {
                        FileInfo fi = new FileInfo(d);
                        if (fi.Attributes.ToString().IndexOf("ReadOnly") != -1)
                            fi.Attributes = FileAttributes.Normal;
                        //txtMessage.AddMessage("正在删除文件:" + d, false);
                        File.Delete(d);//直接删除其中的文件  
                    }
                    else
                        DeleteFolder(d);////递归删除子文件夹
                    try
                    {
                        Directory.Delete(d, true);
                    }
                    catch
                    {
                        ;
                    }
                }
            }
        }
        private  void DeleteFolder(string dir)
        {
            foreach (string d in Directory.GetFileSystemEntries(dir))
            {
                if (File.Exists(d))
                {
                    FileInfo fi = new FileInfo(d);
                    if (fi.Attributes.ToString().IndexOf("ReadOnly") != -1)
                        fi.Attributes = FileAttributes.Normal;
                    //txtMessage.AddMessage("正在删除文件:" + d, false);
                    File.Delete(d);//直接删除其中bai的文件  
                }
                else
                {
                    DirectoryInfo d1 = new DirectoryInfo(d);
                    if (d1.GetFiles().Length != 0)
                    {
                        DeleteFolder(d1.FullName);////递归删除子文件夹
                    }
                    //txtMessage.AddMessage("正在删除文件夹:"+ d, false);
                    Directory.Delete(d,true);
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
              string result = string.Empty;

                string key = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\WinRAR.exe";
                RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(key);
                if (registryKey != null)
                {
                    result = registryKey.GetValue("").ToString();
                }
                registryKey.Close();
            if (result != string.Empty)
            {
                DeCompressRar("Template/apk_安广厂测_0522.zip", "Template/");
            }
            File.Delete("Template/apk_安广厂测_0522.zip");
                //return result;
            

        }

        /// <summary>
        /// 将格式为rar的压缩文件解压到指定的目录
        /// </summary>
        /// <param name="rarFileName">要解压rar文件的路径</param>
        /// <param name="saveDir">解压后要保存到的目录</param>
        public static void DeCompressRar(string rarFileName, string saveDir)
        {
            string regKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\WinRAR.exe";
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(regKey);
            if (registryKey == null)
            {
                MessageBox.Show("本机还没有安装解压软件-WinRAR，请安装后再试");
                return;
            }
            string winrarPath = registryKey.GetValue("").ToString();
            registryKey.Close();
            string winrarDir = System.IO.Path.GetDirectoryName(winrarPath);
            String commandOptions = string.Format("x {0} {1} -y", rarFileName, saveDir);
            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = System.IO.Path.Combine(winrarDir, "winrar.exe");
            processStartInfo.Arguments = commandOptions;
            processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            Process process = new Process();
            process.StartInfo = processStartInfo;
            process.Start();
            process.WaitForExit();
            process.Close();
        }
        private void updte(string path,string value,Color p)
        {
            int pp = FixDriveName.IndexOf(path);
            switch (pp)
            {
                case 0:
                    {
                        R1.Text = path+"盘:"+ value;
                        R1.Background = new SolidColorBrush(p);
                    }
                    break;
                case 1:
                    {
                        R2.Text = path + "盘:" + value;
                        R2.Background = new SolidColorBrush(p);
                    }
                    break;
                case 2:
                    {
                        R3.Text = path + "盘:" + value;
                        R3.Background = new SolidColorBrush(p);
                    }
                    break;
                case 3:
                    {
                        R4.Text = path + "盘:" + value;
                        R4.Background = new SolidColorBrush(p);
                    }
                    break;
                case 4:
                    {
                        R5.Text = path + "盘:" + value;
                        R5.Background = new SolidColorBrush(p);
                    }
                    break;
                case 5:
                    {
                        R6.Text = path + "盘:" + value;
                        R6.Background = new SolidColorBrush(p);
                    }
                    break;
                case 6:
                    {
                        R7.Text = path + "盘:" + value;
                        R7.Background = new SolidColorBrush(p);
                    }
                    break;
                case 7:
                    {
                        R8.Text = path + "盘:" + value;
                        R8.Background = new SolidColorBrush(p);
                    }
                    break;
            }
            
            
        }
        private delegate void outputDelegate(string msg,string value,Color p);

        public void USBCopy(object b)
        {
            
            if (CopyPath == string.Empty)
            {
                MessageBox.Show("还没有选择工位，请先选择工位后再插入U盘！！！");
                return;
            }
            DeleteFolder1(b.ToString());
            txtMessage.Dispatcher.Invoke(new outputDelegate(updte), b.ToString(), "正在拷贝", Color.FromRgb(255,255,0));
            CopyDirectory(CopyPath, b.ToString(), true);
            txtMessage.Dispatcher.Invoke(new outputDelegate(updte), b.ToString(), "完成,已弹出", Color.FromRgb(0,255,0));
            string filename = @"\\.\" + b.ToString().Remove(2);
            //打开设备，得到设备的句柄handle.
            IntPtr handle = CreateFile(filename, GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, IntPtr.Zero, 0x3, 0, IntPtr.Zero);
            // 向目标设备发送设备控制码，也就是发送命令。IOCTL_STORAGE_EJECT_MEDIA  弹出U盘。
            uint byteReturned;
            bool result =  DeviceIoControl(handle, IOCTL_STORAGE_EJECT_MEDIA, IntPtr.Zero, 0, IntPtr.Zero, 0, out byteReturned, IntPtr.Zero);
            System.Diagnostics.Debug.WriteLine(result ? "U盘:"+ b.ToString() + "已拷贝完成，请拔掉U盘" : "U盘退出失败");
            System.Diagnostics.Debug.WriteLine(this.ToString()+"退出！~！！！！！！！！！！！！！");
            
        }
        public delegate void RefleshUI(string s);
        private  bool CopyDirectory(string SourcePath, string DestinationPath, bool overwriteexisting)
        {
            bool ret = false;
            try
            {
                SourcePath = SourcePath.EndsWith(@"\") ? SourcePath : SourcePath + @"\";
                DestinationPath = DestinationPath.EndsWith(@"\") ? DestinationPath : DestinationPath + @"\";

                if (Directory.Exists(SourcePath))
                {
                    if (Directory.Exists(DestinationPath) == false)
                        Directory.CreateDirectory(DestinationPath);

                    foreach (string fls in Directory.GetFiles(SourcePath))
                    {
                        string LeftTS ="ovtfac_left_soundtrack.ts";
                        string RightTS ="ovtfac_right_soundtrack.ts";
                        if (File.Exists(DestinationPath + LeftTS) && fls.IndexOf(LeftTS) >= 0)
                        {
                            //MessageBox.Show("");
                        }
                        else if (File.Exists(DestinationPath + RightTS) && fls.IndexOf(RightTS) >= 0)
                        {

                        }
                        else
                        {
                            FileInfo flinfo = new FileInfo(fls);
                            flinfo.CopyTo(DestinationPath + flinfo.Name, overwriteexisting);
                        }
                    
                    }
                    foreach (string drs in Directory.GetDirectories(SourcePath))
                    {
                        DirectoryInfo drinfo = new DirectoryInfo(drs);
                        if (CopyDirectory(drs, DestinationPath + drinfo.Name, overwriteexisting) == false)
                            ret = false;
                    }
                }
                ret = true;
            }
            catch (Exception ex)
            {
                ret = false;
            }
            return ret;
        }
        
        private void sysout(string s)
        {
            System.Diagnostics.Debug.WriteLine(s);
        }
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_DEVICECHANGE)
            {
                switch (wParam.ToInt32())
                {
                    case DBT_DEVICEARRIVAL:
                        DriveInfo[] s = DriveInfo.GetDrives();
                        foreach (DriveInfo d in s)
                        {
                            if(d.DriveType==DriveType.Fixed)
                            {
                                FixDriveName.Remove(d.Name);
                            }
                            if( d.DriveType== DriveType.Removable)
                            {
                                if (d.IsReady)
                                {
                                    if(!driverpath.Contains(d.Name))
                                    {
                                        my = new Thread(new ParameterizedThreadStart(USBCopy));
                                        my.Start(d.Name);
                                        sysout(d.Name + "开始拷贝！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！");
                                        driverpath.Add(d.Name);
                                    }
                                }
                            }
                        }
                        break;
                    case DBT_DEVICEREMOVECOMPLETE:
                        List<string > AfterRemove = new List<string>();
                        DriveInfo[] q = DriveInfo.GetDrives();
                        //遍历加载移除后的可移动磁盘
                        foreach (DriveInfo d in q)
                            {
                                if (d.DriveType == DriveType.Removable)
                                {
                                    AfterRemove.Add(d.Name);
                                }
                            }
                        string str = string.Join(string.Empty, driverpath.Except(AfterRemove).Concat(AfterRemove.Except(driverpath)));
                        driverpath.Remove(str);
                        updte(str,"已移除", Color.FromRgb(128, 128, 128));
                        break;
                    default:
                        break;
                }
            }
            return IntPtr.Zero;
        }


        private void Postion_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                CopyPath = Postion.SelectedValue.ToString();
            }
            catch
            {
                CopyPath = string.Empty;
            }
            
        }

        private void UserVendor_Unloaded(object sender, RoutedEventArgs e)
        {
            if (hwndSource != null)
            {
                hwndSource.RemoveHook(new HwndSourceHook(WndProc));//挂钩
            }
        }

        private void DirSearch(string path)
        {
            try
            {
                foreach (string f in Directory.GetFiles(path))
                {
                    //listview.Items.Add(f);
                }
                foreach (string d in Directory.GetDirectories(path))
                {
                    DirSearch(d);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }



        private void Download(string fileName,string username,string password,string path,string filepath)
        {
            FtpWebRequest reqFTP;
            try
            {
                string filePath = filepath;// "";// Application.StartupPath;
                FileStream outputStream = new FileStream(filePath + "\\" + fileName, FileMode.Create);
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(path + fileName));
                reqFTP.Method = WebRequestMethods.Ftp.DownloadFile;
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(username, password);
                reqFTP.UsePassive = false;
                reqFTP.Timeout = 1000 * 60;
                reqFTP.ReadWriteTimeout = 1000 * 60;
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                Stream ftpStream = response.GetResponseStream();
                long cl = response.ContentLength;
                int bufferSize = 2048;
                int readCount;
                byte[] buffer = new byte[bufferSize];
                readCount = ftpStream.Read(buffer, 0, bufferSize);
                while (readCount > 0)
                {
                    outputStream.Write(buffer, 0, readCount);
                    readCount = ftpStream.Read(buffer, 0, bufferSize);
                }
                ftpStream.Close();
                outputStream.Close();
                response.Close();


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


    }


}
