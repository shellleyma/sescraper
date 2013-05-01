using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Threading;
using SEScraper;

namespace SEScraper
{
    public partial  class main : Form
    {
        

        public main()
        {
            InitializeComponent();
            InitDatabase();
            InitEngines();
        }
        private bool InitDatabase(string filename = null)
        {
            string dbfile;
            if (filename == null)
                dbfile = Application.StartupPath + @"\data\" + GlobalVar.filename;
            else
                dbfile = Application.StartupPath + @"\data\" + filename;
            if (!System.IO.File.Exists(dbfile))
            {
                if (!System.IO.Directory.Exists(Application.StartupPath + @"\data"))
                {
                    System.IO.Directory.CreateDirectory(Application.StartupPath + @"\data");
                }


                System.Data.SQLite.SQLiteConnection sql_con = new System.Data.SQLite.SQLiteConnection("Data Source={dbfile};Version=3;New=True;Compress=True;");
                sql_con.Close();
                sql_con.Dispose();
                //System.IO.File.Create(Application.StartupPath + @"\config\setting.db");
                sqlitehelper settingdb = new sqlitehelper(dbfile);
                string sql = "CREATE TABLE IF NOT EXISTS Content (id integer NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,keyword varchar(256) UNIQUE,flag integer default 0,content TEXT,suburl TEXT)";
                settingdb.ExecuteNonQuery(sql);
                //settingdb.Dispose();
                GC.Collect();


            }

            return true;
        }
        public void InitEngines()
        {
            INIFile engineini = new INIFile(Directory.GetCurrentDirectory() + @"\" + GlobalVar.enginefilename);
            GlobalVar.iniSections = tt.Clone(engineini._sections);
            GlobalVar.current_engine = tt.Clone(engineini._sections["BING"]);
           
            GC.Collect();
        }
       
        private void btnImport_Click(object sender, EventArgs e)
        {
            InitDatabase();

            Thread keywordImport_thread = new Thread(new ThreadStart(batchImportKeywords));
            keywordImport_thread.SetApartmentState ( ApartmentState.STA);
            keywordImport_thread.Start();
            




           
        }
        public static void batchImportKeywords()
        {
            string conn = string.Format(@"Data Source=.\data\{0}; LongNames=0; Timeout=1000; NoTXN=0; SyncPragma=NORMAL; StepAPI=0", GlobalVar.filename);
            OpenFileDialog file = new OpenFileDialog();
            file.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            if (file.ShowDialog() == DialogResult.OK)
            {
                if (file.OpenFile() != null)
                {
                    string path = file.FileName;
                    string[] strArray = null;
                    int length = 0;
                    StreamReader reader = new StreamReader(File.OpenRead(path), Encoding.Default);
                    strArray = reader.ReadToEnd().Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    length = strArray.Length;
                    reader.Close();
                    reader.Dispose();
                    SQLiteConnection connection = new SQLiteConnection();
                    SQLiteCommand command = null;
                    connection.ConnectionString = conn;
                    MessageBox.Show("正在插入关键词，如果导入完毕会有提示框");
                    try
                    {
                        if (connection.State != ConnectionState.Open)
                        {
                            connection.Open();
                        }

                        using (SQLiteTransaction transaction = connection.BeginTransaction())
                        {
                            using (command = connection.CreateCommand())
                            {
                                for (int i = 0; i < length; i++)
                                {
                                    //string[] line = strArray[i].Split('t');
                                    command.CommandText = "insert or ignore into Content (keyword) values (@keyword)";
                                    SQLiteParameter[] para =
                                    {
                                new SQLiteParameter("@keyword",strArray[i])
                               
                                    };
                                    command.Parameters.AddRange(para);
                                    command.ExecuteNonQuery();
                                }
                            }
                            transaction.Commit();
                            strArray = null;
                            GC.Collect();
                        }
                    }
                    catch (Exception exception)
                    {
                        strArray = null;
                        GC.Collect();
                        //throw exception;
                    }
                    finally
                    {
                        strArray = null;
                        connection.Close();
                        connection.Dispose();
                        GC.Collect();
                    }
                }
            }
            MessageBox.Show("关键词导入完毕");
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text.Trim() != "")
            {
                GlobalVar.filename = textBox1.Text + ".db";
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            ThreadControl tc=new ThreadControl();
            Thread multi_crawl = new Thread(new ThreadStart(tc.main_thread));
            Thread store_thread = new Thread(new ThreadStart(tc.store_thread));
            multi_crawl.Start();
            store_thread.Start();
        }
    }
    
}
