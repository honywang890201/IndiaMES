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

namespace PCBATransfer
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
        private bool IsGx6615 = false;//智利项目判断标志位
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

            //cmbType1.ItemsSource = source1;

            try
            {
              // cmbType1.SelectedValue = WinAPI.File.INIFileHelper.Read("ScanType", string.Format("P{0}-1", MenuId), setting_path, string.Empty);
               // cmbType1_SelectionChanged(cmbType1, null);
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

            //cmbType2.ItemsSource = source2;

            try
            {
                string v = WinAPI.File.INIFileHelper.Read("ScanType", string.Format("P{0}-IsScan2", MenuId), setting_path, string.Empty);
                bool b = false;
                bool.TryParse(v, out b);
               // ckbIsScan2.IsChecked = b;

               // cmbType2.SelectedValue = WinAPI.File.INIFileHelper.Read("ScanType", string.Format("P{0}-2", MenuId), setting_path, string.Empty);
               // cmbType2_SelectionChanged(cmbType2, null);
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

            //cmbType3.ItemsSource = source3;

            try
            {
                string v = WinAPI.File.INIFileHelper.Read("ScanType", string.Format("P{0}-IsScan3", MenuId), setting_path, string.Empty);
                bool b = false;
                bool.TryParse(v, out b);
                //ckbIsScan3.IsChecked = b;

               // cmbType3.SelectedValue = WinAPI.File.INIFileHelper.Read("ScanType", string.Format("P{0}-3", MenuId), setting_path, string.Empty);
               // cmbType3_SelectionChanged(cmbType3, null);
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

            //cmbType4.ItemsSource = source4;

            try
            {
                string v = WinAPI.File.INIFileHelper.Read("ScanType", string.Format("P{0}-IsScan4", MenuId), setting_path, string.Empty);
                bool b = false;
                bool.TryParse(v, out b);
                //ckbIsScan4.IsChecked = b;

               // cmbType4.SelectedValue = WinAPI.File.INIFileHelper.Read("ScanType", string.Format("P{0}-4", MenuId), setting_path, string.Empty);
               // cmbType4_SelectionChanged(cmbType4, null);
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

            //cmbType5.ItemsSource = source5;

            try
            {
                string v = WinAPI.File.INIFileHelper.Read("ScanType", string.Format("P{0}-IsScan5", MenuId), setting_path, string.Empty);
                bool b = false;
                bool.TryParse(v, out b);
               // ckbIsScan5.IsChecked = b;

               // cmbType5.SelectedValue = WinAPI.File.INIFileHelper.Read("ScanType", string.Format("P{0}-5", MenuId), setting_path, string.Empty);
               // cmbType5_SelectionChanged(cmbType5, null);
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

            //cmbType6.ItemsSource = source6;

            try
            {
                string v = WinAPI.File.INIFileHelper.Read("ScanType", string.Format("P{0}-IsScan6", MenuId), setting_path, string.Empty);
                bool b = false;
                bool.TryParse(v, out b);
               // ckbIsScan6.IsChecked = b;
               // cmbType6.SelectedValue = WinAPI.File.INIFileHelper.Read("ScanType", string.Format("P{0}-6", MenuId), setting_path, string.Empty);
               // cmbType6_SelectionChanged(cmbType6, null);
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
       Bas_MO.ZhiLiBegin,
       Bas_MO.ZhiLiEnd,
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
      
      
       
       
        public void Submit()
        {
            if(txtScan1.Text.Trim()==string.Empty)
            {
                txtMessage.AddMessage("扫描的数据为空!!!", true);
                SetErrorOrSuccImage(true, false);
                txtScan1.SetFoucs();
                return;
            }
            Parameters parameters = new Parameters();
            //parameters.Add("TwoDimensionalCode", IsCheckTwoDimensionalCode ? txtTwoDimensionalCode.Text.Trim() : (object)DBNull.Value, SqlDbType.NVarChar, int.MaxValue);
            parameters.Add("MOId", SelectRow["MOId"]);
            parameters.Add("SN", txtScan1.Text.Trim());
            parameters.Add("ItemId", SelectRow["ItemId"]);
            parameters.Add("LineId", Framework.App.Resource.LineId);
            parameters.Add("UserId", Framework.App.User.UserId);
            parameters.Add("OP", "SMT");
            parameters.Add("Return_Message", null, SqlDbType.NVarChar, int.MaxValue, ParameterDirection.Output);
            parameters.Add("Return_Value", null, SqlDbType.Int, ParameterDirection.ReturnValue);
            parameters.Add("MOQty", null, SqlDbType.Int, ParameterDirection.Output);
            parameters.Add("TodayQty", null, SqlDbType.Int, ParameterDirection.Output);
            parameters.Add("TotalQty", null, SqlDbType.Int, ParameterDirection.Output);
            Result<Parameters, DataSet> result = null;
            try
            {
                result = DB.DBHelper.ExecuteParametersSource("Inp_Pro_PCBA_Transfer_P", parameters, ExecuteType.StoredProcedure);
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
                MOTotal.Text ="订单总数:"+ result.Value1["MOQty"].ToString();
                MOComp.Text= "订单完成:"+result.Value1["TotalQty"].ToString();
                MOTaday.Text= "今日完成:"+result.Value1["TodayQty"].ToString();
                ClearScan();
                txtScan1.SetFoucs();
                SetErrorOrSuccImage(false, false);
                uph.Quantity++;
            }
        }
     
  

     
    
    


        //根据扫描到的数据（MAC\DSN\DeviceSerialNumber\STBNO首先进入MES中查到到STBNO，因为在恢复出厂的数据库中只有STBNO作为标记）然后进入恢复出厂数据库中查找这个号码是否恢复成功

        private void ClearScan()
        {
            txtScan1.Text = string.Empty;
           
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
                //if (ckbIsScan2.IsChecked.Value)
                //{
                //    txtScan2.Text = string.Empty;
                //    txtScan2.SetFoucs();
                //}
                //else if (ckbIsScan3.IsChecked.Value)
                //{
                //    txtScan3.Text = string.Empty;
                //    txtScan3.SetFoucs();
                //}
                //else if (ckbIsScan4.IsChecked.Value)
                //{
                //    txtScan4.Text = string.Empty;
                //    txtScan4.SetFoucs();
                //}
                //else if (ckbIsScan5.IsChecked.Value)
                //{
                //    txtScan5.Text = string.Empty;
                //    txtScan5.SetFoucs();
                //}
                //else if (ckbIsScan6.IsChecked.Value)
                //{
                //    txtScan6.Text = string.Empty;
                //    txtScan6.SetFoucs();
                //}
                //else if (IsCheckTwoDimensionalCode)
                //{
                //    txtTwoDimensionalCode.Text = string.Empty;
                //    txtTwoDimensionalCode.SetFoucs();
                //}
                else
                {
                    Submit();
                }
            }
        }  
    }
}
