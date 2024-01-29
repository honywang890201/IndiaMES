using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Printer
{
    public abstract class Instance
    {
        public Func<string, System.Data.DataTable> ExecuteFunction = null;

        protected PrintType _PrintType;

        protected string _PrintTemplate = null;
        /// <summary>
        /// 打印模板或者脚本
        /// </summary>
        public string PrintTemplate
        {
            get
            {
                return _PrintTemplate;
            }
        }

        protected string _PrinterName = null;
        /// <summary>
        /// 默认打印机
        /// </summary>
        public string PrinterName
        {
            get
            {
                return _PrinterName;
            }
            set
            {
                _PrinterName = value;
            }
        }

        protected string _PortName = null;
        /// <summary>
        /// 端口名称
        /// </summary>
        public string PortName
        {
            get
            {
                return _PortName;
            }
        }

        protected int? _BaudRate = null;
        /// <summary>
        /// 波特率
        /// </summary>
        public int? BaudRate
        {
            get
            {
                return _BaudRate;
            }
        }

        protected List<string> _Params = null;
        /// <summary>
        /// 参数列表
        /// </summary>
        public List<string> Params
        {
            get
            {
                return _Params;
            }
        }
        /// <summary>
        /// 所有参数值
        /// </summary>
        protected Dictionary<string, string> ParamsValue = new Dictionary<string, string>();
        /// <summary>
        /// 二维码分隔符
        /// </summary>
        public string TwoDimensionCodeSeparator
        {
            get;
            set;
        }

        /// <summary>
        /// 列表参数
        /// </summary>
        protected Dictionary<string, ListParam> ListParams = new Dictionary<string, ListParam>();
        private List<string> ParamN_N = new List<string>();

        /// <summary>
        /// 获取列表参数(列表参数（$*_N$）,主要用于自动分页处理)
        /// </summary>
        protected void GetListParam()
        {
            ListParams.Clear();
            if (Params == null)
            {
                return;
            }

            string param;
            string linkParam;
            int index;
            int num;

            ListParam pm;
            Dictionary<string, ListParam> l = new Dictionary<string, ListParam>();

            #region
            foreach (string p in Params)
            {
                if (_PrintType==PrintType.Script)
                {
                    param = p;
                    index = p.LastIndexOf('_');
                    if (index > 1)
                    {
                        #region
                        linkParam = string.Format("${0}$", param.Substring(1, index - 1));
                        param = p.Substring(index + 1);
                        param = param.Substring(0, param.Length - 1); //param like N or N-N
                        index = param.IndexOf('-');
                        if (index < 0)
                        {
                            if (int.TryParse(param, out num))
                            {
                                num = num < 1 ? 1 : num;
                                if (l.ContainsKey(linkParam))
                                {
                                    pm = l[linkParam];

                                    pm.MinNum = pm.MinNum > num ? num : pm.MinNum;
                                    pm.MaxNum = pm.MaxNum < num ? num : pm.MaxNum;
                                    l[linkParam] = pm;
                                }
                                else
                                {
                                    pm.ListParamName = linkParam;
                                    pm.MinNum = num;
                                    pm.MaxNum = num;
                                    pm.cMaxNum = 0;
                                    l.Add(linkParam, pm);
                                }

                                param = linkParam.Substring(1, linkParam.Length - 2);
                            }
                            else
                            {
                                param = p.Substring(1, p.Length - 2);
                            }
                        }
                        else
                        {
                            int num2;
                            if (int.TryParse(param.Substring(0, index), out num) && int.TryParse(param.Substring(index + 1), out num2))
                            {
                                if (num < 1)
                                {
                                    num = 1;
                                }
                                if (num2 < 1)
                                {
                                    num2 = 1;
                                }
                                if (l.ContainsKey(linkParam))
                                {
                                    pm = l[linkParam];
                                    pm.cMaxNum = pm.cMaxNum < (num > num2 ? num : num2) ? (num > num2 ? num : num2) : pm.cMaxNum;
                                    l[linkParam] = pm;
                                }
                                else
                                {
                                    pm.ListParamName = linkParam;
                                    pm.MinNum = (num < num2 ? num : num2);
                                    pm.MaxNum = (num > num2 ? num : num2);
                                    pm.cMaxNum = (num > num2 ? num : num2);
                                    l.Add(linkParam, pm);
                                }

                                param = linkParam.Substring(1, linkParam.Length - 2);
                                if (!ParamN_N.Contains(p))
                                {
                                    ParamN_N.Add(p);
                                }
                            }
                            else
                            {
                                param = p.Substring(1, p.Length - 2);
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        param = p.Substring(1, p.Length - 2);
                    }
                }
                else
                {
                    param = p;
                    index = p.LastIndexOf('_');
                    if (index > 1 && index < param.Length - 1)
                    {
                        #region
                        linkParam = p.Substring(0, index);
                        param = p.Substring(index + 1);

                        index = param.IndexOf('-');
                        if (index < 0)
                        {
                            if (int.TryParse(param, out num))
                            {
                                num = num < 1 ? 1 : num;
                                if (l.ContainsKey(linkParam))
                                {
                                    pm = l[linkParam];

                                    pm.MinNum = pm.MinNum > num ? num : pm.MinNum;
                                    pm.MaxNum = pm.MaxNum < num ? num : pm.MaxNum;
                                    l[linkParam] = pm;
                                }
                                else
                                {
                                    pm.ListParamName = linkParam;
                                    pm.MinNum = num;
                                    pm.MaxNum = num;
                                    pm.cMaxNum = 0;
                                    l.Add(linkParam, pm);
                                }
                                param = linkParam;
                            }
                            else
                            {
                                param = p;
                            }
                        }
                        else
                        {
                            int num2;
                            if (int.TryParse(param.Substring(0, index), out num) && int.TryParse(param.Substring(index + 1), out num2))
                            {
                                if (num < 1)
                                {
                                    num = 1;
                                }
                                if (num2 < 1)
                                {
                                    num2 = 1;
                                }
                                if (l.ContainsKey(linkParam))
                                {
                                    pm = l[linkParam];

                                    pm.cMaxNum = pm.cMaxNum < (num > num2 ? num : num2) ? (num > num2 ? num : num2) : pm.cMaxNum;
                                    l[linkParam] = pm;
                                }
                                else
                                {
                                    pm.ListParamName = linkParam;
                                    pm.MinNum = (num < num2 ? num : num2);
                                    pm.MaxNum = (num > num2 ? num : num2);
                                    pm.cMaxNum = (num > num2 ? num : num2);
                                    l.Add(linkParam, pm);
                                }
                                param = linkParam;
                                if (!ParamN_N.Contains(p))
                                {
                                    ParamN_N.Add(p);
                                }
                            }
                            else
                            {
                                param = p;
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        param = p;
                    }
                }
            }
            #endregion

            foreach (string p in l.Keys)
            {
                if (l[p].MinNum != l[p].MaxNum)
                {
                    ListParams.Add(p, l[p]);
                }
            }
        }

        public abstract void Print(int number = 1);
        /// <summary>
        /// 设置参数的值
        /// </summary>
        /// <param name="paramName">参数名</param>
        /// <param name="paramValue">参数值</param>
        public abstract void SetParamValue(string paramName, string paramValue);

        /// <summary>
        /// 清空参数的值
        /// </summary>
        public virtual void ClearParamsValue()
        {
            if (Params == null)
                return;
            foreach (string p in Params)
            {
                SetParamValue(p, null);
            }

            ParamsValue.Clear();
        }

        /// <summary>
        /// 关闭
        /// </summary>
        public abstract void Close();
        public abstract void SetPrinter(string printerName);

        /// <summary>
        /// 打印  
        /// </summary>
        /// <param name="source">数据源</param>
        /// <param name="number">打印份数</param>
        public void Print(System.Data.DataRow row, int number = 1)
        {
            if (row == null)
                return;
            ClearParamsValue();
            foreach (string p in Params)
            {
                string param = p;
                if (_PrintType==PrintType.Script)
                {
                    param = param.Substring(1, param.Length - 2);
                }
                for (int j = 0; j < row.Table.Columns.Count; j++)
                {
                    if (row.Table.Columns[j].ColumnName.ToUpper().Trim() == param.ToUpper().Trim())
                    {
                        SetParamValue(p, row[j].ToString());
                        break;
                    }
                }
            } 
            Print(number);
        }

        /// <summary>
        /// 打印 
        /// </summary>
        /// <param name="table">数据源</param>
        /// <param name="number">打印份数</param>
        public void Print( System.Data.DataTable table, int number = 1)
        {
            if (table == null)
                return;
            for (int i = 0; i < table.Rows.Count; i++)
            {
                Print(table.Rows[i], number);
            }
        }

        /// <summary>
        /// 打印
        /// </summary>
        /// <param name="set">数据源</param>
        /// <param name="number">打印份数</param>
        public void Print(System.Data.DataSet set, int number = 1)
        {
            if (set == null || set.Tables.Count < 1 )
            {
                return;
            }

            /*
             source格式要求：
                  1、如果table是1个，就打印此table
                  2、如果table是多于1个，则第1个table只能是1行数据（否则，就会按照1打印了），其它table都当成了列表
             
             算法：主要解决自动分页问题
                    查询参数中是否存在类似于c1,c2,c3...cN这样的变量(并且source有多个table，第1个table仅1行数据),如果存在，表示变量以是一个列表值，有可能需要分页，每页N个，否则，就不用分页
             */

            bool IsNeedPage = false;
            int pageCount = int.MaxValue;

            #region 是否需要分页处理
            if (ListParams.Count > 0)
            {
                if (set.Tables.Count > 1)
                {
                    if (set.Tables[0].Rows.Count == 1)
                    {
                        IsNeedPage = true;
                        //计算总页数，以最小页数为准
                        foreach (string p in ListParams.Keys)
                        {
                            string col = p;
                            if (_PrintType == PrintType.Script)
                            {
                                col = col.Substring(1, col.Length - 2);
                            }

                            for (int i = 1; i < set.Tables.Count; i++)
                            {
                                for (int j = 0; j < set.Tables[i].Columns.Count; j++)
                                {
                                    if (set.Tables[i].Columns[j].ColumnName.ToUpper().Trim() == col.ToUpper().Trim())
                                    {
                                        int rowCount = ListParams[p].MaxNum == 0 ? ListParams[p].cMaxNum : ListParams[p].MaxNum;
                                        int pc = (set.Tables[i].Rows.Count % rowCount == 0) ? set.Tables[i].Rows.Count / rowCount : (set.Tables[i].Rows.Count / rowCount) + 1;
                                        if (pc > 0)
                                        {
                                            if (pageCount > pc)
                                            {
                                                pageCount = pc;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (IsNeedPage)
            {
                if (pageCount == int.MaxValue)
                {
                    IsNeedPage = false;
                }
                else if (pageCount == 0)
                {
                    pageCount = 1;
                    IsNeedPage = false;
                }
            }
            #endregion

            if (!IsNeedPage)
            {
                #region 不需要分页处理的打印
                Print( set.Tables[0], number);
                #endregion
                return;
            }
            else
            {
                for (int n = 1; n <= pageCount; n++)
                {
                    ClearParamsValue();

                    #region
                    if (_PrintType == PrintType.Script)
                    {
                        SetParamValue("$PageCount$", pageCount.ToString());
                        SetParamValue("$PageNowIndex$", n.ToString());
                    }
                    else
                    {
                        SetParamValue("PageCount", pageCount.ToString());
                        SetParamValue("PageNowIndex", n.ToString());
                    }
                    #endregion

                    #region 列表参数赋值
                    foreach (string p in ListParams.Keys)
                    {
                        string param = p;
                        if (_PrintType == PrintType.Script)
                        {
                            param = param.Substring(1, param.Length - 2);
                        }
                        bool IsBreak = false;
                        for (int i = 1; !IsBreak && i < set.Tables.Count; i++)
                        {
                            for (int j = 0; !IsBreak && j < set.Tables[i].Columns.Count; j++)
                            {
                                if (set.Tables[i].Columns[j].ColumnName.ToUpper().Trim() == param.ToUpper().Trim())
                                {
                                    #region 赋值
                                    int c = set.Tables[i].Rows.Count;
                                    c = (c % pageCount == 0) ? c / pageCount : (c / pageCount) + 1;

                                    int h;
                                    for (int t = 0; t < c; t++)
                                    {
                                        h = (n - 1) * c + t;
                                        if (h >= set.Tables[i].Rows.Count)
                                        {
                                            break;
                                        }
                                        if (_PrintType == PrintType.Script)
                                        {
                                            SetParamValue(string.Format("${0}_{1}$", param, t + 1), set.Tables[i].Rows[h][j].ToString());
                                        }
                                        else
                                        {
                                            SetParamValue(string.Format("{0}_{1}", param, t + 1), set.Tables[i].Rows[h][j].ToString());
                                        }
                                    }
                                    if (_PrintType == PrintType.Script)
                                    {
                                        SetParamValue(string.Format("${0}_Count$", param), set.Tables[i].Rows.Count.ToString());
                                        SetParamValue(string.Format("${0}_PageNowCount$", param), c.ToString());
                                    }
                                    else
                                    {
                                        SetParamValue(string.Format("{0}_Count", param), set.Tables[i].Rows.Count.ToString());
                                        SetParamValue(string.Format("{0}_PageNowCount", param), c.ToString());
                                    }

                                    #region 处理N-N的情况
                                    foreach (string pms in ParamN_N)
                                    {
                                        string pm = pms;
                                        if (_PrintType == PrintType.Script)
                                        {
                                            pm = pm.Substring(1, pm.Length - 2);
                                        }
                                        if (pm.StartsWith(param) && pm.Length > param.Length)
                                        {
                                            pm = pm.Substring(param.Length + 1);
                                            int index = pm.IndexOf('-');
                                            if (index > 0 && index < pm.Length - 1)
                                            {
                                                int num1;
                                                int num2;
                                                if (int.TryParse(pm.Substring(0, index), out num1) && int.TryParse(pm.Substring(index + 1), out num2))
                                                {
                                                    if (num1 > 0 && num2 > 0 && num1 != num2)
                                                    {
                                                        if (num1 > num2)
                                                        {
                                                            num1 = num1 + num2;
                                                            num2 = num1 - num2;
                                                            num1 = num1 - num2;
                                                        }

                                                        string v = string.Empty;
                                                        if (num2 > c)
                                                        {
                                                            #region N-N
                                                            for (int k = num1; k <= num2; k++)
                                                            {
                                                                if (k > set.Tables[i].Rows.Count)
                                                                {
                                                                    break;
                                                                }
                                                                if (k <= 0)
                                                                {
                                                                    continue;
                                                                }

                                                                if (v != string.Empty)
                                                                {
                                                                    if (!string.IsNullOrEmpty(TwoDimensionCodeSeparator))
                                                                    {
                                                                        v = v + TwoDimensionCodeSeparator;
                                                                    }
                                                                }
                                                                v = v + set.Tables[i].Rows[k - 1][j].ToString();
                                                            }
                                                            #endregion
                                                        }
                                                        else
                                                        {
                                                            #region N-N
                                                            for (int k = num1; k <= num2; k++)
                                                            {
                                                                h = (n - 1) * c + (k - 1);
                                                                if (h < set.Tables[i].Rows.Count && h >= 0)
                                                                {
                                                                    if (set.Tables[i].Rows[h][j].ToString() != string.Empty)
                                                                    {
                                                                        if (v != string.Empty)
                                                                        {
                                                                            if (!string.IsNullOrEmpty(TwoDimensionCodeSeparator))
                                                                            {
                                                                                v = v + TwoDimensionCodeSeparator;
                                                                            }
                                                                        }
                                                                        v = v + set.Tables[i].Rows[h][j].ToString();
                                                                    }
                                                                }
                                                            }
                                                            #endregion
                                                        }
                                                        SetParamValue(pms, v);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    #endregion

                                    #endregion
                                    IsBreak = true;
                                    break;
                                }
                            }
                        }

                    }
                    #endregion

                    #region 所有参数赋值(非列表参数)
                    foreach (string p in Params)
                    {
                        string param = p;
                        if (_PrintType == PrintType.Script)
                        {
                            param = param.Substring(1, param.Length - 2);
                        }
                        for (int j = 0; j < set.Tables[0].Columns.Count; j++)
                        {
                            if (set.Tables[0].Columns[j].ColumnName.ToUpper().Trim() == param.ToUpper().Trim())
                            {
                                SetParamValue(p, set.Tables[0].Rows[0][j].ToString());
                            }
                        }
                    }
                    #endregion

                    Print(number);
                }
            }
        }

        /// <summary>
        /// 列表参数定义
        /// </summary>
        protected struct ListParam
        {
            public string ListParamName;
            public int MinNum;
            public int MaxNum;
            public int cMaxNum;
        }

        public static Instance Factory(string PrintTemplate, string PrinterName, string PortName, int BaudRate)
        {
            PrintType t = GetPrintType(PrintTemplate);

            switch(t)
            {
                case PrintType.CodeSoft:
                    return new CodeSoft(PrintTemplate, PrinterName);
                case PrintType.Bartender:
                    return new Bartender(PrintTemplate, PrinterName);
                case PrintType.CusScript:
                    return new Script();
                default:
                    Script script = new Script();
                    script.Init(PrintTemplate, PortName, BaudRate);
                    return script;
            }
        }

        public static PrintType GetPrintType(string PrintTemplate)
        {
            if (string.IsNullOrEmpty(PrintTemplate))
                return PrintType.Script;
            PrintTemplate = PrintTemplate.ToUpper();
            if (PrintTemplate.EndsWith(".TXT"))
            {
                return PrintType.Script;
            }
            else if (PrintTemplate.EndsWith(".LAB"))
            {
                return PrintType.CodeSoft;
            }
            else if (PrintTemplate.EndsWith(".BTW"))
            {
                return PrintType.Bartender;
            }
            else if (PrintTemplate.EndsWith(".ZHL"))
            {
                return PrintType.CusScript;
            }
            else
            {
                return PrintType.Script;
            }
        }
    }

    public enum PrintType
    {
        /// <summary>
        /// CodeSoft
        /// </summary>
        CodeSoft,
        /// <summary>
        /// Bartender
        /// </summary>
        Bartender,
        /// <summary>
        /// 脚本
        /// </summary>
        Script,
        /// <summary>
        /// 自定义脚本
        /// </summary>
        CusScript,

    }
}
