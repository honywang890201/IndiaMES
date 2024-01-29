using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using WinAPI;

namespace Printer
{
    /// <summary>
    /// 脚本打印
    /// </summary>
    class Script : Printer.Instance
    {
        private string AfterAnalysisScriptText = null;
        private int portType = -1;
        private SerialPort comPort = null;
        private WriteLPT lptPort = null;

        /// <summary>
        /// 所有函数
        /// </summary>
        private List<string> Functions = null;
        /// <summary>
        /// 解析函数的Sql语句
        /// </summary>
        private string AnalysisFunctionSql = null;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="PrintTemplate">脚本文件路径</param>
        /// <param name="PortName">端口</param>
        /// <param name="BaudRate">波特率</param>
        public void Init(string PrintTemplate, string PortName, int BaudRate)
        {
            _PrintType = PrintType.Script;

            if (string.IsNullOrEmpty(PrintTemplate))
            {
                throw (new Exception("没有指定模板"));
            }
            if (!System.IO.File.Exists(PrintTemplate))
            {
                throw (new Exception(string.Format("模板[{0}]不存在", PrintTemplate)));
            }
            if (string.IsNullOrEmpty(PortName))
            {
                throw (new Exception("没有指定端口"));
            }

            PrintTemplate = WinAPI.File.TXTFileHelper.Read(PrintTemplate);

            this._PrintTemplate = PrintTemplate;
            this._PortName = PortName;
            this._BaudRate = BaudRate;
            if (_PortName.ToUpper().StartsWith("COM"))//串口
            {
                Close();
                comPort = new SerialPort();
                comPort.PortName = _PortName;
                comPort.BaudRate = _BaudRate.Value;
                comPort.DataBits = 8;
                comPort.StopBits = StopBits.One;
                comPort.Parity = Parity.None;
                comPort.Open();
                GetAllParamNames();
                GetListParam();
                portType = 0;
            }
            else if (_PortName.ToUpper().StartsWith("LPT"))//并口
            {
                Close();
                lptPort = new WriteLPT();
                lptPort.OpenPort(_PortName, _BaudRate.Value, 0, 8, 1);
                GetAllParamNames();
                GetListParam();
                portType = 1;
            }
            else
            {
                if (Params != null)
                {
                    Params.Clear();
                }
                if (Functions != null)
                {
                    Functions.Clear();
                }
                ListParams.Clear();
                portType = -1;
                throw new Exception(string.Format("不支持的端口类型:{0}", PortName));
            }
        }

