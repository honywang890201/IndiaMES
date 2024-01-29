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
using System.Windows.Shapes;

namespace BoxPrint_M
{
    /// <summary>
    /// FillBox.xaml 的交互逻辑
    /// </summary>
    public partial class FillBox : Window
    {
        private UserControl1 uc = null;
        private long moId = 0;
        private int boxQty = 0;

        public DataTable Source
        {
            get;
            private set;
        }
        public string BoxSN
        {
            get;
            private set;
        }
        public long BoxId
        {
            get;
            private set;
        }

        public FillBox(UserControl1 uc, long moId, int boxQty)
        {
            InitializeComponent();
            this.uc = uc;
            this.moId = moId;
            this.boxQty = boxQty;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtBoxSN.SetFoucs();
        }

        private void txtBoxSN_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter&&txtBoxSN.Text.Trim()!=string.Empty)
            {
                string sql = @"SELECT Inp_Box.BoxId,
	   Inp_Box.BoxSN,
	   Inp_Box.MOId,
	   Bas_MO.MOCode,
	   Bas_Item.ItemCode,
	   Bas_Item.BoxQty
FROM dbo.Inp_Box  WITH(NOLOCK) 
LEFT JOIN dbo.Bas_MO  WITH(NOLOCK) ON dbo.Bas_MO.MOId = dbo.Inp_Box.MOId
LEFT JOIN dbo.Bas_Item  WITH(NOLOCK) ON Inp_Box.ItemId=Bas_Item.ItemId
WHERE Inp_Box.BoxSN=@BoxSN";

                Parameters parameters = new Parameters().Add("BoxSN", txtBoxSN.Text.Trim());
                BoxSN = txtBoxSN.Text.Trim();
                int BoxQty = 0;
                try
                {
                    DataTable table = DB.DBHelper.GetDataTable(sql, parameters, ExecuteType.Text);
                    if(table.Rows.Count<1)
                    {
                        uc.AddMessage("箱号[" + txtBoxSN.Text.Trim() + "]错误！", true);
                        txtBoxSN.Text = string.Empty;
                        txtBoxSN.SetFoucs();
                        return;
                    }
                    else if ((long)table.Rows[0]["MOId"] != moId)
                    {
                        uc.AddMessage(string.Format("箱号[" + txtBoxSN.Text.Trim() + "]对应的工单为[{0}],请选择正确的工单操作！", table.Rows[0]["MOCode"]), true);
                        txtBoxSN.Text = string.Empty;
                        txtBoxSN.SetFoucs();
                        return;
                    }

                    if (!int.TryParse(table.Rows[0]["BoxQty"].ToString(), out BoxQty))
                    {
                        uc.AddMessage("料号[" + table.Rows[0]["ItemCode"].ToString() + "]未维护装箱数量，请先维护装箱数量！", true);
                        txtBoxSN.Text = string.Empty;
                        txtBoxSN.SetFoucs();
                        return;
                    }

                    if (BoxQty<1)
                    {
                        uc.AddMessage("料号[" + table.Rows[0]["ItemCode"].ToString() + "]未维护装箱数量，请先维护装箱数量！", true);
                        txtBoxSN.Text = string.Empty;
                        txtBoxSN.SetFoucs();
                        return;
                    }


                    BoxId = (long)table.Rows[0]["BoxId"];
                    BoxSN = table.Rows[0]["BoxSN"].ToString();
                }
                catch(Exception ex)
                {
                    uc.AddMessage(ex.Message, true);
                    txtBoxSN.Text = string.Empty;
                    txtBoxSN.SetFoucs();
                    return;
                }

                sql = @"SELECT Inp_Lot.LotId,
       Inp_Lot.LotSN,
	   Inp_Lot_Mac.Mac,
	   Inp_Lot_EN.EN
FROM dbo.Inp_Lot  WITH(NOLOCK) 
LEFT JOIN dbo.Inp_Lot_EN  WITH(NOLOCK) ON dbo.Inp_Lot_EN.LotId = dbo.Inp_Lot.LotId
LEFT JOIN dbo.Inp_Lot_Mac  WITH(NOLOCK) ON dbo.Inp_Lot_Mac.LotId = dbo.Inp_Lot.LotId
WHERE Inp_Lot.BoxId=@BoxId";

                parameters = new Parameters().Add("BoxId", BoxId);

                try
                {
                    Source = DB.DBHelper.GetDataTable(sql, parameters, ExecuteType.Text);

                    if(Source.Rows.Count>=boxQty)
                    {
                        uc.AddMessage(string.Format("箱号[" + BoxSN + "]已装满，不能补箱！"),true);
                        txtBoxSN.Text = string.Empty;
                        txtBoxSN.SetFoucs();
                        return;
                    }

                    uc.AddMessage(string.Format("箱号[" +BoxSN+ "]正确，开始补箱！"), false);
                    this.DialogResult = true;
                }
                catch (Exception ex)
                {
                    uc.AddMessage(ex.Message, true);
                    txtBoxSN.Text = string.Empty;
                    txtBoxSN.SetFoucs();
                    return;
                }
            }
        }
    }
}
