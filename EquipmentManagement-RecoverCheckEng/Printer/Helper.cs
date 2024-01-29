using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Printer
{
    public static class Helper
    {
        private static string setting_path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Framework.SysVar.SETTING_FILE_NAME);
        public static Data.Result PrintCode(System.Data.DataSet set, string templateCode)
        {
            List<System.Data.DataTable> dts = null;
            if(set!=null)
            {
                dts = new List<System.Data.DataTable>();
                foreach(System.Data.DataTable dt in set.Tables)
                {
                    dts.Add(dt);
                }
            }

            return PrintCode(dts, templateCode);
        }

        public static Data.Result PrintCode(List<System.Data.DataTable> dts, string templateCode)
        {
            Data.Result result = new Data.Result() { HasError = false };


            Data.Parameters paraeters = new Data.Parameters();
            System.Data.DataTable dt = null;

            if (dts != null)
            {
                if (dts.Count == 1)
                {
                    dt = dts[0];
                    dt.TableName = "Source";
                }
                else if (dts.Count == 2)
                {
                    dt = dts[1];
                    dt.TableName = "Source";

                    if (dts[0].Rows.Count > 0)
                    {
                        foreach (System.Data.DataColumn col in dts[0].Columns)
                        {
                            paraeters.Add(col.ColumnName, dts[0].Rows[0][col]);
                        }
                    }
                }
            }

            string printerName = null;
            try
            {
                printerName = WinAPI.File.INIFileHelper.Read(string.Format("PrintCode-{0}", templateCode), "PrinterName", setting_path);
            }
            catch { }

            if (string.IsNullOrEmpty(printerName))
            {
                printerName = WinAPI.Computer.LocalPrinterName;
            }

            try
            {
                Data.Result<string> r = Printer.InstanceEx.Print(templateCode, dt, paraeters, true, false, printerName);
                if (r.HasError)
                {
                    result.HasError = true;
                    result.Message = r.Message;
                }
                else
                {
                    WinAPI.File.INIFileHelper.Write(string.Format("PrintCode-{0}", templateCode), "PrinterName", r.Value, setting_path);
                    result.HasError = false;
                }
            }
            catch (Exception e)
            {
                result.HasError = true;
                result.Message = e.Message;
            }

            return result;
        }
    }
}
