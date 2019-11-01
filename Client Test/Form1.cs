using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Bt.SysLib;
using Bt;
using System.Threading;
namespace Client_Test
{
    public partial class Form1 : Form
    {

        public static String resultOut;
        //--------------------------------------------------------------
		// DLLImport
		//--------------------------------------------------------------
		[DllImport("coredll.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Auto)]
		public static extern IntPtr CreateEvent(IntPtr lpEventAttributes, [In, MarshalAs(UnmanagedType.Bool)] bool bManualReset, [In, MarshalAs(UnmanagedType.Bool)] bool bIntialState, [In, MarshalAs(UnmanagedType.BStr)] string lpName);

        [DllImport("coredll.dll", SetLastError = true)]
        public static extern Int32 WaitForSingleObject(IntPtr Handle, Int32 Wait);

		[DllImport("coredll.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Auto)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool CloseHandle(IntPtr hObject);
        MsgWindow MsgWin;
        public Form1()
        {
            InitializeComponent();
            this.MsgWin = new MsgWindow();
            MsgWin.ScanSuccess += new EventHandler(handleScanOK);
        }
        private void setTextbox(TextBox tb, String Text)
        {
            tb.Invoke((Action)delegate
            {
                tb.Text = Text;
            });
        }
        private void setLabel(Label lbl, String Text, Color color)
        {
            lbl.Invoke((Action)delegate
            {
                lbl.Text = Text;
                lbl.BackColor = color;
            });
        }
        public void ExecuteClient(String data)
        {

            try
            {

                // Establish the remote endpoint  
                // for the socket. This example  
                // uses port 11111 on the local  
                // computer. 
                IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddr = IPAddress.Parse(textBox2.Text);
                IPEndPoint localEndPoint = new IPEndPoint(ipAddr,Convert.ToInt32(textBox3.Text) );

                // Creation TCP/IP Socket using  
                // Socket Class Costructor 
                Socket sender = new Socket(ipAddr.AddressFamily,
                           SocketType.Stream, ProtocolType.Tcp);

                try
                {

                    // Connect Socket to the remote  
                    // endpoint using method Connect() 
                    //s
                    sender.Connect(localEndPoint);

                    // We print EndPoint information  
                    // that we are connected 
                    //Console.WriteLine("Socket connected to -> {0} ",
                    //              sender.RemoteEndPoint.ToString());

                    // Creation of messagge that 
                    // we will send to Server 
                    //Thread.Sleep(500);
                    byte[] messageSent = Encoding.ASCII.GetBytes(data+"\n");
                    int byteSent = sender.Send(messageSent);

                    // Data buffer 
                    byte[] messageReceived = new byte[1024];

                    // We receive the messagge using  
                    // the method Receive(). This  
                    // method returns number of bytes 
                    // received, that we'll use to  
                    // convert them to string 
                    int byteRecv = sender.Receive(messageReceived);
                    String result = Encoding.ASCII.GetString(messageReceived, 0, byteRecv);
                    if (result == "OK\n")
                    {
                        setLabel(label1, "RESULT OK", Color.Green);
                        callLED(1000, 0, 1, 4);
                        callBuzzer(500, 100, 1, 10, 3);
                    }
                    else if (result == "NG\n")
                    {
                        setLabel(label1, "RESULT NOT GOOD", Color.Red);
                        callLED(1000, 0, 1, 2);
                        callBuzzer(500, 100, 1, 1, 3);

                    }
                    else
                    {
                        setLabel(label1, "RESULT ERROR", Color.Orange);
                    }
                    
                    //Console.WriteLine("Message from Server -> {0}",
                    //      Encoding.ASCII.GetString(messageReceived,
                    //                                 0, byteRecv));

                    // Close Socket using  
                    // the method Close() 
                    setTextbox(textBox1, "");
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();
                }

                // Manage of Socket's Exceptions 
                catch (ArgumentNullException ane)
                {
                    ErrorLED();
                    MessageBox.Show(ane.Message);//Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }

                catch (SocketException se)
                {
                    ErrorLED();
                    MessageBox.Show(se.Message);
                    //Console.WriteLine("SocketException : {0}", se.ToString());
                }

                catch (Exception ex)
                {
                    ErrorLED();
                    MessageBox.Show(ex.Message); //Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }
            }

            catch (Exception e)
            {
                ErrorLED();
                 MessageBox.Show(e.Message); //Console.WriteLine(e.ToString());
            }
        }
        private void ErrorLED()
        {
            callBuzzer(100, 50, 4, 1, 3);
            callLED(100, 50, 4, 2);
            label1.Text = "Connection Error";
            label1.BackColor = Color.Red;



        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            resultOut = textBox1.Text;
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ExecuteClient(textBox1.Text);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            textBox1.Focus();
            callBuzzer(500,100, 1, 16, 3);

            callLED(200, 100, 3, 1);
            Int32 ret = 0;
			String disp = "";
            
            try
            {
                // Scan mode = Set to "individual"
                //ScanMode = 1;

                ret = Bt.ScanLib.Control.btScanEnable();
                if (ret != LibDef.BT_OK)
                {
                    disp = "btScanEnable error ret[" + ret + "]";
                    MessageBox.Show(disp, "Error");
                    return;
                }
            }catch{


                }

        }
        private void handleScanOK(object s, EventArgs e)
        {
            //Thread.Sleep(900);
            //for (int i = 0; i < 100000000; i++)
            //{
            //    //do nothing 
            //}
            //ExecuteClient(textBox1.Text);
            //Thread.Sleep(100);
            //textBox1.Text = resultOut;
            //MessageBox.Show(resultOut);
            ExecuteClient(resultOut);
            textBox1.Focus();
        }
        private void callLED(int onTime, int offTime, int count, int color)
        {
            Int32 ret = 0;
            String disp = "";

            LibDef.BT_LED_PARAM stLedSet = new LibDef.BT_LED_PARAM();				// LED control structure (Set)

            // Set to repeat "500 ms on, 500 ms off" 3 times
            stLedSet.dwOn = Convert.ToUInt16(onTime);					// Rumble time [ms] (1 to 5000)
            stLedSet.dwOff = Convert.ToUInt16(offTime);						// Stop time [ms] (0 to 5000)
            stLedSet.dwCount = Convert.ToUInt16(count);					// Rumble count [times] (0 to 100)
            switch (color)
            {
                case 1:
                    stLedSet.bColor = LibDef.BT_LED_BLUE;	// Light color
                    break;
                case 2:
                    stLedSet.bColor = LibDef.BT_LED_RED;	// Light color
                    break;
                case 3:
                    stLedSet.bColor = LibDef.BT_LED_CYAN;	// Light color
                    break;
                case 4:
                    stLedSet.bColor = LibDef.BT_LED_GREEN;	// Light color
                    break;
                case 5:
                    stLedSet.bColor = LibDef.BT_LED_MAGENTA;	// Light color
                    break;
                case 6:
                    stLedSet.bColor = LibDef.BT_LED_WHITE;	// Light color
                    break;
                case 7:
                    stLedSet.bColor = LibDef.BT_LED_YELLOW;	// Light color
                    break;
                default:
                    stLedSet.bColor = LibDef.BT_LED_WHITE;	// Light color
                    break;
            }

            try
            {
                // btLED Light on
                ret = Device.btLED(1, stLedSet);
                if (ret != LibDef.BT_OK)
                {
                    disp = "btLED error ret[" + ret + "]";
                    MessageBox.Show(disp, "Error");
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        private void callBuzzer(int onTime, int offTime, int count, int tone, int volume)
        {
            Int32 ret = 0;
			String disp = "";

			Bt.LibDef.BT_BUZZER_PARAM stBuzzerSet = new Bt.LibDef.BT_BUZZER_PARAM();			// Buzzer control structure (Set)

			// Set to repeat "500 ms on, 500 ms off" 3 times
			stBuzzerSet.dwOn = Convert.ToUInt16(onTime);		// Rumble time [ms] (1 to 5000)
			stBuzzerSet.dwOff = Convert.ToUInt16(offTime);		// Stop time [ms] (0 to 5000)
			stBuzzerSet.dwCount = Convert.ToUInt16(count);	// Rumble count [times] (0 to 100)
			stBuzzerSet.bTone = Convert.ToByte(tone);		// Musical scale (1 to 16)
			stBuzzerSet.bVolume = Convert.ToByte(volume);	// Buzzer volume (1 to 3)

			try
			{
				// btBuzzer Rumble
				ret = Bt.SysLib.Device.btBuzzer(1, stBuzzerSet);
				if (ret != Bt.LibDef.BT_OK)
				{
					disp = "btBuzzer error ret[" + ret + "]";
					MessageBox.Show(disp, "Error");
					return;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

        private void button1_Click(object sender, EventArgs e)
        {
            //eventStarted();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }
        

    }
    public class scanSuccessArgs : EventArgs
    {
      public scanSuccessArgs(int iTotal)
      { Total = iTotal; }
      Int32 resultCount = 0;
      Byte[] codedataGet;
      Int32 codeLen = 0;
      LibDef.BT_SCAN_REPORT stReportGet = new LibDef.BT_SCAN_REPORT();
      LibDef.BT_SCAN_QR_REPORT stQrReportGet = new LibDef.BT_SCAN_QR_REPORT();
      public int Total { get; set; }
    }
    public class MsgWindow : Microsoft.WindowsCE.Forms.MessageWindow
    {
        public event EventHandler ScanSuccess;
        public MsgWindow()
        {
        }

        protected override void WndProc(ref Microsoft.WindowsCE.Forms.Message msg)
        {
            switch (msg.Msg)
            {
                case (Int32)LibDef.WM_BT_SCAN:
                    // When reading is successful
                    if (msg.WParam.ToInt32() == (Int32)LibDef.BTMSG_WPARAM.WP_SCN_SUCCESS)
                    {
                        //Thread.Sleep(200);
                        //Bt.SysLib.Device.
                        //Console.WriteLine(msg.Result.ToString());

                        Byte[] codedataGet;
                        String strCodedata = "";
                        Int32 codeLen = 0;
                        UInt16 symbolGet = 0;
                        codeLen = Bt.ScanLib.Control.btScanGetStringSize();
                        codedataGet = new Byte[codeLen];
                        int ret = Bt.ScanLib.Control.btScanGetString(codedataGet, ref symbolGet);
                        if (ret != LibDef.BT_OK)
                        {
                            Form1.resultOut = "ERROR BAMBANG";
                        }
                        Form1.resultOut = System.Text.Encoding.GetEncoding(0).GetString(codedataGet, 0, codeLen);
                        EventHandler handler = ScanSuccess;
                        if (null != handler) handler(this, EventArgs.Empty  );

                        //Form1.
                        //Console.Write("OK");
                        //if (ScanMode == 1)
                        //{
                        //    // Reading (individual)
                        //    //ScanData_kobetu();
                        //}
                        //else if (ScanMode == 2)
                        //{
                        //    // Reading (batch)
                        //    //ScanData_ikkatu();
                        //}
                    }
                    break;
            }
            base.WndProc(ref msg);
        }
    }
}