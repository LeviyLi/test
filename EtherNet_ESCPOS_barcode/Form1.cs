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
//using System.Threading; 不含读操作

namespace Serial_ESCPOS
{
    public partial class Form1 : Form
    {
        Socket c = null;
        String str_ip = null;
        int port = 9100;

        
        public Form1()
        {
            InitializeComponent();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void statusStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

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
                    button2.Enabled = true;
                    button3.Enabled = true;
                    button4.Enabled = true;
                    button5.Enabled = true;
                    button6.Enabled = true;
                    button7.Enabled = true;
                    button8.Enabled = true;                                      
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
            // 结束演示，关闭IP连接
            ipWrite("\n-------------------------------\n关闭TCP/IP连接!\n");
            c.Close();

            buttonClosePort.Enabled = false;
            buttonOpenPort.Enabled = true;
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Feed and cut paper, GS V
            byte[] cutPaper = new byte[] { 0x1D, 0x56, 0x42, 0x00 };

            //------------------------------------------------------
            ipWrite("测试一维条码，<A>型命令格式!\n\n");
            
            // 1.UPC-A, 选择HRI字符显示位置, Default
            ipWrite("\nUPC-A! 不显示 HRI 字符\n\n");
            byte[] printBarcode = new byte[] { 0x1d, 0x6b, 0x00 };
            ipWrite(printBarcode, 0, printBarcode.Length);

            String contentBarcode = "098765432198\x0\xA";   //data = 12
            ipWrite(contentBarcode);
           
            // 2.UPC-E, HRI显示于条码上部，GS H n=1
            ipWrite("\nUPC-E! HRI 显示在条码上方\n\n");
            byte[] setHRI = new byte[] { 0x1D, 0x48, 0x01 };
            ipWrite(setHRI, 0, setHRI.Length);

            printBarcode[2] = (byte)'\x01';
            ipWrite(printBarcode, 0, printBarcode.Length);
            
            ipWrite(contentBarcode);  // UPC-E, data must be start from '0'

            // 3.JAN-13/EAN-13, HRI显示于条码下部，GS H n=2
            ipWrite("\nJAN-13/EAN-13! HRI 显示于条码下方\n\n");
            setHRI[2] = (byte)'\x02';
            ipWrite(setHRI, 0, setHRI.Length);

            printBarcode[2] = (byte)'\x02';
            ipWrite(printBarcode, 0, printBarcode.Length);

            ipWrite(contentBarcode);  // JAN/EAN-13, 不满13，自动加1位.

            // 4.JAN-8/EAN-8, HRI显示于条码上部和下部，GS H n=3
            ipWrite("\nJAN-8/EAN-8! HRI 同时显示在条码的上方和下方\n\n");
            setHRI[2] = (byte)'\x03';
            ipWrite(setHRI, 0, setHRI.Length);

            printBarcode[2] = (byte)'\x03';
            ipWrite(printBarcode, 0, printBarcode.Length);
            
            String contentBarcode2 = "87654321\x0\xA";   //data = 8
            ipWrite(contentBarcode2);
                        
            // 5.CODE39, HRI字体选择，FontA
            ipWrite("\nCODE39! HRI 字体设置为 FontA\n\n");
            byte[] setHRIfont = new byte[] { 0x1D, 0x66, 0x00 };
            ipWrite(setHRIfont, 0, setHRIfont.Length);

            printBarcode[2] = (byte)'\x04';
            ipWrite(printBarcode, 0, printBarcode.Length);

            String contentBarcode3 = "1A/. $%+-*\x0\xA";   // CODE38长度不限，适合算术内容
            ipWrite(contentBarcode3);

            // 6.ITF, HRI字体选择，FontB
            ipWrite("\nITF! HRI 字体设置为 FontB\n\n");
            setHRIfont[2] = (byte)'\x01';
            ipWrite(setHRIfont, 0, setHRIfont.Length);

            printBarcode[2] = (byte)'\x05';
            ipWrite(printBarcode, 0, printBarcode.Length);

            String contentBarcode4 = "1122334455667788\x0\xA";   //ITF, data must be even.
            ipWrite(contentBarcode4);

            // 7.CODABAR, 恢复各项默认,HRI不显示，HRI font is FontA
            ipWrite("\nCODEBAR! 默认:不显示 HRI, HRI 字体是 FontA\n\n");
            setHRI[2] = (byte)'\x00';
            ipWrite(setHRI, 0, setHRI.Length);

            setHRIfont[2] = (byte)'\x00';
            ipWrite(setHRIfont, 0, setHRIfont.Length);

            printBarcode[2] = (byte)'\x06';
            ipWrite(printBarcode, 0, printBarcode.Length);

            String contentBarcode5 = "A12$./:+-d\x0\xA";   //CODEBAR
            ipWrite(contentBarcode5);

            // Feed and cut paper
            ipWrite(cutPaper, 0, cutPaper.Length);
        }



