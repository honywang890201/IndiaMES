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
using System.Collections;
using System.Threading;

namespace BoxPrint_BarcodeCompare
{
    /// <summary>
    /// BarcodeCompare.xaml 的交互逻辑
    /// </summary>
    public partial class BarcodeCompare : Component.Controls.User.UserVendor
    {
        const string MES_ABS_UserId = "UserId";
        const string MES_ABS_UserCode = "UserCode";
        const string MES_ABS_UserName = "UserName";
        const string MES_ABS_ResId = "ResId";
        const string MES_ABS_ResCode = "ResCode";
        const string MES_ABS_ResName = "ResName";
        const string MES_ABS_LineId = "LineId";
        const string MES_ABS_LineCode = "LineCode";
        const string MES_ABS_LineName = "LineName";
        const string MES_ABS_ShiftTypeId = "ShiftTypeId";
        const string MES_ABS_ShiftTypeCode = "ShiftTypeCode";
        const string MES_ABS_ShiftTypeName = "ShiftTypeName";
        const string MES_ABS_OPId = "OPId";
        const string MES_ABS_OPCode = "OPCode";
        const string MES_ABS_OPName = "OPName";
        const string MES_ABS_ReturnValue = "ReturnValue";
        const string MES_ABS_ReturnMessage = "ReturnMessage";
        const string MES_ABS_IsTransferMO = "IsTransferMO";
        const string MES_ABS_MOId = "MOId";
        const string MES_ABS_LotSNType = "LotSNType";
        const string MES_ABS_LotSN = "LotSN";
        const string MES_ABS_MOCode = "MOCode";
        const string MES_ABS_OrderCode = "OrderCode";
        const string MES_ABS_CustomerName = "CustomerName";
        const string MES_ABS_CustomerItemCode = "CustomerItemCode";
        const string MES_ABS_CustomerItemName = "CustomerItemName";
        const string MES_ABS_CustomerItemSpecification = "CustomerItemSpecification";
        const string MES_ABS_ModelCode = "ModelCode";
        const string MES_ABS_ItemCode = "ItemCode";
        const string MES_ABS_ItemName = "ItemName";
        const string MES_ABS_DeviceType = "DeviceType";
        const string MES_ABS_Type = "Type";
        const string MES_ABS_MOQty = "MOQty";
        const string MES_ABS_LotId = "LotId";
        //从MES获取
        const string MES_STAR_OUI = "OUI";
        const string MES_STAR_SN = "SN";
        const string MES_STAR_CHIPID = "ChipId";
        const string MES_STAR_EXTSN = "Extsn";
        const string MES_STAR_HW = "HW";
        const string MES_STAR_SW = "SW";
        const string MES_STAR_VSCID = "Vscid";
        const string MES_STAR_STBTYPE = "StbType";
        const string MES_STAR_PRODUCTTYPE = "Product";
        //从二维码获取
        const string MES_STAR_SCAN_MAC = "mac";
        const string MES_STAR_SCAN_OUI = "oui";
        const string MES_STAR_SCAN_SN = "sn";
        const string MES_STAR_SCAN_CHIPID = "chipid";
        const string MES_STAR_SCAN_EXTSN = "extsn";
        const string MES_STAR_SCAN_HW = "hw";
        const string MES_STAR_SCAN_SW = "sw";
        const string MES_STAR_SCAN_PRODUCTTYPE = "producttype";
        const string MES_STAR_SCAN_STBTYPE = "stbtype";
        //四达标清没有此项，高清才有。
        const string MES_STAR_SCAN_SMARTCARD = "smartcard";

        const int MES_ABS_INTERFACE_10 = 10;
        const int MES_ABS_INTERFACE_12 = 12;
        const int MES_ABS_INTERFACE_13 = 13;
        const int MES_ABS_INTERFACE_15 = 15;
        const int MES_ABS_INTERFACE_20 = 20;
        const int MES_ABS_INTERFACE_30 = 30;
        const int MES_ABS_INTERFACE_40 = 40;
        const int MES_ABS_INTERFACE_41 = 41;
        const int MES_ABS_INTERFACE_60 = 60;
        const int MES_ABS_INTERFACE_70 = 70;
        const int MES_ABS_INTERFACE_71 = 71;

        //数据保存节点信息
        private string setting_path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Framework.SysVar.SETTING_FILE_NAME);
        const string BARCODE_COMPARE_INFO = "BardCompareInfo";
        const string BARCODE_CUSTOMER_INDEX = "CustomerIndex";
        const string BARCODE_CHECK_VSCID = "CheckVscID";//check range
        const string BARCODE_VSCID_START = "VscIDStart";
        const string BARCODE_VSCID_END = "VscIDEnd";
        const string BARCODE_VIDEO_TYPE = "VideoType";//AV ,HDMI

        private static readonly object Lock = new object();
        private Dictionary<string, string> mapLoginData;
        private string OrderNumber = string.Empty;
        private long MOId = -1;
        //四达和华电识别用，false时为四达，true时为华电
        private bool IsStarOrHuadian = false;
        //四达高清及标清识别标志，false为标清（有卡）.true为高清（无卡）
        private bool IsStardOrHDMI = false;
        private DataTable table = null;
        private DataRowView SelectRow = null;
        private string Header = null;

        public BarcodeCompare(Framework.SystemAuthority systemAuthority)
        {
            InitializeComponent();
            root.Background = new ImageBrush(WinAPI.File.ImageHelper.ConvertToImageSource(Component.App.BackgroudImage));

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //OrderTip.Content = "order" + "compare";
            Button_Click2(sender, e);
        }
        private void Button_Click2(object sender, RoutedEventArgs e)
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
                        MessageBox.Show("工单错误" + selector.moId.ToString(), "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    SelectRow = table.DefaultView[0];
                    OrderNum.Content = "      订单号为:" + SelectRow["MOCode"].ToString() + "        " + SelectRow["CustomerDesc"] + "项目,客户代码:" + SelectRow["CustomerCode"].ToString();
                    OrderNumber = SelectRow["MOCode"].ToString();
                    MOId = selector.moId;
                    //如果获取到的客户编号是0016，则是四达项目
                    if (SelectRow["CustomerCode"].ToString() == "0016")
                    {
                        IsStarOrHuadian = false;

                    }
                    //客户编号是00101为华电项目
                    if (SelectRow["CustomerCode"].ToString() == "00101")
                    {
                        IsStarOrHuadian = true;
                    }
                    switch (IsStarOrHuadian)
                    {
                        case true:
                            CheckVSCID.IsEnabled = false;
                            CheckVSCID.IsChecked = false;
                            VscStart.IsEnabled = false;
                            VscEnd.IsEnabled = false;
                            StandType.IsEnabled = false;
                            HdmiType.IsEnabled = false;
                            break;
                        case false:
                            CheckVSCID.IsEnabled = true;
                            VscStart.IsEnabled = true;
                            VscEnd.IsEnabled = true;
                            StandType.IsEnabled = true;
                            HdmiType.IsEnabled = true;
                            break;
                    }
                    try
                    {
                        WinAPI.File.INIFileHelper.Write(BARCODE_COMPARE_INFO, BARCODE_CUSTOMER_INDEX, CustomerComBox.SelectedIndex.ToString(), setting_path);
                    }
                    catch { }


                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
        }

