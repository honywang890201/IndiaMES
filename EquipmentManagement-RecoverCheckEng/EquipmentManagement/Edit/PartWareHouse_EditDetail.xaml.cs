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

namespace EquipmentManagement.Edit
{
    /// <summary>
    /// PartWareHouse_EditDetail.xaml 的交互逻辑
    /// </summary>
    public partial class PartWareHouse_EditDetail : Component.ControlsEx.Window
    {
        public long ItemId
        {
            get;
            private set;
        }
        public string ItemCode
        {
            get;
            private set;
        }

        public string Unit
        {
            get;
            private set;
        }

        public string ItemSpecification
        {
            get;
            private set;
        }
        public decimal Qty
        {
            get;
            private set;
        }

        public string PartWareHouseDetailDesc
        {
            get;
            private set;
        }

        public PartWareHouse_EditDetail(long? itemId, decimal? qty,string partWareHouseDetailDesc)
        {
            InitializeComponent();
            try
            {
                if (itemId.HasValue)
                {
                    txtItem.Value = itemId.Value;
                }
            }
            catch
            {

            }

            if(qty.HasValue)
            {
                txtQty.Value = qty;
            }

            txtPartWareHouseDetailDesc.Text = partWareHouseDetailDesc;
        }
        

        private void CircleImageButton_Click(object sender, RoutedEventArgs e)
        {
            if(txtItem.Text==string.Empty)
            {
                Component.MessageBox.MyMessageBox.ShowError("请输入物料代码。");
                txtItem.Focus();
                return;
            }

            try
            {
                ItemId = (long)txtItem.Value;
                ItemCode = txtItem.SelectRow["ItemCode"].ToString();
                ItemSpecification = txtItem.SelectRow["ItemSpecification"].ToString();
                Unit = txtItem.SelectRow["Unit"].ToString();
            }
            catch(Exception ex)
            {
                Component.MessageBox.MyMessageBox.ShowError(string.Format("物料代码{0}", ex.Message));
                txtItem.Focus();
                return;
            }

            if(!txtQty.Value.HasValue)
            {
                Component.MessageBox.MyMessageBox.ShowError("请输入数量。");
                txtQty.Focus();
                return;
            }

            Qty = txtQty.Value.Value;
            if(Qty<=(decimal)0.0)
            {
                Component.MessageBox.MyMessageBox.ShowError("请输入正确的数量。");
                txtQty.Focus();
                return;
            }
            PartWareHouseDetailDesc = txtPartWareHouseDetailDesc.Text.Trim();
            this.DialogResult = true;
        }

        private void txtItem_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (txtItem.SelectRow != null)
                {
                    txtItemSpecification.Text = txtItem.SelectRow["ItemSpecification"].ToString();
                    txtUnit.Text = txtItem.SelectRow["Unit"].ToString();
                }
                else
                {
                    txtItemSpecification.Text = string.Empty;
                    txtUnit.Text = string.Empty;
                }
            }
            catch { }
        }
    }
}
