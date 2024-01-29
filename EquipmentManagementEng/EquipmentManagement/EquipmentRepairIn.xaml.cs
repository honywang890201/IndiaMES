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
    /// EquipmentRepairIn.xaml 的交互逻辑
    /// </summary>
    public partial class EquipmentRepairIn : Component.Controls.User.UserVendor
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
        public EquipmentRepairIn(Framework.SystemAuthority authority) :
            base(authority)
        {
            InitializeComponent();
            SetBtnVisibility();
        }
        public EquipmentRepairIn(List<Framework.SystemAuthority> authoritys, Data.Parameters parameters, string flag) :
            base(authoritys, parameters, flag)
        {
            InitializeComponent();
            SetBtnVisibility();

            this.LoadComplete += new RoutedEventHandler((sender, e) =>
            {
                if (parameters != null)
                {
                    if (parameters.IsExists("EquipmentRepairInCode") && parameters["EquipmentRepairInCode"] != null && parameters["EquipmentRepairInCode"].ToString().Trim() != string.Empty)
                    {
                        if (!LoadEquipmentRepairInCode(parameters["EquipmentRepairInCode"].ToString().Trim()))
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
                if (IsManager)
                {
                    colGetOrder.Visible = false;
                    colReceive.Visible = false;
                    colAgree.Visible = true;
                    colDelegate.Visible = true;
                    colAddAssistRepairUser.Visible = true;
                    colReDelegate.Visible = false;
                    colOutSourcing.Visible = true;
                    colScrap.Visible = true;

                    colGetOrder.ShowInColumnChooser = false;
                    colReceive.ShowInColumnChooser = false;
                    colAgree.ShowInColumnChooser = true;
                    colDelegate.ShowInColumnChooser = true;
                    colAddAssistRepairUser.ShowInColumnChooser = true;
                    colReDelegate.ShowInColumnChooser = false;
                    colOutSourcing.ShowInColumnChooser = true;
                    colScrap.ShowInColumnChooser = true;
                }
                else
                {
                    colGetOrder.Visible = true;
                    colReceive.Visible = true;
                    colAgree.Visible = false;
                    colDelegate.Visible = false;
                    colAddAssistRepairUser.Visible = false;
                    colReDelegate.Visible = true;
                    colOutSourcing.Visible = true;
                    colScrap.Visible = true;


                    colGetOrder.ShowInColumnChooser = true;
                    colReceive.ShowInColumnChooser = true;
                    colAgree.ShowInColumnChooser = false;
                    colDelegate.ShowInColumnChooser = false;
                    colAddAssistRepairUser.ShowInColumnChooser = false;
                    colReDelegate.ShowInColumnChooser = true;
                    colOutSourcing.ShowInColumnChooser = true;
                    colScrap.ShowInColumnChooser = true;
                }

                btnCancel.Visibility = Visibility.Visible;
                btnSave.Visibility = Visibility.Visible;
                btnEdit.Visibility = Visibility.Collapsed;

                txtEquipmentRepairInCode.IsReadOnly = true;
                btnEquipmentRepairIn.Visibility = Visibility.Collapsed;
            }
            else
            {
                colGetOrder.Visible = false;
                colReceive.Visible = false;
                colAgree.Visible = false;
                colDelegate.Visible = false;
                colAddAssistRepairUser.Visible = false;
                colReDelegate.Visible = false;
                colOutSourcing.Visible = false;
                colScrap.Visible = false;


                colGetOrder.ShowInColumnChooser = false;
                colReceive.ShowInColumnChooser = false;
                colAgree.ShowInColumnChooser = false;
                colDelegate.ShowInColumnChooser = false;
                colAddAssistRepairUser.ShowInColumnChooser = false;
                colReDelegate.ShowInColumnChooser = false;
                colOutSourcing.ShowInColumnChooser = false;
                colScrap.ShowInColumnChooser = false;

                btnCancel.Visibility = Visibility.Collapsed;
                btnSave.Visibility = Visibility.Collapsed;

                if(this.DataContext==null)
                {
                    btnEdit.Visibility = Visibility.Collapsed;
                }
                else
                {
                    if (IsRepair)
                    {
                        DataRowView row = this.DataContext as DataRowView;
                        if (row.Row.Field<string>("Status").ToUpper() == "Close".ToUpper())
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

                txtEquipmentRepairInCode.IsReadOnly = false;
                btnEquipmentRepairIn.Visibility = Visibility.Visible;
            }
        }

        private void txtEquipmentRepairInCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.DataContext = null;
        }

        private bool LoadEquipmentRepairInCode(string code)
        {
            string sql = @"SELECT ROW_NUMBER()OVER(ORDER BY Inp_EquipmentRepairIn.EquipmentRepairInCode DESC) AS RowIndex,
	   Inp_EquipmentRepairIn.EquipmentRepairInId,
	   Inp_EquipmentRepairIn.EquipmentRepairInCode,
	   Inp_EquipmentRepairIn.EquipmentRepairInDesc,
	   Bas_RepairType.RepairTypeCode+'-'+ISNULL(Bas_RepairType.RepairTypeName,'') AS RepairTypeName,
	   dbo.Ftn_GetStatusName(Inp_EquipmentRepairIn.Type,'EquipmentRepairInType') AS [Type],
	   dbo.Ftn_GetStatusName(Inp_EquipmentRepairIn.Status,'EquipmentRepairInStatus') AS StatusName,
	   Inp_EquipmentRepairIn.Status ,
	   MainRepairUser.SysUserCode+'-'+ISNULL(MainRepairUser.SysUserName,'') AS MainRepairUserName, 
	   SendRepairUser.SysUserCode+'-'+ISNULL(SendRepairUser.SysUserName,'') AS SendRepairUserName, 
	   Inp_EquipmentRepairIn.SendRepairDateTime
FROM dbo.Inp_EquipmentRepairIn WITH(NOLOCK) 
LEFT JOIN dbo.Bas_RepairType WITH(NOLOCK) ON dbo.Bas_RepairType.RepairTypeId = dbo.Inp_EquipmentRepairIn.RepairTypeId
LEFT JOIN dbo.Set_User MainRepairUser WITH(NOLOCK) ON Inp_EquipmentRepairIn.MainRepairUserId=MainRepairUser.SysUserId
LEFT JOIN dbo.Set_User SendRepairUser WITH(NOLOCK) ON Inp_EquipmentRepairIn.SendRepairUserId=SendRepairUser.SysUserId
WHERE Inp_EquipmentRepairIn.EquipmentRepairInCode=@EquipmentRepairInCode
";
            Data.Parameters parameters = new Parameters()
                .Add("EquipmentRepairInCode", code);
            try
            {
                DataTable dt = DB.DBHelper.GetDataTable(sql, parameters, null, true);
                if (dt.Rows.Count < 1)
                {
                    Component.MessageBox.MyMessageBox.ShowError(string.Format("送修单[{0}]错误。", code));
                    return false;
                }
                else
                {
                    txtEquipmentRepairInCode.Text = dt.Rows[0]["EquipmentRepairInCode"].ToString().Trim();
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

        private void txtEquipmentRepairInCode_LostFocus(object sender, RoutedEventArgs e)
        {
            if (this.DataContext == null)
            {
                if (txtEquipmentRepairInCode.Text.Trim() != string.Empty)
                {
                    if (!LoadEquipmentRepairInCode(txtEquipmentRepairInCode.Text.Trim()))
                    {
                        txtEquipmentRepairInCode.Text = string.Empty;
                    }
                }
            }
        }

        private void btnEquipmentRepairIn_Click(object sender, RoutedEventArgs e)
        {
            Selector.SelectorEquipmentRepairIn win = new Selector.SelectorEquipmentRepairIn(null);
            if (win.ShowDialog())
            {
                txtEquipmentRepairInCode.Text = win.SelectRow["EquipmentRepairInCode"].ToString().Trim();
                this.DataContext = win.SelectRow;
            }
        }

        private void UserVendor_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            FormStateChanged();
            matrix1.ItemsSource = null;
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
            string sql = @"SELECT Inp_EquipmentRepairInDetail.EquipmentRepairInDetailId,
	   Inp_EquipmentRepairInDetail.EquipmentId,
	   Bas_Equipment.EquipmentCode,
	   Bas_Equipment.EquipmentDesc,
	   Inp_EquipmentRepairInDetail.RepairTypeId,
	   Bas_RepairType.RepairTypeCode+'-'+ISNULL(Bas_RepairType.RepairTypeName,'') AS RepairTypeName,
	   Inp_EquipmentRepairInDetail.Status,
	   dbo.Ftn_GetStatusName(Inp_EquipmentRepairInDetail.Status,'EquipmentRepairInStatus') AS StatusName,
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
	   Inp_EquipmentRepairInDetail.SendRepairComment,
	   Inp_EquipmentRepairInDetail.DelegateComment,
	   Inp_EquipmentRepairInDetail.RejectDateTime,
	   Inp_EquipmentRepairInDetail.RejectComment,
	   Inp_EquipmentRepairInDetail.ReDelegateDateTime,
	   Inp_EquipmentRepairInDetail.ReDelegateComment,
	   Inp_EquipmentRepairInDetail.OutSourcingDateTime,
	   Inp_EquipmentRepairInDetail.OutSourcingComment,
	   Inp_EquipmentRepairInDetail.ScrapDateTime,
	   Inp_EquipmentRepairInDetail.ScrapComment,
	   SureUser.SysUserId AS SureUserId,
	   SureUser.SysUserCode+'-'+ISNULL(SureUser.SysUserName,'') AS SureUserName,
	   Inp_EquipmentRepairInDetail.SureDateTime,
	   Inp_EquipmentRepairInDetail.NotAgreeComment,
       0 AS EditFlag,--0：未编辑  1：委派   2：接受    3：驳回   4：抢单   5：同意   6：拒绝   7：重新委派   8：委外维修    9：报废
	   0 AS EditAssistRepairUserFlag ,--0：未编辑  1：已修改
       '' AS GetText,--抢单文本
       '' AS DelegateText,--委派文本
       '' AS ReceiveText,--接受驳回文本
       '' AS ReceiveForeground,--接受驳回文本颜色
       '' AS AgreeText,--同意拒绝文本
       '' AS AgreeForeground,--同意拒绝文本颜色
       '' AS ReDelegateText,--重新委派文本
       '' AS OutSourcingText,--委外维修文本
       '' AS ScrapText,--报废文本
	   CASE WHEN ISNULL(Inp_EquipmentRepairInDetail.Status,'')='WaitDelegate' THEN 'Visible' ELSE 'Collapsed' END AS CanGetOrder, --抢单
	   CASE WHEN ISNULL(Inp_EquipmentRepairInDetail.Status,'')='Delegate' AND Inp_EquipmentRepairInDetail.MainRepairUserId=@UserId THEN 'Visible' ELSE 'Collapsed' END AS CanReceive,--接受驳回
	   CASE WHEN ISNULL(Inp_EquipmentRepairInDetail.Status,'') IN('WaitRejectSure','WaitReDelegateSure','WaitOutSourcingSure','WaitScrapSure') THEN 'Visible' ELSE 'Collapsed' END AS CanAgree,--同意拒绝
	   CASE WHEN ISNULL(Inp_EquipmentRepairInDetail.Status,'')='WaitDelegate' THEN 'Visible' ELSE 'Collapsed' END AS CanDelegate, --委派
	   CASE WHEN ISNULL(Inp_EquipmentRepairInDetail.Status,'')!='Close' THEN 'Visible' ELSE 'Collapsed' END AS CanAddAssistRepairUser, --添加维修协助人
	   CASE WHEN ISNULL(Inp_EquipmentRepairInDetail.Status,'') IN('WaitRepair','Repair') AND Inp_EquipmentRepairInDetail.MainRepairUserId=@UserId THEN 'Visible' ELSE 'Collapsed' END AS CanReDelegate,--重新委派
	   CASE WHEN (@IsManager=1 AND ISNULL(Inp_EquipmentRepairInDetail.Status,'')='WaitDelegate') OR (@IsManager=0 AND Inp_EquipmentRepairInDetail.MainRepairUserId=@UserId AND ISNULL(Inp_EquipmentRepairInDetail.Status,'') IN('WaitRepair','Repair')) THEN 'Visible' ELSE 'Collapsed' END AS CanOutSourcing,--外协
	   CASE WHEN (@IsManager=1 AND ISNULL(Inp_EquipmentRepairInDetail.Status,'')='WaitDelegate') OR (@IsManager=0 AND Inp_EquipmentRepairInDetail.MainRepairUserId=@UserId AND ISNULL(Inp_EquipmentRepairInDetail.Status,'') IN('WaitRepair','Repair')) THEN 'Visible' ELSE 'Collapsed' END AS CanScrap--报废
FROM dbo.Inp_EquipmentRepairInDetail WITH(NOLOCK) 
LEFT JOIN dbo.Bas_Equipment WITH(NOLOCK) ON dbo.Bas_Equipment.EquipmentId = dbo.Inp_EquipmentRepairInDetail.EquipmentId
LEFT JOIN dbo.Bas_RepairType WITH(NOLOCK) ON dbo.Bas_RepairType.RepairTypeId = dbo.Inp_EquipmentRepairInDetail.RepairTypeId
LEFT JOIN dbo.Set_User MainRepairUser WITH(NOLOCK) ON Inp_EquipmentRepairInDetail.MainRepairUserId=MainRepairUser.SysUserId
LEFT JOIN dbo.Set_User DelegateUser WITH(NOLOCK) ON Inp_EquipmentRepairInDetail.DelegateUserId=DelegateUser.SysUserId
LEFT JOIN dbo.Set_User ReceiveUser WITH(NOLOCK) ON Inp_EquipmentRepairInDetail.ReceiveUserId=ReceiveUser.SysUserId
LEFT JOIN dbo.Set_User RepairCompleteUser WITH(NOLOCK) ON Inp_EquipmentRepairInDetail.RepairCompleteUserId=RepairCompleteUser.SysUserId
LEFT JOIN dbo.Set_User SureUser WITH(NOLOCK) ON Inp_EquipmentRepairInDetail.SureUserId=SureUser.SysUserId
WHERE Inp_EquipmentRepairInDetail.EquipmentRepairInId=@EquipmentRepairInId
";
            Data.Parameters parameters = new Parameters()
                    .Add("EquipmentRepairInId", row["EquipmentRepairInId"])
                    .Add("UserId", Framework.App.User.UserId)
                    .Add("IsManager", IsManager);
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

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (IsRepair && this.DataContext != null)
            {
                FormState = ViewState.Modify;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (Component.MessageBox.MyMessageBox.ShowQuestion(string.Format("确定取消吗？")) != MessageBoxResult.OK)
                return;

            FormState = ViewState.Normal;
            QueryDetail();
        }

        private void btnAllDelegate_Click(object sender, RoutedEventArgs e)
        {
            if (!IsRepair || !IsManager)
                return;

            if(matrix1.SelectedItems.Count<1)
            {
                Component.MessageBox.MyMessageBox.ShowError("请选中要委派的行。");
                return;
            }

            long? repairTypeId = null;
            long? mainRepairUserId = null;
            string delegateComment = string.Empty;

            foreach (DataRowView row in matrix1.SelectedItems)
            {
                string equipmentCode = row.Row.Field<string>("EquipmentCode");
                string status = row.Row.Field<string>("Status");
                if (status.ToUpper()!= "WaitDelegate".ToUpper())
                {
                    Component.MessageBox.MyMessageBox.ShowError(string.Format("设备[{0}]的状态为[{1}]，不能委派。",equipmentCode, row.Row.Field<string>("StatusName")));
                    return;
                }

                if (!repairTypeId.HasValue)
                {
                    repairTypeId = row.Row.Field<long?>("RepairTypeId");
                }

                if (!mainRepairUserId.HasValue)
                {
                    mainRepairUserId = row.Row.Field<long?>("MainRepairUserId");
                }

                if(string.IsNullOrEmpty(delegateComment))
                {
                    delegateComment = row.Row.Field<string>("DelegateComment");
                }
            }

            Edit.EquipmentRepairIn_EditDelegate win = new Edit.EquipmentRepairIn_EditDelegate(repairTypeId, mainRepairUserId, delegateComment);
            if(win.ShowDialog())
            {
                foreach (DataRowView row in matrix1.SelectedItems)
                {
                    row.Row["EditFlag"] = 1;
                    row.Row["RepairTypeId"] = win.RepairTypeId.Value;
                    row.Row["RepairTypeName"] = win.RepairTypeName;
                    row.Row["MainRepairUserId"] = win.MainRepairUserId.Value;
                    row.Row["MainRepairUserName"] = win.MainRepairUserName;
                    row.Row["DelegateComment"] = win.DelegateComment;
                    row.Row["DelegateText"] = "已委派";
                }
            }

        }

        private void btnDelegate_Click(object sender, RoutedEventArgs e)
        {
            DataRowView row = (sender as Component.ControlsEx.CircleImageButton).Tag as DataRowView;
            if (row == null)
                return;

            if (!IsRepair || !IsManager)
                return;

            string equipmentCode = row.Row.Field<string>("EquipmentCode");
            string status = row.Row.Field<string>("Status");
            if (status.ToUpper() != "WaitDelegate".ToUpper())
            {
                Component.MessageBox.MyMessageBox.ShowError(string.Format("设备[{0}]的状态为[{1}]，不能委派。", equipmentCode, row.Row.Field<string>("StatusName")));
                return;
            }

            long? repairTypeId = row.Row.Field<long?>("RepairTypeId");
            long? mainRepairUserId = row.Row.Field<long?>("MainRepairUserId");
            string delegateComment = row.Row.Field<string>("DelegateComment");

            Edit.EquipmentRepairIn_EditDelegate win = new Edit.EquipmentRepairIn_EditDelegate(repairTypeId, mainRepairUserId, delegateComment);
            if (win.ShowDialog())
            {
                row.Row["EditFlag"] = 1;
                row.Row["RepairTypeId"] = win.RepairTypeId.Value;
                row.Row["RepairTypeName"] = win.RepairTypeName;
                row.Row["MainRepairUserId"] = win.MainRepairUserId.Value;
                row.Row["MainRepairUserName"] = win.MainRepairUserName;
                row.Row["DelegateComment"] = win.DelegateComment;
                row.Row["DelegateText"] = "已委派";
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (Component.MessageBox.MyMessageBox.ShowQuestion(string.Format("确定保存吗？")) != MessageBoxResult.OK)
                return;

            StringBuilder delegateXml = new StringBuilder();
            List<DataRowView> delegateRows = new List<DataRowView>();

            StringBuilder receiveXml = new StringBuilder();
            List<DataRowView> receiveRows = new List<DataRowView>();

            StringBuilder rejectXml = new StringBuilder();
            List<DataRowView> rejectRows = new List<DataRowView>();

            StringBuilder getXml = new StringBuilder();
            List<DataRowView> getRows = new List<DataRowView>();

            StringBuilder agreeXml = new StringBuilder();
            List<DataRowView> agreeRows = new List<DataRowView>();

            StringBuilder notAgreeXml = new StringBuilder();
            List<DataRowView> notAgreeRows = new List<DataRowView>();

            StringBuilder reDelegateXml = new StringBuilder();
            List<DataRowView> reDelegateRows = new List<DataRowView>();

            StringBuilder outSourcingXml = new StringBuilder();
            List<DataRowView> outSourcingRows = new List<DataRowView>();

            StringBuilder assistRepairUserXml = new StringBuilder();
            List<DataRowView> assistRepairUserRows = new List<DataRowView>();

            StringBuilder scrapXml = new StringBuilder();
            List<DataRowView> scrapRows = new List<DataRowView>();



            delegateXml.Append(string.Format("<xml>"));
            receiveXml.Append(string.Format("<xml>"));
            rejectXml.Append(string.Format("<xml>"));
            getXml.Append(string.Format("<xml>"));
            agreeXml.Append(string.Format("<xml>"));
            notAgreeXml.Append(string.Format("<xml>"));
            reDelegateXml.Append(string.Format("<xml>"));
            outSourcingXml.Append(string.Format("<xml>"));
            assistRepairUserXml.Append(string.Format("<xml>"));
            scrapXml.Append(string.Format("<xml>"));

            foreach (DataRowView row in (matrix1.ItemsSource as DataView) )
            {
                if (row.Row.Field<int>("EditFlag") == 1)
                {
                    delegateRows.Add(row);
                    delegateXml.Append(string.Format("<row EquipmentRepairInDetailId=\"{0}\" RepairTypeId=\"{1}\" MainRepairUserId=\"{2}\" DelegateComment=\"{3}\"/>",
                        row["EquipmentRepairInDetailId"],
                        row["RepairTypeId"],
                        row["MainRepairUserId"],
                        WinAPI.File.XMLHelper.StringFormat(row["DelegateComment"].ToString())));
                }
                else if (row.Row.Field<int>("EditFlag") == 2)
                {
                    receiveRows.Add(row);
                    receiveXml.Append(string.Format("<row EquipmentRepairInDetailId=\"{0}\" MainRepairUserId=\"{1}\"/>",
                        row["EquipmentRepairInDetailId"],
                        row["MainRepairUserId"]));
                }
                else if (row.Row.Field<int>("EditFlag") == 3)
                {
                    rejectRows.Add(row);
                    rejectXml.Append(string.Format("<row EquipmentRepairInDetailId=\"{0}\" MainRepairUserId=\"{1}\" RejectComment=\"{2}\"/>",
                        row["EquipmentRepairInDetailId"],
                        row["MainRepairUserId"],
                        WinAPI.File.XMLHelper.StringFormat(row["RejectComment"].ToString())));
                }
                else if (row.Row.Field<int>("EditFlag") == 4)
                {
                    getRows.Add(row);
                    getXml.Append(string.Format("<row EquipmentRepairInDetailId=\"{0}\" RepairTypeId=\"{1}\" />",
                        row["EquipmentRepairInDetailId"],
                        row["RepairTypeId"]));
                }
                else if (row.Row.Field<int>("EditFlag") == 5)
                {
                    agreeRows.Add(row);
                    agreeXml.Append(string.Format("<row EquipmentRepairInDetailId=\"{0}\"  />",
                        row["EquipmentRepairInDetailId"]));
                }
                else if (row.Row.Field<int>("EditFlag") == 6)
                {
                    notAgreeRows.Add(row);
                    notAgreeXml.Append(string.Format("<row EquipmentRepairInDetailId=\"{0}\"  NotAgreeComment=\"{1}\"/>",
                        row["EquipmentRepairInDetailId"],
                        WinAPI.File.XMLHelper.StringFormat(row["NotAgreeComment"].ToString())));
                }
                else if (row.Row.Field<int>("EditFlag") == 7)
                {
                    reDelegateRows.Add(row);
                    reDelegateXml.Append(string.Format("<row EquipmentRepairInDetailId=\"{0}\"  ReDelegateComment=\"{1}\"/>",
                        row["EquipmentRepairInDetailId"],
                        WinAPI.File.XMLHelper.StringFormat(row["ReDelegateComment"].ToString())));
                }
                else if (row.Row.Field<int>("EditFlag") == 8)
                {
                    outSourcingRows.Add(row);
                    outSourcingXml.Append(string.Format("<row EquipmentRepairInDetailId=\"{0}\"  OutSourcingComment=\"{1}\"/>",
                        row["EquipmentRepairInDetailId"],
                        WinAPI.File.XMLHelper.StringFormat(row["OutSourcingComment"].ToString())));
                }
                else if (row.Row.Field<int>("EditFlag") == 9)
                {
                    scrapRows.Add(row);
                    scrapXml.Append(string.Format("<row EquipmentRepairInDetailId=\"{0}\"  ScrapComment=\"{1}\"/>",
                        row["EquipmentRepairInDetailId"],
                        WinAPI.File.XMLHelper.StringFormat(row["ScrapComment"].ToString())));
                }


                if (row.Row.Field<int>("EditAssistRepairUserFlag") == 1)
                {
                    assistRepairUserRows.Add(row);
                    assistRepairUserXml.Append(string.Format("<row EquipmentRepairInDetailId=\"{0}\"  AssistRepairUserId=\"{1}\"/>",
                        row["EquipmentRepairInDetailId"],
                        WinAPI.File.XMLHelper.StringFormat(row["AssistRepairUserId"].ToString())));
                }
            }
            delegateXml.Append(string.Format("</xml>"));
            receiveXml.Append(string.Format("</xml>"));
            rejectXml.Append(string.Format("</xml>"));
            getXml.Append(string.Format("</xml>"));
            agreeXml.Append(string.Format("</xml>"));
            notAgreeXml.Append(string.Format("</xml>"));
            reDelegateXml.Append(string.Format("</xml>"));
            outSourcingXml.Append(string.Format("</xml>"));
            assistRepairUserXml.Append(string.Format("</xml>"));
            scrapXml.Append(string.Format("</xml>"));

            if (delegateRows.Count>0)
            {
                #region
                string sql = "Prd_Equipment_Repair_Delegate";
                Data.Parameters parameters = new Parameters()
                    .Add("EquipmentRepairInId", (this.DataContext as DataRowView)["EquipmentRepairInId"])
                    .Add("Xml", delegateXml.ToString(), SqlDbType.Xml, int.MaxValue)
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
                    foreach (DataRowView row in delegateRows)
                    {
                        row.Row["EditFlag"] = 0;
                        row.Row["DelegateText"] = "";
                    }
                }
                else
                {
                    Component.MessageBox.MyMessageBox.ShowError(parameters["Return_Message"].ToString());
                    return;
                }
                #endregion
            }

            if (receiveRows.Count > 0)
            {
                #region
                string sql = "Prd_Equipment_Repair_Receive";
                Data.Parameters parameters = new Parameters()
                    .Add("EquipmentRepairInId", (this.DataContext as DataRowView)["EquipmentRepairInId"])
                    .Add("Xml", receiveXml.ToString(), SqlDbType.Xml, int.MaxValue)
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
                    foreach (DataRowView row in receiveRows)
                    {
                        row.Row["EditFlag"] = 0;
                        row.Row["ReceiveText"] = "";
                        row.Row["ReceiveForeground"] = "";
                    }
                }
                else
                {
                    Component.MessageBox.MyMessageBox.ShowError(parameters["Return_Message"].ToString());
                    return;
                }
                #endregion
            }

            if (rejectRows.Count > 0)
            {
                #region
                string sql = "Prd_Equipment_Repair_Reject";
                Data.Parameters parameters = new Parameters()
                    .Add("EquipmentRepairInId", (this.DataContext as DataRowView)["EquipmentRepairInId"])
                    .Add("Xml", rejectXml.ToString(), SqlDbType.Xml, int.MaxValue)
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
                    foreach (DataRowView row in rejectRows)
                    {
                        row.Row["EditFlag"] = 0;
                        row.Row["ReceiveText"] = "";
                        row.Row["ReceiveForeground"] = "";
                    }
                }
                else
                {
                    Component.MessageBox.MyMessageBox.ShowError(parameters["Return_Message"].ToString());
                    return;
                }
                #endregion
            }

            if (getRows.Count > 0)
            {
                #region
                string sql = "Prd_Equipment_Repair_Get";
                Data.Parameters parameters = new Parameters()
                    .Add("EquipmentRepairInId", (this.DataContext as DataRowView)["EquipmentRepairInId"])
                    .Add("Xml", getXml.ToString(), SqlDbType.Xml, int.MaxValue)
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
                    foreach (DataRowView row in getRows)
                    {
                        row.Row["EditFlag"] = 0;
                        row.Row["GetText"] = "";
                    }
                }
                else
                {
                    Component.MessageBox.MyMessageBox.ShowError(parameters["Return_Message"].ToString());
                    return;
                }
                #endregion
            }

            if (agreeRows.Count > 0)
            {
                #region
                string sql = "Prd_Equipment_Repair_Agree";
                Data.Parameters parameters = new Parameters()
                    .Add("EquipmentRepairInId", (this.DataContext as DataRowView)["EquipmentRepairInId"])
                    .Add("Xml", agreeXml.ToString(), SqlDbType.Xml, int.MaxValue)
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
                    foreach (DataRowView row in agreeRows)
                    {
                        row.Row["EditFlag"] = 0;
                        row.Row["AgreeText"] = "";
                        row.Row["AgreeForeground"] = "";
                    }
                }
                else
                {
                    Component.MessageBox.MyMessageBox.ShowError(parameters["Return_Message"].ToString());
                    return;
                }
                #endregion
            }

            if (notAgreeRows.Count > 0)
            {
                #region
                string sql = "Prd_Equipment_Repair_NotAgree";
                Data.Parameters parameters = new Parameters()
                    .Add("EquipmentRepairInId", (this.DataContext as DataRowView)["EquipmentRepairInId"])
                    .Add("Xml", notAgreeXml.ToString(), SqlDbType.Xml, int.MaxValue)
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
                    foreach (DataRowView row in notAgreeRows)
                    {
                        row.Row["EditFlag"] = 0;
                        row.Row["AgreeText"] = "";
                        row.Row["AgreeForeground"] = "";
                    }
                }
                else
                {
                    Component.MessageBox.MyMessageBox.ShowError(parameters["Return_Message"].ToString());
                    return;
                }
                #endregion
            }

            if (reDelegateRows.Count > 0)
            {
                #region
                string sql = "Prd_Equipment_Repair_ReDelegate";
                Data.Parameters parameters = new Parameters()
                    .Add("EquipmentRepairInId", (this.DataContext as DataRowView)["EquipmentRepairInId"])
                    .Add("Xml", reDelegateXml.ToString(), SqlDbType.Xml, int.MaxValue)
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
                    foreach (DataRowView row in reDelegateRows)
                    {
                        row.Row["EditFlag"] = 0;
                        row.Row["ReDelegateText"] = "";
                    }
                }
                else
                {
                    Component.MessageBox.MyMessageBox.ShowError(parameters["Return_Message"].ToString());
                    return;
                }
                #endregion
            }

            if (outSourcingRows.Count > 0)
            {
                #region
                string sql = "Prd_Equipment_Repair_OutSourcing";
                Data.Parameters parameters = new Parameters()
                    .Add("EquipmentRepairInId", (this.DataContext as DataRowView)["EquipmentRepairInId"])
                    .Add("IsManager", IsManager)
                    .Add("Xml", outSourcingXml.ToString(), SqlDbType.Xml, int.MaxValue)
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
                    foreach (DataRowView row in outSourcingRows)
                    {
                        row.Row["EditFlag"] = 0;
                        row.Row["OutSourcingText"] = "";
                    }
                }
                else
                {
                    Component.MessageBox.MyMessageBox.ShowError(parameters["Return_Message"].ToString());
                    return;
                }
                #endregion
            }

            if (assistRepairUserRows.Count > 0)
            {
                #region
                string sql = "Prd_Equipment_Repair_AssistRepairUser";
                Data.Parameters parameters = new Parameters()
                    .Add("EquipmentRepairInId", (this.DataContext as DataRowView)["EquipmentRepairInId"])
                    .Add("Xml", assistRepairUserXml.ToString(), SqlDbType.Xml, int.MaxValue)
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
                    foreach (DataRowView row in assistRepairUserRows)
                    {
                        row.Row["EditAssistRepairUserFlag"] = 0;
                    }
                }
                else
                {
                    Component.MessageBox.MyMessageBox.ShowError(parameters["Return_Message"].ToString());
                    return;
                }
                #endregion
            }

            if (scrapRows.Count > 0)
            {
                #region
                string sql = "Prd_Equipment_Repair_Scrap";
                Data.Parameters parameters = new Parameters()
                    .Add("EquipmentRepairInId", (this.DataContext as DataRowView)["EquipmentRepairInId"])
                    .Add("IsManager", IsManager)
                    .Add("Xml", scrapXml.ToString(), SqlDbType.Xml, int.MaxValue)
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
                    foreach (DataRowView row in scrapRows)
                    {
                        row.Row["EditFlag"] = 0;
                        row.Row["ScrapText"] = "";
                    }
                }
                else
                {
                    Component.MessageBox.MyMessageBox.ShowError(parameters["Return_Message"].ToString());
                    return;
                }
                #endregion
            }


            Component.MessageBox.MyMessageBox.Show("保存成功。");


            FormState = ViewState.Normal;
            QueryDetail();
            return;


        }

        private void btnAllReceive_Click(object sender, RoutedEventArgs e)
        {
            if (!IsRepair || IsManager)
                return;

            if (matrix1.SelectedItems.Count < 1)
            {
                Component.MessageBox.MyMessageBox.ShowError("请选中要接受的行。");
                return;
            }


            if (Component.MessageBox.MyMessageBox.ShowQuestion(string.Format("确定接受吗？")) != MessageBoxResult.OK)
                return;

            foreach (DataRowView row in matrix1.SelectedItems)
            {
                string equipmentCode = row.Row.Field<string>("EquipmentCode");
                string status = row.Row.Field<string>("Status");
                if (status.ToUpper() != "Delegate".ToUpper())
                {
                    Component.MessageBox.MyMessageBox.ShowError(string.Format("设备[{0}]的状态为[{1}]，不能接受。", equipmentCode, row.Row.Field<string>("StatusName")));
                    return;
                }
            }

            foreach (DataRowView row in matrix1.SelectedItems)
            {
                row.Row["EditFlag"] = 2;
                row.Row["ReceiveText"] = "已接受";
                row.Row["ReceiveForeground"] = "Green";
            }
        }

        private void btnAllReject_Click(object sender, RoutedEventArgs e)
        {
            if (!IsRepair || IsManager)
                return;

            if (matrix1.SelectedItems.Count < 1)
            {
                Component.MessageBox.MyMessageBox.ShowError("请选中要驳回的行。");
                return;
            }

            string rejectComment = string.Empty;

            foreach (DataRowView row in matrix1.SelectedItems)
            {
                string equipmentCode = row.Row.Field<string>("EquipmentCode");
                string status = row.Row.Field<string>("Status");
                if (status.ToUpper() != "Delegate".ToUpper())
                {
                    Component.MessageBox.MyMessageBox.ShowError(string.Format("设备[{0}]的状态为[{1}]，不能驳回。", equipmentCode, row.Row.Field<string>("StatusName")));
                    return;
                }

                if (string.IsNullOrEmpty(rejectComment))
                {
                    rejectComment = row.Row.Field<string>("RejectComment");
                }
            }

            Edit.EquipmentRepairIn_EditReject win = new Edit.EquipmentRepairIn_EditReject(rejectComment);
            if (win.ShowDialog())
            {
                foreach (DataRowView row in matrix1.SelectedItems)
                {
                    row.Row["EditFlag"] = 3;
                    row.Row["ReceiveText"] = "已驳回";
                    row.Row["ReceiveForeground"] = "Red";
                    row.Row["RejectComment"] = win.RejectComment;
                }
            }
        }

        private void btnReceive_Click(object sender, RoutedEventArgs e)
        {
            DataRowView row = (sender as Component.ControlsEx.CircleImageButton).Tag as DataRowView;
            if (row == null)
                return;

            if (!IsRepair || IsManager)
                return;

            string equipmentCode = row.Row.Field<string>("EquipmentCode");
            string status = row.Row.Field<string>("Status");
            if (status.ToUpper() != "Delegate".ToUpper())
            {
                Component.MessageBox.MyMessageBox.ShowError(string.Format("设备[{0}]的状态为[{1}]，不能接受。", equipmentCode, row.Row.Field<string>("StatusName")));
                return;
            }

            if (Component.MessageBox.MyMessageBox.ShowQuestion(string.Format("确定接受吗？")) != MessageBoxResult.OK)
                return;

            row.Row["EditFlag"] = 2;
            row.Row["ReceiveText"] = "已接受";
            row.Row["ReceiveForeground"] = "Green";
        }

        private void btnReject_Click(object sender, RoutedEventArgs e)
        {
            DataRowView row = (sender as Component.ControlsEx.CircleImageButton).Tag as DataRowView;
            if (row == null)
                return;

            if (!IsRepair || IsManager)
                return;

            string equipmentCode = row.Row.Field<string>("EquipmentCode");
            string status = row.Row.Field<string>("Status");
            if (status.ToUpper() != "Delegate".ToUpper())
            {
                Component.MessageBox.MyMessageBox.ShowError(string.Format("设备[{0}]的状态为[{1}]，不能驳回。", equipmentCode, row.Row.Field<string>("StatusName")));
                return;
            }


            string rejectComment = row.Row.Field<string>("RejectComment");

            Edit.EquipmentRepairIn_EditReject win = new Edit.EquipmentRepairIn_EditReject(rejectComment);
            if (win.ShowDialog())
            {
                row.Row["EditFlag"] = 3;
                row.Row["ReceiveText"] = "已驳回";
                row.Row["ReceiveForeground"] = "Red";
                row.Row["RejectComment"] = win.RejectComment;
            }
        }

        private void btnAllGet_Click(object sender, RoutedEventArgs e)
        {
            if (!IsRepair || IsManager)
                return;

            if (matrix1.SelectedItems.Count < 1)
            {
                Component.MessageBox.MyMessageBox.ShowError("请选中要抢单的行。");
                return;
            }


            if (Component.MessageBox.MyMessageBox.ShowQuestion(string.Format("确定抢单吗？")) != MessageBoxResult.OK)
                return;

            foreach (DataRowView row in matrix1.SelectedItems)
            {
                string equipmentCode = row.Row.Field<string>("EquipmentCode");
                string status = row.Row.Field<string>("Status");
                if (status.ToUpper() != "WaitDelegate".ToUpper())
                {
                    Component.MessageBox.MyMessageBox.ShowError(string.Format("设备[{0}]的状态为[{1}]，不能抢单。", equipmentCode, row.Row.Field<string>("StatusName")));
                    return;
                }
            }

            foreach (DataRowView row in matrix1.SelectedItems)
            {
                row.Row["EditFlag"] = 4;
                row.Row["GetText"] = "已抢单";
            }
        }

        private void btnGet_Click(object sender, RoutedEventArgs e)
        {
            DataRowView row = (sender as Component.ControlsEx.CircleImageButton).Tag as DataRowView;
            if (row == null)
                return;

            if (!IsRepair || IsManager)
                return;

            string equipmentCode = row.Row.Field<string>("EquipmentCode");
            string status = row.Row.Field<string>("Status");
            if (status.ToUpper() != "WaitDelegate".ToUpper())
            {
                Component.MessageBox.MyMessageBox.ShowError(string.Format("设备[{0}]的状态为[{1}]，不能抢单。", equipmentCode, row.Row.Field<string>("StatusName")));
                return;
            }

            if (Component.MessageBox.MyMessageBox.ShowQuestion(string.Format("确定抢单吗？")) != MessageBoxResult.OK)
                return;

            row.Row["EditFlag"] = 4;
            row.Row["GetText"] = "已抢单";
        }

        private void btnAllAgree_Click(object sender, RoutedEventArgs e)
        {
            if (!IsRepair || !IsManager)
                return;

            if (matrix1.SelectedItems.Count < 1)
            {
                Component.MessageBox.MyMessageBox.ShowError("请选中要同意的行。");
                return;
            }


            if (Component.MessageBox.MyMessageBox.ShowQuestion(string.Format("确定同意吗？")) != MessageBoxResult.OK)
                return;

            foreach (DataRowView row in matrix1.SelectedItems)
            {
                string equipmentCode = row.Row.Field<string>("EquipmentCode");
                string status = row.Row.Field<string>("Status");
                if (status.ToUpper() != "WaitRejectSure".ToUpper()&& status.ToUpper() != "WaitReDelegateSure".ToUpper()&& status.ToUpper() != "WaitOutSourcingSure".ToUpper() && status.ToUpper() != "WaitScrapSure".ToUpper())
                {
                    Component.MessageBox.MyMessageBox.ShowError(string.Format("设备[{0}]的状态为[{1}]，不能同意。", equipmentCode, row.Row.Field<string>("StatusName")));
                    return;
                }
            }

            foreach (DataRowView row in matrix1.SelectedItems)
            {
                row.Row["EditFlag"] = 5;
                row.Row["AgreeText"] = "已同意";
                row.Row["AgreeForeground"] = "Green";
            }
        }

        private void btnAgree_Click(object sender, RoutedEventArgs e)
        {
            DataRowView row = (sender as Component.ControlsEx.CircleImageButton).Tag as DataRowView;
            if (row == null)
                return;

            if (!IsRepair || !IsManager)
                return;

            string equipmentCode = row.Row.Field<string>("EquipmentCode");
            string status = row.Row.Field<string>("Status");
            if (status.ToUpper() != "WaitRejectSure".ToUpper() && status.ToUpper() != "WaitReDelegateSure".ToUpper() && status.ToUpper() != "WaitOutSourcingSure".ToUpper() && status.ToUpper() != "WaitScrapSure".ToUpper())
            {
                Component.MessageBox.MyMessageBox.ShowError(string.Format("设备[{0}]的状态为[{1}]，不能同意。", equipmentCode, row.Row.Field<string>("StatusName")));
                return;
            }

            if (Component.MessageBox.MyMessageBox.ShowQuestion(string.Format("确定同意吗？")) != MessageBoxResult.OK)
                return;

            row.Row["EditFlag"] = 5;
            row.Row["AgreeText"] = "已同意";
            row.Row["AgreeForeground"] = "Green";
        }

        private void btnAllNotAgree_Click(object sender, RoutedEventArgs e)
        {
            if (!IsRepair || !IsManager)
                return;

            if (matrix1.SelectedItems.Count < 1)
            {
                Component.MessageBox.MyMessageBox.ShowError("请选中要拒绝的行。");
                return;
            }

            string notAgreeComment = string.Empty;

            foreach (DataRowView row in matrix1.SelectedItems)
            {
                string equipmentCode = row.Row.Field<string>("EquipmentCode");
                string status = row.Row.Field<string>("Status");
                if (status.ToUpper() != "WaitRejectSure".ToUpper() && status.ToUpper() != "WaitReDelegateSure".ToUpper() && status.ToUpper() != "WaitOutSourcingSure".ToUpper() && status.ToUpper() != "WaitScrapSure".ToUpper())
                {
                    Component.MessageBox.MyMessageBox.ShowError(string.Format("设备[{0}]的状态为[{1}]，不能拒绝。", equipmentCode, row.Row.Field<string>("StatusName")));
                    return;
                }

                if (string.IsNullOrEmpty(notAgreeComment))
                {
                    notAgreeComment = row.Row.Field<string>("NotAgreeComment");
                }
            }

            Edit.EquipmentRepairIn_EditNotAgree win = new Edit.EquipmentRepairIn_EditNotAgree(notAgreeComment);
            if (win.ShowDialog())
            {
                foreach (DataRowView row in matrix1.SelectedItems)
                {
                    row.Row["EditFlag"] = 6;
                    row.Row["AgreeText"] = "已拒绝";
                    row.Row["AgreeForeground"] = "Red";
                    row.Row["NotAgreeComment"] = win.NotAgreeComment;
                }
            }
        }

        private void btnNotAgree_Click(object sender, RoutedEventArgs e)
        {
            DataRowView row = (sender as Component.ControlsEx.CircleImageButton).Tag as DataRowView;
            if (row == null)
                return;

            if (!IsRepair || !IsManager)
                return;

            string equipmentCode = row.Row.Field<string>("EquipmentCode");
            string status = row.Row.Field<string>("Status");
            if (status.ToUpper() != "WaitRejectSure".ToUpper() && status.ToUpper() != "WaitReDelegateSure".ToUpper() && status.ToUpper() != "WaitOutSourcingSure".ToUpper() && status.ToUpper() != "WaitScrapSure".ToUpper())
            {
                Component.MessageBox.MyMessageBox.ShowError(string.Format("设备[{0}]的状态为[{1}]，不能拒绝。", equipmentCode, row.Row.Field<string>("StatusName")));
                return;
            }


            string notAgreeComment = row.Row.Field<string>("NotAgreeComment");

            Edit.EquipmentRepairIn_EditNotAgree win = new Edit.EquipmentRepairIn_EditNotAgree(notAgreeComment);
            if (win.ShowDialog())
            {
                row.Row["EditFlag"] = 6;
                row.Row["AgreeText"] = "已拒绝";
                row.Row["AgreeForeground"] = "Red";
                row.Row["NotAgreeComment"] = win.NotAgreeComment;
            }
        }

        private void btnAllReDelegate_Click(object sender, RoutedEventArgs e)
        {
            if (!IsRepair || IsManager)
                return;

            if (matrix1.SelectedItems.Count < 1)
            {
                Component.MessageBox.MyMessageBox.ShowError("请选中要重新委派的行。");
                return;
            }

            string reDelegateComment = string.Empty;

            foreach (DataRowView row in matrix1.SelectedItems)
            {
                string equipmentCode = row.Row.Field<string>("EquipmentCode");
                string status = row.Row.Field<string>("Status");
                if (status.ToUpper() != "WaitRepair".ToUpper()&& status.ToUpper() != "Repair".ToUpper())
                {
                    Component.MessageBox.MyMessageBox.ShowError(string.Format("设备[{0}]的状态为[{1}]，不能重新委派。", equipmentCode, row.Row.Field<string>("StatusName")));
                    return;
                }

                if (string.IsNullOrEmpty(reDelegateComment))
                {
                    reDelegateComment = row.Row.Field<string>("ReDelegateComment");
                }
            }

            Edit.EquipmentRepairIn_EditReDelegate win = new Edit.EquipmentRepairIn_EditReDelegate(reDelegateComment);
            if (win.ShowDialog())
            {
                foreach (DataRowView row in matrix1.SelectedItems)
                {
                    row.Row["EditFlag"] = 7;
                    row.Row["ReDelegateText"] = "已重新委派";
                    row.Row["ReDelegateComment"] = win.ReDelegateComment;
                }
            }
        }

        private void btnReDelegate_Click(object sender, RoutedEventArgs e)
        {
            DataRowView row = (sender as Component.ControlsEx.CircleImageButton).Tag as DataRowView;
            if (row == null)
                return;

            if (!IsRepair || IsManager)
                return;

            string equipmentCode = row.Row.Field<string>("EquipmentCode");
            string status = row.Row.Field<string>("Status");
            if (status.ToUpper() != "WaitRepair".ToUpper() && status.ToUpper() != "Repair".ToUpper())
            {
                Component.MessageBox.MyMessageBox.ShowError(string.Format("设备[{0}]的状态为[{1}]，不能重新委派。", equipmentCode, row.Row.Field<string>("StatusName")));
                return;
            }


            string reDelegateComment = row.Row.Field<string>("ReDelegateComment");

            Edit.EquipmentRepairIn_EditReDelegate win = new Edit.EquipmentRepairIn_EditReDelegate(reDelegateComment);
            if (win.ShowDialog())
            {
                row.Row["EditFlag"] = 7;
                row.Row["ReDelegateText"] = "已重新委派";
                row.Row["ReDelegateComment"] = win.ReDelegateComment;
            }
        }

        private void btnAllOutSourcing_Click(object sender, RoutedEventArgs e)
        {
            if (!IsRepair)
                return;

            if (matrix1.SelectedItems.Count < 1)
            {
                Component.MessageBox.MyMessageBox.ShowError("请选中要委外维修的行。");
                return;
            }

            string outSourcingComment = string.Empty;

            foreach (DataRowView row in matrix1.SelectedItems)
            {
                string equipmentCode = row.Row.Field<string>("EquipmentCode");
                string status = row.Row.Field<string>("Status");
                if(IsManager)
                {
                    if (status.ToUpper() != "WaitDelegate".ToUpper())
                    {
                        Component.MessageBox.MyMessageBox.ShowError(string.Format("设备[{0}]的状态为[{1}]，不能委外维修。", equipmentCode, row.Row.Field<string>("StatusName")));
                        return;
                    }
                }
                else
                {
                    if (status.ToUpper() != "WaitRepair".ToUpper() && status.ToUpper() != "Repair".ToUpper())
                    {
                        Component.MessageBox.MyMessageBox.ShowError(string.Format("设备[{0}]的状态为[{1}]，不能委外维修。", equipmentCode, row.Row.Field<string>("StatusName")));
                        return;
                    }
                }

                if (string.IsNullOrEmpty(outSourcingComment))
                {
                    outSourcingComment = row.Row.Field<string>("OutSourcingComment");
                }
            }

            Edit.EquipmentRepairIn_EditOutSourcing win = new Edit.EquipmentRepairIn_EditOutSourcing(outSourcingComment);
            if (win.ShowDialog())
            {
                foreach (DataRowView row in matrix1.SelectedItems)
                {
                    row.Row["EditFlag"] = 8;
                    row.Row["OutSourcingText"] = "已委外维修";
                    row.Row["OutSourcingComment"] = win.OutSourcingComment;
                }
            }
        }

        private void btnOutSourcing_Click(object sender, RoutedEventArgs e)
        {
            DataRowView row = (sender as Component.ControlsEx.CircleImageButton).Tag as DataRowView;
            if (row == null)
                return;

            if (!IsRepair )
                return;

            string equipmentCode = row.Row.Field<string>("EquipmentCode");
            string status = row.Row.Field<string>("Status");
            if (IsManager)
            {
                if (status.ToUpper() != "WaitDelegate".ToUpper())
                {
                    Component.MessageBox.MyMessageBox.ShowError(string.Format("设备[{0}]的状态为[{1}]，不能委外维修。", equipmentCode, row.Row.Field<string>("StatusName")));
                    return;
                }
            }
            else
            {
                if (status.ToUpper() != "WaitRepair".ToUpper() && status.ToUpper() != "Repair".ToUpper())
                {
                    Component.MessageBox.MyMessageBox.ShowError(string.Format("设备[{0}]的状态为[{1}]，不能委外维修。", equipmentCode, row.Row.Field<string>("StatusName")));
                    return;
                }
            }


            string outSourcingComment = row.Row.Field<string>("OutSourcingComment");

            Edit.EquipmentRepairIn_EditOutSourcing win = new Edit.EquipmentRepairIn_EditOutSourcing(outSourcingComment);
            if (win.ShowDialog())
            {
                row.Row["EditFlag"] = 8;
                row.Row["OutSourcingText"] = "已委外维修";
                row.Row["OutSourcingComment"] = win.OutSourcingComment;
            }
        }

        private void btnAllAddAssistRepairUser_Click(object sender, RoutedEventArgs e)
        {
            if (!IsRepair || !IsManager)
                return;

            if (matrix1.SelectedItems.Count < 1)
            {
                Component.MessageBox.MyMessageBox.ShowError("请选中要编辑协助维修人的行。");
                return;
            }

            long? repairTypeId = null;

            foreach (DataRowView row in matrix1.SelectedItems)
            {
                string equipmentCode = row.Row.Field<string>("EquipmentCode");
                string status = row.Row.Field<string>("Status");
                if (status.ToUpper() == "Close".ToUpper())
                {
                    Component.MessageBox.MyMessageBox.ShowError(string.Format("设备[{0}]的状态为[{1}]，不能编辑协助维修人。", equipmentCode, row.Row.Field<string>("StatusName")));
                    return;
                }

                if (!repairTypeId.HasValue)
                {
                    repairTypeId = row.Row.Field<long?>("RepairTypeId");
                }
            }

            Edit.EquipmentRepairIn_AddAssistRepairUser win = new Edit.EquipmentRepairIn_AddAssistRepairUser(null,null, repairTypeId);
            if (win.ShowDialog())
            {
                foreach (DataRowView row in matrix1.SelectedItems)
                {
                    row.Row["EditAssistRepairUserFlag"] = 1;
                    row.Row["AssistRepairUserId"] = win.Ids;
                    row.Row["AssistRepairUserName"] = win.Codes;
                }
            }
        }

        private void btnAddAssistRepairUser_Click(object sender, RoutedEventArgs e)
        {
            DataRowView row = (sender as Component.ControlsEx.CircleImageButton).Tag as DataRowView;
            if (row == null)
                return;

            if (!IsRepair || !IsManager)
                return;

            string equipmentCode = row.Row.Field<string>("EquipmentCode");
            string status = row.Row.Field<string>("Status");
            if (status.ToUpper() == "Close".ToUpper())
            {
                Component.MessageBox.MyMessageBox.ShowError(string.Format("设备[{0}]的状态为[{1}]，不能编辑协助维修人。", equipmentCode, row.Row.Field<string>("StatusName")));
                return;
            }


            long? repairTypeId = row.Row.Field<long?>("RepairTypeId");

            Edit.EquipmentRepairIn_AddAssistRepairUser win = new Edit.EquipmentRepairIn_AddAssistRepairUser(row.Row.Field<string>("AssistRepairUserId"), row.Row.Field<string>("AssistRepairUserName"), repairTypeId);
            if (win.ShowDialog())
            {
                row.Row["EditAssistRepairUserFlag"] = 1;
                row.Row["AssistRepairUserId"] = win.Ids;
                row.Row["AssistRepairUserName"] = win.Codes;
            }
        }

        private void GridImageColumn_Click(object value, object parameter)
        {
            DataRowView row = parameter as DataRowView;
            Data.Parameters parameters = new Data.Parameters().Add("EquipmentRepairInDetailId", row["EquipmentRepairInDetailId"]);
            Component.App.Portal.OpenPlugin("EquipmentRepair", string.Format(" - {0}", row["EquipmentCode"]), parameters, row["EquipmentCode"].ToString());
        }

        private void btnAllScrap_Click(object sender, RoutedEventArgs e)
        {
            if (!IsRepair)
                return;

            if (matrix1.SelectedItems.Count < 1)
            {
                Component.MessageBox.MyMessageBox.ShowError("请选中要报废的行。");
                return;
            }

            string scrapComment = string.Empty;

            foreach (DataRowView row in matrix1.SelectedItems)
            {
                string equipmentCode = row.Row.Field<string>("EquipmentCode");
                string status = row.Row.Field<string>("Status");
                if (IsManager)
                {
                    if (status.ToUpper() != "WaitDelegate".ToUpper())
                    {
                        Component.MessageBox.MyMessageBox.ShowError(string.Format("设备[{0}]的状态为[{1}]，不能报废。", equipmentCode, row.Row.Field<string>("StatusName")));
                        return;
                    }
                }
                else
                {
                    if (status.ToUpper() != "WaitRepair".ToUpper() && status.ToUpper() != "Repair".ToUpper())
                    {
                        Component.MessageBox.MyMessageBox.ShowError(string.Format("设备[{0}]的状态为[{1}]，不能报废。", equipmentCode, row.Row.Field<string>("StatusName")));
                        return;
                    }
                }

                if (string.IsNullOrEmpty(scrapComment))
                {
                    scrapComment = row.Row.Field<string>("ScrapComment");
                }
            }

            Edit.EquipmentRepairIn_EditScrap win = new Edit.EquipmentRepairIn_EditScrap(scrapComment);
            if (win.ShowDialog())
            {
                foreach (DataRowView row in matrix1.SelectedItems)
                {
                    row.Row["EditFlag"] = 9;
                    row.Row["ScrapText"] = "已报废";
                    row.Row["ScrapComment"] = win.ScrapComment;
                }
            }
        }

        private void btnScrap_Click(object sender, RoutedEventArgs e)
        {
            DataRowView row = (sender as Component.ControlsEx.CircleImageButton).Tag as DataRowView;
            if (row == null)
                return;

            if (!IsRepair)
                return;

            string equipmentCode = row.Row.Field<string>("EquipmentCode");
            string status = row.Row.Field<string>("Status");
            if (IsManager)
            {
                if (status.ToUpper() != "WaitDelegate".ToUpper())
                {
                    Component.MessageBox.MyMessageBox.ShowError(string.Format("设备[{0}]的状态为[{1}]，不能报废。", equipmentCode, row.Row.Field<string>("StatusName")));
                    return;
                }
            }
            else
            {
                if (status.ToUpper() != "WaitRepair".ToUpper() && status.ToUpper() != "Repair".ToUpper())
                {
                    Component.MessageBox.MyMessageBox.ShowError(string.Format("设备[{0}]的状态为[{1}]，不能报废。", equipmentCode, row.Row.Field<string>("StatusName")));
                    return;
                }
            }


            string scrapComment = row.Row.Field<string>("ScrapComment");

            Edit.EquipmentRepairIn_EditScrap win = new Edit.EquipmentRepairIn_EditScrap(scrapComment);
            if (win.ShowDialog())
            {
                row.Row["EditFlag"] = 9;
                row.Row["ScrapText"] = "已报废";
                row.Row["ScrapComment"] = win.ScrapComment;
            }
        }
    }

    public class VisibilityConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return Visibility.Collapsed;
            Visibility v;
            if(Enum.TryParse<Visibility>(value.ToString(),out v))
            {
                return v;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BrushesConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return Brushes.Black;

            string color = value.ToString().Trim();
            if(color==string.Empty)
            {
                return Brushes.Black;
            }

            try
            {
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
            }
            catch
            {
                return Brushes.Black;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
