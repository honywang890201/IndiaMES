using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Printer
{
    /// <summary>
    /// Bartender打印
    /// </summary>
    public class Bartender : Printer.Instance
    {

        /// <summary>
        /// Bartender应用程序
        /// </summary>
        private BarTender.Application _App = null;
        private BarTender.Format _Format = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="PrintTemplate">模板路径</param>
        /// <param name="PrinterName">默认打印机</param>
        public Bartender(string PrintTemplate, string PrinterName = null)
        {
            _PrintType = PrintType.Bartender;
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


            _App = new BarTender.Application();
            _Format = _App.Formats.Open(PrintTemplate, false, PrinterName);
            _App.Visible = false;
            _Format.PrintSetup.NumberSerializedLabels = 1;//序列标签数,如果设置大于1，则于流水号加1依次打印出来
        

            if(_App!=null)
            {
                _Params = new List<string>();
                for (int i = 0; i < _Format.NamedSubStrings.Count; i++)
                {
                    string paramName = _Format.NamedSubStrings.GetSubString(i + 1).Name;
                    _Params.Add(paramName);
                }
            }

            if (string.IsNullOrEmpty(_PrinterName))
            {
                _PrinterName = WinAPI.Computer.LocalPrinterName;//设为默认打印机
            }

            GetListParam();
        }

        public override void Close()
        {
            if (_Format != null)
            {
                try
                {
                    _Format.Close(BarTender.BtSaveOptions.btDoNotSaveChanges);
                }
                catch { }
            }
            if (_App != null)
            {
                try
                {
                    _App.Quit(BarTender.BtSaveOptions.btDoNotSaveChanges);
                }
                catch { }
            }
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
                if (_Format != null)
                {
                    _Format.Printer = PrinterName;
                }
            }
        }

        public override void Print(int number = 1)
        {
            for (int i = 0; i < number; i++)
            {
                _Format.PrintOut(false,false);//第2个参数是 是否显示打印机属性的。可以设置打印机路径
            }
        }

        public override void SetParamValue(string paramName, string paramValue)
        {
            if (_App == null || _Format == null)
                return;

            if (!Params.Contains(paramName))
            {
                return;
            }

            try
            {
                _Format.SetNamedSubStringValue(paramName, paramValue ?? string.Empty);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
