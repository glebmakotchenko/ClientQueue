//
// Поменяй выбор пользователя
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Threading;
using System.Timers;
using System.IO;

namespace StartClientDemo
{
    public partial class Form1 : Form
    {
        static string ipWithPort = "";
        public static string ip = "";
        public static int port = 0;
        public static int sum = 0;

        class IniFile
        {
            private readonly string filePath;

            public IniFile(string filePath)
            {
                this.filePath = filePath;
            }

            public string ReadValue(string section, string key)
            {
                var stringBuilder = new StringBuilder(255);
                NativeMethods.GetPrivateProfileString(section, key, "", stringBuilder, 255, filePath);
                return stringBuilder.ToString();
            }
        }

        static class NativeMethods
        {
            [System.Runtime.InteropServices.DllImport("kernel32.dll")]
            internal static extern int GetPrivateProfileString(string section, string key, string defaultValue, StringBuilder value, int size, string filePath);
        }



        public Form1()
        {
            string path = "ip_with_port.txt";
            // асинхронное чтение
            using (StreamReader reader = new StreamReader(path))
            {
                ipWithPort = reader.ReadToEndAsync().Result.ToString();
            }
            string[] words = ipWithPort.Split(new char[] { ':' });
            ip = words[0];
            port = Convert.ToInt32(words[1]);


            /*var iniFilePath = "config.ini";
            var iniFile = new IniFile(iniFilePath);

            var ip = iniFile.ReadValue("Server", "IP");
            var port = iniFile.ReadValue("Server", "Port");*/

            Console.WriteLine($"IP: {ip}, Port: {port}");

            InitializeComponent();
            getUserName(); // получили логин, делаем запрос по на список всех пользователей и ищем по логину user_id
            getUserID();

            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
            button5.Enabled = false;
            button6.Enabled = false;

            пригласитьToolStripMenuItem.Enabled = false;
            отклонитьToolStripMenuItem.Enabled = false;
            начатьПриемToolStripMenuItem.Enabled = false;
            перенаправитьToolStripMenuItem.Enabled = false;
            отложитьToolStripMenuItem.Enabled = false;
            завершитьToolStripMenuItem.Enabled = false;

            addTreeView(cnnToServer("Получить состояние очередей с проверкой"));
            JObject resp = cnnToServer("Получить состояние очередей");
            addTreeView(resp);
            getLastState(resp);

            cnnToServer("Получить перечень услуг");

            timer1.Start();
            
        }

        Dictionary<string, string> serviceWithId = new Dictionary<string, string>();
        Dictionary<string, string> customerComments = new Dictionary<string, string>();

        string selectServiceid = "";
        public static class customerInfo
        {
            public static string Id;
            public static string Num;
        }
        Dictionary<string, string> customerWithId = new Dictionary<string, string>();
        string userId = "";
        string customerId = "";

        public static class Args
        {
            public static string serviceId;
            public static string comments;
            public static bool returned;
        }

        public static class timerArgs
        {
            public static string result;
            public static string timerValue;
            public static bool onlyMyClient = false;
        }

