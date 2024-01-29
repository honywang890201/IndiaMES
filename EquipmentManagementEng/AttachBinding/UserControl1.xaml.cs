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

namespace AttachBinding
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class UserControl1 : Component.Controls.User.UserVendor
    {
        private string setting_path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Framework.SysVar.SETTING_FILE_NAME);
        private DataRowView SelectRow = null;
        private List<ComboBox> PrinterComboBoxs = new List<ComboBox>();
        private Dictionary<long, Printer.Instance> TemplatePath = new Dictionary<long, Printer.Instance>();
        private bool _IsLoad = false;

        private bool IsCheckTwoDimensionalCode = false;
        private int TwoDimensionalCodeLength = 0;
        private bool IsZN = false;//兆能项目判断标志，用于是否强制打开连接ovt恢复出厂数据库
        private bool DataBaseChange = false;//判断是否可以切换数据库连接状态
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
            source2.Add(new KeyValuePair<string, string>("PowerSN", "电源条码"));
            source2.Add(new KeyValuePair<string, string>("RemoteControl", "遥控器条码"));
            cmbType2.ItemsSource = source2;
            try
            {
                string v = WinAPI.File.INIFileHelper.Read("ScanType", string.Format("P{0}-IsScan2", MenuId), setting_path, string.Empty);
                bool b = false;
                bool.TryParse(v, out b);

                cmbType2.SelectedValue = WinAPI.File.INIFileHelper.Read("ScanType", string.Format("P{0}-2", MenuId), setting_path, string.Empty);
                cmbType2_SelectionChanged(cmbType2, null);
            }
            catch
            {

            }
            List<KeyValuePair<string, string>> source3 = new List<KeyValuePair<string, string>>();
            source3.Add(new KeyValuePair<string, string>("PowerSN", "电源条码"));
            source3.Add(new KeyValuePair<string, string>("RemoteControl", "遥控器条码"));
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
        private void LoadMO()
        {
            gridTemplate.Children.Clear();
            gridTemplate.RowDefinitions.Clear();
            CloseTemplate();
            TemplatePath.Clear();
            PrinterComboBoxs.Clear();
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
                if(IsCheckTwoDimensionalCode)
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
                        block.Text = "选择打印机";
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
                //WinAPI.File.INIFileHelper.Write("ScanType", string.Format("P{0}-IsScan2", MenuId), ckbIsScan2.IsChecked.HasValue ? ckbIsScan2.IsChecked.Value.ToString() : false.ToString(), setting_path);
                WinAPI.File.INIFileHelper.Write("ScanType", string.Format("P{0}-IsScan3", MenuId), ckbIsScan3.IsChecked.HasValue ? ckbIsScan3.IsChecked.Value.ToString() : false.ToString(), setting_path);   
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

            //if (ckbIsScan2.IsChecked.Value)
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
            Parameters parameters = new Parameters();
            parameters.Add("LotSN", txtScan1.Text.Trim());// cmbType1.SelectedValue.ToString());
            parameters.Add("AttachType1", ((KeyValuePair<string, string>)cmbType2.SelectedItem).Key);
            parameters.Add("AttachCode1", txtScan2.Text.Trim());
            parameters.Add("IsCheckAttach2", ckbIsScan3.IsChecked.Value);
            if (ckbIsScan3.IsChecked.Value)
            {
                parameters.Add("AttachType2", cmbType3.SelectedValue.ToString());
                parameters.Add("AttachCode2", txtScan3.Text.Trim());
            }
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
                result = DB.DBHelper.ExecuteParametersSource("Prd_Inp_AttachAndPower_Binding", parameters, ExecuteType.StoredProcedure);
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
                Print(result.Value2);
            }
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
        private void ClearScan()
        {
            txtScan1.Text = string.Empty;
            txtScan2.Text = string.Empty;
            txtScan3.Text = string.Empty;
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
                txtScan2.Text = string.Empty;
                txtScan2.SetFoucs();
                //if (ckbIsScan3.IsChecked.Value)
                //{
                //    txtScan3.Text = string.Empty;
                //    txtScan3.SetFoucs();
                //}
                //else
                //{
                //    Submit();
                //}
            }
        }

        private void txtScan2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (ckbIsScan3.IsChecked.Value)
                {
                    txtScan3.Text = string.Empty;
                    txtScan3.SetFoucs();
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
                Submit();
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
            //if (lblScanType2 != null && cmbType2 != null && lblScan2 != null && txtScan2 != null)
            //{
            //    if (ckbIsScan2.IsChecked.Value)
            //    {
            //        lblScanType2.Visibility = Visibility.Visible;
            //        cmbType2.Visibility = Visibility.Visible;
            //        lblScan2.Visibility = Visibility.Visible;
            //        txtScan2.Visibility = Visibility.Visible;
            //    }
            //    else
            //    {
            //        lblScanType2.Visibility = Visibility.Collapsed;
            //        cmbType2.Visibility = Visibility.Collapsed;
            //        lblScan2.Visibility = Visibility.Collapsed;
            //        txtScan2.Visibility = Visibility.Collapsed;
            //    }
            //}
        }

        private void BtnAddPrint_Click(object sender, RoutedEventArgs e)
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
                AddPrint p = new AddPrint(cmbType1.SelectedValue == null ? null : cmbType1.SelectedValue.ToString(), this, (long)SelectRow["MOId"], login.UserId);
                p.Owner = Component.App.Portal;
                p.ShowDialog();
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
    }
}
