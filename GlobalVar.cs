using System;
using System.Collections.Generic;
using System.Text;

namespace SEScraper
{
    class GlobalVar
    {
        public static string filename = "data.db";
        public static Dictionary<string,string> current_engine=new Dictionary<string,string>();
        public static int MaxDivCount = 20;
        public static bool RandomUserAgent = false;
    }
}
