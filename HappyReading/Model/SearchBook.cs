using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HappyReading.Model
{
    /// <summary>
    /// 搜索中用到的字段  继承书源类
    /// </summary>
    public class SearchBook: BookSource
    {
        /// <summary>
        /// 搜索的页面文本
        /// </summary>
        public string Html { set; get; }

        /// <summary>
        /// 书籍地址
        /// </summary>
        public string BookUrl { set; get; }
    }
}
