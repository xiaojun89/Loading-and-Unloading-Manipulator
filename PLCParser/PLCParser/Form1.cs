using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.IO;

namespace PLCParser
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private List<string> list = new List<string>();

        private void btn_Parser_Click(object sender, EventArgs e)
        {
            label1.Text = "";
            Application.DoEvents();
            string regTempExpression = comboBox1.Text.ToUpper();
            Regex rg = new Regex(regTempExpression + @"\d+(.\d)*");
            list.Clear();
            listBox1.Items.Clear();
            if (File.Exists(textBox1.Text))
            {
                FileStream fs = new FileStream(textBox1.Text, FileMode.OpenOrCreate);
                StreamReader sr = new StreamReader(fs);

                try
                {
                    while (!sr.EndOfStream)
                    {
                        string s = sr.ReadLine();
                        Match m = rg.Match(s);
                        if (m.Success && list.IndexOf(m.Value) < 0)
                            list.Add(m.Value);
                    }

                }
                finally
                {
                    sr.Close();
                    fs.Close();
                }


                list.Sort(SortMethod);

                for (int i = 0; i < list.Count; i++)
                {
                    listBox1.Items.Add(list[i]);
                }

            }
            else
                MessageBox.Show("文件不存在");

            label1.Text = "Done";


        }

        private int SortMethod(string s1, string s2)
        {
            int result = 0;
            if ((s1 == null) && (s2 == null))
            {
                return 0;
            }
            else if ((s1 != null) && (s2 == null))
            {
                return 1;
            }
            else if ((s1 == null) && (s2 != null))
            {
                return -1;
            }
            if (s1.Equals(s2))
                return 0;

            string temp1 = s1.Substring(1);
            string temp2 = s2.Substring(1);

            try
            {
                double comp1 = Double.Parse(temp1);
                double comp2 = Double.Parse(temp2);
                if (comp1 > comp2)
                    result = 1;
                else if (comp1 < comp2)
                    result = -1;
                else if (temp1.Length < temp2.Length)//temp1是整数
                    result = -1;
                else if (temp1.Length > temp2.Length)//temp2是整数
                    result = 1;
                else
                    result = 0;
            }
            catch
            {
                result = -1;
            }
            return result;
        }
        private void btn_browse_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = openFileDialog1.FileName;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            label1.Text = "";
            Application.DoEvents();

            string SaveFileName = openFileDialog1.FileName.Substring(0, openFileDialog1.FileName.LastIndexOf('\\') + 1) + @"address.txt";
            
            if (File.Exists(SaveFileName))
                File.Delete(SaveFileName);

            using (FileStream fs = new FileStream(SaveFileName, FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    foreach (var item in list)
                    {
                        sw.WriteLine(item);
                    }
                }
            }
            label1.Text = "Done";

        }

        private void button2_Click(object sender, EventArgs e)
        {
            label1.Text = "";
            Application.DoEvents();

            string SaveFileName = openFileDialog1.FileName.Substring(0, openFileDialog1.FileName.LastIndexOf('\\') + 1) + @"address.txt";

            if (!File.Exists(SaveFileName))
                File.Create(SaveFileName);

            using (FileStream fs = new FileStream(SaveFileName, FileMode.Append))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    foreach (var item in list)
                    {
                        sw.WriteLine(item);
                    }
                }
            }

            label1.Text = "Done";

        }
    }
}
