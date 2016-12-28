using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Web.Configuration;
namespace CDManager_DiskServices.AppClass.XML
{
    public class XMLHelper
    {
        public static string getAppSettingValue(string key)
        {
            try { return WebConfigurationManager.AppSettings[key]; }
            catch { return null; }
        }

        public static bool setAppSettingValue(string key, string newValue)
        {
            try
            { 
                //读取web.config文件
                Configuration config = WebConfigurationManager.OpenWebConfiguration("~");
                //读取appSettings节点属性
                AppSettingsSection appSet = (AppSettingsSection)config.GetSection("appSettings");
                appSet.Settings[key].Value = newValue;
                config.Save();
                return true;
            }
            catch
            { return false; }
        }
    }
}
