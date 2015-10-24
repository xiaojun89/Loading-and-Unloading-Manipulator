using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;

namespace SuperReplace
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public string FileName = string.Empty;
        public string ToReplaceListFileName = string.Empty;
        public string ToOpenXml = string.Empty;
        public string ToSaveXml = string.Empty;
        public string SaveFileName = string.Empty;

        public Dictionary<string, string> Addresses;
        public Dictionary<string, string> Comments;

        public XmlDocument doc;

        private void button1_Click(object sender, EventArgs e)
        {
            label3.Text = "";
            label6.Text = "";
            Application.DoEvents();

            Addresses = new Dictionary<string, string>();
            Comments = new Dictionary<string, string>();

            LoadParaConfigFile(ToOpenXml);
            LoadToReplaceFile();

            if (LoadLadFile(FileName))
            {
                label3.Text = "LAD转换成功";
            }
            else
            {
                label3.Text = "LAD转换失败";
            }

            if (SaveNewXML(ToSaveXml))
            {
                label6.Text = "XML生成成功";
            }
            else
            {
                label6.Text = "XML生成失败";
            }

        }

        public bool LoadToReplaceFile()
        {
            bool result = false;
            try
            {
                if (ToReplaceListFileName != string.Empty && File.Exists(ToReplaceListFileName))
                {
                    using (FileStream fs = new FileStream(ToReplaceListFileName, FileMode.Open))
                    {
                            using (StreamReader sr = new StreamReader(fs, Encoding.Default))
                            {
                                    while (!sr.EndOfStream)
                                    {
                                        string item = sr.ReadLine();
                                        string[] itempair = item.Split(new char[] {' ', '\t'}, StringSplitOptions.RemoveEmptyEntries);
                                        int count = itempair.Length;
                                        if (count >= 2)
                                        {
                                            if (!Addresses.ContainsKey(itempair[0]))
                                            {
                                                Addresses.Add(itempair[0], itempair[1]);
                                            }
                                        }
                                        if (count >= 3)
                                        {
                                            if (!Comments.ContainsKey(itempair[0]))
                                            {
                                                Comments.Add(itempair[0], itempair[2]);
                                            }
                                            else
                                            {
                                                Comments[itempair[0]] = itempair[2];
                                            }
                                            if (!Comments.ContainsKey(itempair[1]))
                                            {
                                                Comments.Add(itempair[1], Comments[itempair[0]].Replace("1#", "2#"));
                                            }
                                            else
                                            {
                                                Comments[itempair[1]] = Comments[itempair[0]].Replace("1#", "2#");
                                            }


                                        }

                                    }
                            }
                    }
                    result = true;
                }
            }
            catch
            {
                result = false;
            }
            return result;
        }

        //替换LAD文件的过程
        public bool LoadLadFile(string filename)
        {
            bool result = false;
            Regex rg = new Regex(@"[XYRGFD]\d+(.\d)*");
            Regex commentRegex = new Regex("1#");
            try
            {
                if (filename != string.Empty && File.Exists(filename))
                {
                    using (FileStream fs = new FileStream(filename, FileMode.Open))
                    {
                        if (File.Exists(SaveFileName))
                            File.Delete(SaveFileName);

                        using (FileStream wfs = new FileStream(SaveFileName, FileMode.Create))
                        {
                            using (StreamReader sr = new StreamReader(fs, Encoding.Unicode))
                            {
                                using (StreamWriter sw = new StreamWriter(wfs))
                                {
                                    while (!sr.EndOfStream)
                                    {
                                        string sourseLine = sr.ReadLine();
                                        string newLine = sourseLine;
                                        Match commentMatch = commentRegex.Match(sourseLine);
                                        if (commentMatch.Success)
                                        {
                                            newLine = sourseLine.Replace(commentMatch.Value, "2#");
                                        }
                                        Match m = rg.Match(sourseLine);
                                        if (m.Success && Addresses.ContainsKey(m.Value))
                                        {
                                            newLine = sourseLine.Replace(m.Value, Addresses[m.Value]);
                                        }
                                        sw.WriteLine(newLine);
                                    }
                                }
                            }
                        }
                    }
                }
                result = true;
            }
            catch
            {
                result = false;
            }
            return result;
        }

        //加载现有的注释信息
        public void LoadParaConfigFile(string filename)
        {
            try
            {
                if (filename != string.Empty && File.Exists(filename))
                {
                    doc = new XmlDocument();
                    doc.Load(filename);
                    XmlElement _pararoot = doc.DocumentElement;

                    foreach (XmlNode n in _pararoot.ChildNodes)
                    {
                        XmlElement xe1 = (XmlElement)n;
                        if (n.Name == "注释")
                        {
                            foreach (XmlNode para in n.ChildNodes)
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
                    }

                }
            }
            catch
            {
            }
        }

        //生成XML文件
        public bool SaveNewXML(string filename)
        {
            bool result = false;
            try
            {
                if (filename != string.Empty && File.Exists(filename))
                {
                    if (File.Exists(filename))
                        File.Delete(filename);
                }
                XmlNode xn = doc.SelectSingleNode("//注释");
                xn.RemoveAll();
                foreach (var pair in Comments)
                {
                    XmlElement xe = doc.CreateElement(pair.Key);
                    xe.SetAttribute("Text", pair.Value);
                    xn.AppendChild(xe);
                }
                //using (FileStream fs = new FileStream(filename, FileMode.Create))
                {
                    doc.Save(filename);
                }
                result = true;
            }
            catch
            {
                result = false;
            }
            return result;

        }
        //选择需要替换的源LAD文件
        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = @"D:\20151012\Zhonglun-Robot-PLC\workingPLC";
            openFileDialog1.Filter = "LAD文件|*.lad|TXT文件|*.txt";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                FileName = openFileDialog1.FileName;
                label1.Text = FileName;
            }
            SaveFileName = FileName.Substring(0, FileName.LastIndexOf('\\') + 1) + @"robot2.lad";
            label2.Text = SaveFileName;
            ToReplaceListFileName = FileName.Substring(0, FileName.LastIndexOf('\\') + 1) + @"ToReplace.txt";
            label4.Text = ToReplaceListFileName;
        }

        //选择替换后的目标LAD文件
        private void button3_Click(object sender, EventArgs e)
        {
            saveFileDialog1.DefaultExt = "lad";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                SaveFileName = saveFileDialog1.FileName;
                label2.Text = SaveFileName;
            }
        }

        //选择用于指定替换文本的替换字典
        private void button4_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "TXT文件|*.txt";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                ToReplaceListFileName = openFileDialog1.FileName;
                label4.Text = ToReplaceListFileName;
            }

        }

        //选择需要替换的源XML文件
        private void button6_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "XML文件|*.xml";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                ToOpenXml = openFileDialog1.FileName;
                ToSaveXml = ToOpenXml.Substring(0, ToOpenXml.LastIndexOf('\\') + 1) + @"ZL-WXM-ROBOT-ALL.XML";
                label5.Text = ToOpenXml;
                label7.Text = ToSaveXml;
            }

        }

    }
}
