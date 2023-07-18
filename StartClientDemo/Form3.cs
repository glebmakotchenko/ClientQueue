using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static StartClientDemo.Form1;

namespace StartClientDemo
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
            radioButton1.Checked = true;
            checkBox1.Enabled = true;

            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;

            dateTimePicker1.Format = DateTimePickerFormat.Custom;
            dateTimePicker1.CustomFormat = "HH:mm";
            DateTime now = DateTime.Now;
            dateTimePicker1.Value = now;
            dateTimePicker1.Value = dateTimePicker1.Value.AddMinutes(35);
        }

        public string setTimer()
        {
            string timer = "";

            if (radioButton1.Checked)
            {
                if (comboBox2.SelectedItem.ToString() == "Бессрочно")
                {
                    timer = comboBox2.SelectedItem.ToString();
                }
                else
                {
                    string[] words = comboBox2.SelectedItem.ToString().Split(' ');
                    timer = words[0];
                }
            }
            else
            {
                if (dateTimePicker1.Value > DateTime.Now)
                {
                    timer = dateTimePicker1.Value.ToShortTimeString();
                }
                else
                {
                    dateTimePicker1.Value = DateTime.Now;
                    dateTimePicker1.Value = dateTimePicker1.Value.AddMinutes(35);
                    timer = dateTimePicker1.Value.ToShortTimeString();
                }
                //timer = dateTimePicker1.Value.ToShortTimeString();
                string[] timers = timer.Split(':');
                int[] time = new int[timers.Length];
                time[0] = Convert.ToInt32(timers[0]);
                time[1] = Convert.ToInt32(timers[1]);
                timer = ((time[0] - DateTime.Now.Hour) * 60 + (time[1] - DateTime.Now.Minute)).ToString();
            }

            return timer;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            dateTimePicker1.Enabled = !radioButton1.Checked;
            if (comboBox2.SelectedIndex != 0)
            {
                //checkBox1.Enabled = false;
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            comboBox2.Enabled = !radioButton2.Checked;
            //checkBox1.Enabled = !radioButton2.Checked;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Console.WriteLine(setTimer());
            timerArgs.result = comboBox1.SelectedItem.ToString();
            timerArgs.timerValue = setTimer();
            timerArgs.onlyMyClient = checkBox1.Checked;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox2.SelectedIndex != 0) 
            {
                checkBox1.Checked = false;
                //checkBox1.Enabled = false;
            }
            else
            {
                //checkBox1.Enabled = true;
            }
        }
    }
}
