using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using SeasideResearch.LibCurlNet;

namespace SEScraper
{
    class OneWorker
    {
        string keyword;
        string html;
        Stream stream;
        string nextpage;
        int count = 0;
        List<string> divcontent;

        public OneWorker(string inputkeyword)
        {
            keyword = inputkeyword;
        }
        //主工作线程
        public void work()
        {
            
            string current_url;
            current_url = GlobalVar.current_engine["Host"] + @"/" + GlobalVar.current_engine["QUERY"];
            current_url = current_url.Replace(@"[query]", System.Web.HttpUtility.UrlEncode(keyword, System.Text.Encoding.UTF8));
            nextpage = "not null";
            while (divcontent.Count<GlobalVar.MaxDivCount && nextpage!=string.Empty)
            {
                string raw_html = gethtml(current_url);
                
                html_process(raw_html);
            }
            filter_process();
            save_process();
        }
        //过滤以及其他处理过程
        public void filter_process()
        { 
        
        }
        //输出保存的方法
        public void save_process()
        { 
           //save to txt test
   
        
        }
        //html解析方法，获取content存储到List<string> divcontent里
        public void html_process(string input)
        {
           
        
        }
        //利用libcurl实现的http-get获取网页源代码
        public string gethtml(string url)
        {

            try
            {

                //Curl.GlobalInit((int)CURLinitFlag.CURL_GLOBAL_ALL);
                Easy easy = new Easy();
                string buffer=string.Empty;
                Easy.WriteFunction wf = new Easy.WriteFunction(OnWriteData);
                easy.SetOpt(CURLoption.CURLOPT_WRITEFUNCTION, wf);
                //easy.SetOpt(CURLoption.CURLOPT_NOSIGNAL, 1);
                easy.SetOpt(CURLoption.CURLOPT_USERAGENT,get_useragent());
                easy.SetOpt(CURLoption.CURLOPT_FOLLOWLOCATION, true);
                easy.SetOpt(CURLoption.CURLOPT_URL,url);
                //easy.SetOpt(CURLoption.CURLOPT_POST, true);

                easy.Perform();
                easy.Cleanup();
                //Console.WriteLine(Thread.CurrentThread.Name);


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return this.html;
        }
        public   Int32 OnWriteData(Byte[] buf, Int32 size, Int32 nmemb,
       Object extraData)
        {
            //stream.Write(buf,0,size);
            html=System.Text.Encoding.UTF8.GetString(buf);
            return size * nmemb;
        }
        //获取一个useragent
        public string get_useragent()
        { 
            string u_agent;
            if (GlobalVar.RandomUserAgent == false)
                u_agent = "Mozilla 4.0 (compatible; MSIE 6.0; Win32";
            else
                u_agent = random_useragent();

            return u_agent;

        }
        //随机获取一个useragent
        public string random_useragent()
        {


            if (File.Exists(System.Environment.CurrentDirectory+@"\user_agent.txt"))
            {
                string[] useragents = File.ReadAllText(System.Environment.CurrentDirectory + @"\user_agent.txt").Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                Random rnd = new Random();
                Shuffle(rnd, useragents);
                return useragents[0];
            }
            else return "Mozilla 4.0 (compatible; MSIE 6.0; Win32";
           
        
        }
        public static void Shuffle<T>(Random rng, T[] array)
        {
            int n = array.Length;
            while (n > 1)
            {
                int k = rng.Next(n--);
                T temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }
        }
       
    }
}
