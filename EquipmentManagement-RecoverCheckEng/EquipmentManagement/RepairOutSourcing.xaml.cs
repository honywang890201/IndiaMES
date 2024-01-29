using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using Data;
using DB;
using Log;

namespace EquipmentManagement
{
    /// <summary>
    /// RepairOutSourcing.xaml 的交互逻辑
    /// </summary>
    public partial class RepairOutSourcing : Component.Controls.User.UserVendor
    {
        private ViewState _FormState = ViewState.Normal;
        private ViewState FormState
        {
            get
            {
                return _FormState;
            }
            set
            {
                if(value!= _FormState)
                {
                    _FormState = value;
                    FormStateChanged();
                }
            }
        }

        private void FormStateChanged()
        {
            if(FormState==ViewState.Normal)
            {
                if(this.DataContext!=null)
                {
                    DataRowView row = this.DataContext as DataRowView;

                    btnEdit.Visibility = CheckAuthority('2') && row.Row.Field<string>("Status").ToUpper() == "WaitClose".ToUpper() ? Visibility.Visible : Visibility.Collapsed;
                    btnClose.Visibility = CheckAuthority('6') && row.Row.Field<string>("Status").ToUpper() == "WaitClose".ToUpper() ? Visibility.Visible : Visibility.Collapsed;
                }
                else
                {
                    btnEdit.Visibility = Visibility.Collapsed;
                    btnClose.Visibility = Visibility.Collapsed;
                }

                btnCancel.Visibility = Visibility.Collapsed;
                btnSave.Visibility = Visibility.Collapsed;

                txtEquipmentRepairOutSourcingCode.IsEnabled = true;
                btnEquipmentRepairOutSourcing.IsEnabled = true;
                txtEquipmentRepairOutSourcingDesc.IsReadOnly = true;
                colResult.IsEnabled = false;
                colComment.IsEnabled = false;

                colResult.Visible = false;
                colResult.ShowInColumnChooser = false;
                colResultName.Visible = true;
                colResultName.ShowInColumnChooser = true;
            }
            else if(FormState == ViewState.Modify)
            {
                btnEdit.Visibility = Visibility.Collapsed;
                btnClose.Visibility = Visibility.Collapsed;
                btnCancel.Visibility = Visibility.Visible;
                btnSave.Visibility = Visibility.Visible;

                txtEquipmentRepairOutSourcingCode.IsEnabled = false;
                btnEquipmentRepairOutSourcing.IsEnabled = false;
                txtEquipmentRepairOutSourcingDesc.IsReadOnly = false;
                colResult.IsEnabled = true;
                colComment.IsEnabled = true;

                colResult.Visible = true;
                colResult.ShowInColumnChooser = true;
                colResultName.Visible = false;
                colResultName.ShowInColumnChooser = false;
            }
            else
            {
                btnEdit.Visibility = Visibility.Collapsed;
                btnClose.Visibility = Visibility.Collapsed;
                btnCancel.Visibility = Visibility.Collapsed;
                btnSave.Visibility = Visibility.Collapsed;

                txtEquipmentRepairOutSourcingCode.IsEnabled = true;
                btnEquipmentRepairOutSourcing.IsEnabled = true;
                txtEquipmentRepairOutSourcingDesc.IsReadOnly = true;
                colResult.IsEnabled = false;
                colComment.IsEnabled = false;

                colResult.Visible = true;
                colResult.ShowInColumnChooser = true;
                colResultName.Visible = false;
                colResultName.ShowInColumnChooser = false;
            }
        }

        public RepairOutSourcing(Framework.SystemAuthority authority) :
            base(authority)
        {
            InitializeComponent();
            FormStateChanged();
            LoadResult();
        }
        public RepairOutSourcing(List<Framework.SystemAuthority> authoritys, Data.Parameters parameters, string flag) :
            base(authoritys, parameters, flag)
        {
            InitializeComponent();
            FormStateChanged();
            LoadResult();

            this.LoadComplete += new RoutedEventHandler((sender, e) =>
            {
                if (parameters != null)
                {
                    if (parameters.IsExists("EquipmentRepairOutSourcingCode") && parameters["EquipmentRepairOutSourcingCode"] != null && parameters["EquipmentRepairOutSourcingCode"].ToString().Trim() != string.Empty)
                    {
                        if (!LoadEquipmentRepairOutSourcingCode(parameters["EquipmentRepairOutSourcingCode"].ToString().Trim()))
                        {
                            this.Close();
                        }
                    }
                    else
                    {
                        this.Close();
                    }
                }
            });
        }