        private void UserVendor_Loaded(object sender, RoutedEventArgs e)
        {
            //return;
            //init
            //CustomerComBox.SelectedIndex = 0;

            Button_Click2(null, null);
            CustomerComBox.SelectedIndex = int.Parse(WinAPI.File.INIFileHelper.Read(BARCODE_COMPARE_INFO, BARCODE_CUSTOMER_INDEX, setting_path, "0"));

            if (int.Parse(WinAPI.File.INIFileHelper.Read(BARCODE_COMPARE_INFO, BARCODE_CHECK_VSCID, setting_path, "0")) == 1)
            {
                //CheckVscID
                CheckVSCID.IsChecked = true;
            }
            VscStart.Text = WinAPI.File.INIFileHelper.Read(BARCODE_COMPARE_INFO, BARCODE_VSCID_START, setting_path, "0");
            VscEnd.Text = WinAPI.File.INIFileHelper.Read(BARCODE_COMPARE_INFO, BARCODE_VSCID_END, setting_path, "0");
            if (int.Parse(WinAPI.File.INIFileHelper.Read(BARCODE_COMPARE_INFO, BARCODE_VIDEO_TYPE, setting_path, "0")) == 1)
            {
                //hdmi
                HdmiType.IsChecked = true;
            }
            else
            {
                StandType.IsChecked = true;
            }
            OneCode.Focus();
            if (CustomerComBox.SelectedIndex == 0)
            {
                CheckVSCID.IsChecked = false;
            }
        }
        private void CodeKeyDown(object sender, KeyEventArgs e)
        {
            //Debug("key down",false);
        }

        private void CodeKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                
                if (OrderNumber == string.Empty)
                {
                    Debug("请选择订单", false);
                    return;
                }
                string strOne, strTwo, temp, strLotID = "";
                string strOneSub1 = "", strOneSub2 = "";
                string strVscidStar = "", strVscidEnd = "";
                int iDataNum = 0;
                System.Windows.Documents.FlowDocument doc;
                if (OneCode.IsFocused)
                {
                    if (OneCode.Text == String.Empty)
                    {
                        //Debug("请扫描机身二维码", false);
                    }
                    else
                    {
                        strOne = OneCode.Text;
                        strOne.Trim();
                        if (strOne.IndexOf('_') == -1 && IsStarOrHuadian)
                        {
                            OneCode.Clear();
                        }
                        else
                        {
                            if (CheckVSCID.IsChecked == true)
                            {
                                //判断VSCID范围
                                if (String.Compare(strOne, strVscidStar, true) > -1 &&
                                    String.Compare(strOne, strVscidEnd, true) < 1)
                                {
                                    //
                                }
                                else
                                {
                                    Debug("VSCID不在范围", true);
                                    OneCode.Clear();
                                    return;
                                }

                            }
                            TwoCode.Focus();
                        }
                        Debug(string.Empty, false);
                        doc = TwoCode.Document;
                        doc.Blocks.Clear();
                    }
                    return;
                }
                else if (!TwoCode.IsFocused)
                {
                    Debug("请扫描盒子界面二维码", false);
                    return;
                }
                //二维码
                Dictionary<string, string> m_mapMesScan = new Dictionary<string, string>();
                Dictionary<string, string> m_mapGetFromMes = new Dictionary<string, string>();

                m_mapMesScan.Clear();
                strOne = OneCode.Text;
                strOne.Trim();
                strTwo = new TextRange(TwoCode.Document.ContentStart,
                               TwoCode.Document.ContentEnd).Text;

                //华电
                if (IsStarOrHuadian)
                {
                    iDataNum = 3;
                    MesScanParse(strTwo, ref m_mapMesScan);
                    if (m_mapMesScan.Count() < iDataNum)
                    {
                        return;
                    }

                }
                ////四达标清
                //else if (!IsStarOrHuadian && StandType.IsChecked == true)
                //{
                //    iDataNum = 8;
                //}
                ////四达高清
                //else if (!IsStarOrHuadian && HdmiType.IsChecked == true)
                //{
                //    iDataNum = 9;
                //}
                else
                {
                    MesScanParse(strTwo, ref m_mapMesScan);
                    //检测扫描是否完成，以stbtype为结束标志，如果没有这个的话将会抛出异常，异常处理里不做任何操作直接返回。
                    try
                    {
                        string tempa = m_mapMesScan[MES_STAR_SCAN_STBTYPE];
                    }
                    catch (Exception)
                    {
                        return;
                    }
                    //判断是否为高清，如果没有smartcard项目的话会抛出异常，在异常处理中直接将该项目定义为标清机
                    try
                    {
                        string tempa = m_mapMesScan[MES_STAR_SCAN_SMARTCARD];
                        IsStardOrHDMI = true;
                    }
                    catch (Exception)
                    {
                        IsStardOrHDMI = false;
                    }


                }
                //if (m_mapMesScan.Count() < iDataNum)
                //{
                //    return;
                //}
                //区分四达和华电项目
                //if (IsStarOrHuadian)
                //{
                //    //华电项目
                //    //strOne.ToLower();
                //    strOneSub1 = strOne.Substring(0, strOne.IndexOf('_'));
                //    strOneSub2 = strOne.Substring(strOne.IndexOf('_') + 1, strOne.Length - strOneSub1.Length - 1);

