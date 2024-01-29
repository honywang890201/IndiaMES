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
    /// EquipmentRepair_EditRepairLocation.xaml 的交互逻辑
    /// </summary>
    public partial class EquipmentRepair_EditRepairLocation : Component.ControlsEx.Window
    {
        public long? RepairAreaId
        {
            get;
            private set;
        }
        public string RepairAreaName
        {
            get;
            private set;
        }
        public long? RepairAreaLocationId
        {
            get;
            private set;
        }
        public string RepairAreaLocationName
        {
            get;
            private set;
        }
        public string Summarize
        {
            get;
            private set;
        }

        public EquipmentRepair_EditRepairLocation(long? repairAreaId, long? repairAreaLocationId, string summarize)
        {
            InitializeComponent();
            txtSummarize.Text = summarize;
            this.RepairAreaId = repairAreaId;
            this.RepairAreaLocationId = repairAreaLocationId;
            LoadRepairArea(repairAreaId);
        }

        private void LoadRepairArea(long? repairAreaId)
        {
            string sql = @"SELECT RepairAreaId,
	   RepairAreaCode+'-'+ISNULL(RepairAreaName,'') AS RepairAreaName
FROM dbo.Bas_RepairArea
ORDER BY Sequence";
            try
            {
                System.Data.DataTable dt = DB.DBHelper.GetDataTable(sql, null, null, true);
                cmbRepairArea.ItemsSource = dt.DefaultView;
                cmbRepairArea.SelectedValuePath = "RepairAreaId";
                cmbRepairArea.DisplayMemberPath = "RepairAreaName";

                if(repairAreaId.HasValue)
                {
                    cmbRepairArea.SelectedValue = repairAreaId.Value;
                }
                else
                {
                    if(dt.Rows.Count>0)
                    {
                        cmbRepairArea.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception e)
            {
                Component.MessageBox.MyMessageBox.ShowError(e.Message);
            }
        }

        private void LoadRepairAreaLocation()
        {
            if (cmbRepairArea.SelectedItem == null)
                return;
            long repairAreaId = (cmbRepairArea.SelectedItem as DataRowView).Row.Field<long>("RepairAreaId");

            string sql = @"SELECT RepairAreaLocationId,
	   RepairAreaLocationCode+'-'+ISNULL(RepairAreaLocationName,'') AS RepairAreaLocationName
FROM dbo.Bas_RepairArea_Location WITH(NOLOCK) 
WHERE RepairAreaId=@RepairAreaId
ORDER BY Sequence";
            Data.Parameters parameters = new Data.Parameters().Add("RepairAreaId", repairAreaId);
            try
            {
                System.Data.DataTable dt = DB.DBHelper.GetDataTable(sql, parameters, null, false);
                cmbMainRepairAreaLocation.ItemsSource = dt.DefaultView;
                cmbMainRepairAreaLocation.SelectedValuePath = "RepairAreaLocationId";
                cmbMainRepairAreaLocation.DisplayMemberPath = "RepairAreaLocationName";

                if (RepairAreaLocationId.HasValue)
                {
                    cmbMainRepairAreaLocation.SelectedValue = RepairAreaLocationId.Value;
                }
                else
                {
                    if (dt.Rows.Count > 0)
                    {
                        cmbMainRepairAreaLocation.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception e)
            {
                Component.MessageBox.MyMessageBox.ShowError(e.Message);
            }
        }

        private void cmbRepairArea_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadRepairAreaLocation();
        }

        private void CircleImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (cmbRepairArea.SelectedItem == null)
            {
                Component.MessageBox.MyMessageBox.ShowError("请选择维修区域。");
                cmbRepairArea.Focus();
                return;
            }
            if (cmbMainRepairAreaLocation.SelectedItem == null)
            {
                Component.MessageBox.MyMessageBox.ShowError("请选择维修位置。");
                cmbMainRepairAreaLocation.Focus();
                return;
            }


            if (Component.MessageBox.MyMessageBox.ShowQuestion(string.Format("确定保存吗？")) != MessageBoxResult.OK)
                return;

            if(cmbRepairArea.SelectedItem == null)
            {
                RepairAreaId = null;
                RepairAreaName = null;
            }
            else
            {
                RepairAreaId = (cmbRepairArea.SelectedItem as DataRowView).Row.Field<long?>("RepairAreaId");
                RepairAreaName = (cmbRepairArea.SelectedItem as DataRowView).Row.Field<string>("RepairAreaName");
            }

            if (cmbMainRepairAreaLocation.SelectedItem == null)
            {
                RepairAreaLocationId = null;
                RepairAreaLocationName = null;
            }
            else
            {
                RepairAreaLocationId = (cmbMainRepairAreaLocation.SelectedItem as DataRowView).Row.Field<long?>("RepairAreaLocationId");
                RepairAreaLocationName = (cmbMainRepairAreaLocation.SelectedItem as DataRowView).Row.Field<string>("RepairAreaLocationName");
            }

            Summarize = txtSummarize.Text.Trim();
            this.DialogResult = true;
        }
    }
}
