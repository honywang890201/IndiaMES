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

namespace CartonQRPrint
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class UserControl1 : Component.Controls.User.UserVendor
    {
        private DataRowView SelectRow = null;
        private long tId = 0;
        private string QRPDFFilePath = string.Empty;
        private Dictionary<long, Printer.Instance> TemplatePath = new Dictionary<long, Printer.Instance>();
        private List<ComboBox> PrinterComboBoxs = new List<ComboBox>();
        private PrintToPDF PtP = new PrintToPDF();
        private int num = 0;
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
                //if (SelectRow["MOId"] == null)
                //{
                //    Component.MessageBox.MyMessageBox.ShowError("请输入工单！");
                //    return;
                //}
            }
            catch (Exception ex)
            {
                Component.MessageBox.MyMessageBox.ShowError(ex.Message);
                return;
            }
            long moId = 0;// (long)tbMO.Value;
            view.Sort= "BoxSN ASC";//升序排序，按顺序打印
            if (view == null || view.Count < 1)
            {
                Component.MessageBox.MyMessageBox.ShowError("没有获取到MAC数据！");
                return;
            }

           // List<Dictionary<string, object>> l = new List<Dictionary<string, object>>();
            //int rowNo = 0;
            long conut = view.Count;
            string box = "";
            long page = conut / 8;
           // DataColumn col = null;
            for (int i=0;i< page; i++)
            {
                num = i;
                box = "";
               for (int j=0;j<8;j++)
                { 
                     box += view.Table.Rows[i*8+j]["BoxSN"].ToString()+",";
                }
                SendRus(box);
            }
            //判断是否有不满整页的情况
            long pageleft = conut - page*8;
            if(pageleft>0)
            {
                box = "";
                for (int j = 0; j < pageleft; j++)
                {
                    box += view.Table.Rows[(int)page * 8 + j]["BoxSN"].ToString() + ",";
                }
                num += 1;
                SendRus(box);
            }
            


        }
        private  void SendRus(string sn)
        {
            Parameters parameters = new Parameters();
            parameters.Add("BoxSN", sn, null, int.MaxValue);
            parameters.Add("MOId", 10);
            parameters.Add("Return_Message", null, SqlDbType.NVarChar, int.MaxValue, ParameterDirection.Output);
            parameters.Add("Return_Value", null, SqlDbType.Int, ParameterDirection.ReturnValue);


            Result<Parameters, DataSet> result = null;
            try
            {
                result = DB.DBHelper.ExecuteParametersSource("Prd_Inp_Pack_Box_QRPrint", parameters, ExecuteType.StoredProcedure);
            }
            catch (Exception ex)
            {

                return;
            }

            if (result.HasError)
            {

                return;
            }

            if ((int)result.Value1["Return_Value"] != 1)
            {

                return;
            }
            else
            {

            }
            System.Threading.Thread p = new System.Threading.Thread(update);
            p.Start();
            Print(result.Value2);
            // p.Start();
            //System.Threading.Thread.Sleep(1000);

        }
        public void update()
        {
            PtP.PrintPDF(QRPDFFilePath, num);
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
            //return;
            string headerCode = string.Empty;// tbQueryHeaderCode.Text.Trim();
            string macStart = tbMacStart.Text.Trim();
            string macEnd = tbMacEnd.Text.Trim();

            if (string.IsNullOrEmpty(headerCode) && string.IsNullOrEmpty(tbQueryMO.Text) && string.IsNullOrEmpty(macStart) && string.IsNullOrEmpty(macEnd))
            {
                Component.MessageBox.MyMessageBox.ShowWarning("请输入查询条件！");
                return;
            }

            string sql = @" SELECT 
             Bas_P60WIFIMac.MOId,
			 Bas_P60WIFIMac.WIFIMac,
			 Bas_P60WIFIMac.IsUsed
			 FROM Bas_P60WIFIMac				
LEFT JOIN Bas_MO ON  Bas_MO.MOId=Bas_P60WIFIMac.MOId
WHERE Bas_MO.MOCode=@MOCode";
            //if (!string.IsNullOrEmpty(headerCode))
            //{
            //    sql = sql + " AND Bas_MO_Mac.HeaderCode=@HeaderCode ";
            //}
            //if (!string.IsNullOrEmpty(tbQueryMO.Text))
            //{
            //    sql = sql + " AND Bas_MO.MOCode=@MOCode ";
            //}
            if (!string.IsNullOrEmpty(macStart))
            {
                sql = sql + " AND Bas_MO_PalletSN.PalletSN>=@MacStart ";
            }
            if (!string.IsNullOrEmpty(macEnd))
            {
                sql = sql + " AND Bas_MO_PalletSN.PalletSN<=@MacEnd ";
            }

            Parameters parameters = new Parameters()
               // .Add("HeaderCode", headerCode, SqlDbType.NVarChar, 50)
               .Add("MOCode", tbQueryMO.Text, SqlDbType.NVarChar, 50)
               .Add("MacStart", macStart, SqlDbType.NVarChar, 50)
               .Add("MacEnd", macEnd, SqlDbType.NVarChar, 50);

            if (MessageBox.Show(string.Format("确定查询数据数据？"), "提示", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.Cancel)
                return;

            int handle = Component.MaskBusy.Busy(root, "正在查询数据...");
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

        }

        private void dataGridQuery_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
        }

        private void UserVendor_Loaded(object sender, RoutedEventArgs e)
        {

        }
        private void LoadMO()
        {
            Order.Text = "当前订单:"+SelectRow["MOCode"].ToString();
            string sql = @"SELECT Bas_MO_Template.MOTemplateId AS TemplateId,
	                       Bas_MO_Template.TemplateDesc,
	                       Bas_MO_Template.Copies,
	                       Set_File.FileId,
	                       Set_File.FileServerId,
	                       Bas_MO_Template.Sequence
                            FROM dbo.Bas_MO_Template   WITH(NOLOCK) 
                            LEFT JOIN dbo.Set_File  WITH(NOLOCK) ON Bas_MO_Template.TemplateId=Set_File.FileId
                            WHERE MOId=@MOId AND OPId=@OPId  ORDER BY Sequence";
            Parameters parameters = new Parameters();
            parameters.Add("MOId", SelectRow["MOId"]);
            parameters.Add("OPId", Framework.App.Resource.StationId);
            //Component.MaskBusy.Busy(root, "正在下载模板...");
            try
            {
                DataTable source = DB.DBHelper.GetDataTable(sql, parameters, ExecuteType.Text);
                WinAPI.SysFunction.KillProcessByName("lppa");
                foreach (DataRow row in source.Rows)
                {
                    byte[] buffer = null;
                    Component.Controls.User.FileHelper helper = null;
                    try
                    {
                        helper = new Component.Controls.User.FileHelper();
                        helper.Init((long)row["FileId"]);
                        if (helper.FileServerHelper != null)
                        {
                            buffer = helper.FileServerHelper.WriteFile(helper.ServerFileName);
                        }
                    }
                    catch (Exception ex)
                    {
                        //txtMessage.AddMessage(ex.Message, true);
                        continue;
                    }
                    if (buffer == null)
                        continue;
                    string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Template", helper.FileName);
                    try
                    {
                        WinAPI.File.FileHelper.Write(buffer, path);
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                    try
                    {
                        Printer.Instance cs = Printer.Instance.Factory(path, null, null, 0);//
                        tId = (long)row["TemplateId"];
                        TemplatePath.Add((long)row["TemplateId"], cs);
                    }
                    catch (Exception ex)
                    {
                        //txtMessage.AddMessage(ex.Message, true);
                    }
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            QRPDFFilePath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "/" + SelectRow["MOCode"].ToString();
            System.IO.Directory.CreateDirectory(QRPDFFilePath);
        }
        public void Print(DataSet set)
        {
            string pValue = "";
            int tableIndex = 0;
            if (set == null || set.Tables.Count < 1)
                return;
            if(TemplatePath.Count==0)
            {
                MessageBox.Show("没有上传模板，不能打印");
                return;
            }
            foreach (string pName in TemplatePath[tId].Params)
            {
                int index = pName.IndexOf('$');
                string parameterName = pName;
                int rowIndex = 1;
                if (index > 0 && index < pName.Length - 1)
                {
                    if (int.TryParse(pName.Substring(index + 1), out rowIndex))
                    {
                        parameterName = pName.Substring(0, index);
                    }
                }
                if (rowIndex > 0 && rowIndex <= set.Tables[tableIndex].Rows.Count)
                {
                    if (set.Tables[tableIndex].Columns.Contains(parameterName))
                    {
                        pValue = set.Tables[tableIndex].Rows[rowIndex - 1][parameterName].ToString();
                        TemplatePath[tId].SetParamValue(pName, set.Tables[tableIndex].Rows[rowIndex - 1][parameterName].ToString());
                    }
                    else
                    {
                        pValue = "";
                        TemplatePath[tId].SetParamValue(pName, "");
                    }
                }
                else
                {
                    TemplatePath[tId].SetParamValue(pName, "");
                }
            }
            TemplatePath[tId].SetPrinter("Foxit Reader PDF Printer");
            TemplatePath[tId].Print(1);
        }
    
         
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Component.Windows.MOSelector selector = new Component.Windows.MOSelector(MenuId);
            selector.Owner = Component.App.Portal;
            if (selector.ShowDialog().Value)
            {
                string sql = @"SELECT Bas_MO.MOId,
	   Bas_MO.ItemId,
	   Bas_MO.WorkflowId,
	   Bas_MO.CustomerId,
	   Bas_MO.MOCode,
	   Bas_MO.Qty,
	   Bas_Item.ItemCode,
	   Bas_Item.ItemSpecification,
	   Bas_Workflow.WorkflowCode,
	   Bas_Workflow.WorkflowDesc,
	   Bas_Customer.CustomerCode,
	   Bas_Customer.CustomerDesc,
	   Bas_Item.ENType
FROM dbo.Bas_MO  WITH(NOLOCK) 
LEFT JOIN dbo.Bas_Workflow  WITH(NOLOCK) ON dbo.Bas_Workflow.WorkflowId = dbo.Bas_MO.WorkflowId
LEFT JOIN dbo.Bas_Item  WITH(NOLOCK) ON dbo.Bas_Item.ItemId = dbo.Bas_MO.ItemId
LEFT JOIN dbo.Bas_Customer  WITH(NOLOCK) ON dbo.Bas_Customer.CustomerId = dbo.Bas_MO.CustomerId
WHERE Bas_MO.MOId=@MOId";

                Parameters parameters = new Parameters();
                parameters.Add("MOId", selector.moId);

                try
                {
                    DataTable table = DB.DBHelper.GetDataTable(sql, parameters, ExecuteType.Text, null, true);
                    if (table.Rows.Count < 1)
                    {
                        MessageBox.Show("工单错误", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    SelectRow = table.DefaultView[0];
                   
                    LoadMO();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
        }
    }
}
