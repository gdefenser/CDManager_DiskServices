using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace CDManager_DiskServices.AppClass.Core
{
    public class CDString
    {
        public static string getFileName(string str)
        {
            return str.Replace(" ", "_").
                Replace("+", "_").
                Replace(":", "").
                Replace("\\", "").
                Replace("/", "").
                Replace("*", "").
                Replace("?", "").
                Replace("\"", "").
                Replace("<", "").
                Replace(">", "").
                Replace("|", "").
                Replace("C#", "CSharp").
                Replace("：","");
        }

        public static string getItemValue(string str)
        {
            if (String.IsNullOrEmpty(str)) { return "暂无信息"; }
            else { return str; }
        }


    }
}
