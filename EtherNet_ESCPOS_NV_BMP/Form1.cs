using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Serial_ESCPOS
{
    public partial class Form1 : Form
    {
        Socket c = null;
        String str_ip = null;
        int port = 9100;

        Thread readThread;
        static bool _continue;

        //Image img = null;
        Bitmap bmp = null;
        String bmp_filename = null;
                
        delegate void SetTextCallback(string text);

        public Form1()
        {
            InitializeComponent();
        }

        private void statusStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void buttonOpenPort_Click(object sender, EventArgs e)
        {
            // IP地址检查
            if ((numericUpDown1.Value != 10)
                && (numericUpDown1.Value != 172) && (numericUpDown1.Value != 192))
            {
                MessageBox.Show("私有IP地址允许的网段范围是:\n"
                                + "10.0.0.0--10.255.255.255\n"
                                + "172.16.0.0--172.31.255.255\n"
                                + "192.168.0.0--192.168.255.255\n");
            }
            else if ((numericUpDown1.Value == 172)
                 && ((numericUpDown2.Value < 16) || (numericUpDown2.Value > 31)))
            {
                MessageBox.Show("私有IP地址允许的网段范围是:\n"
                                + "172.16.0.0--172.31.255.255\n");
            }
            else if ((numericUpDown1.Value == 192) && (numericUpDown2.Value != 168))
            {
                MessageBox.Show("私有IP地址允许的网段范围是:\n"
                                + "192.168.0.0--192.168.255.255\n");
            }
            else
            {
                // Connect this IP address on TCP Port9100
                str_ip = numericUpDown1.Value + "." + numericUpDown2.Value + "."
                       + numericUpDown3.Value + "." + numericUpDown4.Value;
                IPAddress ip = IPAddress.Parse(str_ip);

                try
                {
                    //把ip和端口转化为IPEndPoint实例
                    IPEndPoint ip_endpoint = new IPEndPoint(ip, port);

                    //创建一个Socket
                    c = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);


                    //连接到服务器
                    c.Connect(ip_endpoint);
                    //应对同步Connect超时过长的办法，猜测应该是先用异步方式建立以个连接然后，
                    //确认连接是否可用，然后报错或者关闭后，重新建立一个同步连接                    

                    c.SendTimeout = 1000;

                    //初始化打印机，并打印
                    ipWrite("\x1b\x40打开TCP/IP连接!\n-------------------------------\n\n");

                    //操作按钮启用
                    buttonClosePort.Enabled = true;
                    buttonOpenPort.Enabled = false;
                    button1.Enabled = true;
                    button5.Enabled = true;
                    button6.Enabled = true;
                    button7.Enabled = true;
                    button8.Enabled = true;
                    button9.Enabled = true;
                    button10.Enabled = true;
                    button11.Enabled = true;

                    //读线程
                    _continue = true;
                    readThread = new Thread(Read);

                    //读线程启动
                    readThread.Start();
                }
                catch (ArgumentNullException e1)
                {
                    //MessageBox.Show(String.Format("参数意外:{0}", e1));
                    MessageBox.Show("Socket参数设置错误!");
                }
                catch (SocketException e2)
                {
                    //MessageBox.Show(String.Format("Socket连接意外:{0}", e2));
                    MessageBox.Show("连接不到指定IP的打印机!");
                }
            }
        }

        private void ipWrite(String str_send)
        {
            //String str_send = "爱普生（中国）有限公司!\n";
            Byte[] byte_send = Encoding.GetEncoding("gb18030").GetBytes(str_send);
            ipWrite(byte_send, 0, byte_send.Length);
        }

        private void ipWrite(Byte[] byte_send, int start, int length)
        {
            try
            {
                //发送测试信息
                c.Send(byte_send, length, 0);
            }
            catch (SocketException e2)
            {
                MessageBox.Show(String.Format("Socket连接意外:{0}", e2));
            }
        }

        private void buttonClosePort_Click(object sender, EventArgs e)
        {
            ipClose();
        }

        private void ipClose()
        {
            // 结束演示，关闭IP连接
            ipWrite("\n-------------------------------\n关闭TCP/IP连接!\n");

            _continue = false;
            readThread.Abort();
            c.Close();

            buttonClosePort.Enabled = false;
            buttonOpenPort.Enabled = true;
            button1.Enabled = false;
            button5.Enabled = false;
            button6.Enabled = false;
            button7.Enabled = false;
            button8.Enabled = false;
            button9.Enabled = false;
            button10.Enabled = false;
            button11.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            try
            {
                //openFileDialog1.InitialDirectory = "c:\\";
                openFileDialog1.Filter = "bmp files (*.bmp)|*.bmp|All files (*.*)|*.*";
                openFileDialog1.FilterIndex = 2;
                openFileDialog1.RestoreDirectory = true;

                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    //img = Image.FromFile(openFileDialog1.FileName);
                    bmp_filename = openFileDialog1.FileName;
                    bmp = new Bitmap(openFileDialog1.FileName, true);
                    pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                    pictureBox1.Image = bmp;
                }

                button9.Enabled = true;
                button10.Enabled = true;
                button11.Enabled = true;

            }
            catch (Exception ex)
            {
                MessageBox.Show("打开BMP位图文件失败：" + ex.Message);
            }


        }

        private void button5_Click(object sender, EventArgs e)
        {
            ipWrite("\n2.1 <GS ( L>查询NV存储的容量!\n\n");

            // GS (/8 L, Transmit the NV graphics memory capacity.
            // <Function 48>
            byte[] gsNV = new byte[] { 0x1D, 0x28, 0x4C, 0x02, 0x00, 0x30, 0x00 };
            ipWrite(gsNV, 0, gsNV.Length);

            textBox1.Text = "查询NV存储的容量!\r\nTM-T82为 256K= 256 x 1024= 262144\r\n";            
        }

        private void button6_Click(object sender, EventArgs e)
        {
            ipWrite("\n2.2 <GS ( L> 查询NV存储的剩余容量!\n\n");

            // GS (/8 L, Transmit the remaining capacity of the NV graphics memory
            // <Function 48>
            byte[] gsNV = new byte[] { 0x1D, 0x28, 0x4C, 0x02, 0x00, 0x30, 0x03 };
            ipWrite(gsNV, 0, gsNV.Length);

            textBox1.Text = "查询NV存储的剩余容量!\r\n";
        }

        private void button7_Click(object sender, EventArgs e)
        {
            ipWrite("\n2.3 <GS ( L> 读取NV存储中的所有索引!\n\n");

            // GS (/8 L, Transmit the key code list for defined NV graphics
            // <Function 64>
            byte[] gsNV = new byte[] { 0x1D, 0x28, 0x4C, 0x04, 0x00, 0x30, 0x40, 0x4B, 0x43 };
            ipWrite(gsNV, 0, gsNV.Length);

            textBox1.Text = "读取NV存储中的所有索引!\r\n";            
        }

        private void button8_Click(object sender, EventArgs e)
        {
            ipWrite("\n2.4 <GS ( L> 删除NV存储中的所有索引对应BMP!\n\n");
            // GS (/8 L, Delete all NV graphics data
            // <Function 65>
            byte[] gsNV = new byte[] { 0x1D, 0x28, 0x4C, 0x05, 0x00, 0x30, 0x41, 0x43, 0x4C, 0x52 };
            ipWrite(gsNV, 0, gsNV.Length);

            textBox1.Text = "删除NV存储中的所有索引对应BMP!\r\n";
        }

        private void button9_Click(object sender, EventArgs e)
        {
            ipWrite("\n\n1.1 <GS D>将选择的Windows BMP文件与指定索引绑定，并存储到NV中!\n");
            ipWrite("\n\n索引范围：\n  32<= key1 <=126\n  32<= key1 <=126\n\n");
            textBox1.Text = "<GS D>将Windows BMP文件与指定索引绑定，并存储到NV中!\r\n";

            if (!File.Exists(bmp_filename))
            {
                textBox1.Text = "指定的Window BMP文件不存在:\n" + bmp_filename;
                return;
            }
            else
            {
                ipWrite("\n选择Windows BMP文件为:" + bmp_filename + "\n");

                FileInfo fi = new FileInfo(bmp_filename);
                FileStream fs = fi.OpenRead();

                // GS D <Function 67>; Define Windows BMP NV file
                // Key code = (0x20, 0x20)
                byte[] gsBmp = new byte[] { 0x1D, 0x44, 0x30, 0x43, 0x30, 0x20, 0x20, 0x30, 0x31 };

                // BMP的索引
                String[] bmpKey = comboBox4.Text.Split(new Char[] { ',' });
                gsBmp[5] = byte.Parse(bmpKey[0]);
                gsBmp[6] = byte.Parse(bmpKey[1]); 

                ipWrite(gsBmp, 0, gsBmp.Length);

                byte[] data = new byte[1];

                while (fs.Read(data, 0, 1) != 0)
                {
                    ipWrite(data, 0, 1);
                }

                ipWrite("\n\n完成BMP存于NV，并与索引<" + comboBox4.Text + ">绑定!\n\n");
                fs.Close();
                fs.Dispose();
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            textBox1.Text = "打印索引<" + comboBox4.Text + ">对应的BMP!\r\n";
            ipWrite("\n1.2 <GS ( L>打印索引<" + comboBox4.Text + ">对应的BMP!\n\n");
            
            // GS (/8 L, Print the specified NV graphics data
            byte[] gsBmp = new byte[] { 0x1D, 0x28, 0x4C, 0x06, 0x00, 
                                        0x30, 0x45, 0x00, 0x00, 0x00, 0x00 };
            
            // 位图的索引
            String[] bmpKey = comboBox4.Text.Split(',');
            gsBmp[7] = byte.Parse(bmpKey[0]);
            gsBmp[8] = byte.Parse(bmpKey[1]);

            // NV位图的放大方式
            if (comboBox5.Text == "原始大小")
            {
                gsBmp[9] = (byte)'\x01';
                gsBmp[10] = (byte)'\x01';
                ipWrite("\na.原始大小!\n");
            }
            else if (comboBox5.Text == "倍宽")
            {
                gsBmp[9] = (byte)'\x02';
                gsBmp[10] = (byte)'\x01';
                ipWrite("\nb.倍宽!\n");
            }
            else if (comboBox5.Text == "倍高")
            {
                gsBmp[9] = (byte)'\x01';
                gsBmp[10] = (byte)'\x02';
                ipWrite("\nc.倍高!\n");
            }
            else if (comboBox5.Text == "四倍大小")
            {
                gsBmp[9] = (byte)'\x02';
                gsBmp[10] = (byte)'\x02';
                ipWrite("\nd.倍宽倍高，四倍大小!\n");
            }
            
            ipWrite(gsBmp, 0, gsBmp.Length);
        }

        private void button11_Click(object sender, EventArgs e)
        {
            // GS (/8 L, Delete the specified NV bitmap data
            byte[] gsBmp = new byte[] { 0x1D, 0x28, 0x4C, 0x04, 0x00, 0x30, 0x42, 0x20, 0x20 };

            // 位图的索引
            String[] bmpKey = comboBox4.Text.Split(',');
            gsBmp[7] = byte.Parse(bmpKey[0]);
            gsBmp[8] = byte.Parse(bmpKey[1]);

            ipWrite(gsBmp, 0, gsBmp.Length);

            textBox1.Text = "删除索引<" + comboBox4.Text + ">对应的BMP!\r\n";
            ipWrite("\n1.3 <GS ( L>删除索引<" + comboBox4.Text + ">对应的BMP!\n\n");
        }
                     
        private void SetText(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.textBox1.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.textBox1.Text += text;
            }
        }

        private void Read()
        {
            while (_continue)
            {
                Byte[] byte_recv = new Byte[64];
                int byte_num;

                //从打印机端接受返回信息
                byte_num = c.Receive(byte_recv, byte_recv.Length, 0);

                try
                {
                    if (byte_num > 0)
                    {
                        this.SetText("\n" + ByteArrayToHexString(byte_recv, byte_num) + "\n");
                    }
                }
                catch (TimeoutException) { }
            }
        }

        /// <summary> Converts an array of bytes into a formatted string of hex digits (ex: E4 CA B2)</summary>
        /// <param name="data"> The array of bytes to be translated into a string of hex digits. </param>
        /// <returns> Returns a well formatted string of hex digits with spacing. </returns>
        private static string ByteArrayToHexString(byte[] data, int length)
        {
            StringBuilder sb = new StringBuilder(length * 8);

            //PadLeft,PadRight分别是左对齐和右对齐字符串长度，不足部分用指定字符填充
            for (int i = 0; i < length; i++)
            {
                if (data[i] != 0)
                {
                    sb.Append("<" + Convert.ToChar(data[i]) + ">");
                    sb.Append(Convert.ToString(data[i], 16).PadLeft(2, '0').PadLeft(3, ':'));
                    sb.Append("\r\n");
                }
                else
                {
                    sb.Append("< >:00\r\n");
                }
            }
            //组成结果如此, "<A>:38 <0>:30"

            return sb.ToString().ToUpper();
        }
   }
}
