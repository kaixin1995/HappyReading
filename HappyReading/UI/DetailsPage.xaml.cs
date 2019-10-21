using HappyReading.BLL;
using HappyReading.Model;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace HappyReading.UI
{
    /// <summary>
    /// DetailsPage.xaml 的交互逻辑
    /// </summary>
    public partial class DetailsPage
    {
        Book book;
        MainWindow mainWindow;
        Config config;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="book"></param>
        /// <param name="mainWindow"></param>
        /// <param name="config"></param>
        public DetailsPage(Book book,MainWindow mainWindow=null,Config config=null)
        {
            InitializeComponent();

            Thread thread = new Thread(new ThreadStart(delegate
            {
                this.book = book;
                this.mainWindow = mainWindow;
                this.config = config;

                this.Dispatcher.Invoke(new Action(() =>
                {
                    this.Title = "《" + book.Name + "》-最新章节：" + book.Newest;
                    this.DataContext = book;
                    if (DataFetch.BoolBookAdd(book.Name))
                    {
                        Join.Background = Brushes.DarkTurquoise;
                        Join.Content = "已加入";
                        Join.IsEnabled = false;
                    }
                }));

                this.Dispatcher.Invoke(new Action(() =>
                {
                    buffer.Visibility = Visibility.Hidden;
                }));
            }));
            thread.IsBackground = true;  //是否为后台线程
            thread.Start();
            this.Closed+= BeforeClosing;
        }

        /// <summary>
        /// 关闭事件
        /// </summary>
        private void BeforeClosing(object sender, EventArgs e)
        {
            if (mainWindow != null&& !WindowsShow)
            {
                mainWindow.Visibility = Visibility.Visible;
                //获取所有书籍
                mainWindow.DataContext = DataFetch.GetBooks();

            }
        }

        /// <summary>
        /// 检测是否点击按钮进入其他地方
        /// 主要防止点击阅读等按钮会，会让主界面和阅读页面一起出现
        /// </summary>
        private bool WindowsShow = false;


        /// <summary>
        /// 立即阅读
        /// </summary>
        private void Read_Click(object sender, RoutedEventArgs e)
        {
            ReadingPage rp = new ReadingPage(book, mainWindow,config);
            rp.Show();
            //加入这一步，防止主界面可见度为真
            WindowsShow = true;
            this.Close();
        }

        /// <summary>
        /// 加入书架
        /// </summary>
        private void Join_Click(object sender, RoutedEventArgs e)
        {
            DataFetch.BookAdd(book);
            Join.Background = Brushes.DarkTurquoise;
            Join.Content = "已加入";
            Join.IsEnabled = false;
        }


        /// <summary>
        /// 查看目录
        /// </summary>
        private void Catalog_Click(object sender, RoutedEventArgs e)
        {
            ReadingPage rp = new ReadingPage(book, mainWindow,config);
            //rp.NewListView.Visibility = Visibility.Visible;
            rp.ListShow();
            rp.Show();
            //加入这一步，防止主界面可见度为真
            WindowsShow = true;
            this.Close();
        }
    }
}
