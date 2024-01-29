using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MaterialSN.CodeSoft
{
    /// <summary>
    /// 对codesoft 打印软件的简单封装(ActiveX控件的封装)
    /// </summary>
    public class CodeSoftFacade
    {
        private string _printerName = string.Empty;
        private string _templatePath = string.Empty;
        LabelManager2.IApplication _app;
        LabelManager2.Document _doc;

        //为测试时系统中没有安装CodeSoft使用
        private static string NoCodeSoft = System.Configuration.ConfigurationSettings.AppSettings["NoCodeSoft"];

        public CodeSoftFacade(string printername, string path)
            : this()
        {
            if (NoCodeSoft != "1")
                OpenTemplate(printername, path);
        }

        public void OpenTemplate(string printername, string path)
        {

            if (NoCodeSoft != "1")
            {
                if (this._templatePath != path)
                {
                    this._printerName = printername;
                    this._templatePath = path;

                    if (this._templatePath == null || this._templatePath == string.Empty)
                    {
                        throw new Exception("$ERROR_Label_template_empty");
                    }

                    if (_app == null)
                        throw new Exception("打开CodeSoft程序出错!");

                    /// 设定标签模板文件路径
                    _doc = _app.Documents.Open(_templatePath, true);

                    if (_doc == null)
                    {
                        throw new Exception("$ERROR_Label_Open");
                    }

                    /// 设定标签打印时用的打印机
                    if (this._printerName != null && this._printerName != string.Empty)
                    {
                        if (_doc.Printer == null)
                            throw new Exception("设定打印机出错 !");

                        _doc.Printer.SwitchTo(this._printerName, string.Empty, false);

                    }
                }
            }




        }

        public void OpenTemplateMaterialLot(string printername, string fileName, string varName)
        {
            if (NoCodeSoft != "1")
            {
                string path = string.Empty;
                path = System.Environment.CurrentDirectory + "\\" + fileName;

                if (this._templatePath != path)
                {
                    this._printerName = printername;
                    this._templatePath = path;

                    if (string.IsNullOrEmpty(varName))
                    {
                        throw new Exception("$Print_VarName_ISempty");
                    }

                    if (this._templatePath == null || this._templatePath == string.Empty)
                    {
                        throw new Exception("$ERROR_Label_template_empty");
                    }

                    if (_app == null)
                        throw new Exception("打开CodeSoft程序出错!");

                    /// 设定标签模板文件路径
                    _doc = _app.Documents.Open(_templatePath, true);

                    if (_doc == null)
                    {
                        throw new Exception("打印模板打开出错：" + fileName);

                    }

                    /// 设定标签打印时用的打印机
                    if (this._printerName != null && this._printerName != string.Empty)
                    {
                        if (_doc.Printer == null)
                            throw new Exception("设定打印机出错 !");

                        _doc.Printer.SwitchTo(this._printerName, string.Empty, false);

                    }
                }
            }
        }
        public CodeSoftFacade()
        {
            if (NoCodeSoft != "1")
            {
                _app = new LabelManager2.Application();

                if (_app == null)
                    throw new Exception("打开CodeSoft程序出错!");

                _app.Visible = false;
            }
        }

        ~CodeSoftFacade()
        {
            this.ReleaseCom();
        }
        //added by leon yuan 2008/05/28, 允许一次打印多张标签
        private LabelPrintVars _labelPrintVars = new LabelPrintVars();
        public LabelPrintVars LabelPrintVars
        {
            get
            {
                return _labelPrintVars;
            }
            set
            {
                _labelPrintVars = value;
            }
        }

        //将相应的variable传给模板文件进行打印
        public void Print(string[] vars)
        {
            try
            {
                if (NoCodeSoft != "1")
                {
                    if (_doc == null)
                    {
                        throw new Exception("$ERROR_Label_Open 2");
                    }

                    if (_doc.Variables == null)
                        throw new Exception("打开模板变量出错 1!");

                    if (vars == null)
                        throw new Exception("程序传递参数出错 2!");

                    if (_doc.Variables.Count < vars.Length)
                        throw new Exception("$ERROR_Lable_Vars_Count");

                    if (vars != null && vars.Length > 0)
                    {
                        for (int i = 0; i < vars.Length; i++)
                        {
                            LabelManager2.Variable var = (LabelManager2.Variable)_doc.Variables.Item("var" + i.ToString());

                            if (var == null)
                                throw new Exception("打开模板变量出错 3! " + i.ToString());

                            var.Value = vars[i];
                        }

                        //added by leon yuan 2008/05/28, 允许一次打印多张标签
                        //process #3
                        for (int i = 0; i < _labelPrintVars.LabelVars_No3.Length; i++)
                        {
                            LabelManager2.Variable var = (LabelManager2.Variable)_doc.Variables.Item(_labelPrintVars.LabelVars_No3[i]);

                            if (_labelPrintVars.LabelValues_No3[i].Trim().Length > 0)
                            {

                                if (var == null)
                                    throw new Exception("打开模板变量出错 4! " + i.ToString());

                                var.Value = _labelPrintVars.LabelValues_No3[i].Trim();
                            }
                            else
                            {
                                if (var != null)
                                {

                                    var.Value = "";
                                }
                            }
                        }
                        //Process #2
                        for (int i = 0; i < _labelPrintVars.LabelVars_No2.Length; i++)
                        {
                            LabelManager2.Variable var = (LabelManager2.Variable)_doc.Variables.Item(_labelPrintVars.LabelVars_No2[i]);

                            if (_labelPrintVars.LabelValues_No2[i].Trim().Length > 0)
                            {
                                if (var == null)
                                    throw new Exception("打开模板变量出错 4! " + i.ToString());

                                var.Value = _labelPrintVars.LabelValues_No2[i].Trim();
                            }
                            else
                            {
                                if (var != null)
                                {

                                    var.Value = "";
                                }
                            }
                        }

                        _doc.PrintDocument(1);
                    }
                }
            }
            finally
            {
                ReleaseCom();
            }
        }


        //bighai 20090306
        public void Print(string[] vars, string varName, string fileName)
        {


            if (NoCodeSoft != "1")
            {
                if (_doc == null)
                {
                    throw new Exception("打印模板打开出错：" + fileName);
                }

                if (_doc.Variables == null)
                    //throw new Exception("打开模板变量出错 !");
                    throw new Exception("打开模板变量出错：" + varName);

                if (vars == null)
                    throw new Exception("程序传递参数出错 !");

                if (_doc.Variables.Count < vars.Length)
                    throw new Exception("$ERROR_Lable_Vars_Count");

                if (vars != null && vars.Length > 0)
                {
                    for (int i = 0; i < vars.Length; i++)
                    {
                        LabelManager2.Variable var = (LabelManager2.Variable)_doc.Variables.Item(varName);

                        if (var == null)
                            throw new Exception("打开模板变量出错：" + varName);

                        var.Value = vars[i];

                    }

                    _doc.PrintDocument(1);

                }
            }
        }

        public void Print(string strDataFile, string strDescFile)
        {
            try
            {
                if (NoCodeSoft != "1")
                {
                    if (_doc == null)
                    {
                        throw new Exception("$ERROR_Label_Open 2");
                    }

                    if (_doc.Variables == null)
                        throw new Exception("打开模板变量出错 !");

                    if (!System.IO.File.Exists(strDescFile))
                        throw new Exception("描述文件不存在 !");

                    if (!System.IO.File.Exists(strDataFile))
                        throw new Exception("数据文件不存在 !");

                    _doc.Database.OpenASCII(strDataFile, strDescFile);

                    _doc.Merge(1, 1, 1, 1, 1, "");

                }
            }
            finally
            {
                ReleaseCom();
            }
        }

        /// <summary>
        /// 释放对code soft activeX 控件的引用
        /// </summary>
        public void ReleaseCom()
        {
            if (_app != null)
            {
                try
                {
                    _app.Quit();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(_app);
                    _app = null;
                }
                catch
                {

                }
            }
        }
    }
}
