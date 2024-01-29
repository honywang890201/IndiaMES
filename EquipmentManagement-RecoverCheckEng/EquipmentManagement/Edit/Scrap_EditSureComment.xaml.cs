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
    /// Scrap_EditSureComment.xaml 的交互逻辑
    /// </summary>
    public partial class Scrap_EditSureComment : Component.ControlsEx.Window
    {
        public string SureComment
        {
            get;
            private set;
        }

        private bool IsReject = false;

        public Scrap_EditSureComment(bool isReject)
        {
            InitializeComponent();
            this.IsReject = isReject;
            if (isReject)
            {
                this.Title = "驳回备注";
                tbTitle.Text = "驳回备注";
                tbComment.Text = "驳回备注";
                btnSure.Content = "驳回";
                btnSure.ImageSource = WinAPI.File.ImageHelper.ConvertToImageSource(Properties.Resources.delete);
            }
            else
            {
                this.Title = "同意备注";
                tbTitle.Text = "同意备注";
                tbComment.Text = "同意备注";
                btnSure.Content = "同意";
                btnSure.ImageSource = WinAPI.File.ImageHelper.ConvertToImageSource(Properties.Resources.yes);
            }

        }
        private void CircleImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsReject)
            {
                if (Component.MessageBox.MyMessageBox.ShowQuestion(string.Format("确定吗驳回？")) != MessageBoxResult.OK)
                    return;
            }
            else
            {
                if (Component.MessageBox.MyMessageBox.ShowQuestion(string.Format("确定同意吗？")) != MessageBoxResult.OK)
                    return;
            }

            SureComment = txtSureComment.Text.Trim();
            this.DialogResult = true;
        }
    }
}