        /// <summary>
        /// 获取所有参数
        /// </summary>
        private void GetAllParamNames()
        {
            AnalysisFunctionSql = null;
            if (_Params != null)
            {
                _Params.Clear();
            }
            else
            {
                _Params = new List<string>();
            }
            if (Functions != null)
            {
                Functions.Clear();
            }
            else
            {
                Functions = new List<string>();
            }

            if (_PrintTemplate == null)
                return;

            string s;
            char ch;
            int index;

            #region 提取出脚本中的所有变量
            s = _PrintTemplate;
            index = s.IndexOf("$");
            while (index >= 0 && index < s.Length - 2)
            {
                s = s.Substring(index + 1);
                index = s.IndexOf("$");
                if (index < 0)
                {
                    break;
                }
                if (index == 0)
                {
                    continue;
                }
                else
                {
                    string p = string.Format("${0}$", s.Substring(0, index));
                    if (p.Contains('\r') || p.Contains('\n'))//不能包含回车换行
                    {
                        continue;
                    }
                    if (!Params.Contains(p))
                    {
                        Params.Add(p);
                    }
                    if (index >= s.Length - 2)
                    {
                        break;
                    }
                    else
                    {
                        s = s.Substring(index + 1);
                        index = s.IndexOf("$");
                    }
                }
            }
            #endregion

            #region 提取脚本中的所有函数(函数必须以&开头)
            s = _PrintTemplate;
            index = s.IndexOf("&");
            while (index >= 0 && index < s.Length - 1)
            {
                s = s.Substring(index + 1);
                index = s.IndexOf("(");
                if (index <= 0 || index > s.Length - 2)
                {
                    break;
                }
                bool IsExistsFunctionName = true;//是否存在函数名

                #region 从左括号从左移，确定正确的函数名
                for (int i = index - 1; i >= 0; i--)
                {
                    ch = s.ToCharArray()[i];
                    if (ch == '&')
                    {
                        if (i == index - 1)
                        {
                            IsExistsFunctionName = false;
                        }
                        else
                        {
                            s = s.Substring(i + 1);
                        }
                        break;
                    }
                    if ((ch >= '0' && ch <= '9') || (ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z') || (ch == '_') || (ch == '.') || (ch == '-') || (ch == '+')
                        || (ch == '[') || (ch == ']') || (ch == '~') || (ch == '!') || (ch == '#'))
                    {
                        continue;
                    }
                    else
                    {
                        IsExistsFunctionName = false;
                        break;
                    }
                }
                #endregion

                if (!IsExistsFunctionName)
                {
                    s = s.Substring(index + 1);
                    index = s.IndexOf("&");
                    continue;
                }
                else
                {
                    index = s.IndexOf("(");
                }

                //往后移，直到函数结尾,有可能嵌套函数
                //当左括号出现的个数=右括号出现的个数时,表示到达函数结尾处(要排除引号''中的括号)
                int lc = 1; //左括号(出现的个数
                int rc = 0;//右括号)出现的个数
                bool IsHaveLeftQuotation = false;//有左引号

                #region 获取函数
                for (int i = index + 1; i < s.Length; i++)
                {
                    ch = s.ToCharArray()[i];
                    if (ch == '\'')
                    {
                        if (IsHaveLeftQuotation)
                        {
                            IsHaveLeftQuotation = false;
                        }
                        else
                        {
                            IsHaveLeftQuotation = true;
                        }
                        continue;
                    }

                    if (IsHaveLeftQuotation)
                    {
                        continue;
                    }
                    else if (ch == '\r' || ch == '\n')
                    {
                        break;
                    }
                    else if (ch == '(')
                    {
                        lc++;
                    }
                    else if (ch == ')')
                    {
                        rc++;
                        if (rc == lc)
                        {
                            string fun = string.Format("&{0}", s.Substring(0, i + 1));
                            Functions.Add(fun);
                            index = i;
                            break;
                        }
                    }
                }
                #endregion

                if (index < s.Length - 1)
                {
                    s = s.Substring(index + 1);
                    index = s.IndexOf("&");
                }
                else
                {
                    break;
                }
            }
            #endregion

            ConstructAnalysisFunctionSql();
        }

        /// <summary>
        /// 构造解析函数的Sql语句
        /// </summary>
        private void ConstructAnalysisFunctionSql()
        {
            AnalysisFunctionSql = null;
            if (Functions == null || Functions.Count < 1)
            {
                return;
            }

            string sqlTop = "DECLARE @Sql NVARCHAR(MAX)";
            string sqlCenter = string.Empty;
            string sqlBottom = string.Empty;

            for (int i = 0; i < Functions.Count; i++)
            {
                sqlTop = sqlTop + "\r\n";
                sqlCenter = sqlCenter + "\r\n\r\n";

                sqlTop = sqlTop + string.Format("DECLARE @c{0} NVARCHAR(MAX)", i);
                sqlCenter = sqlCenter + string.Format(@"
BEGIN TRY
	SET @Sql='SET @c{0}={1}'
	EXEC sys.sp_executesql @Sql,N'@c{0} NVARCHAR(MAX) OUTPUT',@c{0} OUTPUT
END TRY
BEGIN CATCH
	SET @c{0}=''
END CATCH 
", i, Functions[i].Substring(1).Replace("'", "''"));
                if (sqlBottom == string.Empty)
                {
                    sqlBottom = string.Format("\r\n\r\nSELECT @c{0} AS c{0}", i);
                }
                else
                {
                    sqlBottom = sqlBottom + string.Format(",@c{0} AS c{0}", i);
                }
            }
            AnalysisFunctionSql = sqlTop + sqlCenter + sqlBottom;
        }

        public override void Close()
        {
            if (portType == 0)
            {
                if (comPort != null)
                {
                    try { comPort.Close(); }
                    catch { }
                }
            }
            else if (portType == 1)
            {
                if (lptPort != null)
                {
                    try { lptPort.ClosePort(); }
                    catch { }
                }
            }
        }

        public override void Print(int number = 1)
        {
            AfterAnalysisScriptText = _PrintTemplate;
            if (string.IsNullOrEmpty(AfterAnalysisScriptText))
                return;

            //解析函数
            AnalysisScriptFunction();

            //赋值
            foreach (string p in Params)
            {
                string v = string.Empty;
                if (ParamsValue.ContainsKey(p))
                {
                    v = ParamsValue[p];
                }
                AfterAnalysisScriptText = AfterAnalysisScriptText.Replace(p, v);
            }

            if (number < 1)
            {
                number = 1;
            }

            for (int i = 0; i < number; i++)
            {
                if (portType == 0)
                {
                    comPort.Write(AfterAnalysisScriptText);
                }
                else if (portType == 1)
                {
                    //lptPort
                    //lptPort.WritePort(AfterAnalysisScriptText);
                    //lptPort.CleanPortData();
                    // lptPort.CleanPortData();

                    //LPT每次打印的是上一次扫描的,改为每次都重新打开端口
                    if (!lptPort.Opened)
                    {
                        lptPort.OpenPort(PortName, BaudRate.Value, 0, 8, 1);
                    }
                    lptPort.WritePort(AfterAnalysisScriptText);
                    lptPort.ClosePort();

                    //WriteLPT lptPortNew = new WriteLPT();
                    //lptPortNew.OpenPort(PortName, BaudRate, 0, 8, 1);
                    //lptPortNew.WritePort(AfterAnalysisScriptText);
                    //lptPortNew.ClosePort();
                }
            }
        }

        public override void SetParamValue(string paramName, string paramValue)
        {
            if (!_Params.Contains(paramName))
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
        }

        /// <summary>
        /// 解析脚本函数
        /// </summary>
        private void AnalysisScriptFunction()
        {
            if (string.IsNullOrEmpty(AfterAnalysisScriptText))
                return;

            string sql = AnalysisFunctionSql;

            System.Data.DataTable source = null;
            if (!string.IsNullOrEmpty(sql))
            {
                foreach (string p in Params)
                {
                    string v = string.Empty;
                    if (ParamsValue.ContainsKey(p))
                    {
                        v = ParamsValue[p];
                    }
                    v = string.Format("''{0}''", v.Replace("'", "''"));
                    sql = sql.Replace(p, v);
                }

                try
                {
                    if (ExecuteFunction!=null)
                    {
                        source = ExecuteFunction(sql);
                    }
                }
                catch { }
            }
            for (int i = 0; i < Functions.Count; i++)
            {
                string v = string.Empty;
                if (source != null && source.Rows.Count > 0)
                {
                    try { v = source.Rows[0][string.Format("c{0}", i)].ToString(); }
                    catch { }
                }
                if (v == null)
                    v = string.Empty;
                AfterAnalysisScriptText = AfterAnalysisScriptText.Replace(Functions[i], v);
            }
        }

        public override void SetPrinter(string printerName)
        {
        }

    }
}
