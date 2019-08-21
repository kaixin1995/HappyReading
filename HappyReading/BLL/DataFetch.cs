using HappyReading.DAL;
using HappyReading.Model;
using HappyReading.UI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;

namespace HappyReading.BLL
{
    public class DataFetch
    {

        /// <summary>
        /// 路径
        /// </summary>
        public static string Path = AppDomain.CurrentDomain.BaseDirectory;


        /// <summary>
        /// 软件更新
        /// </summary>
        public static void SoftwareUpdate()
        {
            try
            {
                Update update = JsonHelper.DeserializeJsonToObject<Update>(GetHtml.GetHttp("https://script.haokaikai.cn/Update/HappyReading.json"));
                if (update.State == 1)
                {
                    MessageBox.Show(update.Msg, "软件暂停使用");
                    //关闭全部窗体并退出程序使用
                    Application.Current.Shutdown();
                    return;
                }

                if (Config.Edition != update.Edition)
                {
                    MessageBox.Show(update.Msg + "\n更新日期：" + update.UpdateDate + "\n点击确定后会自动打开下载地址！", "更新提示");
                    //打开网页
                    System.Diagnostics.Process.Start(update.Download);
                }
            }
            catch (Exception ex)
            {
                Tool.TextAdditional(ex.Message);
            }
        }

        /// <summary>
        /// 判断指定表中是否存在某列
        /// </summary>
        /// <param name="table">表名</param>
        /// <param name="column">列名</param>
        /// <returns>如果存在返回true</returns>
        public static bool ColumnExistence(string table, string column)
        {
            //获取指定表的全部列信息
            DataTable dt = SQLiteDBHelper.ExecuteDataTable("PRAGMA  table_info([" + table + "])", null);
            foreach (DataRow myRow in dt.Rows)
            {
                if (myRow[1].ToString() == column) return true;
            }
            return false;
        }



        /// <summary>
        /// 获取我的书架
        /// </summary>
        /// <param name="fattening">判断是否为养肥区，默认不是</param>
        /// <returns></returns>
        public static List<Book> GetBooks(bool fattening=false)
        {
            int Fattening = fattening ? 1 : 0;

            //获取DataTable
            DataTable dt = SQLiteDBHelper.ExecuteDataTable("select *from Books where Fattening="+ Fattening, null);
            List<Book> Books= ModelConvertHelper<Book>.DataTableToList(dt);

            foreach (Book book in Books)
            {
                book.Newest = Tool.InterceptStr(book.Newest);
                book.Name= Tool.InterceptStr(book.Name);
            }
            
            //获取所有书籍
            return Books;
        }

