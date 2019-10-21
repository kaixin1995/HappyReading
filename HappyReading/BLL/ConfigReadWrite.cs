using HappyReading.DAL;
using HappyReading.Model;
using System;

namespace HappyReading.BLL
{
    /// <summary>
    /// 配置读写
    /// </summary>
    class ConfigReadWrite
    {

        /// <summary>
        /// 写入配置
        /// </summary>
        /// <param name="config"></param>
        public static void SetConfig(Config config)
        {
            ConfigHelper.SetConfig("Theme", config.Theme);
            ConfigHelper.SetConfig("FontSize", config.FontSize.ToString());
            ConfigHelper.SetConfig("Typeface", config.Typeface);
            ConfigHelper.SetConfig("SourceStation", config.SourceStation);
            ConfigHelper.SetConfig("FattenNumber", config.FattenNumber.ToString());
            ConfigHelper.SetConfig("Width_height", config.Width_height);
        }

        /// <summary>
        /// 读取配置
        /// </summary>
        public static Config GetConfig()
        {
            Config config = new Config();
            config.FontSize = Convert.ToInt32(ConfigHelper.GetConfig("FontSize"));
            config.Typeface = ConfigHelper.GetConfig("Typeface");
            config.Theme = ConfigHelper.GetConfig("Theme");
            config.SourceStation= ConfigHelper.GetConfig("SourceStation");
            config.FattenNumber = Convert.ToInt32(ConfigHelper.GetConfig("FattenNumber"));
            config.Width_height = ConfigHelper.GetConfig("Width_height");
            return config;
        }
    }
}
