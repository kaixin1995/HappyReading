using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

namespace HappyReading.BLL
{
    public class Tool
    {
        /// <summary>
        /// 更新日期格式化
        /// </summary>
        /// <param name="date">更新日期</param>
        /// <returns>格式化的文本</returns>
        public static string GetUpdataDate(string date)
        {
            if (date.Length <= 1)
            {
                Console.WriteLine("时间获取失败~");
                return "";
            }

            DateTime Getdate = Convert.ToDateTime(date);
            DateTime newDate = DateTime.Now;
            TimeSpan timeSpan = newDate - Getdate;


            //一小时内
            if (timeSpan.TotalMinutes < 60 && timeSpan.TotalHours < 1)
            {
                return (int)timeSpan.TotalMinutes + "分前";
            }

            //24小时内
            if (timeSpan.TotalHours < 24 && (int)timeSpan.TotalHours > 1)
            {
                return (int)timeSpan.TotalHours + "小时前";
            }

            //月
            if (timeSpan.TotalDays > 30&& timeSpan.TotalDays<365)
            {
                return ((int)timeSpan.TotalDays / 30) + "月前";
            }

            //年
            if (timeSpan.TotalDays > 365)
            {
                return ((int)timeSpan.TotalDays/365) + "年前";
            }
            

            return (int)timeSpan.TotalDays + "天前";
        }


        /// <summary>
        /// 智能拼凑文章URL
        /// </summary>
        /// <param name="host">主页</param>
        /// <param name="url">文章页</param>
        /// <param name="BookListUrl">目录页</param>
        /// <returns>返回处理的文章链接</returns>
        public static string ObtainUrl(string host, string url, string BookListUrl = null)
        {
            if (url.Length > 0)
            {
                if (url.StartsWith("http"))
                {
                    return url;
                }
                else if (url.Substring(0, 1) == "/")
                {
                    return host + url;
                }
                else
                {
                    return BookListUrl + url;
                }
            }
            return url;
        }


        /// <summary>
        /// 获取系统中文字体
        /// </summary>
        /// <returns>返回字体列表</returns>
        public static List<string> GetTypeface()
        {
            List<string> Typeface = new List<string>();
            foreach (FontFamily fontfamily in Fonts.SystemFontFamilies)
            {
                LanguageSpecificStringDictionary fontdics = fontfamily.FamilyNames;

                //判断该字体是不是中文字体   英文字体为en-us
                if (fontdics.ContainsKey(XmlLanguage.GetLanguage("zh-cn")))
                {
                    string fontfamilyname = null;
                    if (fontdics.TryGetValue(XmlLanguage.GetLanguage("zh-cn"), out fontfamilyname))
                    {
                        Typeface.Add(fontfamilyname);
                    }
                }
            }
            return Typeface;
        }



        /// <summary>
        /// 正则查找
        /// </summary>
        /// <param name="reString">正文</param>
        /// <param name="regexCode">正则表达式</param>
        /// <returns>返回结果(单条数据)</returns>
        public static string GetRegexStr(string reString, string regexCode)
        {
            try
            {
                System.Text.RegularExpressions.Regex reg;//正则表达式变量
                reg = new System.Text.RegularExpressions.Regex(regexCode);//初始化正则对象

                System.Text.RegularExpressions.MatchCollection mc = reg.Matches(reString);//匹配;
                string temp = string.Empty;//声明一个临时变量
                for (int ic = 0; ic < mc.Count; ic++)
                {
                    System.Text.RegularExpressions.GroupCollection gc = mc[ic].Groups;//获取所有分组
                    System.Collections.Specialized.NameValueCollection nc = new System.Collections.Specialized.NameValueCollection();
                    for (int i = 0; i < gc.Count; i++)
                    {
                        temp = gc[i].Value;  //得到组对应数据

                    }

                }
                return temp;
            }
            catch
            {
                return string.Empty;
            }

        }

        /// <summary>
        /// 获取名称
        /// </summary>
        /// <param name="str">输入字段</param>
        /// <returns>返回名称</returns>
        public static string GetName(string str)
        {
            if (str.StartsWith("<a"))
            {
                
                return GetRegexStr(str, ">([\\s\\S]*?)</a>").Trim();
            }
            return str;
        }


        /// <summary>
        /// 获取链接
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string GetUrl(string str)
        {
            if (str.StartsWith("<a"))
            {
                return GetRegexStr(str, "href *= *['\"]*(\\S+)[\"']").Trim();
            }
            return str;

        }


        /// <summary>
        /// 函数运行超时则终止执行（超时则返回true，否则返回false）
        /// </summary>
        /// <typeparam name="T">参数类型</typeparam>
        /// <param name="action">要被执行的函数</param>
        /// <param name="p">函数需要的一个参数</param>
        /// <param name="timeoutMilliseconds">超时时间(毫秒)</param>
        /// <returns>超时则返回true，否则返回false</returns>
        public static void CallWithTimeout<T>(Action<T> action, T p, int timeoutMilliseconds)
        {
            Thread threadToKill = null;
            Action wrappedAction = () =>
            {
                threadToKill = Thread.CurrentThread;
                action(p);
            };

            IAsyncResult result = wrappedAction.BeginInvoke(null, null);
            if (result.AsyncWaitHandle.WaitOne(timeoutMilliseconds))
            {
                wrappedAction.EndInvoke(result);
            }
            else
            {
                threadToKill.Abort();
                throw new TimeoutException();
            }
        }