        /// <summary>
        /// 根据链接来获取书源
        /// </summary>
        /// <param name="URL">链接</param>
        /// <returns>返回书源，如果不存在则返回null</returns>
        public static BookSource URLGetBookSource(string URL)
        {
            string[] Urls = URL.Split(new string[] { "://", "/" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (BookSource bookSource in TempData.GetBookSourceS())
            {
                if (URL.Contains(bookSource.Url) || URL.Contains(bookSource.SearchUrl)|| bookSource.Url.Contains(Urls[1])|| bookSource.SearchUrl.Contains(Urls[1]))
                {
                    return bookSource;
                }
            }

            return null;
        }


        /// <summary>
        /// 更新全部书籍
        /// </summary>
        public static void BooksUpdate(object ob)
        {
            //获取全部书籍
            DataTable dt = SQLiteDBHelper.ExecuteDataTable("select *from Books", null);
            List<Book> Books = ModelConvertHelper<Book>.DataTableToList(dt);
            foreach (Book book in Books)
            {
                //获取初始搜索文本
                string html = GetHtml.GetHttpWebRequest(book.Url);

                BookSource bs = GetBookSource(book.Source);

                //检测书源有效性
                if (bs.Title is null)
                {
                    Tips tips = new Tips(book.Name + "的书源发现异常，请检查书源是否被删除或禁用！");
                    tips.Show();
                    return;
                }
                //获取最新章节
                string LatestChapters = Tool.GetRegexStr(html, bs.NewestRegular).Trim();

                //获取更新时间
                string Update = Tool.GetRegexStr(html, bs.UpdateRegular).Trim();

                SQLiteDBHelper.ExecuteNonQuery("Update Books set 'Newest' = '"+ LatestChapters + "', 'Update' = '"+ Tool.GetUpdataDate(Update) + "' where Id =" + book.Id, null);
            }
            ((MainWindow)ob).Dispatcher.Invoke(new Action(() =>
            {
                ((MainWindow)ob).DataContext = DataFetch.GetBooks();
            }));
        }

        /// <summary>
        /// 获取全部书源
        /// </summary>
        /// <param name="State">状态1代表只获取正常的书源，如果为其他则获取全部书源</param>
        /// <returns></returns>
        public static List<BookSource> GetBookSources(int State=1)
        {
            //获取DataTable
            DataTable dt;
            if (State == 1)
            {
                //获取DataTable
                dt = SQLiteDBHelper.ExecuteDataTable("select *from BookSource where State=1", null);
            }
            else
            {
                //获取DataTable
                dt = SQLiteDBHelper.ExecuteDataTable("select *from BookSource", null);
            }
            
            //获取所有书源
            return ModelConvertHelper<BookSource>.DataTableToList(dt);
        }

        /// <summary>
        /// 更新书源状态
        /// </summary>
        public static void UpdateSourceState()
        {
            Thread thread = new Thread(new ThreadStart(delegate
            {
                //遍历源站
                foreach (BookSource bookSource in GetBookSources(0))
                {
                    //1代表网站可以访问，0代表不可以访问
                    int state = GetHtml.GetUnicom(bookSource.Url) ? 1 : 0;
                    UpdateState((int)bookSource.Id, state);
                }
                TempData.UpdateBookSourceS();
            }));
            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// 搜索书籍
        /// </summary>
        /// <param name="bookSource">书源</param>
        /// <param name="Keyword">书名关键词</param>
        /// <returns>返回搜索结果</returns>
        public static List<Book> Search(BookSource bookSource,string Keyword)
        {
            /*
            //这里先判断是否是全部书源搜索
            if (bookSource.Id == -5)
            {
                List<Book> Books = new List<Book>();
                foreach (BookSource Source in TempData.GetBookSourceS())
                {
                    try
                    {
                        if (Books.Count <= 0)
                        {
                            List<Book> T_books = Search(Source, Keyword);
                            if (T_books != null)
                            {
                                Books = T_books;
                            }
                        }
                        else
                        {
                            Books = Books.Concat(Search(Source, Keyword)).ToList<Book>();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        Tool.TextAdditional(ex.Message);
                    }
                }
                return Books;
            }*/

            //获取编码
            string Code = GetHtml.GetCode(bookSource.SearchUrl);

            //忽略大小写进行比较
            if (!Code.Equals("utf-8", StringComparison.OrdinalIgnoreCase))
            {
                if (Code.Trim().Length <= 0)
                {
                    Code = "utf-8";
                }
                //这里处理一下keyword
                Keyword = Tool.EncodingConvert(Keyword, Encoding.GetEncoding(Code));
            }
            //存放书籍的书架
            List<Book> books = new List<Book>();

            //获取初始搜索文本
            string html = GetHtml.GetHttpWebRequest(bookSource.SearchUrl + Keyword.Trim());

            //获取搜索书籍范围
            string htmlRange = Tool.GetRegexStr(html, bookSource.AddressRangeRegular).Trim();

            //分割搜索书籍
            string[] bookList = htmlRange.Split(new string[] { bookSource.AddressCuttingRegular }, StringSplitOptions.RemoveEmptyEntries);

            //遍历搜索到的书籍
            for (int i = 0; i < bookList.Length; i++)
            {
                //获取到书籍地址(对链接做出处理)
                string BookUrl = Tool.ObtainUrl(bookSource.Url, Tool.GetRegexStr(bookList[i], bookSource.AddressRegular));

                //防止因为找不到书籍而导致的错误
                if (BookUrl.Length <= 3)
                {
                    continue;
                }

                string BookHtml = GetHtml.GetHttpWebRequest(BookUrl);

                //获取书籍名称
                string BookName = Tool.GetRegexStr(BookHtml, bookSource.BookNameRegular);

                //书名都不存在，自然需要跳过本次循环
                if (BookName.Length <= 0)
                {
                    continue;
                }

                //获取作者
                string BookAuthor = Tool.GetRegexStr(BookHtml, bookSource.AuthorRegular);

                //获取更新时间
                string BookUpdate = Tool.GetUpdataDate(Tool.GetRegexStr(BookHtml, bookSource.UpdateRegular));

                //获取最新章节名称
                string BookNewest = Tool.GetRegexStr(BookHtml, bookSource.NewestRegular);

                //获取源站名称
                string BookSource = bookSource.Title;

                //封面图
                string BookImage = Tool.GetRegexStr(BookHtml, bookSource.ImageRegular);

                //更新状态
                string BookUpdateState = Tool.GetRegexStr(BookHtml, bookSource.StateRegular);

                //简介
                string BookDetails = Tool.GetRegexStr(BookHtml, bookSource.DetailsRegular);

                books.Add(new Book { Author = BookAuthor, Update = BookUpdate, Name = BookName, Source = BookSource, Newest = BookNewest, Url = BookUrl, Details = BookDetails, Image = BookImage, UpdateState = BookUpdateState });
            }

            if (books.Count <= 0)
            {
                return null;
            }
            return books;
    }


        /*
        /// <summary>
        /// 所有源站遍历搜索某书
        /// </summary>
        /// <param name="Works">书籍信息</param>
        /// <returns>返回所有源搜索结果</returns>
        public static List<Book> SourceSearch(Book Works)
        {
            string BookName = Works.Name;
            List<BookSource> bookSources = TempData.GetBookSourceS();
            //存储书的书架
            List<Book> Books = new List<Book>();
            foreach (BookSource bookSource in bookSources)
            {
                List<Book> books = Search(bookSource, BookName);
                if (books != null)
                {
                    foreach (Book book in books)
                    {
                        if (book.Name == BookName || book.Author == Works.Author && book.Name.Contains(BookName))
                        {
                            book.Id = Works.Id;
                            Books.Add(book);
                        }
                    }
                }
                
            }
            return Books;
        }*/


        /// <summary>
        /// 根据Id获取书籍的信息
        /// </summary>
        /// <param name="Id">书籍的Id</param>
        /// <returns>书籍的具体信息</returns>
        public static Book GetBook(int Id)
        {

            //获取DataTable
            DataTable dt = SQLiteDBHelper.ExecuteDataTable("select *from Books where Id=" + Id, null);

            //获取指定ID书籍的对象
            Book book = DAL.ModelConvertHelper<Book>.DataTableToModel(dt);

            //判断是否为NULL
            if (book != null && book is Book)
            {
                return book;
            }

            return null;

        }


        /// <summary>
        /// 获取书籍目录
        /// </summary>
        /// <param name="book"></param>
        public static void GetList(Book book)
        {
            BookSource bs = GetBookSource(book.Source);

            Dictionary<string, string> NewList = new Dictionary<string, string>();

            //范围
            string temp_str= Tool.GetRegexStr(GetHtml.GetHttpWebRequest(book.Url), bs.DirectoryScopeRegular);
            //分割
            string [] temp_strs= temp_str.Split(new string[] { bs.DirectoryCuttingRegular }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string tempStr in temp_strs)
            {
                string Title = Tool.GetRegexStr(tempStr, bs.DirectoryTieleRegular);
                string Url = Tool.ObtainUrl(bs.Url, Tool.GetRegexStr(tempStr, bs.DirectoryUrlRegular), book.Url);
                
                //如果标题相同就会报错，所以这里回家进行简单的处理
                if (!NewList.ContainsKey(Title)&& Title.Trim().Length>2)
                {
                    NewList.Add(Title, Url);
                }
                /* 这里加上后会重复显示
                else
                {
                    NewList.Add(Title + Guid.NewGuid(), Url);
                }*/
                
            }
            book.ListUrl = NewList;

        }

        
        /// <summary>
        /// 更换书籍的源
        /// </summary>
        /// <param name="book">书籍信息</param>
        /// <returns>返回更改结果</returns>
        public static bool UpdateBookSource(Book book)
        {
            string sql = "update Books set Url='" + book.Url + "',UpdateState = '" + book.UpdateState + "',Details = '" + book.Details + "',Author = '" + book.Author + "',Source = '" + book.Source + "',Image = '" + book.Image + "',Newest = '" + book.Newest + "','Update' = '" + book.Update + "' where Id = " + book.Id;
            return (SQLiteDBHelper.ExecuteNonQuery(sql, null) <= 0) ? false : true;
            //需要更新作者、更新日期、最新章节、书籍地址、书源信息、封面图
        }

        /// <summary>
        /// 更新阅读章节数
        /// </summary>
        /// <param name="Id">书籍ID</param>
        /// <param name="Read">阅读章节数</param>
        /// <returns></returns>
        public static bool UpdateUpdateReadingBook(int Id,int Read)
        {
            return (SQLiteDBHelper.ExecuteNonQuery("update Books set Read='"+ Read + "' where Id=" + Id, null) <= 0) ? false : true;
        }


        /// <summary>
        /// 删除指定Id的书籍
        /// </summary>
        /// <param name="Id"></param>
        /// <returns>返回删除结果</returns>
        public static bool DeleteBook(int Id)
        {
            return (SQLiteDBHelper.ExecuteNonQuery("Delete from Books where Id=" + Id, null) <= 0) ? false : true;
        }


        /// <summary>
        /// 删除书源
        /// </summary>
        /// <param name="Id">书源ID</param>
        /// <returns>返回删除结果</returns>
        public static bool DeleteBookSource(int Id)
        {
            return (SQLiteDBHelper.ExecuteNonQuery("Delete from BookSource where Id=" + Id, null) <= 0) ? false : true;
        }



        /// <summary>
        /// 根据书源名获取书源对象
        /// </summary>
        /// <param name="SourceName">书源名</param>
        /// <returns>返回书源对象</returns>
        public static BookSource GetBookSource(string SourceName)
        {
            //获取DataTable
            DataTable dt = SQLiteDBHelper.ExecuteDataTable("select *from BookSource where Title='" + SourceName+ "' and State!=0", null);

            //获取指定ID书籍的对象
            BookSource source = DAL.ModelConvertHelper<BookSource>.DataTableToModel(dt);

            return source;
        }


        /// <summary>
        /// 获取目录数
        /// </summary>
        /// <param name="book">书籍对象</param>
        /// <returns>返回目录数</returns>
        public static int GetListCount(Book book)
        {
            //获取初始搜索文本
            string html = GetHtml.GetHttpWebRequest(book.Url);

            BookSource bs = GetBookSource(book.Source);

            //获取书籍范围
            string htmlRange = Tool.GetRegexStr(html, bs.DirectoryScopeRegular).Trim();

            //分割章节
            string[] bookList = htmlRange.Split(new string[] { bs.DirectoryCuttingRegular}, StringSplitOptions.RemoveEmptyEntries);

            return bookList.Length;
        }

        /// <summary>
        /// 更新书源状态
        /// </summary>
        /// <param name="id">书源ID</param>
        /// <param name="state">书源状态</param>
        /// <returns></returns>
        public static bool UpdateState(int id, int state)
        {
            string sql = "update BookSource set State="+state+" where Id="+id;
            return (SQLiteDBHelper.ExecuteNonQuery(sql, null) <= 0) ? false : true;
        }


        /// <summary>
        /// 加入或者移出养肥区
        /// </summary>
        /// <param name="id">书籍ID</param>
        /// <param name="state">true为加入养肥区</param>
        /// <returns></returns>
        public static bool Add_MoveOut(int id,bool state=false)
        {
            
            int fatten = state ? 1 : 0;
            string sql = "update Books set Fattening=" + fatten + " where Id=" + id;
            return (SQLiteDBHelper.ExecuteNonQuery(sql, null) <= 0) ? false : true;
        }


        /// <summary>
        /// 增加书源
        /// </summary>
        /// <param name="bookSource">书源内容</param>
        /// <returns>返回结果</returns>
        public static bool SourceAdd(BookSource bookSource)
        {
            //增加之前先判断是否已经存在该书源

            int count= Convert.ToInt32(SQLiteDBHelper.ExecuteScalar("select count(*) as count from BookSource where Url='" + bookSource.Url + "' or Title='" + bookSource.Title + "'", "count"));

            if (count > 0)
            {
                return false;
            }
            
            
            string sql= "INSERT INTO BookSource('Title','Url','SearchUrl','AddressRangeRegular','AddressCuttingRegular','AddressRegular','BookNameRegular','AuthorRegular','UpdateRegular','NewestRegular','DetailsRegular','StateRegular','DirectoryScopeRegular','DirectoryCuttingRegular','DirectoryTieleRegular','DirectoryUrlRegular','ContentTitleRegular','ContentRegular','ImageRegular','State') VALUES('" + bookSource.Title+"', '"+bookSource.Url+"', '"+bookSource.SearchUrl+"', '"+bookSource.AddressRangeRegular+"', '"+bookSource.AddressCuttingRegular+"', '"+bookSource.AddressRegular+ "','" + bookSource.BookNameRegular + "', '" + bookSource.AuthorRegular+"', '"+bookSource.UpdateRegular+"', '"+bookSource.NewestRegular+"', '"+bookSource.DetailsRegular+"', '"+bookSource.StateRegular+"', '"+bookSource.DirectoryScopeRegular+"', '"+bookSource.DirectoryCuttingRegular+"', '"+bookSource.DirectoryTieleRegular+"', '"+bookSource.DirectoryUrlRegular+"', '"+bookSource.ContentTitleRegular+"', '"+bookSource.ContentRegular+"', '"+bookSource.ImageRegular+"'," +bookSource.State+")";

            TempData.UpdateBookSourceS();

            return (SQLiteDBHelper.ExecuteNonQuery(sql, null) <= 0) ? false : true;
        }


        /// <summary>
        /// 更新书源
        /// </summary>
        /// <param name="bookSource"></param>
        /// <returns></returns>
        public static bool SourceUpdate(BookSource bookSource)
        {
            string sql = "update BookSource set  'Title'='" + bookSource.Title + "','Url'='" + bookSource.Url + "','SearchUrl'='" + bookSource.SearchUrl + "','AddressRangeRegular'='"+ bookSource.AddressRangeRegular + "','AddressCuttingRegular'='" + bookSource.AddressCuttingRegular + "','AddressRegular'='" + bookSource.AddressRegular + "','BookNameRegular'='" + bookSource.BookNameRegular + "','AuthorRegular'='" + bookSource.AuthorRegular + "','UpdateRegular'='" + bookSource.UpdateRegular + "','NewestRegular'='" + bookSource.NewestRegular + "','DetailsRegular'='" + bookSource.DetailsRegular + "','StateRegular'='" + bookSource.StateRegular + "','DirectoryScopeRegular'='" + bookSource.DirectoryScopeRegular + "','DirectoryCuttingRegular'='" + bookSource.DirectoryCuttingRegular + "','DirectoryTieleRegular'='" + bookSource.DirectoryTieleRegular + "','DirectoryUrlRegular'='" + bookSource.DirectoryUrlRegular + "','ContentTitleRegular'='" + bookSource.ContentTitleRegular + "','ContentRegular'='"+ bookSource.ContentRegular + "','ImageRegular'='"+ bookSource.ImageRegular + "' where Id="+ bookSource.Id;
            //TempData.UpdateBookSourceS();
            return (SQLiteDBHelper.ExecuteNonQuery(sql, null) <= 0) ? false : true;
        }

        /// <summary>
        /// 添加书籍
        /// </summary>
        /// <param name="book">书籍对象</param>
        /// <returns>返回添加的结果</returns>
        public static bool BookAdd(Book book)
        {
            if (BoolBookAdd(book.Name))
            {
                Tips tips = new Tips("《" + book.Name + "》已经添加到书架，请勿重复添加，谢谢合作！");
                tips.Show();
                return false;
            }

            string sql= "INSERT INTO Books('Name','Url','UpdateState','Details','Author','Source','Image','Newest','Update','Read','ReadChapter') VALUES('"
                + book.Name+"', '"
                +book.Url+"', '"
                +book.UpdateState + "', '"
                +book.Details + "', '"
                +book.Author+"', '"
                +book.Source+"', '"
                +book.Image+"', '"
                +book.Newest+"', '"
                +book.Update+"',0, '')";
            return (SQLiteDBHelper.ExecuteNonQuery(sql, null) <= 0) ? false : true;
        }

        /// <summary>
        /// 书架中是否存在该书籍
        /// </summary>
        /// <param name="BookName">书籍名称</param>
        /// <returns>返回查询结果</returns>
        public static bool BoolBookAdd(string BookName)
        {
            string Sql = "select count(*) as 次数 from Books where Name='"+BookName+"'";
            return (Convert.ToInt32(SQLiteDBHelper.ExecuteScalar(Sql, "次数")) > 0) ? true : false;
        }
    }
}
