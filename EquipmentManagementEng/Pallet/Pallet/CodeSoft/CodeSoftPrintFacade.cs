using MS.BaseFram.Common.Domain;
using MS.BaseFram.Common.DomainDataProvider;
using MS.BaseFram.Web.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MaterialSN.CodeSoft
{
    public class CodeSoftPrintFacade : MarshalByRefObject
    {
        #region dataprovider
        private IDomainDataProvider _domainDataProvider = null;
        private FacadeHelper _helper = null;

        public CodeSoftPrintFacade(IDomainDataProvider domainDataProvider)
        {
            this._domainDataProvider = domainDataProvider;
            this._helper = new FacadeHelper(DataProvider);
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        public CodeSoftPrintFacade()
        {
            this._helper = new FacadeHelper(DataProvider);
        }

        protected IDomainDataProvider DataProvider
        {
            get
            {
                if (_domainDataProvider == null)
                {
                   // _domainDataProvider = DomainDataProviderManager.DomainDataProvider();
                }
                return _domainDataProvider;
            }
        }
        #endregion

        #region Print

        private bool _IsBatchPrint = true;
        private string _DataDescFileName = "Label.dsc";
        protected CodeSoftPrintData _CodeSoftPrintData = new CodeSoftPrintData();
        protected CodeSoftFacade _CodeSoftFacade = new CodeSoftFacade();

        public string DataDescFileName
        {
            get { return _DataDescFileName; }
            set { _DataDescFileName = value; }
        }

        //public UserControl.Messages Print(string printer, string templatePath, MKeyPart mKeyPart, List<MKeyPartDetail> mKeyPartDetailList, List<string> reserveInfo)
        //{
        //    UserControl.Messages messages = new UserControl.Messages();

        //    try
        //    {
        //        try
        //        {
        //            this.PrePrint();
        //            _CodeSoftFacade.OpenTemplate(printer, templatePath);
        //        }
        //        catch (System.Exception ex)
        //        {
        //            messages.Add(new UserControl.Message(UserControl.MessageType.Error, ex.Message));
        //            return messages;
        //        }

        //        //批量打印前生成文本文件
        //        string strBatchDataFile = string.Empty;
        //        if (_IsBatchPrint)
        //        {
        //            strBatchDataFile = CreateFile();
        //        }

        //        for (int i = 0; i < mKeyPartDetailList.Count; i++)
        //        {
        //            LabelPrintVars labelPrintVars = new LabelPrintVars();

        //            string[] vars = new string[0];

        //            if (messages.IsSuccess())
        //            {
        //                try
        //                {
        //                    //要传给Codesoft的数组，字段顺序不能修改
        //                    vars = this.GetPrintVars(mKeyPart, mKeyPartDetailList[i], reserveInfo);

        //                    //批量打印前的写文件
        //                    if (_IsBatchPrint)
        //                    {
        //                        string[] printVars = ProcessVars(vars, labelPrintVars);
        //                        WriteFile(strBatchDataFile, printVars);
        //                    }
        //                    //直接打印
        //                    else
        //                    {
        //                        _CodeSoftFacade.LabelPrintVars = labelPrintVars;
        //                        _CodeSoftFacade.Print(vars);
        //                    }
        //                }
        //                catch (System.Exception ex)
        //                {
        //                    messages.Add(new UserControl.Message(UserControl.MessageType.Error, ex.Message));
        //                    return messages;
        //                }
        //            }
        //        }

        //        //批量打印
        //        if (_IsBatchPrint)
        //        {
        //            try
        //            {
        //                _CodeSoftFacade.Print(strBatchDataFile, GetDataDescPath(_DataDescFileName));
        //            }
        //            catch (System.Exception ex)
        //            {
        //                messages.Add(new UserControl.Message(UserControl.MessageType.Error, ex.Message));
        //                return messages;
        //            }
        //        }

        //        messages.Add(new UserControl.Message(UserControl.MessageType.Success, "$Success_Print_Label"));
        //    }
        //    finally
        //    {
        //    }

        //    return messages;
        //}


        public virtual void PrePrint()
        {
        }

        //取出打印中要传给模板的数据，各个子类应分别实现这个方法
        public string[] GetPrintVars(string serialNo)
        {
            string[] returnValue = new string[1];
            returnValue[0] = serialNo;
            return returnValue;
        }

        public string[] GetPrintVars(string po,string suppilercode,
            string suppilername, string itemcode, string itemname, string itemspec,string sn,
            string qty,string unit,string datecode,string lotcode)
        {
            string[] returnValue = new string[12];
            for (int i = 0; i < returnValue.Length; i++)
            {
                returnValue[i] = string.Empty;
            }

            returnValue[0] = po;
            returnValue[1] = suppilercode;
            returnValue[2] = suppilername;
            returnValue[3] = itemcode;
            returnValue[4] = itemname;
            returnValue[5] = itemspec;

            returnValue[6] = sn;
            returnValue[7] = qty;
            returnValue[8] = unit;
            returnValue[9] = datecode;
            returnValue[10] = lotcode;
            return returnValue;
        }

        public string[] GetPrintVars(string MaterialCode, string ProdCycle)
        {
            string[] returnValue = new string[12];
            for (int i = 0; i < returnValue.Length; i++)
            {
                returnValue[i] = string.Empty;
            }

            returnValue[0] = MaterialCode;
            returnValue[1] = ProdCycle;

            return returnValue;
        }

        //取出打印中要传给模板的数据，各个子类应分别实现这个方法
        //protected string[] GetPrintVars(MKeyPart mKeyPart, MKeyPartDetail mKeyPartDetail, List<string> reserveInfo)
        //{
        //    if (mKeyPart == null || mKeyPartDetail == null)
        //        return null;

        //    int count = (reserveInfo == null) ? 0 : reserveInfo.Count;
        //    string[] returnValue = new string[20 + count];
        //    for (int i = 0; i < returnValue.Length; i++)
        //    {
        //        returnValue[i] = string.Empty;
        //    }

        //    returnValue[1] = mKeyPart.MItemCode;
        //    returnValue[2] = mKeyPart.LotNO;
        //    returnValue[3] = mKeyPart.PCBA;
        //    returnValue[4] = mKeyPart.BIOS;
        //    returnValue[5] = mKeyPart.Version;
        //    returnValue[6] = mKeyPart.VendorItemCode;
        //    returnValue[7] = mKeyPart.VendorCode;
        //    returnValue[8] = mKeyPart.DateCode;
        //    returnValue[9] = mKeyPart.MoCode;
        //    returnValue[10] = mKeyPart.MITEMNAME;
        //    returnValue[11] = mKeyPartDetail.SerialNo;

        //    for (int i = 0; i < count; i++)
        //    {
        //        returnValue[20 + i] = reserveInfo[i];
        //    }

        //    return returnValue;
        //}

        //bighai.wang 2009/03/03 发料物料标签打印
        protected string[] GetPrintVarsMaterialLot(string materialLot)
        {
            if (materialLot == null)
                return null;


            string[] returnValue = new string[1];

            returnValue[0] = materialLot;

            return returnValue;
        }

        //取出打印中要传给模板的数据，各个子类应分别实现这个方法
        //#3
        protected string GetPrintVars_No3()
        {
            return _CodeSoftPrintData.No3Seq;
        }

        //取出打印中要传给模板的数据，各个子类应分别实现这个方法
        //#2
        protected string GetPrintVars_No2()
        {
            return _CodeSoftPrintData.No2Seq;
        }

        /// <summary>
        /// 处理批量打印的数据，以标签为单位
        /// </summary>
        /// <param name="vars"></param>
        /// <param name="labelPrintVars"></param>
        /// <returns></returns>
        public string[] ProcessVars(string[] vars, LabelPrintVars labelPrintVars)
        {
            //获取最大的序号
            int intMaxSeq = vars.Length - 1;
            for (int i = 0; i < labelPrintVars.LabelVars_No2.Length; i++)
            {
                int intSeq = int.Parse(labelPrintVars.LabelVars_No2[i].Substring(3));
                if (intMaxSeq < intSeq) { intMaxSeq = intSeq; }
            }
            for (int i = 0; i < labelPrintVars.LabelVars_No3.Length; i++)
            {
                int intSeq = int.Parse(labelPrintVars.LabelVars_No3[i].Substring(3));
                if (intMaxSeq < intSeq) { intMaxSeq = intSeq; }
            }

            //****赋值******
            string[] results = new string[intMaxSeq + 1];
            for (int i = 0; i <= intMaxSeq; i++)
            { results[i] = string.Empty; }

            //vars
            for (int i = 0; i < vars.Length; i++)
            {
                results[i] = vars[i];
            }
            //并排打印赋值
            for (int i = 0; i < labelPrintVars.LabelVars_No2.Length; i++)
            {
                int intSeq = int.Parse(labelPrintVars.LabelVars_No2[i].Substring(3));
                results[intSeq] = labelPrintVars.LabelValues_No2[i];
            }
            for (int i = 0; i < labelPrintVars.LabelVars_No3.Length; i++)
            {
                int intSeq = int.Parse(labelPrintVars.LabelVars_No3[i].Substring(3));
                results[intSeq] = labelPrintVars.LabelValues_No3[i];
            }

            return results;
        }

        public string CreateFile()
        {
            string strFile = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "PrintTemp");
            if (System.IO.Directory.Exists(strFile) == false)
            {
                System.IO.Directory.CreateDirectory(strFile);
            }

            //临时目录中的旧文件适当删除一些
            DirectoryInfo dir = new DirectoryInfo(strFile);
            FileInfo[] files = dir.GetFiles();
            if (files != null && files.Length > 0)
            {
                for (int i = 0; i < files.Length; i++)
                {
                    if (files[i].CreationTime.AddDays(30) < DateTime.Now)
                    {
                        try
                        {
                            files[i].Attributes &= ~FileAttributes.ReadOnly;
                            files[i].Delete();
                        }
                        catch { }
                    }
                }
            }
            strFile =Path.Combine(strFile, "LabelPrint_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + DateTime.Now.Millisecond + "_Data.txt");
            StreamWriter writer = new StreamWriter(strFile);
            writer.Close();
            return strFile;
        }

        public void WriteFile(string strFile, string[] vars)
        {
            string strOutput = string.Empty;
            for (int i = 0; i < vars.Length; i++)
            { strOutput += "\"" + vars[i] + "\","; }
            if (strOutput.Length > 0) { strOutput = strOutput.Substring(0, strOutput.Length - 1); }

            StreamWriter writer = new StreamWriter(strFile, true, Encoding.Default);
            writer.WriteLine(strOutput);
            writer.Close();
        }

        public string GetDataDescPath(string fileName)
        {
            string strFile = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "MaterialSNTemplate");
            strFile = Path.Combine(strFile, fileName);
            return strFile;
        }

        #endregion
        #region 需要传递给CodeSoft进行打印的数据
        /// <summary>
        /// 
        /// </summary>
        public class CodeSoftPrintData
        {

            public string No3Seq;
            public string No2Seq;
            public string No2Year;
            public string VASeq;
            public string Year;
            public string Month;
            public string Day;
            public string Week;

        }
        #endregion
    }
}
