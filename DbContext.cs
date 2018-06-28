using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.IO;

namespace MasterNodoOCB
{
    public class DbContext
    {        
        private const string DBName = @"C:\NodoOCB\data\database.sqlite";
        private const string SQLScript = "https://ocbcoin.org/sqlite/masternodoocb.sql";
        private static bool IsDbRecentlyCreated = false;

        public static void Up()
        {
            DirectoryInfo DIR = new DirectoryInfo(@"C:\NodoOCB\data");
            if (!DIR.Exists)
            {
                DIR.Create();
            }

            if (!File.Exists(Path.GetFullPath(DBName)))
            {
                SQLiteConnection.CreateFile(DBName);
                IsDbRecentlyCreated = true;
            }

            using (var ctx = GetInstance())
            {
                if (IsDbRecentlyCreated)
                {
                    string url = SQLScript;
                    System.Net.WebRequest req = System.Net.WebRequest.Create(url);
                    System.Net.WebResponse resp = req.GetResponse();

                    using (var reader = new StreamReader(resp.GetResponseStream()))
                    {
                        var query = "";
                        var line = "";
                        while ((line = reader.ReadLine()) != null)
                        {
                            query += line;
                        }

                        using (var command = new SQLiteCommand(query, ctx))
                        {
                            command.ExecuteNonQuery();
                        }
                    }

                }
            }
        }

        public static SQLiteConnection GetInstance()
        {
            var db = new SQLiteConnection(
                string.Format("Data Source={0};Version=3;", DBName)
            );

            db.Open();

            return db;
        }
    }
}