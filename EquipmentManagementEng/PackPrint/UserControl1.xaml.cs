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
using System.Runtime.InteropServices;

namespace PackPrint
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
        private bool IsPrintAttch = false;


        /////////////////////////////////////////////////////////////////////////////////////////////////////
        MySqlDataReader reader = null;
        String mysqlStr = null; //"Database=ott_package;Data Source=192.168.0.119;User Id=root;Password=123456;pooling=false;CharSet=utf8;port=3306";
        MySqlConnection mysql = null;// new MySqlConnection(mysqlStr);
        MySqlCommand mySqlCommand = null;// new MySqlCommand("SELECT COUNT(*)FROM restore_order_all_t", mysql);
        //华电识别用，true时为华电
        private bool IsHuadian = false;
        private bool IsDrPengBuDing = false;//鹏博士小布丁项目判断标志，用于获取鹏博士烧号数据中的WiFi mac并上传至mes方便后期报盘文件导出
        private bool IsP60 = false;//P60项目判断使用，耦合测试时用的WiFimac作为过站标志。
        private long MOId = -1;

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


        private void UserVendor_Loaded(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("StationId:" + Framework.App.Resource.StationId.ToString() + "ShiftTypeId:" + Framework.App.Resource.ShiftTypeId.ToString() + "LineId:" + Framework.App.Resource.LineId.ToString()
            //   + "ResourceId:" + Framework.App.Resource.ResourceId.ToString() + "UserId:" + Framework.App.User.UserId.ToString());
            if (_IsLoad)
                return;
            _IsLoad = true;
            Match m = Regex.Match(Framework.App.ServiceUrl, @"\d{1,3}.\d{1,3}.\d{1,3}.\d{1,3}");
            if (m.Success)
            {
                MESServerIp = m.Value;
            }
            string[] arr=MESServerIp.Split('.');
            if(arr[2]=="0")
            {
                FactoryServerIp = "192.168.0.24";
            }
            if(arr[2] == "2")
            {
                FactoryServerIp = "192.168.2.11";
            }
            btnSelectorMO_Click(btnSelectorMO, null);
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

            try
            {
                string v = WinAPI.File.INIFileHelper.Read("ScanType", string.Format("P{0}-CheckEN", MenuId), setting_path, string.Empty);
                bool b = false;
                bool.TryParse(v, out b);
               // ckbIsBindEN.IsChecked = b;
            }
            catch
            {

            }
            //WinAPI.File.INIFileHelper.Write("DataBaseConfig", "IsDateConnect", "1", setting_path);
            if (WinAPI.File.INIFileHelper.Read("DataBaseConfig", "IsDateConnect", setting_path, "1")=="1")
            {
                //IsConnect.IsChecked = true;
            }
            else
            {
                //IsConnect.IsChecked = false;
            }

            List<KeyValuePair<string, string>> source4 = new List<KeyValuePair<string, string>>();
            source4.Add(new KeyValuePair<string, string>("PowerSN", "电源条码"));
            source4.Add(new KeyValuePair<string, string>("RemoteControl", "遥控器条码"));
            attachCmb.ItemsSource = source4;
            try
            {
                attachCmb.SelectedValue = WinAPI.File.INIFileHelper.Read("attachCmb", string.Format("P{0}-2", MenuId), setting_path, string.Empty);
                AttachCmb_SelectionChanged(attachCmb, null);
            }
            catch
            {

            }
            

        }

        private void LoadMO()
        {
            gridTemplate.Children.Clear();
            gridTemplate.RowDefinitions.Clear();
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
                ckbIsBindEN.Visibility = Visibility.Visible;
                IsENOnlineAssembly = false;
                btnAddPrint.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (SelectRow["ENType"].ToString().ToUpper() == "No".ToUpper())
                {
                    ckbIsBindEN.IsChecked = false;
                    ckbIsBindEN.Visibility = Visibility.Collapsed;
                    IsENOnlineAssembly = false;
                }
                else
                {
                    ckbIsBindEN.Visibility = Visibility.Visible;
                    ckbIsBindEN.IsChecked = true;
                    if (SelectRow["ENType"].ToString().ToUpper() == "OnLine".ToUpper())
                    {

                        IsENOnlineAssembly = true;
                    }
                    else
                    {

                        IsENOnlineAssembly = false;
                    }
                    
                }
                ckbIsBindEN_Checked(null,null);
                tbMOCode.Text = SelectRow["MOCode"].ToString();
                tbQty.Text = SelectRow["Qty"].ToString();
                tbItemCode.Text = SelectRow["ItemCode"].ToString();
                tbItemSpec.Text = SelectRow["ItemSpecification"].ToString();
                tbWorkflow.Text = SelectRow["WorkflowCode"].ToString();
                tbCustomer.Text = SelectRow["CustomerCode"].ToString();
                if (!string.IsNullOrEmpty(SelectRow["CustomerDesc"].ToString().Trim()))
                    tbCustomer.Text = tbCustomer.Text + "/" + SelectRow["CustomerDesc"].ToString();

                //if (!string.IsNullOrEmpty(SelectRow["IsPrintAttch"].ToString().Trim())&& (bool)SelectRow["IsPrintAttch"]==true)
                //{
                //    IsPrintAttch = true;
                //    attachType.Visibility = Visibility.Visible;
                //    attachCmb.Visibility = Visibility.Visible;
                //    attach.Visibility = Visibility.Visible;
                //    attachCode.Visibility = Visibility.Visible;
                //}
                //else
                //{
                //    IsPrintAttch = false;
                //    attachType.Visibility = Visibility.Collapsed;
                //    attachCmb.Visibility = Visibility.Collapsed;
                //    attach.Visibility = Visibility.Collapsed;
                //    attachCode.Visibility = Visibility.Collapsed;
                //}



                //    //兆能\鹏博士客户代码
                //    if ((SelectRow["CustomerCode"].ToString() == "0010")|| (SelectRow["CustomerCode"].ToString() == "00105"))
                //{
                //    IsConnect.IsChecked = true;
                //}
                ////鹏博士小布丁项目时
                //if (SelectRow["CustomerCode"].ToString() == "00107")
                //{
                //    IsDrPengBuDing = true;
                //}
                ////其他项目时
                //else
                //{
                //    IsDrPengBuDing = false;
                //}
                ////兆能魔百盒、P60项目时需要卡站耦合测试
                //if (SelectRow["CustomerCode"].ToString() == "00110")
                //{
                //    IsP60 = true;
                //}
                ////其他项目时
                //else
                //{
                //    IsP60 = false;
                //}
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
                        gridTemplate.RowDefinitions.Add(new RowDefinition());
                        TextBlock block = new TextBlock();
                        block.Text = "Choose printer";
                        block.FontSize = 16;
                        block.FontWeight = FontWeights.Bold;
                        block.HorizontalAlignment = HorizontalAlignment.Center;
                        block.SetValue(Grid.RowProperty, gridTemplate.RowDefinitions.Count - 1);
                        block.SetValue(Grid.ColumnProperty, 1);
                        gridTemplate.Children.Add(block);

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
                            if (helper.FileServerHelper != null)
                            {
                                buffer = helper.FileServerHelper.WriteFile(helper.ServerFileName);
                            }
                        }
                        catch (Exception ex)
                        {
                            txtMessage.AddMessage(ex.Message, true);
                            continue;
                        }
                        if (buffer == null)
                            continue;
                        string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Template", helper.FileName);
                        try
                        {
                            WinAPI.File.FileHelper.Write(buffer, path);
                        }
                        catch (Exception ex)
                        {
                            txtMessage.AddMessage(ex.Message, true);
                            continue;
                        }
                        gridTemplate.RowDefinitions.Add(new RowDefinition());
                        TextBlock block = new TextBlock();
                        string headerText = row["TemplateDesc"].ToString().Trim();
                        if (!string.IsNullOrEmpty(headerText))
                            headerText = headerText + "：";
                        block.Text = headerText;
                        block.HorizontalAlignment = HorizontalAlignment.Right;
                        block.VerticalAlignment = VerticalAlignment.Center;
                        block.SetValue(Grid.RowProperty, gridTemplate.RowDefinitions.Count - 1);
                        block.SetValue(Grid.ColumnProperty, 0);
                        gridTemplate.Children.Add(block);
                        ComboBox cmb = new ComboBox();
                        cmb.MinWidth = 200;
                        cmb.HorizontalAlignment = HorizontalAlignment.Left;
                        cmb.SetValue(Grid.RowProperty, gridTemplate.RowDefinitions.Count - 1);
                        cmb.SetValue(Grid.ColumnProperty, 1);
                        gridTemplate.Children.Add(cmb);
                        cmb.ItemsSource = WinAPI.Computer.LocalPrinters;
                        cmb.Tag = row["TemplateId"];
                        cmb.DataContext = row;
                        PrinterComboBoxs.Add(cmb);
                        string pName = null;
                        try
                        {
                            pName = WinAPI.File.INIFileHelper.Read("ScanType", string.Format("Printers{0}-{1}", MenuId, row["TemplateId"]), setting_path, string.Empty);
                            if (string.IsNullOrEmpty(pName))
                                pName = WinAPI.Computer.LocalPrinterName;

                            if (string.IsNullOrEmpty(pName))
                                cmb.SelectedValue = pName;
                        }
                        catch { }

                        cmb.SelectionChanged += new SelectionChangedEventHandler((sender, e) =>
                        {
                            if (cmb.SelectedValue != null)
                            {
                                DataRow _row = cmb.DataContext as DataRow;
                                WinAPI.File.INIFileHelper.Write("ScanType", string.Format("Printers{0}-{1}", MenuId, _row["TemplateId"]), cmb.SelectedValue.ToString(), setting_path);
                            }
                        });

                        try
                        {
                            Printer.Instance cs = Printer.Instance.Factory(path, null, null, 0);//
                            TemplatePath.Add((long)row["TemplateId"], cs);
                        }
                        catch (Exception ex)
                        {
                            txtMessage.AddMessage(ex.Message, true);
                        }
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                uph.Start();
            }
        }

        private void btnSelectorMO_Click(object sender, RoutedEventArgs e)
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
                        MessageBox.Show("Order error", "error!!!", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    SelectRow = table.DefaultView[0];
                    MOId = selector.moId;
                    //客户编号是00101为华电项目
                    if (SelectRow["CustomerCode"].ToString() == "00101")
                    {
                        //IsHuadian = true;
                    }
                    else
                    {
                        //IsHuadian = false;
                    }
                    LoadMO();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "error!!!", MessageBoxButton.OK, MessageBoxImage.Error);
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

        public override void CloseControl()
        {
            try
            {
                WinAPI.File.INIFileHelper.Write("ScanType", string.Format("P{0}-IsScan2", MenuId), ckbIsScan2.IsChecked.HasValue ? ckbIsScan2.IsChecked.Value.ToString() : false.ToString(), setting_path);
                WinAPI.File.INIFileHelper.Write("ScanType", string.Format("P{0}-IsScan3", MenuId), ckbIsScan3.IsChecked.HasValue ? ckbIsScan3.IsChecked.Value.ToString() : false.ToString(), setting_path);
                WinAPI.File.INIFileHelper.Write("ScanType", string.Format("P{0}-CheckEN", MenuId), ckbIsBindEN.IsChecked.HasValue ? ckbIsBindEN.IsChecked.Value.ToString() : false.ToString(), setting_path);
            }
            catch { }
            CloseTemplate();
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

        public void Submit()
        {
            if (string.IsNullOrEmpty(tbMOCode.Text.Trim()))
            {
                txtMessage.AddMessage("Choose order", true);
                btnSelectorMO.Focus();
                SetErrorOrSuccImage(true, false);
                return;
            }
            if (cmbType1.SelectedValue == null || cmbType1.SelectedItem == null)
            {
                txtMessage.AddMessage("Choose scan type 1", true);
                cmbType1.Focus();
                SetErrorOrSuccImage(true, false);
                return;
            }
            if (txtScan1.Text.Trim() == string.Empty)
            {
                txtMessage.AddMessage(string.Format("scan please{0}！", lblScan1.Text), true);
                txtScan1.Text = string.Empty;
                txtScan1.SetFoucs();
                SetErrorOrSuccImage(true, false);
                return;
            }

            if (ckbIsScan2.IsChecked.Value)
            {
                if (cmbType2.SelectedValue == null || cmbType2.SelectedItem == null)
                {
                    txtMessage.AddMessage("Choose scan type 2！", true);
                    cmbType2.Focus();
                    SetErrorOrSuccImage(true, false);
                    return;
                }
                if (txtScan2.Text.Trim() == string.Empty)
                {
                    txtMessage.AddMessage(string.Format("scan please{0}！", lblScan2.Text), true);
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
                    txtMessage.AddMessage("Choose scan type 3！", true);
                    cmbType3.Focus();
                    SetErrorOrSuccImage(true, false);
                    return;
                }
                if (txtScan3.Text.Trim() == string.Empty)
                {
                    txtMessage.AddMessage(string.Format("scan please{0}！", lblScan3.Text), true);
                    txtScan3.Text = string.Empty;
                    txtScan3.SetFoucs();
                    SetErrorOrSuccImage(true, false);
                    return;
                }
            }
            if (ckbIsBindEN.IsChecked.Value)
            {
                if (!IsENOnlineAssembly)
                {
                    if (txtEN.Text.Trim() == string.Empty)
                    {
                        txtMessage.AddMessage(string.Format("scan please EN！"), true);
                        txtEN.Text = string.Empty;
                        txtEN.SetFoucs();
                        SetErrorOrSuccImage(true, false);
                        return;
                    }
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
                parameters.Add("ScanCode2", txtScan2.Text.Trim(), "string", 500);
            }
            parameters.Add("IsScan3", ckbIsScan3.IsChecked.Value);
            if (ckbIsScan3.IsChecked.Value)
            {
                parameters.Add("ScanType3", cmbType3.SelectedValue.ToString());
                parameters.Add("ScanTypeDesc3", ((KeyValuePair<string, string>)cmbType3.SelectedItem).Value);
                parameters.Add("ScanCode3", txtScan3.Text.Trim());
            }

            parameters.Add("IsScanEN", ckbIsBindEN.IsChecked.Value);
            if (ckbIsBindEN.IsChecked.Value)
            {
                parameters.Add("EN", txtEN.Text.Trim());
            }
            string QueryResult = string.Empty;
            //如果是华电的时候，要先从恢复出厂数据库中找到所有数据（内部sn、mac、chipid）查询合法后上传至mes
            if(IsHuadian)
            {
               if(!QueryAndUpdate(txtScan1.Text.Trim(),ref QueryResult))
                {
                    txtMessage.AddMessage(QueryResult, true);
                    ClearScan();
                    txtScan1.SetFoucs();
                    SetErrorOrSuccImage(true, false);
                    return;
                }
               else
                {
                    txtMessage.AddMessage(QueryResult, false);
                }
            }
            //如果是要绑定EN且EN输入框为可见状态时要对比扫描到的数据是否和EN一致，如果不一致则强制返回。不进行绑定及mes过站
            if (ckbIsBindEN.IsChecked.Value && !IsENOnlineAssembly)
            {
                if (txtEN.Text.Trim() != txtScan1.Text.Trim())
                {
                    txtMessage.AddMessage("扫描的条码不一致请重新扫描", true);
                    ClearScan();
                    txtScan1.SetFoucs();
                    SetErrorOrSuccImage(true, false);
                    return;
                }
            }
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //如果连接恢复出厂数据库选项被激活，则先要到恢复出厂数据库中查找数据
            if (IsConnect.IsChecked.Value)
            {
                long CheckReslut = 0;
                CheckReslut = QueryRestoreStatus(txtScan1.Text.Trim());
                if(CheckReslut==-1)
                {
                    txtMessage.AddMessage("此号码没有存在MES中，请确认此号码是否存在于MES", true);
                    ClearScan();
                    txtScan1.SetFoucs();
                    SetErrorOrSuccImage(true, false);
                    return;
                }
                if (CheckReslut == 0)
                {
                    txtMessage.AddMessage("此机恢复出厂不成功!!!!,请确认后再次扫描", true);
                    ClearScan();
                    txtScan1.SetFoucs();
                    SetErrorOrSuccImage(true, false);
                    return;
                }
            }
            //如果此项目为鹏博士小布丁项目，则要在提交前获取WiFi的mac地址并上传至mes
            if (IsDrPengBuDing)
            {
                if (!UpdateDrPengWifiMac(txtScan1.Text.Trim()))
                {
                    return;
                }
            }
            //如果为P60则要查询是否耦合测试通过
            if(IsP60)
            {
                if(!QueryP60WifiMac(txtScan1.Text.Trim()))
                {
                    return;
                }
            }
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
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
                result = DB.DBHelper.ExecuteParametersSource("Prd_Inp_Pack_ScanPrint", parameters, ExecuteType.StoredProcedure);
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
            }
            uph.Quantity++;
            Print(result.Value2);

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
                string sql = @"SELECT IsUsed FROM Bas_P60WIFIMac WHERE MOId=@MOId AND WIFIMac='"+ WifiMac + "'";
                Parameters parameters = new Parameters();
                parameters.Add("MOId", MOId);
                try
                {
                    DataTable table = DB.DBHelper.GetDataTable(sql, parameters, ExecuteType.Text, null, true);
                    if (table.Rows.Count < 1)
                    {
                        txtMessage.AddMessage("此WiFi mac["+ WifiMac +"]耦合测试未通过，请查证!!!", true);
                        ClearScan();
                        txtScan1.SetFoucs();
                        return false;
                    }
                    Result = table.DefaultView[0];
                    if (Result["IsUsed"].ToString()== "True")
                    {
                        return true;
                    }
                    else
                    {

                        txtMessage.AddMessage("当前机顶盒["+ STBNO + "]耦合测试未通过，请返回耦合测试工位测试通过后再次测试！！！", true);
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
           // return true;
        
        //根据扫描到数据从鹏博士烧号数据库中找到和扫描到的数据对应的WiFi模块的mac地址，并将其更新到mes的数据库中，方便后统一从mes服务器导出报盘文件
        public bool UpdateDrPengWifiMac(string WaitForUpdate)
        {
            string WifiMac = string.Empty;
            //在恢复出厂的数据库中查找是否已经恢复成功
            mysqlStr = "Database=ott_package;Data Source=" + FactoryServerIp + ";User Id=root;Password=123456;pooling=false;CharSet=utf8;port=3306";
            mysql = new MySqlConnection(mysqlStr);
            mySqlCommand = new MySqlCommand("SELECT mac FROM restore_order_all_t WHERE SerialNum= '" + WaitForUpdate + "' OR mac = '" + WaitForUpdate + "'", mysql);
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
                txtMessage.AddMessage("没有获取到正确的WiFi模块mac地址!!!", true);
                ClearScan();
                txtScan1.SetFoucs();
                return false;
            }
            else
            {
                Parameters bindingwifi = new Parameters();
                bindingwifi.Add("Mac", WifiMac);
                bindingwifi.Add("Sn", WaitForUpdate);
                bindingwifi.Add("MOId", SelectRow["MOId"]);
                bindingwifi.Add("OPId", Framework.App.Resource.StationId);
                bindingwifi.Add("Return_Message", null, SqlDbType.NVarChar, int.MaxValue, ParameterDirection.Output);
                bindingwifi.Add("Return_Value", null, SqlDbType.Int, ParameterDirection.Output);
                Result<Parameters, DataSet> result = null;
                try
                {
                    result = DB.DBHelper.ExecuteParametersSource("Pro_Inp_BindWiFi_Mac", bindingwifi, ExecuteType.StoredProcedure);
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


        //根据扫描到的数据（MAC\DSN\DeviceSerialNumber\STBNO首先进入MES中查到到STBNO，因为在恢复出厂的数据库中只有STBNO作为标记）然后进入恢复出厂数据库中查找这个号码是否恢复成功
        public long  QueryRestoreStatus(string data)
        {
            string SerialNum = null;
            long Count = 0;
            //在mes中查找STBNO
            string sql = @"SELECT STBNO FROM [dbo].[Bas_MO_Mac] WHERE DSN='"+ data + "' OR DeviceSerialNumber='" + data + "' OR STBNO='" + data + "' OR MAC='"+ data + "'";
            Parameters parameters = new Parameters();
            //parameters.Add("ItemId", SelectRow["ItemId"]);
            //parameters.Add("OPId", Framework.App.Resource.StationId);
            try
            {
                DataTable source = DB.DBHelper.GetDataTable(sql, parameters, ExecuteType.Text);
                if (source.Rows.Count > 0)
                {
                    DataRowView SelectRow = source.DefaultView[0];
                    SerialNum=SelectRow[0].ToString();
                }
                else
                {
                    return -1;
                }
            }
            catch(Exception e)
            {
                txtMessage.AddMessage(e.ToString(), true);
                return -2;
            }
            //在恢复出厂的数据库中查找是否已经恢复成功
            mysqlStr = "Database=ott_package;Data Source="+FactoryServerIp+";User Id=root;Password=123456;pooling=false;CharSet=utf8;port=3306";
            mysql = new MySqlConnection(mysqlStr);
            mySqlCommand = new MySqlCommand("SELECT COUNT(*)FROM restore_order_all_t where SerialNum='"+ SerialNum + "'", mysql);
            try
            {
                mysql.Open();
                Count= Convert.ToInt32(mySqlCommand.ExecuteScalar().ToString());
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                mysql.Close();
            }
            mysql.Close();
            return Count;
        }
        //华电项目特殊处理，mes需要将盒子的mac，内部sn及chipid上传至mes。因为这些数据在恢复工厂自己的恢复出厂MySQL数据库，所以要跟据扫描到数据先从MySQL中拿到后再上传
        //返回值定义：
        //0:成功  -1：
        private bool QueryAndUpdate(string WaitForQuery, ref string  Result)
        {
            string H_Mac=string.Empty, H_Isn=string.Empty, H_ChipId=string.Empty;
            //在恢复出厂的数据库中查找是否已经恢复成功
            mysqlStr = "Database=ott_package;Data Source=" + FactoryServerIp + ";User Id=root;Password=123456;pooling=false;CharSet=utf8;port=3306";
            mysql = new MySqlConnection(mysqlStr);
            mySqlCommand = new MySqlCommand("SELECT sn, mac, chipid FROM huadian_mes_info WHERE mac = '" + WaitForQuery + "' OR chipid = '" + WaitForQuery + "' OR sn = '" + WaitForQuery + "'", mysql);
            try
            {
                //获得读取结果
                mysql.Open();
                MySqlDataReader mysqldr = mySqlCommand.ExecuteReader();
                while (mysqldr.Read())//mysqldr.Read()返回的是bool值，意在判断是否有下一条数据
                {
                    H_Mac = mysqldr["mac"].ToString();
                    H_ChipId = mysqldr["chipid"].ToString();
                    H_Isn = mysqldr["sn"].ToString();
                }
            }
           catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                mysql.Close();
                return false;//恢复出厂数据库中没有恢复记录
            }
            mysql.Close();
            if (H_Mac == string.Empty || H_Isn == string.Empty || H_ChipId == string.Empty)
            {
                Result = "恢复出厂数据库中没有查找到完整的数据";
                return false;//恢复出厂数据库中没有查找到完整的数据
            }
            else
            {
                Parameters parameters = new Parameters();
                parameters.Add("LotSN", H_Isn);//mac nvarhar
                parameters.Add("GetCode", H_ChipId);//chipid nvarhar
                parameters.Add("MOId", MOId);//bigint
                parameters.Add("LineId", Framework.App.Resource.LineId);//bigint
                parameters.Add("ResId", Framework.App.Resource.ResourceId);//bigint
                parameters.Add("ShiftTypeId", Framework.App.Resource.ShiftTypeId);//bigint
                parameters.Add("UserId", Framework.App.User.UserId);//bigint
                parameters.Add("OPId", Framework.App.Resource.StationId);//bigint
                parameters.Add("PluginId", PluginId);//bigint
                parameters.Add("Comment", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                parameters.Add("FocusedParam", null, SqlDbType.NVarChar, 50, ParameterDirection.Output);
                parameters.Add("Return_Message", null, SqlDbType.NVarChar, int.MaxValue, ParameterDirection.Output);
                parameters.Add("ReturnValue", null, SqlDbType.Int, ParameterDirection.Output);

                Result<Parameters, DataSet> result = null;
                try
                {
                    result = DB.DBHelper.ExecuteParametersSource("Prd_Inp_BindingProductSN_NoMove", parameters, ExecuteType.StoredProcedure);
                }
                catch (Exception ex)
                {
                    Result = ex.ToString();
                    return false;
                }
                if ((int)result.Value1["ReturnValue"] != 1)
                {
                    Result =result.Value1["Return_Message"].ToString();
                    return false;
                }
                else 
                {
                    Result = result.Value1["Return_Message"].ToString();
                    return true;
                }
            }
        }
        private void ClearScan()
        {
            txtScan1.Text = string.Empty;
            txtScan2.Text = string.Empty;
            txtScan3.Text = string.Empty;
            attachCode.Text = string.Empty;
            txtEN.Text = string.Empty;
        }
        public void Print(DataSet set)
        {
            if (set == null || set.Tables.Count < 1)
                return;
            int tableIndex = 0;
            foreach (ComboBox cmb in PrinterComboBoxs)
            {
                long tId = (long)cmb.Tag;
                DataRow row = cmb.DataContext as DataRow;
                int PrintNum = 0;
                if (row["Copies"].ToString().Trim() != null)
                {
                    int.TryParse(row["Copies"].ToString().Trim(), out PrintNum);
                }
                if (PrintNum < 1)
                    PrintNum = 1;
                if (TemplatePath.ContainsKey(tId))
                {
                    TemplatePath[tId].ClearParamsValue();
                    foreach (string pName in TemplatePath[tId].Params)
                    {
                        if (set.Tables[tableIndex].Rows.Count > 0)
                        {
                            if (set.Tables[tableIndex].Columns.Contains(pName))
                            {
                                TemplatePath[tId].SetParamValue(pName, set.Tables[tableIndex].Rows[0][pName].ToString());
                            }
                            else
                            {
                                TemplatePath[tId].SetParamValue(pName, "");
                            }
                        }
                        else
                        {
                            TemplatePath[tId].SetParamValue(pName, "");
                        }
                    }
                    if (cmb.SelectedValue != null)
                        TemplatePath[tId].SetPrinter(cmb.SelectedValue.ToString());
                    TemplatePath[tId].Print(PrintNum);
                }
                tableIndex++;
                if (tableIndex > set.Tables.Count - 1)
                {
                    break;
                }
            }

        }

        private void btnAddPrint_Click(object sender, RoutedEventArgs e)
        {
            if (SelectRow == null)
            {
                MessageBox.Show("choose order first ！");
                return;
            }
            Component.Windows.AuthorityLogin login = new Component.Windows.AuthorityLogin(MenuId, '4');
            login.Owner = Component.App.Portal;
            if (login.ShowDialog().Value)
            {
                AddPrint p = new AddPrint(cmbType1.SelectedValue == null ? null : cmbType1.SelectedValue.ToString(), this, (long)SelectRow["MOId"], login.UserId);
                p.Owner = Component.App.Portal;
                p.ShowDialog();
            }
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
                if(txtScan1.Text.IndexOf("_")>-1)
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
                else if (ckbIsBindEN.IsChecked.Value && !IsENOnlineAssembly)
                {
                    txtEN.Text = string.Empty;
                    txtEN.SetFoucs();
                }
                else if(IsPrintAttch)
                {
                    attachCode.Text = string.Empty;
                    attachCode.SetFoucs();
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
                else if (ckbIsBindEN.IsChecked.Value && !IsENOnlineAssembly)
                {
                    txtEN.Text = string.Empty;
                    txtEN.SetFoucs();
                }
                else if (IsPrintAttch)
                {
                    attachCode.Text = string.Empty;
                    attachCode.SetFoucs();
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
                if (ckbIsBindEN.IsChecked.Value && !IsENOnlineAssembly)
                {
                    txtEN.Text = string.Empty;
                    txtEN.SetFoucs();
                }
                else if (IsPrintAttch)
                {
                    attachCode.Text = string.Empty;
                    attachCode.SetFoucs();
                }
                else
                {
                    Submit();
                }
            }
        }
        private void txtEN_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (IsPrintAttch)
                {
                    attachCode.Text = string.Empty;
                    attachCode.SetFoucs();
                }
                else
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

        private void ckbIsBindEN_Checked(object sender, RoutedEventArgs e)
        {
            if (txtEN != null)
            {
                if (ckbIsBindEN.IsChecked.Value && !IsENOnlineAssembly)
                {
                    txtEN.Visibility = Visibility.Visible;
                    lblEN.Visibility = Visibility.Visible;
                }
                else
                {
                    txtEN.Visibility = Visibility.Collapsed;
                    lblEN.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            
            
        }

        private void CkbIsBindEN_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("此订单为排序包装，不允许去掉EN绑定");
            ckbIsBindEN.IsChecked = true;
            //bool Status;
            //Component.Windows.AuthorityLogin login = new Component.Windows.AuthorityLogin(MenuId, '4');
            //login.Owner = Component.App.Portal;
            //Status = (bool)ckbIsBindEN.IsChecked;
            //if (!login.ShowDialog().Value)
            //{
            //    ckbIsBindEN.IsChecked = !Status;
            //}
            //else
            //{
            //    ckbIsBindEN.IsChecked = Status;
            //}
        }

        private void AttachCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (attachCmb.SelectedValue != null && attachCmb.SelectedItem != null)
            {
                WinAPI.File.INIFileHelper.Write("attachCmb", string.Format("P{0}-2", MenuId), attachCmb.SelectedValue.ToString(), setting_path);
                attach.Text = ((KeyValuePair<string, string>)attachCmb.SelectedItem).Value + "：";
            }
        }

        private void AttachCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (AttachSubmit())
                {
                    Submit();
                }
            }
        }
        private bool AttachSubmit()
        {
            Parameters parameters = new Parameters();
            parameters.Add("LotSN", txtScan1.Text.Trim());// cmbType1.SelectedValue.ToString());
            parameters.Add("AttachType1", ((KeyValuePair<string, string>)attachCmb.SelectedItem).Key);
            parameters.Add("AttachCode1", attachCode.Text.Trim());
            parameters.Add("IsCheckAttach2",false);
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
                result = DB.DBHelper.ExecuteParametersSource("Prd_Inp_AttachAndPower_Binding_NoMove", parameters, ExecuteType.StoredProcedure);
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
                return true;
            }
            
        }
        private void IsConnect_Click(object sender, RoutedEventArgs e)
        {
            bool Status;
            Component.Windows.AuthorityLogin login = new Component.Windows.AuthorityLogin(MenuId, '4');
            login.Owner = Component.App.Portal;
            Status =(bool)IsConnect.IsChecked;
            if ((!login.ShowDialog().Value) || (login.UserId != 1016))
            {
                IsConnect.IsChecked = !Status;
            }
            else
            {
                IsConnect.IsChecked = Status;
            }
            if (IsConnect.IsChecked == true)
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
