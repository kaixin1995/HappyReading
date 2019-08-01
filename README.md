## 开心阅读WPF版4.2：
去年的时候写了开心阅读Winform版，当时只是为了练手写的，没想到喜欢的人不少，所以就萌生了写一版更加完善的版本，第一版其实功能已经挺完善，但是却有一些缺陷，例如书源是写死在代码中的，一旦书源站出现无法打开，或者说信息发生改变，这种情况下，要么删除源，要么修改源，但是因为书源并非外置，用户压根就无法修改这些，再加上因为Winform写UI界面确实难搞，所以界面写的也有点不尽人意。  后来一直想要重写一个版本，这个月刚好时间比较充足，便重写了这个版本。

## 本版本的亮点：
+ 软件使用WPF编写，界面比之前有了很大的提升  
+ 书源外置，自带书源编辑器，允许用户自定义书源  
+ 加入养肥区功能，支持把书源加入养肥区，书籍养肥后会自动进行提醒  
+ 支持主题自定义，支持护眼模式、黑夜模式，并且新增加透明模式  
+ 支持文章换源  
+ 支持语音朗读书籍  
+ 支持书籍下载（测试）  
+ 支持热键快速隐藏、快速关闭  
+ 更多……
----

## 2019年07月30日更新内容：
1.增加E小说及笔趣阁C小说源站 
2.优化阅读页配置项不立即生效的BUG  
3.增加修改字体同步更改主界面外观  
4.优化数据库读取逻辑，尽可能减少数据库实时读取  

## 2019年07月23日更新内容：
1.增加另外识别书籍的方式，修复书名修改后无法更换书源的BUG  
2.智能优化网页抓取，支持utf-8、GBK等网页编码  
3.增加预读取功能，智能后面的章节  

## 2019年06月11日更新内容：
1.增加书源导入导出;   
2.优化翻页逻辑;  
3.增加语音朗读功能;  
4.增加全部书源搜索功能;  
5.去除阅读页固定大小的限制

## 2019年06月17日更新内容：
1.增加等待动画，减少因加载慢而导致的卡顿;   
2.优化线程，大幅度提高打开速度;  
3.实现阅读页大小记忆保存;  
4.实现书籍下载功能（测试）  
5.修复了若干个BUG  

----
## 界面
### 我的书架  
  ![我的书架](https://github.com/kaixin1995/HappyReading/blob/master/Image/%E6%88%91%E7%9A%84%E4%B9%A6%E6%9E%B6.png)

### 换源界面  
![换源界面](https://github.com/kaixin1995/HappyReading/blob/master/Image/%E6%8D%A2%E6%BA%90%E7%95%8C%E9%9D%A2.png)

### 加载动画  
![换源界面](https://github.com/kaixin1995/HappyReading/blob/master/Image/%E6%8D%A2%E6%BA%90%E5%8A%A8%E7%94%BB.png)

### 搜索页面
![搜索页面](https://github.com/kaixin1995/HappyReading/blob/master/Image/%E6%90%9C%E7%B4%A2%E9%A1%B5.png)  

### 设置面板
![设置面板](https://github.com/kaixin1995/HappyReading/blob/master/Image/%E8%AE%BE%E7%BD%AE%E9%9D%A2%E6%9D%BF.png)  

### 书籍详情页
![书籍详情页](https://github.com/kaixin1995/HappyReading/blob/master/Image/%E8%AF%A6%E6%83%85%E9%A1%B5.png)  

### 提醒界面
![提醒界面](https://github.com/kaixin1995/HappyReading/blob/master/Image/%E6%8F%90%E9%86%92%E7%95%8C%E9%9D%A2.png)  

### 书源管理
![书源管理](https://github.com/kaixin1995/HappyReading/blob/master/Image/%E4%B9%A6%E6%BA%90%E7%AE%A1%E7%90%86.png) 

### 阅读页
> 正常模式  
> ![正常模式](https://github.com/kaixin1995/HappyReading/blob/master/Image/%E6%AD%A3%E5%B8%B8%E6%A8%A1%E5%BC%8F.png)  
> 护眼模式  
> ![护眼模式](https://github.com/kaixin1995/HappyReading/blob/master/Image/%E6%8A%A4%E7%9C%BC%E6%A8%A1%E5%BC%8F.png)  
> 黑夜模式  
> ![黑夜模式](https://github.com/kaixin1995/HappyReading/blob/master/Image/%E9%BB%91%E5%A4%9C%E6%A8%A1%E5%BC%8F.png)  
> 透明模式（透明背景，黑字）  
> ![透明模式](https://github.com/kaixin1995/HappyReading/blob/master/Image/%E9%80%8F%E6%98%8E%E6%A8%A1%E5%BC%8F.png)  

----

## 本软件使用.net4.5进行开发，界面使用WPF，界面UI模块使用的是开源UI组件[mahapps](https://mahapps.com/)  

## 已编译安装包:[百度云](https://pan.baidu.com/s/1d5ehJGD6bJvYSYAHOyGX3A) 提取码:d4qt   | [蓝奏云](https://www.lanzous.com/i592pde) 


## 免责声明
1.本软件书籍信息全部来自于网络，本软件不会修改任何书籍信息。  
2.书籍版本属于原作者，本软件只提供数据查询服务，不提供任何贩卖和存储服务。  
3.任何通过使用本服务而搜索链接到的第三方网页均系第三方提供或制作，您可能从该第三方网页上获得资讯及享用服务，开心阅读无法对其合法性负责，亦不承担任何法律责任。  
4.因本服务搜索结果根据您键入的关键字自动搜索获得并生成，不代表开心阅读赞成被搜索链接到的第三方网页上的内容或立场。  
5.您应对使用搜索的结果自行承担风险。开心阅读不做任何形式的保证：不保证搜索结果满足您的要求，不保证搜索服务不中断，不保证搜索结果的安全性、准确性、及时性、合法性。因网络状况、通讯故障、第三方网站等任何原因而导致您不能正常使用本服务的，开心阅读不承担任何法律责任。
