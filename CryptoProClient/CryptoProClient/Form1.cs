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
        const string container_name = "le-4cd9f341-5657-4431-99f7-c7c4f33de108";
        byte[] mode;
        X509Certificate2 cert;
        Gost3410CryptoServiceProvider csp;
        Gost3410 sign;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            mode = new byte[2];
            mode[0] = 0x01;
            mode[1] = 0x02;

            cert = get_certificate_by_name("flex2424");
            CspParameters csp_params = new CspParameters(75, null, container_name);
            csp = new Gost3410CryptoServiceProvider(csp_params);
            sign = cert.PublicKey.Key as Gost3410;

            foreach (StoreLocation storeLocation in (StoreLocation[])
                Enum.GetValues(typeof(StoreLocation)))
                if (storeLocation.ToString() == "CurrentUser")
                    foreach (StoreName storeName in (StoreName[])
                        Enum.GetValues(typeof(StoreName)))
                    {
                        if (storeName.ToString() == "My")
                        {
                            // Выводим все сертификаты в /Текущий пользователь/Личное
                            X509Store store = new X509Store(storeName, storeLocation);

                            try
                            {
                                store.Open(OpenFlags.OpenExistingOnly);
                                foreach (X509Certificate2 certificate in store.Certificates)
                                {
                                    comboBox1.Items.Add(certificate.GetName());
                                }
                            }
                            catch (CryptographicException)
                            {
                                MessageBox.Show("Exception in comboBox");
                            }
                        }
                    }

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
            Gost3410Parameters public_key = csp.ExportParameters(false);
            GostSharedSecretAlgorithm agree_key = csp.CreateAgree(sign.ExportParameters(false));
            byte[] wrapped_key = agree_key.Wrap(gost, GostKeyWrapMethod.CryptoProKeyWrap);
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, public_key);
            byte[] public_key_bytes = ms.ToArray();
            ms.Close();

            MemoryStream memory_stream = new MemoryStream();
            CryptoStream cs = new CryptoStream(memory_stream, gost.CreateEncryptor(), CryptoStreamMode.Write);
            string plain_text = richTextBox1.Text;
            byte[] plain_text_bytes = Encoding.ASCII.GetBytes(plain_text);

            cs.Write(plain_text_bytes, 0, plain_text_bytes.Length);
            cs.FlushFinalBlock();

            byte[] cipher_text_bytes = memory_stream.ToArray();
            memory_stream.Close();
            cs.Close();
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            string plain_text = richTextBox1.Text;
            byte[] plain_text_bytes = Encoding.ASCII.GetBytes(plain_text);

            Gost3410CryptoServiceProvider gost = new Gost3410CryptoServiceProvider();
            Gost3411CryptoServiceProvider hash = new Gost3411CryptoServiceProvider();

            byte[] signature = gost.SignData(plain_text_bytes, hash);

            send_data("signature", signature);

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

        private void send_data(string flag, byte[] data)
        {
            try
            {
                Int32 port = 9595;
                TcpClient client = new TcpClient("127.0.0.1", port);
                NetworkStream stream = client.GetStream();
                if (flag == "encryption")
                {
                    stream.Write(mode, 0, 1);
                    MessageBox.Show("ecnrypt");
                }
                else
                {
                    stream.Write(mode, 1, 1);
                    MessageBox.Show("sign");
                }
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
        
    }
}
