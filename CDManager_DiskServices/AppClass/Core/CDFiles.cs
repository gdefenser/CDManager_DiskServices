using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace CDManager_DiskServices.AppClass.Core
{
    public class CDFiles
    {
        public static void DeleteFloder(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            List<FileInfo> file_list = dir.GetFiles().ToList();
            if (file_list.Count > 0) { }
        }
    }
}