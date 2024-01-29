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
using System.Windows.Shapes;

namespace EquipmentManagement.Selector
{
    /// <summary>
    /// SelectorEquipmentRepairIn.xaml 的交互逻辑
    /// </summary>
    public partial class SelectorEquipmentRepairIn : Component.ControlsEx.Window
    {
        public System.Data.DataRowView SelectRow
        {
            get;
            private set;
        }

        private string Status = null;
        public SelectorEquipmentRepairIn(string status)
        {
            InitializeComponent();
            this.Status = status;
            LoadStatus();
            LoadType();
            LoadRepairType();
        }

        private void LoadStatus()
        {
            string sql = @"DECLARE @t TABLE(rowIndex INT,String NVARCHAR(50))
INSERT INTO @t
SELECT rowIndex,String FROM dbo.Ftn_GetTableFromString(@Code,',')

SELECT Code,Name
FROM dbo.Set_SystemType WITH(NOLOCK) 
WHERE TypeCode='EquipmentRepairInStatus'
AND (NOT EXISTS(SELECT * FROM @t) OR EXISTS(SELECT * FROM @t WHERE String=Set_SystemType.Code))
ORDER BY Sequence";
            Data.Parameters parameters = new Data.Parameters()
                        .Add("Code", Status);
            try
            {
                System.Data.DataTable dt = DB.DBHelper.GetDataTable(sql, parameters, null, true);
                System.Data.DataRow row = dt.NewRow();
                dt.Rows.InsertAt(row, 0);
                cmbStatus.ItemsSource = dt.DefaultView;
                cmbStatus.SelectedValuePath = "Code";
                cmbStatus.DisplayMemberPath = "Name";
                cmbStatus.SelectedIndex = 0;
            }
            catch (Exception e)
            {
                Component.MessageBox.MyMessageBox.ShowError(e.Message);
            }
        }

