using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;

namespace IO_Generator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            AddList("X", 0, 3);
            AddList("X", 10, 17);
            AddList("Y", 0, 2);
            AddList("Y", 10, 15);
        }

        public string FileName = string.Empty;
        public string AddressTable = string.Empty;
        public string SaveFileName = string.Empty;

        public List<string> Type = new List<string>();
        public List<int> MajorStart = new List<int>();
        public List<int> MajorEnd = new List<int>();
        public Dictionary<string, string> Comments;

        public void AddList(string stype, int start, int end)
        {
            Type.Add(stype);
            MajorStart.Add(start);
            MajorEnd.Add(end);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            label3.Text = "";
            Application.DoEvents();

            Comments = new Dictionary<string, string>();

            for (int i = 0; i < Type.Count; i++)
            {
                for (int j = MajorStart[i]; j <= MajorEnd[i]; j++)
                {
                    for (int k = 0; k < 8; k++)
                    {
                        string key = Type[i] + j.ToString() + "." + k.ToString();
                        Comments.Add(key, string.Empty);
                    }
                }
            }

            if (AddressTable != string.Empty && File.Exists(AddressTable))
            {
                using (FileStream fs = new FileStream(AddressTable, FileMode.Open))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        while (!sr.EndOfStream)
                        {
                            string sourseLine = sr.ReadLine();
                            if (!Comments.ContainsKey(sourseLine))
                            {
                                Comments.Add(sourseLine, string.Empty);
                            }
                        }
                    }
                }

            }


            LoadParaConfigFile(FileName);

            using (FileStream fs = new FileStream(SaveFileName, FileMode.OpenOrCreate))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    foreach (var item in Comments)
                    {
                        sw.WriteLine(string.Format("    <{0} Text=\"{1}\" />", item.Key, item.Value));
                    }
                }
            }

            label3.Text = "成功";
        }

        public void LoadParaConfigFile(string filename)
        {
            try
            {
                if (filename != string.Empty && File.Exists(filename))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(filename);
                    XmlElement _pararoot = doc.DocumentElement;

                    foreach (XmlNode n in _pararoot.ChildNodes)
                    {
                        XmlElement xe1 = (XmlElement)n;
                        if (n.Name == "端口诊断")
                        {
                            foreach (XmlNode m in n.ChildNodes)
                            {
                                AddValue(m);
                            }
                        }
                        else if (n.Name == "注释")
                        {
                            AddValue(n);
                        }
                    }

                }
            }
            catch
            {
            }
        }
        public void AddValue(XmlNode m)
        {
            foreach (XmlNode para in m.ChildNodes)
            {
                XmlElement xe = (XmlElement)para;
                if (xe.HasAttribute("Text"))
                {
                    if (Comments.ContainsKey(xe.Name))
                    {
                        string s1 = Comments[xe.Name];
                        string s2 = xe.GetAttribute("Text");
                        if (s1.Length == 0)
                            Comments[xe.Name] = s2;
                        else if (s2.Length == 0)
                            Comments[xe.Name] = s1;
                        else
                            Comments[xe.Name] = (s1.Length >= s2.Length) ? s2 : s1;
                    }
                    else
                    {
                        Comments.Add(xe.Name, xe.GetAttribute("Text"));
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "XML文件|*.xml";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                FileName = openFileDialog1.FileName;
                label1.Text = FileName;
            }
            SaveFileName = FileName.Substring(0, FileName.LastIndexOf('\\') + 1) + @"helloworld.txt";
            label2.Text = SaveFileName;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            saveFileDialog1.DefaultExt = "txt";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                SaveFileName = saveFileDialog1.FileName;
                label2.Text = SaveFileName;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "TXT文件|*.txt";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                AddressTable = openFileDialog1.FileName;
                label4.Text = AddressTable;
            }
        }


    }
}
