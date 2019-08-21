using HappyReading.BLL;
using HappyReading.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HappyReading.DAL
{
    class TempData
    {

        /// <summary>
        /// 全部书源
        /// </summary>
        private static List<BookSource> BookSourceS = null;


        /// <summary>
        /// 写入全部书源
        /// </summary>
        /// <param name="bookSources"></param>
        public static void SetBookSourceS(List<BookSource> bookSources)
        {
            BookSourceS = bookSources;
        }

        /// <summary>
        /// 更新全部书源
        /// </summary>
        public static void UpdateBookSourceS()
        {
            BookSourceS = DataFetch.GetBookSources();
        }


        /// <summary>
        /// 初始化
        /// </summary>
        public static void Initialization()
        {
            if (BookSourceS == null)
            {
                BookSourceS = DataFetch.GetBookSources();
            }
        }


        /// <summary>
        /// 获取全部书源
        /// </summary>
        /// <returns></returns>
        public static List<BookSource> GetBookSourceS()
        {
            Initialization();
            return BookSourceS;
        }


        /// <summary>
        /// 存放网站编码的地方
        /// </summary>
        public static Dictionary<string, string> UrlCode = new Dictionary<string, string>();


        /// <summary>
        /// 获取网站编码
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetUrlCode(string url)
        {
            try
            {
                //把网址分割，取出域名部分
                string[] Urls = url.Split(new string[] { "://", "/" }, StringSplitOptions.RemoveEmptyEntries);
                if (UrlCode.ContainsKey(Urls[1]))
                {
                    return UrlCode[Urls[1]];
                }
                else
                {
                    UrlCode[Urls[1]] = GetHtml.GetCode(url);
                    return UrlCode[Urls[1]];
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                //Exception ex
                Tool.TextAdditional(ex.Message);
                return "";
            }
        }
    }
}
