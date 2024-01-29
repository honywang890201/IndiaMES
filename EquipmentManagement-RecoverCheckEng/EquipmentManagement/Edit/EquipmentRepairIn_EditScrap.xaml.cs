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
    /// EquipmentRepairIn_EditScrap.xaml 的交互逻辑
    /// </summary>
    public partial class EquipmentRepairIn_EditScrap : Component.ControlsEx.Window
    {
        public string ScrapComment
        {
            get;
            private set;
        }

        public EquipmentRepairIn_EditScrap(string scrapComment)
        {
            InitializeComponent();
            txtScrapComment.Text = scrapComment;
        }
        

        private void CircleImageButton_Click(object sender, RoutedEventArgs e)
        {

            if (Component.MessageBox.MyMessageBox.ShowQuestion(string.Format("确定报废吗？")) != MessageBoxResult.OK)
                return;

            ScrapComment = txtScrapComment.Text.Trim();
            this.DialogResult = true;
        }
    }
}
