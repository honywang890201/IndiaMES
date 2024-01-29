using Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PackPrint
{
    /// <summary>
    /// AddPrint.xaml 的交互逻辑
    /// </summary>
    public partial class AddPrint : Window
    {
        private UserControl1 uc = null;
        private long moId = 0;
        private long userId = 0;
        public AddPrint(string scanType, UserControl1 uc, long moId, long userId)
        {
            InitializeComponent();
            this.uc = uc;
            this.moId = moId;
            this.userId = userId;

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
                cmbType.SelectedValue = scanType;
                cmbType_SelectionChanged(cmbType, null);
            }
            catch
            {
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (cmbType.SelectedValue == null)
            {
                cmbType.Focus();
            }
            else
            {
                txtScan.SetFoucs();
            }
            txtDescript.Text = "Scan";
        }

        private void cmbType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbType.SelectedValue != null && cmbType.SelectedItem != null)
            {
                lblScan.Text = ((KeyValuePair<string, string>)cmbType.SelectedItem).Value + "：";
            }
        }
        private void txtScan_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (cmbType.SelectedValue == null)
                {
                    uc.AddMessage("请选择扫描类型！", true);
                    cmbType.Focus();
                    return;
                }
                //此处为了兼容华电打印机身及彩盒时要扫描二维码。二维码同时带有mac及sn，格式为mac_sn,因为这个sn不在系统内，所以在扫描到之后要丢弃
                if (txtScan.Text.IndexOf("_") > -1)
                {
                    txtScan.Text = txtScan.Text.Substring(0, txtScan.Text.IndexOf("_"));
                }
                if (txtScan.Text.Trim() == string.Empty)
                {
                    uc.AddMessage(string.Format("请扫描{0}！", lblScan.Text), true);
                    txtScan.Text = string.Empty;
                    txtScan.SetFoucs();
                    return;
                }
                if (txtDescript.Text.Trim() == string.Empty)
                {
                    uc.AddMessage(string.Format("请输入{0}！", lblDescript.Text), true);
                    txtDescript.Text = string.Empty;
                    txtDescript.SetFoucs();
                    return;
                }
                Parameters parameters = new Parameters();
                parameters.Add("ScanType", cmbType.SelectedValue.ToString());
                parameters.Add("ScanTypeDesc", cmbType.Text);
                parameters.Add("ScanCode", txtScan.Text.Trim());
                parameters.Add("MOId", moId);
                parameters.Add("LineId", Framework.App.Resource.LineId);
                parameters.Add("ResId", Framework.App.Resource.ResourceId);
                parameters.Add("ShiftTypeId", Framework.App.Resource.ShiftTypeId);
                parameters.Add("UserId", userId);
                parameters.Add("OPId", Framework.App.Resource.StationId);
                parameters.Add("PluginId", uc.PluginId);
                parameters.Add("Descript", txtDescript.Text.Trim());
                parameters.Add("Return_Message", null, SqlDbType.NVarChar, int.MaxValue, ParameterDirection.Output);
                parameters.Add("Return_Value", null, SqlDbType.Int, ParameterDirection.ReturnValue);
                Result<Parameters, DataSet> result = null;
                try
                {
                    result = DB.DBHelper.ExecuteParametersSource("Prd_Inp_Pack_ScanPrint_AddPrint", parameters, ExecuteType.StoredProcedure);
                }
                catch (Exception ex)
                {
                    uc.AddMessage(ex.Message, true);
                    txtScan.Text = string.Empty;
                    txtScan.SetFoucs();
                    return;
                }
                if (result.HasError)
                {
                    uc.AddMessage(result.Message, true);
                    txtScan.Text = string.Empty;
                    txtScan.SetFoucs();
                    return;
                }
                if ((int)result.Value1["Return_Value"] != 1)
                {
                    uc.AddMessage(result.Value1["Return_Message"].ToString(), true);
                    txtScan.Text = string.Empty;
                    txtScan.SetFoucs();
                    return;
                }
                else
                {
                    uc.AddMessage(result.Value1["Return_Message"].ToString(), false);
                    txtScan.Text = string.Empty;
                    txtScan.SetFoucs();
                }
                uc.Print(result.Value2);
            }
        }
    }
}
