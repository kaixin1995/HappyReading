using HappyReading.DAL;
using HappyReading.Model;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace HappyReading.BLL
{
    class GetHtml
    {

        /// <summary>
        /// 带浏览器信息抓取网页（utf-8）
        /// </summary>
        /// <param name="url">网页地址</param>
        /// <returns></returns>
        public static string GetHttp(string url, string Code= "utf-8")
        {
            try
            {
                Uri uri = new Uri(url);
                HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(uri);
                myReq.UserAgent = "User-Agent:Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705";
                myReq.Accept = "*/*";
                myReq.KeepAlive = true;
                myReq.Headers.Add("Accept-Language", "zh-cn,en-us;q=0.5");

                //设置超时时间为20秒
                //myReq.Timeout = 5000;
                HttpWebResponse result = (HttpWebResponse)myReq.GetResponse();
               
                Stream receviceStream = result.GetResponseStream();
                StreamReader readerOfStream = new StreamReader(receviceStream,Encoding.GetEncoding(Code));
                string strHTML = readerOfStream.ReadToEnd();
                readerOfStream.Close();
                receviceStream.Close();
                result.Close();
                return strHTML;
            }
            catch 
            {
                return null;
            }
        }



        /// <summary>
        /// 获取网页内容
        /// </summary>
        /// <param name="url">需要查询的链接</param>
        /// <returns></returns>
        public static string GetHttpWebRequest(string url)
        {
            //网站编码
            string code = string.Empty;
            BookSource bookSource = DataFetch.URLGetBookSource(url);

            //防止为NULL
            //if (bookSource == null) return "";

            if (bookSource.Code == null|| bookSource.Code.Trim().Length<=0)
            {
                code = GetCode(url);
                bookSource.Code = code;
                SQLiteDBHelper.ExecuteDataTable("update BookSource set Code='"+ code + "' where Url='"+ bookSource .Url+ "'; ", null);
                TempData.UpdateBookSourceS();

            }
            else
            {
                code = bookSource.Code;
            }

            return GetHttp(url, code);
        }

        public void DoSomething()
        {
            // 休眠 5秒 
            Thread.Sleep(new TimeSpan(0, 0, 0, 5));
        }

        /// <summary>
        /// 智能判断网站编码
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetCode(string url)
        {

            //网站编码
            string Code = string.Empty;

            string html = GetHttp(url);
            if (html==null)
            {
                return "";
            }
            
            ;
            if ((Code = Tool.GetRegexStr(html, "charset=\"([\\s\\S]*?)\"").Trim()).Length > 2) return Code;
            

            if ((Code = Tool.GetRegexStr(html, "charset=([\\s\\S]*?)\"").Trim()).Length > 2) return Code;

            if (html.Contains("charset=gbk") || html.Contains("charset=GBK"))
            {
                return "GBK";
            }
            else if (html.Contains("charset=GB2312") || html.Contains("charset=gb2312"))
            {
                return "GB2312";
            }

            return Code;

        }


        /// <summary>
        /// 检测网站是否返回码为200
        /// </summary>
        /// <param name="url">判断的链接</param>
        /// <returns></returns>
        public static bool GetUnicom(string url)
        {
            string StatusCode = null;
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.CreateDefault(new Uri(url));
                req.Method = "HEAD";
                req.Timeout = 1000;
                HttpWebResponse res = (HttpWebResponse)req.GetResponse();
                StatusCode = res.StatusCode.ToString();
                if (StatusCode == "OK")
                {
                    return true;
                }
            }
            catch (WebException ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
            return false;
        }
 
    }
}
