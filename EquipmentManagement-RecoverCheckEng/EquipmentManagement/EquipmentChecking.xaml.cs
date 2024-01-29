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
    /// EquipmentChecking.xaml 的交互逻辑
    /// </summary>
    public partial class EquipmentChecking : Component.Controls.User.UserVendor
    {
        public EquipmentChecking(Framework.SystemAuthority authority) :
            base(authority)
        {
            InitializeComponent();
            LoadRepairType();
            LoadEquipmentCheckingResult();
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

        private void LoadEquipmentCheckingResult()
        {
            string sql = @"
SELECT Code,Name
FROM dbo.Set_SystemType WITH(NOLOCK) 
WHERE TypeCode='EquipmentCheckingResult'
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
                    Component.MessageBox.MyMessageBox.ShowError(string.Format("设备状态为[{0}]，不能巡检。", dt.Rows[0].Field<string>("StatusName")));
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
            cmbRepairType.SelectedIndex = 0;
            cmbRepairType.IsEnabled = false;
            cmbMainRepairUser.SelectedIndex = 0;
            cmbMainRepairUser.IsEnabled = false;
            txtStartCheckingDateTime.SelectedDate = null;
            txtEndCheckingDateTime.SelectedDate = null;
            txtCheckingDesc.Text = string.Empty;
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
            string sql = @"SELECT Bas_Equipment_SpotCheckingItem.EquipmentSpotCheckingItemId,
	   Bas_Equipment_SpotCheckingItem.EquipmentSpotCheckingItemCode,
	   Bas_Equipment_SpotCheckingItem.EquipmentSpotCheckingItemDesc,
	   '' AS Result,
	   '' AS Summarize,
       'Blue' AS RowForeground
FROM dbo.Bas_Equipment_SpotCheckingItem WITH(NOLOCK) 
WHERE Bas_Equipment_SpotCheckingItem.EquipmentId=@EquipmentId
";
            Data.Parameters parameters = new Parameters()
                    .Add("EquipmentId", row["EquipmentId"]);
            
            Component.MaskBusy.Busy(grid, "正在加载巡检项目。。。");
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

            if (!txtStartCheckingDateTime.SelectedDate.HasValue)
            {
                Component.MessageBox.MyMessageBox.ShowError("请输入开始巡检时间。");
                txtStartCheckingDateTime.Focus();
                return;
            }
            if (!txtEndCheckingDateTime.SelectedDate.HasValue)
            {
                Component.MessageBox.MyMessageBox.ShowError("请输入截止巡检时间。");
                txtEndCheckingDateTime.Focus();
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
                    xml.Append(string.Format("<row EquipmentSpotCheckingItemId=\"{0}\" Result=\"{1}\" Summarize=\"{2}\"/>",
                        row["EquipmentSpotCheckingItemId"],
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


            if(!isHasValue)
            {
                Component.MessageBox.MyMessageBox.ShowError("没有需要提交的巡检项。");
                return;
            }

            object mainRepairUserId = null;
            if (isHasError)
            {
                if(cmbRepairType.SelectedIndex==0)
                {
                    Component.MessageBox.MyMessageBox.ShowError("巡检中有不良项，需要送修，请选择维修类型。");
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


            string sql = "Prd_Equipment_Checking";
            Data.Parameters parameters = new Parameters()
                .Add("EquipmentId", (this.DataContext as DataRowView)["EquipmentId"])
                .Add("Xml", xml.ToString(), SqlDbType.Xml, int.MaxValue)
                .Add("StartCheckingDateTime", txtStartCheckingDateTime.SelectedDate.Value.ToString("yyyy-MM-dd HH:mm:ss"))
                .Add("EndCheckingDateTime", txtEndCheckingDateTime.SelectedDate.Value.ToString("yyyy-MM-dd HH:mm:ss"))
                .Add("CheckingDesc", txtCheckingDesc.Text, SqlDbType.NVarChar, 500)
                .Add("RepairTypeId", cmbRepairType.SelectedValue)
                .Add("MainRepairUserId", mainRepairUserId)
                .Add("Summarize", txtSummarize.Text, SqlDbType.NVarChar, int.MaxValue)
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
    }
}
