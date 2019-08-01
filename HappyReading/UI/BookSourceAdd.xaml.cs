using HappyReading.BLL;
using HappyReading.DAL;
using HappyReading.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HappyReading.UI
{
    /// <summary>
    /// BookSourceAdd.xaml 的交互逻辑
    /// </summary>
    public partial class BookSourceAdd
    {
        MainWindow mainWindow;

        public BookSourceAdd()
        {
            InitializeComponent();
            //加载书源
            ListSource.ItemsSource = DataFetch.GetBookSources();
        }


        public BookSourceAdd(MainWindow mainWindow)
        {
            InitializeComponent();
            //加载书源
            ListSource.ItemsSource = DataFetch.GetBookSources();

            this.Closed += BeforeClosing;
            this.mainWindow = mainWindow;
        }


        /// <summary>
        /// 关闭前
        /// </summary>
        private void BeforeClosing(object sender, EventArgs e)
        {
            mainWindow.Visibility = Visibility.Visible;
            mainWindow.RefreshData();
        }

        /// <summary>
        /// 搜索状态
        /// </summary>
        private bool state_search = false;

        /// <summary>
        /// 书籍详情获取状态
        /// </summary>
        private bool state_details = false;

        /// <summary>
        /// 目录获取状态
        /// </summary>
        private bool state_Catalog = false;

        /// <summary>
        /// 正文获取状态
        /// </summary>
        private bool state_Text = false;

        /// <summary>
        /// 搜索按钮点击
        /// </summary>
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            SearchBook searchBook = new SearchBook();
            //搜索链接拼凑
            searchBook.BookUrl= SearchUrl.Text + SearchBookName.Text;
            searchBook.AddressRangeRegular = AddressRangeRegular.Text;
            searchBook.AddressCuttingRegular = AddressCuttingRegular.Text;
            searchBook.AddressRegular = AddressRegular.Text;


            Thread td = new Thread(SearchBook);
            //把线程设置为后台线程
            td.IsBackground = true;
            td.Start(searchBook);
        }

        private void SearchBook(object Search_Book)
        {
            SearchBook searchBook = (SearchBook)Search_Book;
            string html = GetHtml.GetHttpWebRequest(searchBook.BookUrl);
            List<Book> books = new List<Book>();

            //获取搜索书籍范围
            string htmlRange = Tool.GetRegexStr(html, searchBook.AddressRangeRegular).Trim();

            //分割搜索书籍
            string[] bookList = htmlRange.Split(new string[] { searchBook.AddressCuttingRegular }, StringSplitOptions.RemoveEmptyEntries);

            int i = 0;
            foreach (string str in bookList)
            {
                string url= Tool.GetRegexStr(str, searchBook.AddressRegular).Trim();
                if (url.Length > 3)
                {
                    i++;
                    books.Add(new Book { Id=i,Url= url });
                }
            }

            //Invoke是同步  BeginInvoke是异步
            Dispatcher.Invoke((Action)delegate
            {
                SearchList.ItemsSource = books;
            });

            if (books.Count > 0)
            {
                state_search = true;
            }
        }


        /// <summary>
        /// 链接处理
        /// </summary>
        private void LinkProcessing_Click(object sender, RoutedEventArgs e)
        {
            Book book = SearchList.SelectedItem as Book;
            if (book != null && book is Book)
            {
                if (((MenuItem)sender).Tag.ToString() == "copy")
                {
                    Clipboard.SetDataObject(book.Url);
                }
                else
                {
                    System.Diagnostics.Process.Start(book.Url);
                }
            }
        }


        /// <summary>
        /// 获取书籍内容
        /// </summary>
        private void ObtainBook_Click(object sender, RoutedEventArgs e)
        {
            Book book = new Book();
            string html= GetHtml.GetHttpWebRequest(Book_url.Text.Trim());

            book.Name= Tool.GetRegexStr(html, BookNameRegular.Text).Trim();
            book.UpdateState= Tool.GetRegexStr(html, StateRegular.Text).Trim();
            book.Newest= Tool.GetRegexStr(html, NewestRegular.Text).Trim();
            book.Update= Tool.GetRegexStr(html, UpdateRegular.Text).Trim(); 
            book.Image= Tool.GetRegexStr(html, ImageRegular.Text).Trim(); 
            book.Author= Tool.GetRegexStr(html, AuthorRegular.Text).Trim();
            book.Details= Tool.GetRegexStr(html, DetailsRegular.Text).Trim();
            //获取所有书籍
            this.DataContext = book;
            if (book.Name.Length > 0 && book.Details.Length > 0)
            {
                state_details = true;
            }
        }


        /// <summary>
        /// 获取目录
        /// </summary>
        private void GetDirectory_Click(object sender, RoutedEventArgs e)
        {
            string url = List_url.Text.Trim();

            //获取范围化的文本
            string html = Tool.GetRegexStr(GetHtml.GetHttpWebRequest(url), DirectoryScopeRegular.Text).Trim();

            //得到被分割的目录
            string[] List =html.Split(new string[] { DirectoryCuttingRegular.Text }, StringSplitOptions.RemoveEmptyEntries);


            //这个是处理的目录
            List<GetDirectory> NewList = new List<GetDirectory>();

            int Newid = 0;

            foreach (string chapter in List)
            {
                string NewUrl= Tool.GetRegexStr(chapter, DirectoryUrlRegular.Text).Trim();
                if (NewUrl.Length > 3)
                {
                    Newid++;
                    string NewTitle = Tool.GetRegexStr(chapter, DirectoryTieleRegular.Text).Trim();
                    NewList.Add(new GetDirectory() { Id = Newid, Title = NewTitle, Url = NewUrl });
                }

            }

            Catalog.ItemsSource = NewList;
            if (NewList.Count > 1)
            {
                state_Catalog = true;
            }
            Tips tips = new Tips("目录获取完毕~");
            tips.Show();
        }


        /// <summary>
        /// 正文获取
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextObtain_Click(object sender, RoutedEventArgs e)
        {
            string url = TextLink.Text;
            //获取范围化的文本
            string html = GetHtml.GetHttpWebRequest(url);

            string Text= Tool.GetRegexStr(html, ContentRegular.Text).Trim();

            string Title = Tool.GetRegexStr(html, ContentTitleRegular.Text).Trim();

            TextContent.Content = Title;
            New_Text.Text = Tool.HtmlFilter(Text);
            if (Text.Length > 10)
            {
                state_Text = true;
            }
        }


        /// <summary>
        /// 保存&更新书源
        /// </summary>
        private void Keep_Click(object sender, RoutedEventArgs e)
        {
            BookSource bookSource = new BookSource();
            bookSource.Title = sourceName.Text.Trim();
            bookSource.Url = sourceUrl.Text.Trim();
            bookSource.SearchUrl = SearchUrl.Text;
            bookSource.AddressRangeRegular = AddressRangeRegular.Text.Replace("'", "''");
            bookSource.AddressCuttingRegular = AddressCuttingRegular.Text.Replace("'", "''");
            bookSource.AddressRegular = AddressRegular.Text.Replace("'", "''");
            bookSource.BookNameRegular = BookNameRegular.Text.Replace("'", "''");
            bookSource.AuthorRegular = AuthorRegular.Text.Replace("'", "''");
            bookSource.UpdateRegular = UpdateRegular.Text.Replace("'", "''");
            bookSource.NewestRegular = NewestRegular.Text.Replace("'", "''");
            bookSource.DetailsRegular = DetailsRegular.Text.Replace("'", "''");
            bookSource.StateRegular = StateRegular.Text.Replace("'", "''");
            bookSource.DirectoryScopeRegular = DirectoryScopeRegular.Text.Replace("'", "''");
            bookSource.DirectoryCuttingRegular = DirectoryCuttingRegular.Text.Replace("'", "''");
            bookSource.DirectoryTieleRegular = DirectoryTieleRegular.Text.Replace("'", "''");
            bookSource.DirectoryUrlRegular = DirectoryUrlRegular.Text.Replace("'", "''");
            bookSource.ContentTitleRegular = ContentTitleRegular.Text.Replace("'", "''");
            bookSource.ContentRegular = ContentRegular.Text.Replace("'", "''");
            bookSource.ImageRegular = ImageRegular.Text.Replace("'", "''");
            bookSource.State = 1;
            

            //这个是更新原有源
            if (((Button)sender).Content.ToString() != "保存当前书源")
            {
                int id = Convert.ToInt32(Keep.Tag);
                bookSource.Id = id;
                string Msg = DataFetch.SourceUpdate(bookSource) ? "更新成功~" : "更新失败！";
                Tips tips = new Tips(Msg);
                tips.Show();
                //加载书源
                ListSource.ItemsSource = DataFetch.GetBookSources();
            }
            else //这个是增加新源
            {
                if (state_search && state_details && state_Catalog && state_Text)
                {
                    if (sourceName.Text.Trim().Length > 0 && sourceName.Text != "请输入新书源名~" && sourceUrl.Text.Trim().Length > 0 && sourceUrl.Text != "请输入新书源URL~")
                    {
                        string Msg = DataFetch.SourceAdd(bookSource) ? "添加成功~" : "添加失败！";
                        new Tips(Msg).Show();
                        
                    }
                    else
                    {
                        new Tips("请输入书源名和书源链接！").Show();
                    }
                }
                else
                {
                    new Tips("请将其他页面的内容全部填写完毕，并保证准确无误后在进行提交！").Show();
                }
            }

            //加载书源
            ListSource.ItemsSource = DataFetch.GetBookSources();
            TempData.UpdateBookSourceS();
            Empty();
            
        }


        /// <summary>
        /// 清空所有的痕迹
        /// </summary>
        private void Empty()
        {
            sourceName.Text = string.Empty;
            sourceUrl.Text = string.Empty;
            SearchUrl.Text = string.Empty;
            AddressRangeRegular.Text = string.Empty;
            AddressCuttingRegular.Text = string.Empty;
            AddressRegular.Text = string.Empty;
            BookNameRegular.Text = string.Empty;
            AuthorRegular.Text = string.Empty;
            UpdateRegular.Text = string.Empty;
            NewestRegular.Text = string.Empty;
            DetailsRegular.Text = string.Empty;
            StateRegular.Text = string.Empty;
            DirectoryScopeRegular.Text = string.Empty;
            DirectoryCuttingRegular.Text = string.Empty;
            DirectoryTieleRegular.Text = string.Empty;
            DirectoryUrlRegular.Text = string.Empty;
            ContentTitleRegular.Text = string.Empty;
            ContentRegular.Text = string.Empty;
            ImageRegular.Text = string.Empty;
            Keep.Content = "保存当前书源";
            Catalog.ItemsSource = null;
            this.DataContext = null;
            SearchList.ItemsSource = null;
        }


        /// <summary>
        /// 删除书源
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            BookSource  bookSource = ListSource.SelectedItem as BookSource;
            if (bookSource != null && bookSource is BookSource)
            {
                if (MessageBox.Show("确定要删除书源【" + bookSource.Title + "】？ 本操作不可以逆转，请谨慎操作~", "提示", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
                {
                    DataFetch.DeleteBookSource((int)bookSource.Id);
                    //加载书源
                    ListSource.ItemsSource = DataFetch.GetBookSources();
                    TempData.UpdateBookSourceS();
                }
            }
        }


        /// <summary>
        /// 编辑书源
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            BookSource bookSource = ListSource.SelectedItem as BookSource;
            if (bookSource != null && bookSource is BookSource)
            {
                sourceName.Text = bookSource.Title;
                sourceUrl.Text = bookSource.Url;
                SearchUrl.Text = bookSource.SearchUrl;
                AddressRangeRegular.Text = bookSource.AddressRangeRegular;
                AddressCuttingRegular.Text = bookSource.AddressCuttingRegular;
                AddressRegular.Text = bookSource.AddressRegular;
                BookNameRegular.Text = bookSource.BookNameRegular;
                AuthorRegular.Text = bookSource.AuthorRegular;
                UpdateRegular.Text = bookSource.UpdateRegular;
                NewestRegular.Text = bookSource.NewestRegular;
                DetailsRegular.Text = bookSource.DetailsRegular;
                StateRegular.Text = bookSource.StateRegular;
                DirectoryScopeRegular.Text = bookSource.DirectoryScopeRegular;
                DirectoryCuttingRegular.Text = bookSource.DirectoryCuttingRegular;
                DirectoryTieleRegular.Text = bookSource.DirectoryTieleRegular;
                DirectoryUrlRegular.Text = bookSource.DirectoryUrlRegular;
                ContentTitleRegular.Text = bookSource.ContentTitleRegular;
                ContentRegular.Text = bookSource.ContentRegular;
                ImageRegular.Text = bookSource.ImageRegular;

                //识别更新还是增加
                Keep.Content = "更新当前书源";
                Keep.Tag = bookSource.Id;
            }
        }


        /// <summary>
        /// 检测书源可用率
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Testing_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("状态码如果为1代表书源站正常访问，如果为0代表不可访问","提示");
            Thread thread = new Thread(new ThreadStart(delegate
            {
                //遍历源站
                foreach (BookSource bookSource in TempData.GetBookSourceS())
                {
                    //1代表网站可以访问，0代表不可以访问
                    int state = GetHtml.GetUnicom(bookSource.Url) ? 1 : 0;
                    DataFetch.UpdateState((int)bookSource.Id, state);
                }

                this.Dispatcher.Invoke(new Action(() =>
                {
                    //加载书源
                    ListSource.ItemsSource = DataFetch.GetBookSources();
                    TempData.UpdateBookSourceS();
                }));
            }));
            thread.IsBackground = true;
            thread.Start();

           
        }

        #region 清空默认文字
        /// <summary>
        /// 清空默认文字
        /// </summary>
        private void SourceName_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sourceName.Text == "请输入新书源名~")
            {
                sourceName.Text = string.Empty;
            }
        }

        /// <summary>
        /// 清空默认文字
        /// </summary>
        private void SourceUrl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sourceUrl.Text == "请输入新书源URL~")
            {
                sourceUrl.Text = string.Empty;
            }
        }
        #endregion


        /// <summary>
        /// 导入书源
        /// </summary>
        private void Import_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "BookSource (*.json)|*.json"
            };

            var result = openFileDialog.ShowDialog();
            if (result == true)
            {
                try
                {
                    StreamReader sr = new StreamReader(openFileDialog.FileName, Encoding.UTF8);
                    var Book_Source = JsonHelper.DeserializeJsonToObject<BookSource>(sr.ReadToEnd());
                    sr.Close();
                    if (Book_Source.Title.Length > 1 && Book_Source.Url.Length > 1)
                    {
                        string Msg = DataFetch.SourceAdd(Book_Source) ? "添加成功~" : "添加失败！";
                        new Tips(Msg).Show();
                        //加载书源
                        ListSource.ItemsSource = DataFetch.GetBookSources();
                        TempData.UpdateBookSourceS();
                    }
                    else
                    {
                        new Tips("错误的书源").Show();
                    }
                }
                catch (Exception ex)
                {
                    new Tips("书源异常:"+ex.Message).Show();
                }
                
               
            }
        }


        /// <summary>
        /// 导出书源
        /// </summary>
        private void Export_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BookSource bookSource = ListSource.SelectedItem as BookSource;
                if (bookSource != null && bookSource is BookSource)
                {
                    Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                    dlg.FileName = bookSource.Title;
                    dlg.DefaultExt = ".json"; // Default file extension
                    dlg.Filter = "BookSource (.json)|*.json"; // Filter files by extension

                    // Show save file dialog box
                    Nullable<bool> result = dlg.ShowDialog();

                    // Process save file dialog box results
                    if (result == true)
                    {
                        // Save document
                        string filename = dlg.FileName;
                        using (StreamWriter sw = new StreamWriter(filename))
                        {
                            sw.Write(JsonHelper.SerializeObject(bookSource));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                new Tips(ex.Message).Show();
            }
        }
    }
}
