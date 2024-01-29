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
    /// EquipmentMaintenance.xaml 的交互逻辑
    /// </summary>
    public partial class EquipmentMaintenance : Component.Controls.User.UserVendor
    {
        public EquipmentMaintenance(Framework.SystemAuthority authority) :
            base(authority)
        {
            InitializeComponent();
            LoadRepairType();
            LoadEquipmentMaintenanceResult();
        }

        private void LoadRepairType()
        {
            string sql = @"SELECT RepairTypeId,
	   RepairTypeCode+'-'+ISNULL(RepairTypeName,'') AS RepairTypeName
FROM dbo.Bas_RepairType
ORDER BY RepairTypeCode";
            try
            {
                System.Data.DataTable dt = DB.DBHelper.GetDataTable(sql, null, null, true);
                System.Data.DataRow row = dt.NewRow();
                dt.Rows.InsertAt(row, 0);
                cmbRepairType.ItemsSource = dt.DefaultView;
                cmbRepairType.SelectedValuePath = "RepairTypeId";
                cmbRepairType.DisplayMemberPath = "RepairTypeName";
                cmbRepairType.SelectedIndex = 0;
            }
            catch (Exception e)
            {
                Component.MessageBox.MyMessageBox.ShowError(e.Message);
            }
        }

        private void LoadEquipmentMaintenanceResult()
        {
            string sql = @"
SELECT Code,Name
FROM dbo.Set_SystemType WITH(NOLOCK) 
WHERE TypeCode='EquipmentMaintenanceResult'
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

        private void txtEquipmentCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.DataContext = null;
        }

        private bool LoadEquipmentCode(string code)
        {
            string sql = @"SELECT ROW_NUMBER()OVER(ORDER BY Bas_Equipment.EquipmentCode DESC) AS RowIndex,
	   Bas_Equipment.EquipmentId,
	   Bas_EquipmentGroup.EquipmentGroupCode+'-'+ISNULL(dbo.Bas_EquipmentGroup.EquipmentGroupDesc,'') AS EquipmentGroupName,
	   Bas_EquipmentModel.EquipmentModelCode+'-'+ISNULL(Bas_EquipmentModel.EquipmentModelDesc,'') AS EquipmentModelName,
	   Bas_Equipment.EquipmentCode,
	   Bas_Equipment.EquipmentDesc,
	   Bas_Equipment.Status,
	   dbo.Ftn_GetStatusName(Bas_Equipment.Status,'EquipmentStatus') AS StatusName,
	   Bas_WorkShop.WorkShopCode+'-'+ISNULL(Bas_WorkShop.WorkShopDesc,'') AS WorkShopName,
	   Bas_Factory.FactoryCode+'-'+ISNULL(Bas_Factory.FactoryName,'') AS FactoryName,
	   Bas_Company.CompanyCode+'-'+ISNULL(Bas_Company.CompanyDesc,'') AS CompanyName
FROM dbo.Bas_Equipment WITH(NOLOCK) 
LEFT JOIN dbo.Bas_EquipmentModel WITH(NOLOCK) ON dbo.Bas_EquipmentModel.EquipmentModelId = dbo.Bas_Equipment.EquipmentModelId
LEFT JOIN dbo.Bas_EquipmentGroup WITH(NOLOCK) ON dbo.Bas_EquipmentGroup.EquipmentGroupId = dbo.Bas_Equipment.EquipmentGroupId
LEFT JOIN dbo.Bas_WorkShop WITH(NOLOCK) ON dbo.Bas_WorkShop.WorkShopId = dbo.Bas_Equipment.WorkShopId
LEFT JOIN dbo.Bas_Factory WITH(NOLOCK) ON dbo.Bas_Factory.FactoryId = dbo.Bas_Equipment.FactoryId
LEFT JOIN dbo.Bas_Company WITH(NOLOCK) ON dbo.Bas_Company.CompanyId = dbo.Bas_Equipment.CompanyId
WHERE Bas_Equipment.EquipmentCode=@EquipmentCode
";
            Data.Parameters parameters = new Parameters()
                .Add("EquipmentCode", code);
            try
            {
                DataTable dt = DB.DBHelper.GetDataTable(sql, parameters, null, true);
                if (dt.Rows.Count < 1)
                {
                    Component.MessageBox.MyMessageBox.ShowError(string.Format("设备错误。"));
                    return false;
                }
                else if(dt.Rows[0].Field<string>("Status").ToUpper() == "SendRepair".ToUpper()|| dt.Rows[0].Field<string>("Status").ToUpper() == "Scrap".ToUpper() || dt.Rows[0].Field<string>("Status").ToUpper() == "WaitScrapSure".ToUpper())
                {
                    Component.MessageBox.MyMessageBox.ShowError(string.Format("设备状态为[{0}]，不能保养。", dt.Rows[0].Field<string>("StatusName")));
                    return false;
                }
                else
                {
                    txtEquipmentCode.Text = dt.Rows[0]["EquipmentCode"].ToString().Trim();
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

        private void txtEquipmentCode_LostFocus(object sender, RoutedEventArgs e)
        {
            if (this.DataContext == null)
            {
                if (txtEquipmentCode.Text.Trim() != string.Empty)
                {
                    if (!LoadEquipmentCode(txtEquipmentCode.Text.Trim()))
                    {
                        txtEquipmentCode.Text = string.Empty;
                    }
                }
            }
        }

        private void btnEquipment_Click(object sender, RoutedEventArgs e)
        {
            Selector.SelectorEquipment win = new Selector.SelectorEquipment(null, "SendRepair,Scrap,WaitScrapSure");
            if (win.ShowDialog())
            {
                txtEquipmentCode.Text = win.SelectRow["EquipmentCode"].ToString().Trim();
                this.DataContext = win.SelectRow;
            }
        }

        private void UserVendor_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            matrix1.ItemsSource = null;
            matrix2.ItemsSource = null;
            cmbRepairType.SelectedIndex = 0;
            cmbRepairType.IsEnabled = false;
            cmbMainRepairUser.SelectedIndex = 0;
            cmbMainRepairUser.IsEnabled = false;
            txtStartMaintenanceDateTime.SelectedDate = null;
            txtEndMaintenanceDateTime.SelectedDate = null;
            txtMaintenanceDesc.Text = string.Empty;
            txtSummarize.Text = string.Empty;
            if (DataContext == null)
            {
                return;
            }
            else
            {
                QueryDetail();
            }
        }

        private void QueryDetail()
        {
            DataRowView row = DataContext as DataRowView;
            string sql = @"SELECT Bas_Equipment_MaintenanceItem.EquipmentMaintenanceItemId,
	   Bas_Equipment_MaintenanceItem.EquipmentMaintenanceItemCode,
	   Bas_Equipment_MaintenanceItem.EquipmentMaintenanceItemDesc,
	   CAST(Bas_Equipment_MaintenanceItem.LastMaintenanceDateTime AS DATE) AS LastMaintenanceDateTime,
	   CASE WHEN Bas_Equipment_MaintenanceItem.LastMaintenanceDateTime IS NULL THEN NULL ELSE CAST(DATEADD(DAY,Bas_Equipment_MaintenanceItem.MaintenanceCycle,Bas_Equipment_MaintenanceItem.LastMaintenanceDateTime) AS DATE) END AS PlanMaintenanceDateTime,
	   CASE WHEN Bas_Equipment_MaintenanceItem.LastMaintenanceDateTime IS NULL THEN NULL ELSE DATEDIFF(DAY,GETDATE(),DATEADD(DAY,Bas_Equipment_MaintenanceItem.MaintenanceCycle,Bas_Equipment_MaintenanceItem.LastMaintenanceDateTime)) END  AS [Days],
	   '' AS Result,
	   '' AS Summarize,
       'Blue' AS RowForeground
FROM dbo.Bas_Equipment_MaintenanceItem WITH(NOLOCK) 
WHERE Bas_Equipment_MaintenanceItem.EquipmentId=@EquipmentId
";
            Data.Parameters parameters = new Parameters()
                    .Add("EquipmentId", row["EquipmentId"]);

            Component.MaskBusy.Busy(grid, "正在加载保养项目。。。");
            new TaskFactory<Data.Result<System.Data.DataTable>>().StartNew(() =>
            {
                Data.Result<System.Data.DataTable> result = new Data.Result<System.Data.DataTable>();
                try
                {
                    System.Data.DataTable r1 = DB.DBHelper.GetDataTable(sql, parameters, null, true);

                    result.HasError = false;
                    result.Value = r1;
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

                        DataTable dt = new DataTable();
                        dt.Columns.Add("ItemId", typeof(long));
                        dt.Columns.Add("ItemCode", typeof(string));
                        dt.Columns.Add("ItemSpecification", typeof(string));
                        dt.Columns.Add("Qty", typeof(decimal));

                        matrix2.ItemsSource = dt.DefaultView;

                    }
                }));
                Component.MaskBusy.Hide(grid);
            });
        }

        private void colResult_CellValueChanged(object sender, RoutedEventArgs e)
        {
            if(e.OriginalSource!=null&&e.OriginalSource is DataRowView)
            {
                DataRowView row = e.OriginalSource as DataRowView;
                if(row.Row.Field<string>("Result").ToUpper()=="NG")
                {
                    row.Row["RowForeground"] = "Red";
                }
                else
                {
                    row.Row["RowForeground"] = "Blue";
                }
            }
            if (matrix1.ItemsSource == null)
            {
                cmbRepairType.SelectedIndex = 0;
                cmbRepairType.IsEnabled = false;
                cmbMainRepairUser.SelectedIndex = 0;
                cmbMainRepairUser.IsEnabled = false;
            }
            else
            {
                bool isNG = false;
                foreach (DataRowView row in (matrix1.ItemsSource as DataView))
                {
                    if (row.Row.Field<string>("Result").ToUpper() == "NG")
                    {
                        isNG = true;
                        break;
                    }
                }


                if (isNG)
                {
                    cmbRepairType.IsEnabled = true;
                    cmbMainRepairUser.IsEnabled = true;
                }
                else
                {
                    cmbRepairType.SelectedIndex = 0;
                    cmbRepairType.IsEnabled = false;
                    cmbMainRepairUser.SelectedIndex = 0;
                    cmbMainRepairUser.IsEnabled = false;
                }
            }
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext == null)
                return;
            if(matrix1.ItemsSource==null)
            {
                return;
            }

            if (!txtStartMaintenanceDateTime.SelectedDate.HasValue)
            {
                Component.MessageBox.MyMessageBox.ShowError("请输入开始保养时间。");
                txtStartMaintenanceDateTime.Focus();
                return;
            }
            if (!txtEndMaintenanceDateTime.SelectedDate.HasValue)
            {
                Component.MessageBox.MyMessageBox.ShowError("请输入截止保养时间。");
                txtEndMaintenanceDateTime.Focus();
                return;
            }

            StringBuilder xml = new StringBuilder();
            bool isHasValue = false;
            bool isHasError = false;
            xml.Append(string.Format("<xml>"));
            foreach (DataRowView row in (matrix1.ItemsSource as DataView))
            {
                if (row.Row.Field<string>("Result") != "")
                {
                    xml.Append(string.Format("<row EquipmentMaintenanceItemId=\"{0}\" Result=\"{1}\" Summarize=\"{2}\"/>",
                        row["EquipmentMaintenanceItemId"],
                        row["Result"],
                        WinAPI.File.XMLHelper.StringFormat(row["Summarize"].ToString())));

                    if (row.Row.Field<string>("Result").ToUpper() == "NG")
                    {
                        isHasError = true;
                    }

                    isHasValue = true;
                }
            }
            xml.Append(string.Format("</xml>"));

            StringBuilder xml1 = new StringBuilder();
            xml1.Append(string.Format("<xml>"));
            foreach (DataRowView row in (matrix2.ItemsSource as DataView))
            {
                xml1.Append(string.Format("<row ItemId=\"{0}\" Qty=\"{1}\"/>",
                    row["ItemId"],
                    row["Qty"]));
            }
            xml1.Append(string.Format("</xml>"));


            if (!isHasValue)
            {
                Component.MessageBox.MyMessageBox.ShowError("没有需要提交的保养项。");
                return;
            }

            object mainRepairUserId = null;
            if (isHasError)
            {
                if(cmbRepairType.SelectedIndex==0)
                {
                    Component.MessageBox.MyMessageBox.ShowError("保养中有不良项，需要送修，请选择维修类型。");
                    cmbRepairType.Focus();
                    return;
                }
                if(cmbMainRepairUser.SelectedIndex>0)
                {
                    mainRepairUserId = cmbMainRepairUser.SelectedValue;
                }
            }

            if (Component.MessageBox.MyMessageBox.ShowQuestion(string.Format("确定提交吗？")) != MessageBoxResult.OK)
                return;


            string sql = "Prd_Equipment_Maintenance";
            Data.Parameters parameters = new Parameters()
                .Add("EquipmentId", (this.DataContext as DataRowView)["EquipmentId"])
                .Add("XmlDetail", xml.ToString(), SqlDbType.Xml, int.MaxValue)
                .Add("XmlParts", xml1.ToString(), SqlDbType.Xml, int.MaxValue)
                .Add("StartMaintenanceDateTime", txtStartMaintenanceDateTime.SelectedDate.Value.ToString("yyyy-MM-dd HH:mm:ss"))
                .Add("EndMaintenanceDateTime", txtEndMaintenanceDateTime.SelectedDate.Value.ToString("yyyy-MM-dd HH:mm:ss"))
                .Add("MaintenanceDesc", txtMaintenanceDesc.Text, SqlDbType.NVarChar, 500)
                .Add("RepairTypeId", cmbRepairType.SelectedValue)
                .Add("MainRepairUserId", mainRepairUserId)
                .Add("Summarize", txtSummarize.Text)
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
                txtEquipmentCode.Text = string.Empty;
                return;
            }
            else
            {
                Component.MessageBox.MyMessageBox.ShowError(parameters["Return_Message"].ToString());
            }
        }

        private void cmbRepairType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cmbMainRepairUser.ItemsSource = null;
            if(cmbRepairType.SelectedIndex==0)
            {
                return;
            }

            string sql = @"DECLARE @ParamValue NVARCHAR(50)

