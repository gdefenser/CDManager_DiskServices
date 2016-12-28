using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace CDManager_DiskServices.AppClass.FTP.Serv_uControl
{
    /// <summary>
    /// FTP用户类
    /// </summary>
    class FtpUserInfo
    {
        private string _UserName;
        private string _Password;
        private PropertyControl _PropertyList = new PropertyControl();

        public PropertyControl PropertyList
        {
            get { return _PropertyList; }
            set { _PropertyList = value; }
        }

        public string UserName
        {
            get { return _UserName; }
            set { _UserName = value; }
        }

        public string Password
        {
            get { return _Password; }
            set 
            {
                const int randomCount = 2;
                char[] randomHeader = new char[randomCount];
                Random randomChar = new Random();
                for (int i = 0; i < randomCount; i++)
                {
                    int rand = randomChar.Next(26) + 97;
                    randomHeader[i] = (char)rand;
                }
                string PassHeader = new string(randomHeader);
                string md5_Before = PassHeader + value;

                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] data = System.Text.Encoding.Default.GetBytes(md5_Before);
                byte[] md5data = md5.ComputeHash(data);
                string md5_After = "";
                for (int i = 0; i < md5data.Length; i++)
                {
                    md5_After += md5data[i].ToString("x").PadLeft(2, '0');
                }
                string passstr = PassHeader + md5_After;
                _Password = passstr; 
            }
        }

        public FtpUserInfo(string MatchIniStr)
        {
            Match m_username = Regex.Match(MatchIniStr, @"\[USER=(.*?)\|", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
            _UserName = m_username.Groups[1].Value;

            MatchCollection mc_DomainUser = Regex.Matches(MatchIniStr, @"^([^\[].*?)=(.*)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

            _Password = mc_DomainUser[0].Groups[2].Value;

            for (int i = 1; i < mc_DomainUser.Count;i++ )
            {
                Match m = mc_DomainUser[i];
                _PropertyList.Add(m.Groups[1].Value, m.Groups[2].Value);
            }
        }

        public FtpUserInfo()
        {
        }

        /// <summary>
        /// 将各种属性转换成字符串
        /// </summary>
        /// <returns>属性字符串</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[USER=" + _UserName + "|1]");
            sb.AppendLine("Password=" + _Password);
            sb.Append(_PropertyList.ToString());

            return sb.ToString();
        }
    }
}
