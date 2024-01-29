using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SeasideResearch.LibCurlNet;
namespace USBCopy
{
    class FtpDownLoad
    {
        static BinaryWriter bw = null;
        public void mFtpDownLoad(string url,string path,string filename,string username, string psd)
        {
            try
            {
                Curl.GlobalInit((int)CURLinitFlag.CURL_GLOBAL_ALL);
                Easy easy = new Easy();
                Easy.WriteFunction wf = new Easy.WriteFunction(OnWriteData);
                easy.SetOpt(CURLoption.CURLOPT_URL, url);
                easy.SetOpt(CURLoption.CURLOPT_VERBOSE, 1);
                easy.SetOpt(CURLoption.CURLOPT_WRITEFUNCTION, wf);
                easy.SetOpt(CURLoption.CURLOPT_NOPROGRESS,0);
                easy.SetOpt(CURLoption.CURLOPT_USERPWD, username+":"+ psd);
                bw = new BinaryWriter(new FileStream(path+ filename, FileMode.Create));
                //Int32 a = 0;
                easy.Perform();
                easy.Cleanup();
                bw.Close();
                Curl.GlobalCleanup();
           }
           catch (Exception ex)
           {
               
            }
           

        }
        public static Int32 OnWriteData(Byte[] buf, Int32 size, Int32 nmemb,
                                        Object extraData)
        {
            bw.Write(buf);
            return size * nmemb;
        }
    }
}
