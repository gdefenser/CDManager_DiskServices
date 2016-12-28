using CDManager_DiskServices.AppClass.Core;
using CDManager_DiskServices.AppClass.FTP.Serv_uAdvCon;
using CDManager_DiskServices.AppClass.XML;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Services;

namespace CDManager_DiskServices.CD
{
    /// <summary>
    /// CDFile 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消注释以下行。 
    // [System.Web.Script.Services.ScriptService]
    public class CDFile : System.Web.Services.WebService
    {

        [WebMethod]//文件长度
        public string GetFile(string glytm, string isbn, string ztm, string cdxh)
        {
            List<FileInfo> listFile = new List<FileInfo>();
            try
            {
                Serv_UControl sc = Serv_UControl.getServUContorl();
                string homedir = sc.GetFtpUserHomeDir(glytm).Replace("\r", "");//获取管理员目录
                string path = XMLHelper.getAppSettingValue("FTP_Home") + "\\Download\\" + CDString.getFileName(isbn + ztm) + "\\" + cdxh.Replace(" ", "_") + "\\";
                DirectoryInfo dir = new DirectoryInfo(path);
                listFile = dir.GetFiles().ToList();//获取FTP服务器文件列表
                FileInfo file = listFile.FirstOrDefault();
                return file.Length + "," + file.Extension;
            }
            catch { return null; }
        }

        [WebMethod]//移除文件
        public bool RemoveFile(string glytm, string isbn, string ztm, string cdxh)
        {
            try
            {
                Serv_UControl sc = Serv_UControl.getServUContorl();
                string homedir = sc.GetFtpUserHomeDir(glytm).Replace("\r", "");//获取管理员目录
                string path = XMLHelper.getAppSettingValue("FTP_Home") + "\\Download\\" + isbn.Replace(" ", "_") + CDString.getFileName(ztm) + "\\" + cdxh.Replace(" ", "_");
                DirectoryInfo dir = new DirectoryInfo(path);
                FileInfo file = dir.GetFiles().FirstOrDefault();//获取FTP服务器文件列表
                if (file != null)
                {
                    file.Delete();//删除文件
                }
                if (dir.GetFiles().Count() < 1)
                {
                    dir.Delete();//删除目录
                }
                return true;
            }
            catch { return false; }
        }

        [WebMethod]//上传确认
        public string UploadConfirm(string glytm, string isbn, string ztm, string cdxh)
        {
            try
            {
                Serv_UControl sc = Serv_UControl.getServUContorl();
                return sc.MoveFile(glytm, isbn, ztm, cdxh);
            }
            catch { return "确认错误!"; }
        }

        [WebMethod]//更改文件名
        public bool UpdateFileName(string isbn, string ztm, string cdxh, string cdmc)
        {
            try
            {
                string path = XMLHelper.getAppSettingValue("FTP_Home") + "\\Download\\" + CDString.getFileName(isbn + ztm) + "\\" + cdxh.Replace(" ", "_");
                DirectoryInfo dir = new DirectoryInfo(path);
                List<FileInfo> listFile = dir.GetFiles().ToList();

                if (listFile.Count == 1)
                {
                    FileInfo file = listFile.First();
                    string new_path = XMLHelper.getAppSettingValue("FTP_Home") + "\\Download\\" + CDString.getFileName(isbn + ztm) + "\\" + cdxh;
                    if (!Directory.Exists(new_path)) { Directory.CreateDirectory(new_path); }
                    File.Move(file.FullName, new_path + "\\" + CDString.getFileName(cdmc) + file.Extension);
                    listFile = dir.GetFiles().ToList();
                    if (listFile.Count == 0)
                    {
                        Directory.Delete(path);
                        return true;
                    }
                    else { return false; }
                }
                else { return false; }
            }
            catch { return false; }
        }

        [WebMethod]//获取文件名
        public string GetFileName(string isbn, string ztm, string cdxh)
        {
            try
            {
                string path = XMLHelper.getAppSettingValue("FTP_Home") + "\\Download\\" + CDString.getFileName(isbn + ztm) + "\\" + CDString.getFileName(cdxh);
                DirectoryInfo dir = new DirectoryInfo(path);

                if (dir.Exists)
                {
                    return dir.GetFiles().First().Name;
                }
                else { return null; }
            }
            catch { return null; }
        }

        [WebMethod]//获取文件未确认列表
        public List<string> GetUploadedFiles(string glytm)
        {
            string path = XMLHelper.getAppSettingValue("FTP_Home") + "\\" + glytm;
            DirectoryInfo dir = new DirectoryInfo(path);
            List<string> listResult = new List<string>();

            foreach (FileInfo fi in dir.GetFiles().ToList())
            {
                string extension = fi.Extension;
                if (extension != ".txt" && extension != ".ini")
                {
                    string item = fi.Name.Split('.')[0] + "," + extension;
                    double length = fi.Length / 1024.0 / 1024.0;
                    if (length >= 1024) { item += "," + (length / 1024).ToString("0.00") + "Gb"; }
                    else { item += "," + length.ToString("0.00") + "Mb"; }
                    item += "," + fi.LastAccessTime;
                    listResult.Add(item);
                }
            }
            return listResult;
        }

        [WebMethod]//移除未确认文件
        public bool RemoveUploadedFile(string glytm, string name)
        {
            try
            {
                Serv_UControl sc = Serv_UControl.getServUContorl();
                string path = sc.GetFtpUserHomeDir(glytm).Replace("\r", "");//获取管理员目录
                FileInfo file = new DirectoryInfo(path).GetFiles().First(f => f.Name == name);
                file.Delete();
                return true;
            }
            catch { return false; }
        }

        [WebMethod]//更改未确认文件
        public bool UpdateUploadedFile(string glytm, string name, string newName, string ext)
        {
            try
            {
                Serv_UControl sc = Serv_UControl.getServUContorl();
                string path = sc.GetFtpUserHomeDir(glytm).Replace("\r", "");//获取管理员目录
                FileInfo file = new DirectoryInfo(path).GetFiles().ToList().First(f => f.Name == name + ext);
                file.MoveTo(path + "\\" + newName + ext);
                return true;
            }
            catch { return false; }
        }
    }
}
