using System;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace SEScraper
{
    public class RWIniFile
    {
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, System.Text.StringBuilder retVal, int size, string filePath);

        //参数说明：section：INI文件中的段落名称；key：INI文件中的关键字；def：无法读取时候时候的缺省数值；retVal：读取数值；size：数值的大小；filePath：INI文件的完整路径和名称。

        //读取键值 
        public static string ReadIni(string 主键名, string 子键名, string 默认键值, int 数值大小, string 文件路径)
        {
            string m_ret = 默认键值;
            try
            {
                System.Text.StringBuilder 返回值 = new System.Text.StringBuilder(默认键值);
                GetPrivateProfileString(主键名, 子键名, 默认键值, 返回值, 数值大小, 文件路径);
                m_ret = 返回值.ToString();
            }
            catch
            {
                m_ret = 默认键值;
            }
            return m_ret;
        }

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        //参数说明：section：INI文件中的段落；key：INI文件中的关键字；val：INI文件中关键字的数值；filePath：INI文件的完整的路径和名称。
        //写入键值 
        public static bool WriteIni(string 主键名, string 子键名, string 数值, string 文件路径)
        {
            bool m_ret = true;
            try
            {
                WritePrivateProfileString(主键名, 子键名, 数值, 文件路径);
            }
            catch
            {
                m_ret = false;
            }
            return m_ret;
        }
    }
}
