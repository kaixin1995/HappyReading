using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HappyReading.Model
{
    /// <summary>
    /// 获取目录
    /// </summary>
    public class GetDirectory
    {
        /// <summary>
        /// 文章ID
        /// </summary>
        public int Id { set; get; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { set; get; }


        /// <summary>
        /// 链接
        /// </summary>
        public string Url { set; get; }
    }
}
