using Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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
using System.Diagnostics;

namespace BoxPrint_M
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

        private int BoxQty = 0;

        private long ZhlTemplateID = -1;
        private string ZhlTemplateFile = "";
        private string labelFile = "labelFile.zhl"; //打印机模板文件名
        private string dataPath = "labeData.txt"; //数据文件
        private string batPath = "print.bat"; //bat运行程序名


        private DataTable ScanSource = new DataTable();

        public UserControl1(Framework.SystemAuthority authority) :
            base(authority)
        {
            InitializeComponent();

            grid.LoadingRow += new EventHandler<DataGridRowEventArgs>((sender, e) =>
            {
                e.Row.Header = e.Row.GetIndex() + 1;
            });


            ScanSource.Columns.Add("LotId", typeof(System.Int64));
            ScanSource.Columns.Add("ScanCode", typeof(System.String));
            ScanSource.Columns.Add("LotSN", typeof(System.String));
            ScanSource.Columns.Add("Mac", typeof(System.String));
            ScanSource.Columns.Add("EN", typeof(System.String));
            ScanSource.Columns.Add("IsDo", typeof(System.Boolean));
            grid.ItemsSource = ScanSource.DefaultView;

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

            btnSelectorMO_Click(btnSelectorMO, null);

            List<KeyValuePair<string, string>> source = new List<KeyValuePair<string, string>>();
            source.Add(new KeyValuePair<string, string>("LotSN", "批次条码"));
            source.Add(new KeyValuePair<string, string>("Mac", "MAC"));
            source.Add(new KeyValuePair<string, string>("DeviceSerialNumber", "设备标识"));
            source.Add(new KeyValuePair<string, string>("GponSN", "GponSN"));
            source.Add(new KeyValuePair<string, string>("CISN", "CISN"));
            source.Add(new KeyValuePair<string, string>("DSN", "DSN"));
            source.Add(new KeyValuePair<string, string>("EN", "EN"));

            cmbType.ItemsSource = source;

            try
            {
                cmbType.SelectedValue = WinAPI.File.INIFileHelper.Read("ScanType", string.Format("P{0}", MenuId), setting_path, string.Empty);
                cmbType_SelectionChanged(cmbType, null);
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

            BoxQty = 0;

            if (SelectRow == null)
            {
                tbMOCode.Text = string.Empty;
                tbQty.Text = string.Empty;
                tbItemCode.Text = string.Empty;
                tbItemSpec.Text = string.Empty;
                tbWorkflow.Text = string.Empty;
                tbCustomer.Text = string.Empty;
                tbBoxQty.Text = string.Empty;
                tbENFlowHex.Text = string.Empty;
                tbENFlowLength.Text = string.Empty;
                btnAddPrint.Visibility = Visibility.Collapsed;
            }
            else
            {
                int.TryParse(SelectRow["BoxQty"].ToString(), out BoxQty);
                tbMOCode.Text = SelectRow["MOCode"].ToString();
                tbQty.Text = SelectRow["Qty"].ToString();
                tbItemCode.Text = SelectRow["ItemCode"].ToString();
                tbItemSpec.Text = SelectRow["ItemSpecification"].ToString();
                tbBoxQty.Text = BoxQty.ToString();
                tbENFlowLength.Text = SelectRow["ENFlowLength"].ToString();
                tbENFlowHex.Text = SelectRow["ENFlowHex"].ToString();
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
                if (BoxQty < 1)
                {
                    txtMessage.AddMessage(string.Format("料号[{0}]未设置每箱数量！", SelectRow["ItemCode"]), true);
                }

                if (SelectRow["ENType"].ToString().ToUpper() != "No".ToUpper())
                {
                    if (SelectRow["ENFlowLength"] == null || SelectRow["ENFlowLength"].ToString().Trim() == string.Empty)
                    {
                        txtMessage.AddMessage(string.Format("料号[{0}]未设置EN流水号位数！", SelectRow["ItemCode"]), true);
                    }

                    if (SelectRow["ENFlowHex"] == null || SelectRow["ENFlowHex"].ToString().Trim() == string.Empty)
                    {
                        txtMessage.AddMessage(string.Format("料号[{0}]未设置EN流水号进制！", SelectRow["ItemCode"]), true);
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
                        //MessageBox.Show(" helper.FileName:" + path); //模板文件名

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
                        //MessageBox.Show(" headerTexte:" + headerText); //模板标题名

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
                        //MessageBox.Show(" TemplateId:" + cmb.Tag);

                        if (path.Length > 0 &&
                          (".zhl".IndexOf(path.Substring(path.LastIndexOf(".") + 1)) > -1))
                        {
                            this.ZhlTemplateFile = path;
                            this.ZhlTemplateID = Convert.ToInt64(cmb.Tag);
                        }

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
	   Bas_Item.BoxQty,
	   Bas_Item.ENFlowLength,
	   dbo.Bas_Item.ENFlowHex,
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
                    LoadMO();


                    ScanSource.Rows.Clear();
                    txtBoxSN.Text = string.Empty;
                    txtScan.Text = string.Empty;
                    txtScan.SetFoucs();
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

                            System.IO.File.Delete(this.dataPath);
                            System.IO.File.Delete(this.batPath);
                            System.IO.File.Delete(this.labelFile);

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

        private void cmbType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbType.SelectedValue != null && cmbType.SelectedItem != null)
            {
                WinAPI.File.INIFileHelper.Write("ScanType", string.Format("P{0}", MenuId), cmbType.SelectedValue.ToString(), setting_path);
                lblScan.Text = ((KeyValuePair<string, string>)cmbType.SelectedItem).Value + "：";
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
            parameters.Add("ScanedCodes", WinAPI.File.XMLHelper.Convert(ScanSource), SqlDbType.Xml, int.MaxValue);
            parameters.Add("MOId", SelectRow["MOId"]);
            parameters.Add("ItemId", SelectRow["ItemId"]);
            parameters.Add("LineId", Framework.App.Resource.LineId);
            parameters.Add("ResId", Framework.App.Resource.ResourceId);
            parameters.Add("ShiftTypeId", Framework.App.Resource.ShiftTypeId);
            parameters.Add("UserId", Framework.App.User.UserId);
            parameters.Add("OPId", Framework.App.Resource.StationId);
            parameters.Add("PluginId", PluginId);
            parameters.Add("OutLotId", DBNull.Value, SqlDbType.BigInt, ParameterDirection.Output);
            parameters.Add("OutLotSN", DBNull.Value, SqlDbType.NVarChar, 50, ParameterDirection.Output);
            parameters.Add("OutMac", DBNull.Value, SqlDbType.NVarChar, 50, ParameterDirection.Output);
            parameters.Add("OutEN", DBNull.Value, SqlDbType.NVarChar, 50, ParameterDirection.Output);
            parameters.Add("OutBoxSN", txtBoxSN.Text.Trim(), SqlDbType.NVarChar, 50, ParameterDirection.InputOutput);
            parameters.Add("OutIsCompleteBox", DBNull.Value, SqlDbType.Bit, ParameterDirection.Output);
            parameters.Add("Return_Message", null, SqlDbType.NVarChar, int.MaxValue, ParameterDirection.Output);
            parameters.Add("Return_Value", null, SqlDbType.Int, ParameterDirection.ReturnValue);


            Result<Parameters, DataSet> result = null;
            try
            {
                result = DB.DBHelper.ExecuteParametersSource("Prd_Inp_Pack_Box", parameters, ExecuteType.StoredProcedure);
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
                txtMessage.AddMessage(result.Value1["Return_Message"].ToString(), false);
                SetErrorOrSuccImage(false, false);


                txtBoxSN.Text = result.Value1["OutBoxSN"] == null ? string.Empty : result.Value1["OutBoxSN"].ToString(); ;

                DataRow row = ScanSource.NewRow();
                row["LotId"] = result.Value1["OutLotId"];
                row["ScanCode"] = txtScan.Text.Trim();
                row["LotSN"] = result.Value1["OutLotSN"];
                row["Mac"] = result.Value1["OutMac"];
                row["EN"] = result.Value1["OutEN"];
                row["IsDo"] = false;
                ScanSource.Rows.Add(row);
                grid.ScrollIntoView(grid.Items[grid.Items.Count - 1]);

                bool isCompleteBox = false;
                if (result.Value1["OutIsCompleteBox"] != null)
                {
                    bool.TryParse(result.Value1["OutIsCompleteBox"].ToString(), out isCompleteBox);
                }

                if (isCompleteBox)
                {
                    Print(result.Value2);
                    txtBoxSN.Text = string.Empty;
                    ScanSource.Rows.Clear();
                }

                txtScan.Text = string.Empty;
                txtScan.SetFoucs();
            }
        }

        private void RunBat(string batPath)
        {
            Process pro = new Process();
            FileInfo file = new FileInfo(batPath);
            pro.StartInfo.WorkingDirectory = file.Directory.FullName;
            pro.StartInfo.FileName = batPath;
            pro.StartInfo.CreateNoWindow = true;
            pro.Start();
            pro.WaitForExit();

        }


        void BackRunBat(string batPath)
        {
        ProcessStartInfo p = new ProcessStartInfo();
        p.FileName = batPath;
        p.WindowStyle = ProcessWindowStyle.Hidden;
        p.ErrorDialog = false;
        p.CreateNoWindow = true;
        Process.Start(p);
        }

        private void PrintZhl(DataSet set)
        {
            int tableIndex = 0;
            if (ScanSource != null)
            {
                uph.Quantity = uph.Quantity + ScanSource.Rows.Count;
            }
            if (set == null || set.Tables.Count < 1)
                return;

            //Output bat
            #region
            string path = System.AppDomain.CurrentDomain.BaseDirectory + @"Template\";
            this.labelFile = path + "labelFile.zhl"; //打印机模板文件名
            this.dataPath = path + "labeData.txt"; //数据文件
            this.batPath = path + "print.bat"; //bat运行程序名
            string devPrinter = "";  //默认选择的打印机设置名
            if (System.IO.File.Exists(batPath))
            {
                System.IO.File.Delete(batPath);
            }

            System.IO.StreamWriter file = new System.IO.StreamWriter(dataPath, false);
            //MessageBox.Show(string.Format("path:{0},{1},{2}", path, labelFile, batPath));

            //查找打印机
            foreach (ComboBox cmb in PrinterComboBoxs)
            {
                long tId = (long)cmb.Tag;
                if (tId == this.ZhlTemplateID)
                {
                    if (cmb.SelectedValue != null)
                        devPrinter = cmb.SelectedValue.ToString();
                }
            }

            //查找.zhl打印模板文件
            //string[] files = Directory.GetFiles(path);
            //foreach (string fileName in files)
            //{
            //    string exname = fileName.Substring(fileName.LastIndexOf(".") + 1);//得到后缀名
            //    if (".zhl".IndexOf(fileName.Substring(fileName.LastIndexOf(".") + 1)) > -1)//如果后缀名为.txt文件
            //    {
            //        FileInfo fi = new FileInfo(fileName);//建立FileInfo对象
            //        labelFile = fi.FullName;
            //        break;
            //    }
            //}
            this.labelFile = this.ZhlTemplateFile;


            // 生成Bat文件
            //if (!System.IO.File.Exists(batPath))
            {
                try
                {
                    //System.IO.File.Delete(batPath);
                    System.IO.StreamWriter filebat = new System.IO.StreamWriter(batPath, false);
                    string s = "";
                    if (string.IsNullOrEmpty(devPrinter))
                    {
                        s = string.Format("PrintAPI.exe -t \"{0}\" -d \"{1}\"", this.labelFile, this.dataPath);
                    }
                    else
                    {
                        s = string.Format("PrintAPI.exe -t \"{0}\" -d \"{1}\" -p \"{2}\"", this.labelFile, this.dataPath, devPrinter);
                    }
                    
                    filebat.Write(s);
                    filebat.Close();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }
            }

            DataTable dt = set.Tables[tableIndex];
            {
                bool bTitle = true;
                bool bNewBox = true;
                bool[] titleValue = { true, true, true, true, true, true, true, true, true };
                string[] title = { "BoxSN", "BoxNum", "Mac", "CSN", "DeviceSerialNumber", "CISN", "GponSN", "STBNO", "DSN" };
                foreach (DataRow dr in dt.Rows)
                {
                    if (bTitle)
                    {
                        for (int i = 0; i < title.Length; i++)  //通过第一行数据是否为空，确定后期打印
                        {
                            string dcItem = dr[title[i]].ToString();
                            titleValue[i] = dcItem.Length > 0 ? true : false;
                            //file.WriteLine("dcItem:{0},{1}",dcItem, titleValue[i]);
                        }
                        bTitle = false;
                    }

                    if (bNewBox)
                    {
                        for (int i = 0; i < title.Length; i++)     //第一条才打 “BoxSN", "BoxNum”
                        {
                            if (titleValue[i])
                            {
                                file.Write("{0},", dr[title[i]]); //列名,单元格数据
                            }
                        }
                        bNewBox = false;
                    }
                    else
                    {
                        for (int i = 2; i < title.Length; i++)
                        {
                            if (titleValue[i])
                            {
                                file.Write("{0},", dr[title[i]]); //列名,单元格数据
                            }
                        }

                    }

                }
            }
            #endregion
            file.Close();
            BackRunBat(batPath);

        }



        public void Print(DataSet set)
        {
            string pValue = "";
            string pTitle = "";
            int tableIndex = 0;

            if (ScanSource != null)
            {
                uph.Quantity = uph.Quantity + ScanSource.Rows.Count;
            }
            if (set == null || set.Tables.Count < 1)
                return;

            //file.WriteLine("=========================================================");
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

                //MessageBox.Show("btnSubmit_Click !!");

                if(tId == this.ZhlTemplateID)
                {
                    PrintZhl(set);
                }
                else 
                {
                   // file.WriteLine("tId:" + tId.ToString());
                    if (TemplatePath.ContainsKey(tId))
                    {
                        //file.WriteLine("tId2:" + tId.ToString());

                        foreach (string pName in TemplatePath[tId].Params)
                        {
                            int index = pName.IndexOf('$');
                            string parameterName = pName;
                            int rowIndex = 1;

                          //  file.WriteLine("pName:" + pName);
                            if (index > 0 && index < pName.Length - 1)
                            {
                                if (int.TryParse(pName.Substring(index + 1), out rowIndex))
                                {
                                    parameterName = pName.Substring(0, index);
                                }
                            }

                           // file.WriteLine("rowIndex:" + rowIndex.ToString());
                            if (rowIndex > 0 && rowIndex <= set.Tables[tableIndex].Rows.Count)
                            {
                                if (set.Tables[tableIndex].Columns.Contains(parameterName))
                                {
                                    pValue = set.Tables[tableIndex].Rows[rowIndex - 1][parameterName].ToString();
                                    TemplatePath[tId].SetParamValue(pName, set.Tables[tableIndex].Rows[rowIndex - 1][parameterName].ToString());


                                }
                                else
                                {
                                    pValue = "";
                                    TemplatePath[tId].SetParamValue(pName, "");
                                }
                             //   file.WriteLine("pValue:" + pValue);
                            }
                            else
                            {
                                TemplatePath[tId].SetParamValue(pName, "");
                            }
                           // file.WriteLine(pName + "\t" + pValue);
                        }


                        if (cmb.SelectedValue != null)
                            TemplatePath[tId].SetPrinter(cmb.SelectedValue.ToString());
                        TemplatePath[tId].Print(PrintNum);
                       // file.WriteLine("PrintTemplate:" + TemplatePath[tId].PrintTemplate);
                    }

                    tableIndex++;
                    if (tableIndex > set.Tables.Count - 1)
                    {
                       // file.WriteLine("break: tableIndex{0},Count{1}", tableIndex, set.Tables.Count);
                        break;
                    }
                }
            }
            //file.Close();

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

        private void txtScan_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Submit();
            }
        }

        public bool IsCanCloseControl(string header, ref string tipMessage)
        {
            return true;
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(string.Format("确定清空扫描吗？"), "提示", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.Cancel)
                return;

            ScanSource.Rows.Clear();
            txtBoxSN.Text = string.Empty;
            txtScan.Text = string.Empty;
            AddMessage("清空扫描成功！", false);
            SetErrorOrSuccImage(false, true);
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (ScanSource.Rows.Count < 1)
            {
                AddMessage("没有要提交的数据！", true);
                txtScan.Focus();
                return;
            }


            if (MessageBox.Show(string.Format("确定尾数箱提交吗？"), "提示", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.Cancel)
                return;


            Parameters parameters = new Parameters();
            parameters.Add("ScanedCodes", WinAPI.File.XMLHelper.Convert(ScanSource), SqlDbType.Xml, int.MaxValue);
            parameters.Add("IsAddRow", false);
            parameters.Add("LotId", DBNull.Value);
            parameters.Add("BoxSN", txtBoxSN.Text.Trim(), SqlDbType.NVarChar, 50);
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
                result = DB.DBHelper.ExecuteParametersSource("Prd_Inp_Pack_Box_Submit", parameters, ExecuteType.StoredProcedure);
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
                SetErrorOrSuccImage(true, false);
                txtScan.Text = string.Empty;
                txtScan.SetFoucs();
                return;
            }
            else
            {
                txtMessage.AddMessage(result.Value1["Return_Message"].ToString(), false);
                SetErrorOrSuccImage(false, false);


                Print(result.Value2);


                txtBoxSN.Text = string.Empty;
                ScanSource.Rows.Clear();
                txtScan.Text = string.Empty;
                txtScan.SetFoucs();
            }
        }


        private void btnFillBoxSN_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(tbMOCode.Text.Trim()))
            {
                txtMessage.AddMessage("请先选择工单！", true);
                btnSelectorMO.Focus();
                SetErrorOrSuccImage(true, false);
                return;
            }

            if (BoxQty<1)
            {
                txtMessage.AddMessage(string.Format("料号[{0}]未设置每箱数量！", SelectRow["ItemCode"]), true);
            }

            FillBox box = new FillBox(this,(long)SelectRow["MOId"], BoxQty);
            box.Owner = Component.App.Portal;
            if (box.ShowDialog().Value)
            {
                ScanSource.Rows.Clear();
                txtBoxSN.Text = box.BoxSN;
                txtScan.Text = string.Empty;
                foreach (DataRow row in box.Source.Rows)
                {
                    DataRow newRow = ScanSource.NewRow();
                    newRow["LotId"] = row["LotId"];
                    newRow["ScanCode"] = string.Empty;
                    newRow["LotSN"] = row["LotSN"];
                    newRow["Mac"] = row["Mac"];
                    newRow["EN"] = row["EN"];
                    newRow["IsDo"] = true;
                    ScanSource.Rows.Add(newRow);
                }
                txtScan.SetFoucs();
            }
        }
    }
}
