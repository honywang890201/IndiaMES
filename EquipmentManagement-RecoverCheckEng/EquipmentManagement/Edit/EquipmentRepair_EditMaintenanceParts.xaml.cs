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
    /// EquipmentRepair_EditMaintenanceParts.xaml 的交互逻辑
    /// </summary>
    public partial class EquipmentRepair_EditMaintenanceParts : Component.ControlsEx.Window
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

        public EquipmentRepair_EditMaintenanceParts(long? itemId, string itemCode, decimal? qty)
        {
            InitializeComponent();
            try
            {
                if (itemId.HasValue)
                {
                    txtItem.Value = itemId.Value;
                }
                if (qty.HasValue)
                {
                    txtQty.Value = qty.Value;
                }
            }
            catch
            {

            }
        }
        

        private void CircleImageButton_Click(object sender, RoutedEventArgs e)
        {
            if(txtItem.Text==string.Empty)
            {
                Component.MessageBox.MyMessageBox.ShowError("请输入部件代码。");
                txtItem.Focus();
                return;
            }

            try
            {
                ItemId = (long)txtItem.Value;
                ItemCode = txtItem.SelectRow["ItemCode"].ToString();
                ItemSpecification = txtItem.SelectRow["ItemSpecification"].ToString();
            }
            catch(Exception ex)
            {
                Component.MessageBox.MyMessageBox.ShowError(string.Format("部件代码{0}", ex.Message));
                txtItem.Focus();
                return;
            }

            if(!txtQty.Value.HasValue)
            {
                Component.MessageBox.MyMessageBox.ShowError("请输入使用数量。");
                txtQty.Focus();
                return;
            }

            Qty = txtQty.Value.Value;
            if(Qty<=(decimal)0.0)
            {
                Component.MessageBox.MyMessageBox.ShowError("请输入正确的使用数量。");
                txtQty.Focus();
                return;
            }
            this.DialogResult = true;
        }

        private void txtItem_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (txtItem.SelectRow != null)
                {
                    txtItemSpecification.Text = txtItem.SelectRow["ItemSpecification"].ToString();
                }
                else
                {
                    txtItemSpecification.Text = string.Empty;
                }
            }
            catch { }
        }
    }
}
