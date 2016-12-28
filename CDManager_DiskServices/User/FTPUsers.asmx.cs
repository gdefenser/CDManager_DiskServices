using CDManager_DiskServices.AppClass.FTP.Serv_uAdvCon;
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
    /// CheckFTPUsers 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消注释以下行。 
    // [System.Web.Script.Services.ScriptService]
    public class FTPUsers : System.Web.Services.WebService
    {
        Serv_UControl sc = Serv_UControl.getServUContorl();
        [WebMethod]//检查FTP用户账号
        public bool Admin(string glytm, string mm)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(XMLHelper.getAppSettingValue("FTP_Home") + "\\" + glytm);
                if (!dir.Exists) { dir.Create(); }

                if (!sc.IsExistUser(glytm)) { sc.CreateUser(glytm, mm, glytm); }

                if (!File.Exists(XMLHelper.getAppSettingValue("FTP_Home") + "\\" + glytm + "\\上传的光盘文件命名请包含正确的ISBN和光盘条码.ini"))
                { File.Create(XMLHelper.getAppSettingValue("FTP_Home") + "\\" + glytm + "\\上传的光盘文件命名请包含正确的ISBN和光盘条码.ini"); }

                return true;
            }
            catch { return false; }
        }

        [WebMethod]//获取传输并发数
        public string GetMaxUser()
        {
            return sc.GetFtpMaxUsersCount("reader").Replace("\r", "").Replace("\n", "");
        }

        [WebMethod]//设置传输并发数
        public bool SetMaxUser(string user, string new_max)
        {
            bool result = false;
            try
            {
                string max = sc.GetFtpMaxUsersCount("reader");              
                if (new_max != max)
                {
                    if (sc.SetFtpMaxUsersCount("reader", new_max)) { result = true; }
                }
            }
            catch { }
            return result;
        }

        [WebMethod]//更改FTP用户密码
        public bool UpdatePasswd(string glytm,string mm)
        {
            return sc.ChangePassword(glytm, mm);
        }

        [WebMethod]//新建FTP用户
        public bool NewUser(string glytm, string mm)
        {
            return sc.CreateUser(glytm, mm, glytm, "WLP");
        }

        [WebMethod]//删除FTP用户
        public bool DeleteUser(string glytm)
        {
            return sc.DeleteUser(glytm);
        }
    }
}
