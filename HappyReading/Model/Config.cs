using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HappyReading.Model
{
    /// <summary>
    /// 配置项文件
    /// </summary>
    public class Config
    {

        /// <summary>
        /// 返回主题
        /// </summary>
        public List<string> GetTheme { get { return new List<string> { "正常模式", "护眼模式", "黑夜模式", "透明模式" }; } }

        /// <summary>
        /// 返回可选的字体大小
        /// </summary>
        public List<int> GetTypefaceSize { get { return new List<int> { 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30 }; } }

        /// <summary>
        /// 返回可选养肥区章节数
        /// </summary>
        public List<int> GetFattenNumber { get { return new List<int> {10,15,30,50,75,80,100,150,180 }; } }


        /// <summary>
        /// 默认书源
        /// </summary>
        public string SourceStation { set; get; }

        /// <summary>
        /// 字体
        /// </summary>
        public string Typeface { set; get; }

        /// <summary>
        /// 阅读主题
        /// </summary>
        public string Theme { set; get; }

        /// <summary>
        /// 字体大小
        /// </summary>
        public int FontSize { set; get; }


        /// <summary>
        /// 养肥区章节数
        /// </summary>
        public int FattenNumber { set; get; }

        /// <summary>
        /// 当前软件版本
        /// </summary>
        public static string Edition = "4.3";


        /// <summary>
        /// 记录阅读页的宽高
        /// </summary>
        public string Width_height { set; get; }
    }
}