                //    //Debug(strOneSub1, false);
                //    //Debug(strOneSub2, false);
                //    //Debug(m_mapMesScan[MES_STAR_SCAN_MAC], false);
                //    //Debug(m_mapMesScan[MES_STAR_SCAN_SN], false);
                //    if (String.Compare(strOneSub1, m_mapMesScan[MES_STAR_SCAN_MAC], false) != 0
                //        || String.Compare(strOneSub2, m_mapMesScan[MES_STAR_SCAN_SN], false) != 0)
                //    {
                //        temp = "对比数据有误," + strOneSub1 + "," + strOneSub2 + "; " + m_mapMesScan[MES_STAR_SCAN_MAC] + "," + m_mapMesScan[MES_STAR_SCAN_SN];
                //        Debug(temp, false);
                //        OneCode.Clear();
                //        doc = TwoCode.Document;
                //        doc.Blocks.Clear();
                //        OneCode.Focus();
                //        return;
                //    }

                //    Parameters parameters = new Parameters();
                //    parameters.Add("LotSN", strOneSub1);//mac nvarhar
                //    parameters.Add("GetCode", m_mapMesScan[MES_STAR_SCAN_CHIPID]);//chipid nvarhar
                //    parameters.Add("MOId", MOId);//bigint
                //    parameters.Add("LineId", Framework.App.Resource.LineId);//bigint
                //    parameters.Add("ResId", Framework.App.Resource.ResourceId);//bigint
                //    parameters.Add("ShiftTypeId", Framework.App.Resource.ShiftTypeId);//bigint
                //    parameters.Add("UserId", Framework.App.User.UserId);//bigint
                //    parameters.Add("OPId", Framework.App.Resource.StationId);//bigint
                //    parameters.Add("PluginId", PluginId);//bigint
                //    parameters.Add("Comment", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                //    parameters.Add("FocusedParam", null, SqlDbType.NVarChar, 50, ParameterDirection.Output);
                //    parameters.Add("Return_Message", null, SqlDbType.NVarChar, int.MaxValue, ParameterDirection.Output);
                //    parameters.Add("ReturnValue", null, SqlDbType.Int, ParameterDirection.Output);

                //    Result<Parameters, DataSet> result = null;
                //    try
                //    {
                //        result = DB.DBHelper.ExecuteParametersSource("Prd_Inp_BindingProductSN", parameters, ExecuteType.StoredProcedure);
                //    }
                //    catch (Exception ex)
                //    {
                //        temp = "Exception:" + ex.Message;
                //        Debug(ex.Message, true);
                //        OneCode.Clear();
                //        doc = TwoCode.Document;
                //        doc.Blocks.Clear();
                //        OneCode.Focus();
                //        return;
                //    }

                //    if (result.HasError)
                //    {
                //        temp = "Result Error:" + result.Message;
                //        Debug(temp, true);
                //        OneCode.Clear();
                //        doc = TwoCode.Document;
                //        doc.Blocks.Clear();
                //        OneCode.Focus();
                //        return;
                //    }

