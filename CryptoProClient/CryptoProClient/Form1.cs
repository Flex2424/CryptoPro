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

using CryptoPro.Sharpei;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace CryptoProClient
{
    public partial class Form1 : Form
    {
        string container_name = "le-4cd9f341-5657-4431-99f7-c7c4f33de108";
        Gost3410 signature;
        byte[] command;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
         
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                Int32 port = 9595;
                TcpClient client = new TcpClient("127.0.0.1", port);
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(richTextBox1.Text);
                NetworkStream stream = client.GetStream();

                stream.Write(data, 0, data.Length);

                data = new Byte[256];
                String responseData = String.Empty;

                Int32 bytes = stream.Read(data, 0, data.Length);
                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                MessageBox.Show("Received: " + responseData);

                stream.Close();
                client.Close();

            }
            catch (ArgumentNullException exception)
            {
                MessageBox.Show("ArgumentNullException: " + exception);
            }
            catch (SocketException exception)
            {
                MessageBox.Show("SocketException: " + exception);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Gost28147 gost = Gost28147.Create();
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, gost.CreateEncryptor(),  CryptoStreamMode.Write);
            
            string plain_text = richTextBox1.Text;
            byte[] plain_text_bytes = Encoding.ASCII.GetBytes(plain_text);

            cs.Write(plain_text_bytes, 0, plain_text_bytes.Length);
            cs.FlushFinalBlock();

            byte[] cipher_text_bytes = ms.ToArray();
            
            cs.Close();
            ms.Close();

        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            X509Certificate2 cert = get_certificate_by_name("flex2424");

        }

        private X509Certificate2 get_certificate_by_name(string name)
        {
           X509Store store = new X509Store(StoreLocation.CurrentUser);
           store.Open(OpenFlags.OpenExistingOnly | OpenFlags.ReadOnly);
           X509Certificate2Collection found =
               store.Certificates.Find(X509FindType.FindBySubjectName, name, false);

           // Проверяем, что нашли ровно один сертификат.
           if (found.Count == 0)
           {
               MessageBox.Show("Сертификат не найден.");
               return null;
           }
           if (found.Count > 1)
           {
               MessageBox.Show("Найдено более одного сертификата.");
               return null;
           }
           X509Certificate2 cert = found[0];
           return cert;
        }
    }
}
