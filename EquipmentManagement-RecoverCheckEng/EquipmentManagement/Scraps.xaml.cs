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
    /// Scraps.xaml 的交互逻辑
    /// </summary>
    public partial class Scraps : Component.Controls.User.UserVendor
    {
        public Scraps(Framework.SystemAuthority authority) :
            base(authority)
        {
            InitializeComponent();
            LoadStatus();
            LoadType();
        }

        private void LoadStatus()
        {
            string sql = @"SELECT Code,Name
FROM dbo.Set_SystemType WITH(NOLOCK) 
WHERE TypeCode='EquipmentScrapStatus'
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

        private void LoadType()
        {
            string sql = @"SELECT Code,Name
FROM dbo.Set_SystemType WITH(NOLOCK) 
WHERE TypeCode='EquipmentScrapType'
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

        private string whereSql = null;
        private Data.Parameters queryParameters = null;
        private void QuerySum()
        {
            whereSql = string.Format(@"WHERE 1=1 ");
            queryParameters = new Data.Parameters();
            if (txtEquipmentScrapCode.Text.Trim() != string.Empty)
            {
                whereSql = whereSql + " AND Bas_EquipmentScrap.EquipmentScrapCode LIKE '%'+@EquipmentScrapCode+'%' ";
                queryParameters.Add("EquipmentScrapCode", txtEquipmentScrapCode.Text.Trim());
            }
            if (cmbType.SelectedValue != null && cmbType.SelectedValue.ToString().Trim() != string.Empty)
            {
                whereSql = whereSql + " AND Bas_EquipmentScrap.Type=@Type ";
                queryParameters.Add("Type", cmbType.SelectedValue);
            }
            if (cmbStatus.SelectedValue != null && cmbStatus.SelectedValue.ToString().Trim() != string.Empty)
            {
                whereSql = whereSql + " AND Bas_EquipmentScrap.[Status]=@Status2 ";
                queryParameters.Add("Status2", cmbStatus.SelectedValue);
            }
            if (txtEquipmentRepairInCode.Text.Trim() != string.Empty)
            {
                whereSql = whereSql + " AND Inp_EquipmentRepairIn.EquipmentRepairInCode LIKE '%'+@EquipmentRepairInCode+'%' ";
                queryParameters.Add("EquipmentRepairInCode", txtEquipmentRepairInCode.Text.Trim());
            }
            if (txtEquipmentRepairOutSourcingCode.Text.Trim() != string.Empty)
            {
                whereSql = whereSql + " AND Inp_EquipmentRepairOutSourcing.EquipmentRepairOutSourcingCode LIKE '%'+@EquipmentRepairOutSourcingCode+'%' ";
                queryParameters.Add("EquipmentRepairOutSourcingCode", txtEquipmentRepairOutSourcingCode.Text.Trim());
            }

            string sql = string.Format(@"SELECT COUNT(*) FROM dbo.Bas_EquipmentScrap WITH(NOLOCK) 
LEFT JOIN dbo.Inp_EquipmentRepairIn WITH(NOLOCK) ON Bas_EquipmentScrap.Type='RepairScrap' AND Bas_EquipmentScrap.SoureId=Inp_EquipmentRepairIn.EquipmentRepairInId
LEFT JOIN dbo.Inp_EquipmentRepairOutSourcing WITH(NOLOCK) ON Bas_EquipmentScrap.Type='RepairOutSourcingScrap' AND Bas_EquipmentScrap.SoureId=Inp_EquipmentRepairOutSourcing.EquipmentRepairOutSourcingId {0}", whereSql);
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
SELECT ROW_NUMBER()OVER(ORDER BY Bas_EquipmentScrap.CreateDateTime ASC) AS RowIndex,
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
                    Component.MaskBusy.Hide(grid);
                }));
            });
        }

        private void btnQuery_Click(object sender, RoutedEventArgs e)
        {
            QuerySum();
        }

        private void GridLinkColumn_LinkClick(object value, object parameter)
        {
            DataRowView row = parameter as DataRowView;
            Data.Parameters parameters = new Data.Parameters().Add("EquipmentScrapCode", row["EquipmentScrapCode"]);
            Component.App.Portal.OpenPlugin("EquipmentScrap", string.Format(" - {0}", row["EquipmentScrapCode"]), parameters, row["EquipmentScrapCode"].ToString());
        }
    }
}