                //    if ((int)result.Value1["ReturnValue"] != 1)
                //    {
                //        temp = result.Value1["Return_Message"].ToString();
                //        Debug(temp, false);
                //    }
                //    else
                //    {
                //        temp = result.Value1["Return_Message"].ToString();
                //        Debug(temp, true);
                //    }
                //    OneCode.Clear();
                //    doc = TwoCode.Document;
                //    doc.Blocks.Clear();
                //    OneCode.Focus();
                //}
                //四达项目
                //else
                if (!IsStarOrHuadian)
                {

                    if (!IsStardOrHDMI)
                    {
                        //如果扫描的是chipid,需要转为extsn
                        if (String.Compare(strOne, m_mapMesScan[MES_STAR_SCAN_CHIPID], true) == 0)
                        {
                            strOne = m_mapMesScan[MES_STAR_SCAN_EXTSN];
                        }
                    }
                    //数据解析出来了
                    //标清贴纸上有chipid和外部sn，扫描到任意一个条码都可以和盒子号码对比，成功后即可

                    if ((m_mapMesScan.Keys.Contains(MES_STAR_SCAN_EXTSN) && String.Compare(strOne, m_mapMesScan[MES_STAR_SCAN_EXTSN], true) == 0) ||
                        (m_mapMesScan.Keys.Contains(MES_STAR_SCAN_SMARTCARD) && String.Compare(strOne, m_mapMesScan[MES_STAR_SCAN_SMARTCARD], true) == 0) ||
                        (m_mapMesScan.Keys.Contains(MES_STAR_SCAN_CHIPID) && String.Compare(strOne, m_mapMesScan[MES_STAR_SCAN_CHIPID], true) == 0))
                    {
                        //标清的一维码有两个sn,chipip
                        //通过20,判断是不是可以测试

                        if (MesAbs20(m_mapMesScan[MES_STAR_SCAN_EXTSN], ref strLotID))
                        {
                            //是这个工位的，上传chipID和VSCID
                            if (MesAbs70(strLotID, strOne, m_mapMesScan[MES_STAR_SCAN_CHIPID]))
                            {
                                //获取所有的数据进行比较,获取所有数据
                                m_mapGetFromMes.Clear();
                                if (MesAbs71(strOne, ref m_mapGetFromMes))
                                {
                                    if (MesCompare(m_mapGetFromMes, m_mapMesScan))
                                    {
                                        //过站
                                        if (MesAbs30(strLotID))
                                        {
                                            PlayResultWav(true);
                                        }
                                        else
                                        {
                                           
                                        }
                                    }
                                    else
                                    {
                                        Debug("mes号码和机顶盒号码对比有误", false);

                                    }
                                    TwoCode.Focus();
                                }
                                else
                                {
                                    //Debug("MES 71 失败", false);
                                }
                            }
                            else
                            {
                                //Debug("MES 70 失败", false);
                            }
                        }
                        else
                        {
                            //不是这个工位的，提示
                            //m_colorGroup = m_color_red;
                            //temp = String.Format("数据不是这个工位的，请检查外部 extsn=" + m_mapMesScan[MES_STAR_SCAN_EXTSN]);
                            //Debug(temp, true);
                        }
                    }
                    else
                    {
                        Debug("机身贴纸条码和实际号码不一致", false);
                    }

                    if (sender.GetHashCode() == OneCode.GetHashCode() && OneCode.Text != String.Empty)
                    {
                        //MessageBox.Show(OneCode.Text, "调试", MessageBoxButton.OK, MessageBoxImage.Error);
                        //Debug(OneCode.Text,true);
                        TwoCode.Focus();
                    }
                    else if (sender.GetHashCode() == TwoCode.GetHashCode() && strTwo != String.Empty)
                    {
                        //Debug(strTwo, false);
                        OneCode.Clear();
                        doc = TwoCode.Document;
                        doc.Blocks.Clear();
                        OneCode.Focus();
                    }
                }

            }
        }
        /**
         * 播放结果声音**/
        private void PlayResultWav(bool ResultWAV)
        {
            if(ResultWAV)
            {
                System.Media.SoundPlayer player = new System.Media.SoundPlayer("./SoundFile/Pass.wav");
                player.Play();
            }
            else
            {
                System.Media.SoundPlayer player = new System.Media.SoundPlayer("./SoundFile/NG.wav");
                player.Play();
            }
        }
        /* bShowResult=false时代表失败
             */
        private void Debug(string msg, bool bShowResult)
        {
            ErrorTip.Content = msg;
            if (msg == string.Empty)
            {
                CompareResultTip.Content = string.Empty;
                OneCode.Background = new SolidColorBrush(Colors.White);
                TwoCode.Background = new SolidColorBrush(Colors.White);
            }
            else
            {
                if (!bShowResult)
                {
                    OneCode.Background = new SolidColorBrush(Colors.Red);
                    TwoCode.Background = new SolidColorBrush(Colors.Red);
                    CompareResultTip.Content = "失败!!!";
                    PlayResultWav(false);
                    CompareResultTip.Foreground = new SolidColorBrush(Colors.Red);
                }
                else
                {
                    OneCode.Background = new SolidColorBrush(Colors.LightGreen);
                    TwoCode.Background = new SolidColorBrush(Colors.LightGreen);
                    CompareResultTip.Content = "成功!!!";
                    CompareResultTip.Foreground = new SolidColorBrush(Colors.Green);
                }
            }







            //ErrorTip.Content = msg;
            //if (msg == String.Empty)
            //{
            //    CompareResultTip.Content = "成功!!!";
            //    CompareResultTip.Foreground = new SolidColorBrush(Colors.Green);
            //}
            //else
            //{
            //    if (bShowResult)
            //    {
            //        CompareResultTip.Content = "失败!!!";
            //    }
            //    CompareResultTip.Foreground = new SolidColorBrush(Colors.Red);
            //}
            //MessageBox.Show(msg, "调试", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    Debug("key is empty", false);
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

        /********************

        扫码枪号号码对比

        ********************/
        private bool MesCompare(Dictionary<string, string> m_mapGet, Dictionary<string, string> m_mapScan)
        {
            string temp;
            bool result = true;
            if (String.Compare(m_mapGet[MES_STAR_OUI], m_mapScan[MES_STAR_SCAN_OUI], true) != 0)
            {
                temp = String.Format("OUI %s,scanValue %s", m_mapGet[MES_STAR_OUI], m_mapScan[MES_STAR_SCAN_OUI]);
                Debug(temp, true);
                return false;
            }
            if (String.Compare(m_mapGet[MES_STAR_SN], m_mapScan[MES_STAR_SCAN_SN], true) != 0)
            {
                temp = String.Format("SN %s,scanValue %s", m_mapGet[MES_STAR_SN], m_mapScan[MES_STAR_SCAN_SN]);
                Debug(temp, true);
                return false;
            }
            if (String.Compare(m_mapGet[MES_STAR_CHIPID], m_mapScan[MES_STAR_SCAN_CHIPID], true) != 0)
            {
                temp = String.Format("ChipId %s,chipid %s", m_mapGet[MES_STAR_CHIPID], m_mapScan[MES_STAR_SCAN_CHIPID]);
                Debug(temp, true);
                return false;
            }
            if (String.Compare(m_mapGet[MES_STAR_EXTSN], m_mapScan[MES_STAR_SCAN_EXTSN], true) != 0)
            {
                temp = String.Format("Extsn %s,extsn %s", m_mapGet[MES_STAR_EXTSN], m_mapScan[MES_STAR_SCAN_EXTSN]);
                Debug(temp, true);
                return false;
            }
            if (String.Compare(m_mapGet[MES_STAR_HW], m_mapScan[MES_STAR_SCAN_HW], true) != 0)
            {
                temp = String.Format("HW %s,hw %s", m_mapGet[MES_STAR_HW], m_mapScan[MES_STAR_SCAN_HW]);
                Debug(temp, true);
                return false;
            }
            if (String.Compare(m_mapGet[MES_STAR_SW], m_mapScan[MES_STAR_SCAN_SW], true) != 0)
            {
                temp = String.Format("SW %s,sw %s", m_mapGet[MES_STAR_SW], m_mapScan[MES_STAR_SCAN_SW]);
                Debug(temp, true);
                return false;
            }
            if (!IsStardOrHDMI)
            {
                //标清没有smcartcard
                if (String.Compare(m_mapGet[MES_STAR_VSCID], m_mapScan[MES_STAR_SCAN_EXTSN], true) != 0)
                {
                    temp = String.Format("Vscid %s,smartcard %s", m_mapGet[MES_STAR_VSCID], m_mapScan[MES_STAR_SCAN_EXTSN]);
                    Debug(temp, true);
                    return false;
                }
            }
            else
            {
                //四达要求，vscid改成smartcard
                if (String.Compare(m_mapGet[MES_STAR_VSCID], m_mapScan[MES_STAR_SCAN_SMARTCARD], true) != 0)
                {
                    temp = String.Format("Vscid %s,smartcard %s", m_mapGet[MES_STAR_VSCID], m_mapScan[MES_STAR_SCAN_SMARTCARD]);
                    Debug(temp, true);
                    return false;
                }
            }
            if (String.Compare(m_mapGet[MES_STAR_STBTYPE], m_mapScan[MES_STAR_SCAN_STBTYPE], true) != 0)
            {
                temp = String.Format("StbType %s,stbtype %s", m_mapGet[MES_STAR_STBTYPE], m_mapScan[MES_STAR_SCAN_STBTYPE]);
                Debug(temp, true);
                return false;
            }

            if (String.Compare(m_mapGet[MES_STAR_PRODUCTTYPE], m_mapScan[MES_STAR_SCAN_PRODUCTTYPE], true) != 0)
            {
                temp = String.Format("Product %s,product %s", m_mapGet[MES_STAR_PRODUCTTYPE], m_mapScan[MES_STAR_SCAN_PRODUCTTYPE]);
                Debug(temp, true);
                return false;
            }
            return result;
        }

        private bool MesAbs(int commandType, ref List<string> strArray, ref string csResult)
        {
            //return false;
            Monitor.Enter(Lock);
            string commandTemp, strProduceName, temp;
            bool bResult = true;

            commandTemp = String.Format("{0}", commandType);
            strProduceName = String.Format("Interf_{0}", commandType);
            Parameters parameters = new Parameters();
            switch (commandType)
            {
                case 10://登录
                    if (strArray.Count() < 3)
                    {
                        bResult = false;
                        break;
                    }

                    commandTemp = String.Format("<document><UserCode>{0}</UserCode><UserPwd>{1}</UserPwd><ResCode>{2}</ResCode></document>", strArray[0], strArray[1], strArray[2]);
                    break;
                case 12://获取工单的测试信息
                    if (strArray.Count() < 1)
                    {
                        bResult = false;
                        break;
                    }
                    commandTemp = String.Format("<document><MOId>{0}</MOId></document>", strArray[0]);
                    break;
                case 13://校验工单是否存在
                    if (strArray.Count() < 1)
                    {
                        bResult = false;
                        break;
                    }
                    commandTemp = String.Format("<document><MOCode>{0}</MOCode></document>", strArray[0]);
                    break;
                case 15://由成品工单获取BOSA型号列表
                    if (strArray.Count() < 1)
                    {
                        bResult = false;
                        break;
                    }
                    commandTemp = String.Format("<document><MOId>%s</MOId></document>", strArray[0]);
                    break;
                case 20://校验批次条码
                    if (strArray.Count() < 8)
                    {
                        bResult = false;
                        break;
                    }
                    commandTemp = String.Format("<document><IsTransferMO>{0}</IsTransferMO><MOId>{1}</MOId><LotSNType>{2}</LotSNType><LotSN>{3}</LotSN><LineId>{4}</LineId><ResId>{5}</ResId><UserId>{6}</UserId><OPId>{7}</OPId></document>",
                        new object[] { strArray[0], strArray[1], strArray[2], strArray[3], strArray[4], strArray[5], strArray[6], strArray[7] });
                    break;
                case 30://过站
                    if (strArray.Count() < 11)
                    {
                        bResult = false;
                        break;
                    }
                    //bResult = false;
                    commandTemp = String.Format("<document><IsTransferMO>{0}</IsTransferMO><MOId>{1}</MOId><LotId>{2}</LotId><LineId>{3}</LineId><ResId>{4}</ResId><ShiftTypeId>{5}</ShiftTypeId><UserId>{6}</UserId><OPId>{7}</OPId><IsPass>{8}</IsPass><TestLog>{9}</TestLog><FilePath>{10}</FilePath></document> ",
                        new object[] { strArray[0], strArray[1], strArray[2], strArray[3], strArray[4], strArray[5], strArray[6], strArray[7], strArray[8], strArray[9], strArray[10] });
                    break;
                case 40://获取批次条码对应的个性化数据
                    if (strArray.Count() < 1)
                    {
                        bResult = false;
                        break;
                    }
                    commandTemp = String.Format("<document><LotId>{0}</LotId></document> ", strArray[0]);
                    break;
                case 41://获取Bosa的所有信息
                    if (strArray.Count() < 1)
                    {
                        bResult = false;
                        break;
                    }
                    commandTemp = String.Format("<document><LotId>{0}</LotId></document> ", strArray[0]);
                    break;
                case 60:
                    if (strArray.Count() < 8)
                    {
                        bResult = false;
                        break;
                    }
                    commandTemp = String.Format("<document><GetCode>{0}</GetCode><LotId>{1}</LotId><MOId>{2}</MOId><LineId>{3}</LineId><ResId>{4}</ResId><ShiftTypeId>{5}</ShiftTypeId><UserId>{6}</UserId><OPId>{7}</OPId></document> ",
                        strArray);
                    break;
                case 70:
                    if (strArray.Count() < 16)
                    {
                        bResult = false;
                        break;
                    }
                    commandTemp = String.Format("<document><Order_Number>{0}</Order_Number><CSN>{1}</CSN><PIN>{2}</PIN><Smartcard_Number>{3}</Smartcard_Number><CPd>{4}</CPd><ChipId>{5}</ChipId><CVC>{6}</CVC><Box_Id>{7}</Box_Id><Client_NO>{8}</Client_NO><LotId>{9}</LotId><MOId>{10}</MOId><LineId>{11}</LineId><ResId>{12}</ResId><ShiftTypeId>{13}</ShiftTypeId><UserId>{14}</UserId><OPId>{15}</OPId></document>",
                        new object[] { strArray[0], strArray[1], strArray[2], strArray[3], strArray[4], strArray[5], strArray[6], strArray[7], strArray[8], strArray[9], strArray[10], strArray[11], strArray[12], strArray[13], strArray[14], strArray[15] });
                    break;
                case 71:
                    if (strArray.Count() < 1)
                    {
                        bResult = false;
                        break;
                    }
                    commandTemp = String.Format("<document><MAC>{0}</MAC></document>", strArray[0]);
                    break;
                default:
                    bResult = false;
                    break;
            }
            if (!bResult)
            {
                Monitor.Exit(Lock);
                return bResult;
            }
            parameters.Add("InterfaceNo", commandType);
            parameters.Add("Params", commandTemp, "", 1024);//@Params XML,
            parameters.Add("AttachParams", "");//NVARCHAR(MAX)=NULL,
            parameters.Add("OutParams", null, SqlDbType.NVarChar, int.MaxValue, ParameterDirection.Output);
            Result<Parameters, DataSet> result = null;
            try
            {
                result = DB.DBHelper.ExecuteParametersSource("Interf_Call", parameters, ExecuteType.StoredProcedure);
            }
            catch (Exception ex)
            {
                temp = "Exception:" + ex.Message;
                Debug(ex.Message, true);
                Monitor.Exit(Lock);
                return false;
            }
            if (result.HasError)
            {
                temp = "Result Error:" + result.Message;
                Debug(temp, true);
                Monitor.Exit(Lock);
                return false;
            }

            if (GetNodeTxtFromStandXml(result.Value1["OutParams"].ToString(), "ReturnValue") != "1")
            {
                temp = GetNodeTxtFromStandXml(result.Value1["OutParams"].ToString(), "ReturnMessage");
                Debug(temp, false);
                Monitor.Exit(Lock);
                return false;
            }
            else
            {
                temp = GetNodeTxtFromStandXml(result.Value1["OutParams"].ToString(), "ReturnMessage");
                Debug(temp, true);
                csResult = result.Value1["OutParams"].ToString();
                Monitor.Exit(Lock);
                return true;
            }
        }

        private bool MesAbs10()
        {
            bool bResult = false;
            List<string> array = new List<string>();
            string csResult = "";
            // WinAPI.File.INIFileHelper.Read(BARCODE_COMPARE_INFO, BARCODE_VSCID_START, setting_path, "0")
            //string userCode = "test user";// g_globalInfo.GetLogInfo().csUserNum;//ZCONF_GET("OVTMES","MesOVTABSUserCode","").c_str();
            //string userPwd = "test pwd";// ZCONF_GET("OVTMES", "MesOVTABSUserPwd", "").c_str();
            //string resCode = "test code";// ZCONF_GET("OVTMES", "MesOVTABSResCode", "").c_str();
            //string moCode = "test order";// g_globalInfo.GetLogInfo().csOrderNum;//ZCONF_GET("OVTMES","MesOVTABSMOCode","").c_str();
            string userCode = Framework.App.User.UserCode;// g_globalInfo.GetLogInfo().csUserNum;//ZCONF_GET("OVTMES","MesOVTABSUserCode","").c_str();
            string userPwd = WinAPI.File.INIFileHelper.Read(BARCODE_COMPARE_INFO, "MesOVTABSUserPwd", setting_path, "0");
            string resCode = Framework.App.Resource.ResourceCode;
            string moCode = OrderNumber.ToString();// g_globalInfo.GetLogInfo().csOrderNum;//ZCONF_GET("OVTMES","MesOVTABSMOCode","").c_str();
            array.Clear();
            array.Add(userCode);
            array.Add(userPwd);
            array.Add(resCode);

            if (MesAbs(MES_ABS_INTERFACE_10, ref array, ref csResult))
            {
                Debug(csResult, false);
                if (int.Parse(GetNodeTxtFromStandXml(csResult, MES_ABS_ReturnValue)) == 1)
                {
                    mapLoginData.Add(MES_ABS_UserId, GetNodeTxtFromStandXml(csResult, MES_ABS_UserId));
                    mapLoginData.Add(MES_ABS_UserCode, GetNodeTxtFromStandXml(csResult, MES_ABS_UserCode));
                    mapLoginData.Add(MES_ABS_UserName, GetNodeTxtFromStandXml(csResult, MES_ABS_UserName));
                    mapLoginData.Add(MES_ABS_ResId, GetNodeTxtFromStandXml(csResult, MES_ABS_ResId));
                    mapLoginData.Add(MES_ABS_ResCode, GetNodeTxtFromStandXml(csResult, MES_ABS_ResCode));
                    mapLoginData.Add(MES_ABS_ResName, GetNodeTxtFromStandXml(csResult, MES_ABS_ResName));
                    mapLoginData.Add(MES_ABS_LineId, GetNodeTxtFromStandXml(csResult, MES_ABS_LineId));
                    mapLoginData.Add(MES_ABS_LineCode, GetNodeTxtFromStandXml(csResult, MES_ABS_LineCode));
                    mapLoginData.Add(MES_ABS_LineName, GetNodeTxtFromStandXml(csResult, MES_ABS_LineName));
                    mapLoginData.Add(MES_ABS_ShiftTypeId, GetNodeTxtFromStandXml(csResult, MES_ABS_ShiftTypeId));
                    mapLoginData.Add(MES_ABS_ShiftTypeCode, GetNodeTxtFromStandXml(csResult, MES_ABS_ShiftTypeCode));
                    mapLoginData.Add(MES_ABS_ShiftTypeName, GetNodeTxtFromStandXml(csResult, MES_ABS_ShiftTypeName));
                    mapLoginData.Add(MES_ABS_OPId, GetNodeTxtFromStandXml(csResult, MES_ABS_OPId));
                    mapLoginData.Add(MES_ABS_OPCode, GetNodeTxtFromStandXml(csResult, MES_ABS_OPCode));
                    mapLoginData.Add(MES_ABS_OPName, GetNodeTxtFromStandXml(csResult, MES_ABS_OPName));
                    mapLoginData.Add(MES_ABS_ReturnValue, GetNodeTxtFromStandXml(csResult, MES_ABS_ReturnValue));
                    mapLoginData.Add(MES_ABS_ReturnMessage, GetNodeTxtFromStandXml(csResult, MES_ABS_ReturnMessage));

                    //调用13接口，通过工单编号数据获取其它数据
                    array.Clear();
                    array.Add(moCode);
                    if (MesAbs(MES_ABS_INTERFACE_13, ref array, ref csResult))
                    {
                        Debug(csResult, false);
                        if (int.Parse(GetNodeTxtFromStandXml(csResult, MES_ABS_ReturnValue)) == 1)
                        {
                            mapLoginData.Add(MES_ABS_MOId, GetNodeTxtFromStandXml(csResult, MES_ABS_MOId));
                            mapLoginData.Add(MES_ABS_MOCode, GetNodeTxtFromStandXml(csResult, MES_ABS_MOCode));
                            mapLoginData.Add(MES_ABS_OrderCode, GetNodeTxtFromStandXml(csResult, MES_ABS_OrderCode));
                            mapLoginData.Add(MES_ABS_CustomerName, GetNodeTxtFromStandXml(csResult, MES_ABS_CustomerName));
                            mapLoginData.Add(MES_ABS_CustomerItemCode, GetNodeTxtFromStandXml(csResult, MES_ABS_CustomerItemCode));
                            mapLoginData.Add(MES_ABS_CustomerItemName, GetNodeTxtFromStandXml(csResult, MES_ABS_CustomerItemName));
                            mapLoginData.Add(MES_ABS_CustomerItemSpecification, GetNodeTxtFromStandXml(csResult, MES_ABS_CustomerItemSpecification));
                            mapLoginData.Add(MES_ABS_ModelCode, GetNodeTxtFromStandXml(csResult, MES_ABS_ModelCode));
                            mapLoginData.Add(MES_ABS_ItemCode, GetNodeTxtFromStandXml(csResult, MES_ABS_ItemCode));
                            mapLoginData.Add(MES_ABS_ItemName, GetNodeTxtFromStandXml(csResult, MES_ABS_ItemName));
                            mapLoginData.Add(MES_ABS_DeviceType, GetNodeTxtFromStandXml(csResult, MES_ABS_DeviceType));
                            mapLoginData.Add(MES_ABS_Type, GetNodeTxtFromStandXml(csResult, MES_ABS_Type));
                            mapLoginData.Add(MES_ABS_ShiftTypeName, GetNodeTxtFromStandXml(csResult, MES_ABS_ShiftTypeName));
                            mapLoginData.Add(MES_ABS_MOQty, GetNodeTxtFromStandXml(csResult, MES_ABS_MOQty));
                            bResult = true;
                        }
                    }
                    else
                    {
                        Debug("call 13 interface error", false);
                    }
                }
                else
                {

                }
            }
            else
            {
                Debug("call 10 interface error", false);
            }
            return bResult;
        }

        /******************

        通过该接口判断是不是可以进行测试
        是否属于该工单

        *******************/
        private bool MesAbs20(string strSN, ref string lotID)
        {
            List<string> strMesArray = new List<string>();
            string csMesResult = "";
            strMesArray.Clear();
            strMesArray.Add("0");
            strMesArray.Add(MOId.ToString());//工单 mapLoginData[MES_ABS_MOId]
            strMesArray.Add("Mac");//LotSNType
            strMesArray.Add(strSN);
            strMesArray.Add(Framework.App.Resource.LineId.ToString());//mapLoginData[MES_ABS_LineId]
            strMesArray.Add(Framework.App.Resource.ResourceId.ToString());//mapLoginData[MES_ABS_ResId]
            strMesArray.Add(Framework.App.User.UserId.ToString());//mapLoginData[MES_ABS_UserId]
            strMesArray.Add(Framework.App.Resource.StationId.ToString());//mapLoginData[MES_ABS_OPId]
            if (MesAbs(MES_ABS_INTERFACE_20, ref strMesArray, ref csMesResult))
            {
                lotID = GetNodeTxtFromStandXml(csMesResult, MES_ABS_LotId);
                return true;
            }
            else
            {
                return false;
            }
        }

        /**************
        在产品测试完成后调用，完成过站动作

        ***************/
        private bool MesAbs30(string strLotID)
        {
            Monitor.Enter(Lock);
            string csResult = "";
            bool bResult = false;
            List<string> strArray = new List<string>(); ;
            strArray.Clear();
            strArray.Add("0");
            strArray.Add(MOId.ToString());//mapLoginData[MES_ABS_MOId]
            //批次条码ID,从20接口获取
            strArray.Add(strLotID);//LotId
            strArray.Add(Framework.App.Resource.LineId.ToString());//mapLoginData[MES_ABS_LineId]
            strArray.Add(Framework.App.Resource.ResourceId.ToString());//mapLoginData[MES_ABS_ResId]
            strArray.Add(Framework.App.Resource.ShiftTypeId.ToString());//mapLoginData[MES_ABS_ShiftTypeId]
            strArray.Add(Framework.App.User.UserId.ToString());//mapLoginData[MES_ABS_UserId]
            strArray.Add(Framework.App.Resource.StationId.ToString());//mapLoginData[MES_ABS_OPId]

            //测试结果  0：表示测试NG   1：表示测试PASS
            strArray.Add("1");
            strArray.Add("test log test");//TestLog
            strArray.Add("filepath test");//Filepath
            if (MesAbs(MES_ABS_INTERFACE_30, ref strArray, ref csResult))
            {
                //Debug(csResult, false);
                if (int.Parse(GetNodeTxtFromStandXml(csResult, MES_ABS_ReturnValue)) == 1)
                {
                    bResult = true;
                }
            }
            Monitor.Exit(Lock);
            return bResult;
        }

        /***********
        <document>
          <Order_Number></Order_Number>
          <CSN></CSN>  --替代Serial_Number字段
          <PIN></PIN>
          <Smartcard_Number></Smartcard_Number>
          <CPd></CPd>
          <ChipId></ChipId>
          <CVC></CVC>
          <Box_Id></Box_Id>
          <Client_NO></Client_NO>
          <LotId></LotId>
          <MOId></MOId>
          <LineId></LineId>
          <ResId></ResId>
          <ShiftTypeId></ShiftTypeId>
          <UserId></UserId>
          <OPId></OPId>
         </document>
        说明：
        Order_Number
        CSN  --替代Serial_Number字段
        PIN
        Smartcard_Number
        CPd
        ChipId
        CVC
        Box_Id
        Client_NO
        MOId：工单Id（如果IsTransferMO=1，必须传入工单Id；如果IsTransferMO=0，工单Id可为空，非空时校验批次条码的工单）
        LotId：批次条码Id
        LineId：线别Id，从登录的10接口中获取
        ResId：资源Id，从登录的10接口中获取
        ShiftTypeId：班制Id，从登录的10接口中获取
        UserId：用户Id，从登录的10接口中获取
        OPId：工序Id，从登录的10接口中获取
        IsPass：测试结果  0：表示测试NG   1：表示测试PASS
        TestLog：测试简要信息
        FilePath：上传日志地址信息

        该函数主要是上传数据
        ************/
        private bool MesAbs70(string lotID, string vscID, string chipID)
        {
            //CDlgMesScan* lpThis = (CDlgMesScan*)pPar;
            List<string> strMesArray = new List<string>();
            string csMesResult = "";
            strMesArray.Clear();
            strMesArray.Add(OrderNumber/*g_globalInfo.GetLogInfo().csOrderNum*/);//order_number
            strMesArray.Add(vscID);//CSN,用VSCID
            strMesArray.Add("PIN");//PIN
            strMesArray.Add("Smartcard_Number");//Smartcard_Number
            strMesArray.Add("CPd");//CPd
            strMesArray.Add(chipID);//ChipId
            strMesArray.Add("CVC");//CVC
            strMesArray.Add("Box_Id");//Box_Id
            strMesArray.Add("Client_NO");//Client_NO
            strMesArray.Add(lotID);//LotId
            strMesArray.Add(MOId.ToString());//mapLoginData[MES_ABS_MOId]
            strMesArray.Add(Framework.App.Resource.LineId.ToString());//mapLoginData[MES_ABS_LineId]
            strMesArray.Add(Framework.App.Resource.ResourceId.ToString());//mapLoginData[MES_ABS_ResId]
            strMesArray.Add(Framework.App.Resource.ShiftTypeId.ToString());//mapLoginData[MES_ABS_ShiftTypeId]
            strMesArray.Add(Framework.App.User.UserId.ToString());//mapLoginData[MES_ABS_UserId]
            strMesArray.Add(Framework.App.Resource.StationId.ToString());//mapLoginData[MES_ABS_OPId]

            if (MesAbs(MES_ABS_INTERFACE_70, ref strMesArray, ref csMesResult))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /******************

        通过一个数据，获取其它所有的数据

        *******************/
        private bool MesAbs71(string strSn, ref Dictionary<string, string> m_mapGet)
        {
            List<string> strMesArray = new List<string>();
            string csMesResult = "";
            strMesArray.Clear();
            strMesArray.Add(strSn);
            if (MesAbs(MES_ABS_INTERFACE_71, ref strMesArray, ref csMesResult))
            {
                //theApp.AddDebugLog(csMesResult);
                if (int.Parse(GetNodeTxtFromStandXml(csMesResult, MES_ABS_ReturnValue)) == 1)
                {
                    m_mapGet[MES_STAR_OUI] = GetNodeTxtFromStandXml(csMesResult, MES_STAR_OUI);
                    m_mapGet[MES_STAR_SN] = GetNodeTxtFromStandXml(csMesResult, MES_STAR_SN);
                    m_mapGet[MES_STAR_CHIPID] = GetNodeTxtFromStandXml(csMesResult, MES_STAR_CHIPID);
                    m_mapGet[MES_STAR_EXTSN] = GetNodeTxtFromStandXml(csMesResult, MES_STAR_EXTSN);
                    m_mapGet[MES_STAR_HW] = GetNodeTxtFromStandXml(csMesResult, MES_STAR_HW);
                    m_mapGet[MES_STAR_SW] = GetNodeTxtFromStandXml(csMesResult, MES_STAR_SW);
                    m_mapGet[MES_STAR_VSCID] = GetNodeTxtFromStandXml(csMesResult, MES_STAR_VSCID);
                    m_mapGet[MES_STAR_STBTYPE] = GetNodeTxtFromStandXml(csMesResult, MES_STAR_STBTYPE);
                    m_mapGet[MES_STAR_PRODUCTTYPE] = GetNodeTxtFromStandXml(csMesResult, MES_STAR_PRODUCTTYPE);
                    return true;
                }
                else
                {
                    Debug(csMesResult, false);
                    return false;
                }
            }
            else
            {
                return false;
            }
        }


        /*************

        从XML数据中获取节点数据
        节点<不能有空格>的内容间不能有空格

        **************/
        private string GetNodeTxtFromStandXml(string src, string nodeName)
        {
            string formatBegin, formatEnd, strResult;
            int len, indexBegin, indexEnd;
            formatBegin = "<" + nodeName + ">";
            formatEnd = "</" + nodeName + ">";
            indexBegin = src.IndexOf(formatBegin);
            indexEnd = src.IndexOf(formatEnd);
            len = formatBegin.Length;
            //strResult = src.Mid(indexBegin + len, indexEnd - indexBegin - len);
            strResult = src.Substring(indexBegin + len, indexEnd - indexBegin - len);
            strResult.Trim();
            return strResult;
        }

        private void CustomerSelectChange(object sender, SelectionChangedEventArgs e)
        {
            switch (CustomerComBox.SelectedIndex)
            {
                case 0:
                    CheckVSCID.IsEnabled = false;
                    CheckVSCID.IsChecked = false;
                    VscStart.IsEnabled = false;
                    VscEnd.IsEnabled = false;
                    StandType.IsEnabled = false;
                    HdmiType.IsEnabled = false;
                    break;
                case 1:
                    CheckVSCID.IsEnabled = true;
                    VscStart.IsEnabled = true;
                    VscEnd.IsEnabled = true;
                    StandType.IsEnabled = true;
                    HdmiType.IsEnabled = true;
                    break;
            }
            try
            {
                WinAPI.File.INIFileHelper.Write(BARCODE_COMPARE_INFO, BARCODE_CUSTOMER_INDEX, CustomerComBox.SelectedIndex.ToString(), setting_path);
            }
            catch { }
        }

        private void TextLostFocus(object sender, RoutedEventArgs e)
        {
            string strKey = "", strValue = "";
            if (sender.GetHashCode() == VscStart.GetHashCode())
            {
                strKey = BARCODE_VSCID_START;
                strValue = VscStart.Text.Trim();
            }
            else if (sender.GetHashCode() == VscEnd.GetHashCode())
            {
                strKey = BARCODE_VSCID_END;
                strValue = VscEnd.Text.Trim();
            }
            else if (sender.GetHashCode() == CheckVSCID.GetHashCode())
            {
                strKey = BARCODE_CHECK_VSCID;
                strValue = CheckVSCID.IsChecked == true ? "1" : "0";
            }
            else
            {
                return;
            }
            try
            {
                WinAPI.File.INIFileHelper.Write(BARCODE_COMPARE_INFO, strKey, strValue, setting_path);
            }
            catch { }
        }

        private void CheckRadio(object sender, RoutedEventArgs e)
        {
            string strKey = "", strValue = "";
            if (sender.GetHashCode() == StandType.GetHashCode())
            {
                strKey = BARCODE_VIDEO_TYPE;
                strValue = "0";
            }
            else if (sender.GetHashCode() == HdmiType.GetHashCode())
            {
                strKey = BARCODE_VIDEO_TYPE;
                strValue = "1";
            }
            else
            {
                return;
            }
            try
            {
                WinAPI.File.INIFileHelper.Write(BARCODE_COMPARE_INFO, strKey, strValue, setting_path);
            }
            catch { }
        }
    }
}
