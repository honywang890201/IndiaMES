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
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;

namespace ColorBoxCheck
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class UserControl1 : Component.Controls.User.UserVendor
    {
        private string setting_path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Framework.SysVar.SETTING_FILE_NAME);
        private DataRowView SelectRow = null;

        private bool _IsLoad = false;

        private bool IsCheckTwoDimensionalCode = false;
        private int TwoDimensionalCodeLength = 0;
        //private bool IsZN = false;//兆能项目判断标志，用于是否强制打开连接ovt恢复出厂数据库
       // private bool IsDrPeng = false;//鹏博士项目判断标志，用于获取鹏博士烧号数据中的WiFi mac并上传至mes方便后期报盘文件导出
        private bool IsGSAT = false;//菲律宾GSAT项目判断标志，用于获取菲律宾GSAT数据中的卡号并上传至mes方便后期报盘文件导出
        private bool IsGx6616 = false;//6616烧号项目判断标志位
        private bool IsGx6615 = false;//6615烧号项目判断标志位
        private bool IsSllk = false;//斯里兰卡项目判断标志位
        private bool IsP60 = false;//P60项目判断使用，耦合测试时用的WiFimac作为过站标志。
        private string ZhiLiBegin, ZhiLiEnd;
        private bool DataBaseChange = false;//判断是否可以切换数据库连接状态
        private long MOId = -1;
        /////////////////////////////////////////////////////////////////////////////////////////////////////
        MySqlDataReader reader = null;
        String mysqlStr = null; //"Database=ott_package;Data Source=192.168.0.119;User Id=root;Password=123456;pooling=false;CharSet=utf8;port=3306";
        MySqlConnection mysql = null;// new MySqlConnection(mysqlStr);
        MySqlCommand mySqlCommand = null;// new MySqlCommand("SELECT COUNT(*)FROM restore_order_all_t", mysql);
        string MESServerIp = null, FactoryServerIp = null;
        /////////////////////////////////////////////////////////////////////////////////////////////////////

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

        //private void BtnSelectorMO_Click(object sender, RoutedEventArgs e)
        //{

        //}
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

            BtnSelectorMO_Click(btnSelectorMO, null);

            List<KeyValuePair<string, string>> source1 = new List<KeyValuePair<string, string>>();
            source1.Add(new KeyValuePair<string, string>("LotSN", "批次条码"));
            source1.Add(new KeyValuePair<string, string>("Mac", "MAC"));
            source1.Add(new KeyValuePair<string, string>("DeviceSerialNumber", "设备标识"));
            source1.Add(new KeyValuePair<string, string>("GponSN", "GponSN"));
            source1.Add(new KeyValuePair<string, string>("CISN", "CISN"));
            source1.Add(new KeyValuePair<string, string>("DSN", "DSN"));
            source1.Add(new KeyValuePair<string, string>("EN", "EN"));

            cmbType1.ItemsSource = source1;

            try
            {
                cmbType1.SelectedValue = WinAPI.File.INIFileHelper.Read("ScanType", string.Format("P{0}-1", MenuId), setting_path, string.Empty);
                cmbType1_SelectionChanged(cmbType1, null);
            }
            catch
            {

            }

            List<KeyValuePair<string, string>> source2 = new List<KeyValuePair<string, string>>();
            source2.Add(new KeyValuePair<string, string>("LotSN", "批次条码"));
            source2.Add(new KeyValuePair<string, string>("Mac", "MAC"));
            source2.Add(new KeyValuePair<string, string>("DeviceSerialNumber", "设备标识"));
            source2.Add(new KeyValuePair<string, string>("GponSN", "GponSN"));
            source2.Add(new KeyValuePair<string, string>("CISN", "CISN"));
            source2.Add(new KeyValuePair<string, string>("DSN", "DSN"));
            source2.Add(new KeyValuePair<string, string>("EN", "EN"));

            cmbType2.ItemsSource = source2;

            try
            {
                string v = WinAPI.File.INIFileHelper.Read("ScanType", string.Format("P{0}-IsScan2", MenuId), setting_path, string.Empty);
                bool b = false;
                bool.TryParse(v, out b);
                ckbIsScan2.IsChecked = b;

                cmbType2.SelectedValue = WinAPI.File.INIFileHelper.Read("ScanType", string.Format("P{0}-2", MenuId), setting_path, string.Empty);
                cmbType2_SelectionChanged(cmbType2, null);
            }
            catch
            {

            }
            List<KeyValuePair<string, string>> source3 = new List<KeyValuePair<string, string>>();
            source3.Add(new KeyValuePair<string, string>("LotSN", "批次条码"));
            source3.Add(new KeyValuePair<string, string>("Mac", "MAC"));
            source3.Add(new KeyValuePair<string, string>("DeviceSerialNumber", "设备标识"));
            source3.Add(new KeyValuePair<string, string>("GponSN", "GponSN"));
            source3.Add(new KeyValuePair<string, string>("CISN", "CISN"));
            source3.Add(new KeyValuePair<string, string>("DSN", "DSN"));
            source3.Add(new KeyValuePair<string, string>("EN", "EN"));

            cmbType3.ItemsSource = source3;

            try
            {
                string v = WinAPI.File.INIFileHelper.Read("ScanType", string.Format("P{0}-IsScan3", MenuId), setting_path, string.Empty);
                bool b = false;
                bool.TryParse(v, out b);
                ckbIsScan3.IsChecked = b;

                cmbType3.SelectedValue = WinAPI.File.INIFileHelper.Read("ScanType", string.Format("P{0}-3", MenuId), setting_path, string.Empty);
                cmbType3_SelectionChanged(cmbType3, null);
            }
            catch
            {

            }

            List<KeyValuePair<string, string>> source4 = new List<KeyValuePair<string, string>>();
            source4.Add(new KeyValuePair<string, string>("LotSN", "批次条码"));
            source4.Add(new KeyValuePair<string, string>("Mac", "MAC"));
            source4.Add(new KeyValuePair<string, string>("DeviceSerialNumber", "设备标识"));
            source4.Add(new KeyValuePair<string, string>("GponSN", "GponSN"));
            source4.Add(new KeyValuePair<string, string>("CISN", "CISN"));
            source4.Add(new KeyValuePair<string, string>("DSN", "DSN"));
            source4.Add(new KeyValuePair<string, string>("EN", "EN"));

            cmbType4.ItemsSource = source4;

            try
            {
                string v = WinAPI.File.INIFileHelper.Read("ScanType", string.Format("P{0}-IsScan4", MenuId), setting_path, string.Empty);
                bool b = false;
                bool.TryParse(v, out b);
                ckbIsScan4.IsChecked = b;

                cmbType4.SelectedValue = WinAPI.File.INIFileHelper.Read("ScanType", string.Format("P{0}-4", MenuId), setting_path, string.Empty);
                cmbType4_SelectionChanged(cmbType4, null);
            }
            catch
            {

            }

            List<KeyValuePair<string, string>> source5 = new List<KeyValuePair<string, string>>();
            source5.Add(new KeyValuePair<string, string>("LotSN", "批次条码"));
            source5.Add(new KeyValuePair<string, string>("Mac", "MAC"));
            source5.Add(new KeyValuePair<string, string>("DeviceSerialNumber", "设备标识"));
            source5.Add(new KeyValuePair<string, string>("GponSN", "GponSN"));
            source5.Add(new KeyValuePair<string, string>("CISN", "CISN"));
            source5.Add(new KeyValuePair<string, string>("DSN", "DSN"));
            source5.Add(new KeyValuePair<string, string>("EN", "EN"));

            cmbType5.ItemsSource = source5;

            try
            {
                string v = WinAPI.File.INIFileHelper.Read("ScanType", string.Format("P{0}-IsScan5", MenuId), setting_path, string.Empty);
                bool b = false;
                bool.TryParse(v, out b);
                ckbIsScan5.IsChecked = b;

                cmbType5.SelectedValue = WinAPI.File.INIFileHelper.Read("ScanType", string.Format("P{0}-5", MenuId), setting_path, string.Empty);
                cmbType5_SelectionChanged(cmbType5, null);
            }
            catch
            {

            }

            List<KeyValuePair<string, string>> source6 = new List<KeyValuePair<string, string>>();
            source6.Add(new KeyValuePair<string, string>("LotSN", "批次条码"));
            source6.Add(new KeyValuePair<string, string>("Mac", "MAC"));
            source6.Add(new KeyValuePair<string, string>("DeviceSerialNumber", "设备标识"));
            source6.Add(new KeyValuePair<string, string>("GponSN", "GponSN"));
            source6.Add(new KeyValuePair<string, string>("CISN", "CISN"));
            source6.Add(new KeyValuePair<string, string>("DSN", "DSN"));
            source6.Add(new KeyValuePair<string, string>("EN", "EN"));

            cmbType6.ItemsSource = source6;

            try
            {
                string v = WinAPI.File.INIFileHelper.Read("ScanType", string.Format("P{0}-IsScan6", MenuId), setting_path, string.Empty);
                bool b = false;
                bool.TryParse(v, out b);
                ckbIsScan6.IsChecked = b;
                cmbType6.SelectedValue = WinAPI.File.INIFileHelper.Read("ScanType", string.Format("P{0}-6", MenuId), setting_path, string.Empty);
                cmbType6_SelectionChanged(cmbType6, null);
            }
            catch
            {

            }
            if (WinAPI.File.INIFileHelper.Read("DataBaseConfig", "IsDateConnect", setting_path, "1") == "1")
            {
                DataBaseChange = true;
            }
            else
            {
                DataBaseChange = false;
            }
        }



        private void LoadMO()
        {
            lblTwoDimensionalCode.Visibility = Visibility.Collapsed;
            txtTwoDimensionalCode.Visibility = Visibility.Collapsed;
            IsCheckTwoDimensionalCode = false;
            TwoDimensionalCodeLength = 0;
            if (SelectRow == null)
            {
                tbMOCode.Text = string.Empty;
                tbQty.Text = string.Empty;
                tbItemCode.Text = string.Empty;
                tbItemSpec.Text = string.Empty;
                tbWorkflow.Text = string.Empty;
                tbCustomer.Text = string.Empty;
            }
            else
            {
                bool.TryParse(SelectRow["IsCheckTwoDimensionalCode"].ToString(), out IsCheckTwoDimensionalCode);
                int.TryParse(SelectRow["TwoDimensionalCodeLength"].ToString(), out TwoDimensionalCodeLength);
                if (IsCheckTwoDimensionalCode)
                {
                    lblTwoDimensionalCode.Visibility = Visibility.Visible;
                    txtTwoDimensionalCode.Visibility = Visibility.Visible;
                }

                tbMOCode.Text = SelectRow["MOCode"].ToString();
                tbQty.Text = SelectRow["Qty"].ToString();
                tbItemCode.Text = SelectRow["ItemCode"].ToString();
                tbItemSpec.Text = SelectRow["ItemSpecification"].ToString();
                tbWorkflow.Text = SelectRow["WorkflowCode"].ToString();
                tbCustomer.Text = SelectRow["CustomerCode"].ToString();
                if (!string.IsNullOrEmpty(SelectRow["CustomerDesc"].ToString().Trim()))
                    tbCustomer.Text = tbCustomer.Text + "/" + SelectRow["CustomerDesc"].ToString();
               
                //兆能项目时自动勾选连接恢复出厂数据库
                if (SelectRow["CustomerCode"].ToString() == "0010")
                {
                    IsConnect.IsChecked = true;
                }
                else
                {
                    if (WinAPI.File.INIFileHelper.Read("DataBaseConfig", "IsDateConnect", setting_path, "1") == "1")
                    {
                        IsConnect.IsChecked = true;
                    }
                    else
                    {
                        IsConnect.IsChecked = false;
                    }
                }
                //菲律宾GSAT项目时
                if (SelectRow["CustomerCode"].ToString() == "00108")
                {
                    IsGSAT = true;
                }
                //其他项目时
                else
                {
                    IsGSAT = false;
                }
               
                //智利项/尼日利亚6616目时
                if (SelectRow["CustomerCode"].ToString() == "00111")
                {
                    IsGx6616 = true;
                    ZhiLiBegin = SelectRow["ZhiLiBegin"].ToString();
                    ZhiLiEnd = SelectRow["ZhiLiEnd"].ToString();
                    
                }
                //其他项目时
                else
                {
                    IsGx6616 = false;
                    ZhiLiBegin = string.Empty;
                    ZhiLiEnd = string.Empty;

                }
                //9ja-6615时
                if (SelectRow["CustomerCode"].ToString() == "00122")
                {
                    IsGx6615= true;
                    ZhiLiBegin = SelectRow["ZhiLiBegin"].ToString();
                    ZhiLiEnd = SelectRow["ZhiLiEnd"].ToString();

                }
                //其他项目时
                else
                {
                    IsGx6615 = false;
                    ZhiLiBegin = string.Empty;
                    ZhiLiEnd = string.Empty;

                }

                //兆能魔百盒、P60项目时需要卡站耦合测试
                if (SelectRow["CustomerCode"].ToString() == "00110")
                {
                    IsP60 = true;
                }
                //其他项目时
                else
                {
                    IsP60 = false;
                }
                if (!string.IsNullOrEmpty(SelectRow["WorkflowDesc"].ToString().Trim()))
                    tbWorkflow.Text = tbWorkflow.Text + "/" + SelectRow["WorkflowDesc"].ToString();
                if (SelectRow["WorkflowId"] == null || SelectRow["WorkflowId"].ToString().Trim() == string.Empty)
                {
                    txtMessage.AddMessage(string.Format("工单[{0}]未设置工作流！", SelectRow["MOCode"]), true);
                }
                //IsConnect_Checked(null, null);
                uph.Start();
            }
        }

        private void BtnSelectorMO_Click(object sender, RoutedEventArgs e)
        {
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
	   Bas_Item.IsCheckTwoDimensionalCode,
	   Bas_Item.TwoDimensionalCodeLength
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

        public override void CloseControl()
        {
            try
            {
                WinAPI.File.INIFileHelper.Write("ScanType", string.Format("P{0}-IsScan2", MenuId), ckbIsScan2.IsChecked.HasValue ? ckbIsScan2.IsChecked.Value.ToString() : false.ToString(), setting_path);
                WinAPI.File.INIFileHelper.Write("ScanType", string.Format("P{0}-IsScan3", MenuId), ckbIsScan3.IsChecked.HasValue ? ckbIsScan3.IsChecked.Value.ToString() : false.ToString(), setting_path);
                WinAPI.File.INIFileHelper.Write("ScanType", string.Format("P{0}-IsScan4", MenuId), ckbIsScan3.IsChecked.HasValue ? ckbIsScan4.IsChecked.Value.ToString() : false.ToString(), setting_path);
            }
            catch { }
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
        private void cmbType1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbType1.SelectedValue != null && cmbType1.SelectedItem != null)
            {
                WinAPI.File.INIFileHelper.Write("ScanType", string.Format("P{0}-1", MenuId), cmbType1.SelectedValue.ToString(), setting_path);
                lblScan1.Text = ((KeyValuePair<string, string>)cmbType1.SelectedItem).Value + "：";
            }
        }
        private void cmbType2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbType2.SelectedValue != null && cmbType2.SelectedItem != null)
            {
                WinAPI.File.INIFileHelper.Write("ScanType", string.Format("P{0}-2", MenuId), cmbType2.SelectedValue.ToString(), setting_path);
                lblScan2.Text = ((KeyValuePair<string, string>)cmbType2.SelectedItem).Value + "：";
            }
        }
        private void cmbType3_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbType3.SelectedValue != null && cmbType3.SelectedItem != null)
            {
                WinAPI.File.INIFileHelper.Write("ScanType", string.Format("P{0}-3", MenuId), cmbType3.SelectedValue.ToString(), setting_path);
                lblScan3.Text = ((KeyValuePair<string, string>)cmbType3.SelectedItem).Value + "：";
            }
        }
        private void cmbType4_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbType4.SelectedValue != null && cmbType4.SelectedItem != null)
            {
                WinAPI.File.INIFileHelper.Write("ScanType", string.Format("P{0}-4", MenuId), cmbType4.SelectedValue.ToString(), setting_path);
                lblScan4.Text = ((KeyValuePair<string, string>)cmbType4.SelectedItem).Value + "：";
            }
        }
        private void cmbType5_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbType5.SelectedValue != null && cmbType5.SelectedItem != null)
            {
                WinAPI.File.INIFileHelper.Write("ScanType", string.Format("P{0}-5", MenuId), cmbType5.SelectedValue.ToString(), setting_path);
                lblScan5.Text = ((KeyValuePair<string, string>)cmbType5.SelectedItem).Value + "：";
            }
        }
        private void cmbType6_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbType6.SelectedValue != null && cmbType6.SelectedItem != null)
            {
                WinAPI.File.INIFileHelper.Write("ScanType", string.Format("P{0}-6", MenuId), cmbType6.SelectedValue.ToString(), setting_path);
                lblScan6.Text = ((KeyValuePair<string, string>)cmbType6.SelectedItem).Value + "：";
            }
        }
        public void Submit()
        {
            if (string.IsNullOrEmpty(tbMOCode.Text.Trim()))
            {
                txtMessage.AddMessage("请先选择工单！", true);
                btnSelectorMO.Focus();
                SetErrorOrSuccImage(true, false);
                return;
            }
            if (cmbType1.SelectedValue == null || cmbType1.SelectedItem == null)
            {
                txtMessage.AddMessage("请选择扫描类型1！", true);
                cmbType1.Focus();
                SetErrorOrSuccImage(true, false);
                return;
            }
            if (txtScan1.Text.Trim() == string.Empty)
            {
                txtMessage.AddMessage(string.Format("请扫描{0}！", lblScan1.Text), true);
                txtScan1.Text = string.Empty;
                txtScan1.SetFoucs();
                SetErrorOrSuccImage(true, false);
                return;
            }
            if (ckbIsScan2.IsChecked.Value)
            {
                if (cmbType2.SelectedValue == null || cmbType2.SelectedItem == null)
                {
                    txtMessage.AddMessage("请选择扫描类型2！", true);
                    cmbType2.Focus();
                    SetErrorOrSuccImage(true, false);
                    return;
                }
                if (txtScan2.Text.Trim() == string.Empty)
                {
                    txtMessage.AddMessage(string.Format("请扫描{0}！", lblScan2.Text), true);
                    txtScan2.Text = string.Empty;
                    txtScan2.SetFoucs();
                    SetErrorOrSuccImage(true, false);
                    return;
                }
            }
            if (ckbIsScan3.IsChecked.Value)
            {
                if (cmbType3.SelectedValue == null || cmbType3.SelectedItem == null)
                {
                    txtMessage.AddMessage("请选择扫描类型3！", true);
                    cmbType3.Focus();
                    SetErrorOrSuccImage(true, false);
                    return;
                }
                if (txtScan3.Text.Trim() == string.Empty)
                {
                    txtMessage.AddMessage(string.Format("请扫描{0}！", lblScan3.Text), true);
                    txtScan3.Text = string.Empty;
                    txtScan3.SetFoucs();
                    SetErrorOrSuccImage(true, false);
                    return;
                }
            }
            if (ckbIsScan4.IsChecked.Value)
            {
                if (cmbType4.SelectedValue == null || cmbType4.SelectedItem == null)
                {
                    txtMessage.AddMessage("请选择扫描类型4！", true);
                    cmbType4.Focus();
                    SetErrorOrSuccImage(true, false);
                    return;
                }
                if (txtScan4.Text.Trim() == string.Empty)
                {
                    txtMessage.AddMessage(string.Format("请扫描{0}！", lblScan4.Text), true);
                    txtScan4.Text = string.Empty;
                    txtScan4.SetFoucs();
                    SetErrorOrSuccImage(true, false);
                    return;
                }
            }
            if (ckbIsScan5.IsChecked.Value)
            {
                if (cmbType5.SelectedValue == null || cmbType5.SelectedItem == null)
                {
                    txtMessage.AddMessage("请选择扫描类型5！", true);
                    cmbType5.Focus();
                    SetErrorOrSuccImage(true, false);
                    return;
                }
                if (txtScan5.Text.Trim() == string.Empty)
                {
                    txtMessage.AddMessage(string.Format("请扫描{0}！", lblScan5.Text), true);
                    txtScan5.Text = string.Empty;
                    txtScan5.SetFoucs();
                    SetErrorOrSuccImage(true, false);
                    return;
                }
            }
            if (ckbIsScan6.IsChecked.Value)
            {
                if (cmbType6.SelectedValue == null || cmbType6.SelectedItem == null)
                {
                    txtMessage.AddMessage("请选择扫描类型6！", true);
                    cmbType6.Focus();
                    SetErrorOrSuccImage(true, false);
                    return;
                }
                if (txtScan6.Text.Trim() == string.Empty)
                {
                    txtMessage.AddMessage(string.Format("请扫描{0}！", lblScan6.Text), true);
                    txtScan6.Text = string.Empty;
                    txtScan6.SetFoucs();
                    SetErrorOrSuccImage(true, false);
                    return;
                }
            }
            if (IsCheckTwoDimensionalCode)
            {
                if (txtTwoDimensionalCode.Text.Trim() == string.Empty)
                {
                    txtMessage.AddMessage(string.Format("请扫描二维码！"), true);
                    txtTwoDimensionalCode.Text = string.Empty;
                    txtTwoDimensionalCode.SetFoucs();
                    SetErrorOrSuccImage(true, false);
                    return;
                }
            }
            if(0>1)
                {
                //检查恢复出厂数据是否存在
                if (IsConnect.IsChecked.Value)
                {
                    if (QueryRestoreStatus(txtScan1.Text.Trim()) <= 0)
                    {
                        txtMessage.AddMessage("此机恢复出厂不成功!!!!,请确认后再次扫描", true);
                        ClearScan();
                        txtScan1.SetFoucs();
                        SetErrorOrSuccImage(true, false);
                        return;
                    }
                }
            }
            //获取菲律宾GSAT的卡号ovt@admi
            if (IsGSAT)
            {
                if (!UpdateGSATCard(txtScan1.Text.Trim()))
                {
                    return;
                }
            }
            //根据扫描到的数据从烧号数据库中拿到sn号码作为条码对比的数据
            if(IsGx6616)
            {
                if(txtScan1.Text!=string.Empty)
                {
                    txtScan1.Text= Get6616SerialNumber(txtScan1.Text.Trim());
                }
                if (txtScan2.Text != string.Empty)
                {
                    txtScan2.Text = Get6616SerialNumber(txtScan2.Text.Trim());
                    if(txtScan2.Text!= txtScan1.Text)
                    {
                        txtMessage.AddMessage("扫描到的条码不一致", true);
                        ClearScan();
                        txtScan1.SetFoucs();
                        SetErrorOrSuccImage(true, false);
                        return;
                    }
                }
                if (txtScan3.Text != string.Empty)
                {
                    txtScan3.Text = Get6616SerialNumber(txtScan3.Text.Trim());
                    if(txtScan3.Text!= txtScan2.Text)
                    {
                        txtMessage.AddMessage("扫描到的条码不一致", true);
                        ClearScan();
                        txtScan1.SetFoucs();
                        SetErrorOrSuccImage(true, false);
                        return;
                    }
                }
                if (txtScan4.Text != string.Empty)
                {
                    txtScan4.Text = Get6616SerialNumber(txtScan4.Text.Trim());
                    if (txtScan4.Text != txtScan3.Text)
                    {
                        txtMessage.AddMessage("扫描到的条码不一致", true);
                        ClearScan();
                        txtScan1.SetFoucs();
                        SetErrorOrSuccImage(true, false);
                        return;
                    }
                }
                if (txtScan5.Text != string.Empty)
                {
                    txtScan5.Text = Get6616SerialNumber(txtScan5.Text.Trim());
                    if (txtScan5.Text != txtScan4.Text)
                    {
                        txtMessage.AddMessage("扫描到的条码不一致", true);
                        ClearScan();
                        txtScan1.SetFoucs();
                        SetErrorOrSuccImage(true, false);
                        return;
                    }
                }
                if (txtScan6.Text != string.Empty)
                {
                    txtScan6.Text = Get6616SerialNumber(txtScan6.Text.Trim());
                    if (txtScan6.Text != txtScan5.Text)
                    {
                        txtMessage.AddMessage("扫描到的条码不一致", true);
                        ClearScan();
                        txtScan1.SetFoucs();
                        SetErrorOrSuccImage(true, false);
                        return;
                    }
                }
                if(!Updata6616Card(txtScan1.Text))
                {
                    return;
                }

            }
            if (IsGx6615)
            {
                if (txtScan1.Text != string.Empty)
                {
                    txtScan1.Text = Get6615SerialNumber(txtScan1.Text.Trim());
                }
                if (txtScan2.Text != string.Empty)
                {
                    txtScan2.Text = Get6615SerialNumber(txtScan2.Text.Trim());
                    if (txtScan2.Text != txtScan1.Text)
                    {
                        txtMessage.AddMessage("扫描到的条码不一致", true);
                        ClearScan();
                        txtScan1.SetFoucs();
                        SetErrorOrSuccImage(true, false);
                        return;
                    }
                }
                if (txtScan3.Text != string.Empty)
                {
                    txtScan3.Text = Get6615SerialNumber(txtScan3.Text.Trim());
                    if (txtScan3.Text != txtScan2.Text)
                    {
                        txtMessage.AddMessage("扫描到的条码不一致", true);
                        ClearScan();
                        txtScan1.SetFoucs();
                        SetErrorOrSuccImage(true, false);
                        return;
                    }
                }
                if (txtScan4.Text != string.Empty)
                {
                    txtScan4.Text = Get6615SerialNumber(txtScan4.Text.Trim());
                    if (txtScan4.Text != txtScan3.Text)
                    {
                        txtMessage.AddMessage("扫描到的条码不一致", true);
                        ClearScan();
                        txtScan1.SetFoucs();
                        SetErrorOrSuccImage(true, false);
                        return;
                    }
                }
                if (txtScan5.Text != string.Empty)
                {
                    txtScan5.Text = Get6615SerialNumber(txtScan5.Text.Trim());
                    if (txtScan5.Text != txtScan4.Text)
                    {
                        txtMessage.AddMessage("扫描到的条码不一致", true);
                        ClearScan();
                        txtScan1.SetFoucs();
                        SetErrorOrSuccImage(true, false);
                        return;
                    }
                }
                if (txtScan6.Text != string.Empty)
                {
                    txtScan6.Text = Get6615SerialNumber(txtScan6.Text.Trim());
                    if (txtScan6.Text != txtScan5.Text)
                    {
                        txtMessage.AddMessage("扫描到的条码不一致", true);
                        ClearScan();
                        txtScan1.SetFoucs();
                        SetErrorOrSuccImage(true, false);
                        return;
                    }
                }
                if (!Updata6615Card(txtScan1.Text))
                {
                    return;
                }

            }
            //如果为P60则要查询是否耦合测试通过
            if (IsP60)
            {
                if (!QueryP60WifiMac(txtScan1.Text.Trim()))
                {
                    return;
                }
            }
            Parameters parameters = new Parameters();
            parameters.Add("ScanType1", cmbType1.SelectedValue.ToString());
           parameters.Add("ScanTypeDesc1", ((KeyValuePair<string, string>)cmbType1.SelectedItem).Value);
            parameters.Add("ScanCode1", txtScan1.Text.Trim());
            parameters.Add("IsScan2", ckbIsScan2.IsChecked.Value);
            if (ckbIsScan2.IsChecked.Value)
            {
                parameters.Add("ScanType2", cmbType2.SelectedValue.ToString());
                parameters.Add("ScanTypeDesc2", ((KeyValuePair<string, string>)cmbType2.SelectedItem).Value);
                parameters.Add("ScanCode2", txtScan2.Text.Trim());
            }
            parameters.Add("IsScan3", ckbIsScan3.IsChecked.Value);
            if (ckbIsScan3.IsChecked.Value)
            {
                parameters.Add("ScanType3", cmbType3.SelectedValue.ToString());
                parameters.Add("ScanTypeDesc3", ((KeyValuePair<string, string>)cmbType3.SelectedItem).Value);
                parameters.Add("ScanCode3", txtScan3.Text.Trim());
            }
            parameters.Add("IsScan4", ckbIsScan4.IsChecked.Value);
            if (ckbIsScan4.IsChecked.Value)
            {
                parameters.Add("ScanType4", cmbType4.SelectedValue.ToString());
                parameters.Add("ScanTypeDesc4", ((KeyValuePair<string, string>)cmbType4.SelectedItem).Value);
                parameters.Add("ScanCode4", txtScan4.Text.Trim());
            }
            parameters.Add("IsScan5", ckbIsScan5.IsChecked.Value);
            if (ckbIsScan5.IsChecked.Value)
            {
                parameters.Add("ScanType5", cmbType5.SelectedValue.ToString());
                parameters.Add("ScanTypeDesc5", ((KeyValuePair<string, string>)cmbType5.SelectedItem).Value);
                parameters.Add("ScanCode5", txtScan5.Text.Trim());
            }
            parameters.Add("IsScan6", ckbIsScan6.IsChecked.Value);
            if (ckbIsScan6.IsChecked.Value)
            {
                parameters.Add("ScanType6", cmbType6.SelectedValue.ToString());
                parameters.Add("ScanTypeDesc6", ((KeyValuePair<string, string>)cmbType6.SelectedItem).Value);
                parameters.Add("ScanCode6", txtScan6.Text.Trim());
            }
            parameters.Add("TwoDimensionalCode", IsCheckTwoDimensionalCode ? txtTwoDimensionalCode.Text.Trim() : (object)DBNull.Value, SqlDbType.NVarChar, int.MaxValue);
            parameters.Add("MOId", SelectRow["MOId"]);
            parameters.Add("ItemId", SelectRow["ItemId"]);
            parameters.Add("LineId", Framework.App.Resource.LineId);
            parameters.Add("ResId", Framework.App.Resource.ResourceId);
            parameters.Add("ShiftTypeId", Framework.App.Resource.ShiftTypeId);
            parameters.Add("UserId", Framework.App.User.UserId);
            parameters.Add("OPId", Framework.App.Resource.StationId);
            parameters.Add("PluginId", PluginId);
            parameters.Add("Return_Message", null, SqlDbType.NVarChar, int.MaxValue, ParameterDirection.Output);
            parameters.Add("Return_Value", null, SqlDbType.Int, ParameterDirection.ReturnValue);


            Result<Parameters, DataSet> result = null;
            try
            {
                result = DB.DBHelper.ExecuteParametersSource("Prd_Inp_Pack_ColorBoxCheck", parameters, ExecuteType.StoredProcedure);
                //sult = DB.DBHelper.ExecuteParametersSource("Test_Inp_Pack_ColorBoxCheck", parameters, ExecuteType.StoredProcedure);
            }
            catch (Exception ex)
            {
                txtMessage.AddMessage(ex.Message, true);
                ClearScan();
                txtScan1.SetFoucs();
                SetErrorOrSuccImage(true, false);
                return;
            }

            if (result.HasError)
            {
                txtMessage.AddMessage(result.Message, true);
                ClearScan();
                txtScan1.SetFoucs();
                SetErrorOrSuccImage(true, false);
                return;
            }

            if ((int)result.Value1["Return_Value"] != 1)
            {
                txtMessage.AddMessage(result.Value1["Return_Message"].ToString(), true);
                ClearScan();
                txtScan1.SetFoucs();
                SetErrorOrSuccImage(true, false);
                return;
            }
            else
            {
                txtMessage.AddMessage(result.Value1["Return_Message"].ToString(), false);
                ClearScan();
                txtScan1.SetFoucs();
                SetErrorOrSuccImage(false, false);
                uph.Quantity++;
            }
        }
        //根据扫描到数据从数据库中找到P60的WiFimac，并从mes中查找这个WiFimac是否经过耦合测试
        public bool QueryP60WifiMac(string STBNO)
        {
            string WifiMac = string.Empty;
            DataRowView Result = null;
            //在恢复出厂的数据库中查找是否已经恢复成功
            mysqlStr = "Database=ott_package;Data Source=" + FactoryServerIp + ";User Id=root;Password=123456;pooling=false;CharSet=utf8;port=3306";
            mysql = new MySqlConnection(mysqlStr);
            mySqlCommand = new MySqlCommand("SELECT mac FROM restore_order_all_t WHERE SerialNum= '" + STBNO + "' OR mac = '" + STBNO + "'", mysql);
            try
            {
                //获得读取结果
                mysql.Open();
                MySqlDataReader mysqldr = mySqlCommand.ExecuteReader();
                while (mysqldr.Read())//mysqldr.Read()返回的是bool值，意在判断是否有下一条数据
                {
                    WifiMac = mysqldr["mac"].ToString();
                }
            }
            catch (Exception e)
            {
                txtMessage.AddMessage(e.ToString(), true);
                mysql.Close();
                return false;
            }
            mysql.Close();
            if (WifiMac == string.Empty)
            {
                txtMessage.AddMessage("此盒子恢复出厂不成功，没有获取到WiFi mac", true);
                ClearScan();
                txtScan1.SetFoucs();
                return false;
            }
            else
            {
                string sql = @"SELECT IsUsed FROM Bas_P60WIFIMac WHERE MOId=@MOId AND WIFIMac='" + WifiMac + "'";
                Parameters parameters = new Parameters();
                parameters.Add("MOId", MOId);
                try
                {
                    DataTable table = DB.DBHelper.GetDataTable(sql, parameters, ExecuteType.Text, null, true);
                    if (table.Rows.Count < 1)
                    {
                        txtMessage.AddMessage("WiFi mac[" + WifiMac + "]没有找到测试记录，可能还没有测试", true);
                        ClearScan();
                        txtScan1.SetFoucs();
                        return false;
                    }
                    Result = table.DefaultView[0];
                    if (Result["IsUsed"].ToString() == "True")
                    {
                        return true;
                    }
                    else
                    {

                        txtMessage.AddMessage("当前机顶盒[" + STBNO + "]耦合测试未通过，请返回耦合测试工位测试通过后再次测试！！！", true);
                        ClearScan();
                        txtScan1.SetFoucs();
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    txtMessage.AddMessage(ex.ToString(), true);
                    //txtMessage.AddMessage("执行查询操作错误", true);
                    ClearScan();
                    txtScan1.SetFoucs();
                    return false;
                }
            }
        }
        //没有用这个了，放弃
        public string GetSllkSerialNumber(string WaitForString)
        {
            string SerialNumber = string.Empty;
            string StbId = string.Empty;
            //SELECT * FROM `product_vn_dvt_gx6616_32` WHERE `serial_number` = '421026200700105116' AND `stb_id` LIKE '%123%' LIMIT 0, 1000
            //在恢复出厂的数据库中查找是否已经恢复成功       AND burn_status=1 
            mysqlStr = "Database=serialnumber;Data Source=" + FactoryServerIp + ";User Id=root;Password=123456;pooling=false;CharSet=utf8;port=3306";
            mysql = new MySqlConnection(mysqlStr);
            mySqlCommand = new MySqlCommand("SELECT serial_number,stb_id FROM product_vn_dvt_gx6616_32 WHERE serial_number='" + WaitForString + "' OR stb_id LIKE '%" + WaitForString + "%' OR chip_id='" + WaitForString + "' AND burn_status=1 ", mysql);
            try
            {
                //获得读取结果
                mysql.Open();
                MySqlDataReader mysqldr = mySqlCommand.ExecuteReader();
                while (mysqldr.Read())//mysqldr.Read()返回的是bool值，意在判断是否有下一条数据
                {
                    SerialNumber = mysqldr["serial_number"].ToString();
                    StbId = mysqldr["stb_id"].ToString();
                }
            }
            catch (Exception e)
            {
                txtMessage.AddMessage(e.ToString(), true);
                mysql.Close();
                return string.Empty;
            }
            mysql.Close();
            if (SerialNumber == string.Empty)
            {
                txtMessage.AddMessage("没有获取到烧号信息或盒子对号未通过!!!", true);
                ClearScan();
                txtScan1.SetFoucs();
                SetErrorOrSuccImage(true, false);
                return string.Empty;
            }
            else
            {
                if (ZhiLiBegin != string.Empty && ZhiLiEnd != string.Empty)
                {
                    //if (ZhiLiBegin == string.Empty || ZhiLiEnd == string.Empty)
                    //{
                    //    txtMessage.AddMessage("智利项目没有设置stbid范围！！！！！！！！！！！！", true);
                    //    ClearScan();
                    //    txtScan1.SetFoucs();
                    //    SetErrorOrSuccImage(true, false);
                    //    return string.Empty;
                    //}
                    //else 
                    if (String.Compare(StbId, ZhiLiBegin) < 0 || String.Compare(StbId, ZhiLiEnd) > 0)
                    {
                        txtMessage.AddMessage("stbid不在设置的范围[" + ZhiLiBegin + "~" + ZhiLiEnd + "]内！！！", true);
                        ClearScan();
                        txtScan1.SetFoucs();
                        SetErrorOrSuccImage(true, false);
                        return string.Empty;
                    }
                }
            }
            return SerialNumber;
        }
        public string Get6616SerialNumber(string WaitForString)
        {
            string SerialNumber = string.Empty;
            string StbId = string.Empty;
            //SELECT * FROM `product_vn_dvt_gx6616_32` WHERE `serial_number` = '421026200700105116' AND `stb_id` LIKE '%123%' LIMIT 0, 1000
            //在恢复出厂的数据库中查找是否已经恢复成功       AND burn_status=1 
            mysqlStr = "Database=serialnumber;Data Source=" + FactoryServerIp + ";User Id=root;Password=123456;pooling=false;CharSet=utf8;port=3306";
            mysql = new MySqlConnection(mysqlStr);
            mySqlCommand = new MySqlCommand("SELECT serial_number,stb_id FROM product_vn_dvt_gx6616_32 WHERE (serial_number='" + WaitForString + "' OR stb_id LIKE '%"+ WaitForString + "%' OR chip_id='"+ WaitForString + "') AND burn_status=1 ", mysql);
            try
            {
                //获得读取结果
                mysql.Open();
                MySqlDataReader mysqldr = mySqlCommand.ExecuteReader();
                while (mysqldr.Read())//mysqldr.Read()返回的是bool值，意在判断是否有下一条数据
                {
                    SerialNumber = mysqldr["serial_number"].ToString();
                    StbId= mysqldr["stb_id"].ToString();
                }
            }
            catch (Exception e)
            {
                txtMessage.AddMessage(e.ToString(), true);
                mysql.Close();
                return string.Empty;
            }
            mysql.Close();
            if (SerialNumber == string.Empty)
            {
                txtMessage.AddMessage("没有获取到烧号信息或盒子对号未通过!!!", true);
                ClearScan();
                txtScan1.SetFoucs();
                SetErrorOrSuccImage(true, false);
                return string.Empty;
            }
            else
            {
                if (ZhiLiBegin != string.Empty && ZhiLiEnd != string.Empty)
                {
                    //if (ZhiLiBegin == string.Empty || ZhiLiEnd == string.Empty)
                    //{
                    //    txtMessage.AddMessage("智利项目没有设置stbid范围！！！！！！！！！！！！", true);
                    //    ClearScan();
                    //    txtScan1.SetFoucs();
                    //    SetErrorOrSuccImage(true, false);
                    //    return string.Empty;
                    //}
                    //else 
                    if (String.Compare(StbId, ZhiLiBegin) < 0 || String.Compare(StbId, ZhiLiEnd) > 0)
                    {
                        txtMessage.AddMessage("stbid不在设置的范围[" + ZhiLiBegin + "~" + ZhiLiEnd + "]内！！！", true);
                        ClearScan();
                        txtScan1.SetFoucs();
                        SetErrorOrSuccImage(true, false);
                        return string.Empty;
                    }
                }
            }
            return SerialNumber;
        }
        public string Get6615SerialNumber(string WaitForString)
        {
            string SerialNumber = string.Empty;
            string ChipId = string.Empty;
            //SELECT * FROM `product_vn_dvt_gx6616_32` WHERE `serial_number` = '421026200700105116' AND `stb_id` LIKE '%123%' LIMIT 0, 1000
            //在恢复出厂的数据库中查找是否已经恢复成功       AND burn_status=1 
            mysqlStr = "Database=serialnumber;Data Source=" + FactoryServerIp + ";User Id=root;Password=123456;pooling=false;CharSet=utf8;port=3306";
            mysql = new MySqlConnection(mysqlStr);
            mySqlCommand = new MySqlCommand("SELECT serial_number,chip_id FROM product_vn_dvt_gx6615_19 WHERE serial_number='" + WaitForString + "' OR chip_id LIKE '%" + WaitForString + "%' OR ca_id='" + WaitForString + "'", mysql);
            try
            {
                //获得读取结果
                mysql.Open();
                MySqlDataReader mysqldr = mySqlCommand.ExecuteReader();
                while (mysqldr.Read())//mysqldr.Read()返回的是bool值，意在判断是否有下一条数据
                {
                    SerialNumber = mysqldr["serial_number"].ToString();
                    ChipId = mysqldr["chip_id"].ToString();
                }
            }
            catch (Exception e)
            {
                txtMessage.AddMessage(e.ToString(), true);
                mysql.Close();
                return string.Empty;
            }
            mysql.Close();
            if (SerialNumber == string.Empty)
            {
                txtMessage.AddMessage("没有获取到烧号信息或盒子对号未通过!!!", true);
                ClearScan();
                txtScan1.SetFoucs();
                SetErrorOrSuccImage(true, false);
                return string.Empty;
            }
            else
            {
                if (ZhiLiBegin != string.Empty && ZhiLiEnd != string.Empty)
                {
                    //if (ZhiLiBegin == string.Empty || ZhiLiEnd == string.Empty)
                    //{
                    //    txtMessage.AddMessage("智利项目没有设置stbid范围！！！！！！！！！！！！", true);
                    //    ClearScan();
                    //    txtScan1.SetFoucs();
                    //    SetErrorOrSuccImage(true, false);
                    //    return string.Empty;
                    //}
                    //else 
                    if (String.Compare(ChipId, ZhiLiBegin) < 0 || String.Compare(ChipId, ZhiLiEnd) > 0)
                    {
                        txtMessage.AddMessage("ChipId不在设置的范围[" + ZhiLiBegin + "~" + ZhiLiEnd + "]内！！！", true);
                        ClearScan();
                        txtScan1.SetFoucs();
                        SetErrorOrSuccImage(true, false);
                        return string.Empty;
                    }
                }
            }
            return SerialNumber;
        }
        public bool Updata6616Card(string SN)
        {
            string SerialNumber = string.Empty;
            string ChipId = string.Empty;
            string StbId = string.Empty;
            //在恢复出厂的数据库中查找是否已经恢复成功
            mysqlStr = "Database=serialnumber;Data Source=" + FactoryServerIp + ";User Id=root;Password=123456;pooling=false;CharSet=utf8;port=3306";
            mysql = new MySqlConnection(mysqlStr);
            mySqlCommand = new MySqlCommand("SELECT serial_number,chip_id,stb_id FROM product_vn_dvt_gx6616_32 WHERE chip_id = '" + SN + "' OR stb_id = '" + SN + "' OR serial_number='" + SN + "'", mysql);
            try
            {
                //获得读取结果
                mysql.Open();
                MySqlDataReader mysqldr = mySqlCommand.ExecuteReader();
                while (mysqldr.Read())//mysqldr.Read()返回的是bool值，意在判断是否有下一条数据
                {
                    SerialNumber = mysqldr["serial_number"].ToString();
                    ChipId =mysqldr["chip_id"].ToString();
                    StbId= mysqldr["stb_id"].ToString();
                }
            }
            catch (Exception e)
            {
                txtMessage.AddMessage(e.ToString(), true);
                mysql.Close();
                return false;
            }
            if(SerialNumber!=string.Empty&& ChipId!=string.Empty&& StbId!=string.Empty)
            {
                string myStbId = StbId;
                string myChipId = ChipId;
                ChipId = "C " + ChipId + " 0";
                ChipId=ChipId.Insert(6, " ");
                ChipId=ChipId.Insert(11, " ");
                myStbId = myStbId.Substring(8);//尼日利亚使用8位
                Parameters bindinzhili = new Parameters();
                bindinzhili.Add("myChipId", myChipId);
                bindinzhili.Add("Sn", SerialNumber);
                bindinzhili.Add("ChipId", ChipId);
                bindinzhili.Add("myStbId", myStbId);
                bindinzhili.Add("StbId", StbId);
                bindinzhili.Add("MOId", SelectRow["MOId"]);
                bindinzhili.Add("OPId", Framework.App.Resource.StationId);
                bindinzhili.Add("Return_Message", null, SqlDbType.NVarChar, int.MaxValue, ParameterDirection.Output);
                bindinzhili.Add("Return_Value", null, SqlDbType.Int, ParameterDirection.Output);
                Result<Parameters, DataSet> result = null;
                try
                {
                    result = DB.DBHelper.ExecuteParametersSource("Pro_BindZhiLi", bindinzhili, ExecuteType.StoredProcedure);
                }
                catch (Exception ex)
                {
                    txtMessage.AddMessage(ex.Message, true);
                    ClearScan();
                    txtScan1.SetFoucs();
                    SetErrorOrSuccImage(true, false);
                    return false;
                }
                if (result.HasError)
                {
                    txtMessage.AddMessage(result.Message, true);
                    ClearScan();
                    txtScan1.SetFoucs();
                    SetErrorOrSuccImage(true, false);
                    return false;
                }
                if ((int)result.Value1["Return_Value"] != 1)
                {
                    txtMessage.AddMessage(result.Value1["Return_Message"].ToString(), true);
                    ClearScan();
                    txtScan1.SetFoucs();
                    SetErrorOrSuccImage(true, false);
                    return false;
                }
                else
                {
                    txtMessage.AddMessage(result.Value1["Return_Message"].ToString(), false);
                    SetErrorOrSuccImage(false, false);
                    return true;
                }
            }
            return false;
        }

        public bool Updata6615Card(string SN)
        {
            string SerialNumber = string.Empty;
            string ChipId = string.Empty;
            string CaId = string.Empty;
            //在恢复出厂的数据库中查找是否已经恢复成功
            mysqlStr = "Database=serialnumber;Data Source=" + FactoryServerIp + ";User Id=root;Password=123456;pooling=false;CharSet=utf8;port=3306";
            mysql = new MySqlConnection(mysqlStr);
            mySqlCommand = new MySqlCommand("SELECT serial_number,chip_id,ca_id FROM product_vn_dvt_gx6615_19 WHERE chip_id = '" + SN + "' OR ca_id = '" + SN + "' OR serial_number='" + SN + "'", mysql);
            try
            {
                //获得读取结果
                mysql.Open();
                MySqlDataReader mysqldr = mySqlCommand.ExecuteReader();
                while (mysqldr.Read())//mysqldr.Read()返回的是bool值，意在判断是否有下一条数据
                {
                    SerialNumber = mysqldr["serial_number"].ToString();
                    ChipId = mysqldr["chip_id"].ToString();
                    CaId = mysqldr["ca_id"].ToString();
                }
            }
            catch (Exception e)
            {
                txtMessage.AddMessage(e.ToString(), true);
                mysql.Close();
                return false;
            }
            if (SerialNumber != string.Empty && ChipId != string.Empty && CaId != string.Empty)
            {
                //string myStbId = StbId;
                //string myChipId = ChipId;
                //ChipId = "C " + ChipId + " 0";
                //ChipId = ChipId.Insert(6, " ");
                //ChipId = ChipId.Insert(11, " ");
               // myStbId = myStbId.Substring(8);//尼日利亚使用8位
                Parameters bindinzhili = new Parameters();
                bindinzhili.Add("myChipId", string.Empty);
                bindinzhili.Add("Sn", SerialNumber);
                bindinzhili.Add("StbId", CaId);
                bindinzhili.Add("ChipId", ChipId);
                bindinzhili.Add("myStbId", string.Empty);
                
                bindinzhili.Add("MOId", SelectRow["MOId"]);
                bindinzhili.Add("OPId", Framework.App.Resource.StationId);
                bindinzhili.Add("Return_Message", null, SqlDbType.NVarChar, int.MaxValue, ParameterDirection.Output);
                bindinzhili.Add("Return_Value", null, SqlDbType.Int, ParameterDirection.Output);
                Result<Parameters, DataSet> result = null;
                try
                {
                    result = DB.DBHelper.ExecuteParametersSource("Pro_BindZhiLi", bindinzhili, ExecuteType.StoredProcedure);
                }
                catch (Exception ex)
                {
                    txtMessage.AddMessage(ex.Message, true);
                    ClearScan();
                    txtScan1.SetFoucs();
                    SetErrorOrSuccImage(true, false);
                    return false;
                }
                if (result.HasError)
                {
                    txtMessage.AddMessage(result.Message, true);
                    ClearScan();
                    txtScan1.SetFoucs();
                    SetErrorOrSuccImage(true, false);
                    return false;
                }
                if ((int)result.Value1["Return_Value"] != 1)
                {
                    txtMessage.AddMessage(result.Value1["Return_Message"].ToString(), true);
                    ClearScan();
                    txtScan1.SetFoucs();
                    SetErrorOrSuccImage(true, false);
                    return false;
                }
                else
                {
                    txtMessage.AddMessage(result.Value1["Return_Message"].ToString(), false);
                    SetErrorOrSuccImage(false, false);
                    return true;
                }
            }
            return false;
        }
        public string GetZhiLiSerialNumber(string WaitForString)
        {
            string SerialNumber = string.Empty;
            string StbId = string.Empty;
            //在恢复出厂的数据库中查找是否已经恢复成功       AND burn_status=1 
            mysqlStr = "Database=serialnumber;Data Source=" + FactoryServerIp + ";User Id=root;Password=123456;pooling=false;CharSet=utf8;port=3306";
            mysql = new MySqlConnection(mysqlStr);
            mySqlCommand = new MySqlCommand("SELECT serial_number,stb_id FROM product_vn_dvt_gx6616_32 WHERE chip_id = '" + WaitForString + "' OR stb_id = '" + WaitForString + "' OR serial_number='"+ WaitForString + "' AND burn_status=1 ", mysql);
            try
            {
                //获得读取结果
                mysql.Open();
                MySqlDataReader mysqldr = mySqlCommand.ExecuteReader();
                while (mysqldr.Read())//mysqldr.Read()返回的是bool值，意在判断是否有下一条数据
                {
                    SerialNumber = mysqldr["serial_number"].ToString();
                    StbId= mysqldr["stb_id"].ToString();
                }
            }
            catch (Exception e)
            {
                txtMessage.AddMessage(e.ToString(), true);
                mysql.Close();
                return string.Empty;
            }
            mysql.Close();
            if (SerialNumber == string.Empty)
            {
                txtMessage.AddMessage("没有获取到烧号信息或盒子没有对号通过!!!", true);
                ClearScan();
                txtScan1.SetFoucs();
                SetErrorOrSuccImage(true, false);
                return string.Empty;
            }
            else
            {
                if (ZhiLiBegin == string.Empty || ZhiLiEnd == string.Empty)
                {
                    txtMessage.AddMessage("智利项目没有设置stbid范围！！！！！！！！！！！！", true);
                    ClearScan();
                    txtScan1.SetFoucs();
                    SetErrorOrSuccImage(true, false);
                    return string.Empty;
                }
                else if(String.Compare(StbId,ZhiLiBegin)<0|| String.Compare(StbId, ZhiLiEnd)> 0)
                {
                    txtMessage.AddMessage("stbid不在设置的范围["+ZhiLiBegin+"~"+ZhiLiEnd+"]内！！！", true);
                    ClearScan();
                    txtScan1.SetFoucs();
                    SetErrorOrSuccImage(true, false);
                    return string.Empty;
                }
            }
            
            return SerialNumber;
        }
        public bool UpdateGSATCard(string sn)
        {
            string CardNumber = string.Empty;
            //在恢复出厂的数据库中查找是否已经恢复成功
            mysqlStr = "Database=serialnumber;Data Source=" + FactoryServerIp + ";User Id=root;Password=123456;pooling=false;CharSet=utf8;port=3306";
            mysql = new MySqlConnection(mysqlStr);
            mySqlCommand = new MySqlCommand("SELECT m_card FROM gsat_stb WHERE m_sn = '" + sn + "' OR m_card = '" + sn + "'", mysql);
            try
            {
                //获得读取结果
                mysql.Open();
                MySqlDataReader mysqldr = mySqlCommand.ExecuteReader();
                while (mysqldr.Read())//mysqldr.Read()返回的是bool值，意在判断是否有下一条数据
                {
                    CardNumber = mysqldr["m_card"].ToString();
                }
            }
            catch (Exception e)
            {
                txtMessage.AddMessage(e.ToString(), true);
                mysql.Close();
                return false;
            }
            mysql.Close();
            if (CardNumber == string.Empty)
            {
                txtMessage.AddMessage("没有获取到正确的卡号信息!!!", true);
                ClearScan();
                txtScan1.SetFoucs();
                SetErrorOrSuccImage(true, false);
                return false;
            }
            else
            {
                Parameters bindingwifi = new Parameters();
                bindingwifi.Add("Card", CardNumber);
                bindingwifi.Add("Sn", sn);
                bindingwifi.Add("MOId", SelectRow["MOId"]);
                bindingwifi.Add("OPId", Framework.App.Resource.StationId);
                bindingwifi.Add("Return_Message", null, SqlDbType.NVarChar, int.MaxValue, ParameterDirection.Output);
                bindingwifi.Add("Return_Value", null, SqlDbType.Int, ParameterDirection.Output);
                Result<Parameters, DataSet> result = null;
                try
                {
                    result = DB.DBHelper.ExecuteParametersSource("Pro_Inp_BindCard", bindingwifi, ExecuteType.StoredProcedure);
                }
                catch (Exception ex)
                {
                    txtMessage.AddMessage(ex.Message, true);
                    ClearScan();
                    txtScan1.SetFoucs();
                    SetErrorOrSuccImage(true, false);
                    return false;
                }
                if (result.HasError)
                {
                    txtMessage.AddMessage(result.Message, true);
                    ClearScan();
                    txtScan1.SetFoucs();
                    SetErrorOrSuccImage(true, false);
                    return false;
                }
                if ((int)result.Value1["Return_Value"] != 1)
                {
                    txtMessage.AddMessage(result.Value1["Return_Message"].ToString(), true);
                    ClearScan();
                    txtScan1.SetFoucs();
                    SetErrorOrSuccImage(true, false);
                    return false;
                }
                else
                {
                    txtMessage.AddMessage(result.Value1["Return_Message"].ToString(), false);
                    SetErrorOrSuccImage(false, false);
                    return true;
                }
            }
            return true;
        }
        public bool UpdateDrPengWifiMac(string WaitForUpdate)
        {
            string WifiMac = string.Empty;
            //在恢复出厂的数据库中查找是否已经恢复成功
            mysqlStr = "Database=serialnumber;Data Source=" + FactoryServerIp + ";User Id=root;Password=123456;pooling=false;CharSet=utf8;port=3306";
            mysql = new MySqlConnection(mysqlStr);
            mySqlCommand = new MySqlCommand("SELECT wifimac FROM product_sn_iptv_9385_13_19 WHERE serial_number = '" + WaitForUpdate + "' OR mac = '" + WaitForUpdate + "'", mysql);
            try
            {
                //获得读取结果
                mysql.Open();
                MySqlDataReader mysqldr = mySqlCommand.ExecuteReader();
                while (mysqldr.Read())//mysqldr.Read()返回的是bool值，意在判断是否有下一条数据
                {
                    WifiMac = mysqldr["wifimac"].ToString();
                }
            }
            catch (Exception e)
            {
                txtMessage.AddMessage(e.ToString(),true);
                mysql.Close();
                return false;
            }
            mysql.Close();
            if(WifiMac==string .Empty)
            {
                txtMessage.AddMessage("没有获取到正确的WiFi模块mac地址!!!", true);
                return false;
            }
            else
            {
                string sql = @"UPDATE dbo.Bas_MO_Mac SET WirelessNetName='"+ WifiMac + "' WHERE Mac='"+ WaitForUpdate + "' OR DeviceSerialNumber='"+ WaitForUpdate + "' AND MOId=@MOId";
                Parameters parameters = new Parameters();
                parameters.Add("MOId", SelectRow["MOId"]);
                try
                {
                    DataTable source = DB.DBHelper.GetDataTable(sql, parameters, ExecuteType.Text);
                }
                catch (Exception e)
                {
                    txtMessage.AddMessage(e.ToString(), true);
                    return false;
                }
            }
           return true;
        }
        //根据扫描到的数据（MAC\DSN\DeviceSerialNumber\STBNO首先进入MES中查到到STBNO，因为在恢复出厂的数据库中只有STBNO作为标记）然后进入恢复出厂数据库中查找这个号码是否恢复成功
        public long QueryRestoreStatus(string data)
        {
            string SerialNum = null;
            long Count = 0;
            //在mes中查找STBNO
            string sql = @"SELECT STBNO FROM [dbo].[Bas_MO_Mac] WHERE DSN='" + data + "' OR DeviceSerialNumber='" + data + "' OR STBNO='" + data + "'";
            Parameters parameters = new Parameters();
            //parameters.Add("ItemId", SelectRow["ItemId"]);
            //parameters.Add("OPId", Framework.App.Resource.StationId);
            try
            {
                DataTable source = DB.DBHelper.GetDataTable(sql, parameters, ExecuteType.Text);
                if (source.Rows.Count > 0)
                {
                    DataRowView SelectRow = source.DefaultView[0];
                    SerialNum = SelectRow[0].ToString();
                }
                else
                {
                    return -1;
                }
            }
            catch (Exception e)
            {
                txtMessage.AddMessage(e.ToString(), true);
                return -2;
            }
            //在恢复出厂的数据库中查找是否已经恢复成功
            mysqlStr = "Database=ott_package;Data Source=" + FactoryServerIp + ";User Id=root;Password=123456;pooling=false;CharSet=utf8;port=3306";
            mysql = new MySqlConnection(mysqlStr);
            mySqlCommand = new MySqlCommand("SELECT COUNT(*)FROM restore_order_all_t where SerialNum='" + SerialNum + "'", mysql);
            try
            {
                mysql.Open();
                Count = Convert.ToInt32(mySqlCommand.ExecuteScalar().ToString());
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                mysql.Close();
            }
            mysql.Close();
            return Count;
        }
        private void ClearScan()
        {
            txtScan1.Text = string.Empty;
            txtScan2.Text = string.Empty;
            txtScan3.Text = string.Empty;
            txtScan4.Text = string.Empty;
            txtScan5.Text = string.Empty;
            txtScan6.Text = string.Empty;
            txtTwoDimensionalCode.Text = string.Empty;
        }

        public void AddMessage(string message, bool isError)
        {
            txtMessage.AddMessage(message, isError);

            SetErrorOrSuccImage(isError, false);
        }

        private void txtScan1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //此处为了兼容华电打印机身及彩盒时要扫描二维码。二维码同时带有mac及sn，格式为mac_sn,因为这个sn不在系统内，所以在扫描到之后要丢弃
                if (txtScan1.Text.IndexOf("_") > -1)
                {
                    txtScan1.Text = txtScan1.Text.Substring(0, txtScan1.Text.IndexOf("_"));
                }
                if (ckbIsScan2.IsChecked.Value)
                {
                    txtScan2.Text = string.Empty;
                    txtScan2.SetFoucs();
                }
                else if (ckbIsScan3.IsChecked.Value)
                {
                    txtScan3.Text = string.Empty;
                    txtScan3.SetFoucs();
                }
                else if (ckbIsScan4.IsChecked.Value)
                {
                    txtScan4.Text = string.Empty;
                    txtScan4.SetFoucs();
                }
                else if (ckbIsScan5.IsChecked.Value)
                {
                    txtScan5.Text = string.Empty;
                    txtScan5.SetFoucs();
                }
                else if (ckbIsScan6.IsChecked.Value)
                {
                    txtScan6.Text = string.Empty;
                    txtScan6.SetFoucs();
                }
                else if (IsCheckTwoDimensionalCode)
                {
                    txtTwoDimensionalCode.Text = string.Empty;
                    txtTwoDimensionalCode.SetFoucs();
                }
                else
                {
                    Submit();
                }
            }
        }

        private void txtScan2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //此处为了兼容华电打印机身及彩盒时要扫描二维码。二维码同时带有mac及sn，格式为mac_sn,因为这个sn不在系统内，所以在扫描到之后要丢弃
                if (txtScan2.Text.IndexOf("_") > -1)
                {
                    txtScan2.Text = txtScan2.Text.Substring(0, txtScan2.Text.IndexOf("_"));
                }
                if (ckbIsScan3.IsChecked.Value)
                {
                    txtScan3.Text = string.Empty;
                    txtScan3.SetFoucs();
                }
                else if (ckbIsScan4.IsChecked.Value)
                {
                    txtScan4.Text = string.Empty;
                    txtScan4.SetFoucs();
                }
                else if (ckbIsScan5.IsChecked.Value)
                {
                    txtScan5.Text = string.Empty;
                    txtScan5.SetFoucs();
                }
                else if (ckbIsScan6.IsChecked.Value)
                {
                    txtScan6.Text = string.Empty;
                    txtScan6.SetFoucs();
                }
                else if (IsCheckTwoDimensionalCode)
                {
                    txtTwoDimensionalCode.Text = string.Empty;
                    txtTwoDimensionalCode.SetFoucs();
                }
                else
                {
                    Submit();
                }
            }
        }

        private void txtScan3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //此处为了兼容华电打印机身及彩盒时要扫描二维码。二维码同时带有mac及sn，格式为mac_sn,因为这个sn不在系统内，所以在扫描到之后要丢弃
                if (txtScan3.Text.IndexOf("_") > -1)
                {
                    txtScan3.Text = txtScan3.Text.Substring(0, txtScan3.Text.IndexOf("_"));
                }
                if (ckbIsScan4.IsChecked.Value)
                {
                    txtScan4.Text = string.Empty;
                    txtScan4.SetFoucs();
                }
                else if (ckbIsScan5.IsChecked.Value)
                {
                    txtScan5.Text = string.Empty;
                    txtScan5.SetFoucs();
                }
                else if (ckbIsScan6.IsChecked.Value)
                {
                    txtScan6.Text = string.Empty;
                    txtScan6.SetFoucs();
                }
                else if (IsCheckTwoDimensionalCode)
                {
                    txtTwoDimensionalCode.Text = string.Empty;
                    txtTwoDimensionalCode.SetFoucs();
                }
                else
                {
                    Submit();
                }
            }
        }

        private void txtScan4_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //此处为了兼容华电打印机身及彩盒时要扫描二维码。二维码同时带有mac及sn，格式为mac_sn,因为这个sn不在系统内，所以在扫描到之后要丢弃
                if (txtScan4.Text.IndexOf("_") > -1)
                {
                    txtScan4.Text = txtScan4.Text.Substring(0, txtScan4.Text.IndexOf("_"));
                }
                if (ckbIsScan5.IsChecked.Value)
                {
                    txtScan5.Text = string.Empty;
                    txtScan5.SetFoucs();
                }
                else if (ckbIsScan6.IsChecked.Value)
                {
                    txtScan6.Text = string.Empty;
                    txtScan6.SetFoucs();
                }
                else if (IsCheckTwoDimensionalCode)
                {
                    txtTwoDimensionalCode.Text = string.Empty;
                    txtTwoDimensionalCode.SetFoucs();
                }
                else
                {
                    Submit();
                }
            }
        }

        private void txtScan5_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //此处为了兼容华电打印机身及彩盒时要扫描二维码。二维码同时带有mac及sn，格式为mac_sn,因为这个sn不在系统内，所以在扫描到之后要丢弃
                if (txtScan5.Text.IndexOf("_") > -1)
                {
                    txtScan5.Text = txtScan5.Text.Substring(0, txtScan5.Text.IndexOf("_"));
                }
                if (ckbIsScan6.IsChecked.Value)
                {
                    txtScan6.Text = string.Empty;
                    txtScan6.SetFoucs();
                }
                else if (IsCheckTwoDimensionalCode)
                {
                    txtTwoDimensionalCode.Text = string.Empty;
                    txtTwoDimensionalCode.SetFoucs();
                }
                else
                {
                    Submit();
                }
            }
        }

        private void txtScan6_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //此处为了兼容华电打印机身及彩盒时要扫描二维码。二维码同时带有mac及sn，格式为mac_sn,因为这个sn不在系统内，所以在扫描到之后要丢弃
                if (txtScan6.Text.IndexOf("_") > -1)
                {
                    txtScan6.Text = txtScan6.Text.Substring(0, txtScan6.Text.IndexOf("_"));
                }
                if (IsCheckTwoDimensionalCode)
                {
                    txtTwoDimensionalCode.Text = string.Empty;
                    txtTwoDimensionalCode.SetFoucs();
                }
                else
                {
                    Submit();
                }
            }
        }

        private void txtTwoDimensionalCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (TwoDimensionalCodeLength <=0)
                {
                    Submit();
                }
                else if (txtTwoDimensionalCode.Text.Length >= TwoDimensionalCodeLength)
                {
                    Submit();
                }
            }
        }

        public bool IsCanCloseControl(string header, ref string tipMessage)
        {
            return true;
        }

        private void ckbIsScan2_Checked(object sender, RoutedEventArgs e)
        {
            if (lblScanType2 != null && cmbType2 != null && lblScan2 != null && txtScan2 != null)
            {
                if (ckbIsScan2.IsChecked.Value)
                {
                    lblScanType2.Visibility = Visibility.Visible;
                    cmbType2.Visibility = Visibility.Visible;
                    lblScan2.Visibility = Visibility.Visible;
                    txtScan2.Visibility = Visibility.Visible;
                }
                else
                {
                    lblScanType2.Visibility = Visibility.Collapsed;
                    cmbType2.Visibility = Visibility.Collapsed;
                    lblScan2.Visibility = Visibility.Collapsed;
                    txtScan2.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void ckbIsScan3_Checked(object sender, RoutedEventArgs e)
        {
            if (lblScanType3 != null && cmbType3 != null && lblScan3 != null && txtScan3 != null)
            {
                if (ckbIsScan3.IsChecked.Value)
                {
                    lblScanType3.Visibility = Visibility.Visible;
                    cmbType3.Visibility = Visibility.Visible;
                    lblScan3.Visibility = Visibility.Visible;
                    txtScan3.Visibility = Visibility.Visible;
                }
                else
                {
                    lblScanType3.Visibility = Visibility.Collapsed;
                    cmbType3.Visibility = Visibility.Collapsed;
                    lblScan3.Visibility = Visibility.Collapsed;
                    txtScan3.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void ckbIsScan4_Checked(object sender, RoutedEventArgs e)
        {
            if (lblScanType4 != null && cmbType4 != null && lblScan4 != null && txtScan4 != null)
            {
                if (ckbIsScan4.IsChecked.Value)
                {
                    lblScanType4.Visibility = Visibility.Visible;
                    cmbType4.Visibility = Visibility.Visible;
                    lblScan4.Visibility = Visibility.Visible;
                    txtScan4.Visibility = Visibility.Visible;
                }
                else
                {
                    lblScanType4.Visibility = Visibility.Collapsed;
                    cmbType4.Visibility = Visibility.Collapsed;
                    lblScan4.Visibility = Visibility.Collapsed;
                    txtScan4.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void ckbIsScan5_Checked(object sender, RoutedEventArgs e)
        {
            if (lblScanType5 != null && cmbType5 != null && lblScan5 != null && txtScan5 != null)
            {
                if (ckbIsScan5.IsChecked.Value)
                {
                    lblScanType5.Visibility = Visibility.Visible;
                    cmbType5.Visibility = Visibility.Visible;
                    lblScan5.Visibility = Visibility.Visible;
                    txtScan5.Visibility = Visibility.Visible;
                }
                else
                {
                    lblScanType5.Visibility = Visibility.Collapsed;
                    cmbType5.Visibility = Visibility.Collapsed;
                    lblScan5.Visibility = Visibility.Collapsed;
                    txtScan5.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void ckbIsScan6_Checked(object sender, RoutedEventArgs e)
        {
            if (lblScanType6 != null && cmbType6 != null && lblScan6 != null && txtScan6 != null)
            {
                if (ckbIsScan6.IsChecked.Value)
                {
                    lblScanType6.Visibility = Visibility.Visible;
                    cmbType6.Visibility = Visibility.Visible;
                    lblScan6.Visibility = Visibility.Visible;
                    txtScan6.Visibility = Visibility.Visible;
                }
                else
                {
                    lblScanType6.Visibility = Visibility.Collapsed;
                    cmbType6.Visibility = Visibility.Collapsed;
                    lblScan6.Visibility = Visibility.Collapsed;
                    txtScan6.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void IsConnect_Checked(object sender, RoutedEventArgs e)
        {
          
        }

        private void IsConnect_Click(object sender, RoutedEventArgs e)
        {
            bool Status;
            Component.Windows.AuthorityLogin login = new Component.Windows.AuthorityLogin(MenuId, '4');
            login.Owner = Component.App.Portal;
            Status = IsConnect.IsChecked.Value;
            if (!login.ShowDialog()||(login.UserId!=1016))
            {
                IsConnect.IsChecked= !Status;
            }
            else
            {
                IsConnect.IsChecked = Status;
            }
            if (IsConnect.IsChecked== true)
            {
                WinAPI.File.INIFileHelper.Write("DataBaseConfig", "IsDateConnect", "1", setting_path);
            }
            else
            {
                WinAPI.File.INIFileHelper.Write("DataBaseConfig", "IsDateConnect", "0", setting_path);
            }
        }

    }
}
