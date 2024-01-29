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

namespace PalletByEN
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


        private int PalletQty = 0;
        private bool _IsLoad = false;
        //private bool IsENOnlineAssembly = false;
        private string PalletSN = string.Empty;
        //用于判断是否需要在装栈板位置打印当前栈板内的所有机顶盒的号码信息。true时要打印；false时只打印栈板号及所有箱号
        //由于此功能为兆能要求，所以在load时识别到是兆能客户时自动为true
        public bool IsPrintBoxInfo = false;
        private DataTable ScanSource = new DataTable();

        public UserControl1(Framework.SystemAuthority authority)
            : base(authority)
        {
            InitializeComponent();

            grid.LoadingRow += new EventHandler<DataGridRowEventArgs>((sender, e) =>
            {
                e.Row.Header = e.Row.GetIndex() + 1;
            });

            //定义扫描后显示相对关系
            ScanSource.Columns.Add("BoxId", typeof(System.Int64));
            ScanSource.Columns.Add("BoxSN", typeof(System.String));
            ScanSource.Columns.Add("ScanCode", typeof(System.String));
            grid.ItemsSource = ScanSource.DefaultView;

            root.Background = new ImageBrush(WinAPI.File.ImageHelper.ConvertToImageSource(Component.App.BackgroudImage));

            tbUser.Text = Framework.App.User.UserCode;
            if (!string.IsNullOrEmpty(Framework.App.User.UserDesc))
                tbUser.Text = tbUser.Text + "/" + Framework.App.User.UserDesc;
        }

        private void UserVendor_Loaded(object sender, RoutedEventArgs e)
        {
            if (_IsLoad)
                return;
            _IsLoad = true;

            //默认选择工单
            btnSelectorMO_Click(btnSelectorMO, null);

            List<KeyValuePair<string, string>> source = new List<KeyValuePair<string, string>>();
            source.Add(new KeyValuePair<string, string>("BoxSN", "外箱条码"));
            source.Add(new KeyValuePair<string, string>("LotSN", "批次条码"));
            source.Add(new KeyValuePair<string, string>("Mac", "MAC"));
            source.Add(new KeyValuePair<string, string>("DeviceSerialNumber", "设备标识"));
            source.Add(new KeyValuePair<string, string>("GponSN", "GponSN"));
            source.Add(new KeyValuePair<string, string>("CISN", "CISN"));
            source.Add(new KeyValuePair<string, string>("DSN", "DSN"));
            source.Add(new KeyValuePair<string, string>("EN", "EN"));

            cmbType.ItemsSource = source;
            txtScan.SetFoucs();
            try
            {
                cmbType.SelectedValue = WinAPI.File.INIFileHelper.Read("ScanType", string.Format("P{0}", MenuId), setting_path, string.Empty);
                cmbType_SelectionChanged(cmbType, null);
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
            //清空模板打印机信息
            gridTemplate.Children.Clear();
            gridTemplate.RowDefinitions.Clear();
            btnAddPrint.Visibility = Visibility.Collapsed;

            CloseTemplate();
            TemplatePath.Clear();
            PrinterComboBoxs.Clear();

            PalletQty = 0;

            if (SelectRow == null)
            {
                tbMOCode.Text = string.Empty;
                tbQty.Text = string.Empty;
                tbItemCode.Text = string.Empty;
                tbItemSpec.Text = string.Empty;
                tbCustomer.Text = string.Empty;
                tbPalletQty.Text = string.Empty;
                btnAddPrint.Visibility = Visibility.Collapsed;
            }
            else
            {
                int.TryParse(SelectRow["PalletQty"].ToString(), out PalletQty);
                tbMOCode.Text = SelectRow["MOCode"].ToString();
                tbQty.Text = SelectRow["Qty"].ToString();
                tbItemCode.Text = SelectRow["ItemCode"].ToString();
                tbItemSpec.Text = SelectRow["ItemSpecification"].ToString();
                tbPalletQty.Text = PalletQty.ToString();
                tbENType.Text = SelectRow["ENType"].ToString();
                tbCustomer.Text = SelectRow["CustomerCode"].ToString();
                if (!string.IsNullOrEmpty(SelectRow["CustomerDesc"].ToString().Trim()))
                    tbCustomer.Text = tbCustomer.Text + "/" + SelectRow["CustomerDesc"].ToString();
                if (PalletQty < 1)
                {
                    txtMessage.AddMessage(string.Format("料号[{0}]未设置每栈板箱数！", SelectRow["ItemCode"]), true);
                }
                //兆能客户代码
                if (SelectRow["CustomerCode"].ToString() == "0010")
                {
                    IsPrintBoxInfo= true;
                }
                else
                {
                    IsPrintBoxInfo = false;
                }
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
                //uph.Start();
        }


        private bool IsCompletePallet = false;

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
       Bas_Item.ENType,
	   Bas_Item.ItemSpecification,
	   Bas_Workflow.WorkflowCode,
	   Bas_Workflow.WorkflowDesc,
	   Bas_Customer.CustomerCode,
	   Bas_Customer.CustomerDesc,
	   Bas_Item.PalletQty
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


                    ScanSource.Rows.Clear();
                    IsCompletePallet = false;
                    txtPalletSN.Text = string.Empty;
                    txtScan.Text = string.Empty;
                    txtPalletSN.SetFoucs();
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

        private void btnAddPrint_Click(object sender, RoutedEventArgs e)
        {
            if (SelectRow == null)
            {
                MessageBox.Show("请先选择工单！");
                return;
            }
            //如果每次打印前要输入密码麻烦，就把密码验证注释掉，login.UserId=1即可
            Component.Windows.AuthorityLogin login = new Component.Windows.AuthorityLogin(MenuId, '4');
            login.Owner = Component.App.Portal;
            if (login.ShowDialog().Value)
            {
                AddPrint p = new AddPrint(cmbType.SelectedValue == null ? null : cmbType.SelectedValue.ToString(), this, (long)SelectRow["MOId"], login.UserId);
                p.Owner = Component.App.Portal;
                p.ShowDialog();
            }
        }


        public void AddMessage(string message, bool isError)
        {
            txtMessage.AddMessage(message, isError);

            SetErrorOrSuccImage(isError, false);
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
                    foreach (string pName in TemplatePath[tId].Params)
                    {
                        int index = pName.IndexOf('$');
                        string parameterName = pName;
                        int rowIndex = 1;
                        if (index > 0 && index < pName.Length - 1)
                        {
                            if (int.TryParse(pName.Substring(index + 1), out rowIndex))
                            {
                                parameterName = pName.Substring(0, index);
                            }
                        }

                        if (rowIndex > 0 && rowIndex <= set.Tables[tableIndex].Rows.Count)
                        {
                            if (set.Tables[tableIndex].Columns.Contains(parameterName))
                            {
                                TemplatePath[tId].SetParamValue(pName, set.Tables[tableIndex].Rows[rowIndex - 1][parameterName].ToString());
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

        private void txtPalletSN_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void cmbType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbType.SelectedValue != null && cmbType.SelectedItem != null)
            {
                WinAPI.File.INIFileHelper.Write("ScanType", string.Format("P{0}", MenuId), cmbType.SelectedValue.ToString(), setting_path);
                lblScan.Text = ((KeyValuePair<string, string>)cmbType.SelectedItem).Value + "：";
            }
        }

        private void txtScan_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Submit();
            }
        }


        /// <summary>
        /// 提交箱号
        /// </summary>
        public void Submit()
        {
            if (string.IsNullOrEmpty(tbMOCode.Text.Trim()))
            {
                txtMessage.AddMessage("请先选择工单！", true);
                btnSelectorMO.Focus();
                SetErrorOrSuccImage(true, false);
                return;
            }

            if (cmbType.SelectedValue == null || cmbType.SelectedItem == null)
            {
                txtMessage.AddMessage("请选择扫描类型！", true);
                cmbType.Focus();
                SetErrorOrSuccImage(true, false);
                return;
            }
            if (txtScan.Text.Trim() == string.Empty)
            {
                txtMessage.AddMessage(string.Format("请扫描{0}！", lblScan.Text), true);
                txtScan.Text = string.Empty;
                txtScan.SetFoucs();
                SetErrorOrSuccImage(true, false);
                return;
            }

            foreach (DataRowView row in grid.Items)
            {
                if (row["ScanCode"].ToString().Trim().ToUpper() == txtScan.Text.Trim().ToUpper())
                {
                    txtMessage.AddMessage(string.Format("{0}[{1}]已扫描过，请不要重复扫描！", ((KeyValuePair<string, string>)cmbType.SelectedItem).Value, txtScan.Text), true);
                    txtScan.Text = string.Empty;
                    txtScan.SetFoucs();
                    SetErrorOrSuccImage(true, false);
                    grid.ScrollIntoView(row);
                    return;
                }
            }

            Parameters parameters = new Parameters();
            parameters.Add("ScanType", cmbType.SelectedValue.ToString());
            parameters.Add("ScanTypeDesc", ((KeyValuePair<string, string>)cmbType.SelectedItem).Value);
            parameters.Add("ScanCode", txtScan.Text.Trim());
            parameters.Add("ScanedBoxs", WinAPI.File.XMLHelper.Convert(ScanSource), SqlDbType.Xml, int.MaxValue);
            parameters.Add("MOId", SelectRow["MOId"]);
            parameters.Add("ItemId", SelectRow["ItemId"]);
            parameters.Add("ENType", SelectRow["ENType"]);
            parameters.Add("UserId", Framework.App.User.UserId);
            parameters.Add("PluginId", PluginId);
            parameters.Add("OutBoxId", DBNull.Value, SqlDbType.BigInt, ParameterDirection.Output);
            parameters.Add("OutBoxSN", DBNull.Value, SqlDbType.NVarChar, 50, ParameterDirection.Output);
            parameters.Add("OutPalletSN", txtPalletSN.Text.Trim(), SqlDbType.NVarChar, 50, ParameterDirection.InputOutput);
            parameters.Add("OutIsCompletePallet", DBNull.Value, SqlDbType.Bit, ParameterDirection.Output);
            parameters.Add("Return_Message", null, SqlDbType.NVarChar, int.MaxValue, ParameterDirection.Output);
            parameters.Add("Return_Value", null, SqlDbType.Int, ParameterDirection.ReturnValue);
            Result<Parameters, DataSet> result = null;
            try
            {
                result = DB.DBHelper.ExecuteParametersSource("Prd_Inp_Pack_Pallet_Online", parameters, ExecuteType.StoredProcedure);
            }
            catch (Exception ex)
            {
                txtMessage.AddMessage(ex.Message, true);
                txtScan.Text = string.Empty;
                txtScan.SetFoucs();
                SetErrorOrSuccImage(true, false);
                return;
            }
            if (result.HasError)
            {
                txtMessage.AddMessage(result.Message, true);
                txtScan.Text = string.Empty;
                txtScan.SetFoucs();
                SetErrorOrSuccImage(true, false);
                return;
            }
            if ((int)result.Value1["Return_Value"] != 1)
            {
                txtMessage.AddMessage(result.Value1["Return_Message"].ToString(), true);
                txtScan.Text = string.Empty;
                txtScan.SetFoucs();
                SetErrorOrSuccImage(true, false);
                return;
            }
            else
            {
                AddMessage(result.Value1["Return_Message"].ToString(), false);

                DataRow row = ScanSource.NewRow();
                row["BoxId"] = result.Value1["OutBoxId"];
                row["BoxSN"] = result.Value1["OutBoxSN"];
                row["ScanCode"] = txtScan.Text.Trim();
                ScanSource.Rows.Add(row);
                grid.ScrollIntoView(grid.Items[grid.Items.Count - 1]);

                IsCompletePallet = false;
                if (result.Value1["OutIsCompletePallet"] != null)
                {
                    bool.TryParse(result.Value1["OutIsCompletePallet"].ToString(), out IsCompletePallet);
                }
                txtScan.Text = string.Empty;
                txtScan.SetFoucs();
                txtPalletSN.Text = result.Value1["OutPalletSN"].ToString();
                if (IsCompletePallet)
                {
                    //btnSubmit_Click(btnSubmit, null);
                    //MessageBox.Show(string.Format("栈板[{0}]已扫描完成，请点[提交]按钮提交", txtPalletSN.Text.Trim()), "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    // btnSubmit.Focus();
                    btnSubmit_Click(null, null);
                    txtScan.SetFoucs();
                    return;
                }
            }
        }


        void BSubmit(bool IsLast)
        {

            if (string.IsNullOrEmpty(tbMOCode.Text.Trim()))
            {
                AddMessage("请先选择工单！", true);
                btnSelectorMO.Focus();
                return;
            }
            if (string.IsNullOrEmpty(txtPalletSN.Text.Trim()))
            {
                AddMessage("请扫描栈板条码！", true);
                txtPalletSN.Text = string.Empty;
                txtPalletSN.SetFoucs();
                return;
            }

            if (ScanSource.Rows.Count < 1)
            {
                AddMessage("没有要提交的数据！", true);
                txtScan.Text = string.Empty;
                txtScan.SetFoucs();
                return;
            }

            if (MessageBox.Show(string.Format("确定提交栈板[{0}]吗？", txtPalletSN.Text), "提示", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.Cancel)
                return;


            Parameters parameters = new Parameters();
            parameters.Add("PalletSN", txtPalletSN.Text.Trim(), SqlDbType.NVarChar, 50);
            parameters.Add("IsLast", IsLast);
            parameters.Add("ScanedBoxs", WinAPI.File.XMLHelper.Convert(ScanSource), SqlDbType.Xml, int.MaxValue);
            parameters.Add("MOId", SelectRow["MOId"]);
            //IsPrintBoxInfo为true时则要打印栈板号+箱号+所有盒子号码信息
            if (IsPrintBoxInfo)
            {
                parameters.Add("IsPrintSN", 1);
            }
            //IsPrintBoxInfo为false时则为通用栈板打印只打印栈板号+箱号
            else
            {
                parameters.Add("IsPrintSN", 0);
            }
            parameters.Add("ItemId", SelectRow["ItemId"]);
            parameters.Add("UserId", Framework.App.User.UserId);
            parameters.Add("PluginId", PluginId);
            parameters.Add("Return_Message", null, SqlDbType.NVarChar, int.MaxValue, ParameterDirection.Output);
            parameters.Add("Return_Value", null, SqlDbType.Int, ParameterDirection.ReturnValue);
            Result<Parameters, DataSet> result = null;
            //Parameters result = null;
            try
            {
                result = DB.DBHelper.ExecuteParametersSource("Prd_Inp_Pack_Pallet_Submit_Online_R", parameters, ExecuteType.StoredProcedure);
            }
            catch (Exception ex)
            {
                AddMessage(ex.Message, true);
                txtScan.Text = string.Empty;
                txtScan.SetFoucs();
                return;
            }
            if ((int)result.Value1["Return_Value"] != 1)
            {
                AddMessage(result.Value1["Return_Message"].ToString(), true);
                txtScan.Text = string.Empty;
                txtScan.SetFoucs();
                return;
            }
            else
            {
                AddMessage(result.Value1["Return_Message"].ToString(), false);
                Print(result.Value2);
                ScanSource.Rows.Clear();
                IsCompletePallet = false;
                txtPalletSN.Text = string.Empty;
                txtScan.Text = string.Empty;
                txtScan.SetFoucs();
            }

        }


        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {

            Component.Windows.AuthorityLogin login = new Component.Windows.AuthorityLogin(MenuId, '4');
            login.Owner = Component.App.Portal;
            if (login.ShowDialog().Value)
            {
                BSubmit(false);
            }

            //if (ScanSource.Rows.Count < 1)
            //{
            //    AddMessage("没有要提交的数据！", true);
            //    txtScan.Focus();
            //    return;
            //}

            //if (MessageBox.Show(string.Format("确定尾数箱提交吗？"), "提示", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.Cancel)
            //    return;
            
            //Parameters parameters = new Parameters();
            //parameters.Add("ScanedCodes", WinAPI.File.XMLHelper.Convert(ScanSource), SqlDbType.Xml, int.MaxValue);
            //parameters.Add("IsAddRow", false);
            //parameters.Add("LotId", DBNull.Value);
            //parameters.Add("BoxSN", txtBoxSN.Text.Trim(), SqlDbType.NVarChar, 50);
            //parameters.Add("MOId", SelectRow["MOId"]);
            //parameters.Add("ItemId", SelectRow["ItemId"]);
            //parameters.Add("LineId", Framework.App.Resource.LineId);
            //parameters.Add("ResId", Framework.App.Resource.ResourceId);
            //parameters.Add("ShiftTypeId", Framework.App.Resource.ShiftTypeId);
            //parameters.Add("UserId", Framework.App.User.UserId);
            //parameters.Add("OPId", Framework.App.Resource.StationId);
            //parameters.Add("PluginId", PluginId);
            //parameters.Add("Return_Message", null, SqlDbType.NVarChar, int.MaxValue, ParameterDirection.Output);
            //parameters.Add("Return_Value", null, SqlDbType.Int, ParameterDirection.ReturnValue);

            //Result<Parameters, DataSet> result = null;
            //try
            //{
            //    result = DB.DBHelper.ExecuteParametersSource("Prd_Inp_Pack_Box_Submit", parameters, ExecuteType.StoredProcedure);
            //}
            //catch (Exception ex)
            //{
            //    txtMessage.AddMessage(ex.Message, true);
            //    txtScan.Text = string.Empty;
            //    txtScan.SetFoucs();
            //    SetErrorOrSuccImage(true, false);
            //    return;
            //}

            //if (result.HasError)
            //{
            //    txtMessage.AddMessage(result.Message, true);
            //    txtScan.Text = string.Empty;
            //    txtScan.SetFoucs();
            //    SetErrorOrSuccImage(true, false);
            //    return;
            //}

            //if ((int)result.Value1["Return_Value"] != 1)
            //{
            //    txtMessage.AddMessage(result.Value1["Return_Message"].ToString(), true);
            //    SetErrorOrSuccImage(true, false);
            //    txtScan.Text = string.Empty;
            //    txtScan.SetFoucs();
            //    return;
            //}
            //else
            //{
            //    txtMessage.AddMessage(result.Value1["Return_Message"].ToString(), false);
            //    SetErrorOrSuccImage(false, false);


            //    Print(result.Value2);
                
            //    txtPalletSN.Text = string.Empty;
            //    ScanSource.Rows.Clear();
            //    txtScan.Text = string.Empty;
            //    txtScan.SetFoucs();
            //}
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(string.Format("确定清空扫描吗？"), "提示", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.Cancel)
                return;

            ScanSource.Rows.Clear();
            txtPalletSN.Text = string.Empty;
            txtScan.Text = string.Empty;
            AddMessage("清空扫描成功！", false);
            SetErrorOrSuccImage(false, true);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Component.Windows.AuthorityLogin login = new Component.Windows.AuthorityLogin(MenuId, '4');
            login.Owner = Component.App.Portal;
            if (login.ShowDialog().Value)
            {
                BSubmit(true);
            }
        }
    }
    
}
