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
    }
}
