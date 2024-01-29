using Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BoxSNImport
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class UserControl1 : Component.Controls.User.UserVendor
    {
        public UserControl1(Framework.SystemAuthority authority) :
            base(authority)
        {
            InitializeComponent();
            dataGrid.LoadingRow += new EventHandler<DataGridRowEventArgs>((sender, e) =>
            {
                e.Row.Header = e.Row.GetIndex() + 1;
            });
            dataGridQuery.LoadingRow += new EventHandler<DataGridRowEventArgs>((sender, e) =>
            {
                e.Row.Header = e.Row.GetIndex() + 1;
            });
        }

        private void btnSelectExcel_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Filter = "EXCEL|*.xls;*.xlsx"; //
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                int handle = Component.MaskBusy.Busy(root, "正在读取数据...", dialog.FileName);
                System.Threading.Tasks.Task<DataTable>.Factory.StartNew(() => { return WinAPI.File.ExcelHelper.Read(dialog.FileName); })
                    .ContinueWith(result =>
                    {
                        if (result.Result == null)
                        {
                            dataGrid.ItemsSource = null;
                        }
                        else
                        {
                            dataGrid.ItemsSource = result.Result.DefaultView;
                        }
                        Component.MaskBusy.Hide(root, handle);
                    }, Framework.App.Scheduler);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (tbMO.Value == null || tbMO.Value.ToString() == string.Empty)
                {
                    MessageBox.Show("请输入工单！", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                    tbMO.SetFoucs();
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                tbMO.SetFoucs();
                return;
            }

            if (dataGrid.ItemsSource == null)
            {
                MessageBox.Show("请选择要导入的Excel文件！", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                btnSelectExcel.Focus();
                return;
            }

            DataTable table = (dataGrid.ItemsSource as DataView).Table;
            if (table.Rows.Count < 1)
            {
                MessageBox.Show("没有要导入的数据！", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                btnSelectExcel.Focus();
                return;
            }

            if (MessageBox.Show(string.Format("确定保存数据？(共{0}行)", table.Rows.Count), "提示", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.Cancel)
                return;



            int handle = Component.MaskBusy.Busy(root, "正在保存数据...", string.Format("共{0}行", table.Rows.Count));
            System.Threading.Tasks.Task<Result>.Factory.StartNew(() =>
            {
                Result result = new Result();
                result.HasError = false;

                Parameters parameters = new Parameters();
                parameters.Add("UserId", Framework.App.User.UserId);
                parameters.Add("MOId", tbMO.Value);
                parameters.Add("xml", WinAPI.File.XMLHelper.Convert(table), SqlDbType.Xml, int.MaxValue);
                parameters.Add("Return_Message", DBNull.Value, SqlDbType.NVarChar, int.MaxValue, ParameterDirection.Output);
                parameters.Add("Return_Value", 0, SqlDbType.Int, ParameterDirection.ReturnValue);
                try
                {
                    parameters = DB.DBHelper.ExecuteParameters("Prd_BoxSN_Import", parameters, ExecuteType.StoredProcedure);

                    string message = string.Empty;
                    if (parameters["Return_Message"] != null && parameters["Return_Message"] != DBNull.Value)
                    {
                        message = parameters["Return_Message"].ToString();
                    }

                    if (parameters["Return_Value"] != null && parameters["Return_Value"] != DBNull.Value)
                    {
                        int ret = (int)parameters["Return_Value"];
                        if (ret == 1)
                        {
                            result.HasError = false;
                            if (!string.IsNullOrEmpty(message))
                                result.Message = message;
                            else
                                result.Message = "BoxSN导入成功！";
                            return result;
                        }
                        else
                        {
                            result.HasError = true;
                            if (!string.IsNullOrEmpty(message))
                                result.Message = message;
                           else
                                result.Message = "BoxSN导入失败！";
                            return result;
                        }
                    }
                    else
                    {
                        result.HasError = true;
                        if (!string.IsNullOrEmpty(message))
                            result.Message = message;
                        else
                            result.Message = "BoxSN导入失败！";
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    result.HasError = true;
                    result.Message = ex.Message;
                    return result;
                }



            }).ContinueWith(result =>
            {
                if (result.Result.HasError)
                {
                    MessageBox.Show(result.Result.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show(result.Result.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    dataGrid.ItemsSource = null;
                }
                Component.MaskBusy.Hide(root, handle);
            }, Framework.App.Scheduler);
        }

        private void btnQuery_Click(object sender, RoutedEventArgs e)
        {
            long? moId = null;
            try
            {
                if (tbQueryMO.Value != null)
                {
                    moId = (long)tbQueryMO.Value;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                tbQueryMO.SetFoucs();
                return;
            }

            string headerCode = tbQueryHeaderCode.Text.Trim();
            string BoxSNStart = tbBoxSNStart.Text.Trim();
            string BoxSNEnd = tbBoxSNEnd.Text.Trim();

            if (string.IsNullOrEmpty(headerCode) && tbQueryMO.Value == null && string.IsNullOrEmpty(BoxSNStart) && string.IsNullOrEmpty(BoxSNEnd))
            {
                MessageBox.Show("请输入查询条件！", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string sql = @"SELECT b.HeaderCode,
	   c.MOCode,
	   a.BoxSN,
       a.BoxSN1,
	   a.IsUsed,
	   a.CreateDateTime,
	   b.Status,
	   b.DeleteDateTime,
	   b.DeleteNote
FROM dbo.Bas_MO_BoxSN a  WITH(NOLOCK) 
LEFT JOIN dbo.Bas_MO_BoxSN_ImportHeader b  WITH(NOLOCK)  ON b.HeaderCode = a.HeaderCode
LEFT JOIN dbo.Bas_MO  c WITH(NOLOCK) ON c.MOId = a.MOId
WHERE 1=1 ";
            if (!string.IsNullOrEmpty(headerCode))
            {
                sql = sql + " AND b.HeaderCode=@headerCode ";
            }
            if (tbQueryMO.Value != null)
            {
                sql = sql + " AND a.MOId=@MOId ";
            }
            if (!string.IsNullOrEmpty(BoxSNStart))
            {
                sql = sql + " AND a.BoxSN>=@BoxSNStart ";
            }
            if (!string.IsNullOrEmpty(BoxSNEnd))
            {
                sql = sql + " AND a.BoxSN<=@BoxSNEnd ";
            }
            Parameters parameters = new Parameters();
            parameters.Add("headerCode", headerCode);
            parameters.Add("MOId", moId.HasValue ? moId.Value : (object)DBNull.Value);
            parameters.Add("BoxSNStart", BoxSNStart);
            parameters.Add("BoxSNEnd", BoxSNEnd);

            if (MessageBox.Show(string.Format("确定查询数据数据？"), "提示", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.Cancel)
                return;

            int handle = Component.MaskBusy.Busy(root, "正在查询数据...");
            System.Threading.Tasks.Task<Result<DataTable>>.Factory.StartNew(() =>
            {
                Result<DataTable> result = new Result<DataTable>();
                result.HasError = false;
                DataTable dt = null;
                try
                {
                    dt = DB.DBHelper.GetDataTable(sql, parameters, ExecuteType.Text);
                    result.HasError = false;
                    result.Value = dt;
                }
                catch (Exception ex)
                {
                    result.HasError = true;
                    result.Message = ex.Message;
                }
                return result;

            }).ContinueWith(result =>
            {

                if (result.Result.HasError)
                {
                    MessageBox.Show(result.Result.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    dataGridQuery.ItemsSource = result.Result.Value.DefaultView;
                }
                Component.MaskBusy.Hide(root, handle);
            }, Framework.App.Scheduler);
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            DataView view = dataGridQuery.ItemsSource as DataView;

            if (view == null || view.Count < 1)
            {
                MessageBox.Show("没有获取到要导出的数据！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            System.Windows.Forms.SaveFileDialog fileDialog = new System.Windows.Forms.SaveFileDialog();
            fileDialog.Filter = "EXCEL|*.xls";
            fileDialog.RestoreDirectory = true;
            fileDialog.FileName = "Query";

            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                int handle = Component.MaskBusy.Busy(root, "正在导出数据...");
                System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    WinAPI.File.ExcelHelper.Write(fileDialog.FileName, view.Table);
                }).ContinueWith(bi =>
                {
                    Component.MaskBusy.Hide(root, handle);
                });
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!this.CheckAuthority('3'))
            {
                MessageBox.Show(string.Format("用户[{0} - {1}]无删除权限", Framework.App.User.UserCode, Framework.App.User.UserName), "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string sql = null;
            Parameters parameters = null;

            if (rbtn1.IsChecked.Value)
            {
                string headerCode = tbDeleteHeaderCode.Text.Trim();
                string note = tbDeleteNote1.Text.Trim();
                if (string.IsNullOrEmpty(headerCode))
                {
                    MessageBox.Show("请输入要删除的HeaderCode！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    tbDeleteHeaderCode.SelectAll();
                    tbDeleteHeaderCode.Focus();
                    return;
                }
                if (string.IsNullOrEmpty(note))
                {
                    MessageBox.Show("请输入删除备注！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    tbDeleteNote1.SelectAll();
                    tbDeleteNote1.Focus();
                    return;
                }

                sql = "SELECT HeaderCode,Status,Count FROM dbo.Bas_MO_BoxSN_ImportHeader  WITH(NOLOCK) WHERE HeaderCode=@headerCode";
                parameters = new Parameters();
                parameters.Add("headerCode", headerCode);


                DataTable dt = null;
                try
                {
                    dt = DB.DBHelper.GetDataTable(sql, parameters, ExecuteType.Text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (dt == null || dt.Rows.Count < 1)
                {
                    MessageBox.Show("HeaderCode[" + headerCode + "]错误！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (dt.Rows[0]["Status"].ToString().Trim().ToUpper() == "Delete".ToUpper())
                {
                    MessageBox.Show("HeaderCode[" + headerCode + "]已被删除，请不要重复删除！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (dt.Rows[0]["Status"].ToString().Trim().ToUpper() != "Normal".ToUpper())
                {
                    MessageBox.Show("HeaderCode[" + headerCode + "]状态为[" + dt.Rows[0]["Status"].ToString().Trim() + "]，不能删除！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (MessageBox.Show(string.Format("HeaderCode[{0}]共查询到[{1}]个BoxSN，确定要全部删除吗？", headerCode, dt.Rows[0]["Count"].ToString().Trim()), "提示", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.Cancel)
                    return;

                sql = @"SELECT TOP 1 BoxSN FROM dbo.Bas_MO_BoxSN  WITH(NOLOCK) 
WHERE HeaderCode=@headerCode AND EXISTS(SELECT * FROM dbo.Inp_Box WITH(NOLOCK) WHERE BoxSN=Bas_MO_BoxSN.BoxSN)";
                parameters = new Parameters();
                parameters.Add("headerCode", headerCode);
                try
                {
                    dt = DB.DBHelper.GetDataTable(sql, parameters, ExecuteType.Text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (dt.Rows.Count > 0)
                {
                    MessageBox.Show("BoxSN[" + dt.Rows[0]["BoxSN"].ToString() + "]已在使用中，不能删除！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                sql = @"
UPDATE dbo.Bas_MO_BoxSN_ImportHeader SET Status='Delete',DeleteUserId=@UserId,DeleteNote=@note,DeleteDateTime=GETDATE() WHERE HeaderCode=@headerCode
	DELETE dbo.Bas_MO_BoxSN WHERE HeaderCode=@headerCode";
                parameters = new Parameters();
                parameters.Add("headerCode", headerCode);
                parameters.Add("UserId", Framework.App.User.UserId);
                parameters.Add("note", note, SqlDbType.NVarChar, int.MaxValue);

                try
                {
                    DB.DBHelper.Execute(sql, parameters, ExecuteType.Text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                MessageBox.Show("HeaderCode[" + headerCode + "]删除成功！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            else if (rbtn2.IsChecked.Value)
            {
                string BoxSNStart = tbDeleteBoxSNStart.Text.Trim();
                string BoxSNEnd = tbDeleteBoxSNEnd.Text.Trim();
                string note = tbDeleteNote2.Text.Trim();
                try
                {
                    if (tbDeleteMO.Value == null)
                    {
                        MessageBox.Show("请输入要删除的工单！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                        tbDeleteMO.SetFoucs();
                        return;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    tbDeleteMO.SetFoucs();
                    return;
                }
                if (string.IsNullOrEmpty(BoxSNStart))
                {
                    MessageBox.Show("请输入要删除的BoxSN起始！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    tbDeleteBoxSNStart.SelectAll();
                    tbDeleteBoxSNStart.Focus();
                    return;
                }
                if (string.IsNullOrEmpty(BoxSNEnd))
                {
                    MessageBox.Show("请输入要删除的BoxSN截止！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    tbDeleteBoxSNEnd.SelectAll();
                    tbDeleteBoxSNEnd.Focus();
                    return;
                }
                if (string.IsNullOrEmpty(note))
                {
                    MessageBox.Show("请输入删除备注！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    tbDeleteNote2.SelectAll();
                    tbDeleteNote2.Focus();
                    return;
                }

                sql = "SELECT COUNT(*) FROM Bas_MO_BoxSN  WITH(NOLOCK) WHERE MOId=@MOId AND BoxSN>=@BoxSNStart AND BoxSN<=@BoxSNEnd";
                parameters = new Parameters();
                parameters.Add("MOId", tbDeleteMO.Value);
                parameters.Add("BoxSNStart", BoxSNStart);
                parameters.Add("BoxSNEnd", BoxSNEnd);


                int count = 0;
                try
                {
                    count = (int)DB.DBHelper.GetScalar(sql, parameters, ExecuteType.Text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (count < 1)
                {
                    MessageBox.Show("没有查询到要删除的数据！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (MessageBox.Show(string.Format("共查询到[{0}]个BoxSN，确定要全部删除吗？", count), "提示", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.Cancel)
                    return;

                sql = @"SELECT TOP 1 BoxSN FROM Bas_MO_BoxSN  WITH(NOLOCK) WHERE MOId=@MOId AND BoxSN>=@BoxSNStart AND BoxSN<=@BoxSNEnd AND 
EXISTS(SELECT * FROM dbo.Inp_Box WITH(NOLOCK) WHERE BoxSN=Bas_MO_BoxSN.BoxSN)";
                parameters = new Parameters();
                parameters.Add("MOId", tbDeleteMO.Value);
                parameters.Add("BoxSNStart", BoxSNStart);
                parameters.Add("BoxSNEnd", BoxSNEnd);

                DataTable dt = null;
                try
                {
                    dt = DB.DBHelper.GetDataTable(sql, parameters, ExecuteType.Text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (dt.Rows.Count > 0)
                {
                    MessageBox.Show("BoxSN[" + dt.Rows[0]["BoxSN"].ToString() + "]已在使用中，不能删除！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                sql = @"
DELETE Bas_MO_BoxSN  WHERE MOId=@MOId AND BoxSN>=@BoxSNStart AND BoxSN<=@BoxSNEnd ";
                parameters = new Parameters();
                parameters.Add("MOId", tbDeleteMO.Value);
                parameters.Add("BoxSNStart", BoxSNStart);
                parameters.Add("BoxSNEnd", BoxSNEnd);
                parameters.Add("UserId", Framework.App.User.UserId);
                parameters.Add("Note", note, SqlDbType.NVarChar, int.MaxValue);

                try
                {
                    DB.DBHelper.Execute(sql, parameters, ExecuteType.Text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                MessageBox.Show("删除成功！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
        }

        private void rbtn1_Checked(object sender, RoutedEventArgs e)
        {
            if (gridHeaderCode != null) gridHeaderCode.Visibility = Visibility.Visible;
            if (gridBoxSN != null) gridBoxSN.Visibility = Visibility.Collapsed;
        }

        private void rbtn2_Checked(object sender, RoutedEventArgs e)
        {
            if (gridHeaderCode != null) gridHeaderCode.Visibility = Visibility.Collapsed;
            if (gridBoxSN != null) gridBoxSN.Visibility = Visibility.Visible;
        }
    }
}
