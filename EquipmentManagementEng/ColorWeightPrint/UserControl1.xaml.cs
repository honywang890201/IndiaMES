using Data;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO.Ports;
using System.Runtime.InteropServices;
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

namespace ColorWeightPrint
{


    
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class UserControl1 : Component.Controls.User.UserVendor
    {
       
        private bool _IsLoad = false;
        private Dictionary<long, Printer.Instance> TemplatePath = new Dictionary<long, Printer.Instance>();
        private string setting_path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Framework.SysVar.SETTING_FILE_NAME);

        private string R;

        public delegate void DeleFunc();
        private List<ComboBox> PrinterComboBoxs = new List<ComboBox>();
        private DataRowView SelectRow = null;
        private SerialClass Rs232Instance;

        public UserControl1(Framework.SystemAuthority authority) : base(authority)
        {
            InitializeComponent();

            root.Background = new ImageBrush(WinAPI.File.ImageHelper.ConvertToImageSource(Component.App.BackgroudImage));


            //LoadInf();

            if (!Framework.App.Resource.ResourceId.HasValue || !Framework.App.Resource.StationId.HasValue)
            {
                Component.MessageBox.MyMessageBox.ShowError("没有获取到工序，不能抽检作业，请尝试使用正确的资源重新登录程序。");
                this.IsEnabled = false;
                return;
            }
        }




