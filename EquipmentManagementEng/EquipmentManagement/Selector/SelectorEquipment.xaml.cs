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
    /// SelectorEquipment.xaml 的交互逻辑
    /// </summary>
    public partial class SelectorEquipment : Component.ControlsEx.Window
    {
        public System.Data.DataRowView SelectRow
        {
            get;
            private set;
        }

        private string Status = null;
        private string NoStatus = null;
        public SelectorEquipment(string status,string noStatus)
        {
            InitializeComponent();
            this.Status = status;
            this.NoStatus = noStatus;

            LoadStatus();
            LoadEquipmentGroup();
        }

        private void LoadStatus()
        {
            string sql = @"DECLARE @t1 TABLE(rowIndex INT,String NVARCHAR(50))
INSERT INTO @t1
SELECT rowIndex,String FROM dbo.Ftn_GetTableFromString(@Code1,',')

DECLARE @t2 TABLE(rowIndex INT,String NVARCHAR(50))
INSERT INTO @t2
SELECT rowIndex,String FROM dbo.Ftn_GetTableFromString(@Code2,',')

SELECT Code,Name
FROM dbo.Set_SystemType WITH(NOLOCK) 
WHERE TypeCode='EquipmentStatus'
AND (NOT EXISTS(SELECT * FROM @t1) OR EXISTS(SELECT * FROM @t1 WHERE String=Set_SystemType.Code))
AND (NOT EXISTS(SELECT * FROM @t2) OR NOT EXISTS(SELECT * FROM @t2 WHERE String=Set_SystemType.Code))
ORDER BY Sequence";
            Data.Parameters parameters = new Data.Parameters()
                        .Add("Code1", Status)
                        .Add("Code2", NoStatus);
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

        private void LoadEquipmentGroup()
        {
            string sql = @"SELECT EquipmentGroupId,

       EquipmentGroupCode + '-' + ISNULL(EquipmentGroupDesc, '') AS EquipmentGroupName
FROM dbo.Bas_EquipmentGroup
ORDER BY EquipmentGroupCode";
            try
            {
                System.Data.DataTable dt = DB.DBHelper.GetDataTable(sql, null, null, true);
                System.Data.DataRow row = dt.NewRow();
                dt.Rows.InsertAt(row, 0);
                cmbEquipmentGroup.ItemsSource = dt.DefaultView;
                cmbEquipmentGroup.SelectedValuePath = "EquipmentGroupId";
                cmbEquipmentGroup.DisplayMemberPath = "EquipmentGroupName";
                cmbEquipmentGroup.SelectedIndex = 0;
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
                whereSql = whereSql + " AND EXISTS(SELECT * FROM dbo.Ftn_GetTableFromString(@Status,',') WHERE String=Bas_Equipment.[Status])";
                queryParameters.Add("Status", Status);
            }
            if (!string.IsNullOrEmpty(NoStatus))
            {
                whereSql = whereSql + " AND NOT EXISTS(SELECT * FROM dbo.Ftn_GetTableFromString(@Status1,',') WHERE String=Bas_Equipment.[Status])";
                queryParameters.Add("Status1", NoStatus);
            }
            if (txtWorkShop.Text != string.Empty)
            {
                try
                {
                    whereSql = whereSql + " AND Bas_Equipment.WorkShopId=@WorkShopId ";
                    queryParameters.Add("WorkShopId", txtWorkShop.Value);
                }
                catch (Exception ex)
                {
                    Component.MessageBox.MyMessageBox.ShowError(string.Format("车间{0}", ex.Message));
                    txtWorkShop.SetFoucs();
                    return;
                }
            }
            if (cmbEquipmentGroup.SelectedValue != null && cmbEquipmentGroup.SelectedValue.ToString().Trim() != string.Empty)
            {
                whereSql = whereSql + " AND Bas_Equipment.EquipmentGroupId=@EquipmentGroupId ";
                queryParameters.Add("EquipmentGroupId", cmbEquipmentGroup.SelectedValue);
            }
            if (cmbEquipmentModel.SelectedValue != null && cmbEquipmentModel.SelectedValue.ToString().Trim() != string.Empty)
            {
                whereSql = whereSql + " AND Bas_Equipment.EquipmentModelId=@EquipmentModelId ";
                queryParameters.Add("EquipmentModelId", cmbEquipmentModel.SelectedValue);
            }
            if (txtEquipmentCode.Text.Trim() != string.Empty)
            {
                whereSql = whereSql + " AND Bas_Equipment.EquipmentCode LIKE '%'+@EquipmentCode+'%' ";
                queryParameters.Add("EquipmentCode", txtEquipmentCode.Text.Trim());
            }
            if (cmbStatus.SelectedValue != null && cmbStatus.SelectedValue.ToString().Trim() != string.Empty)
            {
                whereSql = whereSql + " AND Bas_Equipment.[Status]=@Status2 ";
                queryParameters.Add("Status2", cmbStatus.SelectedValue);
            }

            string sql = string.Format(@"SELECT COUNT(*) FROM dbo.Bas_Equipment WITH(NOLOCK) {0}", whereSql);
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
SELECT ROW_NUMBER()OVER(ORDER BY Bas_Equipment.EquipmentCode DESC) AS RowIndex,
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

        private void cmbEquipmentGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cmbEquipmentModel.ItemsSource = null;
            if (cmbEquipmentGroup.SelectedValue != null && cmbEquipmentGroup.SelectedValue.ToString().Trim() != string.Empty)
            {
                string sql = @"SELECT EquipmentModelId,
	   EquipmentModelCode+'-'+ISNULL(EquipmentModelDesc,'') AS EquipmentModelName
FROM dbo.Bas_EquipmentModel WITH(NOLOCK) 
WHERE EquipmentGroupId=@EquipmentGroupId
ORDER BY EquipmentModelCode";
                Data.Parameters parameters = new Data.Parameters()
                            .Add("EquipmentGroupId", cmbEquipmentGroup.SelectedValue);
                try
                {
                    System.Data.DataTable dt = DB.DBHelper.GetDataTable(sql, parameters, null, true);
                    System.Data.DataRow row = dt.NewRow();
                    dt.Rows.InsertAt(row, 0);
                    cmbEquipmentModel.ItemsSource = dt.DefaultView;
                    cmbEquipmentModel.SelectedValuePath = "EquipmentModelId";
                    cmbEquipmentModel.DisplayMemberPath = "EquipmentModelName";
                    cmbEquipmentModel.SelectedIndex = 0;
                }
                catch (Exception ex)
                {
                    Component.MessageBox.MyMessageBox.ShowError(ex.Message);
                }
            }
        }
    }
}