        private void button2_Click(object sender, EventArgs e)
        {
            // Feed and cut paper, GS V
            //byte[] cutPaper = new byte[] { 0x1D, 0x56, 0x42, 0x00 };

            //------------------------------------------------------
            ipWrite("测试一维条码，<B>型命令格式!\n\n");
            byte[] setHRI = new byte[] { 0x1D, 0x48, 0x02 };
            ipWrite(setHRI, 0, setHRI.Length);

            // 1.UPC-A, 条码宽度，即条码的最小单位宽度, default n=3
            ipWrite("\nUPC-A! 默认的条码宽度\n\n");
            byte[] printBarcode = new byte[] { 0x1d, 0x6b, 0x41, 0x0C }; //n=12
            ipWrite(printBarcode, 0, printBarcode.Length);

            String contentBarcode = "098765432198\xA";   //data = 12
            ipWrite(contentBarcode);

            // 2.UPC-E, 条码宽度，即条码的最小单位宽度, GS w n=6
            ipWrite("\nUPC-E! 条码宽度（即线的单位宽度），最大值 n=6\n\n");
            byte[] setWidth = new byte[] { 0x1D, 0x77, 0x06 };
            ipWrite(setWidth, 0, setWidth.Length);

            printBarcode[2] = (byte)'\x42';
            ipWrite(printBarcode, 0, printBarcode.Length);

            ipWrite(contentBarcode);  //UPC-E, data must be start from '0'

            // 3.JAN-13/EAN-13, 条码宽度，即条码的最小单位宽度, GS w n=2
            ipWrite("\nJAN-13/EAN-13! 条码宽度（即线的单位宽度），最小值 n=2\n\n");
            setWidth[2] = (byte)'\x02';
            ipWrite(setWidth, 0, setWidth.Length);

            printBarcode[2] = (byte)'\x43';
            ipWrite(printBarcode, 0, printBarcode.Length);

            ipWrite(contentBarcode);  //JAN/EAN-13, 不满13，自动加1位.

            // 4.JAN-8/EAN-8, 条码宽度，恢复默认, GS w n=3
            ipWrite("\nJAN-8/EAN-8! 条码宽度（即线的单位宽度），默认值 n=3\n\n");
            setWidth[2] = (byte)'\x03';
            ipWrite(setWidth, 0, setWidth.Length);

            printBarcode[2] = (byte)'\x44';
            printBarcode[3] = (byte)'\x08';
            ipWrite(printBarcode, 0, printBarcode.Length);

            String contentBarcode2 = "87654321\x0\xA";   //data = 8
            ipWrite(contentBarcode2);

            // 5.CODE39, 条码高度，T81每毫米为8个点，GS h n=32
            ipWrite("\nCODE39! 高度设置为 4mm, n=32\n\n");
            byte[] setHeight = new byte[] { 0x1D, 0x68, 0x20 };
            ipWrite(setHeight, 0, setHeight.Length);

            printBarcode[2] = (byte)'\x45';
            printBarcode[3] = (byte)'\x0A';
            ipWrite(printBarcode, 0, printBarcode.Length);

            String contentBarcode3 = "1A/. $%+-*\xA";   //CODE38长度不限，适合算术内容
            ipWrite(contentBarcode3);

            // 6.ITF, 修改条码高度，GS h n=240
            ipWrite("\nITF! 高度设置为 3cm, n=240\n\n");
            setHeight[2] = (byte)'\xF0';
            ipWrite(setHeight, 0, setHeight.Length);

            printBarcode[2] = (byte)'\x46';
            printBarcode[3] = (byte)'\x10';
            ipWrite(printBarcode, 0, printBarcode.Length);

            String contentBarcode4 = "1122334455667788\xA";   //ITF, data must be even.
            ipWrite(contentBarcode4);

            // 7.CODEBAR, 修改条码高度，GS h n=162, default
            ipWrite("\nCODEBAR! 高度 2cm, (默认值)n=162\n\n");
            setHeight[2] = (byte)'\xA2';
            ipWrite(setHeight, 0, setHeight.Length);

            printBarcode[2] = (byte)'\x47';
            printBarcode[3] = (byte)'\x0A';
            ipWrite(printBarcode, 0, printBarcode.Length);

            String contentBarcode5 = "A12$./:+-d\xA";   //CODEBAR
            ipWrite(contentBarcode5);

            // 8.CODE93,
            ipWrite("\nCODE93!\n\n");
            printBarcode[2] = (byte)'\x48';
            printBarcode[3] = (byte)'\x0A';
            ipWrite(printBarcode, 0, printBarcode.Length);

            String contentBarcode6 = "aB!@#$%^&*\x0A";   //CODE93, n=1~127
            ipWrite(contentBarcode6);

            // 9.CODE128,
            ipWrite("\nCODE128! 表C: 1 字符值范围是 01~99 两位数字\n\n");
            printBarcode[2] = (byte)'\x49';
            printBarcode[3] = (byte)'\x0E';
            ipWrite(printBarcode, 0, printBarcode.Length);

            String contentBarcode7 = "{A12345{C\x63\x62\x61\x60\x59\x0A";   //CODE128, {A,{B,{C
            ipWrite(contentBarcode7);

            // 10.GS1-128
            ipWrite("\nGS1-128! 2<= n <=255\n");
            ipWrite("\nGS1-128 是替换EAN/JAN-13的下一代商品条码标准，需要用AI表示开头，通常:\n (01)是商品名称\n (10)是生产序列号管理\n\n");

            printBarcode[2] = (byte)'\x4A';
            printBarcode[3] = (byte)'\x14';
            ipWrite(printBarcode, 0, printBarcode.Length);

            String contentBarcode8 = "(01)1234567890123456\x0A";   //GS1-128, 1<= d <=127
            ipWrite(contentBarcode8);

            // 11.GS1 Databar Ominidirection, GS1 Databar Truncated, GS1 Databar Limited
            ipWrite("\nGS1 Databar Ominidirection\nGS1 Databar Truncated\nGS1 Databar Limited\n\n");
            
            printBarcode[2] = (byte)'\x4B';
            printBarcode[3] = (byte)'\x0D';
            ipWrite(printBarcode, 0, printBarcode.Length);

            String contentBarcode9 = "1234567890123\x0A";   //GS1 Databar
            ipWrite(contentBarcode9);

            // 12.GS1 Databar Expanded
            ipWrite("\nGS1 Databar Expanded\n\n");

            printBarcode[2] = (byte)'\x4E';
            printBarcode[3] = (byte)'\x10';
            ipWrite(printBarcode, 0, printBarcode.Length);

            String contentBarcode10 = "1234567890123456\x0A";   //GS1 Databar Expanded
            ipWrite(contentBarcode10);

            // Feed and cut paper
            //ipWrite(cutPaper, 0, cutPaper.Length); 
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Feed and cut paper, GS V
            //byte[] cutPaper = new byte[] { 0x1D, 0x56, 0x42, 0x00 };

            //------------------------------------------------------
            ipWrite("测试二维条码，PDF417!\n\n");

            // 1.PDF417 default setting printing
            ipWrite("\n1.PDF417, 默认设置!\n\n");

            // Store the data in symbol storage area, [function 080]
            byte[] storePDF417 = new byte[] { 0x1D, 0x28, 0x6B, 0xB7, 0x00, 0x30, 0x50, 0x30 };
            ipWrite(storePDF417, 0, storePDF417.Length);

            String contentBarcode = "123456789012345678901234567890"
                                    + "abcedfghijklmnopqrstuvwxyzabcd"
                                    + "123456789012345678901234567890"
                                    + "abcedfghijklmnopqrstuvwxyzabcd"
                                    + "123456789012345678901234567890"
                                    + "abcedfghijklmnopqrstuvwxyzabcd"; //data = 180 个字符
            ipWrite(contentBarcode);

            // Print out data of PDF417, [function 081]
            byte[] printPDF417 = new byte[] { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x30, 0x51, 0x30 };
            ipWrite(printPDF417, 0, printPDF417.Length);

            // 2.PDF417 column set to be 4, [function 065]
            ipWrite("\n2.PDF417, 设置列数为 4!\n\n");
            byte[] columnPDF417 = new byte[] { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x30, 0x41, 0x04 };
            ipWrite(columnPDF417, 0, columnPDF417.Length);

            // Print out data of PDF417
            ipWrite(printPDF417, 0, printPDF417.Length);

            // 3.PDF417 rows set to be 64, [function 066]
            ipWrite("\n3.PDF417, 设置行数为 64!\n\n");
            columnPDF417[7] = (byte)'\x00';                         // 列数恢复默认，即自动计算
            ipWrite(columnPDF417, 0, columnPDF417.Length);

            // data =180, rows至少14行（28), rows > 28 才有意义, n=64
            byte[] rowPDF417 = new byte[] { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x30, 0x42, 0x20 };
            ipWrite(rowPDF417, 0, rowPDF417.Length);

            // Print out data of PDF417 
            ipWrite(printPDF417, 0, printPDF417.Length);

            // 4.PDF417 set width of module, [function 067]
            ipWrite("\n4.PDF417, 设置单元宽度为 5, (2<n<8)!!\n\n");
            rowPDF417[7] = (byte)'\x00';                            // 行数回复默认，即自动计算
            ipWrite(rowPDF417, 0, rowPDF417.Length);

            // data =180, width of module, n=2~8, default n=3
            byte[] widthPDF417 = new byte[] { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x30, 0x43, 0x05 };
            ipWrite(widthPDF417, 0, widthPDF417.Length);

            // Print out data of PDF417 
            ipWrite(printPDF417, 0, printPDF417.Length);

            // 5.PDF417 set height of row, [function 068]
            ipWrite("\n5.PDF417, 设置单元行高为 5, (2<n<8)!\n\n");
            widthPDF417[7] = (byte)'\x03';                            // 单元宽度回复默认，即n=3
            ipWrite(widthPDF417, 0, widthPDF417.Length);

            // data =180, height of row, n=2~8, default n=3
            byte[] heightPDF417 = new byte[] { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x30, 0x44, 0x05 };
            ipWrite(heightPDF417, 0, heightPDF417.Length);

            // Print out data of PDF417 
            ipWrite(printPDF417, 0, printPDF417.Length);

            // 6.PDF417 error crrection level, [function 069]
            ipWrite("\n6.PDF417, 纠错等级设置为 0!\n\n");
            heightPDF417[7] = (byte)'\x03';                            // 单元行高恢复默认，即n=3
            ipWrite(heightPDF417, 0, heightPDF417.Length);

            // data =180, error correction level to be level 0, word 2
            byte[] correctPDF417 = new byte[] { 0x1D, 0x28, 0x6B, 0x04, 0x00, 0x30, 0x45, 0x30, 0x30 };
            ipWrite(correctPDF417, 0, correctPDF417.Length);

            // Print out data of PDF417 
            ipWrite(printPDF417, 0, printPDF417.Length);

            // 7.PDF417 option, standard or truncated
            ipWrite("\n7.PDF417, 设置为截取形式!\n\n");
            correctPDF417[7] = (byte)'\x31';                            // 纠错等级恢复默认，即n=1
            correctPDF417[8] = (byte)'\x01';
            ipWrite(correctPDF417, 0, correctPDF417.Length);

            // data =180, truncated PDF417
            byte[] optionPDF417 = new byte[] { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x30, 0x46, 0x01 };
            ipWrite(optionPDF417, 0, optionPDF417.Length);

            // Print out data of PDF417 
            ipWrite(printPDF417, 0, printPDF417.Length);
            
            // Feed and cut paper
            //ipWrite(cutPaper, 0, cutPaper.Length);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            // Feed and cut paper, GS V
            //byte[] cutPaper = new byte[] { 0x1D, 0x56, 0x42, 0x00 };

            //------------------------------------------------------
            ipWrite("测试二维条码，QR Code!\n\n");

            // 1.QR Code default setting printing
            ipWrite("\n1.QR Code, 默认设置!\n\n");

            // Store the data in symbol storage area
            byte[] storeQRCode = new byte[] { 0x1D, 0x28, 0x6B, 0xB7, 0x00, 0x31, 0x50, 0x30 };
            ipWrite(storeQRCode, 0, storeQRCode.Length);

            String contentBarcode = "123456789012345678901234567890"
                                    + "abcedfghijklmnopqrstuvwxyzabcd"
                                    + "123456789012345678901234567890"
                                    + "abcedfghijklmnopqrstuvwxyzabcd"
                                    + "123456789012345678901234567890"
                                    + "abcedfghijklmnopqrstuvwxyzabcd"; //data = 180 个字符
            ipWrite(contentBarcode);

            // Print out data of QR Code
            byte[] printQRCode = new byte[] { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x51, 0x30 };
            ipWrite(printQRCode, 0, printQRCode.Length);

            // 2.QR Code select model 1, n=49, (default n=50)
            ipWrite("\n2.QR Code, 设置模式1! (默认为模式2)\n\n");
            byte[] modQRCode = new byte[] { 0x1D, 0x28, 0x6B, 0x04, 0x00, 0x31, 0x41, 0x31, 0x00 };
            ipWrite(modQRCode, 0, modQRCode.Length);

            // Print out data of QR Code 
            ipWrite(printQRCode, 0, printQRCode.Length);

            // 3.QR Code set size of module to 7 (n=1~16)
            ipWrite("\n3.QR Code, 设置单元尺寸为 7! (默认值为 3)\n\n");
            modQRCode[7] = (byte)'\x32';
            ipWrite(modQRCode, 0, modQRCode.Length);

            // size n=7, (default n=3)
            byte[] sizeQRCode = new byte[] { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x43, 0x07 };
            ipWrite(sizeQRCode, 0, sizeQRCode.Length);

            // Print out data of QR Code
            ipWrite(printQRCode, 0, printQRCode.Length);

            // 4.QR Code error correction level to Q, (default L)
            ipWrite("\n4.QR Code, 设置纠错等级为 Q!\n\n");
            sizeQRCode[7] = (byte)'\x03';
            ipWrite(sizeQRCode, 0, sizeQRCode.Length);

            // Set error correction level to Q, (L, M, Q, H), default=L
            byte[] correctQRCode = new byte[] { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x45, 0x32 };
            ipWrite(correctQRCode, 0, correctQRCode.Length);

            // Print out data of QR Code
            ipWrite(printQRCode, 0, printQRCode.Length);

            // Return to default
            correctQRCode[7] = (byte)'\x30';
            ipWrite(correctQRCode, 0, correctQRCode.Length);
            
            // Feed and cut paper
            //ipWrite(cutPaper, 0, cutPaper.Length);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            // Feed and cut paper, GS V
            //byte[] cutPaper = new byte[] { 0x1D, 0x56, 0x42, 0x00 };

            //------------------------------------------------------
            ipWrite("测试二维条码，MaxiCode!\n\nMaxiCode最初为UPS公司发明，适合高速扫描环境，比较适合物流公司使用。\n"
                                + "正常最大容量93个字符，纯数字最大可到138个。\n");

            // 1.MaxiCode default setting printing
            // 2~6模式分别由不同定义，可网上查询MaxiCode的详细说明
            ipWrite("\nMaxiCode, 设置模式4! (默认为模式2)\n");

            // Store the data in symbol storage area
            byte[] storeMaxiCode = new byte[] { 0x1D, 0x28, 0x6B, 0x5D, 0x00, 0x32, 0x50, 0x30 };
            ipWrite(storeMaxiCode, 0, storeMaxiCode.Length);

            String contentBarcode = "123456789012345678901234567890"
                                    + "abcedfghijklmnopqrstuvwxyzabcd"                                    
                                    + "123456789012345678901234567890"; //data = 90 个字符, 
            ipWrite(contentBarcode);

            // MaxiCode select model 5, n=53, (default n=50)            
            byte[] modMaxiCode = new byte[] { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x32, 0x41, 0x34 };
            ipWrite(modMaxiCode, 0, modMaxiCode.Length);
            
            // Print out data of MaxiCode
            byte[] printMaxiCode = new byte[] { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x32, 0x51, 0x30 };
            ipWrite(printMaxiCode, 0, printMaxiCode.Length);

            // Return to default
            modMaxiCode[7] = (byte)'\x32';
            ipWrite(modMaxiCode, 0, modMaxiCode.Length);

            // Feed and cut paper
            //ipWrite(cutPaper, 0, cutPaper.Length);

        }

