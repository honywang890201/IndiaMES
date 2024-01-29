using Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BoxPrint_M
{
    /// <summary>
    /// AddPrint.xaml 的交互逻辑
    /// </summary>
    public partial class AddPrint : Window
    {
        private UserControl1 uc = null;
        private long moId = 0;
        private long userId = 0;
        private DataRowView SelectRow = null;
        public AddPrint(string scanType, UserControl1 uc, long moId, long userId)
        {
            InitializeComponent();
            this.uc = uc;
            this.moId = moId;
            this.userId = userId;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtBoxSN.SetFoucs();
        }

        private void txtBoxSN_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtBoxSN.Text.Trim() == string.Empty)
                {
                    uc.AddMessage("请扫描箱号！", true);
                    txtBoxSN.Text = string.Empty;
                    txtBoxSN.SetFoucs();
                    return;
                }

                Parameters parameters = new Parameters();
                parameters.Add("BoxSN", txtBoxSN.Text.Trim());
                parameters.Add("MOId", moId);
                parameters.Add("IsSplitPrint", false);
                parameters.Add("LineId", Framework.App.Resource.LineId);
                parameters.Add("ResId", Framework.App.Resource.ResourceId);
                parameters.Add("ShiftTypeId", Framework.App.Resource.ShiftTypeId);
                parameters.Add("UserId", userId);
                parameters.Add("OPId", Framework.App.Resource.StationId);
                parameters.Add("PluginId", uc.PluginId);
                parameters.Add("Return_Message", null, SqlDbType.NVarChar, int.MaxValue, ParameterDirection.Output);
                parameters.Add("Return_Value", null, SqlDbType.Int, ParameterDirection.ReturnValue);


                Result<Parameters, DataSet> result = null;
                try
                {
                    result = DB.DBHelper.ExecuteParametersSource("Prd_Inp_Pack_Box_AddPrint", parameters, ExecuteType.StoredProcedure);
                }
                catch (Exception ex)
                {
                    uc.AddMessage(ex.Message, true);
                    txtBoxSN.Text = string.Empty;
                    txtBoxSN.SetFoucs();
                    return;
                }

                if (result.HasError)
                {
                    uc.AddMessage(result.Message, true);
                    txtBoxSN.Text = string.Empty;
                    txtBoxSN.SetFoucs();
                    return;
                }

                if ((int)result.Value1["Return_Value"] != 1)
                {
                    uc.AddMessage(result.Value1["Return_Message"].ToString(), true);
                    txtBoxSN.Text = string.Empty;
                    txtBoxSN.SetFoucs();
                    return;
                }
                else
                {
                    uc.AddMessage(result.Value1["Return_Message"].ToString(), false);
                    txtBoxSN.Text = string.Empty;
                    txtBoxSN.SetFoucs();
                }
                uc.Print(result.Value2);
            }
        }

        private void txtBoxSN_Loaded(object sender, RoutedEventArgs e)
        {

        }
        private void  myprint()
        {
            if (txtBoxSN.Text.Trim() == string.Empty)
            {
                uc.AddMessage("请扫描箱号！", true);
                txtBoxSN.Text = string.Empty;
                txtBoxSN.SetFoucs();
                return;
            }

            Parameters parameters = new Parameters();
            parameters.Add("BoxSN", txtBoxSN.Text.Trim());
            parameters.Add("MOId", moId);
            parameters.Add("IsSplitPrint", false);
            parameters.Add("LineId", Framework.App.Resource.LineId);
            parameters.Add("ResId", Framework.App.Resource.ResourceId);
            parameters.Add("ShiftTypeId", Framework.App.Resource.ShiftTypeId);
            parameters.Add("UserId", userId);
            parameters.Add("OPId", Framework.App.Resource.StationId);
            parameters.Add("PluginId", uc.PluginId);
            parameters.Add("Return_Message", null, SqlDbType.NVarChar, int.MaxValue, ParameterDirection.Output);
            parameters.Add("Return_Value", null, SqlDbType.Int, ParameterDirection.ReturnValue);


            Result<Parameters, DataSet> result = null;
            try
            {
                result = DB.DBHelper.ExecuteParametersSource("Prd_Inp_Pack_Box_AddPrint", parameters, ExecuteType.StoredProcedure);
            }
            catch (Exception ex)
            {
                uc.AddMessage(ex.Message, true);
                txtBoxSN.Text = string.Empty;
                txtBoxSN.SetFoucs();
                return;
            }

            if (result.HasError)
            {
                uc.AddMessage(result.Message, true);
                txtBoxSN.Text = string.Empty;
                txtBoxSN.SetFoucs();
                return;
            }

            if ((int)result.Value1["Return_Value"] != 1)
            {
                uc.AddMessage(result.Value1["Return_Message"].ToString(), true);
                txtBoxSN.Text = string.Empty;
                txtBoxSN.SetFoucs();
                return;
            }
            else
            {
                uc.AddMessage(result.Value1["Return_Message"].ToString(), false);
                txtBoxSN.Text = string.Empty;
                txtBoxSN.SetFoucs();
            }
            uc.Print(result.Value2);
        }
        //整个订单全部补打外箱条码
        private void ok_print(object sender, RoutedEventArgs e)
        {
            //string sql = @"SELECT BoxSN FROM Inp_Box WHERE MOId=@MOId and boxsn >'00000' ORDER BY BoxSN";
            //Parameters parameters = new Parameters();
            //parameters.Add("MOId", moId);

            //try
            //{
            //    DataTable source = DB.DBHelper.GetDataTable(sql, parameters, ExecuteType.Text);

            //    if (source.Rows.Count < 1)
            //    {
            //        MessageBox.Show("没有找到箱号");
            //        return;
            //    }
            //    foreach (DataRow dr in source.Rows)
            //    {

            //        txtBoxSN.Text = dr["BoxSN"].ToString();
            //        myprint();
            //        System.Threading.Thread.Sleep(5000);
            //        //MessageBox.Show(dr["BoxSN"].ToString());
            //    }   

            //}
            //catch
            //{

            //}

            string sql = @"SELECT DISTINCT(CartoonSN) FROM Sheet1  ORDER BY CartoonSN";
            Parameters parameters = new Parameters();
            Parameters parameters1 = new Parameters();
            DataTable source1;
            string sql1 = "";
            //parameters.Add("MOId", moId);

            try
            {
                DataTable source = DB.DBHelper.GetDataTable(sql, parameters, ExecuteType.Text);

                if (source.Rows.Count < 1)
                {
                    MessageBox.Show("没有找到箱号");
                    return;
                }
                foreach (DataRow dr in source.Rows)
                {
                   

                    pp(dr["CartoonSN"].ToString());

                }

                
            }
            catch
            {

            }
        }
        private void pp(string boxsn)
        {
            Parameters parameters = new Parameters();
            parameters.Add("BoxSN", boxsn);
            parameters.Add("Return_Message", null, SqlDbType.NVarChar, int.MaxValue, ParameterDirection.Output);
            parameters.Add("Return_Value", null, SqlDbType.Int, ParameterDirection.ReturnValue);


            Result<Parameters, DataSet> result = null;
            try
            {
                result = DB.DBHelper.ExecuteParametersSource("add", parameters, ExecuteType.StoredProcedure);
            }
            catch (Exception ex)
            {
                uc.AddMessage(ex.Message, true);
                txtBoxSN.Text = string.Empty;
                txtBoxSN.SetFoucs();
                return;
            }

            if (result.HasError)
            {
                uc.AddMessage(result.Message, true);
                txtBoxSN.Text = string.Empty;
                txtBoxSN.SetFoucs();
                return;
            }

            //if ((int)result.Value1["Return_Value"] != 1)
            //{
            //    uc.AddMessage(result.Value1["Return_Message"].ToString(), true);
            //    txtBoxSN.Text = string.Empty;
            //    txtBoxSN.SetFoucs();
            //    return;
            //}
            //else
            //{
            //    uc.AddMessage(result.Value1["Return_Message"].ToString(), false);
            //    txtBoxSN.Text = string.Empty;
            //    txtBoxSN.SetFoucs();
            //}
            uc.Print(result.Value2);
        }
    }
   
}