SELECT @ParamValue=Set_SysParam.ParamValue
FROM dbo.Set_SysParam WITH(NOLOCK) 
LEFT JOIN dbo.Set_SysParamGroup WITH(NOLOCK) ON dbo.Set_SysParamGroup.ParamGroupId = dbo.Set_SysParam.ParamGroupId
WHERE Set_SysParamGroup.ParamGroupCode='Department'
AND Set_SysParam.ParamCode='RepairDepartmentCode'

SELECT Set_User.SysUserId,
			   Set_User.SysUserCode+'-'+ISNULL(dbo.Set_User.SysUserName,'') AS RepairUserCode
FROM Set_User WITH(NOLOCK) 
INNER JOIN dbo.Bas_Department WITH(NOLOCK) ON dbo.Bas_Department.DepartmentId = dbo.Set_User.DepartmentId
INNER JOIN dbo.Bas_DepartmentPosition WITH(NOLOCK) ON dbo.Bas_DepartmentPosition.DepartmentPositionId = dbo.Set_User.DepartmentPositionId
WHERE Bas_Department.DepartmentCode=@ParamValue
AND Bas_DepartmentPosition.Type NOT IN('Director','DeputyDirector','Manager','DeputyManager','Supervisor','DeputySupervisor')
AND EXISTS(
	SELECT * FROM dbo.Set_User_RepairType WITH(NOLOCK) WHERE RepairTypeId=@RepairTypeId AND SysUserId=Set_User.SysUserId
)";
            Data.Parameters parameters = new Parameters().Add("RepairTypeId", cmbRepairType.SelectedValue);
            try
            {
                System.Data.DataTable dt = DB.DBHelper.GetDataTable(sql, parameters, null, true);
                System.Data.DataRow row = dt.NewRow();
                dt.Rows.InsertAt(row, 0);
                cmbMainRepairUser.ItemsSource = dt.DefaultView;
                cmbMainRepairUser.SelectedValuePath = "SysUserId";
                cmbMainRepairUser.DisplayMemberPath = "RepairUserCode";
                cmbMainRepairUser.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Component.MessageBox.MyMessageBox.ShowError(ex.Message);
            }
        }

        private void matrix2_LinkClick(object value, object parameter)
        {
            if (this.DataContext == null || matrix2.ItemsSource == null)
                return;

            DataRowView row = parameter as DataRowView;

            Edit.EquipmentRepair_EditMaintenanceParts win = new Edit.EquipmentRepair_EditMaintenanceParts(row.Row.Field<long?>("ItemId"),
                                                                                                      row.Row.Field<string>("ItemCode"),
                                                                                                      row.Row.Field<decimal?>("Qty"));
            if (win.ShowDialog())
            {
                row["ItemId"] = win.ItemId;
                row["ItemCode"] = win.ItemCode;
                row["ItemSpecification"] = win.ItemSpecification;
                row["Qty"] = win.Qty;
            }
        }

        private void btnPartsAdd_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext == null || matrix2.ItemsSource == null)
                return;

            Edit.EquipmentRepair_EditMaintenanceParts win = new Edit.EquipmentRepair_EditMaintenanceParts(null, null, null);
            if (win.ShowDialog())
            {
                DataTable dt = (matrix2.ItemsSource as DataView).Table;
                DataRow row = dt.NewRow();
                row["ItemId"] = win.ItemId;
                row["ItemCode"] = win.ItemCode;
                row["ItemSpecification"] = win.ItemSpecification;
                row["Qty"] = win.Qty;
                dt.Rows.Add(row);
                matrix2.BestFitColumns();
            }
        }

        private void btnPartsRemove_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext == null || matrix2.ItemsSource == null)
                return;

            List<DataRowView> l = new List<DataRowView>();
            foreach (DataRowView row in matrix2.SelectedItems)
            {
                l.Add(row);
            }

            if (l.Count < 1)
            {
                Component.MessageBox.MyMessageBox.Show("请选择要删除的更换部件。");
                return;
            }


            if (Component.MessageBox.MyMessageBox.ShowQuestion(string.Format("确定删除选中的[{0}]行吗？", l.Count)) != MessageBoxResult.OK)
                return;

            foreach (DataRowView row in l)
            {
                (matrix2.ItemsSource as DataView).Table.Rows.Remove(row.Row);
            }
        }
    }
}
