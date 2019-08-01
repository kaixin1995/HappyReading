namespace HappyReading.Model
{
    class Update
    {

        /// <summary>
        /// 状态
        /// </summary>
        public int State { set; get; }

        /// <summary>
        /// 版本号
        /// </summary>

        public string Edition { set; get; }

        /// <summary>
        /// 通知
        /// </summary>

        public string Msg { get; set; }


        /// <summary>
        /// 更新日期
        /// </summary>
        public string UpdateDate { set; get; }

        /// <summary>
        /// 下载地址
        /// </summary>
        public string Download { set; get; }
    }
}