        private void getLastState(JObject resp)
        {
            //JObject resp = cnnToServer("Получить состояние очередей");

            Console.WriteLine(resp);

            addTreeView(resp);
            
            /*if (resp["result"]["postponed"] != null)
            {
                if (resp["result"]["postponed"].ToString() != "")
                {
                    for (int i = 0; i < resp["result"]["postponed"].Count(); i++)
                    {
                        if (!customerWithId.ContainsKey(resp["result"]["postponed"][i]["prefix"].ToString() + resp["result"]["postponed"][i]["number"].ToString()))
                        {
                            customerWithId.Add(resp["result"]["postponed"][i]["prefix"].ToString() + resp["result"]["postponed"][i]["number"].ToString(), resp["result"]["postponed"][i]["id"].ToString());
                            string itemText = resp["result"]["postponed"][i]["prefix"].ToString() + resp["result"]["postponed"][i]["number"].ToString()
                                + " " + resp["result"]["postponed"][i]["post_status"].ToString().Replace('+', ' ')
                                + " " + (Convert.ToInt32(resp["result"]["postponed"][i]["postpone_period"].ToString()) > 0 ? resp["result"]["postponed"][i]["postpone_period"].ToString() : "Бессрочно")
                                + " min." + ((resp["result"]["postponed"][i]["is_mine"] != null && resp["result"]["postponed"][i]["is_mine"].ToString() != "") ? " Private!" : "");
                            if (Convert.ToInt32(resp["result"]["postponed"][i]["postpone_period"].ToString()) > 0)
                            {
                                int interval = (int)(Convert.ToInt64(resp["result"]["postponed"][i]["finish_postpone_period"].ToString()) - Convert.ToInt64(getCurrTime()));
                                addItemWithTimer(interval, itemText);
                            }
                            else
                            {
                                listBox1.Items.Add(itemText);
                            }
                        }
                    }
                }
            }*/

            if (resp != null && resp["result"]["customer"] != null)
            {
                if (resp["result"]["customer"].ToString() != "")
                {
                    customerInfo.Id = resp["result"]["customer"]["id"].ToString();
                    customerInfo.Num = resp["result"]["customer"]["prefix"].ToString() + resp["result"]["customer"]["number"].ToString();

                    if (resp["result"]["customer"]["state"].ToString().Contains("STATE_INVITED"))
                    {
                        button1.Enabled = true;
                        button2.Enabled = true;
                        button3.Enabled = true;

                        пригласитьToolStripMenuItem.Enabled = true;
                        отклонитьToolStripMenuItem.Enabled = true;
                        начатьПриемToolStripMenuItem.Enabled = true;
                    }

                    if (resp["result"]["customer"]["state"].ToString().Contains("STATE_WORK"))
                    {
                        button4.Enabled = true;
                        button5.Enabled = true;
                        button6.Enabled = true;

                        перенаправитьToolStripMenuItem.Enabled = true;
                        отложитьToolStripMenuItem.Enabled = true;
                        завершитьToolStripMenuItem.Enabled = true;
                    }

                    infoTable.Clear();
                    string num = '\n' + resp["result"]["customer"]["prefix"].ToString() + resp["result"]["customer"]["number"].ToString() + '\n';
                    infoTable.AppendText(num);
                    infoTable.SelectionStart = 0;
                    int textSize = infoTable.TextLength - 1;
                    infoTable.SelectionLength = textSize;
                    infoTable.SelectionColor = Color.Red;
                    infoTable.SelectionAlignment = HorizontalAlignment.Center;
                    infoTable.SelectionFont = new Font("Microsoft Sans Serif", 30);
                    infoTable.AppendText("Услуга: " + resp["result"]["customer"]["to_service"]["name"].ToString().Replace('+', ' '));
                    infoTable.SelectionStart = textSize;
                    infoTable.SelectionLength = infoTable.TextLength;
                    infoTable.SelectionAlignment = HorizontalAlignment.Left;
                    infoTable.SelectionFont = new Font("Microsoft Sans Serif", 14);
                }
            }
        }

        public string getCurrTime() // возвращает текущую дату для id запроса (id == текущая дата в миллисекундах)
        {
            return DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();
        }

