using Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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
using WinAPI;
using System.Diagnostics;

namespace BoxPrint_H
{
    /// <summary>
    /// Page1.xaml 的交互逻辑
    /// </summary>
    public partial class Barcode : Component.Controls.User.UserVendor
    {
        public Barcode(Framework.SystemAuthority authority) :
            base(authority)
        {
            InitializeComponent();
            root.Background = new ImageBrush(WinAPI.File.ImageHelper.ConvertToImageSource(Component.App.BackgroudImage));

        }

    }
}
