using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using SeasideResearch.LibCurlNet;
using System.Windows.Forms;
using System.Reflection;
using log4net;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace SEScraper
{
    class OneWorker
    {
        string keyword;
        string html;
        //Stream stream;
        string nextpage;
        //int count = 0;
        string current_url;
        List<string[]> divcontent=new List<string[]>();

        Regex maincontent = new Regex(@"(?is)"+GlobalVar.current_engine["MainContent"].Replace("[...]", "(?<maincontent>.*?)"));
        Regex sublink = new Regex(@"(?is)" + GlobalVar.current_engine["LinksMask"].Replace("[...]", "(?:.*?)").Replace("[LINK]", @"(?<sublink>http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?)"));
        Regex subblock = new Regex(@"(?is)" + GlobalVar.current_engine["SubBlock"].Replace("[...]", "(?<subblock>.*?)"));
        Regex subtitle = new Regex(@"(?is)" + GlobalVar.current_engine["SubTitle"].Replace("[...]", "(?:.*?)").Replace("[SubTitle]", "(?<subtitle>.*?)"));
        Regex subcontent = new Regex(@"(?is)" + GlobalVar.current_engine["SubContent"].Replace("[...]", "(?:.*?)").Replace("[SubContent]", "(?<subcontent>.*?)"));
        Regex nextpage1 = new Regex(@"(?is)" + "(?<nextpage>" + GlobalVar.current_engine["NextPage"].Replace("[...]", ".*?") + ")");
        Regex nextpage2 = new Regex(@"(?is)" + GlobalVar.current_engine["NextPage2"]);
        Regex CaptchaURL = new Regex(@"(?is)" + GlobalVar.current_engine["CaptchaURL"]);
        Regex CaptchaImage = new Regex(@"(?is)" + GlobalVar.current_engine["CaptchaImage"]);
        string CaptchaField = GlobalVar.current_engine["CaptchaField"];

        public OneWorker(string inputkeyword)
        {
            keyword = inputkeyword;
        }
        //主工作线程
        public void work(object state)
        {


            current_url = GlobalVar.current_engine["Hostname"] + @"/" + GlobalVar.current_engine["Query"];
            current_url = current_url.Replace(@"[QUERY]", System.Web.HttpUtility.UrlEncode(keyword, System.Text.Encoding.UTF8));
            nextpage = "not null";
            //MessageBox.Show(keyword);
            while (divcontent.Count < GlobalVar.MaxDivCount && nextpage != string.Empty)
            {
                string raw_html = gethtml(current_url);
                ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
                log.Info(raw_html);

                html_process(raw_html);


            }
            filter_process();
            save_process();
            //return 1;
        }
        //过滤以及其他处理过程
        public void filter_process()
        {

        }
        //输出保存的方法
        public void save_process()
        {
            //prepare string[] ,then add to Global Queue

            string[] divtemp=new string[3];
            divtemp[0]=keyword;
            for (int j = 0; j < divcontent.Count; j++)
            { 
             divtemp[1]=divtemp[1]+"<subtitle>"+divcontent[j][1]+"</subtitle><subcontent>"+divcontent[j][2]+"</subcontent>###";
             divtemp[2]=divtemp[2]+ divcontent[j][0]+"###";
            
            }

           
            GlobalVar.divQue.Enqueue(divtemp);


        }
        //html解析方法，获取content存储到List<string[]> divcontent里
        public void html_process(string input)
        {
           
                //Match m_captcha = CaptchaURL.Match(input);
            
            if (false)
            {

            }
            else
            {
                //获取maincontent
                Match maincontent_now = maincontent.Match(input);
                string maincontent_str = maincontent_now.Groups["maincontent"].ToString();

                MatchCollection mc_subblocks = subblock.Matches(maincontent_str);
                for (int i = 0; i < mc_subblocks.Count; i++)
                {
                    string[] divtemp = new string[3];
                    //获取sublink
                    Match sublink_now = sublink.Match(mc_subblocks[i].Value);
                    divtemp[0] = sublink_now.Groups["sublink"].ToString();
                    //获取subtitle
                    Match subtitle_now = subtitle.Match(mc_subblocks[i].Value);
                    divtemp[1] = subtitle_now.Groups["subtitle"].ToString();
                    //获取subcontent
                    Match subcontent_now = subcontent.Match(mc_subblocks[i].Value);
                    divtemp[2] = subcontent_now.Groups["subcontent"].ToString();
                    



                    divcontent.Add(divtemp);



                    if (divcontent.Count > GlobalVar.MaxDivCount) return;

               } 
                //获取nextpage
                Match nextpage_now = nextpage1.Match(input);
                if (nextpage_now.Success)
                {
                    nextpage = nextpage_now.Groups["nextpage"].ToString();
                    current_url = nextpage;
                }
                else
                {
                    nextpage = string.Empty;
                    current_url = string.Empty;
                }
            }


        }
        //利用libcurl实现的http-get获取网页源代码
        public string gethtml(string url)
        {

            try
            {

                //Curl.GlobalInit((int)CURLinitFlag.CURL_GLOBAL_ALL);
                Easy easy = new Easy();
                ;
                Easy.WriteFunction wf = new Easy.WriteFunction(OnWriteData);
                easy.SetOpt(CURLoption.CURLOPT_WRITEFUNCTION, wf);
                //easy.SetOpt(CURLoption.CURLOPT_NOSIGNAL, 1);
                easy.SetOpt(CURLoption.CURLOPT_USERAGENT, get_useragent());
                easy.SetOpt(CURLoption.CURLOPT_FOLLOWLOCATION, true);
                easy.SetOpt(CURLoption.CURLOPT_URL, url);
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
        public Int32 OnWriteData(Byte[] buf, Int32 size, Int32 nmemb,
       Object extraData)
        {
            //stream.Write(buf,0,size);
            html = System.Text.Encoding.UTF8.GetString(buf);
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


            if (File.Exists(System.Environment.CurrentDirectory + @"\user_agent.txt"))
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