        private void LoadResult()
        {
            string sql = @"
SELECT Code,Name
FROM dbo.Set_SystemType WITH(NOLOCK) 
WHERE TypeCode='EquipmentRepairOutSourcingResult'
ORDER BY Sequence";
            try
            {
                System.Data.DataTable dt = DB.DBHelper.GetDataTable(sql, null, null, true);
                colResult.ItemsSource = dt.DefaultView;
                colResult.SelectedValuePath = "Code";
                colResult.DisplayMemberPath = "Name";
            }
            catch (Exception e)
            {
                Component.MessageBox.MyMessageBox.ShowError(e.Message);
            }
        }
        private void txtEquipmentRepairOutSourcingCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.DataContext = null;
        }

        private bool LoadEquipmentRepairOutSourcingCode(string code)
        {
            string sql = @"SELECT ROW_NUMBER()OVER(ORDER BY Inp_EquipmentRepairOutSourcing.CreateDateTime ASC) AS RowIndex,
       Inp_EquipmentRepairOutSourcing.EquipmentRepairOutSourcingId,
       Inp_EquipmentRepairOutSourcing.EquipmentRepairInId,
	   Inp_EquipmentRepairOutSourcing.EquipmentRepairOutSourcingCode,
	   Inp_EquipmentRepairIn.EquipmentRepairInCode,
	   Inp_EquipmentRepairOutSourcing.Status,
	   dbo.Ftn_GetStatusName(Inp_EquipmentRepairOutSourcing.Status,'EquipmentRepairOutSourcingStatus') AS StatusName,
	   Inp_EquipmentRepairOutSourcing.EquipmentRepairOutSourcingDesc,
	   SendRepairUser.SysUserCode+'-'+ISNULL(SendRepairUser.SysUserName,'') AS SendRepairUserName,
	   Inp_EquipmentRepairOutSourcing.SendRepairDateTime,
	   CloseUser.SysUserCode+'-'+ISNULL(CloseUser.SysUserName,'') AS CloseUserName,
	   Inp_EquipmentRepairOutSourcing.CloseDateTime,
	   UpdateUser.SysUserCode+'-'+ISNULL(UpdateUser.SysUserName,'') AS UpdateUserName,
	   Inp_EquipmentRepairOutSourcing.UpdateDateTime
FROM dbo.Inp_EquipmentRepairOutSourcing WITH(NOLOCK) 
LEFT JOIN dbo.Inp_EquipmentRepairIn WITH(NOLOCK) ON dbo.Inp_EquipmentRepairIn.EquipmentRepairInId = dbo.Inp_EquipmentRepairOutSourcing.EquipmentRepairInId
LEFT JOIN dbo.Set_User SendRepairUser WITH(NOLOCK) ON SendRepairUser.SysUserId = dbo.Inp_EquipmentRepairOutSourcing.SendRepairUserId
LEFT JOIN dbo.Set_User CloseUser WITH(NOLOCK) ON CloseUser.SysUserId = dbo.Inp_EquipmentRepairOutSourcing.CloseUserId
LEFT JOIN dbo.Set_User UpdateUser WITH(NOLOCK) ON UpdateUser.SysUserId = dbo.Inp_EquipmentRepairOutSourcing.UpdateUserId";
            Data.Parameters parameters = new Parameters()
                .Add("EquipmentRepairOutSourcingCode", code);
            try
            {
                DataTable dt = DB.DBHelper.GetDataTable(sql, parameters, null, true);
                if (dt.Rows.Count < 1)
                {
                    Component.MessageBox.MyMessageBox.ShowError(string.Format("委外维修单[{0}]错误。", code));
                    return false;
                }
                else
                {
                    txtEquipmentRepairOutSourcingCode.Text = dt.Rows[0]["EquipmentRepairOutSourcingCode"].ToString().Trim();
                    this.DataContext = dt.DefaultView[0];
                    txtEquipmentRepairOutSourcingDesc.Text = dt.Rows[0]["EquipmentRepairOutSourcingDesc"].ToString().Trim();
                    FormStateChanged();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Component.MessageBox.MyMessageBox.ShowError(ex.Message);
                return false;
            }

        }

        private void txtEquipmentRepairOutSourcingCode_LostFocus(object sender, RoutedEventArgs e)
        {
            if (this.DataContext == null)
            {
                if (txtEquipmentRepairOutSourcingCode.Text.Trim() != string.Empty)
                {
                    if(!LoadEquipmentRepairOutSourcingCode(txtEquipmentRepairOutSourcingCode.Text.Trim()))
                    {
                        txtEquipmentRepairOutSourcingCode.Text = string.Empty;
                        FormStateChanged();
                    }
                }
            }
        }

        private void btnEquipmentRepairOutSourcing_Click(object sender, RoutedEventArgs e)
        {
            Selector.SelectorRepairOutSourcing win = new Selector.SelectorRepairOutSourcing(null);
            if (win.ShowDialog())
            {
                txtEquipmentRepairOutSourcingCode.Text = win.SelectRow["EquipmentRepairOutSourcingCode"].ToString().Trim();
                this.DataContext = win.SelectRow;
                txtEquipmentRepairOutSourcingDesc.Text = win.SelectRow["EquipmentRepairOutSourcingDesc"].ToString().Trim();
                FormStateChanged();
            }
        }

        private void UserVendor_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            matrix1.ItemsSource = null;
            txtEquipmentRepairOutSourcingDesc.Text = string.Empty;
            FormStateChanged();
            if (DataContext == null)
            {
                return;
            }
            else
            {
                DataRowView row = DataContext as DataRowView;

                string sql = @"SELECT Inp_EquipmentRepairOutSourcingDetail.EquipmentRepairOutSourcingDetailId,
	   Bas_Equipment.EquipmentId,
	   Bas_EquipmentGroup.EquipmentGroupCode+'-'+ISNULL(dbo.Bas_EquipmentGroup.EquipmentGroupDesc,'') AS EquipmentGroupName,
	   Bas_EquipmentModel.EquipmentModelCode+'-'+ISNULL(Bas_EquipmentModel.EquipmentModelDesc,'') AS EquipmentModelName,
	   Bas_Equipment.EquipmentCode,
	   Bas_Equipment.EquipmentDesc,
	   Inp_EquipmentRepairOutSourcingDetail.Status,
	   dbo.Ftn_GetStatusName(Inp_EquipmentRepairOutSourcingDetail.Status,'EquipmentRepairOutSourcingStatus') AS StatusName,
	   Bas_WorkShop.WorkShopCode+'-'+ISNULL(Bas_WorkShop.WorkShopDesc,'') AS WorkShopName,
	   Bas_Factory.FactoryCode+'-'+ISNULL(Bas_Factory.FactoryName,'') AS FactoryName,
	   Bas_Company.CompanyCode+'-'+ISNULL(Bas_Company.CompanyDesc,'') AS CompanyName,
	   Inp_EquipmentRepairOutSourcingDetail.Comment,
	   Inp_EquipmentRepairOutSourcingDetail.Result,
	   dbo.Ftn_GetStatusName(Inp_EquipmentRepairOutSourcingDetail.Result,'EquipmentRepairOutSourcingResult') AS ResultName
FROM dbo.Inp_EquipmentRepairOutSourcingDetail WITH(NOLOCK) 
LEFT JOIN dbo.Bas_Equipment WITH(NOLOCK) ON dbo.Bas_Equipment.EquipmentId = dbo.Inp_EquipmentRepairOutSourcingDetail.EquipmentId 
LEFT JOIN dbo.Bas_EquipmentModel WITH(NOLOCK) ON dbo.Bas_EquipmentModel.EquipmentModelId = dbo.Bas_Equipment.EquipmentModelId
LEFT JOIN dbo.Bas_EquipmentGroup WITH(NOLOCK) ON dbo.Bas_EquipmentGroup.EquipmentGroupId = dbo.Bas_Equipment.EquipmentGroupId
LEFT JOIN dbo.Bas_WorkShop WITH(NOLOCK) ON dbo.Bas_WorkShop.WorkShopId = dbo.Bas_Equipment.WorkShopId
LEFT JOIN dbo.Bas_Factory WITH(NOLOCK) ON dbo.Bas_Factory.FactoryId = dbo.Bas_Equipment.FactoryId
LEFT JOIN dbo.Bas_Company WITH(NOLOCK) ON dbo.Bas_Company.CompanyId = dbo.Bas_Equipment.CompanyId
WHERE Inp_EquipmentRepairOutSourcingDetail.EquipmentRepairOutSourcingId=@EquipmentRepairOutSourcingId
ORDER BY Bas_Equipment.EquipmentCode
";
                Data.Parameters parameters = new Parameters()
                        .Add("EquipmentRepairOutSourcingId", row["EquipmentRepairOutSourcingId"]);
                Component.MaskBusy.Busy(grid, "正在加载明细。。。");
                new TaskFactory<Data.Result<System.Data.DataTable>>().StartNew(() =>
                {
                    Data.Result<System.Data.DataTable> result = new Data.Result<System.Data.DataTable>();
                    try
                    {
                        System.Data.DataTable r = DB.DBHelper.GetDataTable(sql, parameters, null, true);

                        result.HasError = false;
                        result.Value = r;
                        return result;
                    }
                    catch (Exception ex)
                    {
                        result.HasError = true;
                        result.Message = ex.Message;
                        return result;
                    }
                }).ContinueWith(result =>
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        if (result.Result.HasError)
                        {
                            Component.MessageBox.MyMessageBox.ShowError(result.Result.Message);
                        }
                        else
                        {
                            matrix1.ItemsSource = result.Result.Value.DefaultView;
                            matrix1.BestFitColumns();
                        }
                    }));
                    Component.MaskBusy.Hide(grid);
                });
            }

        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext == null)
                return;
            if (!CheckAuthority('2'))
                return;

            FormState = ViewState.Modify;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            string code = txtEquipmentRepairOutSourcingCode.Text;
            LoadEquipmentRepairOutSourcingCode(code);

            FormState = ViewState.Normal; 
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext == null)
                return;

            StringBuilder xml = new StringBuilder();
            xml.Append(string.Format("<xml>"));
            foreach (DataRowView row in (matrix1.ItemsSource as DataView))
            {
                xml.Append(string.Format("<row EquipmentRepairOutSourcingDetailId=\"{0}\" Comment=\"{1}\" Result=\"{2}\"/>",
                    row["EquipmentRepairOutSourcingDetailId"],
                    WinAPI.File.XMLHelper.StringFormat(row["Comment"].ToString()),
                    row["Result"]));
            }
            xml.Append(string.Format("</xml>"));

            if (Component.MessageBox.MyMessageBox.ShowQuestion(string.Format("确定保存吗？")) != MessageBoxResult.OK)
                return;


            string sql = "Prd_Equipment_RepairOutSourcing_Save";
            Data.Parameters parameters = new Parameters()
                .Add("EquipmentRepairOutSourcingId", (this.DataContext as DataRowView)["EquipmentRepairOutSourcingId"])
                .Add("EquipmentRepairOutSourcingDesc", txtEquipmentRepairOutSourcingDesc.Text, SqlDbType.NVarChar, int.MaxValue)
                .Add("Xml", xml.ToString(), SqlDbType.Xml, int.MaxValue)
                .Add("UserId", Framework.App.User.UserId)
                .Add("PluginId", PluginId)
                .Add("Comment", DBNull.Value)
                .Add("Return_Message", DBNull.Value, SqlDbType.NVarChar, int.MaxValue, ParameterDirection.Output)
                .Add("Return_Value", 0, SqlDbType.Int, ParameterDirection.ReturnValue);

            try
            {
                parameters = DB.DBHelper.ExecuteParameters(sql, parameters, ExecuteType.StoredProcedure, null, true);
            }
            catch (Exception ex)
            {
                Component.MessageBox.MyMessageBox.ShowError(ex.Message);
                return;
            }

            if ((int)parameters["Return_Value"] == 1)
            {
                Component.MessageBox.MyMessageBox.Show(parameters["Return_Message"].ToString());
                string code = txtEquipmentRepairOutSourcingCode.Text;
                txtEquipmentRepairOutSourcingCode.Text = string.Empty;
                LoadEquipmentRepairOutSourcingCode(code);
                FormState = ViewState.Normal;
                return;
            }
            else
            {
                Component.MessageBox.MyMessageBox.ShowError(parameters["Return_Message"].ToString());
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext == null)
                return;

            if (!CheckAuthority('6'))
                return;

            if (Component.MessageBox.MyMessageBox.ShowQuestion(string.Format("确定关闭吗？")) != MessageBoxResult.OK)
                return;


            string sql = "Prd_Equipment_RepairOutSourcing_Close";
            Data.Parameters parameters = new Parameters()
                .Add("EquipmentRepairOutSourcingId", (this.DataContext as DataRowView)["EquipmentRepairOutSourcingId"])
                .Add("UserId", Framework.App.User.UserId)
                .Add("PluginId", PluginId)
                .Add("Comment", DBNull.Value)
                .Add("Return_Message", DBNull.Value, SqlDbType.NVarChar, int.MaxValue, ParameterDirection.Output)
                .Add("Return_Value", 0, SqlDbType.Int, ParameterDirection.ReturnValue);

            try
            {
                parameters = DB.DBHelper.ExecuteParameters(sql, parameters, ExecuteType.StoredProcedure, null, true);
            }
            catch (Exception ex)
            {
                Component.MessageBox.MyMessageBox.ShowError(ex.Message);
                return;
            }

            if ((int)parameters["Return_Value"] == 1)
            {
                Component.MessageBox.MyMessageBox.Show(parameters["Return_Message"].ToString());
                string code = txtEquipmentRepairOutSourcingCode.Text;
                txtEquipmentRepairOutSourcingCode.Text = string.Empty;
                LoadEquipmentRepairOutSourcingCode(code);
                FormStateChanged();
                return;
            }
            else
            {
                Component.MessageBox.MyMessageBox.ShowError(parameters["Return_Message"].ToString());
            }
        }
    }
}
