using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HappyReading.Model
{
    public class Book
    {

        /// <summary>
        /// ID
        /// </summary>
        public long Id { set; get; }

        /// <summary>
        /// 书名
        /// </summary>
        public string Name { set; get; }

        /// <summary>
        /// 书籍地址
        /// </summary>
        public string Url { set; get; }


        /// <summary>
        /// 更新状态
        /// </summary>
        public string UpdateState { set; get; }


        /// <summary>
        /// 详情页
        /// </summary>
        public string Details { set; get; }

        /// <summary>
        /// 作者
        /// </summary>
        public string Author { set; get; }

        /// <summary>
        /// 书源
        /// </summary>
        public string Source { set; get; }

        /// <summary>
        /// 封面图
        /// </summary>
        public string Image { set; get; }

        /// <summary>
        /// 最新章节
        /// </summary>
        public string Newest { set; get; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public string Update { set; get; }


        /// <summary>
        /// 已阅章节数
        /// </summary>
        public long Read { set; get; }

        /// <summary>
        /// 上次读到的章节名
        /// </summary>
        public string ReadChapter { set; get; }


        /// <summary>
        /// 养肥数
        /// </summary>
        public int FattenNumber { set; get; }


        /// <summary>
        /// 目录信息
        /// </summary>
       public  Dictionary<string, string> ListUrl { set; get; }


        
    }
}
