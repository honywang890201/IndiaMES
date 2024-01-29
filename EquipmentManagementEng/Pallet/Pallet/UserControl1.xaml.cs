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

namespace Pallet
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class UserControl1 : Component.Controls.User.UserVendor
    {
        private string setting_path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Framework.SysVar.SETTING_FILE_NAME);
        private DataRowView SelectRow = null;

        private bool _IsLoad = false;

        private int PalletQty = 0;


        private DataTable ScanSource = new DataTable();

        public UserControl1(Framework.SystemAuthority authority) :
            base(authority)
        {
            InitializeComponent();

            grid.LoadingRow += new EventHandler<DataGridRowEventArgs>((sender, e) =>
            {
                e.Row.Header = e.Row.GetIndex() + 1;
            });


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

            try
            {
                cmbType.SelectedValue = WinAPI.File.INIFileHelper.Read("ScanType", string.Format("P{0}", MenuId), setting_path, string.Empty);
                cmbType_SelectionChanged(cmbType, null);
            }
            catch
            {

            }
        }

        private void cmbType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbType.SelectedValue != null && cmbType.SelectedItem != null)
            {
                WinAPI.File.INIFileHelper.Write("ScanType", string.Format("P{0}", MenuId), cmbType.SelectedValue.ToString(), setting_path);
                lblScan.Text = ((KeyValuePair<string, string>)cmbType.SelectedItem).Value + "：";
            }
        }



        private void LoadMO()
        {
            PalletQty = 0;

            if (SelectRow == null)
            {
                tbMOCode.Text = string.Empty;
                tbQty.Text = string.Empty;
                tbItemCode.Text = string.Empty;
                tbItemSpec.Text = string.Empty;
                tbCustomer.Text = string.Empty;
                tbPalletQty.Text = string.Empty;
            }
            else
            {
                int.TryParse(SelectRow["PalletQty"].ToString(), out PalletQty);
                tbMOCode.Text = SelectRow["MOCode"].ToString();
                tbQty.Text = SelectRow["Qty"].ToString();
                tbItemCode.Text = SelectRow["ItemCode"].ToString();
                tbItemSpec.Text = SelectRow["ItemSpecification"].ToString();
                tbPalletQty.Text = PalletQty.ToString();
                tbCustomer.Text = SelectRow["CustomerCode"].ToString();
                if (!string.IsNullOrEmpty(SelectRow["CustomerDesc"].ToString().Trim()))
                    tbCustomer.Text = tbCustomer.Text + "/" + SelectRow["CustomerDesc"].ToString();
                if (PalletQty < 1)
                {
                    txtMessage.AddMessage(string.Format("料号[{0}]未设置每栈板箱数！", SelectRow["ItemCode"]), true);
                }
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

        private bool IsCompletePallet = false;
        public void Submit()
        {
            if (string.IsNullOrEmpty(tbMOCode.Text.Trim()))
            {
                AddMessage("请先选择工单！", true);
                btnSelectorMO.Focus();
                return;
            }
            if(string.IsNullOrEmpty(txtPalletSN.Text.Trim()))
            {
                AddMessage("请扫描栈板条码！", true);
                txtPalletSN.Text = string.Empty;
                txtPalletSN.SetFoucs();
                return;
            }

            if (IsCompletePallet)
            {
                MessageBox.Show(string.Format("请先提交栈板[{0}]", txtPalletSN.Text.Trim()), "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                btnSubmit.Focus();
                return;
            }

            if (txtScan.Text.Trim() == string.Empty)
            {
                AddMessage(string.Format("请扫描{0}！", lblScan.Text), true);
                txtScan.Text = string.Empty;
                txtScan.SetFoucs();
                SetErrorOrSuccImage(true, false);
                return;
            }

            foreach (DataRowView row in grid.Items)
            {
                if (row["ScanCode"].ToString().Trim().ToUpper() == txtScan.Text.Trim().ToUpper())
                {
                    AddMessage(string.Format("{0}[{1}]已扫描过，请不要重复扫描！", ((KeyValuePair<string, string>)cmbType.SelectedItem).Value, txtScan.Text), true);
                    txtScan.Text = string.Empty;
                    txtScan.SetFoucs();
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
            parameters.Add("UserId", Framework.App.User.UserId);
            parameters.Add("PluginId", PluginId);
            parameters.Add("OutBoxId", DBNull.Value, SqlDbType.BigInt, ParameterDirection.Output);
            parameters.Add("OutBoxSN", DBNull.Value, SqlDbType.NVarChar, 50, ParameterDirection.Output);
            parameters.Add("OutIsCompletePallet", DBNull.Value, SqlDbType.Bit, ParameterDirection.Output);
            parameters.Add("Return_Message", null, SqlDbType.NVarChar, int.MaxValue, ParameterDirection.Output);
            parameters.Add("Return_Value", null, SqlDbType.Int, ParameterDirection.ReturnValue);


            Result<Parameters, DataSet> result = null;
            try
            {
                result = DB.DBHelper.ExecuteParametersSource("Prd_Inp_Pack_Pallet", parameters, ExecuteType.StoredProcedure);
            }
            catch (Exception ex)
            {
                AddMessage(ex.Message, true);
                txtScan.Text = string.Empty;
                txtScan.SetFoucs();
                return;
            }

            if (result.HasError)
            {
                AddMessage(result.Message, true);
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

                if(IsCompletePallet)
                {
                    //btnSubmit_Click(btnSubmit, null);
                    //MessageBox.Show(string.Format("栈板[{0}]已扫描完成，请点[提交]按钮提交", txtPalletSN.Text.Trim()), "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                   // btnSubmit.Focus();
                    btnSubmit_Click(null,null);
                    txtScan.SetFoucs();
                    return;
                }
            }
        }

        public void AddMessage(string message, bool isError)
        {
            txtMessage.AddMessage(message, isError);

            SetErrorOrSuccImage(isError, false);
        }

        private void txtPalletSN_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (string.IsNullOrEmpty(tbMOCode.Text.Trim()))
                {
                    AddMessage("请先选择工单！", true);
                    btnSelectorMO.Focus();
                    return;
                }

                string PalletSN = txtPalletSN.Text.Trim();
                if(string.IsNullOrEmpty(PalletSN))
                {
                    AddMessage("请输入栈板条码！", true);
                    txtPalletSN.Text = string.Empty;
                    txtPalletSN.SetFoucs();
                    return;
                }

                string sql = @"SELECT Bas_MO_PalletSN.MOId,
		   Bas_MO.MOCode,
		   Inp_Pallet.PalletId,
		   Inp_Pallet.[Status],
		   dbo.Ftn_GetStatusName(Inp_Pallet.[Status],'CheckStatus') AS StatusName
	FROM dbo.Bas_MO_PalletSN WITH(NOLOCK) 
	LEFT JOIN dbo.Bas_MO WITH(NOLOCK) ON dbo.Bas_MO.MOId = dbo.Bas_MO_PalletSN.MOId
	LEFT JOIN dbo.Inp_Pallet WITH(NOLOCK) ON dbo.Inp_Pallet.PalletSN = dbo.Bas_MO_PalletSN.PalletSN
	WHERE dbo.Bas_MO_PalletSN.PalletSN=@PalletSN";
                Parameters parameters = new Parameters();
                parameters.Add("PalletSN", PalletSN);

                DataTable dt = null;
                try
                {
                    dt = DB.DBHelper.GetDataTable(sql, parameters);
                }
                catch (Exception ex)
                {
                    AddMessage(ex.Message, true);
                    txtPalletSN.Text = string.Empty;
                    txtPalletSN.SetFoucs();
                    return;
                }

                if (dt == null || dt.Rows.Count < 1)
                {
                    AddMessage(string.Format("栈板条码[{0}]错误！", PalletSN), true);
                    txtPalletSN.Text = string.Empty;
                    txtPalletSN.SetFoucs();
                    return;
                }
                if (dt.Rows[0]["MOId"].ToString().Trim() != SelectRow["MOId"].ToString().Trim())
                {
                    AddMessage(string.Format("栈板[{0}]对应的工单为[{1}]，请确认！", PalletSN, dt.Rows[0]["MOCode"]), true);
                    txtPalletSN.Text = string.Empty;
                    txtPalletSN.SetFoucs();
                    return;
                }
                if (dt.Rows[0]["PalletId"].ToString().Trim() != string.Empty)
                {
                    if (dt.Rows[0]["Status"].ToString().Trim().ToUpper() != "Close".ToUpper())
                    {
                        AddMessage(string.Format("栈板[{0}]状态为[{1}]，不能再次提交！", PalletSN, dt.Rows[0]["StatusName"]), true);
                        txtPalletSN.Text = string.Empty;
                        txtPalletSN.SetFoucs();
                        return;
                    }
                }

                AddMessage(string.Format("栈板[{0}]正确，请扫描箱号！", PalletSN), false);
                txtScan.Text = string.Empty;
                txtScan.SetFoucs();
                return;
            }
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
            IsCompletePallet = false;
            txtPalletSN.Text = string.Empty;
            txtScan.Text = string.Empty;
            AddMessage("清空扫描成功！", false);
            txtPalletSN.SetFoucs();
        }

         void BSubmit()
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
            parameters.Add("ScanedBoxs", WinAPI.File.XMLHelper.Convert(ScanSource), SqlDbType.Xml, int.MaxValue);
            parameters.Add("MOId", SelectRow["MOId"]);
            parameters.Add("ItemId", SelectRow["ItemId"]);
            parameters.Add("UserId", Framework.App.User.UserId);
            parameters.Add("PluginId", PluginId);
            parameters.Add("Return_Message", null, SqlDbType.NVarChar, int.MaxValue, ParameterDirection.Output);
            parameters.Add("Return_Value", null, SqlDbType.Int, ParameterDirection.ReturnValue);

            Parameters result = null;
            try
            {
                result = DB.DBHelper.ExecuteParameters("Prd_Inp_Pack_Pallet_Submit", parameters, ExecuteType.StoredProcedure);
            }
            catch (Exception ex)
            {
                AddMessage(ex.Message, true);
                txtScan.Text = string.Empty;
                txtScan.SetFoucs();
                return;
            }

            if ((int)result["Return_Value"] != 1)
            {
                AddMessage(result["Return_Message"].ToString(), true);
                txtScan.Text = string.Empty;
                txtScan.SetFoucs();
                return;
            }
            else
            {
                AddMessage(result["Return_Message"].ToString(), false);

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
            //if (login.ShowDialog().Value)
            //{
            //    BSubmit();
            //}
            
        }
    }
}
