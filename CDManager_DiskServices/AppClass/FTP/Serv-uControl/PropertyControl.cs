using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace CDManager_DiskServices.AppClass.FTP.Serv_uControl
{
    public class PropertyControl : Hashtable
    {
        public PropertyControl():base(){}

        /// <summary>
        /// 将各种属性转换成字符串
        /// </summary>
        /// <returns>属性字符串</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (DictionaryEntry de in this)
            {
                sb.Append(de.Key.ToString());
                sb.Append('=');
                sb.Append(de.Value.ToString());
                sb.Append("\r\n");
            }
            return sb.ToString();
        }
    }
}
