using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace CryptoProServer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            TcpListener server = null;
            try
            {
                int MaxThreadsCount = Environment.ProcessorCount * 4;
                richTextBox1.ReadOnly = true;
                richTextBox1.Text = "Max Threads: " + MaxThreadsCount.ToString();
                ThreadPool.SetMaxThreads(MaxThreadsCount, MaxThreadsCount);
                ThreadPool.SetMinThreads(2, 2);

                Int32 port = 9595;
                IPAddress local_addr = IPAddress.Parse("127.0.0.1");
                int counter = 0;
                server = new TcpListener(local_addr, port);
                server.Start();

                while (true)
                {
                    richTextBox1.Text += "Waiting for connection...";
                    ThreadPool.QueueUserWorkItem(processing_response, server.AcceptTcpClient());
                    counter++;
                    richTextBox1.Text +="\nConnection #" + counter.ToString() + "!";
                }
            }
            catch (SocketException exception)
            {
                MessageBox.Show("SocketException: " + exception);
            }
            finally
            {
                server.Stop();
            }
        }

        public void processing_response(object client_obj)
        {
            Byte[] bytes = new Byte[256];
            String data = null;

            TcpClient client = client_obj as TcpClient;

            data = null;

            NetworkStream stream = client.GetStream();
            
            int i;

            while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
            {
                // Преобразуем данные в ASCII string.
                data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);

                // Преобразуем строку к верхнему регистру.
                data = data.ToUpper();

                // Преобразуем полученную строку в массив Байт.
                byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);

                // Отправляем данные обратно клиенту (ответ).
                stream.Write(msg, 0, msg.Length);

            }

            client.Close();
        }
    }
}
