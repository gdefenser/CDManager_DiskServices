using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections;

namespace CDManager_DiskServices.AppClass.FTP.Serv_uControl
{
    class iniControl
    {
        private string MainDomainName;
        private string iniPath;
        private string iniContent;
        public iniControl(string iniPath, string MainDomainName = "windpro")
        {
            this.MainDomainName = MainDomainName;
            DomainArr = new Dictionary<string, string>();
            //读入文件
            this.iniPath = iniPath;
            if (!File.Exists(iniPath))
            {
                throw new Exception("不存在文件");
            }
            StreamReader sr = new StreamReader(iniPath, System.Text.Encoding.Default);
            iniContent = sr.ReadToEnd();
            sr.Close();

            //得到配置文件头
            Match m_header = Regex.Match(iniContent, @"\[GLOBAL\].*?(?=\[Domain[0-9]+\])", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
            _header = m_header.Value;

            //得到域名列表
            MatchCollection mc_DomainList = Regex.Matches(iniContent, @"\[(Domain[0-9]+)\].*?((?=\[Domain[0-9]+\])|$)", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
            foreach (Match m in mc_DomainList)
            {
                Match m_key = Regex.Match(_header, @"Domain1=.*?\|\|.*?\|(.*?)\|", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
                string key = m_key.Groups[1].Value;
                string value = m.Groups[0].Value;
                DomainArr.Add(key, value);
            }

            string MainDomainStr = DomainArr[MainDomainName];
            if (MainDomainStr == null)
            {
                throw new Exception("需要处理域错误");
            }

            //得到域名除用户以外的配置信息
            Match m_DomainConfig = Regex.Match(MainDomainStr, @"\[Domain[0-9]+\].?\n(.*?)\n\[USER=", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
            string DomainConfigStr = m_DomainConfig.Groups[1].Value;

            MatchCollection mc_DomainConfigWithoutUserConfig = Regex.Matches(DomainConfigStr, @"^(?!User[0-9]+)((.*?)=(.*?))\r?$", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
            MainDomainConfigWithoutUserConfig = new PropertyControl(); ;
            foreach (Match m in mc_DomainConfigWithoutUserConfig)
            {
                string key = m.Groups[2].Value;
                string value = m.Groups[3].Value;
                MainDomainConfigWithoutUserConfig.Add(key, value);
            }

            ////得到域名用户配置信息
            //MatchCollection mc_DomainConfigInUserConfig = Regex.Matches(DomainConfigStr, @"^(?=User[0-9])+.*$", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

            //得到用户信息
            Match m_DomainUserList = Regex.Match(MainDomainStr, @"\[USER=.*", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
            string UserListStr = m_DomainUserList.Value;

            //得到用户列表
            MatchCollection mc_DomainUserStr = Regex.Matches(UserListStr, @"\[USER=(.*?)\|.*?((?=\n\[USER=)|$)", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
            FtpUserList = new Dictionary<string, FtpUserInfo>();
            foreach (Match m in mc_DomainUserStr)
            {
                try
                {
                    FtpUserInfo userinfo = new FtpUserInfo(m.Value);
                    FtpUserList.Add(m.Groups[1].Value, userinfo);
                }
                catch { }
            }
        }

        private string _header;
        private Dictionary<string, string> DomainArr;
        public PropertyControl MainDomainConfigWithoutUserConfig;
        //private string[] MainDomainConfigInUserConfig;
        public Dictionary<string, FtpUserInfo> FtpUserList;

        public string Header
        {
            get { return _header; }
        }

        /// <summary>
        /// 返回用户配置信息
        /// </summary>
        /// <returns></returns>
        private string GetMainDomainUserConfig()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(MainDomainConfigWithoutUserConfig.ToString());

            int count = 1;
            foreach (var o in FtpUserList)
            {
                string userconfig = "User" + count.ToString() + "=" + o.Key + "|1|0";
                sb.AppendLine(userconfig);
                count++;
            }
            return sb.ToString();
        }

        public void SaveIni()
        {
            StringBuilder sb = new StringBuilder();

            //header
            string[] resultString = Regex.Split(_header, "(?=ProcessID=[0-9]+)", RegexOptions.IgnoreCase);

            if (resultString.Length > 2)
            {
                throw new Exception("发生逻辑错误");
            }

            sb.Append(resultString[0]);
            sb.AppendLine("ReloadSettings=True");
            sb.Append(resultString[1]);

            //域名
            foreach (var o in DomainArr)
            {
                if (o.Key.Equals(MainDomainName))
                {
                    sb.AppendLine();
                    Match m_Domain_id = Regex.Match(o.Value, @"\[Domain([0-9]+)\]", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
                    string str_id = m_Domain_id.Value;
                    sb.AppendLine(str_id);

                    sb.Append(GetMainDomainUserConfig());


                    foreach (var ftpuser in FtpUserList)
                    {
                        sb.Append(ftpuser.Value.ToString());
                    }
                }
                else
                {
                    sb.Append(o.Value);
                }
            }

            StreamWriter sw = new StreamWriter(iniPath, false, System.Text.Encoding.Default);

            try
            {
                sw.Write(sb.ToString());
                sw.Close();
            }
            catch
            {

            }
        }
    }
}