        private void getUserID()
        {
            JObject resp = cnnToServer("Получить перечень пользователей");

            if (resp != null)
            {
                for (int i = 0; i < resp["result"].Count(); i++)
                {
                    if (Environment.UserName == resp["result"][i]["name"].ToString()) // Удалить перед релизом, тк нет списка пользователей, ставиим дефолтного второго
                    {
                        userId = resp["result"][i]["id"].ToString();
                        for (int j = 0; j < resp["result"][i]["plan"].Count(); j++)
                        {
                            string serviceId = resp["result"][i]["plan"][j]["service"]["id"].ToString().Replace('+', ' ');
                            string serviceName = resp["result"][i]["plan"][j]["service"]["name"].ToString().Replace("+", " ");

                            //serviceWithId.Add(serviceId /*serviceId = resp["result"][i]["plan"][j]["id"]*/, serviceName);

                            string layout = "- " + serviceName + ": ";
                            listBox2.Items.Add(layout);
                        }
                    }
                }
                if (userId == "")
                {
                    DialogResult result = MessageBox.Show("Пользователя с таким идентификатором не сущесвтует",
                        "Ошибка",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1,
                        MessageBoxOptions.DefaultDesktopOnly);
                    if (result == DialogResult.OK)
                    {
                        Environment.Exit(1);
                    }
                }
                else
                {
                    iDToolStripMenuItem.Text += " = " + userId;
                }
            }
            else
            {
                DialogResult result = MessageBox.Show("Не удалось подключиться к серверу",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.DefaultDesktopOnly);
                if (result == DialogResult.OK)
                {
                    Environment.Exit(1);

                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            addTreeView(cnnToServer("Получить состояние очередей"));
        }

        private void getUserName()
        {
            label7.Text += " " + Environment.UserName;
            notCalled();
        }

        private JObject cnnToServer(string method)
        {
            TcpClient client = new TcpClient();
            var result = client.BeginConnect(ip, port, null, null);

            var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1));

            if (!success)
            {
                label2.Text = "Не подключен";
                label2.ForeColor = Color.Red;

                return null;
            }
            else
            {
                label2.Text = "Подключен";
                label2.ForeColor = Color.Green;

                for (int i = 0; i < 5; i++)
                {
                    Console.WriteLine(client.Connected);
                }

                if (!client.Connected)
                {
                    MessageBox.Show("не удалось подключиться к серверу");
                }

                JObject request = new JObject();
                request["jsonrpc"] = "2.0";
                request["method"] = method;
                request["id"] = getCurrTime();
                if ((method != "Получить перечень пользователей") && (method != "Получить перечень услуг") && (method != "Получить получение списка возможных результатов"))
                {
                    request["params"] = new JObject();
                    request["params"]["user_id"] = userId;

                    if (method == "Закончить работу с клиентом")
                    {
                        request["params"]["result_id"] = -1;
                        request["params"]["text_data"] += (customerComments.ContainsKey(customerInfo.Id) ? customerComments[customerInfo.Id] : "");
                    }

                    if (method == "Переадресовать клиента к другой услуге")
                    {
                        request["params"]["result_id"] = -1;
                        request["params"]["service_id"] = Args.serviceId;
                        request["params"]["text_data"] += (customerComments.ContainsKey(customerInfo.Id) ? customerComments[customerInfo.Id] : "");
                        request["params"]["request_back"] = Args.returned;
                        request["params"]["customer_id"] = customerInfo.Id;

                    }

                    if (method == "Клиента в пул отложенных")
                    {
                        request["params"]["text_data"] = timerArgs.result;
                        request["params"]["customer_id"] = customerInfo.Id;
                        request["params"]["postponed_period"] = timerArgs.timerValue == "Бессрочно" ? "0" : timerArgs.timerValue;
                        if (timerArgs.onlyMyClient == true) // Эта функция работает некорректно в исходной версии, нужно подумать и довести до ума
                        {
                            request["params"]["is_only_mine"] = userId;
                        }
                    }

                    if (method == "Вызвать отложенного из пула отложенных")
                    {
                        request["params"]["customer_id"] = customerId;
                    }

                    if (method == "Сменить статус отложенному")
                    {
                        request["params"]["customer_id"] = customerId;
                        request["params"]["text_data"] = form4Args.result;
                    }

                    if (method == "Перерыв оператора")
                    {
                        request["params"]["request_back"] = checkBox1.Checked.ToString();
                    }
                }

                string jsonRequest = JsonConvert.SerializeObject(request);

                byte[] data = Encoding.UTF8.GetBytes(jsonRequest);
                NetworkStream stream = client.GetStream();
                stream.Write(data, 0, data.Length);

                byte[] buffer = new byte[2048];
                MemoryStream memoryStream = new MemoryStream();
                int bytesRead;
                do
                {
                    bytesRead = client.GetStream().Read(buffer, 0, buffer.Length);
                    memoryStream.Write(buffer, 0, bytesRead);
                } while (bytesRead != 0);

                string jsonResponse = Encoding.UTF8.GetString(memoryStream.ToArray());

                string response = Uri.UnescapeDataString(jsonResponse);

                if (response != "")
                {
                    if (response[response.Length - 1] == '+')
                    {
                        int ind = response.Length - 10;
                        response = response.Remove(ind);
                    }

                    Console.WriteLine(method);
                    Console.WriteLine(response);

                    JObject responseForm = JObject.Parse(response);
                    client.Close();

                    Console.WriteLine(responseForm["error"]);

                    return responseForm;
                }

                JObject responseForm_ = new JObject();
                client.Close();

                return responseForm_;
            }
            return null;
        }

