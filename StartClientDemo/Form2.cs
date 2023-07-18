using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace StartClientDemo
{
    public partial class Form2 : Form
    {
        private Dictionary<string, string> _serviceWithId;

        public Form2(Dictionary<string, string> serviceWithId)
        {
            InitializeComponent();

            _serviceWithId = serviceWithId;
            foreach (var service in _serviceWithId.Values)
            {
                comboBox1.Items.Add(service);
            }
            comboBox1.SelectedIndex = 0;
        }

        public static void gjh(Dictionary<string, string> _serviceWithId, RichTextBox rh, ComboBox cb, CheckBox ch)
        {
            foreach (var kwv in _serviceWithId)
            {
                if (kwv.Value == cb.SelectedItem.ToString())
                {
                    Form1.Args.serviceId = kwv.Key;
                }
            }

            Form1.Args.comments = rh.Text;
            Form1.Args.returned = ch.Checked;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            gjh(_serviceWithId, richTextBox1, comboBox1, checkBox1);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