         private void UserVendor_Loaded(object sender, RoutedEventArgs e)
        {
            if (_IsLoad)
            {
                return;
            }
            _IsLoad = true;


            btnSelectorMO_Click(btnSelectorMO, null);
            List<KeyValuePair<string, string>> source1 = new List<KeyValuePair<string, string>>();
            source1.Add(new KeyValuePair<string, string>("BoxSN", "箱号"));
            source1.Add(new KeyValuePair<string, string>("LotSN", "批次条码"));
            source1.Add(new KeyValuePair<string, string>("Mac", "MAC"));
            source1.Add(new KeyValuePair<string, string>("DeviceSerialNumber", "设备标识"));
            source1.Add(new KeyValuePair<string, string>("GponSN", "GponSN"));
            source1.Add(new KeyValuePair<string, string>("CISN", "CISN"));
            source1.Add(new KeyValuePair<string, string>("DSN", "DSN"));
            source1.Add(new KeyValuePair<string, string>("EN", "EN"));

            cmbType.ItemsSource = source1;

            
            try
            {
                cmbType.SelectedValue = WinAPI.File.INIFileHelper.Read("ScanType", string.Format("P{0}-1", MenuId), setting_path, string.Empty);
                cmbType_SelectionChanged(cmbType, null);
            }
            catch (Exception)
            {
                
                throw;
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
                 //ckbIsBindEN.Visibility = Visibility.Visible;
                 //IsENOnlineAssembly = false;
                 btnAddPrint.Visibility = Visibility.Collapsed;
             }
             else
             {
                 

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

        private void btnOpenSerialPort_Click(object sender, RoutedEventArgs e)
        {
            Rs232Instance = new SerialClass(); //建立实例
            if (Rs232Instance == null)
            {
                txtMessage.AddMessage("创建串口实例失败", true);
                SetErrorOrSuccImage(true, false);
            }
            try
            {
                string comPortName = tbxCOMPort.Text;
                int baudRate = Convert.ToInt32(tbxbaudRate.Text);
                int dataBits = 8;
                int stopBits = 1;
                Rs232Instance.setSerialPort(comPortName, baudRate, dataBits, stopBits);
                Rs232Instance.DataReceived += new SerialClass.SerialPortDataReceiveEventArgs(sc_DataReceived);
                Rs232Instance.openPort();
                txtMessage.AddMessage("打开串口成功", false);
                SetErrorOrSuccImage(false, false);
            }
            catch
            {
                txtMessage.AddMessage("打开串口实例失败", true);
                SetErrorOrSuccImage(true, false);
            }
        }


        /// <summary>
        /// 接受串口返回内容
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="bits"></param>
        private void sc_DataReceived(object sender, SerialDataReceivedEventArgs e, byte[] bits)
        {
            //R = string.Format(Encoding.Default.GetString(bits));
            string strRcv = null;
            for (int i = 0; i < bits.Length; i++) //窗体显示
            {

                //strRcv += bits[i].ToString("X2");  //16进制显示
                strRcv += Convert.ToChar(bits[i]).ToString();  //16进制显示

            }
            R = strRcv.Replace("\r", "").Replace("\n", "").Trim();

            System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
            new DeleFunc(SetValue));
          

           
        }


        private  void  SetValue()
        {
            if (string.IsNullOrEmpty(R))
            {
                txtMessage.AddMessage(string.Format("获取彩盒重量[{0}]失败！请重试！如果连续三次获取出现此错误。请联系管理员！", R), true);
                SetErrorOrSuccImage(true, false);

            }
            else
            {
                txtWeight.Text = R.ToString();
                //Component.MessageBox.MyMessageBox.Show(string.Format("获取彩盒重量[{0}]成功！", R));
                txtMessage.AddMessage(string.Format("获取彩盒重量[{0}]成功！", R), false);
                SetErrorOrSuccImage(false, false);
                txtWeight_KeyDown(this, null);
                txtSN.SetFoucs();
                
            }
        }


        private void btnCloseSerialPort_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Rs232Instance.closePort();
                txtMessage.AddMessage("关闭串口成功", false);
                SetErrorOrSuccImage(false, false);
            }
            catch
            {
                txtMessage.AddMessage("关闭串口实例失败", true);
                SetErrorOrSuccImage(true, false);
            }
        }

      
        private void cmbType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbType.SelectedValue != null && cmbType.SelectedItem != null)
            {
                WinAPI.File.INIFileHelper.Write("ScanType", string.Format("P{0}-1", MenuId), cmbType.SelectedValue.ToString(), setting_path);
                lblSN.Text = ((KeyValuePair<string, string>)cmbType.SelectedItem).Value + "：";
            }
        }

        private void txtSN_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key==Key.Enter)
            {
                if (string.IsNullOrEmpty(tbMOCode.Text.Trim()))
                {
                    txtMessage.AddMessage("select order first ！", true);
                    btnSelectorMO.Focus();
                    SetErrorOrSuccImage(true, false);
                    return;
                }
                if (cmbType.SelectedValue==null||cmbType.SelectedItem==null)
                {
                    txtMessage.AddMessage("select scan type1！", true);
                    cmbType.Focus();
                    SetErrorOrSuccImage(true, false);
                    return;
                }
                if (txtSN.Text.Trim()==string.Empty)
                {
                    txtMessage.AddMessage(string.Format("please scan  {0}！", lblSN.Text), true);
                    txtSN.Text = string.Empty;
                    txtSN.SetFoucs();
                    SetErrorOrSuccImage(true, false);
                    return;
                }
                txtWeight.SetFoucs();
            }
            
        }

        /// <summary>
        /// 清除SN数据
        /// </summary>
        private void ClearScan()
        {
            txtSN.Text = string.Empty;
            txtWeight.Text = string.Empty;
        }


        /// <summary>
        /// 打印工序标贴
        /// </summary>
        /// <param name="set"></param>
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


        private void txtWeight_KeyDown(object sender, KeyEventArgs e)
        {
            if (e ==null)
            {
                if (string.IsNullOrEmpty(tbMOCode.Text.Trim()))
                {
                    txtMessage.AddMessage("select order first ！", true);
                    btnSelectorMO.Focus();
                    SetErrorOrSuccImage(true, false);
                    return;
                }
                if (cmbType.SelectedValue == null || cmbType.SelectedItem == null)
                {
                    txtMessage.AddMessage("select scan type 1！", true);
                    cmbType.Focus();
                    SetErrorOrSuccImage(true, false);
                    return;
                }
                if (txtSN.Text.Trim() == string.Empty)
                {
                    txtMessage.AddMessage(string.Format("please scan {0}！", lblSN.Text), true);
                    txtSN.Text = string.Empty;
                    txtSN.SetFoucs();
                    SetErrorOrSuccImage(true, false);
                    return;
                }

                if (txtWeight.Text.Trim() == string.Empty)
                {
                    txtMessage.AddMessage("未获取到数量！", true);
                    txtWeight.Text = string.Empty;
                    txtWeight.SetFoucs();
                    SetErrorOrSuccImage(true, false);
                    return;
                }
               
                Parameters parameters = new Parameters();
                parameters.Add("ScanType", cmbType.SelectedValue.ToString());
                parameters.Add("ScanTypeDesc", ((KeyValuePair<string, string>)cmbType.SelectedItem).Value);
                parameters.Add("ScanCode", txtSN.Text.Trim());
                parameters.Add("Weight", txtWeight.Text.Trim());
                parameters.Add("MOId", SelectRow["MOId"]);
                parameters.Add("ItemId", SelectRow["ItemId"]);
                parameters.Add("LineId", Framework.App.Resource.LineId);
                parameters.Add("ResourceId", Framework.App.Resource.ResourceId);
                parameters.Add("ShiftTypeId", Framework.App.Resource.ShiftTypeId);
                parameters.Add("UserId", Framework.App.User.UserId);
                parameters.Add("StationId", Framework.App.Resource.StationId);
                parameters.Add("PluginId", PluginId);
                parameters.Add("Return_Message", null, SqlDbType.NVarChar, int.MaxValue, ParameterDirection.Output);
                parameters.Add("Return_Value", null, SqlDbType.Int, ParameterDirection.ReturnValue);

                Result<Parameters, DataSet> result = null;
                try
                {
                    result = DB.DBHelper.ExecuteParametersSource("Prd_Inp_Pack_ColorBoxWeight", parameters, ExecuteType.StoredProcedure);
                    //result = DB.DBHelper.ExecuteParametersSource("Test_Inp_Pack_ColorBoxWeight", parameters, ExecuteType.StoredProcedure);

                }
                catch (Exception ex)
                {
                    txtMessage.AddMessage(ex.Message, true);
                    ClearScan();
                    txtSN.SetFoucs();
                    SetErrorOrSuccImage(true, false);
                    return;
                }

                if (result.HasError)
                {
                    txtMessage.AddMessage(result.Message, true);
                    ClearScan();
                    txtSN.SetFoucs();
                    SetErrorOrSuccImage(true, false);
                    return;
                }

                if ((int)result.Value1["Return_Value"] != 1)
                {
                    txtMessage.AddMessage(result.Value1["Return_Message"].ToString(), true);
                    ClearScan();
                    txtSN.SetFoucs();
                    SetErrorOrSuccImage(true, false);
                    return;
                }
                else
                {
                    txtMessage.AddMessage(result.Value1["Return_Message"].ToString(), false);
                    ClearScan();
                    txtWeight.SetFoucs();
                    SetErrorOrSuccImage(false, false);
                }
                uph.Quantity++;
                Print(result.Value2);
                txtWeight.Text = string.Empty;
                txtSN.SetFoucs();
            }else if (e.Key == Key.Enter)
            {
                if (string.IsNullOrEmpty(tbMOCode.Text.Trim()))
                {
                    txtMessage.AddMessage("select order first ！", true);
                    btnSelectorMO.Focus();
                    SetErrorOrSuccImage(true, false);
                    return;
                }
                if (cmbType.SelectedValue == null || cmbType.SelectedItem == null)
                {
                    txtMessage.AddMessage("select scan type1！", true);
                    cmbType.Focus();
                    SetErrorOrSuccImage(true, false);
                    return;
                }
                if (txtSN.Text.Trim() == string.Empty)
                {
                    txtMessage.AddMessage(string.Format("please scan {0}！", lblSN.Text), true);
                    txtSN.Text = string.Empty;
                    txtSN.SetFoucs();
                    SetErrorOrSuccImage(true, false);
                    return;
                }

                if (txtWeight.Text.Trim() == string.Empty)
                {
                    txtMessage.AddMessage("未获取到数量！", true);
                    txtWeight.Text = string.Empty;
                    txtWeight.SetFoucs();
                    SetErrorOrSuccImage(true, false);
                    return;
                }
                Parameters parameters = new Parameters();
                Result<Parameters, DataSet> result = null;
                if (cmbType.SelectedValue.ToString() == "BoxSN")
                {
                    parameters.Add("ScanCode", txtSN.Text.Trim());
                    parameters.Add("Weight", txtWeight.Text.Trim());
                    parameters.Add("MOId", SelectRow["MOId"]);
                    parameters.Add("ItemId", SelectRow["ItemId"]);
                    parameters.Add("Return_Message", null, SqlDbType.NVarChar, int.MaxValue, ParameterDirection.Output);
                    parameters.Add("Return_Value", null, SqlDbType.Int, ParameterDirection.Output);
                    try
                    {
                        result = DB.DBHelper.ExecuteParametersSource("Prd_Inp_BoxWeight", parameters, ExecuteType.StoredProcedure);

                    }
                    catch (Exception ex)
                    {
                        txtMessage.AddMessage(ex.Message, true);
                        ClearScan();
                        txtSN.SetFoucs();
                        SetErrorOrSuccImage(true, false);
                        return;
                    }
                }
                else
                {
                    //Parameters parameters = new Parameters();
                    parameters.Add("ScanType", cmbType.SelectedValue.ToString());
                    parameters.Add("ScanTypeDesc", ((KeyValuePair<string, string>)cmbType.SelectedItem).Value);
                    parameters.Add("ScanCode", txtSN.Text.Trim());
                    parameters.Add("Weight", txtWeight.Text.Trim());
                    parameters.Add("MOId", SelectRow["MOId"]);
                    parameters.Add("ItemId", SelectRow["ItemId"]);
                    parameters.Add("LineId", Framework.App.Resource.LineId);
                    parameters.Add("ResourceId", Framework.App.Resource.ResourceId);
                    parameters.Add("ShiftTypeId", Framework.App.Resource.ShiftTypeId);
                    parameters.Add("UserId", Framework.App.User.UserId);
                    parameters.Add("StationId", Framework.App.Resource.StationId);
                    parameters.Add("PluginId", PluginId);
                    parameters.Add("Return_Message", null, SqlDbType.NVarChar, int.MaxValue, ParameterDirection.Output);
                    parameters.Add("Return_Value", null, SqlDbType.Int, ParameterDirection.ReturnValue);
                    try
                    {
                        result = DB.DBHelper.ExecuteParametersSource("Prd_Inp_Pack_ColorBoxWeight", parameters, ExecuteType.StoredProcedure);

                    }
                    catch (Exception ex)
                    {
                        txtMessage.AddMessage(ex.Message, true);
                        ClearScan();
                        txtSN.SetFoucs();
                        SetErrorOrSuccImage(true, false);
                        return;
                    }
                }
                if (result.HasError)
                {
                    txtMessage.AddMessage(result.Message, true);
                    ClearScan();
                    txtSN.SetFoucs();
                    SetErrorOrSuccImage(true, false);
                    return;
                }

                if ((int)result.Value1["Return_Value"] != 1)
                {
                    txtMessage.AddMessage(result.Value1["Return_Message"].ToString(), true);
                    ClearScan();
                    txtSN.SetFoucs();
                    SetErrorOrSuccImage(true, false);
                    return;
                }
                else
                {
                    txtMessage.AddMessage(result.Value1["Return_Message"].ToString(), false);
                    ClearScan();
                    txtWeight.SetFoucs();
                    SetErrorOrSuccImage(false, false);
                }
                uph.Quantity++;
                Print(result.Value2);
                txtWeight.Text = string.Empty;
                txtSN.SetFoucs();
            }
            
        }

        /// <summary>
        /// 补印窗口消息传输
        /// </summary>
        /// <param name="message"></param>
        /// <param name="isError"></param>
        public void AddMessage(string message, bool isError)
        {
            txtMessage.AddMessage(message, isError);

            SetErrorOrSuccImage(isError, false);
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

        //private void txtWeight_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        //{
        //    txtWeight_KeyDown(null, null);
        //    txtSN.Focus();
        //}

        //private void txtWeight_ValueChangeEvent(object sender, EventArgs e)
        //{
        //    txtSN.Focus();
        //}

       
    }
}
