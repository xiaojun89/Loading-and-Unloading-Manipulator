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
        public string SaveFileName = string.Empty;

        public List<string> SourseAddress = new List<string>();
        public List<string> TargetAddress = new List<string>();
        public Dictionary<string, string> Addresses;

        public void AddList(string source, string target)
        {
            SourseAddress.Add(source);
            TargetAddress.Add(target);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            label3.Text = "";
            Application.DoEvents();

            Addresses = new Dictionary<string, string>();

            LoadToReplaceFile();

            if (LoadLadFile(FileName))
            {
                label3.Text = "成功";
            }
            else
            {
                label3.Text = "失败";
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
                            using (StreamReader sr = new StreamReader(fs))
                            {
                                    while (!sr.EndOfStream)
                                    {
                                        string item = sr.ReadLine();
                                        string[] itempair = item.Split(new char[] {' ', '\t'}, StringSplitOptions.RemoveEmptyEntries);
                                        if (!Addresses.ContainsKey(itempair[0]))
                                        {
                                            Addresses.Add(itempair[0], itempair[1]);
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

        public bool LoadLadFile(string filename)
        {
            bool result = false;
            Regex rg = new Regex(@"[XYRGFD]\d+(.\d)+");
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
        
        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = @"D:\20151012\Zhonglun-Robot-PLC\workingPLC";
            openFileDialog1.Filter = "LAD文件|*.lad|TXT文件|*.txt";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                FileName = openFileDialog1.FileName;
                label1.Text = FileName;
            }
            //SaveFileName = FileName.Substring(0, FileName.LastIndexOf('\\') + 1) + @"double_right.txt";
            SaveFileName = FileName.Substring(0, FileName.LastIndexOf('\\') + 1) + @"robot2.lad";
            label2.Text = SaveFileName;
            ToReplaceListFileName = FileName.Substring(0, FileName.LastIndexOf('\\') + 1) + @"ToReplace.txt";
            label4.Text = ToReplaceListFileName;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            saveFileDialog1.DefaultExt = "lad";
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
                ToReplaceListFileName = openFileDialog1.FileName;
                label4.Text = ToReplaceListFileName;
            }

        }
    }
}
