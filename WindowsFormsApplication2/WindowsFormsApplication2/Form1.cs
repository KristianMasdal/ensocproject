//Hentet og modifisert Lab_uke38.pdf fra 
//https://usn.instructure.com/courses/18648/files/1033045/download?wrap=1
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

        //Kalles ved oppstart for opprettelse av socket på egen tråd
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
                    //Debug.WriteLine("invoking UpdDataReceived");
                    
                }
                catch (Exception e)
                {
                    Debug.WriteLine("ERROR " + e);
                }
            }
            sock.Close();
        }

        //Mottar bilder fra webcam på egen tråd. Sender en boolean som veksler mellom true og false
        //boolean'en brukes for å signalisere at webkameraet brukes ved at den ene LEDen blinker.
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

        //mottar data fra Node Red, brukes for å vise tilstand til låsen
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
        //startpunkt
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

        //veksler rødt/grønt signal bilde for å signalisere låsen
        private void lockDoor(Boolean locked)
        {
            pictureBox2.Visible = !locked;
            pictureBox1.Visible = locked;
        }

        private void pictureBox7_Click(object sender, EventArgs e)
        {

        }

        //sender tekstfelt innhold til webserver
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
