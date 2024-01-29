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

namespace PalletByEN
{
    /// <summary>
    /// AddPrint.xaml 的交互逻辑
    /// </summary>
    public partial class AddPrint : Window
    {
        private UserControl1 uc = null;
        private long moId = 0;
        private long userId = 0;

        //整个订单全部补打外箱条码
        private void ok_print(object sender, RoutedEventArgs e)
        {
           
        }
        public AddPrint(string scanType, UserControl1 uc, long moId, long userId)
        {
            InitializeComponent();
            this.uc = uc;
            this.moId = moId;
            this.userId = userId;

            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
           
            txtScan.SetFoucs();
        }

       
        private void myprint()
        {
            if (txtScan.Text.Trim() == string.Empty)
            {
                uc.AddMessage(string.Format("请扫描{0}！", lblScan.Text), true);
                txtScan.Text = string.Empty;
                txtScan.SetFoucs();
                return;
            }
            //if (txtDescript.Text.Trim() == string.Empty)
            //{
            //    uc.AddMessage(string.Format("请输入{0}！", lblDescript.Text), true);
            //    txtDescript.Text = string.Empty;
            //    txtDescript.SetFoucs();
            //    return;
            //}
            //MessageBox.Show(uc.IsPrintBoxInfo.ToString());
            Parameters parameters = new Parameters();
            parameters.Add("PalletSN", txtScan.Text.Trim());
            //根据主程序传递的IsPrintSN值来生成不同的参数给存储过程，用于判断是否要打印整个栈板的机身信息
            if (uc.IsPrintBoxInfo)
            {
                parameters.Add("IsPrintSN", 1);
            }
            else
            {
                parameters.Add("IsPrintSN", 0);
            }
            parameters.Add("MOId", moId);
            parameters.Add("IsSplitPrint", false);
            parameters.Add("LineId", Framework.App.Resource.LineId);
            parameters.Add("ResId", Framework.App.Resource.ResourceId);
            parameters.Add("ShiftTypeId", Framework.App.Resource.ShiftTypeId);
            parameters.Add("UserId", userId);
            parameters.Add("OPId", Framework.App.Resource.StationId);
            parameters.Add("PluginId", uc.PluginId);
            //parameters.Add("Descript", txtDescript.Text.Trim());
            parameters.Add("Return_Message", null, SqlDbType.NVarChar, int.MaxValue, ParameterDirection.Output);
            parameters.Add("Return_Value", null, SqlDbType.Int, ParameterDirection.ReturnValue);


            Result<Parameters, DataSet> result = null;
            try
            {
                result = DB.DBHelper.ExecuteParametersSource("[Prd_Inp_Pack_Pallet_AddPrint_Online]", parameters, ExecuteType.StoredProcedure);
            }
            catch (Exception ex)
            {
                uc.AddMessage(ex.Message, true);
                txtScan.Text = string.Empty;
                txtScan.SetFoucs();
                return;
            }

            if (result.HasError)
            {
                uc.AddMessage(result.Message, true);
                txtScan.Text = string.Empty;
                txtScan.SetFoucs();
                return;
            }

            if ((int)result.Value1["Return_Value"] != 1)
            {
                uc.AddMessage(result.Value1["Return_Message"].ToString(), true);
                txtScan.Text = string.Empty;
                txtScan.SetFoucs();
                return;
            }
            else
            {
                uc.AddMessage(result.Value1["Return_Message"].ToString(), false);
                txtScan.Text = string.Empty;
                txtScan.SetFoucs();
            }
            uc.Print(result.Value2);
        }
        private void txtScan_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
               
                if (txtScan.Text.Trim() == string.Empty)
                {
                    uc.AddMessage(string.Format("请扫描{0}！", lblScan.Text), true);
                    txtScan.Text = string.Empty;
                    txtScan.SetFoucs();
                    return;
                }
                //if (txtDescript.Text.Trim() == string.Empty)
                //{
                //    uc.AddMessage(string.Format("请输入{0}！", lblDescript.Text), true);
                //    txtDescript.Text = string.Empty;
                //    txtDescript.SetFoucs();
                //    return;
                //}
                //MessageBox.Show(uc.IsPrintBoxInfo.ToString());
                Parameters parameters = new Parameters();
                parameters.Add("PalletSN", txtScan.Text.Trim());
                //根据主程序传递的IsPrintSN值来生成不同的参数给存储过程，用于判断是否要打印整个栈板的机身信息
                if (uc.IsPrintBoxInfo)
                {
                    parameters.Add("IsPrintSN", 1);
                }
                else
                {
                    parameters.Add("IsPrintSN", 0);
                }
                parameters.Add("MOId", moId);
                parameters.Add("IsSplitPrint", false);
                parameters.Add("LineId", Framework.App.Resource.LineId);
                parameters.Add("ResId", Framework.App.Resource.ResourceId);
                parameters.Add("ShiftTypeId", Framework.App.Resource.ShiftTypeId);
                parameters.Add("UserId", userId);
                parameters.Add("OPId", Framework.App.Resource.StationId);
                parameters.Add("PluginId", uc.PluginId);
                //parameters.Add("Descript", txtDescript.Text.Trim());
                parameters.Add("Return_Message", null, SqlDbType.NVarChar, int.MaxValue, ParameterDirection.Output);
                parameters.Add("Return_Value", null, SqlDbType.Int, ParameterDirection.ReturnValue);


                Result<Parameters, DataSet> result = null;
                try
                {
                    result = DB.DBHelper.ExecuteParametersSource("[Prd_Inp_Pack_Pallet_AddPrint_Online]", parameters, ExecuteType.StoredProcedure);
                }
                catch (Exception ex)
                {
                    uc.AddMessage(ex.Message, true);
                    txtScan.Text = string.Empty;
                    txtScan.SetFoucs();
                    return;
                }

                if (result.HasError)
                {
                    uc.AddMessage(result.Message, true);
                    txtScan.Text = string.Empty;
                    txtScan.SetFoucs();
                    return;
                }

                if ((int)result.Value1["Return_Value"] != 1)
                {
                    uc.AddMessage(result.Value1["Return_Message"].ToString(), true);
                    txtScan.Text = string.Empty;
                    txtScan.SetFoucs();
                    return;
                }
                else
                {
                    uc.AddMessage(result.Value1["Return_Message"].ToString(), false);
                    txtScan.Text = string.Empty;
                    txtScan.SetFoucs();
                }
                uc.Print(result.Value2);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string sql = @"SELECT PalletSN FROM Bas_MO_PalletSN WHERE MOId=@moid order by PalletSN";
            Parameters parameters = new Parameters();
            parameters.Add("MOId", moId);

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

                    txtScan.Text = dr["PalletSN"].ToString();
                    myprint();
                    System.Threading.Thread.Sleep(5000);
                    //MessageBox.Show(dr["BoxSN"].ToString());
                }

            }
            catch
            {

            }
        }
    }

}
