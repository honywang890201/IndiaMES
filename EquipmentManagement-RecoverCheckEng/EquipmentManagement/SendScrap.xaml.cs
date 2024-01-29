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
    /// SendScrap.xaml 的交互逻辑
    /// </summary>
    public partial class SendScrap : Component.Controls.User.UserVendor
    {
        public SendScrap(Framework.SystemAuthority authority) :
            base(authority)
        {
            InitializeComponent();
            LoadStatus();
            LoadEquipmentGroup();
            LoadRunStatus();
        }

        private void LoadStatus()
        {
            string sql = @"
SELECT Code,Name
FROM dbo.Set_SystemType WITH(NOLOCK) 
WHERE TypeCode='EquipmentStatus'
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

        private void LoadRunStatus()
        {
            string sql = @"
SELECT Code,Name
FROM dbo.Set_SystemType WITH(NOLOCK) 
WHERE TypeCode='EquipmentRunStatus'
ORDER BY Sequence";
            try
            {
                System.Data.DataTable dt = DB.DBHelper.GetDataTable(sql, null, null, true);
                System.Data.DataRow row = dt.NewRow();
                dt.Rows.InsertAt(row, 0);
                cmbRunStatus.ItemsSource = dt.DefaultView;
                cmbRunStatus.SelectedValuePath = "Code";
                cmbRunStatus.DisplayMemberPath = "Name";
                cmbRunStatus.SelectedIndex = 0;
            }
            catch (Exception e)
            {
                Component.MessageBox.MyMessageBox.ShowError(e.Message);
            }
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if(matrix2.ItemsSource==null)
            {
                return;
            }
            DataView view = matrix2.ItemsSource as DataView;
            if(view.Count<1)
            {
                Component.MessageBox.MyMessageBox.ShowError("请添加要报废的设备。");
                return;
            }

            StringBuilder xml = new StringBuilder();
            xml.Append(string.Format("<xml>"));
            foreach (DataRowView row in view)
            {
                xml.Append(string.Format("<row EquipmentId=\"{0}\" Comment=\"{1}\"/>",
                    row["EquipmentId"],
                    WinAPI.File.XMLHelper.StringFormat(row["Comment"].ToString())));
            }
            xml.Append(string.Format("</xml>"));

            if (Component.MessageBox.MyMessageBox.ShowQuestion(string.Format("确定报废吗？")) != MessageBoxResult.OK)
                return;


            string sql = "Prd_Equipment_Scrap";
            Data.Parameters parameters = new Parameters()
                .Add("Xml", xml.ToString(), SqlDbType.Xml, int.MaxValue)
                .Add("EquipmentScrapDesc", txtEquipmentScrapDesc.Text.Trim(), SqlDbType.NVarChar, int.MaxValue)
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
                (matrix2.ItemsSource as DataView).Table.Clear();
                txtEquipmentScrapDesc.Text = string.Empty;
                return;
            }
            else
            {
                Component.MessageBox.MyMessageBox.ShowError(parameters["Return_Message"].ToString());
            }
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

        private void btnQuery_Click(object sender, RoutedEventArgs e)
        {
            string sql = @"SELECT Bas_Equipment.EquipmentId,
	   Bas_EquipmentGroup.EquipmentGroupCode+'-'+ISNULL(dbo.Bas_EquipmentGroup.EquipmentGroupDesc,'') AS EquipmentGroupName,
	   Bas_EquipmentModel.EquipmentModelCode+'-'+ISNULL(Bas_EquipmentModel.EquipmentModelDesc,'') AS EquipmentModelName,
	   Bas_Equipment.EquipmentCode,
	   Bas_Equipment.EquipmentDesc,
	   Bas_Equipment.Status,
	   dbo.Ftn_GetStatusName(Bas_Equipment.Status,'EquipmentStatus') AS StatusName,
	   Bas_WorkShop.WorkShopCode+'-'+ISNULL(Bas_WorkShop.WorkShopDesc,'') AS WorkShopName,
	   Bas_Factory.FactoryCode+'-'+ISNULL(Bas_Factory.FactoryName,'') AS FactoryName,
	   Bas_Company.CompanyCode+'-'+ISNULL(Bas_Company.CompanyDesc,'') AS CompanyName,
	   MachineStatus.Status AS RunStatus,
	   dbo.Ftn_GetStatusName(MachineStatus.Status,'EquipmentRunStatus') AS RunStatusName,
	   MachineStatus.AbnoInfo,
       '' AS Comment,
	   CASE ISNULL(MachineStatus.Status,'') WHEN '3' THEN 'Red' ELSE NULL END AS RowForeground
FROM dbo.Bas_Equipment WITH(NOLOCK) 
LEFT JOIN dbo.Bas_EquipmentModel WITH(NOLOCK) ON dbo.Bas_EquipmentModel.EquipmentModelId = dbo.Bas_Equipment.EquipmentModelId
LEFT JOIN dbo.Bas_EquipmentGroup WITH(NOLOCK) ON dbo.Bas_EquipmentGroup.EquipmentGroupId = dbo.Bas_Equipment.EquipmentGroupId
LEFT JOIN dbo.Bas_WorkShop WITH(NOLOCK) ON dbo.Bas_WorkShop.WorkShopId = dbo.Bas_Equipment.WorkShopId
LEFT JOIN dbo.Bas_Factory WITH(NOLOCK) ON dbo.Bas_Factory.FactoryId = dbo.Bas_Equipment.FactoryId
LEFT JOIN dbo.Bas_Company WITH(NOLOCK) ON dbo.Bas_Company.CompanyId = dbo.Bas_Equipment.CompanyId
LEFT JOIN dbo.MachineStatus WITH(NOLOCK) ON Bas_Equipment.EquipmentCode=MachineStatus.EquiCode
WHERE Bas_Equipment.Status NOT IN('SendRepair','Scrap','WaitScrapSure') ";

            Data.Parameters parameters = new Parameters();
            if (txtWorkShop.Text != string.Empty)
            {
                try
                {
                    sql = sql + " AND Bas_Equipment.WorkShopId=@WorkShopId ";
                    parameters.Add("WorkShopId", txtWorkShop.Value);
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
                sql = sql + " AND Bas_Equipment.EquipmentGroupId=@EquipmentGroupId ";
                parameters.Add("EquipmentGroupId", cmbEquipmentGroup.SelectedValue);
            }
            if (cmbEquipmentModel.SelectedValue != null && cmbEquipmentModel.SelectedValue.ToString().Trim() != string.Empty)
            {
                sql = sql + " AND Bas_Equipment.EquipmentModelId=@EquipmentModelId ";
                parameters.Add("EquipmentModelId", cmbEquipmentModel.SelectedValue);
            }
            if (txtEquipmentCode.Text.Trim() != string.Empty)
            {
                sql = sql + " AND Bas_Equipment.EquipmentCode LIKE '%'+@EquipmentCode+'%' ";
                parameters.Add("EquipmentCode", txtEquipmentCode.Text.Trim());
            }
            if (cmbStatus.SelectedValue != null && cmbStatus.SelectedValue.ToString().Trim() != string.Empty)
            {
                sql = sql + " AND Bas_Equipment.[Status]=@Status2 ";
                parameters.Add("Status2", cmbStatus.SelectedValue);
            }
            if (cmbRunStatus.SelectedValue != null && cmbRunStatus.SelectedValue.ToString().Trim() != string.Empty)
            {
                sql = sql + " AND MachineStatus.[Status]=@Status3 ";
                parameters.Add("Status3", cmbRunStatus.SelectedValue);
            }

            int handleNo = Component.MaskBusy.Busy(grid, "正在查询数据。。。");
            new TaskFactory<Data.Result<DataTable>>().StartNew(() =>
            {
                Data.Result<DataTable> result = new Data.Result<DataTable>();
                try
                {
                    result.Value = DB.DBHelper.GetDataTable(sql, parameters, null, true);

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
                        if(matrix2.ItemsSource==null)
                        {
                            matrix2.ItemsSource = result.Result.Value.Clone().DefaultView;
                            matrix1.ItemsSource = result.Result.Value.DefaultView;
                        }
                        else
                        {
                            for (int i = 0; i < result.Result.Value.Rows.Count; i++)
                            {
                                long equipmentId = result.Result.Value.Rows[i].Field<long>("EquipmentId");

                                if ((from source in (matrix2.ItemsSource as DataView).Table.AsEnumerable()
                                     where source.Field<long>("EquipmentId") == equipmentId
                                     select source).Count() > 0)
                                {
                                    result.Result.Value.Rows.RemoveAt(i);
                                    i--;
                                }
                            }

                            matrix1.ItemsSource = result.Result.Value.DefaultView;
                        }
                        
                    }
                }));
                Component.MaskBusy.Hide(grid, handleNo);
            });
        }

        private void btnAdd_LinkClick(object vlaue, object parameter)
        {
            DataRowView row = parameter as DataRowView;
            (matrix2.ItemsSource as DataView).Table.ImportRow(row.Row);
            (matrix1.ItemsSource as DataView).Table.Rows.Remove(row.Row);
        }

        private void btnRemove_LinkClick(object value, object parameter)
        {
            DataRowView row = parameter as DataRowView;
            if (matrix1.ItemsSource != null)
            {
                (matrix1.ItemsSource as DataView).Table.ImportRow(row.Row);
                if (matrix1.ItemsSource != null)
                {
                    DataView view = matrix1.ItemsSource as DataView;
                    if (view.Count > 0)
                    {
                        (matrix1.View as Component.ControlsEx.TableView).FocusedRowHandle = view.Count - 1;
                    }
                }
            }
           (matrix2.ItemsSource as DataView).Table.Rows.Remove(row.Row);
        }

        private void btnAddAll_Click(object sender, RoutedEventArgs e)
        {
            if (matrix1.ItemsSource != null)
            {
                foreach (DataRowView row in (matrix1.ItemsSource as DataView))
                {
                    (matrix2.ItemsSource as DataView).Table.ImportRow(row.Row);
                }

                (matrix1.ItemsSource as DataView).Table.Rows.Clear();

                if (matrix2.ItemsSource != null)
                {
                    DataView view = matrix2.ItemsSource as DataView;
                    if (view.Count > 0)
                    {
                        (matrix2.View as Component.ControlsEx.TableView).FocusedRowHandle = view.Count - 1;
                    }
                }
            }
        }

        private void btnAddSelected_Click(object sender, RoutedEventArgs e)
        {
            if (matrix1.SelectedItems.Count > 0)
            {
                List<DataRowView> list = new List<DataRowView>();
                foreach (DataRowView row in matrix1.SelectedItems)
                {
                    list.Add(row);
                }

                for (int i = 0; i < list.Count; i++)
                {
                    (matrix2.ItemsSource as DataView).Table.ImportRow(list[i].Row);
                    (matrix1.ItemsSource as DataView).Table.Rows.Remove(list[i].Row);
                }


                if (matrix2.ItemsSource != null)
                {
                    DataView view = matrix2.ItemsSource as DataView;
                    if (view.Count > 0)
                    {
                        (matrix2.View as Component.ControlsEx.TableView).FocusedRowHandle = view.Count - 1;
                    }
                }
            }
        }

        private void btnDeleteAll_Click(object sender, RoutedEventArgs e)
        {
            if (matrix2.ItemsSource != null)
            {
                if (matrix1.ItemsSource != null)
                {
                    foreach (DataRowView row in (matrix2.ItemsSource as DataView))
                    {
                        (matrix1.ItemsSource as DataView).Table.ImportRow(row.Row);
                    }
                }

                (matrix2.ItemsSource as DataView).Table.Rows.Clear();
                if (matrix1.ItemsSource != null)
                {
                    DataView view = matrix1.ItemsSource as DataView;
                    if (view.Count > 0)
                    {
                        (matrix1.View as Component.ControlsEx.TableView).FocusedRowHandle = view.Count - 1;
                    }
                }
            }
        }

        private void btnDeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            if (matrix2.SelectedItems.Count > 0)
            {
                List<DataRowView> list = new List<DataRowView>();
                foreach (DataRowView row in matrix2.SelectedItems)
                {
                    list.Add(row);
                }

                for (int i = 0; i < list.Count; i++)
                {
                    if (matrix1.ItemsSource != null)
                    {
                        (matrix1.ItemsSource as DataView).Table.ImportRow(list[i].Row);
                    }
                    (matrix2.ItemsSource as DataView).Table.Rows.Remove(list[i].Row);
                }

                if (matrix1.ItemsSource != null)
                {
                    DataView view = matrix1.ItemsSource as DataView;
                    if (view.Count > 0)
                    {
                        (matrix1.View as Component.ControlsEx.TableView).FocusedRowHandle = view.Count - 1;
                    }
                }
            }
        }
    }
}
