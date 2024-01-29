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
    /// SelectorRepairOutSourcing.xaml 的交互逻辑
    /// </summary>
    public partial class SelectorRepairOutSourcing : Component.ControlsEx.Window
    {
        public System.Data.DataRowView SelectRow
        {
            get;
            private set;
        }

        private string Status = null;
        public SelectorRepairOutSourcing(string status)
        {
            InitializeComponent();
            this.Status = status;

            LoadStatus();
        }

        private void LoadStatus()
        {
            string sql = @"DECLARE @t1 TABLE(rowIndex INT,String NVARCHAR(50))
INSERT INTO @t1
SELECT rowIndex,String FROM dbo.Ftn_GetTableFromString(@Code1,',')

SELECT Code,Name
FROM dbo.Set_SystemType WITH(NOLOCK) 
WHERE TypeCode='EquipmentRepairOutSourcingStatus'
AND (NOT EXISTS(SELECT * FROM @t1) OR EXISTS(SELECT * FROM @t1 WHERE String=Set_SystemType.Code))
ORDER BY Sequence";
            Data.Parameters parameters = new Data.Parameters()
                        .Add("Code1", Status);
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

        private string whereSql = null;
        private Data.Parameters queryParameters = null;
        private void QuerySum()
        {
            whereSql = string.Format(@"WHERE 1=1 ");
            queryParameters = new Data.Parameters();
            if (!string.IsNullOrEmpty(Status))
            {
                whereSql = whereSql + " AND EXISTS(SELECT * FROM dbo.Ftn_GetTableFromString(@Status,',') WHERE String=Inp_EquipmentRepairOutSourcing.[Status])";
                queryParameters.Add("Status", Status);
            }
            if (txtEquipmentRepairOutSourcingCode.Text.Trim() != string.Empty)
            {
                whereSql = whereSql + " AND Inp_EquipmentRepairOutSourcing.EquipmentRepairOutSourcingCode LIKE '%'+@EquipmentRepairOutSourcingCode+'%' ";
                queryParameters.Add("EquipmentRepairOutSourcingCode", txtEquipmentRepairOutSourcingCode.Text.Trim());
            }
            if (txtEquipmentRepairInCode.Text.Trim() != string.Empty)
            {
                whereSql = whereSql + " AND Inp_EquipmentRepairIn.EquipmentRepairInCode LIKE '%'+@EquipmentRepairInCode+'%' ";
                queryParameters.Add("EquipmentRepairInCode", txtEquipmentRepairInCode.Text.Trim());
            }
            if (cmbStatus.SelectedValue != null && cmbStatus.SelectedValue.ToString().Trim() != string.Empty)
            {
                whereSql = whereSql + " AND Inp_EquipmentRepairOutSourcing.[Status]=@Status2 ";
                queryParameters.Add("Status2", cmbStatus.SelectedValue);
            }

            string sql = string.Format(@"SELECT COUNT(*) FROM dbo.Inp_EquipmentRepairOutSourcing WITH(NOLOCK) 
LEFT JOIN dbo.Inp_EquipmentRepairIn WITH(NOLOCK) ON dbo.Inp_EquipmentRepairIn.EquipmentRepairInId = dbo.Inp_EquipmentRepairOutSourcing.EquipmentRepairInId {0}", whereSql);
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
SELECT ROW_NUMBER()OVER(ORDER BY Inp_EquipmentRepairOutSourcing.CreateDateTime ASC) AS RowIndex,
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
LEFT JOIN dbo.Set_User UpdateUser WITH(NOLOCK) ON UpdateUser.SysUserId = dbo.Inp_EquipmentRepairOutSourcing.UpdateUserId
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
