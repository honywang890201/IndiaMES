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
    /// EquipmentRepairIn_EditDelegate.xaml 的交互逻辑
    /// </summary>
    public partial class EquipmentRepairIn_EditDelegate : Component.ControlsEx.Window
    {
        public long? RepairTypeId
        {
            get;
            private set;
        }
        public string RepairTypeName
        {
            get;
            private set;
        }

        public long? MainRepairUserId
        {
            get;
            private set;
        }
        public string MainRepairUserName
        {
            get;
            private set;
        }

        public string DelegateComment
        {
            get;
            private set;
        }

        public EquipmentRepairIn_EditDelegate(long? repairTypeId, long? mainRepairUserId,string delegateComment)
        {
            InitializeComponent();
            txtDelegateComment.Text = delegateComment;
            this.RepairTypeId = repairTypeId;
            this.MainRepairUserId = mainRepairUserId;
            LoadRepairType(repairTypeId);
        }

        private void LoadRepairType(long? repairTypeId)
        {
            string sql = @"SELECT RepairTypeId,
	   RepairTypeCode+'-'+ISNULL(RepairTypeName,'') AS RepairTypeName
FROM dbo.Bas_RepairType
ORDER BY RepairTypeCode";
            try
            {
                System.Data.DataTable dt = DB.DBHelper.GetDataTable(sql, null, null, true);
                cmbRepairType.ItemsSource = dt.DefaultView;
                cmbRepairType.SelectedValuePath = "RepairTypeId";
                cmbRepairType.DisplayMemberPath = "RepairTypeName";

                if(repairTypeId.HasValue)
                {
                    cmbRepairType.SelectedValue = repairTypeId.Value;
                }
                else
                {
                    if(dt.Rows.Count>0)
                    {
                        cmbRepairType.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception e)
            {
                Component.MessageBox.MyMessageBox.ShowError(e.Message);
            }
        }

        private void LoadMainRepairUserId()
        {
            if (cmbRepairType.SelectedItem == null)
                return;
            long typeId = (cmbRepairType.SelectedItem as DataRowView).Row.Field<long>("RepairTypeId");

            string sql = @"SELECT Set_User.SysUserId,
	   Set_User.SysUserCode+'-'+ISNULL(Set_User.SysUserName,'') AS SysUserName
FROM dbo.Set_User_RepairType WITH(NOLOCK) 
INNER JOIN dbo.Set_User WITH(NOLOCK) ON dbo.Set_User.SysUserId = dbo.Set_User_RepairType.SysUserId
WHERE Set_User_RepairType.RepairTypeId=@RepairTypeId";
            Data.Parameters parameters = new Data.Parameters().Add("RepairTypeId", typeId);
            try
            {
                System.Data.DataTable dt = DB.DBHelper.GetDataTable(sql, parameters, null, false);
                cmbMainRepairUser.ItemsSource = dt.DefaultView;
                cmbMainRepairUser.SelectedValuePath = "SysUserId";
                cmbMainRepairUser.DisplayMemberPath = "SysUserName";

                if (MainRepairUserId.HasValue)
                {
                    cmbMainRepairUser.SelectedValue = MainRepairUserId.Value;
                }
                else
                {
                    if (dt.Rows.Count > 0)
                    {
                        cmbMainRepairUser.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception e)
            {
                Component.MessageBox.MyMessageBox.ShowError(e.Message);
            }
        }

        private void cmbRepairType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadMainRepairUserId();
        }

        private void CircleImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (cmbRepairType.SelectedItem == null)
            {
                Component.MessageBox.MyMessageBox.ShowError("请选择维修类型。");
                cmbRepairType.Focus();
                return;
            }
            if (cmbMainRepairUser.SelectedItem == null)
            {
                Component.MessageBox.MyMessageBox.ShowError("请选择主维修人。");
                cmbMainRepairUser.Focus();
                return;
            }


            if (Component.MessageBox.MyMessageBox.ShowQuestion(string.Format("确定委派吗？")) != MessageBoxResult.OK)
                return;

            RepairTypeId = (cmbRepairType.SelectedItem as DataRowView).Row.Field<long>("RepairTypeId");
            RepairTypeName = (cmbRepairType.SelectedItem as DataRowView).Row.Field<string>("RepairTypeName");
            MainRepairUserId = (cmbMainRepairUser.SelectedItem as DataRowView).Row.Field<long>("SysUserId");
            MainRepairUserName = (cmbMainRepairUser.SelectedItem as DataRowView).Row.Field<string>("SysUserName");
            DelegateComment = txtDelegateComment.Text.Trim();
            this.DialogResult = true;
        }
    }
}
