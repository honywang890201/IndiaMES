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
    /// Scrap.xaml 的交互逻辑
    /// </summary>
    public partial class Scrap : Component.Controls.User.UserVendor
    {
        public Scrap(Framework.SystemAuthority authority) :
            base(authority)
        {
            InitializeComponent();
        }
        public Scrap(List<Framework.SystemAuthority> authoritys, Data.Parameters parameters, string flag) :
            base(authoritys, parameters, flag)
        {
            InitializeComponent();

            this.LoadComplete += new RoutedEventHandler((sender, e) =>
            {
                if (parameters != null)
                {
                    if (parameters.IsExists("EquipmentScrapCode") && parameters["EquipmentScrapCode"] != null && parameters["EquipmentScrapCode"].ToString().Trim() != string.Empty)
                    {
                        if (!LoadEquipmentScrapCode(parameters["EquipmentScrapCode"].ToString().Trim()))
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

        private void txtEquipmentScrapCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.DataContext = null;
        }

        private bool LoadEquipmentScrapCode(string code)
        {
            string sql = @"SELECT ROW_NUMBER()OVER(ORDER BY Bas_EquipmentScrap.CreateDateTime ASC) AS RowIndex,
       Bas_EquipmentScrap.EquipmentScrapId,
	   Bas_EquipmentScrap.EquipmentScrapCode,
	   Bas_EquipmentScrap.Status,
	   dbo.Ftn_GetStatusName(Bas_EquipmentScrap.Status,'EquipmentScrapStatus') AS StatusName,
	   Bas_EquipmentScrap.Type,
	   dbo.Ftn_GetStatusName(Bas_EquipmentScrap.Type,'EquipmentScrapType') AS TypeName,
	   Bas_EquipmentScrap.EquipmentScrapDesc,
	   ScrapUser.SysUserCode+'-'+ISNULL(ScrapUser.SysUserName,'') AS ScrapUserName,
	   Bas_EquipmentScrap.ScrapDateTime,
	   SureUser.SysUserCode+'-'+ISNULL(SureUser.SysUserName,'') AS SureUserName,
	   Bas_EquipmentScrap.SureDateTime,
	   Bas_EquipmentScrap.SureComment,
	   CASE ISNULL(Bas_EquipmentScrap.Type,'') WHEN 'RepairScrap'  THEN Inp_EquipmentRepairIn.EquipmentRepairInCode
											   WHEN 'RepairOutSourcingScrap' THEN Inp_EquipmentRepairOutSourcing.EquipmentRepairOutSourcingCode
											   ELSE NULL END AS SourceCode
FROM dbo.Bas_EquipmentScrap WITH(NOLOCK) 
LEFT JOIN dbo.Inp_EquipmentRepairIn WITH(NOLOCK) ON Bas_EquipmentScrap.Type='RepairScrap' AND Bas_EquipmentScrap.SoureId=Inp_EquipmentRepairIn.EquipmentRepairInId
LEFT JOIN dbo.Inp_EquipmentRepairOutSourcing WITH(NOLOCK) ON Bas_EquipmentScrap.Type='RepairOutSourcingScrap' AND Bas_EquipmentScrap.SoureId=Inp_EquipmentRepairOutSourcing.EquipmentRepairOutSourcingId
LEFT JOIN dbo.Set_User ScrapUser WITH(NOLOCK) ON ScrapUser.SysUserId = dbo.Bas_EquipmentScrap.ScrapUserId
LEFT JOIN dbo.Set_User SureUser WITH(NOLOCK) ON SureUser.SysUserId = dbo.Bas_EquipmentScrap.SureUserId
WHERE Bas_EquipmentScrap.EquipmentScrapCode=@EquipmentScrapCode";
            Data.Parameters parameters = new Parameters()
                .Add("EquipmentScrapCode", code);
            try
            {
                DataTable dt = DB.DBHelper.GetDataTable(sql, parameters, null, true);
                if (dt.Rows.Count < 1)
                {
                    Component.MessageBox.MyMessageBox.ShowError(string.Format("报废单[{0}]错误。", code));
                    return false;
                }
                else
                {
                    txtEquipmentScrapCode.Text = dt.Rows[0]["EquipmentScrapCode"].ToString().Trim();
                    this.DataContext = dt.DefaultView[0];
                    return true;
                }
            }
            catch (Exception ex)
            {
                Component.MessageBox.MyMessageBox.ShowError(ex.Message);
                return false;
            }

        }

        private void txtEquipmentScrapCode_LostFocus(object sender, RoutedEventArgs e)
        {
            if (this.DataContext == null)
            {
                if (txtEquipmentScrapCode.Text.Trim() != string.Empty)
                {
                    if(!LoadEquipmentScrapCode(txtEquipmentScrapCode.Text.Trim()))
                    {
                        txtEquipmentScrapCode.Text = string.Empty;
                    }
                }
            }
        }

        private void btnEquipmentScrap_Click(object sender, RoutedEventArgs e)
        {
            Selector.SelectorScrap win = new Selector.SelectorScrap(null);
            if (win.ShowDialog())
            {
                txtEquipmentScrapCode.Text = win.SelectRow["EquipmentScrapCode"].ToString().Trim();
                this.DataContext = win.SelectRow;
            }
        }

        private void UserVendor_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            matrix1.ItemsSource = null;
            btnReject.Visibility = Visibility.Collapsed;
            btnAgree.Visibility = Visibility.Collapsed;
            if (DataContext == null)
            {
                return;
            }
            else
            {
                DataRowView row = DataContext as DataRowView;
                if (row.Row.Field<string>("Status").ToUpper() == "WaitScrapSure".ToUpper())
                {
                    if (CheckAuthority('6'))
                    {
                        btnReject.Visibility = Visibility.Visible;
                        btnAgree.Visibility = Visibility.Visible;
                    }
                }

                string sql = @"SELECT Bas_EquipmentScrapDetail.EquipmentScrapDetailId,
	   Bas_Equipment.EquipmentId,
	   Bas_EquipmentGroup.EquipmentGroupCode+'-'+ISNULL(dbo.Bas_EquipmentGroup.EquipmentGroupDesc,'') AS EquipmentGroupName,
	   Bas_EquipmentModel.EquipmentModelCode+'-'+ISNULL(Bas_EquipmentModel.EquipmentModelDesc,'') AS EquipmentModelName,
	   Bas_Equipment.EquipmentCode,
	   Bas_Equipment.EquipmentDesc,
	   Bas_Equipment.Status,
	   dbo.Ftn_GetStatusName(Bas_Equipment.Status,'EquipmentStatus') AS StatusName,
	   Bas_WorkShop.WorkShopCode+'-'+ISNULL(Bas_WorkShop.WorkShopDesc,'') AS WorkShopName,
	   Bas_Factory.FactoryCode+'-'+ISNULL(Bas_Factory.FactoryName,'') AS FactoryName,
	   Bas_Company.CompanyCode+'-'+ISNULL(Bas_Company.CompanyDesc,'') AS CompanyName,
	   Bas_EquipmentScrapDetail.Comment
FROM dbo.Bas_EquipmentScrapDetail WITH(NOLOCK) 
LEFT JOIN dbo.Bas_Equipment WITH(NOLOCK) ON dbo.Bas_Equipment.EquipmentId = dbo.Bas_EquipmentScrapDetail.EquipmentId 
LEFT JOIN dbo.Bas_EquipmentModel WITH(NOLOCK) ON dbo.Bas_EquipmentModel.EquipmentModelId = dbo.Bas_Equipment.EquipmentModelId
LEFT JOIN dbo.Bas_EquipmentGroup WITH(NOLOCK) ON dbo.Bas_EquipmentGroup.EquipmentGroupId = dbo.Bas_Equipment.EquipmentGroupId
LEFT JOIN dbo.Bas_WorkShop WITH(NOLOCK) ON dbo.Bas_WorkShop.WorkShopId = dbo.Bas_Equipment.WorkShopId
LEFT JOIN dbo.Bas_Factory WITH(NOLOCK) ON dbo.Bas_Factory.FactoryId = dbo.Bas_Equipment.FactoryId
LEFT JOIN dbo.Bas_Company WITH(NOLOCK) ON dbo.Bas_Company.CompanyId = dbo.Bas_Equipment.CompanyId
WHERE Bas_EquipmentScrapDetail.EquipmentScrapId=@EquipmentScrapId
ORDER BY Bas_Equipment.EquipmentCode
";
                Data.Parameters parameters = new Parameters()
                        .Add("EquipmentScrapId", row["EquipmentScrapId"]);
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

        private void Sure(bool isReject)
        {
            if (this.DataContext == null)
                return;
            if (!CheckAuthority('6'))
            {
                return;
            }

            Edit.Scrap_EditSureComment win = new Edit.Scrap_EditSureComment(isReject);
            if (!win.ShowDialog())
                return;

            string sql = "Prd_Equipment_Scrap_Sure";
            Data.Parameters parameters = new Parameters()
                .Add("EquipmentScrapId", (this.DataContext as DataRowView)["EquipmentScrapId"])
                .Add("IsReject", isReject)
                .Add("SureComment", win.SureComment, SqlDbType.NVarChar, int.MaxValue)
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
                string code = txtEquipmentScrapCode.Text.Trim();
                txtEquipmentScrapCode.Text = string.Empty;
                LoadEquipmentScrapCode(code);
                return;
            }
            else
            {
                Component.MessageBox.MyMessageBox.ShowError(parameters["Return_Message"].ToString());
            }
        }

        private void btnReject_Click(object sender, RoutedEventArgs e)
        {
            Sure(true);
        }

        private void btnAgree_Click(object sender, RoutedEventArgs e)
        {
            Sure(false);
        }
    }
}