        private int getAmountCustomers(JObject response)
        {
            int amount = 0;

            if (response["result"].Count() > 1)
            {
                for (int i = 0; i < response["result"]["self_services"].Count(); i++)
                {
                    amount += (int)response["result"]["self_services"][i]["waiting"];
                }
            }

            return amount;
        }

        private void addTreeView(JObject resp) //
        {
            if (resp != null)
            {
                Console.WriteLine(resp["result"].Count());
                if (resp["result"].Count() > 1)
                {
                    treeView1.Nodes.Clear();
                    int totalCountCustomers = 0;
                    label6.Text = "Всего клиентов:";
                    int item = 0;

                    for (int i = 0; i < resp["result"]["self_services"].Count(); i++)
                    {
                        totalCountCustomers += (int)resp["result"]["self_services"][i]["waiting"];
                        if (resp["result"]["self_services"][i]["line"].Count() != 0)
                        {
                            string serviceName = resp["result"]["self_services"][i]["service_name"].ToString().Replace('+', ' ');

                            for (int k = 0; k < listBox2.Items.Count; k++)
                            {
                                if (listBox2.Items[k].ToString().Contains(serviceName))
                                {
                                    if (listBox2.Items[k].ToString().Contains("человек"))
                                    {
                                        int index = listBox2.Items[k].ToString().IndexOf(':');
                                        listBox2.Items[k] = listBox2.Items[k].ToString().Remove(index + 1);
                                    }
                                    string newItemText = listBox2.Items[k].ToString() + ' ' + resp["result"]["self_services"][k]["line"].Count().ToString() + " человек";
                                    listBox2.Items[k] = newItemText;
                                }
                            }

                            treeView1.Nodes.Add(serviceName);

                            for (int j = 0; j < resp["result"]["self_services"][i]["line"].Count(); j++)
                            {
                                string customerInfo = resp["result"]["self_services"][i]["line"][j]["num"].ToString() + ' ' +
                                    resp["result"]["self_services"][i]["line"][j]["waiting"].ToString() + " минут";

                                treeView1.Nodes[item].Nodes.Add(new TreeNode(customerInfo)); // Услуги пердоставляются не по порядку, поэтому он кидает исключение
                            }
                            item++;
                        }
                        else
                        {
                            string serviceName = resp["result"]["self_services"][i]["service_name"].ToString().Replace('+', ' ');

                            for (int k = 0; k < listBox2.Items.Count; k++)
                            {
                                if (listBox2.Items[k].ToString().Contains(serviceName))
                                {
                                    if (listBox2.Items[k].ToString().Contains("человек"))
                                    {
                                        int index = listBox2.Items[k].ToString().IndexOf(':');
                                        listBox2.Items[k] = listBox2.Items[k].ToString().Remove(index + 1);
                                    }
                                }
                            }
                        }
                    }
                    label6.Text += ' ' + totalCountCustomers.ToString();

                    if (getAmountCustomers(resp) != 0)
                    {
                        button1.Enabled = true;
                        пригласитьToolStripMenuItem.Enabled = true;
                    }

                    if (resp["result"]["postponed"] != null)
                    {
                        if (resp["result"]["postponed"].ToString() != "")
                        {
                            for (int i = 0; i < resp["result"]["postponed"].Count(); i++)
                            {
                                if (!customerWithId.ContainsKey(resp["result"]["postponed"][i]["prefix"].ToString() + resp["result"]["postponed"][i]["number"].ToString()))
                                {
                                    customerWithId.Add(resp["result"]["postponed"][i]["prefix"].ToString() + resp["result"]["postponed"][i]["number"].ToString(), resp["result"]["postponed"][i]["id"].ToString());
                                    string itemText = resp["result"]["postponed"][i]["prefix"].ToString() + resp["result"]["postponed"][i]["number"].ToString()
                                        + " " + resp["result"]["postponed"][i]["post_status"].ToString().Replace('+', ' ')
                                        + " " + (Convert.ToInt32(resp["result"]["postponed"][i]["postpone_period"].ToString()) > 0 ? resp["result"]["postponed"][i]["postpone_period"].ToString() : "Бессрочно")
                                        + " min." + ((resp["result"]["postponed"][i]["is_mine"] != null && resp["result"]["postponed"][i]["is_mine"].ToString() != "") ? " Private!" : "");
                                    if (Convert.ToInt32(resp["result"]["postponed"][i]["postpone_period"].ToString()) > 0)
                                    {
                                        int interval = (int)(Convert.ToInt64(resp["result"]["postponed"][i]["finish_postpone_period"].ToString()) - Convert.ToInt64(getCurrTime()));
                                        addItemWithTimer(interval, itemText);
                                    }
                                    else
                                    {
                                        listBox1.Items.Add(itemText);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void notCalled()
        {
            infoTable.Clear();
            infoTable.AppendText("\nНе вызван");
            infoTable.SelectionStart = 1;
            infoTable.SelectionLength = infoTable.TextLength;
            infoTable.SelectionAlignment = HorizontalAlignment.Center;
            infoTable.SelectionFont = new Font("Microsoft Sans Serif", 30);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            cnnToServer("Получить состояние очередей"); // Добавил недавно, Подумай, стоит ли оставлять
            JObject resp = cnnToServer("Получить следующего клиента");
            if (resp != null)
            {
                customerInfo.Id = resp["result"]["id"].ToString();
                customerInfo.Num = resp["result"]["prefix"].ToString() + resp["result"]["number"].ToString();
                string num = "";
                if (!customerComments.ContainsKey(customerInfo.Id))
                {
                    customerComments.Add(customerInfo.Id, "");
                }

                if (resp["result"]["temp_comments"] != null)
                {
                    if (resp["result"]["temp_comments"].ToString() != "")
                    {
                        if ((customerComments.ContainsKey(customerInfo.Id)) && (customerComments[customerInfo.Id] == ""))
                        {
                            customerComments[customerInfo.Id] += resp["result"]["temp_comments"].ToString();
                        }
                        Console.WriteLine(resp["result"]["temp_comments"].ToString());
                        comments1.Text = resp["result"]["temp_comments"].ToString().Replace('+', ' ');
                    }
                }

                button2.Enabled = true;
                button3.Enabled = true;

                отклонитьToolStripMenuItem.Enabled = true;
                начатьПриемToolStripMenuItem.Enabled = true;

                infoTable.Clear();
                num = '\n' + resp["result"]["prefix"].ToString() + resp["result"]["number"].ToString() + '\n';
                infoTable.AppendText(num);
                infoTable.SelectionStart = 0;
                int textSize = infoTable.TextLength - 1;
                infoTable.SelectionLength = textSize;
                infoTable.SelectionColor = Color.Red;
                infoTable.SelectionAlignment = HorizontalAlignment.Center;
                infoTable.SelectionFont = new Font("Microsoft Sans Serif", 30);
                infoTable.AppendText("Услуга: " + resp["result"]["to_service"]["name"].ToString().Replace('+', ' '));
                infoTable.SelectionStart = textSize;
                infoTable.SelectionLength = infoTable.TextLength;
                infoTable.SelectionAlignment = HorizontalAlignment.Left;
                infoTable.SelectionFont = new Font("Microsoft Sans Serif", 14);
            }
            //infoTable.Refresh();
            //infoTable.Text += "Услуга: " + resp["result"]["to_service"]["name"].ToString().Replace('+', ' ');
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Вы точно хотите удалить клиента из очереди?",
                "Удаление клиента",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.DefaultDesktopOnly);

            if (result == DialogResult.Yes)
            {
                if (cnnToServer("Удалить следующего клиента") != null)
                {
                    JObject resp = cnnToServer("Получить состояние очередей");
                    if (resp != null)
                    {
                        addTreeView(resp);
                        notCalled();
                        comments1.Clear();
                        customerComments.Remove(customerInfo.Id);

                        if (getAmountCustomers(resp) > 0)
                        {
                            button1.Enabled = true;
                            пригласитьToolStripMenuItem.Enabled = true;
                        }
                        else
                        {
                            button1.Enabled = false;
                            пригласитьToolStripMenuItem.Enabled = false;
                        }

                        button2.Enabled = false;
                        button3.Enabled = false;

                        отклонитьToolStripMenuItem.Enabled = false;
                        начатьПриемToolStripMenuItem.Enabled = false;
                    }
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (cnnToServer("Начать работу с клиентом") != null)
            {
                button1.Enabled = false;
                button2.Enabled = false;
                button3.Enabled = false;
                button4.Enabled = true;
                button5.Enabled = true;
                button6.Enabled = true;

                пригласитьToolStripMenuItem.Enabled = false;
                отклонитьToolStripMenuItem.Enabled = false;
                начатьПриемToolStripMenuItem.Enabled = false;
                перенаправитьToolStripMenuItem.Enabled = true;
                отложитьToolStripMenuItem.Enabled = true;
                завершитьToolStripMenuItem.Enabled = true;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            JObject resp = cnnToServer("Получить перечень услуг");

            if (resp != null)
            {
                if (resp["result"].Count() > 1)
                {
                    for (int i = 0; i < resp["result"]["root"]["inner_services"].Count(); i++)
                    {
                        if (!serviceWithId.ContainsKey(resp["result"]["root"]["inner_services"][i]["id"].ToString()))
                        {
                            serviceWithId.Add(resp["result"]["root"]["inner_services"][i]["id"].ToString(),
                                resp["result"]["root"]["inner_services"][i]["name"].ToString().Replace('+', ' '));
                        }
                    }
                }
                Form2 fm2 = new Form2(serviceWithId);
                fm2.StartPosition = FormStartPosition.CenterParent;
                fm2.ShowDialog();
                if (fm2.DialogResult == DialogResult.OK)
                {
                    //cnnToServer("Переадресовать клиента к другой услуге");
                    customerComments[customerInfo.Id] += ("\n" + Environment.UserName.ToString() + ":" +
                            (Args.returned ? (" Вернуть  после обслуживания" + " \n" + Args.comments) : ("" + "\n" + Args.comments)));
                    addTreeView(cnnToServer("Переадресовать клиента к другой услуге"));
                    JObject response = cnnToServer("Получить состояние очередей");
                    addTreeView(response);
                    if (getAmountCustomers(response) > 0)
                    {
                        button1.Enabled = true;
                        пригласитьToolStripMenuItem.Enabled = true;
                    }
                    button4.Enabled = false;
                    button5.Enabled = false;
                    button6.Enabled = false;

                    перенаправитьToolStripMenuItem.Enabled = false;
                    отложитьToolStripMenuItem.Enabled = false;
                    завершитьToolStripMenuItem.Enabled = false;
                    notCalled();
                    comments1.Text = ""; //Args.serviceId + "\n" + Args.returned + "\n" + Args.comments + "\n";

                }
                comments1.Clear();
            }
        }

        private void timerTick(string itemName, System.Windows.Forms.Timer timer)
        {
            listBox1.Items.Remove(itemName);
            cnnToServer("Получить состояние очередей");
            timer.Stop();
        }

        private void addItemWithTimer(int interval, string itemName)
        {
            listBox1.Items.Add(itemName);

            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = interval;
            timer.Tick += (sender, e) =>
            {
                string[] words = itemName.Split(' ');
                customerWithId.Remove(words[0]);
                listBox1.Items.Remove(itemName);
                timer.Stop();
            };

            //cnnToServer("Клиента в пул отложенных");
            timer.Start();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //JObject resp = cnnToServer("Получить получение списка возможных результатов");
            Form3 fm3 = new Form3();
            fm3.StartPosition = FormStartPosition.CenterParent;
            fm3.ShowDialog();
            if (fm3.DialogResult == DialogResult.OK)
            {
                string itemText = customerInfo.Num + ' ' + timerArgs.result + ' ' + timerArgs.timerValue + " min." + (timerArgs.onlyMyClient ? " Private!" : "");
                if (!customerWithId.ContainsKey(customerInfo.Num))
                {
                    customerWithId.Add(customerInfo.Num, customerInfo.Id);
                }

                if (timerArgs.timerValue != "Бессрочно")
                {
                    cnnToServer("Клиента в пул отложенных");
                    addItemWithTimer(Convert.ToInt32(timerArgs.timerValue) * 60000 + 30000, itemText); // ИСПРАВИТЬ НА 60000
                    cnnToServer("Получить состояние очередей");
                }
                else
                {
                    listBox1.Items.Add(itemText);
                    Console.WriteLine(cnnToServer("Клиента в пул отложенных"));
                    //cnnToServer("Клиента в пул отложенных");
                }

                button4.Enabled = false;
                button5.Enabled = false;
                button6.Enabled = false;

                перенаправитьToolStripMenuItem.Enabled = false;
                отложитьToolStripMenuItem.Enabled = false;
                завершитьToolStripMenuItem.Enabled = false;
                notCalled();
                comments1.Clear();
                Console.WriteLine($"{customerInfo.Id} --- {customerInfo.Num} --- {timerArgs.result}");
                Console.WriteLine(itemText);
                //listBox1.Items.Add(itemText);

            }

        }

        private void button6_Click(object sender, EventArgs e)
        {
            JObject resp = cnnToServer("Закончить работу с клиентом");

            if (resp != null)
            {
                if (resp["result"]["customer"]["state"].ToString() == "STATE_FINISH")
                {
                    customerComments.Remove(customerInfo.Id);
                }

                resp = cnnToServer("Получить состояние очередей");
                addTreeView(resp);
                if (getAmountCustomers(resp) > 0)
                {
                    button1.Enabled = true;
                    пригласитьToolStripMenuItem.Enabled = true;
                }
                button4.Enabled = false;
                button5.Enabled = false;
                button6.Enabled = false;

                перенаправитьToolStripMenuItem.Enabled = false;
                отложитьToolStripMenuItem.Enabled = false;
                завершитьToolStripMenuItem.Enabled = false;
                notCalled();
                comments1.Clear();
                Console.WriteLine(resp.ToString());
            }
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void splitContainer2_SplitterMoved(object sender, SplitterEventArgs e)
        {

        }

        private void пригласитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            button1_Click(sender, e);
        }

        private void отклонитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            button2_Click(sender, e);
        }

        private void начатьПриемToolStripMenuItem_Click(object sender, EventArgs e)
        {
            button3_Click(sender, e);
        }

        private void перенаправитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            button4_Click(sender, e);
        }

        private void отложитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            button5_Click(sender, e);
        }

        private void завершитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            button6_Click(sender, e);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }

        private void listBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                listBox1.SelectedIndex = listBox1.IndexFromPoint(e.Location);
                if (listBox1.SelectedIndex != -1)
                {
                    contextMenuStripForDeferred.Show(MousePosition, ToolStripDropDownDirection.Right);
                }
            }
        }

        private void inviteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string[] words = listBox1.Items[listBox1.SelectedIndex].ToString().Split(' ');
            customerId = customerWithId[words[0]];

            customerWithId.Remove(words[0]);
            listBox1.Items.Remove(listBox1.SelectedIndex);
            Console.WriteLine(customerId);
            //cnnToServer("Вызвать отложенного из пула отложенных");
            JObject resp = cnnToServer("Вызвать отложенного из пула отложенных");

            if (resp != null)
            {
                customerInfo.Id = resp["result"]["id"].ToString();
                customerInfo.Num = resp["result"]["prefix"].ToString() + resp["result"]["number"].ToString();
                string num = "";

                if (resp["result"]["temp_comments"] != null)
                {
                    if (resp["result"]["temp_comments"].ToString() != "")
                    {
                        Console.WriteLine(resp["result"]["temp_comments"].ToString());
                        comments1.Text = Environment.UserName.ToString() + ":\n" + resp["result"]["temp_comments"].ToString().Replace('+', ' ');
                    }
                }

                button2.Enabled = true;
                button3.Enabled = true;

                отклонитьToolStripMenuItem.Enabled = true;
                начатьПриемToolStripMenuItem.Enabled = true;

                infoTable.Clear();
                num = '\n' + resp["result"]["prefix"].ToString() + resp["result"]["number"].ToString() + '\n';
                infoTable.AppendText(num);
                infoTable.SelectionStart = 0;
                int textSize = infoTable.TextLength - 1;
                infoTable.SelectionLength = textSize;
                infoTable.SelectionColor = Color.Red;
                infoTable.SelectionAlignment = HorizontalAlignment.Center;
                infoTable.SelectionFont = new Font("Microsoft Sans Serif", 30);
                infoTable.AppendText("Услуга: " + resp["result"]["to_service"]["name"].ToString().Replace('+', ' '));
                infoTable.SelectionStart = textSize;
                infoTable.SelectionLength = infoTable.TextLength;
                infoTable.SelectionAlignment = HorizontalAlignment.Left;
                infoTable.SelectionFont = new Font("Microsoft Sans Serif", 14);

                listBox1.Items.RemoveAt(listBox1.SelectedIndex);
            }
        }

        public static class form4Args
        {
            public static string result;
        }

        private void changeStatusToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string[] words = listBox1.Items[listBox1.SelectedIndex].ToString().Split(' ');
            customerId = customerWithId[words[0]];

            Form4 fm4 = new Form4();
            fm4.StartPosition = FormStartPosition.CenterParent;
            fm4.ShowDialog();

            if (fm4.DialogResult == DialogResult.OK)
            {
                if (cnnToServer("Сменить статус отложенному") != null)
                {
                    listBox1.Items[listBox1.SelectedIndex] = listBox1.Items[listBox1.SelectedIndex].ToString().Replace(words[1] + " " + words[2], form4Args.result);
                }
            }
        }

        private void обновитьСитуациюToolStripMenuItem_Click(object sender, EventArgs e)
        {
            getLastState(cnnToServer("Получить состояние очередей"));
            //addTreeView(cnnToServer("Получить состояние очередей"));
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            JObject resp = cnnToServer("Перерыв оператора");
            if (resp != null)
            {
                if (checkBox1.Checked)
                {
                    timer1.Stop();
                    обновитьСитуациюToolStripMenuItem.Enabled = false;
                    Console.WriteLine(resp);
                }
                else
                {
                    timer1.Start();
                    обновитьСитуациюToolStripMenuItem.Enabled = true;
                    обновитьСитуациюToolStripMenuItem_Click(sender, e);
                }
            }
        }
    }
}