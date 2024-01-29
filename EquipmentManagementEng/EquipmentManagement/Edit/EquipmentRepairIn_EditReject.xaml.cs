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
    /// EquipmentRepairIn_EditReject.xaml 的交互逻辑
    /// </summary>
    public partial class EquipmentRepairIn_EditReject : Component.ControlsEx.Window
    {
        public string RejectComment
        {
            get;
            private set;
        }

        public EquipmentRepairIn_EditReject(string rejectComment)
        {
            InitializeComponent();
            txtRejectComment.Text = rejectComment;
        }
        

        private void CircleImageButton_Click(object sender, RoutedEventArgs e)
        {

            if (Component.MessageBox.MyMessageBox.ShowQuestion(string.Format("确定驳回吗？")) != MessageBoxResult.OK)
                return;

            RejectComment = txtRejectComment.Text.Trim();
            this.DialogResult = true;
        }
    }
}
