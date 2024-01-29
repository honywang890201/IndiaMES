using LabelManager2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Printer
{
    /// <summary>
    /// CodeSoft打印
    /// </summary>
    public class CodeSoft : Printer.Instance
    {
        /// <summary>
        /// CodeSoft应用程序
        /// </summary>
        private ApplicationClass _App = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="PrintTemplate">模板路径</param>
        /// <param name="PrinterName">默认打印机</param>
        public CodeSoft(string PrintTemplate, string PrinterName = null)
        {
            _PrintType = PrintType.CodeSoft;
            if (string.IsNullOrEmpty(PrintTemplate))
            {
                throw (new Exception("没有指定模板"));
            }
            if (!System.IO.File.Exists(PrintTemplate))
            {
                throw (new Exception(string.Format("模板[{0}]不存在", PrintTemplate)));
            }
            _PrintTemplate = PrintTemplate;
            _PrinterName = PrinterName;

            _App = new ApplicationClass();
            _App.Documents.Open(PrintTemplate, true);

            if(_App!=null)
            {
                _Params = new List<string>();
                Document document = _App.ActiveDocument;
                for (int i = 1; i <= document.Variables.FormVariables.Count; i++)
                {
                    string paramName = document.Variables.FormVariables.Item(i).Name;
                    _Params.Add(paramName);
                }
            }

            if (string.IsNullOrEmpty(_PrinterName))
            {
                _PrinterName = WinAPI.Computer.LocalPrinterName;//设为默认打印机
            }

            if (!string.IsNullOrEmpty(_PrinterName))
            {
                _App.ActiveDocument.Printer.SwitchTo(_PrinterName);
                _App.ActiveDocument.FormFeed();
            }
            GetListParam();
        }

        /// <summary>
        /// 关闭
        /// </summary>
        public override void Close()
        {
            try
            {
                if (_App != null)
                {
                    _App.Documents.CloseAll(false);
                    _App.Quit();
                    WinAPI.SysFunction.KillProcessByName("lppa");
                }
            }
            catch { }
        }

        /// <summary>
        /// 设置参数的值
        /// </summary>
        /// <param name="paramName">参数名</param>
        /// <param name="paramValue">参数值</param>
        public override void SetParamValue(string paramName, string paramValue)
        {
            if (_App == null || paramName == null)
                return;

            if (!Params.Contains(paramName))
            {
                return;
            }

            if (ParamsValue.ContainsKey(paramName))
            {
                ParamsValue[paramName] = paramValue;
            }
            else
            {
                ParamsValue.Add(paramName, paramValue);
            }

            Document document = _App.ActiveDocument;
            try
            {
                document.Variables.FormVariables.Item(paramName).Value = paramValue ?? string.Empty;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// 打印
        /// </summary>
        /// <param name="number">打印份数</param>
        public override void Print(int number = 1)
        {
            if (_App == null)
                return;
            Document document = _App.ActiveDocument;
            document.PrintDocument(number);  //打印
            document.Save();
        }

        public override void SetPrinter(string printerName)
        {
            PrinterName = printerName;
            if (string.IsNullOrEmpty(printerName))
            {
                PrinterName = WinAPI.Computer.LocalPrinterName;//设为默认打印机
            }

            if (!string.IsNullOrEmpty(PrinterName))
            {
                if (_App != null && _App.ActiveDocument != null)
                {
                    _App.ActiveDocument.Printer.SwitchTo(PrinterName);
                    _App.ActiveDocument.FormFeed();
                }
            }
        }

    }
}
