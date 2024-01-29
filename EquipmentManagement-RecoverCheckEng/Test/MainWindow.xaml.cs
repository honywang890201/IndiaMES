using Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using BoxPrint_M;
using PackPrint;
using TestMacImport;
using TestPalletSNImport;
using JumpToStation;
using P60wifiMacImport;
using macfrommysql;
using BoxSNImport;
using CartonQRPrint;
namespace Test
   
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();


            Framework.App.Resource.StationId = 68;
            Framework.App.Resource.StationCode = "OP-0001";
            Framework.App.Resource.StationDesc = "框体安装脚轮";//  SELECT * FROM dbo.Bas_OP

            Framework.App.Resource.ShiftTypeId = 7;//SELECT* FROM dbo.Bas_ShiftType

            Framework.App.Resource.LineId = 24;
            Framework.App.Resource.LineCode = "10";
            Framework.App.Resource.LineDesc = "智能自动化产线";//SELECT * FROM dbo.Bas_Line

            Framework.App.Resource.ResourceId = 76;
            Framework.App.User.DepartmentPositionType = "Supervisor";

            Framework.App.User.UserId = 1;
            //Framework.App.User.UserCode = 0;
            Framework.App.User.SupplierId = 55;
            Framework.App.NetCode = "NETOFFICE";
            DB.DBHelper.NetCode = Framework.App.NetCode;
            Framework.App.ServiceUrl = "http://192.168.2.15/MESService/Service.svc";// "http://172.16.0.9/LinkService/Service.svc";
            DB.DBHelper.IsDirectionDB = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            DB.DBHelper.IsDirectionDB = true;
            //DataTable dt = new DataTable();
            //dt.Columns.Add("id", typeof(System.String));
            //dt.Columns.Add("code", typeof(System.String));
            //dt.Rows.Add("M1", "1=M1");
            //dt.Rows.Add("M2", "2=M2");
            //dt.Rows.Add("M3", "3=M3");
            //colcom.ItemsSource = dt.DefaultView;
            //cmb.ItemsSource = dt.DefaultView;

            //txtquery.MetaId = 187;

            //string sql = "SELECT MOCode,Status,ISNULL(IsCanVirtualInput,0) AS IsCanVirtualInput,CreateDateTime FROM bas_MO WHERE CreateDateTime IS NOT NULL";
            //matrix.ItemsSource= DB.DBHelper.GetDataTable(sql, null, null, false).DefaultView;




            //Repair.RepairVendor vendor = new Repair.RepairVendor(new Framework.SystemAuthority(""));
            //DataChainQuery.DataChainVendor vendor = new DataChainQuery.DataChainVendor(new Framework.SystemAuthority(""));
            //ENImport.UserControl1 vendor = new ENImport.UserControl1(new Framework.SystemAuthority(""));
            // TestMacImport.UserControl1 vendor = new TestMacImport.UserControl1(new Framework.SystemAuthority(""));
            //Report.ReportVendor vendor = new Report.ReportVendor(4, new Framework.SystemAuthority(""));
            // BosaImport.Import vendor = new BosaImport.Import(new Framework.SystemAuthority(""));
            //StaticObject.StaticObjectVendor vendor = new StaticObject.StaticObjectVendor(21, new Framework.SystemAuthority(""));
            //// PackPrint.UserControl1 vendor = new PackPrint.UserControl1(new Framework.SystemAuthority(""));
            // BoxSplitPrint.UserControl1 vendor = new BoxSplitPrint.UserControl1(new Framework.SystemAuthority(""));
            // SMTLoadMaterial.UserControl1 vendor = new SMTLoadMaterial.UserControl1(new Framework.SystemAuthority(""));
            // TestPackQuery.UserControl1 vendor = new TestPackQuery.UserControl1(new Framework.SystemAuthority(""));
            //PackPrint.UserControl1 vendor = new PackPrint.UserControl1(new Framework.SystemAuthority(""));
            //vendor.TopMainGrid = g;
            //MaterialSN.UserControl1 vendor = new MaterialSN.UserControl1(new Framework.SystemAuthority(""));
            //Template.UserControl1 vendor = new Template.UserControl1(new Framework.SystemAuthority("0"));
            //SupplierPO.UserControl1 vendor = new SupplierPO.UserControl1(new Framework.SystemAuthority("0"));
            //SupplierPO.UserControl2 vendor = new SupplierPO.UserControl2(new Framework.SystemAuthority("0"));
            //MaterialLotBox.UserControl1 vendor = new MaterialLotBox.UserControl1(new Framework.SystemAuthority("0"));
            //SupplierPO.UserControl3 vendor = new SupplierPO.UserControl3(new Framework.SystemAuthority("0"));
            //WMS.Receive vendor = new WMS.Receive(new Framework.SystemAuthority("0"));

            //EquipmentManagement.PartWareHouseIns vendor = new EquipmentManagement.PartWareHouseIns(new Framework.SystemAuthority("0"));
            //this.Content = vendor;

            TestMacImport.UserControl1 my = new TestMacImport.UserControl1(new Framework.SystemAuthority("0"));
            this.Content = my;
           //BoxPrint_M.UserControl1  BoxPrintVendor = new BoxPrint_M.UserControl1(new Framework.SystemAuthority("0"));
           // this.Content = BoxPrintVendor;
        }

       

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //txt1.Text=Framework.App.Service.Login(txt.Text);
        }
    }
}
