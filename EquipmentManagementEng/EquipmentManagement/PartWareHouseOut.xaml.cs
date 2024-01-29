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
    /// PartWareHouseOut.xaml 的交互逻辑
    /// </summary>
    public partial class PartWareHouseOut : Component.Controls.User.UserVendor
    {
        private Data.ViewState _FormState = Data.ViewState.Normal;
        private Data.ViewState FormState
        {
            get
            {
                return _FormState;
            }
            set
            {
                if (value != _FormState)
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

        private void FormStateChanged()
        {
            if (FormState == Data.ViewState.Add)
            {
                btnAdd.Visibility = Visibility.Collapsed;
                btnModify.Visibility = Visibility.Collapsed;
                btnCancel.Visibility = Visibility.Visible;
                btnSave.Visibility = Visibility.Visible;
                btnDelete.Visibility = Visibility.Collapsed;
                btnSure.Visibility = Visibility.Collapsed;

                txtPartWareHouseCode.IsEnabled = false;
                btnPartWareHouse.IsEnabled = false;
                txtPartWareHouseDesc.IsReadOnly = false;
                pnl.Visibility = Visibility.Visible;

                colSequence.Visible = false;
                colSequence.ShowInColumnChooser = false;
            }
            else if (FormState == Data.ViewState.Modify)
            {
                btnAdd.Visibility = Visibility.Collapsed;
                btnModify.Visibility = Visibility.Collapsed;
                btnCancel.Visibility = Visibility.Visible;
                btnSave.Visibility = Visibility.Visible;
                btnDelete.Visibility = Visibility.Collapsed;
                btnSure.Visibility = Visibility.Collapsed;

                txtPartWareHouseCode.IsEnabled = false;
                btnPartWareHouse.IsEnabled = false;
                txtPartWareHouseDesc.IsReadOnly = false;
                pnl.Visibility = Visibility.Visible;

                colSequence.Visible = false;
                colSequence.ShowInColumnChooser = false;
            }
            else if (FormState == Data.ViewState.Normal)
            {
                btnAdd.Visibility = CheckAuthority('1') ? Visibility.Visible : Visibility.Collapsed;
                if(this.DataContext!=null)
                {
                    DataRowView row = this.DataContext as DataRowView;
                    if(row.Row.Field<string>("Status").ToUpper()== "Close".ToUpper())
                    {
                        btnModify.Visibility = Visibility.Collapsed;
                        btnDelete.Visibility = Visibility.Collapsed;
                        btnSure.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        btnModify.Visibility = CheckAuthority('2') ? Visibility.Visible : Visibility.Collapsed;
                        btnDelete.Visibility = CheckAuthority('3') ? Visibility.Visible : Visibility.Collapsed;
                        btnSure.Visibility = CheckAuthority('6') ? Visibility.Visible : Visibility.Collapsed;
                    }
                }
                else
                {
                    btnModify.Visibility = Visibility.Collapsed;
                    btnDelete.Visibility = Visibility.Collapsed;
                    btnSure.Visibility = Visibility.Collapsed;
                }
                btnCancel.Visibility = Visibility.Collapsed;
                btnSave.Visibility = Visibility.Collapsed;

                txtPartWareHouseCode.IsEnabled = true;
                btnPartWareHouse.IsEnabled = true;
                txtPartWareHouseDesc.IsReadOnly = true;
                pnl.Visibility = Visibility.Collapsed;

                colSequence.Visible = true;
                colSequence.ShowInColumnChooser = true;
            }
        }

        public PartWareHouseOut(Framework.SystemAuthority authority) :
            base(authority)
        {
            InitializeComponent();
            FormStateChanged();
        }
        public PartWareHouseOut(List<Framework.SystemAuthority> authoritys, Data.Parameters parameters, string flag) :
            base(authoritys, parameters, flag)
        {
            InitializeComponent();
            FormStateChanged();

            this.LoadComplete += new RoutedEventHandler((sender, e) =>
            {
                if (parameters != null)
                {
                    if (parameters.IsExists("IsAdd") && parameters["IsAdd"] != null && (bool)parameters["IsAdd"])
                    {
                        //if (CheckAuthority('1'))
                        {
                            Add();
                        }
                        //else
                        //{
                        //    this.Close();
                        //}
                    }
                    else
                    {
                        if (parameters.IsExists("PartWareHouseCode") && parameters["PartWareHouseCode"] != null && parameters["PartWareHouseCode"].ToString().Trim() != string.Empty)
                        {
                            if (!LoadPartWareHouseCode(parameters["PartWareHouseCode"].ToString().Trim()))
                            {
                                this.Close();
                            }
                            else
                            {
                                FormStateChanged();
                            }
                        }
                        else
                        {
                            this.Close();
                        }
                    }
                }
            });
        }

        private void txtPartWareHouseCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.DataContext = null;
        }

        private bool LoadPartWareHouseCode(string code)
        {
            string sql = @"SELECT Bas_PartWareHouse.PartWareHouseId,
	   Bas_PartWareHouse.PartWareHouseCode,
	   Bas_PartWareHouse.PartWareHouseDesc,
	   Bas_PartWareHouse.Status,
	   dbo.Ftn_GetStatusName(Bas_PartWareHouse.Status,'PartWareHouseStatus') AS StatusName,
	   SureUser.SysUserId AS SureUserId,
	   SureUser.SysUserCode+'-'+ISNULL(SureUser.SysUserName,'') AS SureUserName,
	   Bas_PartWareHouse.SureDateTime,
	   CreateUser.SysUserId AS CreateUserId,
	   CreateUser.SysUserCode+'-'+ISNULL(CreateUser.SysUserName,'') AS CreateUserName,
	   Bas_PartWareHouse.CreateDateTime
FROM dbo.Bas_PartWareHouse WITH(NOLOCK) 
LEFT JOIN dbo.Set_User SureUser WITH(NOLOCK) ON Bas_PartWareHouse.SureUserId=SureUser.SysUserId
LEFT JOIN dbo.Set_User CreateUser WITH(NOLOCK) ON Bas_PartWareHouse.CreateUserId=CreateUser.SysUserId
WHERE Bas_PartWareHouse.Type='Out' AND Bas_PartWareHouse.PartWareHouseCode=@PartWareHouseCode";
            Data.Parameters parameters = new Parameters()
                .Add("PartWareHouseCode", code);
            try
            {
                DataTable dt = DB.DBHelper.GetDataTable(sql, parameters, null, true);
                if (dt.Rows.Count < 1)
                {
                    Component.MessageBox.MyMessageBox.ShowError(string.Format("出库单[{0}]错误。", code));
                    return false;
                }
                else
                {
                    txtPartWareHouseCode.Text = dt.Rows[0]["PartWareHouseCode"].ToString().Trim();
                    this.DataContext = dt.DefaultView[0];
                    txtPartWareHouseDesc.Text = dt.Rows[0]["PartWareHouseDesc"].ToString().Trim();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Component.MessageBox.MyMessageBox.ShowError(ex.Message);
                return false;
            }

        }

        private void txtPartWareHouseCode_LostFocus(object sender, RoutedEventArgs e)
        {
            if (this.DataContext == null)
            {
                if (txtPartWareHouseCode.Text.Trim() != string.Empty)
                {
                    if(!LoadPartWareHouseCode(txtPartWareHouseCode.Text.Trim()))
                    {
                        txtPartWareHouseCode.Text = string.Empty;
                    }


                    FormStateChanged();
                }
            }
        }

        private void btnPartWareHouse_Click(object sender, RoutedEventArgs e)
        {
            Selector.SelectorPartWareHouse win = new Selector.SelectorPartWareHouse(null,false);
            if (win.ShowDialog())
            {
                txtPartWareHouseCode.Text = win.SelectRow["PartWareHouseCode"].ToString().Trim();
                this.DataContext = win.SelectRow;
                txtPartWareHouseDesc.Text = win.SelectRow["PartWareHouseDesc"].ToString().Trim();


                FormStateChanged();
            }
        }

        private void Add()
        {
            //if (!CheckAuthority('1'))
            //{
            //    Component.MessageBox.MyMessageBox.ShowError("无新增权限。");
            //    return;
            //}

            txtPartWareHouseCode.Text = string.Empty;
            txtPartWareHouseDesc.Text = string.Empty;
            FormState = ViewState.Add;

            string sql = @"SELECT 0 AS Flag,
       Bas_PartWareHouseDetail.PartWareHouseDetailId,
	   Bas_PartWareHouseDetail.Sequence,
	   Bas_PartWareHouseDetail.ItemId,
	   Bas_Item.ItemCode,
	   Bas_Item.ItemSpecification,
	   Bas_PartWareHouseDetail.Qty,
	   Bas_PartWareHouseDetail.Unit,
	   Bas_PartWareHouseDetail.PartWareHouseDetailDesc
FROM dbo.Bas_PartWareHouseDetail WITH(NOLOCK) 
LEFT JOIN dbo.Bas_Item WITH(NOLOCK) ON dbo.Bas_Item.ItemId = dbo.Bas_PartWareHouseDetail.ItemId
WHERE 1!=1
";
            new TaskFactory<Data.Result<System.Data.DataTable>>().StartNew(() =>
            {
                Data.Result<System.Data.DataTable> result = new Data.Result<System.Data.DataTable>();
                try
                {
                    System.Data.DataTable r = DB.DBHelper.GetDataTable(sql, null, null, true);

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

        private void Modify()
        {
            if (!CheckAuthority('2'))
            {
                Component.MessageBox.MyMessageBox.ShowError("无修改权限。");
                return;
            }
            if (this.DataContext == null)
                return;

            FormState = ViewState.Modify;
        }

        private void Delete()
        {
            if (!CheckAuthority('3'))
            {
                Component.MessageBox.MyMessageBox.ShowError("无删除权限。");
                return;
            }

            if (this.DataContext == null)
                return;
            DataRowView row = this.DataContext as DataRowView;
            if (Component.MessageBox.MyMessageBox.ShowQuestion(string.Format("确定删除出库单[{0}]？", row["PartWareHouseCode"])) != MessageBoxResult.OK)
            {
                return;
            }

            string sql = @"DECLARE @IdFlag BIGINT
DECLARE @PartWareHouseCode NVARCHAR(50)
DECLARE @Status NVARCHAR(50)
DECLARE @StatusName NVARCHAR(50)


SELECT @IdFlag=PartWareHouseId,
		@PartWareHouseCode=PartWareHouseCode,
		@Status=[Status],
		@StatusName=dbo.Ftn_GetStatusName([Status],'PartWareHouseStatus')
FROM dbo.Bas_PartWareHouse WITH(NOLOCK) WHERE PartWareHouseId=@PartWareHouseId

IF @IdFlag IS NULL
 BEGIN
	SET @Return_Message='出库单已被删除。'
	SET @Return_Value=0
	RETURN 
 END 
IF ISNULL(@Status,'')='Close'
 BEGIN
	SET @Return_Message='出库单['+ISNULL(@PartWareHouseCode,'N/A')+']的状态为['+ISNULL(@StatusName,'N/A')+']，不能删除。'	
	SET @Return_Value=0
	RETURN 
 END 

DELETE dbo.Bas_PartWareHouseDetail WHERE PartWareHouseId=@PartWareHouseId
DELETE dbo.Bas_PartWareHouse WHERE PartWareHouseId=@PartWareHouseId


SET @Return_Message='出库单['+ISNULL(@PartWareHouseCode,'N/A')+']删除成功。'	
SET @Return_Value=1
RETURN  ";

            Data.Parameters parameters = new Parameters()
                                .Add("PartWareHouseId", row["PartWareHouseId"])
                                .Add("UserId", Framework.App.User.UserId)
                                .Add("PluginId", PluginId)
                                .Add("Return_Message", DBNull.Value, SqlDbType.NVarChar, int.MaxValue, ParameterDirection.Output)
                                .Add("Return_Value", 0, SqlDbType.Int, ParameterDirection.Output);
            try
            {
                parameters = DB.DBHelper.ExecuteParameters(sql, parameters, null, true);
                if ((int)parameters["Return_Value"] != 1)
                {
                    Component.MessageBox.MyMessageBox.ShowError(parameters["Return_Message"].ToString());
                }
                else
                {
                    Component.MessageBox.MyMessageBox.Show(parameters["Return_Message"].ToString());
                    txtPartWareHouseCode.Text = string.Empty;
                    this.DataContext = null;
                    FormStateChanged();
                }
            }
            catch (Exception e)
            {
                Component.MessageBox.MyMessageBox.ShowError(e.Message);
                return;
            }
        }

        private void Save()
        {
            if(matrix1.ItemsSource==null || (matrix1.ItemsSource as DataView).Count<1)
            {
                Component.MessageBox.MyMessageBox.ShowError("请先添加明细。");
                return;
            }

            if(Component.MessageBox.MyMessageBox.ShowQuestion("确定保存吗?")!=MessageBoxResult.OK)
            {
                return;
            }

            StringBuilder xml = new StringBuilder();


            xml.Append(string.Format("<xml>"));
            foreach (DataRowView row in (matrix1.ItemsSource as DataView))
            {
                xml.Append(string.Format("<row Flag=\"{0}\" PartWareHouseDetailId=\"{1}\" ItemId=\"{2}\" Qty=\"{3}\" PartWareHouseDetailDesc=\"{4}\"/>",
                    row["Flag"],
                    row["PartWareHouseDetailId"],
                    row["ItemId"],
                    row["Qty"],
                    WinAPI.File.XMLHelper.StringFormat(row["PartWareHouseDetailDesc"].ToString())));
            }
            xml.Append(string.Format("</xml>"));
            

            string sql = "Prd_Equipment_Edit_PartWareHouse";
            Data.Parameters parameters = new Parameters()
                .Add("IsAdd", FormState==ViewState.Add?true:false)
                .Add("IsIn", false)
                .Add("PartWareHouseId", FormState == ViewState.Add ? DBNull.Value:(this.DataContext as DataRowView)["PartWareHouseId"])
                .Add("PartWareHouseCode", DBNull.Value, SqlDbType.NVarChar, 50, ParameterDirection.Output)
                .Add("PartWareHouseDesc", txtPartWareHouseDesc.Text.Trim())
                .Add("Xml", xml.ToString(), SqlDbType.Xml, int.MaxValue)
                .Add("UserId", Framework.App.User.UserId)
                .Add("PluginId", PluginId)
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
                matrix1.ItemsSource = null;
                LoadPartWareHouseCode(parameters["PartWareHouseCode"].ToString());
                FormState = ViewState.Normal;
                return;
            }
            else
            {
                Component.MessageBox.MyMessageBox.ShowError(parameters["Return_Message"].ToString());
            }

        }

        private void UserVendor_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            matrix1.ItemsSource = null;
            txtPartWareHouseDesc.Text = string.Empty;
            if (DataContext == null)
            {
                return;
            }
            else
            {
                DataRowView row = DataContext as DataRowView;
                string sql = @"SELECT 0 AS Flag,
	   Bas_PartWareHouseDetail.PartWareHouseDetailId,
	   Bas_PartWareHouseDetail.Sequence,
	   Bas_PartWareHouseDetail.ItemId,
	   Bas_Item.ItemCode,
	   Bas_Item.ItemSpecification,
	   Bas_PartWareHouseDetail.Qty,
	   Bas_PartWareHouseDetail.Unit,
	   Bas_PartWareHouseDetail.PartWareHouseDetailDesc
FROM dbo.Bas_PartWareHouseDetail WITH(NOLOCK) 
LEFT JOIN dbo.Bas_Item WITH(NOLOCK) ON dbo.Bas_Item.ItemId = dbo.Bas_PartWareHouseDetail.ItemId
WHERE Bas_PartWareHouseDetail.PartWareHouseId=@PartWareHouseId
ORDER BY Bas_PartWareHouseDetail.Sequence
";
                Data.Parameters parameters = new Parameters()
                        .Add("PartWareHouseId", row["PartWareHouseId"]);
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
        
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (CheckAuthority('1') )
            {
                Add();
            }
        }

        private void btnModify_Click(object sender, RoutedEventArgs e)
        {
            if (CheckAuthority('2')  && this.DataContext != null)
            {
                Modify();
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (CheckAuthority('3') && this.DataContext != null)
            {
                Delete();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            FormState = ViewState.Normal;
            if (FormState==ViewState.Add)
            {
                txtPartWareHouseCode.Text = string.Empty;
            }
            else
            {
                this.DataContext = null;
                if (txtPartWareHouseCode.Text.Trim() != string.Empty)
                {
                    LoadPartWareHouseCode(txtPartWareHouseCode.Text.Trim());
                }
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void btnItemAdd_Click(object sender, RoutedEventArgs e)
        {
            if (FormState == ViewState.Modify|| FormState == ViewState.Add)
            {
                Edit.PartWareHouse_EditDetail win = new Edit.PartWareHouse_EditDetail(null, null, null);
                if (win.ShowDialog())
                {
                    DataTable dt = (matrix1.ItemsSource as DataView).Table;
                    DataRow row = dt.NewRow();
                    row["Flag"] = 1;
                    row["ItemId"] = win.ItemId;
                    row["ItemCode"] = win.ItemCode;
                    row["ItemSpecification"] = win.ItemSpecification;
                    row["Unit"] = win.Unit;
                    row["Qty"] = win.Qty;
                    row["PartWareHouseDetailDesc"] = win.PartWareHouseDetailDesc;
                    dt.Rows.Add(row);
                    matrix1.BestFitColumns();
                }
            }
        }

        private void btnItemRemove_Click(object sender, RoutedEventArgs e)
        {
            if (FormState == ViewState.Modify || FormState == ViewState.Add)
            {
                List<DataRowView> l = new List<DataRowView>();
                foreach (DataRowView row in matrix1.SelectedItems)
                {
                    l.Add(row);
                }

                if (l.Count < 1)
                {
                    Component.MessageBox.MyMessageBox.Show("请选择要删除的明细。");
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

        private void GridLinkColumn_LinkClick(object value, object parameter)
        {
            if (FormState == ViewState.Modify || FormState == ViewState.Add)
            {
                DataRowView row = parameter as DataRowView;

                Edit.PartWareHouse_EditDetail win = new Edit.PartWareHouse_EditDetail(row.Row.Field<long?>("ItemId"),
                                                                                                          row.Row.Field<decimal?>("Qty"),
                                                                                                          row.Row.Field<string>("PartWareHouseDetailDesc"));
                if (win.ShowDialog())
                {
                    if (row.Row.Field<int>("Flag") == 0)
                    {
                        row["Flag"] = 2;
                    }
                    row["ItemId"] = win.ItemId;
                    row["ItemCode"] = win.ItemCode;
                    row["ItemSpecification"] = win.ItemSpecification;
                    row["Unit"] = win.Unit;
                    row["Qty"] = win.Qty;
                    row["PartWareHouseDetailDesc"] = win.PartWareHouseDetailDesc;
                }
            }
        }

        private void btnSure_Click(object sender, RoutedEventArgs e)
        {
            if (CheckAuthority('6') && this.DataContext != null)
            {
                Sure();
            }
        }

        private void Sure()
        {
            if (!CheckAuthority('6'))
            {
                Component.MessageBox.MyMessageBox.ShowError("无过账权限。");
                return;
            }

            if (this.DataContext == null)
                return;
            DataRowView row = this.DataContext as DataRowView;
            if (Component.MessageBox.MyMessageBox.ShowQuestion(string.Format("确定过账出库单[{0}]？", row["PartWareHouseCode"])) != MessageBoxResult.OK)
            {
                return;
            }

            string sql = @"Prd_Equipment_PartWareHouse_Sure";

            Data.Parameters parameters = new Parameters()
                                .Add("IsIn",false)
                                .Add("PartWareHouseId", row["PartWareHouseId"])
                                .Add("UserId", Framework.App.User.UserId)
                                .Add("PluginId", PluginId)
                                .Add("Return_Message", DBNull.Value, SqlDbType.NVarChar, int.MaxValue, ParameterDirection.Output)
                                .Add("Return_Value", 0, SqlDbType.Int, ParameterDirection.ReturnValue);
            try
            {
                parameters = DB.DBHelper.ExecuteParameters(sql, parameters,ExecuteType.StoredProcedure,null, true);
                if ((int)parameters["Return_Value"] != 1)
                {
                    Component.MessageBox.MyMessageBox.ShowError(parameters["Return_Message"].ToString());
                }
                else
                {
                    Component.MessageBox.MyMessageBox.Show(parameters["Return_Message"].ToString());
                    matrix1.ItemsSource = null;
                    LoadPartWareHouseCode(txtPartWareHouseCode.Text.Trim());
                    FormStateChanged();
                }
            }
            catch (Exception e)
            {
                Component.MessageBox.MyMessageBox.ShowError(e.Message);
                return;
            }
        }
    }
}
