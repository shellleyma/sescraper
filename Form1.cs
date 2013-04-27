using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.SQLite;

namespace SEScraper
{
    public partial class main : Form
    {
        public main()
        {
            InitializeComponent();
            InitDatabase();
        }
        private bool InitDatabase(string filename=null)
        {
            string dbfile;
            if (filename==null) 
                 dbfile=Application.StartupPath + @"\data\"+GlobalVar.filename;
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
                //System.IO.File.Create(Application.StartupPath + @"\config\setting.db");
                sqlitehelper settingdb = new sqlitehelper(dbfile);
                string sql="CREATE TABLE IF NOT EXISTS post (rowid integer NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,title varchar(256),tag varchar(256),description varchar(256),content TEXT,comment TEXT,posttime INTEGER,postname varchar(512))";
                settingdb.ExecuteNonQuery(sql);
                

            }

            return true;
        }
        
    }
}
