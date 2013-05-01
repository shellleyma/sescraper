using System;
using System.Collections.Generic;
using System.Text;

namespace SEScraper
{
    class GlobalVar
    {
        public static IDictionary<string, IDictionary<string, string>> iniSections = new Dictionary<string, IDictionary<string, string>>();
        public static string filename = "data.db";
        public static IDictionary<string,string> current_engine=new Dictionary<string,string>();
        public static int MaxDivCount = 20;
        public static bool RandomUserAgent = false;
        public static Queue<string[]> divQue = new Queue<string[]>();
        public static bool isCrawling = false;
        public static string enginefilename = "engines.ini";
    }
}
