using Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MySql.Data.MySqlClient;
namespace macfrommysql
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
        }

        public void Import()
        {
            DataView view = dataGrid.GetItemSource();
            try
            {
                if (tbMO.Value == null)
                {
                    Component.MessageBox.MyMessageBox.ShowError("请输入工单！");
                    return;
                }
            }
            catch(Exception ex)
            {
                Component.MessageBox.MyMessageBox.ShowError(ex.Message);
                return;
            }
            long moId = (long)tbMO.Value;
            if (view==null||view.Count < 1)
            {
                Component.MessageBox.MyMessageBox.ShowError("没有获取到MAC数据！");
                return;
            }

            List<Dictionary<string, object>> l = new List<Dictionary<string, object>>();
            int rowNo = 0;
            foreach(DataRowView row in view)
            {
                rowNo++;
                Dictionary<string, object> dictionary = new Dictionary<string, object>();
                dictionary.Add("rowNo", rowNo);
                foreach (DataColumn col in view.Table.Columns)
                {
                    if(!dictionary.ContainsKey(col.ColumnName.ToUpper().Trim()))
                    {
                        dictionary.Add(col.ColumnName.ToUpper().Trim(), row[col.ColumnName]);
                    }
                }
                l.Add(dictionary);
            }

            if (MessageBox.Show(string.Format("确定保存数据？(共{0}行)", rowNo), "提示", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.Cancel)
                return;

            Dictionary<string, List<Dictionary<string, object>>> items = new Dictionary<string, List<Dictionary<string, object>>>();
            items.Add("WIFIMAC", l);
            string xml = WinAPI.File.XMLHelper.CreateXML(null, items, null);

            Parameters parameters = new Parameters()
                .Add("UserId", Framework.App.User.UserId)
                .Add("MOId", moId)
                .Add("xml", xml, SqlDbType.Xml, int.MaxValue)
                .Add("Return_Message", DBNull.Value, SqlDbType.NVarChar, int.MaxValue, ParameterDirection.Output)
                .Add("return_value", DBNull.Value, SqlDbType.Int, 50, ParameterDirection.ReturnValue);
                //.Add("MacCheck", ckbMAC.IsChecked.HasValue ? ckbMAC.IsChecked.Value : false)
               // .Add("MacStartCheck", ckbMacStart.IsChecked.HasValue ? ckbMacStart.IsChecked.Value : false)
               // .Add("MacEndCheck", ckbMacEnd.IsChecked.HasValue ? ckbMacEnd.IsChecked.Value : false)
                //.Add("GponSNCheck", ckbGponSN.IsChecked.HasValue ? ckbGponSN.IsChecked.Value : false)
               // .Add("DeviceSerialNumberCheck", ckbDeviceSerialNumber.IsChecked.HasValue ? ckbDeviceSerialNumber.IsChecked.Value : false)
                //.Add("DSNCheck", ckbDSN.IsChecked.HasValue ? ckbDSN.IsChecked.Value : false);

            int handle = Component.MaskBusy.Busy(root, "正在保存数据...");
            System.Threading.Tasks.Task<Result<Parameters>>.Factory.StartNew(() =>
            {
                Result<Parameters> result=new Result<Parameters>();
                result.HasError=false;
                try
                {
                    parameters = DB.DBHelper.ExecuteParameters("Bas_P60WIFIMac_Import", parameters, ExecuteType.StoredProcedure);
                }
                catch (Exception ex)
                {
                    result.HasError = true;
                    result.Message = ex.Message;
                }
                result.Value = parameters;
                return result;

            }).ContinueWith(r =>
            {
                if(r.Result.HasError)
                {
                    MessageBox.Show(r.Result.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (int.Parse(r.Result.Value["return_value"].ToString()) != 1)
                {
                    MessageBox.Show(r.Result.Value["Return_Message"].ToString(), "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show(r.Result.Value["Return_Message"].ToString(), "提示", MessageBoxButton.OK, MessageBoxImage.Question);

                    dataGrid.Value = null;
                }
                

                Component.MaskBusy.Hide(root, handle);
            }, Framework.App.Scheduler);
        }

        public void Update()
        {
            DataView view = null;// dataGridUpdate.GetItemSource();

            if (view == null || view.Count < 1)
            {
                Component.MessageBox.MyMessageBox.ShowError("没有获取到MAC数据！");
                return;
            }

            List<Dictionary<string, object>> l = new List<Dictionary<string, object>>();
            int rowNo = 0;
            foreach (DataRowView row in view)
            {
                rowNo++;
                Dictionary<string, object> dictionary = new Dictionary<string, object>();
                dictionary.Add("rowNo", rowNo);
                foreach (DataColumn col in view.Table.Columns)
                {
                    if (!dictionary.ContainsKey(col.ColumnName.ToUpper().Trim()))
                    {
                        dictionary.Add(col.ColumnName.ToUpper().Trim(), row[col.ColumnName]);
                    }
                }
                l.Add(dictionary);
            }

            if (MessageBox.Show(string.Format("确定更新数据？(共{0}行)", rowNo), "提示", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.Cancel)
                return;

            Dictionary<string, List<Dictionary<string, object>>> items = new Dictionary<string, List<Dictionary<string, object>>>();
            items.Add("macs", l);
            string xml = WinAPI.File.XMLHelper.CreateXML(null, items, null);


            Parameters parameters = new Parameters()
                .Add("UserId", Framework.App.User.UserId)
                .Add("xml", xml, SqlDbType.Xml, int.MaxValue)
                .Add("Return_Message", DBNull.Value, SqlDbType.NVarChar, int.MaxValue, ParameterDirection.Output)
                .Add("return_value", DBNull.Value, SqlDbType.Int, 50, ParameterDirection.ReturnValue);

            int handle = Component.MaskBusy.Busy(root, "正在更新数据...");
            System.Threading.Tasks.Task<Result<Parameters>>.Factory.StartNew(() =>
            {
                Result<Parameters> result = new Result<Parameters>();
                result.HasError = false;
                try
                {
                    parameters = DB.DBHelper.ExecuteParameters("Prd_MO_Mac_Update", parameters, ExecuteType.StoredProcedure);
                }
                catch (Exception ex)
                {
                    result.HasError = true;
                    result.Message = ex.Message;
                }
                result.Value = parameters;
                return result;

            }).ContinueWith(r =>
            {
                if (r.Result.HasError)
                {
                    MessageBox.Show(r.Result.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (int.Parse(r.Result.Value["return_value"].ToString()) != 1)
                {
                    MessageBox.Show(r.Result.Value["Return_Message"].ToString(), "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show(r.Result.Value["Return_Message"].ToString(), "提示", MessageBoxButton.OK, MessageBoxImage.Question);

                    dataGrid.Value = null;
                }


                Component.MaskBusy.Hide(root, handle);
            }, Framework.App.Scheduler);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Import();
        }

        private void btnUpdateSave_Click(object sender, RoutedEventArgs e)
        {
            Update();
        }

        private void btnQuery_Click(object sender, RoutedEventArgs e)
        {
            MySqlDataReader reader = null;
            String mysqlStr = null; //"Database=ott_package;Data Source=192.168.0.119;User Id=root;Password=123456;pooling=false;CharSet=utf8;port=3306";
            MySqlConnection mysql = null;// new MySqlConnection(mysqlStr);
            MySqlCommand mySqlCommand = null;// new MySqlCommand("SELECT COUNT(*)FROM restore_order_all_t", mysql);
            string MESServerIp = null, FactoryServerIp = null;
            Match m = Regex.Match(Framework.App.ServiceUrl, @"\d{1,3}.\d{1,3}.\d{1,3}.\d{1,3}");
            if (m.Success)
            {
                MESServerIp = m.Value;
            }
            string[] arr = MESServerIp.Split('.');
            if (arr[2] == "0")
            {
                FactoryServerIp = "192.168.0.24";
            }
            if (arr[2] == "2")
            {
                FactoryServerIp = "192.168.2.11";
            }
            string headerCode = string.Empty;// tbQueryHeaderCode.Text.Trim();
            string macStart = tbMacStart.Text.Trim();
            string macEnd = tbMacEnd.Text.Trim();

            if (string.IsNullOrEmpty(headerCode) && string.IsNullOrEmpty(tbQueryMO.Text) && string.IsNullOrEmpty(macStart) && string.IsNullOrEmpty(macEnd))
            {
                Component.MessageBox.MyMessageBox.ShowWarning("请输入查询条件！");
                return;
            }
            DataTable inv = new DataTable();
            mysqlStr = "Database=serialnumber;Data Source=" + FactoryServerIp + ";User Id=root;Password=123456;pooling=false;CharSet=utf8;port=3306";
            mysql = new MySqlConnection(mysqlStr);
            mySqlCommand = new MySqlCommand("SELECT serial_number,stb_id FROM product_vn_dvt_gx6616_32 where order_number='WX202006120203'", mysql);
            try
            {
                //获得读取结果
                mysql.Open();
                MySqlDataReader mysqldr = mySqlCommand.ExecuteReader();
                inv.Load(mysqldr);
                //while (mysqldr.Read())//mysqldr.Read()返回的是bool值，意在判断是否有下一条数据
                //{
                //    mysqldr["serial_number"].ToString();
                //    mysqldr["stb_id"].ToString();
                //}
            }
            catch (Exception W)
            {
                
            }
            mysql.Close();

            int handle = Component.MaskBusy.Busy(root, "正在查询数据...");
            System.Threading.Tasks.Task<Result<DataTable>>.Factory.StartNew(() =>
            {
                Result<DataTable> result = new Result<DataTable>() { HasError = false };
                //DataTable dt = null;
                //try
                //{
                //    //dt = DB.DBHelper.GetDataTable(sql, parameters, ExecuteType.Text);
                //}
                //catch (Exception ex)
                //{
                //    result.HasError = true;
                //    result.Message = ex.Message;
                //}
                result.Value = inv;
                return result;

            }).ContinueWith(r =>
            {

                if (r.Result.HasError)
                {
                    MessageBox.Show(r.Result.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    dataGridQuery.ItemsSource = (r.Result.Value).DefaultView;
                }


                Component.MaskBusy.Hide(root, handle);
            }, Framework.App.Scheduler);
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            DataView view = dataGridQuery.ItemsSource as DataView;

            if (view == null || view.Count < 1)
            {
                Component.MessageBox.MyMessageBox.ShowError("没有获取到要导出的数据！");
                return;
            }

            System.Windows.Forms.SaveFileDialog fileDialog = new System.Windows.Forms.SaveFileDialog();
            fileDialog.Filter = "EXCEL|*.xls";
            fileDialog.RestoreDirectory = true;
            fileDialog.FileName = "Query";

            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Component.MaskBusy.Busy(root, "正在导出数据");
                System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    DataTable exportSource = view.Table.Copy();
                    exportSource.Constraints.Clear();
                    for (int i = 0; i < exportSource.Columns.Count; i++)
                    {
                        DataColumn col = exportSource.Columns[i];
                        if (col.ColumnName.ToUpper().Trim().EndsWith("ID"))
                        {
                            exportSource.Columns.Remove(col);
                            i--;
                        }
                    }

                    try
                    {
                        WinAPI.File.ExcelHelper.Write(fileDialog.FileName, exportSource);
                    }
                    catch (Exception ex)
                    {
                        Component.MessageBox.MyMessageBox.ShowError(ex.Message);
                    }
                }).ContinueWith(bi =>
                {
                    Component.MaskBusy.Hide(root);
                });
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            //string headerCode = tbDeleteHeaderCode.Text.Trim();
           // string note = tbDeleteNote.Text.Trim();
            //if (string.IsNullOrEmpty(headerCode))
            //{
            //    Component.MessageBox.MyMessageBox.ShowError("请输入要删除的HeaderCode！");
            //    return;
            //}

//            string sql = "SELECT HeaderCode,MacMin,MacMax,Status,Count FROM dbo.Bas_MO_Mac_ImportHeader  WITH(NOLOCK) WHERE HeaderCode=@headerCode";
//            Parameters parameters = new Parameters().Add("headerCode", headerCode, SqlDbType.NVarChar, 50);
//            DataTable dt = null;
//            try
//            {
//                dt = DB.DBHelper.GetDataTable(sql, parameters, ExecuteType.Text);
//            }
//            catch(Exception ex)
//            {
//                Component.MessageBox.MyMessageBox.ShowError(ex.Message);
//                return;
//            }

//            if (dt == null || dt.Rows.Count < 1)
//            {
//                Component.MessageBox.MyMessageBox.ShowError("HeaderCode[" + headerCode + "]错误！");
//                return;
//            }
//            if(dt.Rows[0]["Status"].ToString().Trim().ToUpper()=="Delete".ToUpper())
//            {
//                Component.MessageBox.MyMessageBox.ShowError("HeaderCode[" + headerCode + "]已被删除，请不要重复删除！");
//                return;
//            }
//            if (dt.Rows[0]["Status"].ToString().Trim().ToUpper() != "Normal".ToUpper())
//            {
//                Component.MessageBox.MyMessageBox.ShowError("HeaderCode[" + headerCode + "]状态为[" + dt.Rows[0]["Status"].ToString().Trim() + "]，不能删除！");
//                return;
//            }

//            if (MessageBox.Show(string.Format("HeaderCode[{0}]共查询到[{1}]({2}~{3})个MAC，确定要全部删除吗？", headerCode, dt.Rows[0]["Count"].ToString().Trim(),
//                 dt.Rows[0]["MacMin"].ToString().Trim(), dt.Rows[0]["MacMax"].ToString().Trim()), "提示", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.Cancel)
//                return;

//            sql = @" SELECT TOP 1 Mac
//		FROM dbo.Bas_MO_Mac  WITH(NOLOCK) 
//		WHERE HeaderCode=@headerCode AND(IsUsed=1 OR EXISTS(SELECT * FROM dbo.Inp_Lot_Mac  WITH(NOLOCK) WHERE Mac=Bas_MO_Mac.Mac))";
//            try
//            {
//                dt = DB.DBHelper.GetDataTable(sql, parameters, ExecuteType.Text);
//            }
//            catch(Exception ex)
//            {
//                Component.MessageBox.MyMessageBox.ShowError(ex.Message);
//                return;
//            }
//            if(dt.Rows.Count>0)
//            {
//                Component.MessageBox.MyMessageBox.ShowError("Mac[" + dt.Rows[0]["Mac"].ToString() + "]已在使用中，不能删除！");
//                return;
//            }
//            sql = @"DELETE dbo.Bas_MO_Mac WHERE HeaderCode=@headerCode
//UPDATE dbo.Bas_MO_Mac_ImportHeader SET Status='Delete',DeleteUserId=@UserId,DeleteNote=@note,DeleteDateTime=GETDATE() WHERE HeaderCode=@headerCode";


//            parameters = new Parameters().Add("headerCode", headerCode, SqlDbType.NVarChar, 50)
//                .Add("UserId", Framework.App.User.UserId)
//                .Add("note", note, SqlDbType.NVarChar, int.MaxValue);

//            try
//            {
//                DB.DBHelper.Execute(sql, parameters, ExecuteType.Text);
//            }
//            catch (Exception ex)
//            {
//                Component.MessageBox.MyMessageBox.ShowError(ex.Message);
//                return;
//            }
//            Component.MessageBox.MyMessageBox.Show("HeaderCode[" + headerCode + "]删除成功！");
//            return;
        }

        private void dataGridQuery_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
        }
    }
}
