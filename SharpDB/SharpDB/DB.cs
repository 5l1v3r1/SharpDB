/*
MIT License

Copyright (c) 2018 Jacob Paul

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace SharpDB
{
    public class DB
    {
        private string dbPath;
        private string db;
        private string dbFolder;
        private string currentDbPath;

        public DB(string v)
        {
            dbPath = v;
            dbFolder = dbPath + @"\db";
        }

        public void CreateDatabase(string name)
        {
            //Check if 'db' directory exists, if not: create one
            if (!Directory.Exists(dbFolder)) Directory.CreateDirectory(dbFolder);
            //Check if database exists so we don't overwrite an existing
            if (Directory.Exists(dbFolder + @"\" + name)) throw new Exception("Database already exists");
            else
            {
                //Create a new folder for the database
                Directory.CreateDirectory(dbFolder + @"\" + name);
                File.WriteAllLines(dbFolder + @"\" + name + @"\properties.json", new string[] { "{ name:'" + name +  "',date:'" + DateTime.Now + "',tables:'0'}" });
            }
        }
        public void EnterDatabase(string dbName)
        {
            if (Directory.Exists(dbFolder + @"\" + dbName))
            {
                db = dbName;
                currentDbPath = dbFolder + @"\" + db;
            }
            else throw new Exception("Database does not exist");
        }
        public void CreateTable(string tbName, string[] tbId)
        {
            if (!string.IsNullOrEmpty(db))
            {
                if (!File.Exists(currentDbPath + @"\" + tbName + ".sdb"))
                {
                    string[] conf = { "<name>" + tbName + "</name><culumnLength>" + tbId.Length + "</culumnLength>" };
                    for(int i = 0; i < tbId.Length; i++)
                    {
                        conf[0] += "<tb" + i + ">" + tbId[i] + "</tb" + i + ">";
                    }

                    File.WriteAllLines(currentDbPath + @"\" + tbName + ".sdb", conf);
                }
                else throw new Exception("Table already exists");
            }
            else throw new Exception("No database given (have you run EnterDatabase?)");
        }
        public void Insert(string tbName, string[] tbInfo)
        {
            if (!string.IsNullOrEmpty(db))
            {
                if (File.Exists(currentDbPath + @"\" + tbName + ".sdb"))
                {
                    string[] tb = File.ReadAllLines(currentDbPath + @"\" + tbName + ".sdb");
                    int tbLength = Int32.Parse(ExtractString(tb[0], "culumnLength"));
                    if (tbLength != tbInfo.Length) throw new Exception("Please enter the correct amount of values");
                    else
                    {
                        var _lines = new List<string>(tb);
                        string lineout = "";
                        for(int i = 0; i < tbLength; i++)
                        {
                            lineout += "<" + ExtractString(tb[0], "tb" + i) + ">" + tbInfo[i] + "</" + ExtractString(tb[0], "tb" + i) + ">";
                        }
                        _lines.Add(lineout);
                        var linesArr = _lines.ToArray();
                        //Array.Sort(linesArr);

                        File.WriteAllLines(currentDbPath + @"\" + tbName + ".sdb", linesArr);
                        
                    }
                }
                else throw new Exception("Table does not exist!");
            }
            else throw new Exception("No database given (have you run EnterDatabase?)");
        }
        public string[] Get(string toGet, string tbName, string[] argName, string[] arg)
        {
            var list = new List<string>();
            string[] tb = File.ReadAllLines(currentDbPath + @"\" + tbName + ".sdb");
            int columnLength = tb.Length - 1;
            int su = 0;
            for (int i = 1; i <= columnLength; i++)
            {
                for(int t = 0; t < argName.Length; t++)
                {
                    string wocon = arg[t].Replace("[CONTAINS]", "");
                    if (ExtractString(tb[i], argName[t]) == arg[t] || (arg[t].Contains("[CONTAINS]") && ExtractString(tb[i], argName[t]).Contains(wocon))) su++;
                    else break;
                    
                }
                if (su == argName.Length) list.Add(ExtractString(tb[i], toGet));
                su = 0;

            }
            var listArr = list.ToArray();
            return listArr;
        }
        public void DeleteDatabase(string dataName)
        {
            if (!Directory.Exists(dbFolder + @"\" + dataName)) throw new Exception("Database does not exist");
            else
            {
                DirectoryInfo di = new DirectoryInfo(dbFolder + @"\" + dataName);

                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    dir.Delete(true);
                }
                Directory.Delete(dbFolder + @"\" + dataName);

            }
        }
        public bool DatabaseExists(string dataName)
        {
            if (Directory.Exists(dbFolder + @"\" + dataName)) return true;
            else return false;
        }
        public bool TableExists(string dataName, string tableName)
        {
            if (Directory.Exists(dbFolder + @"\" + dataName))
            {
                if (File.Exists(dbFolder + @"\" + dataName + @"\" + tableName + ".sdb")) return true;
                else return false;
            }
            else return false;
        }
        private string ExtractString(string s, string tag)
        {
            // You should check for errors in real-world code, omitted for brevity
            var startTag = "<" + tag + ">";
            int startIndex = s.IndexOf(startTag) + startTag.Length;
            int endIndex = s.IndexOf("</" + tag + ">", startIndex);
            return s.Substring(startIndex, endIndex - startIndex);
        }
    }
}
