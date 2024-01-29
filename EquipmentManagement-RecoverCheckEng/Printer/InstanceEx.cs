using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace Printer
{
    public static class InstanceEx
    {
        private static Data.Result<string> Print(Template template, List<System.Data.DataTable> dts, List<System.Data.DataSet> sets, Data.Parameters parameters,
            bool showDialog, bool showPreview, bool useDefaultPrinter, string printerName)
        {
            Data.Result<string> result = new Data.Result<string>();
            if (template == null)
            {
                result.HasError = true;
                result.Message = "模板错误。";
                return result;
            }
            if (string.IsNullOrEmpty(template.Source))
            {
                result.HasError = true;
                result.Message = string.Format("模板源[{0}]为空。", template.TemplateCode);
                return result;
            }

            FastReport.Report report = null;
            try
            {
                report = new FastReport.Report();
                report.LoadFromString(template.Source);
                

                if (dts != null)
                {
                    foreach (System.Data.DataTable dt in dts)
                    {
                        if (dt != null)
                        {
                            report.RegisterData(dt, dt.TableName);
                        }
                    }
                }

                if (sets != null)
                {
                    foreach (System.Data.DataSet set in sets)
                    {
                        if (set != null)
                        {
                            report.RegisterData(set);
                            foreach (System.Data.DataTable dt in set.Tables)
                            {
                                if (!string.IsNullOrEmpty(dt.TableName))
                                {
                                    try
                                    {
                                        report.GetDataSource(dt.TableName).Enabled = true;
                                    }
                                    catch { }
                                }
                            }
                        }
                    }
                }

                if (parameters != null)
                {
                    foreach (Data.Parameter parameter in parameters)
                    {
                        report.SetParameterValue(parameter.ParameterName, parameter.ParameterValue);
                    }
                }

                report.PrintSettings.ShowDialog = showDialog;
                if (useDefaultPrinter)
                {
                    report.PrintSettings.Printer = WinAPI.Computer.LocalPrinterName;
                }
                else if (!string.IsNullOrEmpty(printerName))
                {
                    report.PrintSettings.Printer = printerName;
                }

                if (showPreview)
                {
                    report.Show();
                }
                else
                {
                    report.Print();
                }
                result.Value = report.PrintSettings.Printer;
                result.HasError = false;
                return result;

            }
            catch (Exception e)
            {
                result.HasError = true;
                result.Message = e.Message;
                return result;
            }
            finally
            {
                try
                {
                    if (report != null) report.Dispose();
                }
                catch { }
            }
        }

        public static Data.Result<string> Print(string templateCode, Data.Parameters parameters)
        {
            Data.Result<Template> result = Template.GetTemplate(templateCode);
            if (result.HasError)
            {
                return new Data.Result<string>()
                {
                    HasError = true,
                    Message = result.Message
                };
            }

            return Print(result.Value, null, null, parameters, false, false, true, null);
        }
        public static Data.Result<string> Print(string templateCode, Data.Parameters parameters, bool showDialog)
        {
            Data.Result<Template> result = Template.GetTemplate(templateCode);
            if (result.HasError)
            {
                return new Data.Result<string>()
                {
                    HasError = true,
                    Message = result.Message
                };
            }

            return Print(result.Value, null, null, parameters, showDialog, false, true, null);
        }
        public static Data.Result<string> Print(string templateCode, Data.Parameters parameters, bool showDialog, bool showPreview)
        {
            Data.Result<Template> result = Template.GetTemplate(templateCode);
            if (result.HasError)
            {
                return new Data.Result<string>()
                {
                    HasError = true,
                    Message = result.Message
                };
            }

            return Print(result.Value, null, null, parameters, showDialog, showPreview, true, null);
        }
        public static Data.Result<string> Print(string templateCode, Data.Parameters parameters, bool showDialog, bool showPreview, string printerName)
        {
            Data.Result<Template> result = Template.GetTemplate(templateCode);
            if (result.HasError)
            {
                return new Data.Result<string>()
                {
                    HasError = true,
                    Message = result.Message
                };
            }

            return Print(result.Value, null, null, parameters, showDialog, showPreview, false, printerName);
        }
        public static Data.Result<string> Print(string templateCode)
        {
            Data.Result<Template> result = Template.GetTemplate(templateCode);
            if (result.HasError)
            {
                return new Data.Result<string>()
                {
                    HasError = true,
                    Message = result.Message
                };
            }

            return Print(result.Value, null, null, null, false, false, true, null);
        }
        public static Data.Result<string> Print(string templateCode, bool showDialog)
        {
            Data.Result<Template> result = Template.GetTemplate(templateCode);
            if (result.HasError)
            {
                return new Data.Result<string>()
                {
                    HasError = true,
                    Message = result.Message
                };
            }

            return Print(result.Value, null, null, null, showDialog, false, true, null);
        }
        public static Data.Result<string> Print(string templateCode, bool showDialog, bool showPreview)
        {
            Data.Result<Template> result = Template.GetTemplate(templateCode);
            if (result.HasError)
            {
                return new Data.Result<string>()
                {
                    HasError = true,
                    Message = result.Message
                };
            }

            return Print(result.Value, null, null, null, showDialog, showPreview, true, null);
        }
        public static Data.Result<string> Print(string templateCode, bool showDialog, bool showPreview, string printerName)
        {

            Data.Result<Template> result = Template.GetTemplate(templateCode);
            if (result.HasError)
            {
                return new Data.Result<string>()
                {
                    HasError = true,
                    Message = result.Message
                };
            }

            return Print(result.Value, null, null, null, showDialog, showPreview, false, printerName);
        }

        public static Data.Result<string> Print(string templateCode, System.Data.DataTable dt, Data.Parameters parameters)
        {
            Data.Result<Template> result = Template.GetTemplate(templateCode);
            if (result.HasError)
            {
                return new Data.Result<string>()
                {
                    HasError = true,
                    Message = result.Message
                };
            }
            List<System.Data.DataTable> list = new List<System.Data.DataTable>();
            list.Add(dt);

            return Print(result.Value, list, null, parameters, false, false, true, null);
        }
        public static Data.Result<string> Print(string templateCode, System.Data.DataTable dt, Data.Parameters parameters, bool showDialog)
        {
            Data.Result<Template> result = Template.GetTemplate(templateCode);
            if (result.HasError)
            {
                return new Data.Result<string>()
                {
                    HasError = true,
                    Message = result.Message
                };
            }
            List<System.Data.DataTable> list = new List<System.Data.DataTable>();
            list.Add(dt);

            return Print(result.Value, list, null, parameters, showDialog, false, true, null);
        }
        public static Data.Result<string> Print(string templateCode, System.Data.DataTable dt, Data.Parameters parameters, bool showDialog, bool showPreview)
        {
            Data.Result<Template> result = Template.GetTemplate(templateCode);
            if (result.HasError)
            {
                return new Data.Result<string>()
                {
                    HasError = true,
                    Message = result.Message
                };
            }
            List<System.Data.DataTable> list = new List<System.Data.DataTable>();
            list.Add(dt);

            return Print(result.Value, list, null, parameters, showDialog, showPreview, true, null);
        }
        public static Data.Result<string> Print(string templateCode, System.Data.DataTable dt, Data.Parameters parameters, bool showDialog, bool showPreview, string printerName)
        {
            Data.Result<Template> result = Template.GetTemplate(templateCode);
            if (result.HasError)
            {
                return new Data.Result<string>()
                {
                    HasError = true,
                    Message = result.Message
                };
            }
            List<System.Data.DataTable> list = new List<System.Data.DataTable>();
            list.Add(dt);

            return Print(result.Value, list, null, parameters, showDialog, showPreview, false, printerName);
        }
        public static Data.Result<string> Print(string templateCode, System.Data.DataTable dt)
        {
            Data.Result<Template> result = Template.GetTemplate(templateCode);
            if (result.HasError)
            {
                return new Data.Result<string>()
                {
                    HasError = true,
                    Message = result.Message
                };
            }
            List<System.Data.DataTable> list = new List<System.Data.DataTable>();
            list.Add(dt);

            return Print(result.Value, list, null, null, false, false, true, null);
        }
        public static Data.Result<string> Print(string templateCode, System.Data.DataTable dt, bool showDialog)
        {
            Data.Result<Template> result = Template.GetTemplate(templateCode);
            if (result.HasError)
            {
                return new Data.Result<string>()
                {
                    HasError = true,
                    Message = result.Message
                };
            }
            List<System.Data.DataTable> list = new List<System.Data.DataTable>();
            list.Add(dt);

            return Print(result.Value, list, null, null, showDialog, false, true, null);
        }
        public static Data.Result<string> Print(string templateCode, System.Data.DataTable dt, bool showDialog, bool showPreview)
        {
            Data.Result<Template> result = Template.GetTemplate(templateCode);
            if (result.HasError)
            {
                return new Data.Result<string>()
                {
                    HasError = true,
                    Message = result.Message
                };
            }
            List<System.Data.DataTable> list = new List<System.Data.DataTable>();
            list.Add(dt);

            return Print(result.Value, list, null, null, showDialog, showPreview, true, null);
        }
        public static Data.Result<string> Print(string templateCode, System.Data.DataTable dt, bool showDialog, bool showPreview, string printerName)
        {

            Data.Result<Template> result = Template.GetTemplate(templateCode);
            if (result.HasError)
            {
                return new Data.Result<string>()
                {
                    HasError = true,
                    Message = result.Message
                };
            }
            List<System.Data.DataTable> list = new List<System.Data.DataTable>();
            list.Add(dt);

            return Print(result.Value, list, null, null, showDialog, showPreview, false, printerName);
        }

        public static Data.Result<string> Print(string templateCode, System.Data.DataSet set, Data.Parameters parameters)
        {
            Data.Result<Template> result = Template.GetTemplate(templateCode);
            if (result.HasError)
            {
                return new Data.Result<string>()
                {
                    HasError = true,
                    Message = result.Message
                };
            }
            List<System.Data.DataSet> list = new List<System.Data.DataSet>();
            list.Add(set);

            return Print(result.Value, null, list, parameters, false, false, true, null);
        }
        public static Data.Result<string> Print(string templateCode, System.Data.DataSet set, Data.Parameters parameters, bool showDialog)
        {
            Data.Result<Template> result = Template.GetTemplate(templateCode);
            if (result.HasError)
            {
                return new Data.Result<string>()
                {
                    HasError = true,
                    Message = result.Message
                };
            }
            List<System.Data.DataSet> list = new List<System.Data.DataSet>();
            list.Add(set);

            return Print(result.Value, null, list, parameters, showDialog, false, true, null);
        }
        public static Data.Result<string> Print(string templateCode, System.Data.DataSet set, Data.Parameters parameters, bool showDialog, bool showPreview)
        {
            Data.Result<Template> result = Template.GetTemplate(templateCode);
            if (result.HasError)
            {
                return new Data.Result<string>()
                {
                    HasError = true,
                    Message = result.Message
                };
            }
            List<System.Data.DataSet> list = new List<System.Data.DataSet>();
            list.Add(set);

            return Print(result.Value, null, list, parameters, showDialog, showPreview, true, null);
        }
        public static Data.Result<string> Print(string templateCode, System.Data.DataSet set, Data.Parameters parameters, bool showDialog, bool showPreview, string printerName)
        {
            Data.Result<Template> result = Template.GetTemplate(templateCode);
            if (result.HasError)
            {
                return new Data.Result<string>()
                {
                    HasError = true,
                    Message = result.Message
                };
            }
            List<System.Data.DataSet> list = new List<System.Data.DataSet>();
            list.Add(set);

            return Print(result.Value, null, list, parameters, showDialog, showPreview, false, printerName);
        }
        public static Data.Result<string> Print(string templateCode, System.Data.DataSet set)
        {
            Data.Result<Template> result = Template.GetTemplate(templateCode);
            if (result.HasError)
            {
                return new Data.Result<string>()
                {
                    HasError = true,
                    Message = result.Message
                };
            }
            List<System.Data.DataSet> list = new List<System.Data.DataSet>();
            list.Add(set);

            return Print(result.Value, null, list, null, false, false, true, null);
        }
        public static Data.Result<string> Print(string templateCode, System.Data.DataSet set, bool showDialog)
        {
            Data.Result<Template> result = Template.GetTemplate(templateCode);
            if (result.HasError)
            {
                return new Data.Result<string>()
                {
                    HasError = true,
                    Message = result.Message
                };
            }
            List<System.Data.DataSet> list = new List<System.Data.DataSet>();
            list.Add(set);

            return Print(result.Value, null, list, null, showDialog, false, true, null);
        }
        public static Data.Result<string> Print(string templateCode, System.Data.DataSet set, bool showDialog, bool showPreview)
        {
            Data.Result<Template> result = Template.GetTemplate(templateCode);
            if (result.HasError)
            {
                return new Data.Result<string>()
                {
                    HasError = true,
                    Message = result.Message
                };
            }
            List<System.Data.DataSet> list = new List<System.Data.DataSet>();
            list.Add(set);

            return Print(result.Value, null, list, null, showDialog, showPreview, true, null);
        }
        public static Data.Result<string> Print(string templateCode, System.Data.DataSet set, bool showDialog, bool showPreview, string printerName)
        {

            Data.Result<Template> result = Template.GetTemplate(templateCode);
            if (result.HasError)
            {
                return new Data.Result<string>()
                {
                    HasError = true,
                    Message = result.Message
                };
            }
            List<System.Data.DataSet> list = new List<System.Data.DataSet>();
            list.Add(set);

            return Print(result.Value, null, list, null, showDialog, showPreview, false, printerName);
        }

        public static Data.Result<string> Print(string templateCode, List<System.Data.DataTable> dts, Data.Parameters parameters)
        {
            Data.Result<Template> result = Template.GetTemplate(templateCode);
            if (result.HasError)
            {
                return new Data.Result<string>()
                {
                    HasError = true,
                    Message = result.Message
                };
            }

            return Print(result.Value, dts, null, parameters, false, false, true, null);
        }
        public static Data.Result<string> Print(string templateCode, List<System.Data.DataTable> dts, Data.Parameters parameters, bool showDialog)
        {
            Data.Result<Template> result = Template.GetTemplate(templateCode);
            if (result.HasError)
            {
                return new Data.Result<string>()
                {
                    HasError = true,
                    Message = result.Message
                };
            }

            return Print(result.Value, dts, null, parameters, showDialog, false, true, null);
        }
        public static Data.Result<string> Print(string templateCode, List<System.Data.DataTable> dts, Data.Parameters parameters, bool showDialog, bool showPreview)
        {
            Data.Result<Template> result = Template.GetTemplate(templateCode);
            if (result.HasError)
            {
                return new Data.Result<string>()
                {
                    HasError = true,
                    Message = result.Message
                };
            }

            return Print(result.Value, dts, null, parameters, showDialog, showPreview, true, null);
        }
        public static Data.Result<string> Print(string templateCode, List<System.Data.DataTable> dts, Data.Parameters parameters, bool showDialog, bool showPreview, string printerName)
        {
            Data.Result<Template> result = Template.GetTemplate(templateCode);
            if (result.HasError)
            {
                return new Data.Result<string>()
                {
                    HasError = true,
                    Message = result.Message
                };
            }

            return Print(result.Value, dts, null, parameters, showDialog, showPreview, false, printerName);
        }
        public static Data.Result<string> Print(string templateCode, List<System.Data.DataTable> dts)
        {
            Data.Result<Template> result = Template.GetTemplate(templateCode);
            if (result.HasError)
            {
                return new Data.Result<string>()
                {
                    HasError = true,
                    Message = result.Message
                };
            }

            return Print(result.Value, dts, null, null, false, false, true, null);
        }
        public static Data.Result<string> Print(string templateCode, List<System.Data.DataTable> dts, bool showDialog)
        {
            Data.Result<Template> result = Template.GetTemplate(templateCode);
            if (result.HasError)
            {
                return new Data.Result<string>()
                {
                    HasError = true,
                    Message = result.Message
                };
            }

            return Print(result.Value, dts, null, null, showDialog, false, true, null);
        }
        public static Data.Result<string> Print(string templateCode, List<System.Data.DataTable> dts, bool showDialog, bool showPreview)
        {
            Data.Result<Template> result = Template.GetTemplate(templateCode);
            if (result.HasError)
            {
                return new Data.Result<string>()
                {
                    HasError = true,
                    Message = result.Message
                };
            }

            return Print(result.Value, dts, null, null, showDialog, showPreview, true, null);
        }
        public static Data.Result<string> Print(string templateCode, List<System.Data.DataTable> dts, bool showDialog, bool showPreview, string printerName)
        {

            Data.Result<Template> result = Template.GetTemplate(templateCode);
            if (result.HasError)
            {
                return new Data.Result<string>()
                {
                    HasError = true,
                    Message = result.Message
                };
            }

            return Print(result.Value, dts, null, null, showDialog, showPreview, false, printerName);
        }

        public static Data.Result<string> Print(string templateCode, List<System.Data.DataSet> sets, Data.Parameters parameters)
        {
            Data.Result<Template> result = Template.GetTemplate(templateCode);
            if (result.HasError)
            {
                return new Data.Result<string>()
                {
                    HasError = true,
                    Message = result.Message
                };
            }

            return Print(result.Value, null, sets, parameters, false, false, true, null);
        }
        public static Data.Result<string> Print(string templateCode, List<System.Data.DataSet> sets, Data.Parameters parameters, bool showDialog)
        {
            Data.Result<Template> result = Template.GetTemplate(templateCode);
            if (result.HasError)
            {
                return new Data.Result<string>()
                {
                    HasError = true,
                    Message = result.Message
                };
            }

            return Print(result.Value, null, sets, parameters, showDialog, false, true, null);
        }
        public static Data.Result<string> Print(string templateCode, List<System.Data.DataSet> sets, Data.Parameters parameters, bool showDialog, bool showPreview)
        {
            Data.Result<Template> result = Template.GetTemplate(templateCode);
            if (result.HasError)
            {
                return new Data.Result<string>()
                {
                    HasError = true,
                    Message = result.Message
                };
            }

            return Print(result.Value, null, sets, parameters, showDialog, showPreview, true, null);
        }
        public static Data.Result<string> Print(string templateCode, List<System.Data.DataSet> sets, Data.Parameters parameters, bool showDialog, bool showPreview, string printerName)
        {
            Data.Result<Template> result = Template.GetTemplate(templateCode);
            if (result.HasError)
            {
                return new Data.Result<string>()
                {
                    HasError = true,
                    Message = result.Message
                };
            }

            return Print(result.Value, null, sets, parameters, showDialog, showPreview, false, printerName);
        }
        public static Data.Result<string> Print(string templateCode, List<System.Data.DataSet> sets)
        {
            Data.Result<Template> result = Template.GetTemplate(templateCode);
            if (result.HasError)
            {
                return new Data.Result<string>()
                {
                    HasError = true,
                    Message = result.Message
                };
            }

            return Print(result.Value, null, sets, null, false, false, true, null);
        }
        public static Data.Result<string> Print(string templateCode, List<System.Data.DataSet> sets, bool showDialog)
        {
            Data.Result<Template> result = Template.GetTemplate(templateCode);
            if (result.HasError)
            {
                return new Data.Result<string>()
                {
                    HasError = true,
                    Message = result.Message
                };
            }

            return Print(result.Value, null, sets, null, showDialog, false, true, null);
        }
        public static Data.Result<string> Print(string templateCode, List<System.Data.DataSet> sets, bool showDialog, bool showPreview)
        {
            Data.Result<Template> result = Template.GetTemplate(templateCode);
            if (result.HasError)
            {
                return new Data.Result<string>()
                {
                    HasError = true,
                    Message = result.Message
                };
            }

            return Print(result.Value, null, sets, null, showDialog, showPreview, true, null);
        }
        public static Data.Result<string> Print(string templateCode, List<System.Data.DataSet> sets, bool showDialog, bool showPreview, string printerName)
        {

            Data.Result<Template> result = Template.GetTemplate(templateCode);
            if (result.HasError)
            {
                return new Data.Result<string>()
                {
                    HasError = true,
                    Message = result.Message
                };
            }

            return Print(result.Value, null, sets, null, showDialog, showPreview, false, printerName);
        }

        public static Data.Result<string> Print(string templateCode, List<System.Data.DataTable> dts, List<System.Data.DataSet> sets, Data.Parameters parameters)
        {
            Data.Result<Template> result = Template.GetTemplate(templateCode);
            if (result.HasError)
            {
                return new Data.Result<string>()
                {
                    HasError = true,
                    Message = result.Message
                };
            }

            return Print(result.Value, dts, sets, parameters, false, false, true, null);
        }
        public static Data.Result<string> Print(string templateCode, List<System.Data.DataTable> dts, List<System.Data.DataSet> sets, Data.Parameters parameters, bool showDialog)
        {
            Data.Result<Template> result = Template.GetTemplate(templateCode);
            if (result.HasError)
            {
                return new Data.Result<string>()
                {
                    HasError = true,
                    Message = result.Message
                };
            }

            return Print(result.Value, dts, sets, parameters, showDialog, false, true, null);
        }
        public static Data.Result<string> Print(string templateCode, List<System.Data.DataTable> dts, List<System.Data.DataSet> sets, Data.Parameters parameters, bool showDialog, bool showPreview)
        {
            Data.Result<Template> result = Template.GetTemplate(templateCode);
            if (result.HasError)
            {
                return new Data.Result<string>()
                {
                    HasError = true,
                    Message = result.Message
                };
            }

            return Print(result.Value, dts, sets, parameters, showDialog, showPreview, true, null);
        }
        public static Data.Result<string> Print(string templateCode, List<System.Data.DataTable> dts, List<System.Data.DataSet> sets, Data.Parameters parameters, bool showDialog, bool showPreview, string printerName)
        {
            Data.Result<Template> result = Template.GetTemplate(templateCode);
            if (result.HasError)
            {
                return new Data.Result<string>()
                {
                    HasError = true,
                    Message = result.Message
                };
            }

            return Print(result.Value, dts, sets, parameters, showDialog, showPreview, false, printerName);
        }
        public static Data.Result<string> Print(string templateCode, List<System.Data.DataTable> dts, List<System.Data.DataSet> sets)
        {
            Data.Result<Template> result = Template.GetTemplate(templateCode);
            if (result.HasError)
            {
                return new Data.Result<string>()
                {
                    HasError = true,
                    Message = result.Message
                };
            }

            return Print(result.Value, dts, sets, null, false, false, true, null);
        }
        public static Data.Result<string> Print(string templateCode, List<System.Data.DataTable> dts, List<System.Data.DataSet> sets, bool showDialog)
        {
            Data.Result<Template> result = Template.GetTemplate(templateCode);
            if (result.HasError)
            {
                return new Data.Result<string>()
                {
                    HasError = true,
                    Message = result.Message
                };
            }

            return Print(result.Value, dts, sets, null, showDialog, false, true, null);
        }
        public static Data.Result<string> Print(string templateCode, List<System.Data.DataTable> dts, List<System.Data.DataSet> sets, bool showDialog, bool showPreview)
        {
            Data.Result<Template> result = Template.GetTemplate(templateCode);
            if (result.HasError)
            {
                return new Data.Result<string>()
                {
                    HasError = true,
                    Message = result.Message
                };
            }

            return Print(result.Value, dts, sets, null, showDialog, showPreview, true, null);
        }
        public static Data.Result<string> Print(string templateCode, List<System.Data.DataTable> dts, List<System.Data.DataSet> sets, bool showDialog, bool showPreview, string printerName)
        {

            Data.Result<Template> result = Template.GetTemplate(templateCode);
            if (result.HasError)
            {
                return new Data.Result<string>()
                {
                    HasError = true,
                    Message = result.Message
                };
            }

            return Print(result.Value, dts, sets, null, showDialog, showPreview, false, printerName);
        }
    }

    internal class Template
    {
        public long TemplateId { get; set; }
        public string TemplateCode { get; set; }
        public string TemplateDesc { get; set; }
        public string Source { get; set; }

        public static Data.Result<Template> GetTemplate(string templateCode)
        {
            Data.Result<Template> result = null;
            string sql = "SELECT TemplateId,TemplateCode,TemplateDesc,TemplateType,Source FROM dbo.Bas_Template WITH(NOLOCK) WHERE TemplateCode=@TemplateCode";
            Data.Parameters parameters = new Data.Parameters();
            parameters.Add("TemplateCode", templateCode);
            System.Data.DataTable dt = null;
            try
            {
                dt=DB.DBHelper.GetDataTable(sql, parameters, null, false);
            }
            catch(Exception e)
            {
                result = new Data.Result<Template>();
                result.HasError = true;
                result.Message = e.Message;
                return result;
            }

            if (dt.Rows.Count < 1)
            {
                result = new Data.Result<Template>();
                result.HasError = true;
                result.Message = string.Format("模板[{0}]不存在。", templateCode);
                return result;
            }

            Template template = new Template();
            template.TemplateId = dt.Rows[0].Field<long>("TemplateId");
            template.TemplateCode = dt.Rows[0].Field<string>("TemplateCode");
            template.TemplateDesc = dt.Rows[0].Field<string>("TemplateDesc");
            template.Source = dt.Rows[0].Field<string>("Source");

            result = new Data.Result<Template>();
            result.HasError = false;
            result.Value = template;
            return result;
        }
    }
}
