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
    /// EquipmentRepair.xaml 的交互逻辑
    /// </summary>
    public partial class EquipmentRepair : Component.Controls.User.UserVendor
    {
        private bool IsManager = false;
        private bool IsRepair = false;
        private ViewState _FormState = ViewState.Normal;
        private ViewState FormState
        {
            get
            {
                return _FormState;
            }
            set
            {
                if(value!=_FormState)
                {
                    _FormState = value;
                    FormStateChanged();
                }
                else
                {
                    _FormState = value;
                }
            }
        }
        public EquipmentRepair(Framework.SystemAuthority authority) :
            base(authority)
        {
            InitializeComponent();
            LoadResult();
            SetBtnVisibility();
        }
        public EquipmentRepair(List<Framework.SystemAuthority> authoritys, Data.Parameters parameters, string flag) :
            base(authoritys, parameters, flag)
        {
            InitializeComponent();
            LoadResult();
            SetBtnVisibility();

            this.LoadComplete += new RoutedEventHandler((sender, e) =>
            {
                if (parameters != null)
                {
                    if (parameters.IsExists("EquipmentRepairInDetailId") && parameters["EquipmentRepairInDetailId"] != null && parameters["EquipmentRepairInDetailId"].ToString().Trim() != string.Empty)
                    {
                        Data.Parameters _parameters = new Parameters();
                        _parameters.Add("EquipmentRepairInDetailId", parameters["EquipmentRepairInDetailId"]);
                        _parameters.Add("EquipmentCode", DBNull.Value);

                        long? id = GetLastEquipmentRepairId(_parameters);

                        if(id.HasValue)
                        {
                            LoadEquipmentCode(id.Value);
                        }
                        else
                        {
                            this.Close();
                        }
                    }
                    else
                    {
                        this.Close();
                    }
                }
                else
                {
                    this.Close();
                }
            });
        }



        private void LoadResult()
        {
            string sql = @"
SELECT Code,Name
FROM dbo.Set_SystemType WITH(NOLOCK) 
WHERE TypeCode='EquipmentRepairResult' AND Code!='WaitRepair'
ORDER BY Sequence";
            try
            {
                System.Data.DataTable dt = DB.DBHelper.GetDataTable(sql, null, null, true);
                cmbResult.ItemsSource = dt.DefaultView;
                cmbResult.SelectedValuePath = "Code";
                cmbResult.DisplayMemberPath = "Name";
                cmbResult.SelectedIndex = 0;
            }
            catch (Exception e)
            {
                Component.MessageBox.MyMessageBox.ShowError(e.Message);
            }
        }

        private long? GetLastEquipmentRepairId(Data.Parameters parameters)
        {
            string sql = @"SELECT TOP 1 Inp_EquipmentRepair.EquipmentRepairId
FROM dbo.Inp_EquipmentRepair WITH(NOLOCK) 
LEFT JOIN dbo.Bas_Equipment WITH(NOLOCK) ON dbo.Bas_Equipment.EquipmentId = dbo.Inp_EquipmentRepair.EquipmentId
WHERE Inp_EquipmentRepair.EquipmentRepairInDetailId=ISNULL(@EquipmentRepairInDetailId,Inp_EquipmentRepair.EquipmentRepairInDetailId)
AND Bas_Equipment.EquipmentCode=CASE WHEN ISNULL(@EquipmentCode,'')='' THEN Bas_Equipment.EquipmentCode ELSE @EquipmentCode END
ORDER BY Inp_EquipmentRepair.CreateDateTime DESC";
            try
            {
                DataTable dt = DB.DBHelper.GetDataTable(sql, parameters, null, false);
                if(dt.Rows.Count>0)
                {
                    return dt.Rows[0].Field<long>("EquipmentRepairId");
                }
                return null;
            }
            catch(Exception e)
            {
                Component.MessageBox.MyMessageBox.ShowError(e.Message);
            }
            return null;
        }

        private bool IsRepairDepartment()
        {
            bool repairDepartment = false;
            if (Framework.App.User.DepartmentId.HasValue)
            {
                string sql = @"SELECT Set_SysParam.ParamValue
FROM dbo.Set_SysParam WITH(NOLOCK) 
INNER JOIN dbo.Set_SysParamGroup WITH(NOLOCK) ON dbo.Set_SysParamGroup.ParamGroupId = dbo.Set_SysParam.ParamGroupId
WHERE Set_SysParamGroup.ParamGroupCode='Department'
AND Set_SysParam.ParamCode='RepairDepartmentCode'";

                try
                {
                    System.Data.DataTable dt = DB.DBHelper.GetDataTable(sql, null, null, false);
                    if (dt.Rows.Count > 0)
                    {
                        string repairDepartmentCode = dt.Rows[0].Field<string>("ParamValue");
                        if (!string.IsNullOrEmpty(repairDepartmentCode))
                        {
                            sql = "SELECT DepartmentId FROM dbo.Bas_Department WITH(NOLOCK) WHERE DepartmentCode=@DepartmentCode";
                            Data.Parameters parameters = new Parameters().Add("DepartmentCode", repairDepartmentCode);
                            dt = DB.DBHelper.GetDataTable(sql, parameters, null, false);
                            if (dt.Rows.Count > 0)
                            {
                                if (dt.Rows[0].Field<long>("DepartmentId") == Framework.App.User.DepartmentId.Value)
                                {
                                    repairDepartment = true;
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Component.MessageBox.MyMessageBox.ShowError(e.Message);
                }
            }

            return repairDepartment;
        }

        private void SetBtnVisibility()
        {
            btnEdit.Visibility = CheckAuthority('2') ? Visibility.Visible : Visibility.Collapsed;

            if(string.IsNullOrEmpty(Framework.App.User.DepartmentPositionType))
            {
                btnEdit.Visibility = Visibility.Collapsed;
                IsManager = false;
            }
            else
            {
                if(Framework.App.User.DepartmentPositionType.ToUpper().Trim()== "Director".ToUpper()||
                    Framework.App.User.DepartmentPositionType.ToUpper().Trim() == "DeputyDirector".ToUpper()||
                    Framework.App.User.DepartmentPositionType.ToUpper().Trim() == "Manager".ToUpper()||
                    Framework.App.User.DepartmentPositionType.ToUpper().Trim() == "DeputyManager".ToUpper()||
                    Framework.App.User.DepartmentPositionType.ToUpper().Trim() == "Supervisor".ToUpper()||
                    Framework.App.User.DepartmentPositionType.ToUpper().Trim() == "DeputySupervisor".ToUpper())
                {
                    IsManager = true;
                }
                else
                {
                    IsManager = false;
                }
            }

            IsRepair = IsRepairDepartment();
            FormStateChanged();
        }

        private void FormStateChanged()
        {
            if (FormState == ViewState.Modify)
            {
                btnCancel.Visibility = Visibility.Visible;
                btnSave.Visibility = Visibility.Visible;
                btnEdit.Visibility = Visibility.Collapsed;

                txtEquipmentCode.IsReadOnly = true;
                txtStartRepairDateTime.IsReadOnly = false;
                txtEndRepairDateTime.IsReadOnly = false;
                cmbResult.IsReadOnly = false;
                txtSummarize.IsReadOnly = false;

                pnlRepairLocation.Visibility = Visibility.Visible;
                pnlParts.Visibility = Visibility.Visible;

                btnEquipment.Visibility = Visibility.Collapsed;
            }
            else
            {

                btnCancel.Visibility = Visibility.Collapsed;
                btnSave.Visibility = Visibility.Collapsed;

                txtEquipmentCode.IsReadOnly = false;
                txtStartRepairDateTime.IsReadOnly = true;
                txtEndRepairDateTime.IsReadOnly = true;
                cmbResult.IsReadOnly = true;
                txtSummarize.IsReadOnly = true;

                pnlRepairLocation.Visibility = Visibility.Collapsed;
                pnlParts.Visibility = Visibility.Collapsed;

                btnEquipment.Visibility = Visibility.Visible;

                if(this.DataContext==null)
                {
                    btnEdit.Visibility = Visibility.Collapsed;
                }
                else
                {
                    if (IsRepair&&!IsManager)
                    {
                        DataRowView row = this.DataContext as DataRowView;
                        if (row.Row.Field<string>("DetailStatus").ToUpper() != "WaitRepair".ToUpper()&& row.Row.Field<string>("DetailStatus").ToUpper() != "Repair".ToUpper())
                        {
                            btnEdit.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            btnEdit.Visibility = Visibility.Visible;
                        }
                    }
                    else
                    {
                        btnEdit.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        private void txtEquipmentCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.DataContext = null;
        }

        private bool LoadEquipmentCode(long equipmentRepairId)
        {
            string sql = @"SELECT ROW_NUMBER()OVER(ORDER BY Inp_EquipmentRepairIn.EquipmentRepairInCode DESC) AS RowIndex,
	   Inp_EquipmentRepair.EquipmentRepairId,
	   Inp_EquipmentRepairInDetail.EquipmentRepairInId,
	   Inp_EquipmentRepair.EquipmentRepairInDetailId,
	   Inp_EquipmentRepair.EquipmentId,
	   Inp_EquipmentRepairIn.EquipmentRepairInCode,
	   Inp_EquipmentRepairIn.Type,
	   Inp_EquipmentRepairIn.SoureId,
	   dbo.Ftn_GetStatusName(Inp_EquipmentRepairIn.Type,'EquipmentRepairInType') AS TypeName,
	   Bas_Equipment.EquipmentCode,
	   Bas_Equipment.EquipmentDesc,
	   Inp_EquipmentRepair.RepairTypeId,
	   Bas_RepairType.RepairTypeCode+'-'+ISNULL(Bas_RepairType.RepairTypeName,'') AS RepairTypeName,
	   dbo.Inp_EquipmentRepairInDetail.Status AS DetailStatus,
	   Inp_EquipmentRepair.Status,
	   dbo.Ftn_GetStatusName(Inp_EquipmentRepair.Status,'EquipmentRepairInStatus') AS StatusName,
	   MainRepairUser.SysUserId AS MainRepairUserId,
	   MainRepairUser.SysUserCode+'-'+ISNULL(MainRepairUser.SysUserName,'') AS MainRepairUserName,
	   STUFF((
				SELECT ';'+CAST(Inp_EquipmentRepairInDetail_AssistRepairUser.AssistRepairUserId AS NVARCHAR(50))
				FROM dbo.Inp_EquipmentRepairInDetail_AssistRepairUser WITH(NOLOCK) 
				WHERE Inp_EquipmentRepairInDetail_AssistRepairUser.EquipmentRepairInDetailId=Inp_EquipmentRepairInDetail.EquipmentRepairInDetailId
				FOR XML PATH('')
		),1,1,'') AS AssistRepairUserId,
	   STUFF((
				SELECT ';'+dbo.Set_User.SysUserCode+'-'+ISNULL(dbo.Set_User.SysUserName,'')
				FROM dbo.Inp_EquipmentRepairInDetail_AssistRepairUser WITH(NOLOCK) 
				LEFT JOIN dbo.Set_User WITH(NOLOCK) ON Inp_EquipmentRepairInDetail_AssistRepairUser.AssistRepairUserId=Set_User.SysUserId
				WHERE Inp_EquipmentRepairInDetail_AssistRepairUser.EquipmentRepairInDetailId=Inp_EquipmentRepairInDetail.EquipmentRepairInDetailId
				FOR XML PATH('')
		),1,1,'') AS AssistRepairUserName,
	   DelegateUser.SysUserId AS DelegateUserId,
	   DelegateUser.SysUserCode+'-'+ISNULL(DelegateUser.SysUserName,'') AS DelegateUserName,
	   Inp_EquipmentRepairInDetail.DelegateDateTime,
	   ReceiveUser.SysUserId AS ReceiveUserId,
	   ReceiveUser.SysUserCode+'-'+ISNULL(ReceiveUser.SysUserName,'') AS ReceiveUserName,
	   Inp_EquipmentRepairInDetail.ReceiveDateTime,
	   RepairCompleteUser.SysUserId AS RepairCompleteUserId,
	   RepairCompleteUser.SysUserCode+'-'+ISNULL(RepairCompleteUser.SysUserName,'') AS RepairCompleteUserName,
	   Inp_EquipmentRepairInDetail.RepairCompleteDateTime,
	   SendRepairUser.SysUserId AS SendRepairUserId,
	   SendRepairUser.SysUserCode+'-'+ISNULL(SendRepairUser.SysUserName,'') AS SendRepairUserName,
	   Inp_EquipmentRepairIn.SendRepairDateTime,
	   Inp_EquipmentRepairInDetail.SendRepairComment,
	   Inp_EquipmentRepairInDetail.DelegateComment,
	   Inp_EquipmentRepairInDetail.RejectDateTime,
	   Inp_EquipmentRepairInDetail.RejectComment,
	   Inp_EquipmentRepairInDetail.ReDelegateDateTime,
	   Inp_EquipmentRepairInDetail.ReDelegateComment,
	   Inp_EquipmentRepairInDetail.OutSourcingDateTime,
	   Inp_EquipmentRepairInDetail.OutSourcingComment,
	   SureUser.SysUserId AS SureUserId,
	   SureUser.SysUserCode+'-'+ISNULL(SureUser.SysUserName,'') AS SureUserName,
	   Inp_EquipmentRepairInDetail.SureDateTime,
	   Inp_EquipmentRepairInDetail.NotAgreeComment,
	   Inp_EquipmentRepair.Summarize,
	   Inp_EquipmentRepair.Result,
	   dbo.Ftn_GetStatusName(Inp_EquipmentRepair.Result,'EquipmentRepairResult') AS ResultName,
	   Inp_EquipmentRepair.StartRepairDateTime,
	   Inp_EquipmentRepair.EndRepairDateTime
FROM dbo.Inp_EquipmentRepair WITH(NOLOCK) 
LEFT JOIN dbo.Inp_EquipmentRepairInDetail WITH(NOLOCK) ON Inp_EquipmentRepairInDetail.EquipmentRepairInDetailId=Inp_EquipmentRepair.EquipmentRepairInDetailId
LEFT JOIN dbo.Inp_EquipmentRepairIn WITH(NOLOCK) ON dbo.Inp_EquipmentRepairIn.EquipmentRepairInId = dbo.Inp_EquipmentRepairInDetail.EquipmentRepairInId
LEFT JOIN dbo.Bas_Equipment WITH(NOLOCK) ON dbo.Bas_Equipment.EquipmentId = dbo.Inp_EquipmentRepair.EquipmentId
LEFT JOIN dbo.Bas_RepairType WITH(NOLOCK) ON dbo.Bas_RepairType.RepairTypeId = dbo.Inp_EquipmentRepair.RepairTypeId
LEFT JOIN dbo.Set_User SendRepairUser WITH(NOLOCK) ON Inp_EquipmentRepairIn.SendRepairUserId=SendRepairUser.SysUserId
LEFT JOIN dbo.Set_User MainRepairUser WITH(NOLOCK) ON Inp_EquipmentRepair.MainRepairUserId=MainRepairUser.SysUserId
LEFT JOIN dbo.Set_User DelegateUser WITH(NOLOCK) ON Inp_EquipmentRepairInDetail.DelegateUserId=DelegateUser.SysUserId
LEFT JOIN dbo.Set_User ReceiveUser WITH(NOLOCK) ON Inp_EquipmentRepairInDetail.ReceiveUserId=ReceiveUser.SysUserId
LEFT JOIN dbo.Set_User RepairCompleteUser WITH(NOLOCK) ON Inp_EquipmentRepairInDetail.RepairCompleteUserId=RepairCompleteUser.SysUserId
LEFT JOIN dbo.Set_User SureUser WITH(NOLOCK) ON Inp_EquipmentRepairInDetail.SureUserId=SureUser.SysUserId
WHERE Inp_EquipmentRepair.EquipmentRepairId=@EquipmentRepairId
";
            Data.Parameters parameters = new Parameters()
                .Add("EquipmentRepairId", equipmentRepairId);
            try
            {
                DataTable dt = DB.DBHelper.GetDataTable(sql, parameters, null, true);
                if (dt.Rows.Count < 1)
                {
                    Component.MessageBox.MyMessageBox.ShowError(string.Format("设备错误。"));
                    return false;
                }
                else
                {
                    txtEquipmentCode.Text = dt.Rows[0]["EquipmentCode"].ToString().Trim();
                    this.DataContext = dt.DefaultView[0];
                    txtStartRepairDateTime.SelectedDate = dt.Rows[0].Field<DateTime?>("StartRepairDateTime");
                    txtEndRepairDateTime.SelectedDate = dt.Rows[0].Field<DateTime?>("EndRepairDateTime");
                    cmbResult.SelectedValue = dt.Rows[0].Field<string>("Result");
                    txtSummarize.Text = dt.Rows[0].Field<string>("Summarize");
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
                    Data.Parameters _parameters = new Parameters();
                    _parameters.Add("EquipmentRepairInDetailId", DBNull.Value);
                    _parameters.Add("EquipmentCode", txtEquipmentCode.Text);

                    long? id = GetLastEquipmentRepairId(_parameters);

                    if(id.HasValue)
                    {
                        if (!LoadEquipmentCode(id.Value))
                        {
                            txtEquipmentCode.Text = string.Empty;
                        }
                    }
                    else
                    {
                        txtEquipmentCode.Text = string.Empty;
                    }
                }
            }
        }

        private void btnEquipment_Click(object sender, RoutedEventArgs e)
        {
            Selector.SelectorEquipmentRepair win = new Selector.SelectorEquipmentRepair(null, null, false);
            if (win.ShowDialog())
            {
                txtEquipmentCode.Text = win.SelectRow["EquipmentCode"].ToString().Trim();
                this.DataContext = win.SelectRow;
                txtStartRepairDateTime.SelectedDate = win.SelectRow.Row.Field<DateTime?>("StartRepairDateTime");
                txtEndRepairDateTime.SelectedDate = win.SelectRow.Row.Field<DateTime?>("EndRepairDateTime");
                cmbResult.SelectedValue = win.SelectRow.Row.Field<string>("Result");
                txtSummarize.Text = win.SelectRow.Row.Field<string>("Summarize");
            }
        }

        private void UserVendor_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            FormStateChanged();
            matrix1.ItemsSource = null;
            matrix2.ItemsSource = null;
            matrix3.ItemsSource = null;
            txtStartRepairDateTime.SelectedDate = null;
            txtEndRepairDateTime.SelectedDate = null;
            cmbResult.SelectedValue = null;
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
            string sql1 = @"SELECT 0 AS Flag,
	   Inp_EquipmentRepairDetail.EquipmentRepairDetailId,
	   Inp_EquipmentRepairDetail.RepairAreaId,
	   Bas_RepairArea.RepairAreaCode+'-'+ISNULL(Bas_RepairArea.RepairAreaName,'') AS RepairAreaName,
	   Inp_EquipmentRepairDetail.RepairAreaLocationId,
	   Bas_RepairArea_Location.RepairAreaLocationCode+'-'+ISNULL(Bas_RepairArea_Location.RepairAreaLocationName,'') AS RepairAreaLocationName,
	   Inp_EquipmentRepairDetail.Summarize
FROM Inp_EquipmentRepairDetail WITH(NOLOCK) 
LEFT JOIN dbo.Bas_RepairArea WITH(NOLOCK) ON dbo.Bas_RepairArea.RepairAreaId = dbo.Inp_EquipmentRepairDetail.RepairAreaId
LEFT JOIN dbo.Bas_RepairArea_Location WITH(NOLOCK) ON dbo.Bas_RepairArea_Location.RepairAreaLocationId = dbo.Inp_EquipmentRepairDetail.RepairAreaLocationId
WHERE Inp_EquipmentRepairDetail.EquipmentRepairId=@EquipmentRepairId
";
            Data.Parameters parameters1 = new Parameters()
                    .Add("EquipmentRepairId", row["EquipmentRepairId"]);

            string sql2 = @"SELECT 0 AS Flag,
	   Inp_EquipmentRepair_Parts.EquipmentRepairPartsId,
	   Inp_EquipmentRepair_Parts.ItemId,
	   Bas_Item.ItemCode,
	   Bas_Item.ItemSpecification,
	   Inp_EquipmentRepair_Parts.Qty
FROM dbo.Inp_EquipmentRepair_Parts WITH(NOLOCK) 
LEFT JOIN dbo.Bas_Item WITH(NOLOCK) ON dbo.Bas_Item.ItemId = dbo.Inp_EquipmentRepair_Parts.ItemId
WHERE Inp_EquipmentRepair_Parts.EquipmentRepairId=@EquipmentRepairId
";
            Data.Parameters parameters2 = new Parameters()
                    .Add("EquipmentRepairId", row["EquipmentRepairId"]);

            string sql3 = null;
            Data.Parameters parameters3 = null;

            if (row.Row.Field<string>("Type").ToUpper() == "SpotCheckingSendRepair".ToUpper() && row.Row.Field<long?>("SoureId").HasValue)
            {
                sql3 = @"SELECT Inp_Equipment_SpotCheckingDetail.EquipmentSpotCheckingItemCode AS Code,
	   Inp_Equipment_SpotCheckingDetail.EquipmentSpotCheckingItemDesc AS [Desc],
	   Inp_Equipment_SpotCheckingDetail.Summarize AS Summarize
FROM Inp_Equipment_SpotCheckingDetail WITH(NOLOCK) 
WHERE Inp_Equipment_SpotCheckingDetail.EquipmentSpotCheckingId=@EquipmentSpotCheckingId
AND Inp_Equipment_SpotCheckingDetail.EquipmentId=@EquipmentId
AND Inp_Equipment_SpotCheckingDetail.Result='NG'";
                parameters3= new Parameters()
                    .Add("EquipmentSpotCheckingId", row["SoureId"])
                    .Add("EquipmentId", row["EquipmentId"]);
            }
            else if(row.Row.Field<string>("Type").ToUpper() == "CheckingSendRepair".ToUpper() && row.Row.Field<long?>("SoureId").HasValue)
            {
                sql3 = @"SELECT Inp_Equipment_CheckingDetail.EquipmentCheckingItemCode AS Code,
	   Inp_Equipment_CheckingDetail.EquipmentCheckingItemDesc AS [Desc],
	   Inp_Equipment_CheckingDetail.Summarize AS Summarize
FROM Inp_Equipment_CheckingDetail WITH(NOLOCK) 
WHERE Inp_Equipment_CheckingDetail.EquipmentCheckingId=@EquipmentCheckingId
AND Inp_Equipment_CheckingDetail.EquipmentId=@EquipmentId
AND Inp_Equipment_CheckingDetail.Result='NG'";
                parameters3 = new Parameters()
                    .Add("EquipmentCheckingId", row["SoureId"])
                    .Add("EquipmentId", row["EquipmentId"]);
            }
            else if (row.Row.Field<string>("Type").ToUpper() == "MaintenanceSendRepair".ToUpper() && row.Row.Field<long?>("SoureId").HasValue)
            {
                sql3 = @"SELECT Inp_Equipment_MaintenanceDetail.EquipmentMaintenanceItemCode AS Code,
	   Inp_Equipment_MaintenanceDetail.EquipmentMaintenanceItemDesc AS [Desc],
	   Inp_Equipment_MaintenanceDetail.Summarize AS Summarize
FROM Inp_Equipment_MaintenanceDetail WITH(NOLOCK) 
WHERE Inp_Equipment_MaintenanceDetail.EquipmentMaintenanceId=@EquipmentMaintenanceId
AND Inp_Equipment_MaintenanceDetail.EquipmentId=@EquipmentId
AND Inp_Equipment_MaintenanceDetail.Result='NG'";
                parameters3 = new Parameters()
                    .Add("EquipmentMaintenanceId", row["SoureId"])
                    .Add("EquipmentId", row["EquipmentId"]);
            }


            Component.MaskBusy.Busy(grid, "正在加载明细。。。");
            new TaskFactory<Data.Result<System.Data.DataTable, System.Data.DataTable, System.Data.DataTable>>().StartNew(() =>
            {
                Data.Result<System.Data.DataTable, System.Data.DataTable, System.Data.DataTable> result = new Data.Result<System.Data.DataTable, System.Data.DataTable, System.Data.DataTable>();
                try
                {
                    result.Value1 = DB.DBHelper.GetDataTable(sql1, parameters1, null, true);
                    result.Value2 = DB.DBHelper.GetDataTable(sql2, parameters2, null, true);
                    if(!string.IsNullOrEmpty(sql3))
                    {
                        result.Value3 = DB.DBHelper.GetDataTable(sql3, parameters3, null, true);
                    }

                    result.HasError = false;
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
                        matrix1.ItemsSource = result.Result.Value1.DefaultView;
                        matrix1.BestFitColumns();
                        matrix2.ItemsSource = result.Result.Value2.DefaultView;
                        matrix2.BestFitColumns();
                        if(result.Result.Value3!=null)
                        {
                            matrix3.ItemsSource = result.Result.Value3.DefaultView;
                            matrix3.BestFitColumns();
                        }
                        else
                        {
                            matrix3.ItemsSource = null;
                        }
                    }
                }));
                Component.MaskBusy.Hide(grid);
            });
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (IsRepair && this.DataContext != null)
            {
                if (IsRepair && !IsManager)
                {
                    FormState = ViewState.Modify;
                }
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (Component.MessageBox.MyMessageBox.ShowQuestion(string.Format("确定取消吗？")) != MessageBoxResult.OK)
                return;

            FormState = ViewState.Normal;
            QueryDetail();
        }
        
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if(this.DataContext==null)
            {
                Component.MessageBox.MyMessageBox.ShowError("请先输入设备。");
                txtEquipmentCode.Focus();
                return;
            }
            if(!txtStartRepairDateTime.SelectedDate.HasValue)
            {
                Component.MessageBox.MyMessageBox.ShowError("请输入开始维修时间。");
                txtStartRepairDateTime.Focus();
                return;
            }
            if (!txtEndRepairDateTime.SelectedDate.HasValue)
            {
                Component.MessageBox.MyMessageBox.ShowError("请输入截止维修时间。");
                txtEndRepairDateTime.Focus();
                return;
            }

            if (Component.MessageBox.MyMessageBox.ShowQuestion(string.Format("确定保存吗？")) != MessageBoxResult.OK)
                return;

            StringBuilder detailXml = new StringBuilder();
            StringBuilder partXml = new StringBuilder();


            detailXml.Append(string.Format("<xml>"));
            foreach (DataRowView row in (matrix1.ItemsSource as DataView) )
            {
                detailXml.Append(string.Format("<row Flag=\"{0}\" EquipmentRepairDetailId=\"{1}\" RepairAreaId=\"{2}\" RepairAreaLocationId=\"{3}\" Summarize=\"{4}\"/>",
                    row["Flag"],
                    row["EquipmentRepairDetailId"],
                    row["RepairAreaId"],
                    row["RepairAreaLocationId"],
                    WinAPI.File.XMLHelper.StringFormat(row["Summarize"].ToString())));
            }
            detailXml.Append(string.Format("</xml>"));

            partXml.Append(string.Format("<xml>"));
            foreach (DataRowView row in (matrix2.ItemsSource as DataView))
            {
                partXml.Append(string.Format("<row Flag=\"{0}\" EquipmentRepairPartsId=\"{1}\" ItemId=\"{2}\" Qty=\"{3}\"/>",
                    row["Flag"],
                    row["EquipmentRepairPartsId"],
                    row["ItemId"],
                    row["Qty"]));
            }
            partXml.Append(string.Format("</xml>"));

            string sql = "Prd_Equipment_Repair_Save";
            Data.Parameters parameters = new Parameters()
                .Add("EquipmentRepairId", (this.DataContext as DataRowView)["EquipmentRepairId"])
                .Add("XmlDetail", detailXml.ToString(), SqlDbType.Xml, int.MaxValue)
                .Add("XmlParts", partXml.ToString(), SqlDbType.Xml, int.MaxValue)
                .Add("StartRepairDateTime", txtStartRepairDateTime.SelectedDate.Value.ToString("yyyy-MM-dd HH:mm:ss"))
                .Add("EndRepairDateTime", txtEndRepairDateTime.SelectedDate.Value.ToString("yyyy-MM-dd HH:mm:ss"))
                .Add("Result", cmbResult.SelectedValue)
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
                FormState = ViewState.Normal;
                LoadEquipmentCode((this.DataContext as DataRowView).Row.Field<long>("EquipmentRepairId"));
                return;
            }
            else
            {
                Component.MessageBox.MyMessageBox.ShowError(parameters["Return_Message"].ToString());
            }
        }

        private void btnRepairAreaAdd_Click(object sender, RoutedEventArgs e)
        {
            if(FormState==ViewState.Modify)
            {
                Edit.EquipmentRepair_EditRepairLocation win = new Edit.EquipmentRepair_EditRepairLocation(null, null, string.Empty);
                if(win.ShowDialog())
                {
                    DataTable dt = (matrix1.ItemsSource as DataView).Table;
                    DataRow row = dt.NewRow();
                    row["Flag"] = 1;
                    row["RepairAreaId"] = win.RepairAreaId.Value;
                    row["RepairAreaName"] = win.RepairAreaName;
                    row["RepairAreaLocationId"] = win.RepairAreaLocationId.Value;
                    row["RepairAreaLocationName"] = win.RepairAreaLocationName;
                    row["Summarize"] = win.Summarize;
                    dt.Rows.Add(row);
                    matrix1.BestFitColumns();
                }
            }
        }

        private void btnRepairAreaRemove_Click(object sender, RoutedEventArgs e)
        {
            if (FormState == ViewState.Modify)
            {
                List<DataRowView> l = new List<DataRowView>();
                foreach(DataRowView row in matrix1.SelectedItems)
                {
                    l.Add(row);
                }

                if(l.Count<1)
                {
                    Component.MessageBox.MyMessageBox.Show("请选择要删除的维修位置。");
                    return;
                }

                if (Component.MessageBox.MyMessageBox.ShowQuestion(string.Format("确定删除选中的[{0}]行吗？", l.Count)) != MessageBoxResult.OK)
                    return;

                foreach (DataRowView row in l)
                {
                    (matrix1.ItemsSource as DataView).Table.Rows.Remove(row.Row);
                }
            }
        }

        private void btnPartsAdd_Click(object sender, RoutedEventArgs e)
        {
            if (FormState == ViewState.Modify)
            {
                Edit.EquipmentRepair_EditRepairParts win = new Edit.EquipmentRepair_EditRepairParts(null, null, null);
                if (win.ShowDialog())
                {
                    DataTable dt = (matrix2.ItemsSource as DataView).Table;
                    DataRow row = dt.NewRow();
                    row["Flag"] = 1;
                    row["ItemId"] = win.ItemId;
                    row["ItemCode"] = win.ItemCode;
                    row["ItemSpecification"] = win.ItemSpecification;
                    row["Qty"] = win.Qty;
                    dt.Rows.Add(row);
                    matrix2.BestFitColumns();
                }
            }
        }

        private void btnPartsRemove_Click(object sender, RoutedEventArgs e)
        {
            if (FormState == ViewState.Modify)
            {
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


                if (Component.MessageBox.MyMessageBox.ShowQuestion(string.Format("确定删除选中的[{0}]行吗？",l.Count)) != MessageBoxResult.OK)
                    return;

                foreach (DataRowView row in l)
                {
                    (matrix2.ItemsSource as DataView).Table.Rows.Remove(row.Row);
                }
            }
        }

        private void matrix1_LinkClick(object value, object parameter)
        {
            if (FormState == ViewState.Modify)
            {
                DataRowView row = parameter as DataRowView;

                Edit.EquipmentRepair_EditRepairLocation win = new Edit.EquipmentRepair_EditRepairLocation(row.Row.Field<long?>("RepairAreaId"),
                                                                                                          row.Row.Field<long?>("RepairAreaLocationId"),
                                                                                                          row.Row.Field<string>("Summarize"));
                if (win.ShowDialog())
                {
                    if(row.Row.Field<int>("Flag")==0)
                    {
                        row["Flag"] = 2;
                    }
                    row["RepairAreaId"] = win.RepairAreaId.Value;
                    row["RepairAreaName"] = win.RepairAreaName;
                    row["RepairAreaLocationId"] = win.RepairAreaLocationId.Value;
                    row["RepairAreaLocationName"] = win.RepairAreaLocationName;
                    row["Summarize"] = win.Summarize;
                }
            }
        }

        private void matrix2_LinkClick(object value, object parameter)
        {
            if (FormState == ViewState.Modify)
            {
                DataRowView row = parameter as DataRowView;

                Edit.EquipmentRepair_EditRepairParts win = new Edit.EquipmentRepair_EditRepairParts(row.Row.Field<long?>("ItemId"),
                                                                                                          row.Row.Field<string>("ItemCode"),
                                                                                                          row.Row.Field<decimal?>("Qty"));
                if (win.ShowDialog())
                {
                    if (row.Row.Field<int>("Flag") == 0)
                    {
                        row["Flag"] = 2;
                    }
                    row["ItemId"] = win.ItemId;
                    row["ItemCode"] = win.ItemCode;
                    row["ItemSpecification"] = win.ItemSpecification;
                    row["Qty"] = win.Qty;
                }
            }
        }
    }
}
