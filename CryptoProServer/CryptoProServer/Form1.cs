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
            Thread main_thread = new Thread(check_response);
            main_thread.Start();
        }

        private void check_response()
        {
            NetworkStream ns;
            TcpClient client = null;
            try
            {
                TcpListener listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 9595);
                listener.Start();

                Byte[] bytes = new Byte[256];
                String data = null;

                while (true)
                {
                    client = listener.AcceptTcpClient();
                    NetworkStream stream = client.GetStream();

                    int i;
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                        richTextBox1.Text = DateTime.Now.ToString() + "\n";
                        richTextBox1.Text += data;
                    }
                    client.Close();
                }                
            }
            catch (SocketException exception)
            {
                MessageBox.Show("SocketException: " + exception);
            }
        }

    }
}
