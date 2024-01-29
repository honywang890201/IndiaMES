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

namespace JumpToStation
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class UserControl1 : Component.Controls.User.UserVendor
    {
        DataView vi;
        public UserControl1(Framework.SystemAuthority authority) :
            base(authority)
        {
            InitializeComponent();
        }

        public void Import()
        {
            DataView view = vi;
            if (view==null||view.Count < 1)
            {
                Component.MessageBox.MyMessageBox.ShowError("No any data！");
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
            Dictionary<string, List<Dictionary<string, object>>> items = new Dictionary<string, List<Dictionary<string, object>>>();
            items.Add("LOTSN", l);
            string xml = WinAPI.File.XMLHelper.CreateXML(null, items, null);
            Parameters parameters = new Parameters()
                .Add("UserId", Framework.App.User.UserId)
                .Add("LineId",Framework.App.Resource.LineId)
                .Add("ResourceId",Framework.App.Resource.ResourceId)
                .Add("ShiftTypeId",Framework.App.Resource.ShiftTypeId)
                .Add("PluginId", PluginId)
                .Add("xml", xml, SqlDbType.Xml, int.MaxValue)
                .Add("ToWorkStationCode",tbTOOP.Text)
                .Add("Return_Message", DBNull.Value, SqlDbType.NVarChar, int.MaxValue, ParameterDirection.Output)
                .Add("return_value", DBNull.Value, SqlDbType.Int, 50, ParameterDirection.ReturnValue);

            int handle = Component.MaskBusy.Busy(root, "saving...");
            System.Threading.Tasks.Task<Result<Parameters>>.Factory.StartNew(() =>
            {
                Result<Parameters> result=new Result<Parameters>();
                result.HasError=false;
                try
                {
                    parameters = DB.DBHelper.ExecuteParameters("Pro_Inp_WorkStation_Return", parameters, ExecuteType.StoredProcedure);
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
                    MessageBox.Show(r.Result.Message, "error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (int.Parse(r.Result.Value["return_value"].ToString()) != 1)
                {
                    MessageBox.Show(r.Result.Value["Return_Message"].ToString(), "error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show(r.Result.Value["Return_Message"].ToString(), "warming", MessageBoxButton.OK, MessageBoxImage.Question);

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
                Component.MessageBox.MyMessageBox.ShowError("no any data !!");
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
                    MessageBox.Show(r.Result.Message, "error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (int.Parse(r.Result.Value["return_value"].ToString()) != 1)
                {
                    MessageBox.Show(r.Result.Value["Return_Message"].ToString(), "error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show(r.Result.Value["Return_Message"].ToString(), "warming", MessageBoxButton.OK, MessageBoxImage.Question);

                    dataGrid.Value = null;
                }


                Component.MaskBusy.Hide(root, handle);
            }, Framework.App.Scheduler);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Component.Windows.AuthorityLogin login = new Component.Windows.AuthorityLogin(MenuId, '4');
            login.Owner = Component.App.Portal;
            if ((login.ShowDialog().Value) && (login.UserId == 1020))
            {
                Import();
            }
            else
            {
                MessageBox.Show("you are you administrator can't do this !!!","error",MessageBoxButton.OK);
            }
        }

        private void btnUpdateSave_Click(object sender, RoutedEventArgs e)
        {
            //Update();
        }

//        private void btnQuery_Click(object sender, RoutedEventArgs e)
//        {
//            string headerCode = string.Empty;// tbQueryHeaderCode.Text.Trim();
//            string macStart = tbMacStart.Text.Trim();
//            string macEnd = tbMacEnd.Text.Trim();

//            if (string.IsNullOrEmpty(headerCode) && string.IsNullOrEmpty(tbQueryMO.Text) && string.IsNullOrEmpty(macStart) && string.IsNullOrEmpty(macEnd))
//            {
//                Component.MessageBox.MyMessageBox.ShowWarning("请输入查询条件！");
//                return;
//            }

//            string sql = @"SELECT 
//			 Bas_MO_PalletSN.PalletSN,
//			 Bas_MO_PalletSN.IsUsed
//			 FROM Bas_MO_PalletSN				
//LEFT JOIN Bas_MO ON  Bas_MO.MOId=Bas_MO_PalletSN.MOId
//WHERE Bas_MO.MOCode=@MOCode";
//            if (!string.IsNullOrEmpty(macStart))
//            {
//                sql = sql + " AND Bas_MO_PalletSN.PalletSN>=@MacStart ";
//            }
//            if (!string.IsNullOrEmpty(macEnd))
//            {
//                sql = sql + " AND Bas_MO_PalletSN.PalletSN<=@MacEnd ";
//            }
//            Parameters parameters = new Parameters()
//               .Add("MOCode", tbQueryMO.Text, SqlDbType.NVarChar, 50)
//               .Add("MacStart", macStart, SqlDbType.NVarChar, 50)
//               .Add("MacEnd", macEnd, SqlDbType.NVarChar, 50);

//            if (MessageBox.Show(string.Format("确定查询数据数据？"), "提示", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.Cancel)
//                return;

//            int handle = Component.MaskBusy.Busy(root, "正在查询数据...");
//            System.Threading.Tasks.Task<Result<DataTable>>.Factory.StartNew(() =>
//            {
//                Result<DataTable> result = new Result<DataTable>() { HasError = false };
//                DataTable dt = null;
//                try
//                {
//                    dt = DB.DBHelper.GetDataTable(sql, parameters, ExecuteType.Text);
//                }
//                catch (Exception ex)
//                {
//                    result.HasError = true;
//                    result.Message = ex.Message;
//                }
//                result.Value = dt;
//                return result;

//            }).ContinueWith(r =>
//            {

//                if (r.Result.HasError)
//                {
//                    MessageBox.Show(r.Result.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
//                }
//                else
//                {
//                    dataGridQuery.ItemsSource = (r.Result.Value).DefaultView;
//                }


//                Component.MaskBusy.Hide(root, handle);
//            }, Framework.App.Scheduler);
//        }

        //private void btnExport_Click(object sender, RoutedEventArgs e)
        //{
        //    DataView view = dataGridQuery.ItemsSource as DataView;

        //    if (view == null || view.Count < 1)
        //    {
        //        Component.MessageBox.MyMessageBox.ShowError("没有获取到要导出的数据！");
        //        return;
        //    }
        //    System.Windows.Forms.SaveFileDialog fileDialog = new System.Windows.Forms.SaveFileDialog();
        //    fileDialog.Filter = "EXCEL|*.xls";
        //    fileDialog.RestoreDirectory = true;
        //    fileDialog.FileName = "Query";

        //    if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        //    {
        //        Component.MaskBusy.Busy(root, "正在导出数据");
        //        System.Threading.Tasks.Task.Factory.StartNew(() =>
        //        {
        //            DataTable exportSource = view.Table.Copy();
        //            exportSource.Constraints.Clear();
        //            for (int i = 0; i < exportSource.Columns.Count; i++)
        //            {
        //                DataColumn col = exportSource.Columns[i];
        //                if (col.ColumnName.ToUpper().Trim().EndsWith("ID"))
        //                {
        //                    exportSource.Columns.Remove(col);
        //                    i--;
        //                }
        //            }
        //            try
        //            {
        //                WinAPI.File.ExcelHelper.Write(fileDialog.FileName, exportSource);
        //            }
        //            catch (Exception ex)
        //            {
        //                Component.MessageBox.MyMessageBox.ShowError(ex.Message);
        //            }
        //        }).ContinueWith(bi =>
        //        {
        //            Component.MaskBusy.Hide(root);
        //        });
        //    }
        //}

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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string sql;
            Parameters parameters;
            if(tbMO.Text==string.Empty||tbOP.Text==string.Empty)
            {
                if (tbMO.Text == string.Empty && tbOP.Text == string.Empty && barcode.Text != string.Empty)
                {
                    sql = @"SELECT  Bas_MO.MOCode,
				        Bas_MO_Mac.Mac,
				        Inp_Lot.OPId,
				        Bas_OP.OPDesc
				        FROM Bas_MO_Mac
				        LEFT JOIN Bas_MO on Bas_MO.MOId=Bas_MO_Mac.MOId
				        LEFT JOIN Inp_Lot ON Inp_Lot.LotSN=Bas_MO_Mac.mac
				        LEFT JOIN Bas_OP ON  Bas_OP.OPId=Inp_Lot.OPId
				        WHERE Bas_MO_Mac.Mac='" + barcode.Text + "' OR Bas_MO_Mac.DSN='" + barcode.Text + "' OR Bas_MO_Mac.DeviceSerialNumber='" + barcode.Text + "' OR Bas_MO_Mac.STBNO='" + barcode.Text + "'";
                    parameters = new Parameters();
                }
                else
                {
                    MessageBox.Show("order or station can not empty !!!");
                    return;
                }
            }
            else
            {
                sql = @" SELECT BAS_MO.MOCode,
                Bas_MO_Mac.Mac,
				Inp_Lot.OPId,
				Bas_OP.OPDesc
                FROM BAS_MO
                LEFT JOIN Inp_Lot ON Inp_Lot.MOId = BAS_MO.MOId
                LEFT JOIN Bas_MO_Mac ON Bas_MO_Mac.LotId = Inp_Lot.LotId
                LEFT JOIN Bas_OP ON Bas_OP.OPId = Inp_Lot.OPId
                WHERE BAS_MO.MOCode = @MOCode and Bas_OP.OPCode = @OPCode";
                parameters = new Parameters()
                .Add("MOCode", tbMO.Text, SqlDbType.NVarChar, 50)
                .Add("OPCode", tbOP.Text, SqlDbType.NVarChar, 50);
            }

            int handle = Component.MaskBusy.Busy(root, "saving .....");
            System.Threading.Tasks.Task<Result<DataTable>>.Factory.StartNew(() =>
            {
                Result<DataTable> result = new Result<DataTable>() { HasError = false };
                DataTable dt = null;
                try
                {
                    dt = DB.DBHelper.GetDataTable(sql, parameters, ExecuteType.Text);
                }
                catch (Exception ex)
                {
                    result.HasError = true;
                    result.Message = ex.Message;
                }
                result.Value = dt;
                return result;

            }).ContinueWith(r =>
            {
                if (r.Result.HasError)
                {
                    MessageBox.Show(r.Result.Message, "error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    vi= (r.Result.Value).DefaultView;
                    dataGridQuery1.ItemsSource = vi;
                }
                Component.MaskBusy.Hide(root, handle);
            }, Framework.App.Scheduler);
        }

        private void Control_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Barcode_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key==Key.Enter)
            {
                Button_Click(null, null);
                barcode.SelectAll();
                barcode.Focus();
            }
        }
    }
}
