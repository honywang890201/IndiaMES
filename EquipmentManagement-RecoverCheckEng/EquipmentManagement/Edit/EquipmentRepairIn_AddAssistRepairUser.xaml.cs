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
using System.Data;
using Data;

namespace EquipmentManagement.Edit
{
    /// <summary>
    /// EquipmentRepairIn_AddAssistRepairUser.xaml 的交互逻辑
    /// </summary>
    public partial class EquipmentRepairIn_AddAssistRepairUser : Component.ControlsEx.Window
    {
        public string Ids
        {
            get;
            private set;
        }

        public string Codes
        {
            get;
            private set;
        }

        public long? RepairTypeId
        {
            get;
            private set;
        }

        public EquipmentRepairIn_AddAssistRepairUser(string ids,string codes,long? repairTypeId)
        {
            InitializeComponent();
            this.Codes = codes;
            this.Ids = ids;
            this.RepairTypeId = repairTypeId;

            this.LoadComplete += new RoutedEventHandler((sender, e) =>
            {
                Component.MaskBusy.Busy(grid, "正在加载数据。。。");
                new TaskFactory().StartNew<Result<DataTable>>(() =>
                {
                    return LoadValue();
                }).ContinueWith(result =>
                {
                    if (result.Result.HasError)
                    {
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            Component.MessageBox.MyMessageBox.ShowError(result.Result.Message);
                            this.Close();
                        }));
                    }
                    else
                    {
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            matrix2.ItemsSource = result.Result.Value.DefaultView;
                        }));
                    }
                    Component.MaskBusy.Hide(grid);
                });
            });

        }

        private Result<System.Data.DataTable> LoadValue()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("<xml>");
            if (!string.IsNullOrEmpty(Ids))
            {
                foreach (string c in Ids.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    builder.AppendLine(string.Format("<row>{0}</row>", c));
                }
            }
            builder.AppendLine("</xml>");

            string sql = @"DECLARE @t TABLE(
	Id BIGINT
)

INSERT INTO @t(Id)
SELECT DISTINCT a.Id
FROM(
	SELECT t.value('.[1]','BIGINT') AS Id
	FROM @Xml.nodes('/xml/row') AS c(t)
) a WHERE a.Id IS NOT NULL

SELECT Set_User.SysUserId,
			   Set_User.SysUserCode+'-'+ISNULL(Set_User.SysUserName,'') AS UserName
FROM Set_User WITH(NOLOCK) 
WHERE EXISTS(
	SELECT * FROM @t WHERE Id=Set_User.SysUserId
)";
            Parameters parameters = new Parameters();
            parameters.Add("Xml", builder.ToString(), SqlDbType.Xml, int.MaxValue);

            Result<System.Data.DataTable> Result = new Result<DataTable>();
            try
            {
                Result.Value = DB.DBHelper.GetDataTable(sql, parameters, null, false);
                Result.HasError = false;
            }
            catch(Exception e)
            {
                Result.Message = e.Message;
                Result.HasError = true;
            }

            return Result;
        }

        private void btnQuery_Click(object sender, RoutedEventArgs e)
        {
            string sql = @"DECLARE @ParamValue NVARCHAR(50)

SELECT @ParamValue=Set_SysParam.ParamValue
FROM dbo.Set_SysParam WITH(NOLOCK) 
LEFT JOIN dbo.Set_SysParamGroup WITH(NOLOCK) ON dbo.Set_SysParamGroup.ParamGroupId = dbo.Set_SysParam.ParamGroupId
WHERE Set_SysParamGroup.ParamGroupCode='Department'
AND Set_SysParam.ParamCode='RepairDepartmentCode'


SELECT Set_User.SysUserId,
			   Set_User.SysUserCode+'-'+ISNULL(Set_User.SysUserName,'') AS UserName
FROM Set_User WITH(NOLOCK) 
INNER JOIN dbo.Bas_DepartmentPosition WITH(NOLOCK) ON dbo.Bas_DepartmentPosition.DepartmentPositionId = dbo.Set_User.DepartmentPositionId
INNER JOIN dbo.Bas_Department WITH(NOLOCK) ON dbo.Bas_Department.DepartmentId = dbo.Set_User.DepartmentId
WHERE Bas_Department.DepartmentCode=@ParamValue
AND Bas_DepartmentPosition.DepartmentPositionCode NOT IN('Director','DeputyDirector','Manager','DeputyManager','Supervisor','DeputySupervisor')
AND (
	@RepairTypeId IS NULL OR EXISTS(
		SELECT * FROM dbo.Set_User_RepairType WHERE RepairTypeId=@RepairTypeId AND SysUserId=Set_User.SysUserId
	)
)
AND Set_User.SysUserCode LIKE '%'+@UserCode+'%'
ORDER BY Set_User.SysUserCode";
            Parameters parameters = new Parameters();
            parameters.Add("UserCode", txtUserName.Text.Trim());
            parameters.Add("RepairTypeId", RepairTypeId.HasValue ? (object)RepairTypeId.Value : DBNull.Value);

            Component.MaskBusy.Busy(grid, "正在查询数据。。。");
            new TaskFactory().StartNew<Result<DataTable>>(() =>
            {
                Result<DataTable> r = new Result<DataTable>();
                try
                {
                    r.Value=DB.DBHelper.GetDataTable(sql, parameters, null, false);
                    r.HasError = false;
                    return r;
                }
                catch(Exception ex)
                {
                    r.HasError = true;
                    r.Message = ex.Message;
                    return r;
                }
            }).ContinueWith(result =>
            {
                this.Dispatcher.Invoke(new Action(() =>
                {
                    if (result.Result.HasError)
                    {
                        Component.MessageBox.MyMessageBox.ShowError(result.Result.Message);
                        return;
                    }
                    else
                    {
                        if ((matrix2.ItemsSource as DataView).Count < 1)
                        {
                            matrix1.ItemsSource = result.Result.Value.DefaultView;
                        }
                        else
                        {
                            for (int i = 0; i < result.Result.Value.Rows.Count; i++)
                            {
                                if ((from source in (matrix2.ItemsSource as DataView).Table.AsEnumerable()
                                     where source.Field<long>("SysUserId") == result.Result.Value.Rows[i].Field<long>("SysUserId")
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

                Component.MaskBusy.Hide(grid);
            });
        }

        private void btnAdd_LinkClick(object value, object parameter)
        {
            DataRowView row = parameter as DataRowView;
            (matrix2.ItemsSource as DataView).Table.ImportRow(row.Row);
            (matrix1.ItemsSource as DataView).Table.Rows.Remove(row.Row);
            if (matrix2.ItemsSource != null)
            {
                DataView view = matrix2.ItemsSource as DataView;
                if (view.Count > 0)
                {
                    (matrix2.View as Component.ControlsEx.TableView).FocusedRowHandle = view.Count - 1;
                }
            }
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

        private void btnSure_Click(object sender, RoutedEventArgs e)
        {

            if (Component.MessageBox.MyMessageBox.ShowQuestion(string.Format("确定保存吗？")) != MessageBoxResult.OK)
                return;

            List<string> l1 = new List<string>();
            List<string> l2 = new List<string>();
            foreach (DataRowView row in matrix2.ItemsSource as DataView)
            {
                l1.Add(row["SysUserId"].ToString().Trim());
                l2.Add(row["UserName"].ToString().Trim());
            }

            Ids = string.Join(";", l1);
            Codes = string.Join(";", l2);
            this.DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
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
