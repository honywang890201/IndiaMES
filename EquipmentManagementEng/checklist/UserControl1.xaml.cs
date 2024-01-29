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
using System.Windows.Navigation;
using System.Windows.Shapes;
using WinAPI;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;
using MoonPdfLib;
using System.Net;
using System.IO;
using Microsoft.Win32;
using System.Diagnostics;
using Microsoft.VisualBasic.Devices;
using System.Data.OleDb;
using System.Drawing;
using Spire.Pdf;
using Spire.Pdf.Graphics;
using Spire.Pdf.Grid;
using Spire.Pdf.Tables;

namespace checklist
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class UserControl1 : Component.Controls.User.UserVendor
    {
        private string setting_path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Framework.SysVar.SETTING_FILE_NAME);
        private DataRowView SelectRow = null;

        private bool _IsLoad = false;
        private string MOCode, QTY, CustomerName, SysUserName, strSWVer, strBuildTime, strBoxMac, strBoxSN, strBoxSTB, strCarTonRule, strCartonBox, strPEStatus, strQCStatus, strAttachment, stracsn, stracmac, stracstb, strsnrule, strstbrule, strmacrule, strqrrule, strQRTxt, strItemName;
        private bool IsCheckTwoDimensionalCode = false;
        private int TwoDimensionalCodeLength = 0;
        //private bool IsZN = false;//兆能项目判断标志，用于是否强制打开连接ovt恢复出厂数据库
        // private bool IsDrPeng = false;//鹏博士项目判断标志，用于获取鹏博士烧号数据中的WiFi mac并上传至mes方便后期报盘文件导出
        private bool IsGSAT = false;//菲律宾GSAT项目判断标志，用于获取菲律宾GSAT数据中的卡号并上传至mes方便后期报盘文件导出
        private bool IsGx6615 = false;//智利项目判断标志位
        private bool IsSllk = false;//斯里兰卡项目判断标志位
        private bool IsP60 = false;//P60项目判断使用，耦合测试时用的WiFimac作为过站标志。
        private string ZhiLiBegin, ZhiLiEnd;
        private bool DataBaseChange = false;//判断是否可以切换数据库连接状态
        private long MOId = -1;
        private bool PorQ = false;
        /////////////////////////////////////////////////////////////////////////////////////////////////////
        MySqlDataReader reader = null;
        String mysqlStr = null; //"Database=ott_package;Data Source=192.168.0.119;User Id=root;Password=123456;pooling=false;CharSet=utf8;port=3306";
        MySqlConnection mysql = null;// new MySqlConnection(mysqlStr);
        MySqlCommand mySqlCommand = null;// new MySqlCommand("SELECT COUNT(*)FROM restore_order_all_t", mysql);
        string MESServerIp = null, FactoryServerIp = null;
        /////////////////////////////////////////////////////////////////////////////////////////////////////

        public UserControl1(Framework.SystemAuthority authority) :
            base(authority)
        {
            InitializeComponent();

            root.Background = new ImageBrush(WinAPI.File.ImageHelper.ConvertToImageSource(Component.App.BackgroudImage));

        }
        private void UserVendor_Loaded(object sender, RoutedEventArgs e)
        {
            if (false == System.IO.Directory.Exists("FirstCheck/"))
            {
                //创建pic文件夹
                System.IO.Directory.CreateDirectory("FirstCheck/");
            }
            DeleteFolder("FirstCheck/");
            string sql = @"SELECT Bas_Department.DepartmentName
                            FROM Set_User
			 LEFT JOIN Bas_Department ON Bas_Department.DepartmentId=Set_User.DepartmentId
			 WHERE Set_User.SysUserId=@UserId";
            Parameters parameters = new Parameters();
            parameters.Add("UserId", Framework.App.User.UserId);
            DataTable source = DB.DBHelper.GetDataTable(sql, parameters, ExecuteType.Text);
            SelectRow = source.DefaultView[0];
            tital.Header = SelectRow["DepartmentName"].ToString() ;
            if (SelectRow["DepartmentName"].ToString() == "工程部")
            {
                PorQ = true;
            }
            else if (SelectRow["DepartmentName"].ToString() == "品质部")
            {
                PorQ = false;
            }
            else
            {
                MessageBox.Show("登陆的用户名既不是工程部也不品质人员，限制使用!!!,请退出MES用正确的员工号登陆!!!");
                table.Visibility = Visibility.Collapsed;
            }
        }


        private void LoadMO()
        {
            if (tbMO.Value == null)
            {
                MessageBox.Show("订单不能为空");
                return;
            }
            MOId = (long)tbMO.Value;
            Microsoft.VisualBasic.Devices.Computer MyComputer = new Microsoft.VisualBasic.Devices.Computer();
            string sql = @"SELECT Bas_MO_Template.MOTemplateId AS TemplateId,
	   Bas_MO_Template.TemplateDesc,
	   Bas_MO_Template.Copies,
	   Set_File.FileId,
	   Set_File.FileServerId,
	   Bas_MO_Template.Sequence
FROM dbo.Bas_MO_Template   WITH(NOLOCK) 
LEFT JOIN dbo.Set_File  WITH(NOLOCK) ON Bas_MO_Template.TemplateId=Set_File.FileId
WHERE MOId=@MOId AND OPId=105  ORDER BY Sequence";
            Parameters parameters = new Parameters();
            parameters.Add("MOId", MOId);
            //parameters.Add("OPId", Framework.App.Resource.StationId);
            try
            {
                DataTable source = DB.DBHelper.GetDataTable(sql, parameters, ExecuteType.Text);
                if (source.Rows.Count < 1)
                {
                    sql = @"SELECT Bas_Item_Template.ItemTemplateId AS TemplateId,
	   Bas_Item_Template.TemplateDesc,
	   Bas_Item_Template.Copies,
	   Set_File.FileId,
	   Set_File.FileServerId,
	   Bas_Item_Template.Sequence
FROM dbo.Bas_Item_Template   WITH(NOLOCK) 
LEFT JOIN dbo.Set_File  WITH(NOLOCK) ON Bas_Item_Template.TemplateId=Set_File.FileId
WHERE ItemId=@ItemId AND OPId=105  ORDER BY Sequence";
                    parameters = new Parameters();
                    parameters.Add("ItemId", SelectRow["ItemId"]);
                    //parameters.Add("OPId", Framework.App.Resource.StationId);
                    source = DB.DBHelper.GetDataTable(sql, parameters, ExecuteType.Text);
                }
                if (source.Rows.Count > 0)
                {
                }
                WinAPI.SysFunction.KillProcessByName("lppa");
                foreach (DataRow row in source.Rows)
                {
                    byte[] buffer = null;
                    Component.Controls.User.FileHelper helper = null;
                    try
                    {
                        helper = new Component.Controls.User.FileHelper();
                        helper.Init((long)row["FileId"]);

                        Download(helper.ServerFileName, helper.FileServerHelper.FileServerUser, helper.FileServerHelper.FileServerPwd, helper.FileServerHelper.Address, "FirstCheck/");
                        MyComputer.FileSystem.RenameFile("FirstCheck/" + helper.ServerFileName, helper.FileName);

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "文件传输时内存溢出");
                        continue;
                    }
                }
                string a = "FirstCheck/";// + helper.FileName.Substring(0, helper.FileName.IndexOf("."));
                foreach (string d in Directory.GetFileSystemEntries(a))
                {
                    //EPostion.Items.Add(d);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        /// <summary>
        /// 将格式为rar的压缩文件解压到指定的目录
        /// </summary>
        /// <param name="rarFileName">要解压rar文件的路径</param>
        /// <param name="saveDir">解压后要保存到的目录</param>
        public static void DeCompressRar(string rarFileName, string saveDir)
        {
            string regKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\WinRAR.exe";
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(regKey);
            if (registryKey == null)
            {
                MessageBox.Show("本机还没有安装解压软件-WinRAR，请安装后再试");
                return;
            }
            string winrarPath = registryKey.GetValue("").ToString();
            registryKey.Close();
            string winrarDir = System.IO.Path.GetDirectoryName(winrarPath);
            String commandOptions = string.Format("x {0} {1} -y", rarFileName, saveDir);
            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = System.IO.Path.Combine(winrarDir, "winrar.exe");
            processStartInfo.Arguments = commandOptions;
            processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            Process process = new Process();
            process.StartInfo = processStartInfo;
            process.Start();
            process.WaitForExit();
            process.Close();
        }
        private void Download(string fileName, string username, string password, string path, string filepath)
        {
            FtpWebRequest reqFTP;
            try
            {
                string filePath = filepath;// "";// Application.StartupPath;
                FileStream outputStream = new FileStream(filePath + "\\" + fileName, FileMode.Create);
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(path + fileName));
                reqFTP.Method = WebRequestMethods.Ftp.DownloadFile;
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(username, password);
                reqFTP.UsePassive = false;
                reqFTP.Timeout = 1000 * 60;
                reqFTP.ReadWriteTimeout = 1000 * 60;
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                Stream ftpStream = response.GetResponseStream();
                long cl = response.ContentLength;
                int bufferSize = 2048;
                int readCount;
                byte[] buffer = new byte[bufferSize];
                readCount = ftpStream.Read(buffer, 0, bufferSize);
                while (readCount > 0)
                {
                    outputStream.Write(buffer, 0, readCount);
                    readCount = ftpStream.Read(buffer, 0, bufferSize);
                }
                ftpStream.Close();
                outputStream.Close();
                response.Close();


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (tbMO.Value == null)
            {
               
               MessageBox.Show("订单不能为空");
               return;
            }
            MOId = (long)tbMO.Value;
            if (BoxSN.Text.Trim() != STBSN.Text.Trim())
            {
                MessageBox.Show("机身条码SN与烧录进盒子的SN不一致!!!", "错误提示");
                return;
            }
            if (BoxMAC.Text.Trim() != STBMAC.Text.Trim())
            {
                MessageBox.Show("机身条码MAC与烧录进盒子的MAC不一致!!!", "错误提示");
                return;
            }
            if (BoxSTB.Text.Trim() != STBSTB.Text.Trim())
            {
                MessageBox.Show("机身条码STBNO与烧录进盒子的STBNO不一致!!!", "错误提示");
                return;
            }
            string attachment = string.Empty;
            if(power.IsChecked==true)
            {
                attachment = attachment+ "⊙电源";

            }
            if (battery.IsChecked == true)
            {
                attachment = attachment + "⊙电池";

            }
            if (remote.IsChecked == true)
            {
                attachment = attachment + "⊙遥控器";

            }
            if (hdmicable.IsChecked == true)
            {
                attachment = attachment + "⊙HDMI线";

            }
            if (avcable.IsChecked == true)
            {
                attachment = attachment + "⊙AV线";

            }
            if (netcable.IsChecked == true)
            {
                attachment = attachment + "⊙网线";

            }
            if (manual.IsChecked == true)
            {
                attachment = attachment + "⊙说明书";

            }
            if (warranty.IsChecked == true)
            {
                attachment = attachment + "⊙保修卡";

            }
            if (desiccant.IsChecked == true)
            {
                attachment = attachment + "⊙干燥剂";

            }
            Parameters parameters = new Parameters();
            parameters.Add("MOId", MOId);
            parameters.Add("UserId", Framework.App.User.UserId);
            parameters.Add("Depart", PorQ);
            parameters.Add("SWVer", SWVer.Text.Trim());
            parameters.Add("BuildTime", BuildTime.Text.Trim());
            parameters.Add("BoxMac", BoxMAC.Text.Trim());
            parameters.Add("BoxSN", BoxSN.Text.Trim());
            parameters.Add("BoxSTB", BoxSTB.Text.Trim());
            parameters.Add("STBMac", STBMAC.Text.Trim());
            parameters.Add("STBSN", STBSN.Text.Trim());
            parameters.Add("STBSTB", STBSTB.Text.Trim());
            parameters.Add("Attachment", attachment,SqlDbType.NVarChar, int.MaxValue);
            parameters.Add("macrule", MacRule.Text.Trim(), SqlDbType.NVarChar, int.MaxValue);
            parameters.Add("stbrule", STBRule.Text.Trim(), SqlDbType.NVarChar, int.MaxValue);
            parameters.Add("snrule", SNRule.Text.Trim(), SqlDbType.NVarChar, int.MaxValue);
            parameters.Add("acmac", ACMac.Text.Trim() , SqlDbType.NVarChar, int.MaxValue);
            parameters.Add("acsn", ACSN.Text.Trim(), SqlDbType.NVarChar, int.MaxValue);
            parameters.Add("acstb", ACSTB.Text.Trim(), SqlDbType.NVarChar, int.MaxValue);
            parameters.Add("qrrule", new TextRange(QRRule.Document.ContentStart, QRRule.Document.ContentEnd).Text, SqlDbType.NVarChar, int.MaxValue);
            parameters.Add("QRTxt", new TextRange(qrbarcode.Document.ContentStart,qrbarcode.Document.ContentEnd).Text, SqlDbType.NVarChar, int.MaxValue);
            parameters.Add("CartonSN", Carton.Text.Trim());
            parameters.Add("CartonRule", CartonRule.Text.Trim()); 
            parameters.Add("Return_Message", null, SqlDbType.NVarChar, int.MaxValue, ParameterDirection.Output);
            parameters.Add("Return_Value", null, SqlDbType.Int, ParameterDirection.ReturnValue);
            Result<Parameters, DataSet> result = null;
            try
            {
                result = DB.DBHelper.ExecuteParametersSource("Inp_FirstCheck_P", parameters, ExecuteType.StoredProcedure);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "异常");
                return;
            }

            if (result.HasError)
            {
                MessageBox.Show(result.Message, "异常");
                return;
            }

            if ((int)result.Value1["Return_Value"] != 1)
            {
                MessageBox.Show(result.Value1["Return_Message"].ToString(), "返回错误");
                return;
            }
            else
            {
                MessageBox.Show(result.Value1["Return_Message"].ToString(), "成功");
            }
        }
        private void DeleteFolder(string dir)
        {
            foreach (string d in Directory.GetFileSystemEntries(dir))
            {
                if (File.Exists(d))
                {
                    FileInfo fi = new FileInfo(d);
                    if (fi.Attributes.ToString().IndexOf("ReadOnly") != -1)
                        fi.Attributes = FileAttributes.Normal;
                    //txtMessage.AddMessage("正在删除文件:" + d, false);
                    File.Delete(d);//直接删除其中bai的文件  
                }
                else
                {
                    DirectoryInfo d1 = new DirectoryInfo(d);
                    if (d1.GetFiles().Length != 0)
                    {
                        DeleteFolder(d1.FullName);////递归删除子文件夹
                    }
                    //txtMessage.AddMessage("正在删除文件夹:"+ d, false);
                    Directory.Delete(d, true);
                }
            }
        }
        private void EPostion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (EPostion.SelectedValue != null)
            //{
            //    this.pdfViewer.OpenFile(EPostion.SelectedValue.ToString());
            //}
        }
        private void AddHeader()
        {
            string input = "Grid.pdf";

            //Open the document from disk
            PdfDocument doc = new PdfDocument();
            doc.LoadFromFile(input);
            PdfBrush brush = PdfBrushes.Black;
            PdfPen pen = new PdfPen(brush, 0.75f);
            PdfTrueTypeFont font = new PdfTrueTypeFont(new Font("宋体", 10f), true);
            PdfStringFormat rightAlign = new PdfStringFormat(PdfTextAlignment.Right);
            PdfStringFormat leftAlign = new PdfStringFormat(PdfTextAlignment.Left);
            rightAlign.MeasureTrailingSpaces = true;
            rightAlign.MeasureTrailingSpaces = true;
            PdfMargins margin = doc.PageSettings.Margins;

            float space = font.Height * 0.75f;
            float x = 0;
            float y = 0;
            float width = 0;

            //Create a new pdf document
            PdfDocument newPdf = new PdfDocument();
            PdfPageBase newPage;

            foreach (PdfPageBase page in doc.Pages)
            {
                //Add new page
                newPage = newPdf.Pages.Add(page.Size, new PdfMargins(0));

                newPage.Canvas.SetTransparency(0.5f);
                x = margin.Left;
                width = page.Canvas.ClientSize.Width - margin.Left - margin.Right;
                y = margin.Top - space;

                //Draw header line
                newPage.Canvas.DrawLine(pen, x, y + 10, x + width, y + 10);
                y = y + 10 - font.Height;

                //Draw header image into newPage
                newPage.Canvas.SetTransparency(0.5f);
                PdfImage headerImage = PdfImage.FromFile(@"ovt.png");
                newPage.Canvas.DrawImage(headerImage, new PointF(5, 5));

                //Draw header text into newPage
                newPage.Canvas.DrawString("南宁市欧韦电子科技有限公司", font, brush, x + width, y, rightAlign);

                //Draw footer image into newPage
               // PdfImage footerImage = PdfImage.FromFile(@"ovt.png");
               // newPage.Canvas.DrawImage(footerImage, new PointF(0, newPage.Canvas.ClientSize.Height - footerImage.PhysicalDimension.Height));

                brush = PdfBrushes.DarkBlue;
                font = new PdfTrueTypeFont(new Font("Arial", 12f), true);
                y = newPage.Canvas.ClientSize.Height - margin.Bottom - font.Height;

                //Draw footer text into newPage
                //newPage.Canvas.DrawString("Created by E-iceblue Co,.Ltd", font, brush, x, y, leftAlign);

                newPage.Canvas.SetTransparency(1);

                //Draw the page into newPage
                page.CreateTemplate().Draw(newPage.Canvas, new PointF(0, 0));
            }

            string output = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)+ "/"+MOCode+"首件测试报告.pdf";

            //Save the document
            newPdf.SaveToFile(output);
            newPdf.Close();
            this.pdfViewer.OpenFile(output);

        }

        private void PDFMake()
        {
            PdfDocument doc = new PdfDocument();

            //Margin
            PdfUnitConvertor unitCvtr = new PdfUnitConvertor();
            PdfMargins margin = new PdfMargins();
            margin.Top = unitCvtr.ConvertUnits(2.54f, PdfGraphicsUnit.Centimeter, PdfGraphicsUnit.Point);
            margin.Bottom = margin.Top;
            margin.Left = unitCvtr.ConvertUnits(3.17f, PdfGraphicsUnit.Centimeter, PdfGraphicsUnit.Point);
            margin.Right = margin.Left;

            //Create one page
            PdfPageBase page = doc.Pages.Add(PdfPageSize.A4);
            float y = 2, px = 0;
            float x1 = page.Canvas.ClientSize.Width;

            //Title
            //PdfBrush brush1 = PdfBrushes.Black;
            //PdfTrueTypeFont font1 = new PdfTrueTypeFont(new Font("宋体", 16f), true);
            //PdfStringFormat format1 = new PdfStringFormat(PdfTextAlignment.Center);
            page.Canvas.DrawString("机顶盒组包段首件检查记录",
                new PdfTrueTypeFont(new Font("宋体", 32), true),
                PdfBrushes.Black,
                page.Canvas.ClientSize.Width / 2,
                y,
                new PdfStringFormat(PdfTextAlignment.Center));
            y = y + new PdfTrueTypeFont(new Font("黑体", 32), true).MeasureString("机顶盒组包段首件检查记录", new PdfStringFormat(PdfTextAlignment.Center)).Height;
            y = y + 30;
            page.Canvas.DrawString("客户名称:"+CustomerName,
                new PdfTrueTypeFont(new Font("宋体", 12), true),
                PdfBrushes.Black,
                0,
                y,
                new PdfStringFormat(PdfTextAlignment.Left));
            px = px + new PdfTrueTypeFont(new Font("宋体", 12), true).MeasureString("客户名称:", new PdfStringFormat(PdfTextAlignment.Left)).Width +40;

            page.Canvas.DrawString("产品型号:"+strItemName,
                new PdfTrueTypeFont(new Font("宋体", 12), true),
                PdfBrushes.Black,
                px,
                y,
                new PdfStringFormat(PdfTextAlignment.Left));
            px = px + new PdfTrueTypeFont(new Font("宋体", 12), true).MeasureString("产品型号:", new PdfStringFormat(PdfTextAlignment.Left)).Width + 80;

            page.Canvas.DrawString("订单号:"+MOCode,
                new PdfTrueTypeFont(new Font("宋体", 12), true),
                PdfBrushes.Black,
                px,
                y,
                new PdfStringFormat(PdfTextAlignment.Left));

            px = px + new PdfTrueTypeFont(new Font("宋体", 12), true).MeasureString("订单号:", new PdfStringFormat(PdfTextAlignment.Left)).Width + 110;

            page.Canvas.DrawString("订单数量:"+QTY,
                new PdfTrueTypeFont(new Font("宋体", 12), true),
                PdfBrushes.Black,
                px,
                y,
                new PdfStringFormat(PdfTextAlignment.Left));

            y = y + new PdfTrueTypeFont(new Font("宋体", 12), true).MeasureString("产品型号:", new PdfStringFormat(PdfTextAlignment.Left)).Height;
            y = y + 5;

            //Create a grid
            PdfGrid grid = new PdfGrid();
            grid.Style.Font = new PdfTrueTypeFont(new Font("宋体", 10f), true);

            grid.Style.TextBrush = PdfBrushes.Black;
            grid.Style.BackgroundBrush = PdfBrushes.White;
            grid.Style.CellPadding = new PdfPaddings(1, 1, 1, 1);
            grid.Columns.Add(4);
            float width
                = page.Canvas.ClientSize.Width - (4 + 1);
            grid.Columns[0].Width = width * 0.25f;
            grid.Columns[1].Width = width * 0.25f;
            grid.Columns[2].Width = width * 0.35f;
            grid.Columns[3].Width = width * 0.15f;
           // grid.Columns[4].Width = width * 0.10f;
            PdfGridRow headerRow = grid.Headers.Add(1)[0];
            //headerRow.Style.Font = new PdfTrueTypeFont(new Font("宋体", 11f), true);
            headerRow.Style.BackgroundBrush
                = PdfBrushes.LightGray;
            headerRow.Cells[0].Value = "检验项目";
            headerRow.Cells[0].StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
            headerRow.Cells[1].StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
            headerRow.Cells[3].StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
            headerRow.Cells[1].Value = "检验内容";
            headerRow.Cells[1].ColumnSpan = 2;
            headerRow.Cells[3].Value = "检验结果";
            PdfGridRow Version = grid.Rows.Add();

            Version.Cells[0].Value = "号码核对";
            Version.Cells[0].StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
            Version.Cells[0].RowSpan = 6;
            Version.Cells[1].Value = "机器MAC:\r\n"+strBoxMac;
            Version.Cells[3].StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
            Version.Cells[3].Value = "PASS";
            Version.Cells[1].ColumnSpan = 2;
            Version = grid.Rows.Add();
            Version.Cells[1].Value = "贴纸MAC:\r\n" + strBoxMac;
            Version.Cells[1].ColumnSpan = 2;
            Version.Cells[3].StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
            Version.Cells[3].Value = "PASS";
            Version = grid.Rows.Add();
            Version.Cells[1].Value = "机器SN:\r\n" + strBoxSN;
            Version.Cells[1].ColumnSpan = 2;
            Version.Cells[3].StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
            Version.Cells[3].Value = "PASS";
            Version = grid.Rows.Add();
            Version.Cells[1].Value = "贴纸SN:\r\n" + strBoxSN;
            Version.Cells[1].ColumnSpan = 2;
            Version.Cells[3].StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
            Version.Cells[3].Value = "PASS";
            Version = grid.Rows.Add();
            Version.Cells[1].Value = "机器STB:\r\n"+strBoxSTB;
            Version.Cells[1].ColumnSpan = 2;
            Version.Cells[3].StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
            Version.Cells[3].Value = "PASS";
            Version = grid.Rows.Add();
            Version.Cells[1].Value = "贴纸STB:\r\n" + strBoxSTB;
            Version.Cells[1].ColumnSpan = 2;
            Version.Cells[3].StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
            Version.Cells[3].Value = "PASS";



            Version = grid.Rows.Add();
            Version.Cells[0].Value = "版本信息";
            Version.Cells[0].StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
            Version.Cells[0].RowSpan = 3;
            //Version.Cells[3].StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
            //Version.Cells[3].Value = "PASS";
            //Version.Cells[1].Value = "硬件版本";
            Version.Cells[1].ColumnSpan = 3;
            Version = grid.Rows.Add();
            Version.Cells[3].StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
            Version.Cells[3].Value = "PASS";
            Version.Cells[1].Value = "软件版本:\r\n" + strSWVer;
            Version.Cells[1].ColumnSpan = 2;
            Version = grid.Rows.Add();
            Version.Cells[3].StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
            Version.Cells[3].Value = "PASS";
            Version.Cells[1].Value = "编译时间:\r\n" + strBuildTime;
            Version.Cells[1].ColumnSpan = 2;

            Version = grid.Rows.Add();
            Version.Cells[0].Value = "附件检查";
            Version.Cells[0].StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
            Version.Cells[0].RowSpan = 7;
            Version.Cells[1].Style.Font = new PdfTrueTypeFont(new Font("宋体", 10f), true);
            Version.Cells[3].StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
            Version.Cells[3].Value = "PASS";
            Version.Cells[1].Value = "此单附件包括:\r\n"+strAttachment;
            Version.Cells[1].RowSpan = 7;
            Version.Cells[3].RowSpan = 7;
            //Version.Cells[4].RowSpan = 7;
            Version.Cells[1].ColumnSpan = 2;
            Version = grid.Rows.Add();
            Version.Cells[1].Value = "";
            Version.Cells[1].ColumnSpan = 2;
            Version = grid.Rows.Add();
            Version.Cells[1].Value = "";
            Version.Cells[1].ColumnSpan = 2;
            Version = grid.Rows.Add();
            Version.Cells[1].Value = "";
            Version.Cells[1].ColumnSpan = 2;
            Version = grid.Rows.Add();
            Version.Cells[1].Value = "";
            Version.Cells[1].ColumnSpan = 2;
            Version = grid.Rows.Add();
            Version.Cells[1].Value = "";
            Version.Cells[1].ColumnSpan = 2;
            Version = grid.Rows.Add();
            Version.Cells[1].Value = "";
            Version.Cells[1].ColumnSpan = 2;
            Version = grid.Rows.Add();

            Version.Cells[0].Value = "外箱贴纸检查";
            Version.Cells[0].StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
            Version.Cells[0].RowSpan = 10;
            Version.Cells[1].Style.Font = new PdfTrueTypeFont(new Font("宋体", 10f), true);
            Version.Cells[1].Style.BackgroundBrush
                = PdfBrushes.LightGray;
            Version.Cells[1].StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
            Version.Cells[2].Style.BackgroundBrush
                = PdfBrushes.LightGray;
            Version.Cells[2].StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
            Version.Cells[1].Value = "二维码规则";
            Version.Cells[2].Value = "二维码扫描";
            Version = grid.Rows.Add();
            Version.Cells[1].StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
            Version.Cells[2].StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
            Version.Cells[3].StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
            Version.Cells[3].Value = "PASS";
            Version.Cells[1].Value = strqrrule;
            Version.Cells[2].Value = "见附件页";
            Version = grid.Rows.Add();
            Version.Cells[1].Style.Font = new PdfTrueTypeFont(new Font("宋体", 10f), true);
            Version.Cells[1].Style.BackgroundBrush
                = PdfBrushes.LightGray;
            Version.Cells[1].StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
            Version.Cells[2].Style.BackgroundBrush
                = PdfBrushes.LightGray;
            Version.Cells[2].StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
            Version.Cells[1].Value = "STBID规则";
            Version.Cells[2].Value = "STBID扫描";
            Version = grid.Rows.Add();
            Version.Cells[1].StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
            Version.Cells[2].StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
            Version.Cells[3].StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
            Version.Cells[3].Value = "PASS";
            Version.Cells[1].Value = strstbrule;
            Version.Cells[2].Value = strBoxSTB;
            Version = grid.Rows.Add();
            Version.Cells[1].Style.Font = new PdfTrueTypeFont(new Font("宋体", 10f), true);
            Version.Cells[1].Style.BackgroundBrush
                = PdfBrushes.LightGray;
            Version.Cells[1].StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
            Version.Cells[2].Style.BackgroundBrush
                = PdfBrushes.LightGray;
            Version.Cells[2].StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
            Version.Cells[1].Value = "SN规则";
            Version.Cells[2].Value = "SN扫描";
            Version = grid.Rows.Add();
            Version.Cells[1].StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
            Version.Cells[2].StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
            Version.Cells[3].StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
            Version.Cells[3].Value = "PASS";
            Version.Cells[1].Value = strsnrule;
            Version.Cells[2].Value = strBoxSN;
            Version = grid.Rows.Add();
            Version.Cells[1].Style.Font = new PdfTrueTypeFont(new Font("宋体", 10f), true);
            Version.Cells[1].Style.BackgroundBrush
                = PdfBrushes.LightGray;
            Version.Cells[1].StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
            Version.Cells[2].Style.BackgroundBrush
                = PdfBrushes.LightGray;
            Version.Cells[2].StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
            Version.Cells[1].Value = "MAC规则";
            Version.Cells[2].Value = "MAC扫描";
            Version = grid.Rows.Add();
            Version.Cells[1].StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
            Version.Cells[2].StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
            Version.Cells[3].StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
            Version.Cells[3].Value = "PASS";
            Version.Cells[1].Value = strmacrule;
            Version.Cells[2].Value = strBoxMac;

            Version = grid.Rows.Add();
            Version.Cells[1].Style.Font = new PdfTrueTypeFont(new Font("宋体", 10f), true);
            Version.Cells[1].Style.BackgroundBrush
                = PdfBrushes.LightGray;
            Version.Cells[1].StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
            Version.Cells[2].Style.BackgroundBrush
                = PdfBrushes.LightGray;
            Version.Cells[2].StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
            Version.Cells[1].Value = "箱号规则";
            Version.Cells[2].Value = "箱号扫描";
            Version = grid.Rows.Add();
            Version.Cells[1].StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
            Version.Cells[2].StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
            Version.Cells[3].StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
            Version.Cells[3].Value = "PASS";
            Version.Cells[1].Value = strCarTonRule;
            Version.Cells[2].Value = strCartonBox;


            PdfLayoutResult result1 = grid.Draw(page, new PointF(0, y));
            y = y + result1.Bounds.Height + 5;
            page.Canvas.DrawString("检验人:"+SysUserName,
               new PdfTrueTypeFont(new Font("宋体", 12), true),
               PdfBrushes.Black,
               px,
               y,
               new PdfStringFormat(PdfTextAlignment.Left));

            y = y + new PdfTrueTypeFont(new Font("宋体", 12), true).MeasureString("检验人:", new PdfStringFormat(PdfTextAlignment.Left)).Height;
            y = y + 5;

            PdfPageBase page1 = doc.Pages.Add(PdfPageSize.A4);
            float y1 = 5, px1 = 0;
            float x11 = page1.Canvas.ClientSize.Width;

            page1.Canvas.DrawString("附件页:",
               new PdfTrueTypeFont(new Font("宋体", 32), true),
               PdfBrushes.Black,
               0,//page1.Canvas.ClientSize.Width / 2,
               y1,
               new PdfStringFormat(PdfTextAlignment.Left));
            y1 = y1 + new PdfTrueTypeFont(new Font("黑体", 32), true).MeasureString("机顶盒组包段首件检查记录", new PdfStringFormat(PdfTextAlignment.Center)).Height;
            y1 = y1 + 30;
            page1.Canvas.DrawString("扫描到的条码或二维码:",
                new PdfTrueTypeFont(new Font("宋体", 12), true),
                PdfBrushes.Black,
                0,
                y1,
                new PdfStringFormat(PdfTextAlignment.Left));
            px1 = px1 + new PdfTrueTypeFont(new Font("宋体", 12), true).MeasureString("客户名称:", new PdfStringFormat(PdfTextAlignment.Left)).Width + 80;
            y1 = y1 + new PdfTrueTypeFont(new Font("黑体", 32), true).MeasureString("机顶盒组包段首件检查记录", new PdfStringFormat(PdfTextAlignment.Center)).Height;
            page1.Canvas.DrawLine(new PdfPen(System.Drawing.Color.Red, 1f), 0, y1, page1.Canvas.ClientSize.Width, y1);
            page1.Canvas.DrawString(strQRTxt,
                new PdfTrueTypeFont(new Font("宋体", 8), true),
                PdfBrushes.Black,
                0,
                y1,
                new PdfStringFormat(PdfTextAlignment.Left));


            doc.SaveToFile("Grid.pdf");
            doc.Close();
            //this.pdfViewer.OpenFile("Grid.pdf");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
             MOId = (long)tbMO.Value;
            string sql = @" 
             SELECT Bas_MO.MOCode,
			 Bas_MO.QTY,
			 Bas_Customer.CustomerName,
             Set_User.SysUserName,
			 Inp_FirstCheck.SWVer,
			 Inp_FirstCheck.BuildTime,
			 Inp_FirstCheck.BoxMac,
			 Inp_FirstCheck.BoxSN,
			 Inp_FirstCheck.BoxSTB,
			 Inp_FirstCheck.CartonBox,
			 Inp_FirstCheck.PEStatus,
			 Inp_FirstCheck.QCStatus,
			 Inp_FirstCheck.Attachment,
			 Inp_FirstCheck.acsn,
			 Inp_FirstCheck.acmac,
			 Inp_FirstCheck.acstb,
			 Inp_FirstCheck.snrule,
			 Inp_FirstCheck.stbrule,
			 Inp_FirstCheck.macrule,
			 Inp_FirstCheck.qrrule,
			 Inp_FirstCheck.QRTxt,
             Inp_FirstCheck.CarTonRule,
			 Bas_Item.ItemName
			 FROM Inp_FirstCheck 
			 LEFT JOIN Bas_MO ON Bas_MO.MOId=Inp_FirstCheck.MOId
			 LEFT JOIN Set_User ON Set_User.SysUserId=Inp_FirstCheck.userid
			 LEFT JOIN  Bas_Customer ON Bas_Customer.CustomerId=Bas_MO.CustomerId
			 LEFT JOIN Bas_Item on Bas_Item.ItemId=Bas_MO.ItemId
             WHERE Inp_FirstCheck.MOId = @MOId";
            if(PorQ)
            {
                sql = sql + " and PEStatus is not null";
            }
            else
            {
                sql = sql + " and QCStatus is not null";
            }
            Parameters parameters = new Parameters();
            parameters.Add("MOId", MOId);
            try
            {
                DataTable source = DB.DBHelper.GetDataTable(sql, parameters, ExecuteType.Text);
                if (source.Rows.Count < 1)
                {
                    MessageBox.Show("此订单还没有首件记录");
                    return;
                }
                DataRowView SelectRow = source.DefaultView[0];
                CustomerName = SelectRow["CustomerName"].ToString();
                QTY= SelectRow["QTY"].ToString();
                MOCode= SelectRow["MOCode"].ToString();
                SysUserName= SelectRow["SysUserName"].ToString();
                strItemName= SelectRow["ItemName"].ToString();                
                strCartonBox = SelectRow["CartonBox"].ToString();
                strPEStatus = SelectRow["PEStatus"].ToString();
                strQCStatus = SelectRow["QCStatus"].ToString();
                strAttachment = SelectRow["Attachment"].ToString();
                stracsn = SelectRow["acsn"].ToString();
                stracmac = SelectRow["acmac"].ToString();
                strBuildTime= SelectRow["BuildTime"].ToString();
                strSWVer= SelectRow["SWVer"].ToString();
                strqrrule= SelectRow["qrrule"].ToString();
                strstbrule= SelectRow["stbrule"].ToString();
                strsnrule= SelectRow["snrule"].ToString();
                strBoxSN= SelectRow["BoxSN"].ToString();
                strmacrule= SelectRow["macrule"].ToString();
                strBoxMac = SelectRow["BoxMac"].ToString();
                strBoxSN = SelectRow["BoxSN"].ToString();
                strBoxSTB = SelectRow["BoxSTB"].ToString();
                strCartonBox= SelectRow["CartonBox"].ToString();
                strCarTonRule= SelectRow["CarTonRule"].ToString();
                strQRTxt= SelectRow["QRTxt"].ToString();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            PDFMake();
            AddHeader();
            MessageBox.Show("首件测试报告已经保存到桌面","提示");
        }

        private void EPostion_DropDownOpened(object sender, EventArgs e)
        {
            DeleteFolder("FirstCheck/");
            //EPostion.Items.Clear();
            LoadMO();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            return;

            if (e.Key == Key.Enter)
            {
                string sql = @"select 
				inp_lot.SN AS PCBASN,
				bas_mo_mac.mac AS MAC,
				Inp_Lot_PowerSN.PowerSN as 电源序列号,
				Inp_Lot_Attach.attach 遥控器序列号,
				Inp_lot_ProductSN.ProductSN AS ProductSN,
				bas_mo_mac.macStart AS MacStart,
				bas_mo_mac.macEnd AS MacEnd,
				inp_lot_en.en AS EN,
				bas_mo_mac.LanIp,
				bas_mo_mac.OUI,
				inp_box.boxsn AS BoxSN,
				Inp_Pallet.PalletSN AS PalletSN,
				 Inp_Pallet.[status] ,
				bas_mo_mac.CISN ,
				bas_mo_mac.DeviceSerialNumber,
				bas_mo_mac.DSN,
				bas_mo_mac.GponSN,
				Bas_MO_Mac.UserName,
				Bas_MO_Mac.UserPass,
				bas_mo_mac.WirelessNetName,
				Bas_MO_Mac.WlanPass,
				Bas_MO_Mac.STBNO,
				Convert(varchar(100),INP_lot.LastMoveDateTime,20) as CartonTime  
				from inp_lot with(nolock)
				left join inp_lot_ProductSN WITH(NOLOCK) ON Inp_lot.lotid=Inp_Lot_ProductSN.LotId
				left join bas_mo with(nolock) on inp_lot.moid=bas_mo.moid
				left join inp_lot_mac with(nolock) on Inp_Lot_Mac.lotid=inp_lot.lotid
				left join inp_lot_en with(nolock) on inp_lot_en.lotid=inp_lot.lotid
				left join Inp_Lot_PowerSN with(nolock) on Inp_Lot_PowerSN.lotid=inp_lot.lotid
				left join Inp_Lot_Attach with(nolock) on Inp_Lot_Attach.lotid=inp_lot.lotid
				left join inp_lot_bosa with(nolock) on inp_lot_bosa.lotid=inp_lot.lotid
				left join Inp_Box with(nolock) on inp_lot.boxid=inp_box.boxid
				left join bas_mo_mac with(nolock) on inp_lot_mac.mac=bas_mo_mac.mac
				left join Inp_Pallet with(nolock) on inp_box.PalletId=Inp_Pallet.PalletId
				where 1=1 
				and inp_box.boxsn=@boxsn ORDER BY Bas_MO_Mac.Mac";


                Parameters parameters = new Parameters()
                    .Add("boxsn", Carton.Text.Trim(), SqlDbType.NVarChar, 50);
                int handle = Component.MaskBusy.Busy(root, "正在查询数据...");
                System.Threading.Tasks.Task<Result<DataTable>>.Factory.StartNew(() =>
                {
                    Result<DataTable> result = new Result<DataTable>() { HasError = false };
                    DataTable dt = null;
                    try
                    {
                        dt = DB.DBHelper.GetDataTable(sql, parameters, ExecuteType.Text);
                    }
                    catch (Exception ex)
                    {
                        result.HasError = true;
                        result.Message = ex.Message;
                    }
                    result.Value = dt;
                    return result;

                }).ContinueWith(r =>
                {

                    if (r.Result.HasError)
                    {
                        MessageBox.Show(r.Result.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        dataGridQuery.ItemsSource = (r.Result.Value).DefaultView;
                    }


                    Component.MaskBusy.Hide(root, handle);
                }, Framework.App.Scheduler);

                Carton.SelectAll();
            }

        }





    }
}

