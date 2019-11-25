using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace WindowsFormsApplication2
{
    public partial class Form1 : Form
    {

        private Thread workerThread;
        private Thread streamerThread;
        public string strRecievedUDPMessage;
        public string key;
        Boolean blink = false;

        public void DoReceiveUdp()
        {
            
            UdpClient sock = new UdpClient(9050);
            IPEndPoint iep = new IPEndPoint(IPAddress.Any, 0);
            
            while (true)
            {
                try
                {
                    Debug.WriteLine("Looping..");
                    Debug.WriteLine("Blink: " + blink);
                    byte[] data = sock.Receive(ref iep);
                    //Debug.WriteLine("data stored");
                    strRecievedUDPMessage = Encoding.ASCII.GetString(data, 0, data.Length);
                    //Debug.WriteLine("data converted to string");
                    this.Invoke(new EventHandler(this.UdpDataReceived));
                    //Debug.WriteLine("invoking UpdDaaReceived");
                    
                }
                catch (Exception e)
                {
                    Debug.WriteLine("ERROR " + e);
                }
            }
            sock.Close();
        }

        public void streamCam()
        {
            while (true)
            {
                try
                {
                    if (blink == true)
                    {
                        pictureBox3.Load("http://128.39.112.23:5000/image?blink=True");
                        blink = false;
                    }
                    else
                    {
                        pictureBox3.Load("http://128.39.112.23:5000/image?blink=False");
                        blink = true;
                    }

                }
                catch (Exception e)
                {
                    Debug.WriteLine("ERROR " + e);
                }
            }
        }

        public void UdpDataReceived(object sender, EventArgs e)
        {
            
            if (!strRecievedUDPMessage.Equals("empty"))
            {
                textBox2.Text = strRecievedUDPMessage;
            }
            if (strRecievedUDPMessage.Equals("1"))
            {
                lockDoor(false);
            }
            else
            {
                lockDoor(true);
            }
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            pictureBox3.Load("https://media.giphy.com/media/tXL4FHPSnVJ0A/giphy.gif");
            workerThread = new Thread(this.DoReceiveUdp);
            workerThread.IsBackground = true;
            workerThread.Start();
            streamerThread = new Thread(this.streamCam);
            streamerThread.IsBackground = true;
            streamerThread.Start();
            lockDoor(true);
            
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Text = "$SW";
            if (checkBox1.Checked == true)
                textBox1.Text += ",1";
            else
                textBox1.Text += ",0";
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void checkBoxChanged()
        {
            textBox1.Text = "$LED";
            if (checkBox1.Checked == true)
                textBox1.Text += ",1";
            else
                textBox1.Text += ",0";
            

            byte[] data = new byte[1024];

            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("128.39.112.23"), 9050);

            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            string sendeString = textBox1.Text.ToString()+"\n";
            data = Encoding.ASCII.GetBytes(sendeString);
            server.SendTo(data, data.Length, SocketFlags.None, ipep);
            
        }



        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            checkBoxChanged();
            lockDoor(!checkBox1.Checked);

            byte[] data = new byte[1024];

            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("128.39.112.23"), 9050);

            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            string sendeString = textBox1.Text.ToString() + "\n";
            data = Encoding.ASCII.GetBytes(sendeString);
            server.SendTo(data, data.Length, SocketFlags.None, ipep);


        }


        private void lockDoor(Boolean locked)
        {
            pictureBox2.Visible = !locked;
            pictureBox1.Visible = locked;
        }

        private void pictureBox7_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            key = textBox1.Text.ToString();
            string url = "http://128.39.112.23:5000/createQR?text="+ key;
            pictureBox7.Load(url);
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {

        }
    }
}