        /// <summary>
        /// 自由html过滤系统
        /// </summary>
        /// <param name="html">html字符</param>
        /// <returns>过滤过的字符串</returns>
        public static string HtmlFilter(string strHtml)
        {
            if (string.IsNullOrEmpty(strHtml)) return strHtml;
            
            string countbr =GetRegexStr(strHtml, "<br/>\\s+<br/>");
            if (countbr != "" || countbr.Length > 1)
            {
                strHtml = strHtml.Replace(countbr, "\r\n\r\n");
            }

            strHtml = strHtml.Replace("</div>", "");
            strHtml = strHtml.Replace("<div", "");
            strHtml = strHtml.Replace("热门推荐", "");
            strHtml = strHtml.Replace("divstyle", "");
            strHtml = strHtml.Replace("&gt;", ">");
            strHtml = strHtml.Replace("&lt;", "<");
            strHtml = strHtml.Replace("&nbsp;", " ");
            strHtml = strHtml.Replace("<br/>", "\r\n");
            strHtml = strHtml.Replace("</br>", "\r\n");
            strHtml = strHtml.Replace("<br />", "\r\n");
            strHtml = strHtml.Replace("<b>", "");
            strHtml = strHtml.Replace("</b>", "");
            strHtml = strHtml.Replace("Ｘ２３ＵＳ．ＣＯＭ更新最快", "");
            strHtml = strHtml.Replace("Ｘ２３ＵＳ．ＣＯＭ", "");
            strHtml = strHtml.Replace("最快更新", "");
            strHtml = strHtml.Replace("最新章节！", "");
            strHtml = strHtml.Replace("最新章节", "");
            strHtml = strHtml.Replace("小说网..org", "");
            strHtml = strHtml.Replace("更新最快", "");
            strHtml = strHtml.Replace("２３ＵＳ．ＣＯＭ", "");
            strHtml = strHtml.Replace("２３ＵＳ．ＣＯＭ", "");
            strHtml = strHtml.Replace("&amp;", "");
            strHtml = strHtml.Replace("nbsp;", "");
            strHtml = strHtml.Replace(";;", "");
            strHtml = strHtml.Replace("『3￥3小说网→』﹃值得收藏的网络小說阅读网", "");
            strHtml = strHtml.Replace("<script>readx();</script>", "");
            strHtml = strHtml.Replace("<script>chaptererror();</script>", "");
            strHtml = strHtml.Replace("readx();", "");
            strHtml = strHtml.Replace("style=\"text-align: center\"><script>read3();</script>", "");
            strHtml = strHtml.Replace("&ldquo;", "“");
            strHtml = strHtml.Replace("&rdquo;", "”");
            strHtml = strHtml.Replace("&hellip;", "…");
            strHtml = strHtml.Replace("<p>", "");
            strHtml = strHtml.Replace("</p>", "\r\n\r\n");
            
            return strHtml.Trim();
        }


        /// <summary>
        /// 获取中英文混排字符串的实际长度(字节数)
        /// </summary>
        /// <param name="str">要获取长度的字符串</param>
        /// <returns>字符串的实际长度值（字节数）</returns>
        public static int GetStringLength(string str)
        {
            if (str.Equals(string.Empty))
                return 0;
            int strlen = 0;
            ASCIIEncoding strData = new ASCIIEncoding();
            //将字符串转换为ASCII编码的字节数字
            byte[] strBytes = strData.GetBytes(str);
            for (int i = 0; i <= strBytes.Length - 1; i++)
            {
                if (strBytes[i] == 63)  //中文都将编码为ASCII编码63,即"?"号
                    strlen++;
                strlen++;
            }
            return strlen;
        }

        //计算文字的长度
        public static int Select(double fontSzie,string str)
        {
            TextBlock temp_tb = new TextBlock();
            temp_tb.FontSize = fontSzie;
            temp_tb.Text = "我";
            //actualWidth

            /*
             * 一行最多有42个字符
             * 最多可以有22行
             * 那么总共就是22*42=924个字符
             * 一个字符宽17.86  高13.41
             * 
             */

            double sizeWidth = temp_tb.ActualWidth;

            double count = 750 / sizeWidth;

            TextBlock tb = new TextBlock();
            tb.FontSize = fontSzie;
            tb.Text = str;

            return 0;
        }



        /// <summary>
        /// 截取超过长度的字符串
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string InterceptStr(string str)
        {
            if (str.Length > 13)
            {
                return str.Substring(0, 12) + "…";
            }
            return str;
        }
    }
}
