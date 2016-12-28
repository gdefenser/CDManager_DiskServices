using CDManager_DiskServices.AppClass.XML;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Services;

namespace CDManager_DiskServices.User
{
    /// <summary>
    /// FTPDownloadFloder 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消注释以下行。 
    // [System.Web.Script.Services.ScriptService]
    public class FTPDownloadFloder : System.Web.Services.WebService
    {

        [WebMethod]
        public bool Check()
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(XMLHelper.getAppSettingValue("FTP_Home") + "\\Download");
                if (!dir.Exists){dir.Create();}
                if (!File.Exists(XMLHelper.getAppSettingValue("FTP_Home") + "\\Download\\光盘下载文件夹,请勿增加、修改或删除任何文件和文件夹.ini"))
                { File.Create(XMLHelper.getAppSettingValue("FTP_Home") + "\\Download\\光盘下载文件夹,请勿增加、修改或删除任何文件和文件夹.ini"); }
                return true;
            }
            catch { return false; }
        }
    }
}
