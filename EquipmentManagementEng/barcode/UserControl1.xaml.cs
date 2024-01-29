using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
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

namespace barcode
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
        private string PalletSN=string.Empty;

        string TempltePath, DataFileName,batPath;
        private DataTable ScanSource = new DataTable();

        public UserControl1(Framework.SystemAuthority authority)
            : base(authority)
        {
            InitializeComponent();

           

            //定义扫描后显示相对关系
            ScanSource.Columns.Add("BoxId", typeof(System.Int64));
            ScanSource.Columns.Add("BoxSN", typeof(System.String));
            ScanSource.Columns.Add("ScanCode", typeof(System.String));
            

            root.Background = new ImageBrush(WinAPI.File.ImageHelper.ConvertToImageSource(Component.App.BackgroudImage));

            //tbUser.Text = Framework.App.User.UserCode;
            //if (!string.IsNullOrEmpty(Framework.App.User.UserDesc))
            //    tbUser.Text = tbUser.Text + "/" + Framework.App.User.UserDesc;
        }

        private void UserVendor_Loaded(object sender, RoutedEventArgs e)
        {
            if (_IsLoad)
                return;
            _IsLoad = true;

            //默认选择工单
            btnSelectorMO_Click(null, null);

            List<KeyValuePair<string, string>> source = new List<KeyValuePair<string, string>>();
            source.Add(new KeyValuePair<string, string>("BoxSN", "外箱条码"));
            source.Add(new KeyValuePair<string, string>("LotSN", "批次条码"));
            source.Add(new KeyValuePair<string, string>("Mac", "MAC"));
            source.Add(new KeyValuePair<string, string>("DeviceSerialNumber", "设备标识"));
            source.Add(new KeyValuePair<string, string>("GponSN", "GponSN"));
            source.Add(new KeyValuePair<string, string>("CISN", "CISN"));
            source.Add(new KeyValuePair<string, string>("DSN", "DSN"));
            source.Add(new KeyValuePair<string, string>("EN", "EN"));

           
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
            //gridTemplate.Children.Clear();
            //gridTemplate.RowDefinitions.Clear();
            //btnAddPrint.Visibility = Visibility.Collapsed;

            CloseTemplate();
            TemplatePath.Clear();
            PrinterComboBoxs.Clear();

            PalletQty = 0;

            if (SelectRow == null)
            {
                //tbMOCode.Text = string.Empty;
                //tbQty.Text = string.Empty;
                //tbItemCode.Text = string.Empty;
                //tbItemSpec.Text = string.Empty;
                //tbCustomer.Text = string.Empty;
                //tbPalletQty.Text = string.Empty;
               // btnAddPrint.Visibility = Visibility.Collapsed;
            }
            else
            {
                int.TryParse(SelectRow["PalletQty"].ToString(), out PalletQty);
                //tbMOCode.Text = SelectRow["MOCode"].ToString();
                //tbQty.Text = SelectRow["Qty"].ToString();
                //tbItemCode.Text = SelectRow["ItemCode"].ToString();
                //tbItemSpec.Text = SelectRow["ItemSpecification"].ToString();
                //tbPalletQty.Text = PalletQty.ToString();
                //tbENType.Text = SelectRow["ENType"].ToString();
                //tbCustomer.Text = SelectRow["CustomerCode"].ToString();
                if (!string.IsNullOrEmpty(SelectRow["CustomerDesc"].ToString().Trim()))
                    //tbCustomer.Text = tbCustomer.Text + "/" + SelectRow["CustomerDesc"].ToString();
                if (PalletQty < 1)
                {
                    //txtMessage.AddMessage(string.Format("料号[{0}]未设置每栈板箱数！", SelectRow["ItemCode"]), true);
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
                        //gridTemplate.RowDefinitions.Add(new RowDefinition());
                        TextBlock block = new TextBlock();
                        block.Text = "选择打印机";
                        block.FontSize = 16;
                        block.FontWeight = FontWeights.Bold;
                        block.HorizontalAlignment = HorizontalAlignment.Center;
                        //block.SetValue(Grid.RowProperty, gridTemplate.RowDefinitions.Count - 1);
                        block.SetValue(Grid.ColumnProperty, 1);
                       // gridTemplate.Children.Add(block);

                        //if(Authority!=null&&Authority.IsAddPrint)
                        {
                          
                        }

                    }
                    else
                    {
                       
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
                            continue;
                        }

                        if (buffer == null)
                            continue;

                        string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Template", helper.FileName);
                        TempltePath = path; 
                        try
                        {
                            WinAPI.File.FileHelper.Write(buffer, path);
                            TempletResult.Text = "打印模板下载完成，模板名称为:" + helper.FileName;
                        }
                        catch (Exception ex)
                        {
                            continue;
                        }
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Error);
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
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
        }


        public void Print(DataSet set)
        {
            //生成bat文件  System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Template", "templtdata.txt");
            batPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Template", "BarCodePrint.bat");
            System.IO.StreamWriter filebat = new System.IO.StreamWriter(batPath, false, Encoding.Default);
            string s = "";
            //系统位数判断，根据位数自动定位软件位置
            //64位
                if (IntPtr.Size == 4)
                {
                    s = @"C:" + "\r\n" + @"cd C:\Program Files (x86)\Seagull\BarTender Suite" + "\r\n";
                    s += string.Format("bartend.exe /F=\"{0}\" /D=\"{1}\"" + "/P /X", TempltePath, DataFileName);
                }
                //32位
                else if (IntPtr.Size == 8)
                {
                    s = @"C:" + "\r\n" + @"cd C:\Program Files\Seagull\BarTender Suite" + "\r\n";
                    s += string.Format("bartend.exe /F=\"{0}\" /D=\"{1}\"" + "/P /X", TempltePath, DataFileName);
                }
                filebat.Write(s);
                filebat.Close();
            RunBat(batPath);
            OneCode.Text = string.Empty;
        }

        private void RunBat(string batPath)
        {
            Process pro = new Process();
            FileInfo file = new FileInfo(batPath);
            //pro.StartInfo.WorkingDirectory = path;
            pro.StartInfo.FileName = batPath;
            pro.StartInfo.CreateNoWindow = true;
            pro.Start();
            pro.WaitForExit();

        }
        private void txtScan_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //Submit();
            }
        }

        private ArrayList SplitCString(string strSource, string ch)
        {
            ArrayList alData = new ArrayList();
            char[] charSplit = ch.ToCharArray();
            string[] sArray1 = strSource.Split(charSplit);
            foreach (string str in sArray1)
            {
                alData.Add(str.ToString());
            }
            return alData;
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            btnSelectorMO_Click(null, null);
        }

        private void UserVendor_Unloaded(object sender, RoutedEventArgs e)
        {
            if (System.IO.File.Exists(DataFileName))
            {
                System.IO.File.Delete(DataFileName);
            }
            if (System.IO.File.Exists(batPath))
            {
                System.IO.File.Delete(batPath);
            }
            if (System.IO.File.Exists(TempltePath))
            {
                System.IO.File.Delete(TempltePath);
            }
        }

        /***************

对扫描的数据进行解析

****************/
        private bool MesScanParse(string src, ref Dictionary<string, string> m_mapMesScan)
        {
            string str, temp, key;
            int pos = 0;
            //src.ToLower();
            src = src.Replace("qrcode", "");
            src = src.Replace("QRCODE", "");
            src = src.Replace(" ", "");
            ArrayList alData = new ArrayList();
           alData = SplitCString(src, "\r\n");
            for (int i = 0; i < (int)alData.Count; i++)
            {
                if (alData[i].ToString() == String.Empty)
                {
                    continue;
                }
                temp = alData[i].ToString();
                pos = temp.IndexOf("=");
                key = temp.Substring(0, pos);
                if (key.ToString() == String.Empty)
                {
                    //Debug("key is empty", false);
                    continue;
                }
                str = temp.Substring(pos + 1, temp.Length - pos - 1);
                str.Trim();
                key.Trim();
                //key.ToUpper();
                key = key.ToLower();
                m_mapMesScan[key] = str;
                //Debug(temp,false);
            }
            return true;
        }
        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string strOne;
                //二维码
                Dictionary<string, string> m_mapMesScan = new Dictionary<string, string>();
                m_mapMesScan.Clear();
                strOne = OneCode.Text;
                strOne.Trim();
                MesScanParse(strOne, ref m_mapMesScan);
                //检测扫描是否完成，以stbtype为结束标志，如果没有这个的话将会抛出异常，异常处理里不做任何操作直接返回。
                try
                 {
                     string tempa = m_mapMesScan["stbtype"];
                 }
                catch (Exception)
                 {
                     return;
                 }
                string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Template", "templtdata.txt");
                DataFileName = path;



                FileStream fs = new FileStream(path, FileMode.Create);
                StreamWriter wr = null;
                wr = new StreamWriter(fs);
                try
                {
                    wr.WriteLine(m_mapMesScan["extsn"] +",,"+ m_mapMesScan["smartcard"]);
                }catch
                {
                    wr.WriteLine(m_mapMesScan["extsn"] +",,"+ m_mapMesScan["chipid"]);
                } 
                wr.Close();
                Print(null);
            }
        }
    }
}