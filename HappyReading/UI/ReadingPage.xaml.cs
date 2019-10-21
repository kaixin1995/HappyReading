using HappyReading.BLL;
using HappyReading.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Speech.Synthesis;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace HappyReading.UI
{
    /// <summary>
    /// ReadingPage.xaml 的交互逻辑
    /// </summary>
    public partial class ReadingPage : Window
    {
        private Book book;
        private MainWindow mainWindow;
        private BookSource bookSource;
        /// <summary>
        /// 获取配置项
        /// </summary>
        private Config config;

        /// <summary>
        /// 当前章节数
        /// </summary>
        private int NewPages = 0;

        /// <summary>
        /// 目录的序号
        /// </summary>
        private Dictionary<int, string> NewId = new Dictionary<int, string>();


        /// <summary>
        /// 缓存的本地章节数据
        /// </summary>
        private Dictionary<int, string> NewText = new Dictionary<int, string>();

        /// <summary>
        /// 构造函数入口
        /// </summary>
        /// <param name="book">书籍信息</param>
        /// <param name="mainWindow">主窗口对象</param>
        /// <param name="config">配置项</param>
        public ReadingPage(Book book,MainWindow mainWindow=null,Config config=null)
        {
            InitializeComponent();

            Thread thread = new Thread(new ThreadStart(delegate
            {
                try
                {
                    this.book = book;
                    this.mainWindow = mainWindow;
                    this.config = config;
                    //获取目录
                    DataFetch.GetList(book);
                    bookSource = DataFetch.GetBookSource(book.Source);

                    //主要为遍历目录
                    int i = 0;
                    foreach (KeyValuePair<string, string> kvp in book.ListUrl)
                    {
                        if (kvp.Key.Trim().Length > 2)
                        {
                            i++;
                            NewId.Add(i, kvp.Key);
                        }
                    }
                    Dispatcher.Invoke((Action)delegate
                    {
                        if (book.Read == 0)
                        {
                            Jump(1);
                        }
                        else
                        {
                            Jump((int)book.Read);
                        }
                    });
                }
                catch (Exception ex)
                {
                    Tool.TextAdditional(ex.Message);
                }
            }));
            thread.IsBackground = true;  //是否为后台线程
            thread.Start();
            
            this.Closed += BeforeClosing;
            //newText.IsEnabled = false;

            //窗口大小改变时
            this.SizeChanged += new SizeChangedEventHandler(ReadingPage_Resize);

            //设置定时器（朗读）         
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(10000000);   //时间间隔为一秒
            timer.Tick += new EventHandler(timer_Tick);
        }

        /// <summary>
        /// 窗口大小改变时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReadingPage_Resize(object sender, SizeChangedEventArgs e)
        {
            //这里也要实时修改控件所处的相对位置
            img.Margin = new Thickness(this.Width-img.Width-10, 5, 6, this.Height- img.Height-5);

            //标题
            newTitle.Margin = new Thickness(10, 5, 0, 0);
            if (this.Width != 470 || this.Height != 650)
            {
                //记忆保存此次设置的大小
                config.Width_height = this.Width + "," + this.Height;
                ConfigReadWrite.SetConfig(config);
            }
            
        }

        /// <summary>
        /// 加载配置项
        /// </summary>
        private void LoadConfiguration()
        {
            if (config != null)
            {
                newText.FontFamily = new FontFamily(config.Typeface);
                string [] Size= config.Width_height.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                if (Size.Length == 2)
                {
                    this.Width = Convert.ToInt32(Size[0]);
                    this.Height = Convert.ToInt32(Size[1]);
                }
                newText.FontSize = config.FontSize;
                switch (config.Theme)
                {
                    case "正常模式":
                        {
                            theme.Background  = new SolidColorBrush(Colors.White);
                            newText.Foreground = new SolidColorBrush(Colors.Black);//设置字体颜色
                            newTitle.Foreground = new SolidColorBrush(Colors.Black);//设置字体颜色
                            //调整透明度
                            theme.Opacity = 1;
                            Frame.Opacity = 1;
                            break;
                        }
                    case "护眼模式":
                        {
                            theme.Background=(Brush)new BrushConverter().ConvertFrom("#C4ECCB");
                            newText.Foreground = new SolidColorBrush(Colors.Black);//设置字体颜色
                            newTitle.Foreground = new SolidColorBrush(Colors.Black);//设置字体颜色
                            //调整透明度
                            theme.Opacity = 1;
                            Frame.Opacity = 1;
                            break;
                        }
                    case "黑夜模式":
                        {
                            //DCDCDC
                            theme.Background = (Brush)new BrushConverter().ConvertFrom("#1E1E1E");
                            newText.Foreground = (Brush)new BrushConverter().ConvertFrom("#DCDCDC");
                            newTitle.Foreground = (Brush)new BrushConverter().ConvertFrom("#DCDCDC");
                            //调整透明度
                            theme.Opacity = 1;
                            Frame.Opacity = 1;
                            break;
                        }
                    case "透明模式":
                        {
                            newText.Foreground = new SolidColorBrush(Colors.Black);//设置字体颜色
                            newTitle.Foreground = new SolidColorBrush(Colors.Black);//设置字体颜色
                            //调整透明度
                            theme.Opacity = 0.1;
                            Frame.Opacity = 0.1;


                            break;
                        }
                }
            }
        }

        /// <summary>
        /// 关闭前需要做的事情
        /// </summary>
        private void BeforeClosing(object sender, EventArgs e)
        {
            try
            {
                if (mainWindow != null)
                {
                    mainWindow.Visibility = Visibility.Visible;
                }
            }
            catch
            {

            }
            //释放所有语音资源
            voice.Dispose();  
        }

        /// <summary>
        /// 无边框移动
        /// </summary>
        private void Window_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        /// <summary>
        /// 记录上次位置  用来智能判断下一页
        /// </summary>
        //double LastPosition = 0;

        /// <summary>
        /// 方向键 
        /// 190716修改 暂时去除下一页到底时自动切换下一章功能    以及到顶回退上一章的功能 
        /// </summary>
        private void Direction_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            //滚动位置
            double position = newText.VerticalOffset;

            if (e.Key == Key.Up|| e.Key == Key.W)//上一页 
            {
                this.newText.ScrollToVerticalOffset(position - (this.Height - 100));
            }
            else if (e.Key == Key.Down|| e.Key == Key.S)//下一页
            {
                this.newText.ScrollToVerticalOffset(position + (this.Height - 100));
            }
            else if (e.Key == Key.Left || e.Key == Key.A)//上一章节
            {
                if (NewId.Count > 1 && NewPages > 1)
                {
                    Jump(NewPages - 1);
                }
                else
                {
                    new Tips("无上一章节").Show();
                }
            }
            else if (e.Key == Key.Right || e.Key == Key.D)//下一章节
            {
                if (NewPages < NewId.Count)
                {
                    Jump(NewPages + 1);
                }
                else
                {
                    new Tips("无下一章节").Show();
                }
            }
            else if (e.Key == Key.Enter)//返回目录
            {
                ListShow();
            }



            /*
            switch (e.Key)
            {
                case Key.Up:
                    {
                        //上一页  
                        //SendKeys.SendWait("{PGUP}"); //模拟按键输入
                        this.newText.ScrollToVerticalOffset(position - (this.Height - 100));
                        
                        if (position == 0)
                        {
                            if (NewId.Count > 1 && NewPages > 1)
                            {
                                Jump(NewPages - 1);
                            }
                            else
                            {
                                new Tips("无上一章节").Show();
                            }

                        }
                        break;
                    }
                case Key.Down:
                    {
                        //下一页
                        //SendKeys.SendWait("{PGDN}"); //模拟按键输入
                        
                        if (LastPosition == position && position != 0)
                        {
                            if (NewPages < NewId.Count)
                            {
                                Jump(NewPages + 1);
                            }
                            else
                            {
                                new Tips("无下一章节").Show();
                            }
                        }
                        else
                        {
                            LastPosition = position;
                            this.newText.ScrollToVerticalOffset(position + (this.Height - 100));
                        }
                        
                        this.newText.ScrollToVerticalOffset(position + (this.Height - 100));
                        break;
                    }
                case Key.Left:
                    {
                        if (NewId.Count > 1 && NewPages > 1)
                        {
                            Jump(NewPages - 1);
                        }
                        else
                        {
                            new Tips("无上一章节").Show();
                        }
                        break;
                    }
                case Key.Right:
                    {
                        if (NewPages < NewId.Count)
                        {
                            Jump(NewPages + 1);
                        }
                        else
                        {
                            new Tips("无下一章节").Show();
                        }
                        break;
                    }
                    //这个是回车
                case Key.Enter:
                    {
                        ListShow();
                        break;
                    }
            }*/
        }

        /// <summary>
        /// 点击关闭
        /// </summary>
        private void Img_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }




        /// <summary>
        /// 返回主页
        /// </summary>
        private void Homepage_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        /// <summary>
        /// 查看目录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViewDirectory_Click(object sender, RoutedEventArgs e)
        {
            ListShow();
        }


        /// <summary>
        /// 显示目录
        /// </summary>
        public void ListShow()
        {
            Thread thread = new Thread(new ThreadStart(delegate
            {
                //跳转到指定的项
                KeyValuePair<int, string> keyValuePair = new KeyValuePair<int, string>(NewPages, NewId[NewPages]);

                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    //目录赋值
                    NewListView.ItemsSource = NewId;
                    NewListView.Visibility = Visibility.Visible;

                    //跳转
                    NewListView.ScrollIntoView(keyValuePair);
                }));
            }));
            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// 上一章
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpperChapter_Click(object sender, RoutedEventArgs e)
        {
            if (NewId.Count>1&& NewPages>0)
            {
                Jump(NewPages - 1);
            }
            else
            {
                new Tips("无上一章节").Show();
            }
        }

        /// <summary>
        /// 下一章
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NextChapter_Click(object sender, RoutedEventArgs e)
        {
            if (NewPages < NewId.Count)
            {
                Jump(NewPages + 1);
            }
            else
            {
                new Tips("无下一章节").Show();
            }
        }

        


        /// <summary>
        /// 主题保存
        /// </summary>
        private void ThemePreservation(object sender, RoutedEventArgs e)
        {
            switch (((MenuItem)sender).Name)
            {
                case "Default":
                    {
                        config.Theme = "正常模式";
                        break;
                    }
                case "Eyehelp":
                    {
                        config.Theme = "护眼模式";
                        break;
                    }
                case "Night":
                    {
                        config.Theme = "黑夜模式";
                        break;
                    }
                case "transparent":
                    {
                        config.Theme = "透明模式";
                        break;
                    }
            }
            LoadConfiguration();
            ConfigReadWrite.SetConfig(config);
        }
        
        /// <summary>
        /// 双击目录事件
        /// </summary>
        private void NewListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            KeyValuePair<int, string>? keyValuePair = NewListView.SelectedItem as KeyValuePair<int, string>?;

            if (keyValuePair != null && keyValuePair is KeyValuePair<int, string>?)
            {
                int NewId = ((KeyValuePair<int, string>)keyValuePair).Key;
                Jump(NewId);
                NewListView.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 跳转到直接章节
        /// </summary>
        /// <param name="page">章节数</param>
        /// <param name="cache">是否缓存  如果是true，代表只是缓存，前台阅读页并不会因此而改变</param>
        private void Jump(int page,bool cache=false)
        {
            if (NewId.Count< page)
            {
                //这里是为了禁止读取不存在的章节
                return;
            }

            try
            {
                //文章html
                string html = string.Empty;
                //章节的标题和文本
                string Title = string.Empty;
                string Text = string.Empty;
                if(!cache) buffer.Visibility = Visibility.Visible;

                Thread thread = new Thread(new ThreadStart(delegate
                {
                    Title = NewId[page];

                    //判断本章节是否缓存
                    if (NewText.ContainsKey(page))
                    {
                        Text = NewText[page];
                    }
                    else
                    {
                        html = GetHtml.GetHttpWebRequest(book.ListUrl[NewId[page]]);
                        Text= Tool.GetRegexStr(html, bookSource.ContentRegular);
                        NewText[page] = Text;
                    }
                    if (!cache)
                    {
                        Dispatcher.BeginInvoke((Action)delegate
                        {
                            newTitle.Text = Title;
                            newText.Document.Blocks.Clear();
                            Run run = new Run(Tool.HtmlFilter(Text));
                            Paragraph p = new Paragraph();
                            p.Inlines.Add(run);
                            newText.Document.Blocks.Add(p);
                            ///重置翻页数
                            this.newText.ScrollToVerticalOffset(0);
                            buffer.Visibility = Visibility.Hidden;
                            //保存已阅章节数
                            if (book.Id != 0)
                            {
                                DataFetch.UpdateUpdateReadingBook((int)book.Id, page);
                            }
                            NewPages = page;

                            //这里是缓存下面的章节
                            for (int i = 1; i < 10; i++)
                            {
                                Jump(page + i, true);
                            }
                        });
                    }
                }));
                thread.IsBackground = true;  //是否为后台线程
                thread.Start();
            }
            catch(Exception ex)
            {
                new Tips("打开错误，请检查书源是否存在问题？").Show();
                Tool.TextAdditional(ex.Message);
                buffer.Visibility = Visibility.Hidden;
            }
        }
        

        /// <summary>
        /// 退出目录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExitDirectory_Click(object sender, RoutedEventArgs e)
        {
            NewListView.Visibility = Visibility.Hidden;
        }


        /// <summary>
        /// 双击下一章节
        /// </summary>
        private void NewText_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //取消被选中状态

            if (NewPages < NewId.Count)
            {
                Jump(NewPages+1);
            }
        }

        /// <summary>
        /// 热键标识
        /// </summary>
        int Identification;

        int Pause;

        int BeginAndEnd;

        /// <summary>
        /// 初始化加载
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //关闭图片
            img.Source = new BitmapImage(new Uri(BLL.DataFetch.Path + "Resources/Close_click.png"));
            HwndSource hWndSource, hPause, hBeginAndEnd;
            WindowInteropHelper wih = new WindowInteropHelper(this);
            hWndSource = HwndSource.FromHwnd(wih.Handle);
            hPause = HwndSource.FromHwnd(wih.Handle);
            hBeginAndEnd = HwndSource.FromHwnd(wih.Handle);
            //添加处理程序 
            hWndSource.AddHook(MainWindowProc);
            hPause.AddHook(MainWindowProc);
            hBeginAndEnd.AddHook(MainWindowProc);
            Identification = HotKey.GlobalAddAtom("Alt-D");
            //暂停播放
            Pause= HotKey.GlobalAddAtom("Alt-Q");
            //停止开始
            BeginAndEnd= HotKey.GlobalAddAtom("Alt-W");

            HotKey.RegisterHotKey(wih.Handle, Identification, HotKey.KeyModifiers.Alt, (int)System.Windows.Forms.Keys.D);
            HotKey.RegisterHotKey(wih.Handle, Pause, HotKey.KeyModifiers.Alt, (int)System.Windows.Forms.Keys.Q);
            HotKey.RegisterHotKey(wih.Handle, BeginAndEnd, HotKey.KeyModifiers.Alt, (int)System.Windows.Forms.Keys.W);

            //判断配置项是否为null
            if (config == null)
            {
                
                config = ConfigReadWrite.GetConfig();
            }
            //记载配置项
            LoadConfiguration();


        }

        /// <summary>
        /// 热键
        /// </summary>
        private IntPtr MainWindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case HotKey.WM_HOTKEY:
                    {
                        int sid = wParam.ToInt32();
                        if (sid == Identification)
                        {
                            if (this.Visibility == Visibility.Collapsed)
                            {
                                this.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                this.Visibility = Visibility.Collapsed;
                            }
                        }
                        else if (sid == Pause)//暂停&播放
                        {
                            if (voice.State.ToString().Trim() == "Ready")
                            {
                                ReadingAloud();
                                new Tips("开始朗读").Show();
                            }
                            else if (voice.State.ToString().Trim() == "Paused")
                            {
                                voice.Resume();
                                new Tips("暂停朗读").Show();
                                
                            }
                            else
                            {
                                voice.Pause();
                                new Tips("继续朗读").Show();
                            }
                        }
                        else if (sid == BeginAndEnd)//停止&开始
                        {
                            //如果时钟已经启动，那么二次点击就意味着停止
                            if (timer.IsEnabled)
                            {
                                timer.Stop();
                                voice.SpeakAsyncCancelAll();  //取消朗读
                                new Tips("停止朗读").Show();
                            }
                            else
                            {
                                new Tips("开始朗读").Show();
                                ReadingAloud();
                            }
                        }
                        handled = true;
                        break;
                    }
            }
            return IntPtr.Zero;
        }



        /// <summary>
        /// 鼠标进入关闭按钮时
        /// </summary>
        private void Img_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            //变色关闭图片
            img.Source = new BitmapImage(new Uri(BLL.DataFetch.Path + "Resources/Close_initial.png"));
        }

        /// <summary>
        /// 鼠标离开关闭按钮时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Img_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            //关闭图片
            img.Source = new BitmapImage(new Uri(BLL.DataFetch.Path + "Resources/Close_click.png"));
        }

        /// <summary>
        /// 创建语音实例
        /// </summary>
        SpeechSynthesizer voice = new SpeechSynthesizer();


        /// <summary>
        /// 创建定时器
        /// </summary>
        private DispatcherTimer timer;

        /// <summary>
        /// 朗读听书
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReadingAloud_Click(object sender, RoutedEventArgs e)
        {
            //如果时钟已经启动，那么二次点击就意味着停止
            if (timer.IsEnabled)
            {
                timer.Stop();
                voice.SpeakAsyncCancelAll();  //取消朗读
                return;
            }

            ReadingAloud();
        }

        /// <summary>
        /// 开始朗读
        /// </summary>
        private void ReadingAloud()
        {
            voice.Rate = -1; //设置语速,[-10,10]
            voice.Volume = 100; //设置音量,[0,100]
            TextRange txt = new TextRange(newText.Document.ContentStart, newText.Document.ContentEnd);
            voice.SpeakAsync(txt.Text);  //播放指定的字符串,这是异步朗读

            if (!timer.IsEnabled)
            {
                //开启定时器          
                timer.Start();
            }
            
        }

        /// <summary>
        /// 判断是否处于朗读中
        /// </summary>
        private void timer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (voice.State.ToString().Trim() != "Speaking")
                {
                    if (NewPages < NewId.Count)
                    {
                        Jump(NewPages + 1);
                        ReadingAloud();
                    }
                    else
                    {
                        timer.Stop();
                        new Tips("播放完毕").Show();
                    }
                }
            }
            catch 
            {

            }
        }


        /// <summary>
        /// 双击隐藏标题
        /// </summary>
        private void NewTitle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //鼠标双击判定
            if (e.ClickCount == 2)
            {
                newTitle.Visibility = Visibility.Collapsed;
            }
            
        }



        /// <summary>
        /// 下载书籍
        /// </summary>
        private void Download_Click(object sender, RoutedEventArgs e)
        {
            Thread thread = new Thread(new ThreadStart(delegate
            {
                List<string> Text = new List<string>();
                foreach (KeyValuePair<string, string> keyValuePair in book.ListUrl)
                {
                    string html = GetHtml.GetHttpWebRequest(keyValuePair.Value);
                    Text.Add(keyValuePair.Key + "\r\n" + Tool.HtmlFilter(Tool.GetRegexStr(html, bookSource.ContentRegular)));
                }
                this.Dispatcher.Invoke(new Action(() =>
                {
                    Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                    dlg.FileName = book.Name;
                    dlg.DefaultExt = ".txt"; 
                    dlg.Filter = "BookSource (.txt)|*.txt"; 
                    
                    Nullable<bool> result = dlg.ShowDialog();

                    // Process save file dialog box results
                    if (result == true)
                    {
                        // Save document
                        string filename = dlg.FileName;
                        using (StreamWriter sw = new StreamWriter(filename))
                        {
                            sw.Write(string.Join("\r\n", Text));
                        }
                    }
                }));
            }));
            thread.IsBackground = true;  //是否为后台线程
            thread.Start();
           
        }
    }
}
