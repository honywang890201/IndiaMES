using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorWeightPrint
{
    public class SerialClass
    {
        /// <summary>
        /// 串口
        /// </summary>
        private System.IO.Ports.SerialPort _serialPort = null;


        //定义委托
        public delegate void SerialPortDataReceiveEventArgs(object sender, SerialDataReceivedEventArgs e, byte[] bits);

        //定义接收数据事件
        public event SerialPortDataReceiveEventArgs DataReceived;

        //定义接收错误事件
        public event SerialErrorReceivedEventHandler Error;

        //接收事件是否有效   false 表示有效
        public bool ReceiveEventFlag = false;

        #region 获取串口名

        private string protName;

        public string PortName
        {

            get { return _serialPort.PortName; }

            set
            {

                _serialPort.PortName = value;

                protName = value;

            }

        }

        #endregion

        #region 获取比特率

        private int baudRate;

        public int BaudRate
        {

            get { return _serialPort.BaudRate; }

            set
            {

                _serialPort.BaudRate = value;

                baudRate = value;

            }

        }

        #endregion


        #region 默认构造函数

        /// <summary>

        /// 默认构造函数，操作COM1，速度为9600，没有奇偶校验，8位字节，停止位为1 "COM1", 9600, Parity.None, 8, StopBits.One

        /// </summary>

        public SerialClass()
        {

            _serialPort = new SerialPort();

        }

        #endregion

        #region 构造函数

        /// <summary>

        /// 构造函数,

        /// </summary>

        /// <param name="comPortName"></param>

        public SerialClass(string comPortName)
        {

            _serialPort = new SerialPort(comPortName);

            _serialPort.BaudRate = 9600;

            _serialPort.Parity = Parity.Even;

            _serialPort.DataBits = 8;

            _serialPort.StopBits = StopBits.One;

            _serialPort.Handshake = Handshake.None;

            _serialPort.RtsEnable = true;

            _serialPort.ReadTimeout = 2000;

            setSerialPort();

        }

        #endregion


        #region 构造函数,可以自定义串口的初始化参数

        /// <summary>

        /// 构造函数,可以自定义串口的初始化参数

        /// </summary>

        /// <param name="comPortName">需要操作的COM口名称</param>

        /// <param name="baudRate">COM的速度</param>

        /// <param name="parity">奇偶校验位</param>

        /// <param name="dataBits">数据长度</param>

        /// <param name="stopBits">停止位</param>

        public SerialClass(string comPortName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
        {

            _serialPort = new SerialPort(comPortName, baudRate, parity, dataBits, stopBits);

            _serialPort.RtsEnable = true;  //自动请求

            _serialPort.ReadTimeout = 3000;//超时

            setSerialPort();

        }

        #endregion

        #region 析构函数

        /// <summary>

        /// 析构函数，关闭串口

        /// </summary>

        ~SerialClass()
        {

            if (_serialPort.IsOpen)

                _serialPort.Close();

        }

        #endregion

        #region 设置串口参数

        /// <summary>

        /// 设置串口参数

        /// </summary>

        /// <param name="comPortName">需要操作的COM口名称</param>

        /// <param name="baudRate">COM的速度</param>

        /// <param name="dataBits">数据长度</param>

        /// <param name="stopBits">停止位</param>

        public void setSerialPort(string comPortName, int baudRate, int dataBits, int stopBits)
        {

            if (_serialPort.IsOpen)

                _serialPort.Close();

            _serialPort.PortName = comPortName;

            _serialPort.BaudRate = baudRate;

            _serialPort.Parity = Parity.None;

            _serialPort.DataBits = dataBits;

            _serialPort.StopBits = (StopBits)stopBits;

            _serialPort.Handshake = Handshake.None;

            _serialPort.RtsEnable = false;

            _serialPort.ReadTimeout = 3000;

            _serialPort.NewLine = "/r/n";

            setSerialPort();

        }

        #endregion

        #region 设置接收函数

        /// <summary>

        /// 设置串口资源,还需重载多个设置串口的函数

        /// </summary>

        void setSerialPort()
        {

            if (_serialPort != null)
            {

                //设置触发DataReceived事件的字节数为1

                _serialPort.ReceivedBytesThreshold = 1;

                //接收到一个字节时，也会触发DataReceived事件

                _serialPort.DataReceived += new SerialDataReceivedEventHandler(_serialPort_DataReceived);

                //接收数据出错,触发事件

                _serialPort.ErrorReceived += new SerialErrorReceivedEventHandler(_serialPort_ErrorReceived);

                //打开串口

                //openPort();

            }

        }

        #endregion



        #region 打开串口资源

        /// <summary>

        /// 打开串口资源

        /// <returns>返回bool类型</returns>

        /// </summary>

        public bool openPort()
        {

            bool ok = false;

            //如果串口是打开的，先关闭

            if (_serialPort.IsOpen)

                _serialPort.Close();

            try
            {

                //打开串口

                _serialPort.Open();

                ok = true;

            }

            catch (Exception Ex)
            {

                throw Ex;

            }

            return ok;

        }

        #endregion


        #region 关闭串口

        /// <summary>

        /// 关闭串口资源,操作完成后,一定要关闭串口

        /// </summary>

        public void closePort()
        {

            //如果串口处于打开状态,则关闭

            if (_serialPort.IsOpen)

                _serialPort.Close();

        }

        #endregion

        #region 接收串口数据事件

        /// <summary>

        /// 接收串口数据事件

        /// </summary>

        /// <param name="sender"></param>

        /// <param name="e"></param>

        void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

            //禁止接收事件时直接退出

            if (ReceiveEventFlag)
            {

                return;

            }

            try
            {

                System.Threading.Thread.Sleep(20);

                byte[] _data = new byte[_serialPort.BytesToRead];

                _serialPort.Read(_data, 0, _data.Length);

                if (_data.Length == 0) { return; }

                if (DataReceived != null)
                {

                    DataReceived(sender, e, _data);

                }

                //  _serialPort.DiscardInBuffer();  //清空接收缓冲区  

            }

            catch (Exception ex)
            {

                //throw ex;

            }

        }

        #endregion

        #region 接收数据出错事件

        /// <summary>

        /// 接收数据出错事件

        /// </summary>

        /// <param name="sender"></param>

        /// <param name="e"></param>

        void _serialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {

        }

        #endregion

        #region 发送数据string类型

        public void SendData(string data)
        {

            //发送数据

            //禁止接收事件时直接退出

            if (ReceiveEventFlag)
            {

                return;

            }

            if (_serialPort.IsOpen)
            {

                _serialPort.Write(data);

            }

        }

        #endregion

        #region 发送数据byte类型

        /// <summary>

        /// 数据发送

        /// </summary>

        /// <param name="data">要发送的数据字节</param>

        public void SendData(byte[] data, int offset, int count)
        {

            //禁止接收事件时直接退出

            if (ReceiveEventFlag)
            {

                return;

            }

            try
            {

                if (_serialPort.IsOpen)
                {

                    //_serialPort.DiscardInBuffer();//清空接收缓冲区

                    _serialPort.Write(data, offset, count);

                }

            }

            catch (Exception ex)
            {

                throw ex;

            }

        }

        #endregion

        #region 发送命令

        /// <summary>

        /// 发送命令

        /// </summary>

        /// <param name="SendData">发送数据</param>

        /// <param name="ReceiveData">接收数据</param>

        /// <param name="Overtime">超时时间</param>

        /// <returns></returns>

        public int SendCommand(byte[] SendData, ref  byte[] ReceiveData, int Overtime)
        {



            if (_serialPort.IsOpen)
            {

                try
                {

                    ReceiveEventFlag = true;        //关闭接收事件

                    _serialPort.DiscardInBuffer();  //清空接收缓冲区                

                    _serialPort.Write(SendData, 0, SendData.Length);

                    int num = 0, ret = 0;

                    System.Threading.Thread.Sleep(10);

                    ReceiveEventFlag = false;      //打开事件

                    while (num++ < Overtime)
                    {

                        if (_serialPort.BytesToRead >= ReceiveData.Length)

                            break;

                        System.Threading.Thread.Sleep(10);

                    }



                    if (_serialPort.BytesToRead >= ReceiveData.Length)
                    {

                        ret = _serialPort.Read(ReceiveData, 0, ReceiveData.Length);

                    }

                    else
                    {

                        ret = _serialPort.Read(ReceiveData, 0, _serialPort.BytesToRead);

                    }

                    ReceiveEventFlag = false;      //打开事件

                    return ret;

                }

                catch (Exception ex)
                {

                    ReceiveEventFlag = false;

                    throw ex;

                }

            }

            return -1;

        }

        #endregion

        #region 获取串口

        /// <summary>

        /// 获取所有已连接短信猫设备的串口

        /// </summary>

        /// <returns></returns>

        public string[] serialsIsConnected()
        {

            List<string> lists = new List<string>();

            string[] seriallist = getSerials();

            foreach (string s in seriallist)
            {

            }

            return lists.ToArray();

        }

        #endregion

        #region 获取当前全部串口资源

        /// <summary>

        /// 获得当前电脑上的所有串口资源

        /// </summary>

        /// <returns></returns>

        public string[] getSerials()
        {

            return SerialPort.GetPortNames();

        }

        #endregion

        #region 字节型转换16

        /// <summary>

        /// 把字节型转换成十六进制字符串

        /// </summary>

        /// <param name="InBytes"></param>

        /// <returns></returns>

        public static string ByteToString(byte[] InBytes)
        {

            string StringOut = "";

            foreach (byte InByte in InBytes)
            {

                StringOut = StringOut + String.Format("{0:X2} ", InByte);

            }

            return StringOut;

        }

        #endregion

        #region 十六进制字符串转字节型

        /// <summary>

        /// 把十六进制字符串转换成字节型(方法1)

        /// </summary>

        /// <param name="InString"></param>

        /// <returns></returns>

        public static byte[] StringToByte(string InString)
        {

            string[] ByteStrings;

            ByteStrings = InString.Split(" ".ToCharArray());

            byte[] ByteOut;

            ByteOut = new byte[ByteStrings.Length];

            for (int i = 0; i <= ByteStrings.Length - 1; i++)
            {

                //ByteOut[i] = System.Text.Encoding.ASCII.GetBytes(ByteStrings[i]);

                ByteOut[i] = Byte.Parse(ByteStrings[i], System.Globalization.NumberStyles.HexNumber);

                //ByteOut[i] =Convert.ToByte("0x" + ByteStrings[i]);

            }

            return ByteOut;

        }

        #endregion

        #region 十六进制字符串转字节型

        /// <summary>

        /// 字符串转16进制字节数组(方法2)

        /// </summary>

        /// <param name="hexString"></param>

        /// <returns></returns>

        public byte[] strToToHexByte(string hexString)
        {

            hexString = hexString.Replace(" ", "");

            if ((hexString.Length % 2) != 0)

                hexString += " ";

            byte[] returnBytes = new byte[hexString.Length / 2];

            for (int i = 0; i < returnBytes.Length; i++)

                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);

            return returnBytes;

        }

        #endregion

        #region 字节型转十六进制字符串

        /// <summary>

        /// 字节数组转16进制字符串

        /// </summary>

        /// <param name="bytes"></param>

        /// <returns></returns>

        public static string byteToHexStr(byte[] bytes)
        {

            string returnStr = "";

            if (bytes != null)
            {

                for (int i = 0; i < bytes.Length; i++)
                {

                    returnStr += bytes[i].ToString("X2");

                }

            }

            return returnStr;

        }

        #endregion

        /// <summary>  
        /// 字符串转Unicode  
        /// </summary>  
        /// <param name="source">源字符串</param>  
        /// <returns>Unicode编码后的字符串</returns>  
        public string String2Unicode(string source)
        {
            var bytes = Encoding.Unicode.GetBytes(source);
            var stringBuilder = new StringBuilder();
            for (var i = 0; i < bytes.Length; i += 2)
            {
                stringBuilder.AppendFormat("{0:x2}{1:x2}", bytes[i + 1], bytes[i]);//
            }
            return stringBuilder.ToString();
        }
        /// <summary>    
        /// 字符串转为UniCode码字符串    
        /// </summary>    
        /// <param name="s"></param>    
        /// <returns></returns>    
        public string StringToUnicode(string s)
        {
            char[] charbuffers = s.ToCharArray();
            byte[] buffer;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < charbuffers.Length; i++)
            {
                buffer = System.Text.Encoding.Unicode.GetBytes(charbuffers[i].ToString());
                sb.Append(String.Format("\\u{0:X2}{1:X2}", buffer[1], buffer[0]));
            }
            return sb.ToString();
        }


        public byte[] HexStringToByte(string InString)
        {
            string[] ByteStrings;
            ByteStrings = InString.Split(" ".ToCharArray());
            byte[] ByteOut;
            ByteOut = new byte[ByteStrings.Length - 1];
            for (int i = 0; i == ByteStrings.Length - 1; i++)
            {
                ByteOut[i] = Convert.ToByte(("0x" + ByteStrings[i]));
            }
            return ByteOut;
        }
        public byte[] HexStringToByteArray(string s)
        {
            s = s.Replace(" ", "");
            byte[] buffer = new byte[s.Length / 2];
            for (int i = 0; i < s.Length; i += 2)
            {
                buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);
            }
            return buffer;
        }
        /// <summary>    
        /// Unicode字符串转为正常字符串    
        /// </summary>    
        /// <param name="srcText"></param>    
        /// <returns></returns>    
        public static string UnicodeToString(string srcText)
        {
            string dst = "";
            string src = srcText;
            int len = srcText.Length / 6;
            for (int i = 0; i <= len - 1; i++)
            {
                string str = "";
                str = src.Substring(0, 6).Substring(2);
                src = src.Substring(6);
                byte[] bytes = new byte[2];
                bytes[1] = byte.Parse(int.Parse(str.Substring(0, 2), System.Globalization.NumberStyles.HexNumber).ToString());
                bytes[0] = byte.Parse(int.Parse(str.Substring(2, 2), System.Globalization.NumberStyles.HexNumber).ToString());
                dst += Encoding.Unicode.GetString(bytes);
            }
            return dst;
        }
        /// <summary>
        /// 获取要发送的内容
        /// </summary>
        /// <param name="strSend"></param>
        /// <returns></returns>
        public byte[] GetSend(string strSend)
        {
            //处理数字转换
            string sendBuf = strSend;
            string sendnoNull = sendBuf.Trim();
            string sendNOComma = sendnoNull.Replace(',', ' ');    //去掉英文逗号
            string sendNOComma1 = sendNOComma.Replace('，', ' '); //去掉中文逗号
            string strSendNoComma2 = sendNOComma1.Replace("0x", "");   //去掉0x
            strSendNoComma2.Replace("0X", "");   //去掉0X
            string[] strArray = strSendNoComma2.Split(' ');

            int byteBufferLength = strArray.Length;
            for (int i = 0; i < strArray.Length; i++)
            {
                if (strArray[i] == "")
                {
                    byteBufferLength--;
                }
            }
            // int temp = 0;
            byte[] byteBuffer = new byte[byteBufferLength];
            int ii = 0;
            for (int i = 0; i < strArray.Length; i++)        //对获取的字符做相加运算
            {

                Byte[] bytesOfStr = Encoding.Default.GetBytes(strArray[i]);

                int decNum = 0;
                if (strArray[i] == "")
                {
                    //ii--;     //加上此句是错误的，下面的continue以延缓了一个ii，不与i同步
                    continue;
                }
                else
                {
                    decNum = Convert.ToInt32(strArray[i], 16); //atrArray[i] == 12时，temp == 18 
                }

                try    //防止输错，使其只能输入一个字节的字符
                {
                    byteBuffer[ii] = Convert.ToByte(decNum);
                }
                catch (System.Exception ex)
                {
                    return null;
                }

                ii++;
            }
            return byteBuffer;
        }
        /// <summary>
        /// 位数处理
        /// </summary>
        /// <param name="AddString"></param>
        /// <returns></returns>
        public string AddSplit(string AddString)
        {
            if (AddString.Length == 2)
            {
                return "00 " + AddString;
            }
            if (AddString.Length == 3)
            {
                return AddString.Substring(0, 1) + " " + AddString.Substring(1, 2);
            }
            if (AddString.Length == 4)
            {
                return AddString.Substring(0, 2) + " " + AddString.Substring(2, 2);
            }

            return AddString;

        }
    }
}
