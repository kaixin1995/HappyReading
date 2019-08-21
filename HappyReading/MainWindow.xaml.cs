using HappyReading.BLL;
using HappyReading.DAL;
using HappyReading.Model;
using HappyReading.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace HappyReading
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {

            InitializeComponent();

            //190731增加，这里新增加一个判断，判断是否存在指定列，如果不存在将进行增加列操作
            if (!DataFetch.ColumnExistence("BookSource", "Code")) SQLiteDBHelper.ExecuteDataTable("ALTER TABLE BookSource ADD COLUMN Code Text; ", null);

            string path = AppDomain.CurrentDomain.BaseDirectory;
            //this.Icon = BitmapFrame.Create(new Uri(path + "Resources/favicon.ico"));

            //获取配置项
            config = ConfigReadWrite.GetConfig();

            about.Text = "本软件只是娱乐之作，所有数据来源皆来自于网络，如果有侵犯到他人的权益，请于我进行联系，我会第一时间删除源站，谢谢合作！\n\n本软件默认有五个源，用户可以自定义增加更多的源站，如果你有更好的源站，可以推荐给我，我会第一时间集成进去。\n\n本软件初始发布于吾爱破解，如果你在使用的过程中有发现BUG或者其他不合理的地方，请进行留言。本软件只作用于学习研究，请在下载内24小时内删除本软件。";

            //字体
            Typeface.ItemsSource = Tool.GetTypeface();
            Typeface.Text = config.Typeface;

            //绑定字体大小
            TypefaceSize.ItemsSource = config.GetTypefaceSize;
            TypefaceSize.Text = config.FontSize.ToString();

            //绑定主题
            theme.ItemsSource = config.GetTheme;
            theme.Text = config.Theme;

            //更新书源状态
            DataFetch.UpdateSourceState();

            //刷新数据
            RefreshData();

            //绑定书源
            SourceStation.ItemsSource = lb;
            SourceStation.Text = config.SourceStation;


            //默认显示养肥区
            FertilizingArea.Visibility = Visibility.Visible;
            fatten.IsEnabled = true;
            //绑定养肥区
            fatten.ItemsSource = config.GetFattenNumber;
            fatten.Text = config.FattenNumber.ToString();
            GetFatten();
        }
        

        /// <summary>
        /// 配置项类
        /// </summary>
        Config config;

        /// <summary>
        /// 创建一堆书源的对象
        /// </summary>
        List<BookSource> lb;


        /// <summary>
        /// 刷新数据
        /// </summary>
        public void RefreshData()
        {
            //添加书源
            lb = DataFetch.GetBookSources();
            lb.Add(new BookSource { Id = -5, Title = "全部搜索" });

            //绑定书源
            BookSourceName.ItemsSource = lb;
            BookSourceName.Text = config.SourceStation;

            //获取所有书籍
            this.DataContext = DataFetch.GetBooks();

            //启动时更新书籍
            Update();

            //启动时更新养肥区
            UpdateFatten();
        }

        /// <summary>
        /// 删除书籍
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Delete_Click(object sender, RoutedEventArgs e)
        {

            //获取书籍ID
            int Id = Convert.ToInt32(((MenuItem)sender).Tag);
            Book book = DataFetch.GetBook(Id);

            if (MessageBox.Show("确定要删除《" + book.Name + "》？ 本操作不可以逆转，请谨慎操作~", "提示", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
            {
                DataFetch.DeleteBook(Id);
                this.DataContext = DataFetch.GetBooks();
            }
        }


        /// <summary>
        /// 更新书籍
        /// </summary>
        private void Updata_Click(object sender, RoutedEventArgs e)
        {
            Update();
            //养肥区更新
            UpdateFatten();
        }


        /// <summary>
        /// 更新书籍具体实现
        /// </summary>
        private void Update()
        {
            Thread UpdateBook = new Thread(DataFetch.BooksUpdate);
            UpdateBook.IsBackground = true;
            UpdateBook.Start(this);
        }


        /// <summary>
        /// 阅读书籍
        /// </summary>
        private void Read_Click(object sender, RoutedEventArgs e)
        {
            int Newid = Convert.ToInt32(((MenuItem)sender).Tag);
            Book book = DataFetch.GetBook(Newid);
            UI.ReadingPage page = new UI.ReadingPage(book,this,config);
            page.Show();
            this.Visibility = Visibility.Hidden;

        }

        /// <summary>
        /// 更换书源
        /// </summary>
        private void Replace_Click(object sender, RoutedEventArgs e)
        {
            bookshelf.Visibility = Visibility.Hidden;
            SourceChange.Visibility = Visibility.Visible;
            buffer.Visibility = Visibility.Visible;
            int Newid = Convert.ToInt32(((MenuItem)sender).Tag);
            Book Works = DataFetch.GetBook(Newid);
            SourceChange.ItemsSource = null;

            Thread thread = new Thread(new ThreadStart(delegate
            {
                string BookName = Works.Name;
                List<BookSource> bookSources = TempData.GetBookSourceS();
                //存储书的书架
                List<Book> Books = new List<Book>();
                foreach (BookSource bookSource in bookSources)
                {
                    List<Book> books = DataFetch.Search(bookSource, BookName);
                    if (books != null)
                    {
                        foreach (Book book in books)
                        {
                            if (book.Name == BookName || book.Author == Works.Author && book.Name.Contains(BookName))
                            {
                                book.Id = Works.Id;
                                this.Dispatcher.Invoke(new Action(() =>
                                {
                                    if (SourceChange.Visibility == Visibility.Collapsed)
                                    {
                                        //必须加这个，否则无法修改任何值
                                        SourceChange.ItemsSource = null;
                                        SourceChange.Items.Clear();
                                        return;
                                    }
                                    else
                                    {
                                        foreach (var de in SourceChange.Items)
                                        {
                                            if (((Book)de).Source == book.Source)
                                            {
                                                continue; 
                                            }
                                        }
                                        SourceChange.Items.Add(book);
                                    }
                                    
                                }));
                            }
                        }
                    }

                }
                this.Dispatcher.Invoke(new Action(() =>
                {
                    buffer.Visibility = Visibility.Hidden;
                }));
            }));
            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// 查看书籍
        /// </summary>
        private void CheckBooks_Click(object sender, RoutedEventArgs e)
        {
            int Newid = Convert.ToInt32(((MenuItem)sender).Tag);
            Book book = DataFetch.GetBook(Newid);
            UI.DetailsPage  dp = new UI.DetailsPage(book, this,config);
            dp.Show();
            this.Visibility = Visibility.Hidden;
        }

        #region 搜索按钮

        /// <summary>
        /// 搜索点击
        /// </summary>
        private void SearchClick()
        {
            //书名
            string Keyword = BookName.Text.Trim();

            //搜索字符不得为NULL
            if (Keyword.Length < 1)
            {
                return;
            }
            //必须加这个，否则无法修改任何值
            SearchList.ItemsSource = null;
            SearchList.Items.Clear();
            search.Content = "搜索中~";
            SearchBuffer.Visibility = Visibility.Visible;
            search.IsEnabled = false;

            //查看选择的哪个书源
            int id = BookSourceName.SelectedIndex;
            try
            {
                //全部搜索和其他的不同，故此重写
                if (lb[id].Id == -5)
                {
                    Thread thread = new Thread(new ThreadStart(delegate
                    {
                        foreach (BookSource Source in TempData.GetBookSourceS())
                        {
                            try
                            {
                                List<Book> T_books = DataFetch.Search(Source, Keyword);
                                if (T_books != null)
                                {
                                    Dispatcher.Invoke(new Action(() =>
                                    {
                                        foreach (Book book in T_books)
                                        {
                                            SearchList.Items.Add(book);
                                        }

                                    }));
                                }
                            }
                            catch (Exception ex)
                            {
                                
                                Tool.TextAdditional(ex.Message);
                            }
                        }
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            search.Content = "搜索";
                            SearchBuffer.Visibility = Visibility.Hidden;
                            search.IsEnabled = true;
                        }));
                    }));
                    thread.IsBackground = true;
                    thread.Start();
                }
                else
                {
                    Thread thread = new Thread(new ThreadStart(delegate
                    {
                        List<Book> books = DataFetch.Search(lb[id], Keyword);
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            if (books != null)
                            {
                                SearchList.ItemsSource = books;
                            }
                            search.Content = "搜索";
                            SearchBuffer.Visibility = Visibility.Hidden;
                            search.IsEnabled = true;
                        }));
                    }));
                    thread.IsBackground = true;
                    thread.Start();
                }

                
            }
            catch (Exception ex)
            {
                new Tips(ex.Message).Show();
                Tool.TextAdditional(ex.Message);
                search.Content = "搜索";
                SearchBuffer.Visibility = Visibility.Hidden;
                search.IsEnabled = true;
            }
        }
        /// <summary>
        /// 搜索
        /// </summary>
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            SearchClick();
        } 
        #endregion


        /// <summary>
        /// 清空搜索
        /// </summary>
        private void Empty_Click(object sender, RoutedEventArgs e)
        {
            SearchList.ItemsSource = null;
            BookName.Text = string.Empty;
        }


        /// <summary>
        /// 加入书架
        /// </summary>
        private void JoinBookshelves_Click(object sender, RoutedEventArgs e)
        {
            Book book = SearchList.SelectedItem as Book;
            if (book != null && book is Book)
            {
                if (DataFetch.BookAdd(book))
                {
                    new Tips("添加成功！").Show();
                    //获取所有书籍
                    this.DataContext = DataFetch.GetBooks();
                }
            }
        }


        /// <summary>
        /// 查看详情
        /// </summary>
        private void ViewDetails_Click(object sender, RoutedEventArgs e)
        {
            Book book = SearchList.SelectedItem as Book;
            if (book != null && book is Book)
            {
                UI.DetailsPage ud = new UI.DetailsPage(book,this,config);
                ud.Show();
                this.Visibility = Visibility.Collapsed;
            }
        }


        /// <summary>
        /// 双击事件
        /// </summary>
        private void SearchList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Book book = SearchList.SelectedItem as Book;
            if (book != null && book is Book)
            {
                UI.DetailsPage ud = new UI.DetailsPage(book,this,config);
                ud.Show();
                this.Visibility = Visibility.Hidden;
            }
        }


        /// <summary>
        /// 立即阅读
        /// </summary>
        private void Read_Click_1(object sender, RoutedEventArgs e)
        {
            Book book = SearchList.SelectedItem as Book;
            if (book != null && book is Book)
            {
                UI.ReadingPage rp = new UI.ReadingPage(book,this,config);
                rp.Show();
                this.Visibility = Visibility.Hidden;
            }
        }


        /// <summary>
        /// 点击打开阅读页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewBook_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left&& e.ClickCount == 2)
            {
                int Newid = Convert.ToInt32(((Grid)sender).Tag);
                Book book = DataFetch.GetBook(Newid);
                UI.ReadingPage page = new UI.ReadingPage(book, this,config);
                page.Show();
                this.Visibility = Visibility.Collapsed;
            }
            
        }

        /// <summary>
        /// 保存配置项
        /// </summary>
        private void SaveConfiguration(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;

            if (comboBox.SelectedItem.ToString().Length > 0)
            {
                var msg = comboBox.SelectedItem;
                switch (comboBox.Name)
                {
                    case "Typeface":
                        {
                            config.Typeface = msg.ToString();//about
                            this.FontFamily= new FontFamily(config.Typeface);
                            break;
                        }
                    case "TypefaceSize":
                        {
                            config.FontSize = Convert.ToInt32(msg);
                            break;
                        }
                    case "theme":
                        {
                            config.Theme = msg.ToString();
                            break;
                        }
                    case "SourceStation":
                        {
                            if (msg is BookSource)
                            {
                                config.SourceStation = ((BookSource)msg).Title;
                            }
                            break;
                        }
                    case "fatten":
                        {
                            config.FattenNumber = Convert.ToInt32(msg);
                            break;
                        }
                }
                ConfigReadWrite.SetConfig(config);
            }
        }


        /// <summary>
        /// 退出换源
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void QuitSource_Click(object sender, RoutedEventArgs e)
        {
            //清空已有的部分数据
            SourceChange.ItemsSource = null;
            SourceChange.Items.Clear();
            SourceChange.Visibility = Visibility.Collapsed;
            bookshelf.Visibility = Visibility.Visible;
            buffer.Visibility = Visibility.Hidden;
        }


        /// <summary>
        /// 确定更换书源
        /// </summary>
        private void SureSource_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show(((MenuItem)sender).Tag.ToString());
            //清空已有的部分数据
            SourceChange.ItemsSource = null;
            SourceChange.Items.Clear();
            Book book = SourceChange.SelectedItem as Book;
            if (book != null && book is Book)
            {
                if (!DataFetch.UpdateBookSource(book))
                {
                    new Tips("更换失败~").Show();
                }
                else
                {
                    //更换书籍后，重新刷新全部书籍信息
                    //获取所有书籍
                    this.DataContext = DataFetch.GetBooks();

                    //启动时更新书籍
                    Update();
                }
                SourceChange.Visibility = Visibility.Collapsed;
                bookshelf.Visibility = Visibility.Visible;
            }

        }

        /// <summary>
        /// 书源管理
        /// </summary>
        private void SourceManagement_Click(object sender, RoutedEventArgs e)
        {
            UI.BookSourceAdd bookSourceAdd = new UI.BookSourceAdd(this);
            bookSourceAdd.Show();
            this.Visibility = Visibility.Collapsed;
        }


        /// <summary>
        /// 显示或者隐藏养肥区
        /// </summary>
        private void BoolFertilizingArea_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)BoolFertilizingArea.IsChecked)
            {
                FertilizingArea.Visibility = Visibility.Visible;
                fatten.IsEnabled = true;
                //绑定养肥区
                fatten.ItemsSource = config.GetFattenNumber;
                fatten.Text = config.FattenNumber.ToString();
                GetFatten();
            }
            else
            {
                FertilizingArea.Visibility = Visibility.Collapsed;
                fatten.IsEnabled = false;
            }
        }

        /// <summary>
        /// 获取养肥区
        /// </summary>
        void GetFatten()
        {
            List<Book> books = DataFetch.GetBooks(true);
            FattenArea.ItemsSource = null;
            FattenArea.ItemsSource = books;

            UpdateFatten();

        }


        /// <summary>
        /// 养肥区更新
        /// </summary>
        private void UpdateFatten()
        {
            Thread thread = new Thread(new ThreadStart(delegate
            {
                List<Book> books = DataFetch.GetBooks(true);
                foreach (Book book in books)
                {
                    book.FattenNumber = DataFetch.GetListCount(book) - (int)book.Read;
                    if (book.FattenNumber >= config.FattenNumber)
                    {
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            new Tips("《" + book.Name + "》已经养肥，请尽情享用~").Show();
                        }));
                    }
                }

                this.Dispatcher.Invoke(new Action(() =>
                {
                    if ((bool)BoolFertilizingArea.IsChecked)
                    {
                        FattenArea.ItemsSource = null;
                       FattenArea.ItemsSource = books;
                    }
                }));
            }));
            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// 加入养肥区
        /// </summary>
        private void ItemFatten_Click(object sender, RoutedEventArgs e)
        {
            int id = Convert.ToInt32(((MenuItem)sender).Tag);
            DataFetch.Add_MoveOut(id, true);
            //获取所有书籍
            this.DataContext = DataFetch.GetBooks();

            if (FertilizingArea.Visibility == Visibility.Visible)
            {
                //更新养肥区界面
                GetFatten();
            }
        }


        /// <summary>
        /// 移出养肥区
        /// </summary>
        private void MoveOut_Click(object sender, RoutedEventArgs e)
        {
            Book book = FattenArea.SelectedItem as Book;
            if (book != null && book is Book)
            {
                DataFetch.Add_MoveOut((int)book.Id, false);
                //获取所有书籍
                this.DataContext = DataFetch.GetBooks();
                GetFatten();
            }
            
        }

        /// <summary>
        /// 热键标识
        /// </summary>
        int Identification;

        /// <summary>
        /// 窗口打开时执行的
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            HwndSource hWndSource;
            WindowInteropHelper wih = new WindowInteropHelper(this);
            hWndSource = HwndSource.FromHwnd(wih.Handle);
            //添加处理程序 
            hWndSource.AddHook(MainWindowProc);
            Identification = HotKey.GlobalAddAtom("Shift-S");
            HotKey.RegisterHotKey(wih.Handle, Identification, HotKey.KeyModifiers.Shift, (int)System.Windows.Forms.Keys.S);

            //软件更新
            DataFetch.SoftwareUpdate();
        }
        private IntPtr MainWindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case HotKey.WM_HOTKEY:
                    {
                        int sid = wParam.ToInt32();
                        if (sid == Identification)
                        {
                            Application.Current.Shutdown();
                        }
                        handled = true;
                        break;
                    }
            }
            return IntPtr.Zero;
        }

        /// <summary>
        /// 打开源码下载地址
        /// </summary>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //打开网页
            System.Diagnostics.Process.Start("https://github.com/kaixin1995/HappyReading/");
        }

       
    }
}