        private void LoadType()
        {
            string sql = @"SELECT Code,Name
FROM dbo.Set_SystemType WITH(NOLOCK) 
WHERE TypeCode='EquipmentRepairInType'
ORDER BY Sequence";
            try
            {
                System.Data.DataTable dt = DB.DBHelper.GetDataTable(sql, null, null, true);
                System.Data.DataRow row = dt.NewRow();
                dt.Rows.InsertAt(row, 0);
                cmbType.ItemsSource = dt.DefaultView;
                cmbType.SelectedValuePath = "Code";
                cmbType.DisplayMemberPath = "Name";
                cmbType.SelectedIndex = 0;
            }
            catch (Exception e)
            {
                Component.MessageBox.MyMessageBox.ShowError(e.Message);
            }
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

        private string whereSql = null;
        private Data.Parameters queryParameters = null;
        private void QuerySum()
        {
            whereSql = string.Format(@"WHERE 1=1 ");
            queryParameters = new Data.Parameters();
            if (!string.IsNullOrEmpty(Status))
            {
                whereSql = whereSql + " AND EXISTS(SELECT * FROM dbo.Ftn_GetTableFromString(@Status,',') WHERE String=Inp_EquipmentRepairIn.[Status])";
                queryParameters.Add("Status", Status);
            }
            if (txtEquipmentRepairInCode.Text.Trim() != string.Empty)
            {
                whereSql = whereSql + " AND Inp_EquipmentRepairIn.EquipmentRepairInCode LIKE '%'+@EquipmentRepairInCode+'%' ";
                queryParameters.Add("EquipmentRepairInCode", txtEquipmentRepairInCode.Text.Trim());
            }
            if (cmbType.SelectedValue != null && cmbType.SelectedValue.ToString().Trim() != string.Empty)
            {
                whereSql = whereSql + " AND Inp_EquipmentRepairIn.[Type]=@Type ";
                queryParameters.Add("Type", cmbType.SelectedValue);
            }
            if (cmbStatus.SelectedValue != null && cmbStatus.SelectedValue.ToString().Trim() != string.Empty)
            {
                whereSql = whereSql + " AND Inp_EquipmentRepairIn.[Status]=@Status1 ";
                queryParameters.Add("Status1", cmbStatus.SelectedValue);
            }
            if (cmbRepairType.SelectedValue != null && cmbRepairType.SelectedValue.ToString().Trim() != string.Empty)
            {
                whereSql = whereSql + " AND Inp_EquipmentRepairIn.RepairTypeId=@RepairTypeId ";
                queryParameters.Add("RepairTypeId", cmbRepairType.SelectedValue);
            }

            if (txtMainRepairUser.Text != string.Empty)
            {
                try
                {
                    whereSql = whereSql + " AND Inp_EquipmentRepairIn.MainRepairUserId=@MainRepairUserId ";
                    queryParameters.Add("MainRepairUserId", txtMainRepairUser.Value);
                }
                catch (Exception ex)
                {
                    Component.MessageBox.MyMessageBox.ShowError(string.Format("主维修人{0}", ex.Message));
                    txtMainRepairUser.SetFoucs();
                    return;
                }
            }

            if (txtSendRepairUser.Text != string.Empty)
            {
                try
                {
                    whereSql = whereSql + " AND Inp_EquipmentRepairIn.SendRepairUserId=@SendRepairUserId ";
                    queryParameters.Add("SendRepairUserId", txtSendRepairUser.Value);
                }
                catch (Exception ex)
                {
                    Component.MessageBox.MyMessageBox.ShowError(string.Format("送修人{0}", ex.Message));
                    txtSendRepairUser.SetFoucs();
                    return;
                }
            }

            string sql = string.Format("SELECT COUNT(*) FROM dbo.Inp_EquipmentRepairIn WITH(NOLOCK) {0}", whereSql);
            int handleNo = Component.MaskBusy.Busy(grid, "正在查询数据。。。");
            new TaskFactory<Data.Result<int>>().StartNew(() =>
            {
                Data.Result<int> result = new Data.Result<int>();
                try
                {
                    object r = DB.DBHelper.GetScalar(sql, queryParameters, null, true);

                    result.HasError = false;
                    result.Value = int.Parse(r.ToString());
                    return result;
                }
                catch (Exception e)
                {
                    result.HasError = true;
                    result.Message = e.Message;
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
                        matrix.ItemsSource = null;
                        pager.ReSet();
                        pager.SumCount = result.Result.Value;
                    }
                }));
                Component.MaskBusy.Hide(grid, handleNo);
            });
        }

        private void pager_UpdateSource(object sender, RoutedEventArgs e)
        {
            string sql = string.Format(@"WITH temp AS (
SELECT ROW_NUMBER()OVER(ORDER BY Inp_EquipmentRepairIn.EquipmentRepairInCode DESC) AS RowIndex,
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
{0}
)

SELECT * 
FROM temp
WHERE temp.RowIndex BETWEEN @IndexStart AND @IndexEnd
ORDER BY RowIndex", whereSql);
            Component.MaskBusy.Busy(grid, "正在查询数据。。。");
            int indexStart = pager.PageIndex == 0 ? 0 : ((pager.PageIndex - 1) * pager.PageSize) + 1;
            int indexEnd = pager.PageIndex == 0 ? 0 : pager.PageIndex * pager.PageSize;
            if (queryParameters.IsExists("IndexStart"))
            {
                queryParameters.SetParameterValue(queryParameters.GetParameter("IndexStart"), indexStart);
            }
            else
            {
                queryParameters.Add("IndexStart", indexStart);
            }
            if (queryParameters.IsExists("IndexEnd"))
            {
                queryParameters.SetParameterValue(queryParameters.GetParameter("IndexEnd"), indexEnd);
            }
            else
            {
                queryParameters.Add("IndexEnd", indexEnd);
            }
            new TaskFactory<Data.Result<System.Data.DataTable>>().StartNew(() =>
            {
                Data.Result<System.Data.DataTable> result = new Data.Result<System.Data.DataTable>();
                try
                {
                    System.Data.DataTable r = DB.DBHelper.GetDataTable(sql, queryParameters, null, true);

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
                        matrix.ItemsSource = result.Result.Value.DefaultView;
                        (matrix.View as Component.ControlsEx.TableView).BestFitColumns();
                    }
                }));
                Component.MaskBusy.Hide(grid);
            });
        }

        private void GridLinkColumn_LinkClick(object vlaue, object parameter)
        {
            SelectRow = parameter as System.Data.DataRowView;
            this.DialogResult = true;
        }

        private void btnQuery_Click(object sender, RoutedEventArgs e)
        {
            QuerySum();
        }
    }
}
