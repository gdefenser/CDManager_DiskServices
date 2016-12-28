using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CDManager_DiskServices.AppClass.FTP.Serv_uControl;
using System.Collections;
using System.Text.RegularExpressions;
using System.IO;
using System.Web.Configuration;
using CDManager_DiskServices.AppClass.Core;
using CDManager_DiskServices.AppClass.XML;

namespace CDManager_DiskServices.AppClass.FTP.Serv_uAdvCon
{
    public class Serv_UControl
    {
        public string IniLoc;
        private iniControl con;
        private volatile static Serv_UControl servu;
        private static readonly object lockHelper = new object();

        public Serv_UControl(string iniloc)
        {
            this.IniLoc = iniloc;
            con = new iniControl(IniLoc);
        }

        public static Serv_UControl getServUContorl()
        {
            if (servu == null)
            {
                //给实例化操作加上线程互斥锁
                lock (lockHelper)
                {
                    if (servu == null)
                    {
                        string ini = XMLHelper.getAppSettingValue("FTP_Server") + "\\ServUDaemon.ini";
                        servu = new Serv_UControl(ini);
                    }
                }
            }
            return servu;
        }

        /// <summary>
        /// 修改账户密码
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="newpassword">密码</param>
        /// <returns></returns>
        public bool ChangePassword(string username, string newpassword)
        {
            FtpUserInfo userinfo = con.FtpUserList[username];
            if (userinfo != null)
            {
                userinfo.Password = newpassword;
                con.SaveIni();
                return true;
            }
            return false;
        }
        /// <summary>
        /// 创建账户
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="HomeDir">主目录</param>
        /// <param name="Access">权限</param>
        /// <returns></returns>
        public bool CreateUser(string username, string password, string HomeDir, string Access = "WLP"/*默认为WLP，具体写法请参见serv-u配置文件说明*/)
        {
            if (!con.FtpUserList.ContainsKey(username))
            {
                HomeDir = XMLHelper.getAppSettingValue("FTP_Home") +"\\"+ HomeDir;
                if (!Directory.Exists(HomeDir)) { Directory.CreateDirectory(HomeDir); }
                FtpUserInfo userinfo = new FtpUserInfo();
                userinfo.UserName = username;
                userinfo.Password = password;
                PropertyControl pc = userinfo.PropertyList;
                pc.Add("TimeOut", "600");
                pc.Add("RelPaths", "1");
                pc.Add("MaxUsersLoginPerIP", "1");
                pc.Add("HomeDir", HomeDir);
                pc.Add("MaxNrUsers", "1");
                pc.Add("Access1", HomeDir + "|" + Access);
                con.FtpUserList.Add(username, userinfo);
                con.SaveIni();

                if (!File.Exists(HomeDir + "\\上传的光盘文件命名请包含正确的ISBN和光盘条码.txt")) { File.Create(@HomeDir + "\\上传的光盘文件命名请包含正确的ISBN和光盘条码!.txt"); }
                //if (!File.Exists(@HomeDir + "\\请尽量避免光盘文件名包含中文.txt")) { File.Create(@HomeDir + "\\请尽量避免光盘文件名包含中文.txt"); }


                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="username">用户名</param>
        /// <returns>是否成功删除</returns>
        public bool DeleteUser(string username)
        {
            bool result = con.FtpUserList.Remove(username);
            con.SaveIni();
            return result;
        }
        /// <summary>
        /// 是否存在用户
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public bool IsExistUser(string username)
        {
            return con.FtpUserList.ContainsKey(username);
        }
        /// <summary>
        /// 查找用户根目录
        /// </summary>
        /// <param name="username"></param>
        /// <returns>返回空则不存在用户</returns>
        public string GetFtpUserHomeDir(string username)
        {
            if (con.FtpUserList[username] != null)
            {
                string HomeDir = (string)con.FtpUserList[username].PropertyList["HomeDir"];
                return HomeDir;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 限制下载速度
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="limitByte">速度，单位字节</param>
        /// <returns></returns>
        public bool SpeedLimitDown(string username, long limitByte)
        {
            FtpUserInfo userinfo = con.FtpUserList[username];
            if (userinfo != null)
            {
                userinfo.PropertyList["SpeedLimitDown"] = limitByte.ToString();
                con.SaveIni();
                return true;
            }
            return false;
        }
        /// <summary>
        /// 限制上传速度
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="limitByte">速度，单位字节</param>
        /// <returns></returns>
        public bool SpeedLimitUp(string username, long limitByte)
        {
            FtpUserInfo userinfo = con.FtpUserList[username];
            if (userinfo != null)
            {
                userinfo.PropertyList["SpeedLimitUp"] = limitByte.ToString();
                con.SaveIni();
                return true;
            }
            return false;
        }
        /// <summary>
        /// 封锁IP
        /// </summary>
        /// <param name="IP">IP地址</param>
        public void LockIP(string IP)
        {
            PropertyControl pc = con.MainDomainConfigWithoutUserConfig;
            int max = 1;
            string maxStr = pc["IPAccess1"].ToString();
            foreach (DictionaryEntry de in pc)
            {
                Match m = Regex.Match(de.Key.ToString(), @"IPAccess([0-9]+)", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
                if (m.Success)
                {
                    int num = Convert.ToInt32(m.Groups[1].Value);
                    if (num > max)
                    {
                        max = num;
                        maxStr = de.Value.ToString();
                    }
                }
            }
            pc["IPAccess" + (max + 1).ToString()] = maxStr;
            pc["IPAccess" + (max).ToString()] = "D|" + IP;
            con.SaveIni();
        }
        /// <summary>
        /// 解锁IP
        /// </summary>
        /// <param name="IP">IP地址</param>
        /// <returns></returns>
        public bool UnLockIP(string IP)
        {
            PropertyControl pc = con.MainDomainConfigWithoutUserConfig;
            int needDelete = 0;
            string maxStr = pc["IPAccess1"].ToString();
            foreach (DictionaryEntry de in pc)
            {
                Match m = Regex.Match(de.Key.ToString(), @"IPAccess([0-9]+)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                if (m.Success)
                {
                    Match m1 = Regex.Match(de.Value.ToString(), IP, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    if (m1.Success)
                    {
                        needDelete = Convert.ToInt32(m.Groups[1].Value);
                    }
                }
            }

            if (needDelete != 0)
            {
                string needDeleteStr = "IPAccess" + needDelete.ToString();
                pc.Remove(needDeleteStr);
                string reStr = "IPAccess" + (needDelete + 1).ToString();
                pc.Add(needDeleteStr, pc[reStr]);
                pc.Remove(reStr);
                con.SaveIni();
                return true;
            }
            return false;
        }
        /// <summary>
        /// 返回用户名列表
        /// </summary>
        /// <returns></returns>
        public string[] GetFtpUserList()
        {
            string[] result = new string[con.FtpUserList.Count];
            int i = 0;
            foreach (var ftpuser in con.FtpUserList)
            {
                result[i] = ftpuser.Key;
                i++;
            }
            return result;
        }
        /// <summary>
        /// 返回0则不限制
        /// </summary>
        /// <returns></returns>
        public string GetFtpMaxUsersCount(string username)
        {
            FtpUserInfo userinfo = con.FtpUserList[username];
            string maxUserCount = null;
            if (userinfo != null)
            {
                maxUserCount = userinfo.PropertyList["MaxNrUsers"].ToString();
            }
            return maxUserCount;
        }
        /// <summary>
        /// 设置最大用户数
        /// </summary>
        /// <param name="maxUserCount"></param>
        public bool SetFtpMaxUsersCount(string username, string maxUserCount)
        {
            FtpUserInfo userinfo = con.FtpUserList[username];
            bool IsSet = false;
            if (userinfo != null)
            {
                if (!String.IsNullOrEmpty(maxUserCount))
                {
                    try
                    {
                        int check = Convert.ToInt16(maxUserCount);
                        if (check >= 1)
                        {
                            userinfo.PropertyList["MaxNrUsers"] = maxUserCount;
                            con.SaveIni();
                            IsSet = true;
                        }
                    }
                    catch { }
                }
            }
            return IsSet;
        }

        public string GetPassword(string username)
        {
            FtpUserInfo userinfo = con.FtpUserList[username];
            if (userinfo != null)
            {
                return userinfo.Password;
            }
            return null;
        }
        public string MoveFile(string username, string ISBN, string ZTM, string CDXH)
        {
            try
            {
                string homedir = GetFtpUserHomeDir(username);//获取管理员目录
                homedir = homedir.Replace("\r", "");
                DirectoryInfo dir = new DirectoryInfo(homedir);
                FileInfo file = dir.GetFiles().FirstOrDefault(f => f.Name.Contains(ISBN) && f.Name.Contains(CDXH));//获取FTP服务器文件列表
                if (file != null)
                {
                    string path = XMLHelper.getAppSettingValue("FTP_Home") + "\\Download\\" + ISBN.Replace(" ", "_") +
                      CDString.getFileName(ZTM) + "\\" + CDXH.Replace(" ", "_");

                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    if (dir.Root.Name.Equals(file.Directory.Root.Name))
                    {
                        file.MoveTo(path + "\\" + CDString.getFileName(file.Name.Replace(ISBN.Replace(" ", "_") + "_" + CDXH.Replace(" ", "_"), "")));
                        return CDString.getFileName(file.Name);
                    }
                    else
                    {
                        file.CopyTo(path + "\\" + CDString.getFileName(file.Name.Replace(ISBN.Replace(" ", "_") + "_" + CDXH.Replace(" ", "_"), "")));
                        file.Delete();
                        return CDString.getFileName(file.Name);
                    }
                }
                else
                { return null; }
            }
            catch { return null; }
        }


        /// <summary>
        /// 获取某一用户的虚拟目录列表
        /// </summary>
        /// <param name="username">用户名</param>
        /// <returns></returns>
        public ArrayList GetVirturlDirs(string username)
        {
            ArrayList VirturlDir = new ArrayList();
            PropertyControl pc = con.MainDomainConfigWithoutUserConfig;
            string HomeDir = GetFtpUserHomeDir(username);
            foreach (DictionaryEntry pe in pc)
            {
                string key = pe.Key.ToString();
                if (Regex.IsMatch(key, "VirPath[0-9]+"))
                {
                    Match m_VirturlDir = Regex.Match(pe.Value.ToString(), @"(.*)\|(.*?)\|(.*)", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
                    string pattern = HomeDir.Replace(@"\", @"\\");
                    pattern = pattern.Replace(".", @"\.");
                    pattern = pattern.Replace("*", @"\*");
                    pattern = pattern.Replace("?", @"\?");
                    pattern = pattern.Replace("[", @"\[");
                    pattern = pattern.Replace("]", @"\]");
                    if (Regex.IsMatch(m_VirturlDir.Groups[2].Value, pattern, RegexOptions.IgnoreCase))
                    {
                        VirturlDir.Add(m_VirturlDir.Value);
                    }
                }
            }
            return VirturlDir;
        }
        public string GetDownloadLoc(string username, string ISBN, string ZTM, string CDTM)
        {
            ArrayList VirturlDirs = GetVirturlDirs("reader");
            //string []test = (string [])VirturlDirs.ToArray(typeof(string));

            //按顺序测试每个虚拟路径对应的物理路径是否有容量
            long MinSpaceLength = ((long)10) * 1024 * 1024 * 1024;//最少剩余10GB

            string SourceDir = "";
            string DestinationDir;
            string VirDirName = "";

            bool IsHasFreeSpace = false;
            foreach (object o in VirturlDirs)
            {
                Match m_VirturlDir = Regex.Match((string)o, @"(.*)\|(.*?)\|(.*)", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
                DriveInfo[] allDrives = DriveInfo.GetDrives();
                foreach (DriveInfo di in allDrives)
                {
                    string pattern = di.RootDirectory.ToString().Replace(@"\", @"\\");
                    pattern = pattern.Replace(".", @"\.");
                    pattern = pattern.Replace("*", @"\*");
                    pattern = pattern.Replace("?", @"\?");
                    pattern = pattern.Replace("[", @"\[");
                    pattern = pattern.Replace("]", @"\]");
                    if (Regex.IsMatch(m_VirturlDir.Groups[1].Value, pattern))
                    {
                        if (di.AvailableFreeSpace > MinSpaceLength)
                        {
                            SourceDir = m_VirturlDir.Groups[1].Value;
                            DestinationDir = m_VirturlDir.Groups[2].Value;
                            VirDirName = m_VirturlDir.Groups[3].Value;
                            IsHasFreeSpace = true;
                        }
                        break;
                    }
                }
                if (IsHasFreeSpace)
                {
                    break;
                }
            }

            if (IsHasFreeSpace == false)
            {
                throw new Exception("没有一个虚拟路径对应的盘符有空间存入文件，请添加硬盘,然后增加一个虚拟路径到" + "down");
            }
            else
            {
                string path = XMLHelper.getAppSettingValue("FTP_Home") + "\\Download\\" + ISBN + ZTM + "\\";
                string homeDir = GetFtpUserHomeDir(username).Replace("\r", "");
                DirectoryInfo dir = new DirectoryInfo(homeDir);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                FileInfo file = dir.GetFiles().FirstOrDefault(f => f.Name.Contains(ISBN) && f.Name.Contains(CDTM));
                if (!file.Exists)
                {
                    throw new Exception("不存在文件，请确认输入参数正确");
                }
                if (dir.Root.Equals(file.Directory.Root))
                {
                    file.MoveTo(path);
                }
                else
                {
                    file.CopyTo(path);
                    file.Delete();
                }

                return "/" + VirDirName + "/" + ISBN + ZTM + "/" + file.Name;
            }
        }
    }
}
