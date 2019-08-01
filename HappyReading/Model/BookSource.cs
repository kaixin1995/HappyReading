namespace HappyReading.Model
{
    /// <summary>
    /// 书源信息
    /// </summary>
    public class BookSource
    {
        /// <summary>
        /// 自增ID
        /// </summary>
        public long Id { set; get; }

        /// <summary>
        /// 书源名称
        /// </summary>
        public string Title { set; get; }

        /// <summary>
        /// 书源地址
        /// </summary>
        public string Url { set; get; }

        /// <summary>
        /// 请求方式0表示get，1表示post
        /// </summary>
        public long Method { set; get; }


        /// <summary>
        /// 网站编码
        /// </summary>
        public string Code { set; get; }

        /// <summary>
        /// 搜索页地址
        /// </summary>
        public string SearchUrl { set; get; }

        /// <summary>
        /// 书籍范围截取
        /// </summary>
        public string AddressRangeRegular { set; get; }

        /// <summary>
        /// 书籍地址分隔符
        /// </summary>
        public string AddressCuttingRegular { set; get; }

        /// <summary>
        /// 书籍地址正则获取
        /// </summary>
        public string AddressRegular { set; get; }

        /// <summary>
        /// 书名正则
        /// </summary>
        public string BookNameRegular { set; get; }

        /// <summary>
        /// 作者
        /// </summary>
        public string AuthorRegular { set; get; }

        /// <summary>
        /// 更新日期
        /// </summary>
        public string UpdateRegular { set; get; }

        /// <summary>
        /// 最新章节
        /// </summary>
        public string NewestRegular { set; get; }

        /// <summary>
        /// 书籍详情
        /// </summary>
        public string DetailsRegular { set; get; }

        /// <summary>
        /// 更新状态
        /// </summary>
        public string StateRegular { set; get; }

        /// <summary>
        /// 封面图片获取
        /// </summary>
        public string ImageRegular { set; get; }

        /// <summary>
        /// 目录范围获取
        /// </summary>
        public string DirectoryScopeRegular { set; get; }

        /// <summary>
        /// 目录分隔符
        /// </summary>
        public string DirectoryCuttingRegular { set; get; }

        /// <summary>
        /// 目录标题获取
        /// </summary>
        public string DirectoryTieleRegular { set; get; }

        /// <summary>
        /// 目录链接获取
        /// </summary>
        public string DirectoryUrlRegular { set; get; }

        /// <summary>
        /// 正文标题获取
        /// </summary>
        public string ContentTitleRegular { set; get; }

        /// <summary>
        /// 正文阅读获取
        /// </summary>
        public string ContentRegular { set; get; }

        
        /// <summary>
        /// 表示是否可用，如果是1就代表可用，如果是0就代表不可用
        /// </summary>
        public long State { set; get; }

    }
}
