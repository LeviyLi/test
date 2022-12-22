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
                    button9.Enabled = true;
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
            button5.Enabled = false;
            button6.Enabled = false;
            button7.Enabled = false;
            button8.Enabled = false;
            button9.Enabled = false;
        }       

        private void button1_Click(object sender, EventArgs e)
        {
            /*------ String data to be written ------*/
            String msg1 = "EPSON (CHINA) CORP.\x0A";
            String msg2 = "爱普生(中国)有限公司\x0A";
            int i = 0;

            // Set Font size, GS !
            byte[] fontSize = new byte[] { 0x1D, 0x21, 0x00 };

            // Feed 4 lines, ESC d  
            byte[] feed4Lines = new byte[] { 0x1b, 0x64, 0x04 };

            // Feed and cut paper, GS V
            byte[] cutPaper = new byte[] { 0x1D, 0x56, 0x42, 0x00 };

            //------------------------------------------------------
            // 1.Normal size
            for (i = 0; i < 3; i++)
            {
                ipWrite(msg1);
                ipWrite(msg2);
            }
            ipWrite(feed4Lines, 0, feed4Lines.Length);

            // 2.Double width
            ipWrite("Double width\n");

            fontSize[2] = (byte)'\x10';
            ipWrite(fontSize, 0, fontSize.Length);

            for (i = 0; i < 3; i++)
            {
                ipWrite(msg1);
                ipWrite(msg2);
            }
            ipWrite(feed4Lines, 0, feed4Lines.Length);

            fontSize[2] = (byte)'\x00';                         //Set back to normal
            ipWrite(fontSize, 0, fontSize.Length);

            // 3.Double height
            ipWrite("Double height\n");

            fontSize[2] = (byte)'\x01';
            ipWrite(fontSize, 0, fontSize.Length);

            for (i = 0; i < 3; i++)
            {
                ipWrite(msg1);
                ipWrite(msg2);
            }
            ipWrite(feed4Lines, 0, feed4Lines.Length);

            fontSize[2] = (byte)'\x00';                         //Set back to normal
            ipWrite(fontSize, 0, fontSize.Length);

            // 4.Set font to be 3x3
            ipWrite("3x3 size\n");

            fontSize[2] = (byte)'\x22';
            ipWrite(fontSize, 0, fontSize.Length);

            for (i = 0; i < 3; i++)
            {
                ipWrite(msg1);
                ipWrite(msg2);
            }
            ipWrite(feed4Lines, 0, feed4Lines.Length);

            fontSize[2] = (byte)'\x00';                         //Set back to normal
            ipWrite(fontSize, 0, fontSize.Length);

            // 5.Set font to be biggest, 8x8
            ipWrite("8x8 size, the biggest!\n");
            ipWrite("And demo turn smoothing mode ON\n");

            fontSize[2] = (byte)'\x77';
            ipWrite(fontSize, 0, fontSize.Length);

            ipWrite(msg1);
            ipWrite(msg2);

            fontSize[1] = (byte)'\x62';
            fontSize[2] = (byte)'\x01';
            ipWrite(fontSize, 0, fontSize.Length);
            
            ipWrite(msg1);
            ipWrite(msg2);

            fontSize[2] = (byte)'\x00';
            ipWrite(fontSize, 0, fontSize.Length);

            ipWrite(feed4Lines, 0, feed4Lines.Length);

            fontSize[1] = (byte)'\x21';
            fontSize[2] = (byte)'\x00';                         //Set back to normal
            ipWrite(fontSize, 0, fontSize.Length);

            // Feed and cut paper
            ipWrite(cutPaper, 0, cutPaper.Length);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            /*------ String data to be written ------*/
            String msg1 = "EPSON (CHINA) CORP.\x0A";
            String msg2 = "爱普生(中国)有限公司\x0A";
            int i = 0;

            // Set Font style, ESC !
            byte[] fontStyle = new byte[] { 0x1b, 0x21 , 0x00 };

            // Feed 4 lines, ESC d  
            byte[] feed4Lines = new byte[] { 0x1b, 0x64, 0x04 };

            // Feed and cut paper, GS V
            byte[] cutPaper = new byte[] { 0x1D, 0x56, 0x42, 0x00 };

            //------------------------------------------------------
            // 1.Default font sytle
            for (i = 0; i < 3; i++)
            {
                ipWrite(msg1);
                ipWrite(msg2);
            }
            ipWrite(feed4Lines, 0, feed4Lines.Length);

            // 2.Select fontB,  or use "ESC M"
            ipWrite("ANK fontB\n");   
            fontStyle[2] = (byte)'\x01';               
            ipWrite(fontStyle, 0, fontStyle.Length);  

            for (i = 0; i < 3; i++)
            {
                ipWrite(msg1);
                ipWrite(msg2);
            }
            ipWrite(feed4Lines, 0, feed4Lines.Length);

            fontStyle[2] = (byte)'\x00';
            ipWrite(fontStyle, 0, fontStyle.Length); 

            // 3.Set emphasized mode, or use "ESC E", "ESC G"
            ipWrite("Emphasized\n");

            fontStyle[2] = (byte)'\x08';                
            ipWrite(fontStyle, 0, fontStyle.Length);

            for (i = 0; i < 3; i++)
            {
                ipWrite(msg1);
                ipWrite(msg2);
            }
            ipWrite(feed4Lines, 0, feed4Lines.Length);

            fontStyle[2] = (byte)'\x00';
            ipWrite(fontStyle, 0, fontStyle.Length);

            // 4.Set underline mode, or use "ESC -"
            ipWrite("Underline\n");
            
            fontStyle[2] = (byte)'\x80';
            ipWrite(fontStyle, 0, fontStyle.Length);

            for (i = 0; i < 3; i++)
            {
                ipWrite(msg1);
                ipWrite(msg2);
            }
            ipWrite(feed4Lines, 0, feed4Lines.Length);

            fontStyle[2] = (byte)'\x00';
            ipWrite(fontStyle, 0, fontStyle.Length);

            // 5.Set underline 2-dot width
            ipWrite("Underline 2-dot\n");

            fontStyle[1] = (byte)'\x2d';
            fontStyle[2] = (byte)'\x02';
            ipWrite(fontStyle, 0, fontStyle.Length);

            for (i = 0; i < 3; i++)
            {
                ipWrite(msg1);
                ipWrite(msg2);
            }
            ipWrite(feed4Lines, 0, feed4Lines.Length);

            fontStyle[2] = (byte)'\x00';
            ipWrite(fontStyle, 0, fontStyle.Length);

            // 6.ESC V, Turn 90 degree clockwise rotation mode on/off
            ipWrite("Turn 90 degree clockwise ON\n");

            fontStyle[1] = (byte)'\x56';
            fontStyle[2] = (byte)'\x01';
            ipWrite(fontStyle, 0, fontStyle.Length);

            for (i = 0; i < 3; i++)
            {
                ipWrite(msg1);
                ipWrite(msg2);
            }
            ipWrite(feed4Lines, 0, feed4Lines.Length);

            fontStyle[2] = (byte)'\x00';
            ipWrite(fontStyle, 0, fontStyle.Length);

            // 7.ESC {, Turn upside-down
            ipWrite("Turn on upside-down\n");

            fontStyle[1] = (byte)'\x7b';
            fontStyle[2] = (byte)'\x01';
            ipWrite(fontStyle, 0, fontStyle.Length);

            for (i = 0; i < 3; i++)
            {
                ipWrite(msg1);
                ipWrite(msg2);
            }
            ipWrite(feed4Lines, 0, feed4Lines.Length);

            fontStyle[2] = (byte)'\x00';
            ipWrite(fontStyle, 0, fontStyle.Length);

            // 8.GS B, Turn white/black reverse print mode on/off
            ipWrite("Turn white/black reverse mode ON\n");

            fontStyle[0] = (byte)'\x1d';
            fontStyle[1] = (byte)'\x42';
            fontStyle[2] = (byte)'\x01';
            ipWrite(fontStyle, 0, fontStyle.Length);

            for (i = 0; i < 3; i++)
            {
                ipWrite(msg1);
                ipWrite(msg2);
            }
            ipWrite(feed4Lines, 0, feed4Lines.Length);

            fontStyle[2] = (byte)'\x00';
            ipWrite(fontStyle, 0, fontStyle.Length);

            // Feed and cut paper
            ipWrite(cutPaper, 0, cutPaper.Length);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            /*------ String data to be written ------*/
            String msg1 = "EPSON (CHINA) CORP.\x0A";
            String msg2 = "爱普生(中国)有限公司\x0A";

            // 1.Set chars alignment, ESC a n=0~2
            byte[] fontAlign = new byte[] { 0x1b, 0x61, 0x00 };

            // 默认是左对齐
            ipWrite("1.左对齐\n");
            ipWrite(msg1);
            ipWrite(msg2);

            // 居中
            fontAlign[2] = (byte)'\x01';
            ipWrite(fontAlign, 0, fontAlign.Length);
            ipWrite("居中\n");
            ipWrite(msg1);
            ipWrite(msg2);

            // 靠右
            fontAlign[2] = (byte)'\x02';
            ipWrite(fontAlign, 0, fontAlign.Length);
            ipWrite("右对齐\n");
            ipWrite(msg1);
            ipWrite(msg2);

            fontAlign[2] = (byte)'\x00';
            ipWrite(fontAlign, 0, fontAlign.Length);

            // 2.Print and feed paper, ESC J
            ipWrite("2.打印并进纸若干距离");
            byte[] printFeedPaper = new byte[] { 0x1b, 0x4A, 0xF0 };
            ipWrite(printFeedPaper, 0, printFeedPaper.Length);

            // 3.Set absolute print position, ESC $
            ipWrite("3.设置绝对打印位置\n\n");
            
            byte[] abPosition = new byte[] { 0x1b, 0x24, 0x60, 0x00 };
            ipWrite(abPosition, 0, abPosition.Length);

            ipWrite("你看我缩进打印位置了吧!\n");

            abPosition[2] = (byte)'\x0';
            ipWrite(abPosition, 0, abPosition.Length);

            ipWrite("又恢复了!\n\n");

            // 4.设置字符间距, ESC SP
            ipWrite("4.字符间距的设置只对西文字符起作用\n\n");

            byte[] charSpace = new byte[] { 0x1b, 0x20, 0x10 };
            ipWrite(charSpace, 0, charSpace.Length);
            ipWrite("asdfghjklqwer\n");
            ipWrite("你看对汉字不起作用的!\n\n");

            charSpace[2] = (byte)'\x00';
            ipWrite(charSpace, 0, charSpace.Length);
            ipWrite("Turn to normal char space!\n\n");

            // 5.Tab位置自定义, ESC D
            ipWrite("5.Tab位置自定义\n\n");
            ipWrite("Start\tTab8\tTab16\tTab24\tTab32\tTab40\n");

            byte[] tabDefine = new byte[] { 0x1b, 0x44, 0x0A, 0x14, 0x1E, 0x28, 0x00 };
            ipWrite(tabDefine, 0, tabDefine.Length);

            ipWrite("Start\tTab10\tTab20\tTab30\tTab40\n\n");

            // 6.相对位置移动打印, ESC \
            ipWrite("6.相对位置移动打印\n\n");
            ipWrite("\t起初在这里打印");

            byte[] relMove = new byte[] { 0x1B, 0x5C, 0x40, 0x80 };
            ipWrite(relMove, 0, relMove.Length);
            ipWrite("然后在这!\n\n");

            // 7.设置左边留白，GS L
            ipWrite("7.设置左边空白\n\n");
            
            byte[] leftBlank = new byte[] { 0x1D, 0x4C, 0x50, 0x00 };
            ipWrite(leftBlank, 0, leftBlank.Length);
            ipWrite("看到左边留出的空白了吗？看到左边留出的空白了吗？看到左边留出的空白了吗？看到左边留出的空白了吗？\n");

            byte[] printWidth = new byte[] { 0x1D, 0x57, 0x00, 0x01 };
            ipWrite(printWidth, 0, printWidth.Length);
            ipWrite("缩小允许打印的宽度，看到效果了吗？缩小允许打印的宽度，看到效果了吗？\n\n");

            byte[] initPrinter = new byte[] { 0x1B, 0x40 };
            ipWrite(initPrinter, 0, initPrinter.Length);
          
            // Feed and cut paper
            byte[] cutPaper = new byte[] { 0x1D, 0x56, 0x42, 0x00 };
            ipWrite(cutPaper, 0, cutPaper.Length);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            /*------ String data to be written ------*/
            String msg = "爱普生(中国)有限公司\x0A";

            // 1.行间距离 ESC 3 n=0~255
            byte[] lineSpace = new byte[] { 0x1b, 0x33, 0x00 };

            // 默认行间距
            ipWrite("1.默认行间距\n");
            ipWrite(msg);
            ipWrite(msg);

            // 最小行间距，n=0
            ipWrite(lineSpace, 0, lineSpace.Length);
            ipWrite("\n行间距为0\n");
            ipWrite(msg);
            ipWrite(msg);

            // 行间距离为48
            lineSpace[2] = (byte)'\x30';
            ipWrite(lineSpace, 0, lineSpace.Length);

            ipWrite("\n行间距离为48\n");
            ipWrite(msg);
            ipWrite(msg);

            // 行间距离为96
            lineSpace[2] = (byte)'\x60';
            ipWrite(lineSpace, 0, lineSpace.Length);
            
            ipWrite("\n行间距离为96\n");
            ipWrite(msg);
            ipWrite(msg);

            // 行间距离为192
            lineSpace[2] = (byte)'\xC0';
            ipWrite(lineSpace, 0, lineSpace.Length);

            ipWrite("\n行间距离为192\n");
            ipWrite(msg);
            ipWrite(msg);

            // 2.恢复默认行间距
            byte[] lineSpaceDefault = new byte[] { 0x1b, 0x32 };
            ipWrite(lineSpaceDefault, 0, lineSpaceDefault.Length);

            ipWrite("恢复默认行间距\n");
            ipWrite(msg);
            ipWrite(msg);

            // Feed and cut paper
            byte[] cutPaper = new byte[] { 0x1D, 0x56, 0x42, 0x00 };
            ipWrite(cutPaper, 0, cutPaper.Length);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            ipWrite("自定义字符并打印\n");
            ipWrite("特别注意，自定义字符和位图定义不能同时使用！\n");

            byte[] defineUserChar = new byte[] { 0x1B, 0x26, 0x03, 0x30, 0x30, 0x0C, 
                                0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
                                0x01, 0x02, 0x03, 0x01, 0x02, 0x03, 
                                0x01, 0x02, 0x03, 0x01, 0x02, 0x03, 
                                0x01, 0x02, 0x03, 0x01, 0x02, 0x03, 
                                0x01, 0x02, 0x03, 0x01, 0x02, 0x03, 
                                0x01, 0x02, 0x03, 0x01, 0x02, 0x03 };
            ipWrite(defineUserChar, 0, defineUserChar.Length);

            byte[] selectUserChar = new byte[] { 0x1B, 0x25, 0x01 };    //用户定义区
            ipWrite(selectUserChar, 0, selectUserChar.Length);

            byte[] printUserChar = new byte[] { 0x0A, 0x30, 0x0A };
            ipWrite(printUserChar, 0, printUserChar.Length);

            selectUserChar[2] = (byte)'\x0';                            //退出用户定义区
            ipWrite(selectUserChar, 0, selectUserChar.Length);

            ipWrite(printUserChar, 0, printUserChar.Length);

            byte[] cancelUserChar = new byte[] { 0x1B, 0x3F, 0x30 };    //删除用户定义的某字符
            ipWrite(cancelUserChar, 0, cancelUserChar.Length);

            //验证自定义的字符是否删除了定义
            //ipWrite(selectUserChar, 0, selectUserChar.Length);
            //ipWrite(printUserChar, 0, printUserChar.Length);
            //selectUserChar[2] = (byte)'\x0';                            //退出用户定义区
            //ipWrite(selectUserChar, 0, selectUserChar.Length);

            // Feed and cut paper
            byte[] cutPaper = new byte[] { 0x1D, 0x56, 0x42, 0x00 };
            ipWrite(cutPaper, 0, cutPaper.Length);

        }

        private void button6_Click(object sender, EventArgs e)
        {
            ipWrite("选择国际字符集\n");
            ipWrite("在ASCII 0～127范围内，各国对某些字符有习惯定义\n");

            // ESC R
            byte[] interCharSet = new byte[] { 0x1B, 0x52, 0x00 };
            byte[] interChars = new byte[] { 0x23, 0x24, 0x40, 0x5B, 0x5C, 0x5D, 0x5E,
                                             0x60, 0x7B, 0x7C, 0x7D, 0x7E, 0x0A };
 
            String[] interCharList = new String[] {"U.S.A.", "France", "Germany", 
                                                   "U.K.", "Denmark I", "Sweden",
                                                   "Italy", "Spain I", "Japan",
                                                   "Norway", "Denmark II", "Spain II",
                                                   "Latin America", "Korea", "Slovenia / Croatia",
                                                   "China"};
            for (int i = 0; i < 16; i++)
            {
                interCharSet[2] = (byte)i;
                ipWrite(interCharSet, 0, interCharSet.Length);
                ipWrite(String.Format("\nn = {0}, {1}\n", i, interCharList[i]));
                ipWrite(interChars, 0, interChars.Length);
            }

            // Feed and cut paper
            byte[] cutPaper = new byte[] { 0x1D, 0x56, 0x42, 0x00 };
            ipWrite(cutPaper, 0, cutPaper.Length);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            ipWrite("选择国际字符表\n");
            ipWrite("在单字节字符的128～255的范围内，各国对某些字符有习惯定义。\n");

            byte[] fsDot = new byte[] { 0x1C, 0x2E };
            ipWrite(fsDot, 0, fsDot.Length);

            byte[] interTableSet = new byte[] { 0x1B, 0x74, 0x00 };
            byte[] interTables = new byte[] { 0x00 };

            String[] interTableList = new String[] {"Page 0 [PC437 (USA: Standard Europe)]",
                                                    "Page 1 [Katakana]",
                                                    "Page 2 [PC850: Multilingual]",
                                                    "Page 3 [PC860 (Portuguese)]",
                                                    "Page 4 [PC863 (Canadian-French)",
                                                    "Page 5 [PC865 (Nordic)]",
                                                    "Page 16 [WPC1252]",
                                                    "Page 17 [PC866 (Cyrillic #2)]",
                                                    "Page 18 [PC852 (Latin 2)]",
                                                    "Page 19 [PC858 (Euro)]" };
            int[] interTableNumber = new int[] { 0, 1, 2, 3, 4, 5, 16, 17, 18, 19 };
            
            for(int i = 0; i < interTableList.Length; i++)
            {
                ipWrite(String.Format("\n\n\n n = {0}, {1}\n", interTableNumber[i], interTableList[i]));
             
                interTableSet[2] = (byte)interTableNumber[i];
                ipWrite(interTableSet, 0, interTableSet.Length);
                
                for (int j = 128; j < 255; j++)
                {
                    interTables[0] = (byte)j;
                    ipWrite(interTables, 0, interTables.Length);
                 }
            }
        
            // Feed and cut paper
            byte[] cutPaper = new byte[] { 0x0A, 0x1D, 0x56, 0x42, 0x00 };
            ipWrite(cutPaper, 0, cutPaper.Length);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            ipWrite("宏定义开始...\n");

            byte[] defMacro = new byte[] { 0x1D, 0x3A };
            ipWrite(defMacro, 0, defMacro.Length);

            ipWrite("宏定义打印的内容。\n");   //这里定义你需要的操作。
            
            ipWrite(defMacro, 0, defMacro.Length);

            ipWrite("宏定义结束。\n");
        }

        private void button9_Click(object sender, EventArgs e)
        {
            ipWrite("执行宏定义r次, 间隔t乘100毫秒\n");

            // GS ^ r t n=0
            // 间隔2秒，执行3次
            byte[] runMacro = new byte[] { 0x1D, 0x5E, 0x03, 0x14, 0x00 };
            ipWrite(runMacro, 0, runMacro.Length);
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