        private void button6_Click(object sender, EventArgs e)
        {
            // Feed and cut paper, GS V
            //byte[] cutPaper = new byte[] { 0x1D, 0x56, 0x42, 0x00 };

            //------------------------------------------------------
            ipWrite("测试二维条码，GS1 DataBar\n");

            // 1.GS1 DataBar type = "GS1 DataBar Stacked"
            ipWrite("\n1.GS1 DataBar Stacked, 默认设置!\n"
                               + "可处理GTIN-8,12,13,14，不足14位的，在左侧补0.\n");

            // Store the data in symbol storage area
            byte[] storeGSCode = new byte[] { 0x1D, 0x28, 0x6B, 0x12, 0x00, 0x33, 0x50, 0x30, 0x48 };
            ipWrite(storeGSCode, 0, storeGSCode.Length);

            String contentBarcode = "12345678901234"; //data = 14 个字符, 
            ipWrite(contentBarcode);

            // Print out data of GS1 DataBar
            byte[] printGSCode = new byte[] { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x33, 0x51, 0x30 };
            ipWrite(printGSCode, 0, printGSCode.Length);
                                 
            // 2.GS1 DataBar type = "GS1 DataBar Stacked Omnidirectional"
            ipWrite("\n2.GS1 DataBar Stacked Omnidirectional, 默认设置!\n"
                               + "也可处理GTIN-8,12,13,14，不足14位的，在左侧补0.\n");

            // Store the data in symbol storage area
            storeGSCode[8] = (byte)'\x49';
            ipWrite(storeGSCode, 0, storeGSCode.Length);                      
            ipWrite(contentBarcode);

            // Print out data of GS1 DataBar
            ipWrite(printGSCode, 0, printGSCode.Length);

            // 3.GS1 DataBar type = "GS1 DataBar Expanded Stacked"
            ipWrite("\n3.GS1 DataBar Expanded Stacked, 默认设置!\n"
                               + "最多74个数字（或41个字符），最多11层.\n");

            // Store the data in symbol storage area
            storeGSCode[3] = (byte)'\x2C';
            storeGSCode[8] = (byte)'\x4C';
            ipWrite(storeGSCode, 0, storeGSCode.Length);      

            String contentBarcode2 = "1234567890123456789012345678901234567890"; //data = 40 个字符, 
            ipWrite(contentBarcode2);

            // Print out data of GS1 DataBar
            ipWrite(printGSCode, 0, printGSCode.Length);

            // 4.GS1 DataBar type = "GS1 DataBar Expanded Stacked"
            // Change the width of one module
            ipWrite("\n4.GS1 DataBar Expanded Stacked\n设置单元宽度为 3,（默认值2）!\n");

            // GS1 DataBar select model 3, (default n=2)     
            byte[] modGSCode = new byte[] { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x33, 0x43, 0x03 };
            ipWrite(modGSCode, 0, modGSCode.Length);

            // Print out data of GS1 DataBar
            ipWrite(printGSCode, 0, printGSCode.Length);

            // set the width of module to default
            modGSCode[7] = (byte)'\x02';
            ipWrite(modGSCode, 0, modGSCode.Length);

            // 5.GS1 DataBar type = "GS1 DataBar Expanded Stacked"
            // Change the maximum width of GS1 DataBar Expanded Stacked
            ipWrite("\n5.GS1 DataBar Expanded Stacked\n设置最大宽度304点,（默认值160点宽）!\n");

            // GS1 DataBar select maximum width, (default nL=160)     
            byte[] widthGSCode = new byte[] { 0x1D, 0x28, 0x6B, 0x04, 0x00, 0x33, 0x47, 0x30, 0x01 };
            ipWrite(widthGSCode, 0, widthGSCode.Length);

            // Print out data of GS1 DataBar
            ipWrite(printGSCode, 0, printGSCode.Length);

            // Feed and cut paper
            //ipWrite(cutPaper, 0, cutPaper.Length);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            // Feed and cut paper, GS V
            //byte[] cutPaper = new byte[] { 0x1D, 0x56, 0x42, 0x00 };

            //------------------------------------------------------
            ipWrite("测试二维复合条码\n即指定一维条码与二维条码混合的形式。\n");

            // 1.Composite Symbology default setting printing
            ipWrite("\n1.Composite Symbology (EAN8 + CC-A/B/C), 默认设置\n");

            // Store the data in symbol storage area
            // 定义一维部分数据
            byte[] storeCompCode = new byte[] { 0x1D, 0x28, 0x6B, 0x0D, 0x00, 0x34, 0x50, 0x30, 0x30, 0x41 };
            ipWrite(storeCompCode, 0, storeCompCode.Length);

            String contentBarcode = "12345678"; // data=8 个字符, 
            ipWrite(contentBarcode);

            // 定义二维部分数据
            storeCompCode[8] = (byte)'\x31';    // 定义二维，a=49
            ipWrite(storeCompCode, 0, storeCompCode.Length);
            ipWrite(contentBarcode);

            // Print out data of Composite Symbology
            byte[] printCompCode = new byte[] { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x34, 0x51, 0x30 };
            ipWrite(printCompCode, 0, printCompCode.Length);

            // 2.Composite Symbology, change module width
            ipWrite("\n2.Composite Symbology (EAN13 + CC-A/B/C), 默认设置\n单元宽度为 5，默认值 2\n");

            // Store the data in symbol storage area
            // 定义一维部分数据
            storeCompCode[3] = (byte)'\x12';    // nL=18
            storeCompCode[8] = (byte)'\x30';    // 定义一维，a=48
            storeCompCode[9] = (byte)'\x42';    // EAN13
            ipWrite(storeCompCode, 0, storeCompCode.Length);

            String contentBarcode2 = "0123456789012"; //data = 13 个字符, 
            ipWrite(contentBarcode2);

            // 定义二维部分数据
            storeCompCode[8] = (byte)'\x31';    // 定义二维，a=49
            storeCompCode[9] = (byte)'\x41';    // 二维AUTO
            ipWrite(storeCompCode, 0, storeCompCode.Length);
            ipWrite(contentBarcode2);

            // Composite Symbology; Set module width n=5, (default n=2)
            byte[] widthCompCode = new byte[] { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x34, 0x43, 0x05 };
            ipWrite(widthCompCode, 0, widthCompCode.Length);

            // Print out data of Composite Symbology            
            ipWrite(printCompCode, 0, printCompCode.Length);

            // Change module width back to default
            widthCompCode[7] = (byte)'\x02';
            ipWrite(widthCompCode, 0, widthCompCode.Length);

            // 3.Composite Symbology, turn on HRI, FontA
            ipWrite("\n3.Composite Symbology (UPC-A 12-digit + CC-A/B/C), 显示HRI，FontA\n");

            // Store the data in symbol storage area
            // 定义一维部分数据
            storeCompCode[3] = (byte)'\x11';    // nL=17=12+5
            storeCompCode[8] = (byte)'\x30';    // 定义一维，a=48
            storeCompCode[9] = (byte)'\x43';    // UPC-A
            ipWrite(storeCompCode, 0, storeCompCode.Length);

            String contentBarcode3 = "098765432198"; //data = 12 个字符, 
            ipWrite(contentBarcode3);

            // 定义二维部分数据
            storeCompCode[8] = (byte)'\x31';    // 定义二维，a=49
            storeCompCode[9] = (byte)'\x41';    // 二维AUTO
            ipWrite(storeCompCode, 0, storeCompCode.Length);            
            ipWrite(contentBarcode3);

            // Composite Symbology; HRI, FontA
            byte[] HriCode = new byte[] { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x34, 0x48, 0x01 };
            ipWrite(HriCode, 0, HriCode.Length);

            // Print out data of Composite Symbology            
            ipWrite(printCompCode, 0, printCompCode.Length);

            // 4.Composite Symbology, turn on HRI, FontB
            ipWrite("\n4.Composite Symbology (UPC-E 6-digit + CC-A/B/C), 显示HRI，FontB\n");

            // Store the data in symbol storage area
            // 定义一维部分数据
            storeCompCode[3] = (byte)'\x0B';    // nL=11=6+5
            storeCompCode[8] = (byte)'\x30';    // 定义一维，a=48
            storeCompCode[9] = (byte)'\x44';    // UPC-E
            ipWrite(storeCompCode, 0, storeCompCode.Length);

            String contentBarcode4 = "654321"; // data = 6 个字符, 
            ipWrite(contentBarcode4);

            // 定义二维部分数据
            storeCompCode[8] = (byte)'\x31';    // 定义二维，a=49
            storeCompCode[9] = (byte)'\x41';    // 二维AUTO
            ipWrite(storeCompCode, 0, storeCompCode.Length);            
            ipWrite(contentBarcode4);

            // Composite Symbology; HRI, FontB
            HriCode[7] = (byte)'\x02';
            ipWrite(HriCode, 0, HriCode.Length);

            // Print out data of Composite Symbology            
            ipWrite(printCompCode, 0, printCompCode.Length);

            // 5.Composite Symbology, turn on HRI, Special FontA
            ipWrite("\n5.Composite Symbology (UPC-E 11-digit + CC-A/B/C), 显示HRI，Special FontA\n");

            // Store the data in symbol storage area
            // 定义一维部分数据
            storeCompCode[3] = (byte)'\x11';    // nL=17=12+5
            storeCompCode[8] = (byte)'\x30';    // 定义一维，a=48
            storeCompCode[9] = (byte)'\x45';    // UPC-E 11-digit
            ipWrite(storeCompCode, 0, storeCompCode.Length);

            String contentBarcode5 = "023456000073"; // data = 12 个字符, 
            ipWrite(contentBarcode5);

            // 定义二维部分数据
            storeCompCode[8] = (byte)'\x31';    // 定义二维，a=49
            storeCompCode[9] = (byte)'\x41';    // 二维AUTO
            ipWrite(storeCompCode, 0, storeCompCode.Length);
            ipWrite(contentBarcode5);

            // Composite Symbology; HRI, Special FontA
            HriCode[7] = (byte)'\x61';
            ipWrite(HriCode, 0, HriCode.Length);

            // Print out data of Composite Symbology            
            ipWrite(printCompCode, 0, printCompCode.Length);

            // Feed and cut paper
            //ipWrite(cutPaper, 0, cutPaper.Length);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            // Feed and cut paper, GS V
            //byte[] cutPaper = new byte[] { 0x1D, 0x56, 0x42, 0x00 };

            //------------------------------------------------------
            ipWrite("测试二维复合条码\n即指定一维GS1 DataBar条码与二维条码混合的形式。\n");

            // Composite Symbology; HRI, FontA
            byte[] HriCode = new byte[] { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x34, 0x48, 0x02 };
            ipWrite(HriCode, 0, HriCode.Length);
            
            // 1.Composite Symbology, GS1 DataBar Ominidirectional + CC-A/B/C
            ipWrite("\n1.Composite Symbology \n(GS1 DataBar Ominidirectional + CC-A/B/C)\n");

            // Store the data in symbol storage area
            // 定义一维部分数据
            byte[] storeCompCode = new byte[] { 0x1D, 0x28, 0x6B, 0x13, 0x00, 0x34, 0x50, 0x30, 0x30, 0x46 };
            ipWrite(storeCompCode, 0, storeCompCode.Length);

            String contentBarcode = "12345678901234"; // data=14 个字符, 
            ipWrite(contentBarcode);

            // 定义二维部分数据
            storeCompCode[8] = (byte)'\x31';    // 定义二维，a=49
            storeCompCode[9] = (byte)'\x41';    // 二维AUTO
            ipWrite(storeCompCode, 0, storeCompCode.Length);
            ipWrite(contentBarcode);

            // Print out data of Composite Symbology
            byte[] printCompCode = new byte[] { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x34, 0x51, 0x30 };
            ipWrite(printCompCode, 0, printCompCode.Length);

            // 2.Composite Symbology, GS1 DataBar Truncated + CC-A/B/C
            ipWrite("\n2.Composite Symbology \n(GS1 DataBar Truncated + CC-A/B/C)\n");

            // Store the data in symbol storage area
            // 定义一维部分数据
            storeCompCode[8] = (byte)'\x30';    // 定义一维，a=48
            storeCompCode[9] = (byte)'\x47';    // GS1 DataBar Truncated
            ipWrite(storeCompCode, 0, storeCompCode.Length);                        
            ipWrite(contentBarcode);

            // 定义二维部分数据
            storeCompCode[8] = (byte)'\x31';    // 定义二维，a=49
            storeCompCode[9] = (byte)'\x41';    // 二维AUTO
            ipWrite(storeCompCode, 0, storeCompCode.Length);
            ipWrite(contentBarcode);

            // Print out data of Composite Symbology            
            ipWrite(printCompCode, 0, printCompCode.Length);

            // 3.Composite Symbology, GS1 DataBar Stacked + CC-A/B/C
            ipWrite("\n3.Composite Symbology \n(GS1 DataBar Stacked + CC-A/B/C)\n");

            // Store the data in symbol storage area
            // 定义一维部分数据
            storeCompCode[3] = (byte)'\x13';    // nL=19=14+5
            storeCompCode[8] = (byte)'\x30';    // 定义一维，a=48
            storeCompCode[9] = (byte)'\x48';    // GS1 DataBar Stacked
            ipWrite(storeCompCode, 0, storeCompCode.Length);

            ipWrite(contentBarcode);

            // 定义二维部分数据
            storeCompCode[8] = (byte)'\x31';    // 定义二维，a=49
            storeCompCode[9] = (byte)'\x41';    // 二维AUTO
            ipWrite(storeCompCode, 0, storeCompCode.Length);
            ipWrite(contentBarcode);

            // Print out data of Composite Symbology            
            ipWrite(printCompCode, 0, printCompCode.Length);

            // 4.Composite Symbology, GS1 DataBar Stacked Omnidirectional + CC-A/B/C
            ipWrite("\n4.Composite Symbology \n(GS1 DataBar Stacked Omnidirectional + CC-A/B/C)\n");

            // Store the data in symbol storage area
            // 定义一维部分数据
            storeCompCode[3] = (byte)'\x13';    // nL=19=14+5
            storeCompCode[8] = (byte)'\x30';    // 定义一维，a=48
            storeCompCode[9] = (byte)'\x49';    // GS1 DataBar Stacked Omnidirectional
            ipWrite(storeCompCode, 0, storeCompCode.Length);

            ipWrite(contentBarcode);

            // 定义二维部分数据
            storeCompCode[8] = (byte)'\x31';    // 定义二维，a=49
            storeCompCode[9] = (byte)'\x41';    // 二维AUTO
            ipWrite(storeCompCode, 0, storeCompCode.Length);
            ipWrite(contentBarcode);

            // Print out data of Composite Symbology            
            ipWrite(printCompCode, 0, printCompCode.Length);

            // 5.Composite Symbology, GS1 DataBar Limited + CC-A/B/C
            ipWrite("\n5.Composite Symbology \n(GS1 DataBar Limited + CC-A/B/C)\n");

            // Store the data in symbol storage area
            // 定义一维部分数据
            storeCompCode[3] = (byte)'\x13';    // nL=19=14+5
            storeCompCode[8] = (byte)'\x30';    // 定义一维，a=48
            storeCompCode[9] = (byte)'\x4A';    // GS1 DataBar Limited
            ipWrite(storeCompCode, 0, storeCompCode.Length);

            ipWrite(contentBarcode);

            // 定义二维部分数据
            storeCompCode[8] = (byte)'\x31';    // 定义二维，a=49
            storeCompCode[9] = (byte)'\x41';    // 二维AUTO
            ipWrite(storeCompCode, 0, storeCompCode.Length);
            ipWrite(contentBarcode);

            // Print out data of Composite Symbology            
            ipWrite(printCompCode, 0, printCompCode.Length);

            // 6.Composite Symbology, GS1 DataBar Expanded + CC-A/B/C
            ipWrite("\n6.Composite Symbology \n(GS1 DataBar Expanded + CC-A/B/C)\n");

            // Store the data in symbol storage area
            // 定义一维部分数据
            storeCompCode[3] = (byte)'\x13';    // nL=19=14+5
            storeCompCode[8] = (byte)'\x30';    // 定义一维，a=48
            storeCompCode[9] = (byte)'\x4B';    // GS1 DataBar Expanded
            ipWrite(storeCompCode, 0, storeCompCode.Length);

            ipWrite(contentBarcode);

            // 定义二维部分数据
            storeCompCode[8] = (byte)'\x31';    // 定义二维，a=49
            storeCompCode[9] = (byte)'\x41';    // 二维AUTO
            ipWrite(storeCompCode, 0, storeCompCode.Length);
            ipWrite(contentBarcode);

            // Print out data of Composite Symbology            
            ipWrite(printCompCode, 0, printCompCode.Length);

            // 7.Composite Symbology, GS1 DataBar Expanded Stacked + CC-A/B/C
            ipWrite("\n7.Composite Symbology \n(GS1 DataBar Expanded Stacked + CC-A/B/C)\n");

            // Store the data in symbol storage area
            // 定义一维部分数据
            storeCompCode[3] = (byte)'\x13';    // nL=19=14+5
            storeCompCode[8] = (byte)'\x30';    // 定义一维，a=48
            storeCompCode[9] = (byte)'\x4C';    // GS1 DataBar Expanded Stacked
            ipWrite(storeCompCode, 0, storeCompCode.Length);

            ipWrite(contentBarcode);

            // 定义二维部分数据
            storeCompCode[8] = (byte)'\x31';    // 定义二维，a=49
            storeCompCode[9] = (byte)'\x41';    // 二维AUTO
            ipWrite(storeCompCode, 0, storeCompCode.Length);
            ipWrite(contentBarcode);

            // Print out data of Composite Symbology            
            ipWrite(printCompCode, 0, printCompCode.Length);
            
            // Feed and cut paper
            //ipWrite(cutPaper, 0, cutPaper.Length);
        }  
    }
}
