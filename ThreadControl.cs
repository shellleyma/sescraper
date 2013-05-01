using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Threading;
using Amib.Threading;
using System.Windows.Forms;
using SeasideResearch.LibCurlNet;

namespace SEScraper
{
    class ThreadControl
    {
        public void main_thread()
        {
            

           

            Curl.GlobalInit((int)CURLinitFlag.CURL_GLOBAL_ALL);
            GlobalVar.isCrawling = true;
            //string conn = string.Format(@"Data Source="+System.Environment.CurrentDirectory+"\\data\\{0}", GlobalVar.filename);
            sqlitehelper con = new sqlitehelper(System.Environment.CurrentDirectory + @"\data\" + GlobalVar.filename);
            string sql = "select keyword from Content where flag==0";
            DataTable dt = new DataTable();
            dt = con.GetDataTable(sql);


            int maxWorkerThreads = 12;
            int maxPortThreads = 15;
            ThreadPool.SetMaxThreads(maxWorkerThreads, maxPortThreads);
            ManualResetEvent[] doneEvents = new ManualResetEvent[dt.Rows.Count];

            //IWorkItemResult[] wir = new IWorkItemResult[dt.Rows.Count];
            MessageBox.Show(dt.Rows.Count.ToString());
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                doneEvents[i] = new ManualResetEvent(false);
                //MessageBox.Show(dt.Rows[i]["keyword"].ToString());
                OneWorker one = new OneWorker(dt.Rows[i]["keyword"].ToString());
                ThreadPool.QueueUserWorkItem(one.work, i);
                Thread.Sleep(500);

            }

          

           
        
        }
        public void test()
        {
            SmartThreadPool smartThreadPool = new SmartThreadPool();

            Curl.GlobalInit((int)CURLinitFlag.CURL_GLOBAL_ALL);
            GlobalVar.isCrawling = true;
            //string conn = string.Format(@"Data Source="+System.Environment.CurrentDirectory+"\\data\\{0}", GlobalVar.filename);
            sqlitehelper con = new sqlitehelper(System.Environment.CurrentDirectory + @"\data\" + GlobalVar.filename);
            string sql = "select keyword from Content where flag==0";
            DataTable dt = new DataTable();
            dt = con.GetDataTable(sql);
            IWorkItemResult[] wir = new IWorkItemResult[dt.Rows.Count];
            MessageBox.Show(dt.Rows.Count.ToString());
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                //MessageBox.Show(dt.Rows[i]["keyword"].ToString());
                OneWorker one = new OneWorker(dt.Rows[i]["keyword"].ToString());
                ThreadPool.QueueUserWorkItem(one.work, i);

            }

            bool success = SmartThreadPool.WaitAll(
                   wir);

            if (success)
            {
                GlobalVar.isCrawling = false;

            }
            smartThreadPool.Shutdown();
        }


        public void store_thread()
        {
            while (GlobalVar.isCrawling)
            {
                if (GlobalVar.divQue.Count > 0)
                {
                    sqlitehelper newhelper = new sqlitehelper(System.Environment.CurrentDirectory+@"\data\"+GlobalVar.filename);
                    foreach (string[] divarray in GlobalVar.divQue)
                    {
                        Dictionary<string, string> data = new Dictionary<string, string>();
                        data.Add("content",divarray[1]);
                        data.Add("suburl", divarray[2]);
                        string where = string.Format("keyword={0}",divarray[0]);
                        newhelper.Update("Content", data, where);
                    }
                    
                }

                Thread.Sleep(5000);
            }
        
        }
    }
}
