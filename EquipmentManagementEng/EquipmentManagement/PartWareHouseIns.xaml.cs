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
    /// PartWareHouseIns.xaml 的交互逻辑
    /// </summary>
    public partial class PartWareHouseIns : Component.Controls.User.UserVendor
    {
        public PartWareHouseIns(Framework.SystemAuthority authority) :
            base(authority)
        {
            InitializeComponent();
            LoadStatus();
        }
        private void LoadStatus()
        {
            string sql = @"SELECT Code,Name
FROM dbo.Set_SystemType WITH(NOLOCK) 
WHERE TypeCode='PartWareHouseStatus'
ORDER BY Sequence";
            try
            {
                System.Data.DataTable dt = DB.DBHelper.GetDataTable(sql, null, null, true);
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
            whereSql = string.Format(@"WHERE Bas_PartWareHouse.Type='In' ");
            queryParameters = new Data.Parameters();
            if (txtPartWareHouseCode.Text.Trim() != string.Empty)
            {
                whereSql = whereSql + " AND Bas_PartWareHouse.PartWareHouseCode LIKE '%'+@PartWareHouseCode+'%' ";
                queryParameters.Add("PartWareHouseCode", txtPartWareHouseCode.Text.Trim());
            }
            if (cmbStatus.SelectedValue != null && cmbStatus.SelectedValue.ToString().Trim() != string.Empty)
            {
                whereSql = whereSql + " AND Bas_PartWareHouse.[Status]=@Status1 ";
                queryParameters.Add("Status1", cmbStatus.SelectedValue);
            }
            if (txtStartSureDateTime.SelectedDate.HasValue)
            {
                whereSql = whereSql + " AND Bas_PartWareHouse.SureDateTime>=@StartSureDateTime ";
                queryParameters.Add("StartSureDateTime", txtStartSureDateTime.SelectedDate.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            if (txtEndSureDateTime.SelectedDate.HasValue)
            {
                whereSql = whereSql + " AND Bas_PartWareHouse.SureDateTime<=@EndSureDateTime ";
                queryParameters.Add("EndSureDateTime", txtEndSureDateTime.SelectedDate.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            if (txtStartCreateDateTime.SelectedDate.HasValue)
            {
                whereSql = whereSql + " AND Bas_PartWareHouse.CreateDateTime>=@StartCreateDateTime ";
                queryParameters.Add("StartCreateDateTime", txtStartCreateDateTime.SelectedDate.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            if (txtEndCreateDateTime.SelectedDate.HasValue)
            {
                whereSql = whereSql + " AND Bas_PartWareHouse.CreateDateTime<=@EndCreateDateTime ";
                queryParameters.Add("EndCreateDateTime", txtEndCreateDateTime.SelectedDate.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            }

            string sql = string.Format(@"SELECT COUNT(*) FROM Bas_PartWareHouse WITH(NOLOCK) {0}", whereSql);
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
SELECT ROW_NUMBER()OVER(ORDER BY Bas_PartWareHouse.PartWareHouseCode DESC) AS RowIndex,
	   Bas_PartWareHouse.PartWareHouseId,
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

        private void btnQuery_Click(object sender, RoutedEventArgs e)
        {
            QuerySum();
        }

        private void GridLinkColumn_LinkClick(object value, object parameter)
        {
            DataRowView row = parameter as DataRowView;
            Data.Parameters parameters = new Data.Parameters().Add("PartWareHouseCode", row["PartWareHouseCode"]);
            Component.App.Portal.OpenPlugin("PartWareHouseIn", string.Format(" - {0}", row["PartWareHouseCode"]), parameters, row["PartWareHouseCode"].ToString());
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            Data.Parameters parameters = new Data.Parameters().Add("IsAdd", true);
            Component.App.Portal.OpenPlugin("PartWareHouseIn", string.Format(" - {0}", "新增"), parameters, null);
        }
    }
}
