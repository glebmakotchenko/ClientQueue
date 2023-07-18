using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StartClientDemo
{
    public partial class Form5 : Form
    {
        public Form5()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string ip = textBox1.Text + '.' + textBox2.Text + '.' + textBox3.Text + '.' + textBox4;
            int port = Convert.ToInt32(textBox5.Text);

            Form1.ip = ip;
            Form1.port = port;

            Close();
        }
    }
}
