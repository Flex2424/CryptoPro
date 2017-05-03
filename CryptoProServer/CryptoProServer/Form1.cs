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
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using CryptoPro.Sharpei;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using Kzar.ASN1.BER;

namespace CryptoProServer
{
    public partial class Form1 : Form
    {
        const string container_name = "le-4cd9f341-5657-4431-99f7-c7c4f33de108";
        X509Certificate2 cert;
        Gost3410CryptoServiceProvider csp;
        Gost3410 sign;
        /*
        byte[] wrapped_key = new byte[65];
        byte[] iv = new byte[8];
        byte[] public_key = new byte[355];
        byte[] signed = new byte[64];
        */
        byte[] buffer = new byte[1024];

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CspParameters csp_params = new CspParameters(75, null, container_name);
            csp = new Gost3410CryptoServiceProvider(csp_params);

            Thread main_thread = new Thread(check_response);
            main_thread.Start();
        }

        private void check_response()
        {
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
                    NetworkStream ns = client.GetStream();
                    Byte[] mode_bytes = new Byte[1];
                    int mode = ns.Read(mode_bytes, 0, mode_bytes.Length);

                    if (mode_bytes[0] == 0x01)
                    {
                        richTextBox1.Text += "Received encrypted msg. Try to decrypt: \n";
                        int bytes_read = ns.Read(buffer, 0, buffer.Length);
                        byte[] message = new byte[bytes_read];
                        Array.Copy(buffer, message, bytes_read);
                        BERelement mSeq = BERelement.DecodePacket(message);
                        BERelement sSeq = null;

                        sSeq = mSeq.Items[0];
                        var cert_name = sSeq.Items[0].Value;
                        var wrapped_key2 = sSeq.Items[1].Value;
                        var iv2 = sSeq.Items[2].Value;
                        var public_key_bytes = sSeq.Items[3].Value;
                        var cipher_text_bytes = sSeq.Items[4].Value;

                        MemoryStream ms = new MemoryStream(public_key_bytes);

                        BinaryFormatter bf = new BinaryFormatter();
                        Gost3410Parameters key_params = (Gost3410Parameters)bf.Deserialize(ms);
                        GostSharedSecretAlgorithm agree_key = csp.CreateAgree(key_params);
                        SymmetricAlgorithm gost = agree_key.Unwrap(wrapped_key2, GostKeyWrapMethod.CryptoProKeyWrap);
                        gost.IV = iv2;
                        MemoryStream memoryStream = new MemoryStream();
                        CryptoStream cryptoStream = new CryptoStream(memoryStream,
                        gost.CreateDecryptor(), CryptoStreamMode.Write);
                        int bytesRead = cipher_text_bytes.Length;
                        cryptoStream.Write(cipher_text_bytes, 0, bytesRead);
                        cryptoStream.FlushFinalBlock();
                        byte[] plain_text_bytes = memoryStream.ToArray();
                        richTextBox1.Text += Encoding.ASCII.GetString(plain_text_bytes, 0, plain_text_bytes.Length) + "\n";

                    }
                    if (mode_bytes[0] == 0x02)
                    {
                        richTextBox1.Text += "Received sign. Try to check it: \n";
                        Gost3411CryptoServiceProvider hash = new Gost3411CryptoServiceProvider();

                        int bytes_read = ns.Read(buffer, 0, buffer.Length);
                        byte[] message = new byte[bytes_read];
                        Array.Copy(buffer, message, bytes_read);

                        BERelement mSeq = BERelement.DecodePacket(message);
                        BERelement sSeq = null;

                        sSeq = mSeq.Items[0];
                        var cert_name = sSeq.Items[0].Value;
                        var signed2 = sSeq.Items[1].Value;
                        var msg = sSeq.Items[2].Value;

                        int len = signed2.Length;                        

                        bool test = csp.VerifyData(msg, hash, signed2);
                        if (test) richTextBox1.Text += "Подпись корректна.\r\n";
                        else richTextBox1.Text += "Подпись некорректна.\r\n";
                         
                        /*
                        bool test = csp.VerifyData(message, hash, signed);
                        if (test) richTextBox1.Text += "Подпись корректна.\r\n";
                        else richTextBox1.Text += "Подпись некорректна.\r\n";
                         
                        */
                    }
                    if (mode_bytes[0] == 0x03)
                    {
                        richTextBox1.Text += "Received sign and encryption. Making magic: \n";

                        int bytes_read = ns.Read(buffer, 0, buffer.Length);
                        byte[] message = new byte[bytes_read];
                        Array.Copy(buffer, message, bytes_read);
                        BERelement mSeq = BERelement.DecodePacket(message);
                        BERelement sSeq = null;

                        sSeq = mSeq.Items[0];
                        var cert_name = sSeq.Items[0].Value;
                        var wrapped_key2 = sSeq.Items[1].Value;
                        var iv2 = sSeq.Items[2].Value;
                        var public_key_bytes = sSeq.Items[3].Value;
                        var cipher_text_bytes = sSeq.Items[4].Value;

                        MemoryStream ms = new MemoryStream(public_key_bytes);

                        BinaryFormatter bf = new BinaryFormatter();
                        Gost3410Parameters key_params = (Gost3410Parameters)bf.Deserialize(ms);
                        GostSharedSecretAlgorithm agree_key = csp.CreateAgree(key_params);
                        SymmetricAlgorithm gost = agree_key.Unwrap(wrapped_key2, GostKeyWrapMethod.CryptoProKeyWrap);
                        gost.IV = iv2;
                        MemoryStream memoryStream = new MemoryStream();
                        CryptoStream cryptoStream = new CryptoStream(memoryStream,
                        gost.CreateDecryptor(), CryptoStreamMode.Write);
                        int bytesRead = cipher_text_bytes.Length;
                        cryptoStream.Write(cipher_text_bytes, 0, bytesRead);
                        cryptoStream.FlushFinalBlock();
                        byte[] plain_text_bytes = memoryStream.ToArray();

                        //check signature
                        Gost3411CryptoServiceProvider hash = new Gost3411CryptoServiceProvider();

                        
                        //Array.Copy(buffer, message, bytes_read);

                        BERelement mSeq2 = BERelement.DecodePacket(plain_text_bytes);
                        BERelement sSeq2 = null;

                        sSeq2 = mSeq2.Items[0];
                        var cert_name2 = sSeq2.Items[0].Value;
                        var signed3 = sSeq2.Items[1].Value;
                        var msg2 = sSeq2.Items[2].Value;

                        int len2 = signed3.Length;

                        bool test = csp.VerifyData(msg2, hash, signed3);
                        richTextBox1.Text += "Полученное сообщение: "
                            + Encoding.ASCII.GetString(msg2, 0, msg2.Length) + "\r\n";
                        if (test)
                        {
                            richTextBox1.Text += "Подпись корректна.\r\n";
                        }
                        else
                        {
                            richTextBox1.Text += "Подпись некорректна.\r\n";
                        }

                    }

                    int i;
                    while ((i = ns.Read(bytes, 0, bytes.Length)) != 0)
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

        private void decrypt_msg()
        {

        }

        private void chech_signature()
        {

        }

    }
}
