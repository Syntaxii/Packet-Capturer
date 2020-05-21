using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PacketDotNet;
using SharpPcap;

namespace MyPacketCapturer
{
    public partial class frmCapture : Form
    {
        CaptureDeviceList devices; //List of devices for this computer
        public static ICaptureDevice device; //The device we will be using
        public static String stringPackets = ""; //Data that is captured
        static int numPackets = 0;
        static int ip = 0, arp = 0, udp = 0, icmpv6 = 0, tcp = 0, rtp = 0, ipv6 = 0, ieee = 0, vlan = 0, new1 = 0;
        frmSend fSend; //This will be our send form

        public frmCapture()
        {
            InitializeComponent();
            devices = CaptureDeviceList.Instance; //Get the list of devices

            if(devices.Count < 1) //Check for at least one device
            {
                MessageBox.Show("No capture devices found.");
                Application.Exit();
            }
            //Add devices to combo box
            foreach(ICaptureDevice dev in devices)
            {
                cmbDevices.Items.Add(dev.Description);
            }

            //Get 2nd device and display in combo box
            device = devices[0];
            cmbDevices.Text = device.Description;

            //Register our handler function to the 'packet arrival' event
            device.OnPacketArrival += new SharpPcap.PacketArrivalEventHandler(device_OnPacketArrival);

            //Open  the device for capturing
            int readTimeoutMilliseconds = 1000;
            device.Open(DeviceMode.Promiscuous, readTimeoutMilliseconds);

        }

        private static void device_OnPacketArrival(object sender, CaptureEventArgs packet)
        {
            //Increment the number of packets captured
            numPackets++;

            //Put the packet number in the capture window
            stringPackets += "Packet Number: " + Convert.ToString(numPackets);
            stringPackets += Environment.NewLine;

            //Array to store our data
            byte[] data = packet.Packet.Data;

            //Keep track of the number of bytes displayed per line
            int byteCounter = 0;

            stringPackets += "Destination MAC Address: ";
            //Parsing the packets
            foreach (byte b in data)
            {
                //Add the byte to our string (in hexa)
                if (byteCounter <= 13) stringPackets += b.ToString("X2") + " ";
                byteCounter++;
                switch (byteCounter)
                {
                    case 6: stringPackets += Environment.NewLine;
                        stringPackets += "Source MAC Address: ";
                        break;
                    case 12: stringPackets += Environment.NewLine;
                        stringPackets += "EtherType: ";
                        break;
                    case 14:
                        if (data[12] != 8 && data[12] != 0 && data[12] != 134 && data[12] != 136)
                        {
                            new1++;
                        }
                        if (data[12] == 8) //08
                        {
                            if (data[13] == 0) //00
                            {
                                stringPackets += "(IP)";
                                ip++;
                                stringPackets += Environment.NewLine;
                                stringPackets += "Protocol: ";
                                if (data[23] == 17) //11
                                {
                                    stringPackets += "(UDP)";
                                    udp++;
                                }
                                if (data[23] == 6) //06 
                                {
                                    stringPackets += "(TCP)";
                                    tcp++;
                                }

                            }
                            
                            if (data[13] == 6) //06 
                            {
                                stringPackets += "(ARP)";
                                arp++;
                            }
                        }
                        if (data[12] == 0) //00
                        {
                            if (data[13] == 105) //69
                            {
                                stringPackets += "(RTP)";
                                rtp++;
                            }
                        }
                        if (data[12] == 129) //81 VLAN
                        {
                            stringPackets += "(VLAN)";
                            vlan++;
                        }
                        if (data[12] == 134) //86
                        {
                            if (data[13] == 221) //dd
                            { 
                                stringPackets += "(IPv6)";
                                ipv6++;
                                stringPackets += Environment.NewLine;
                                stringPackets += "Protocol: ";
                                if (data.Length > 62)
                                {
                                    if (data[62] == 143) //ICMPv6
                                    {
                                        stringPackets += "(ICMPv6)";
                                        icmpv6++;
                                    }
                                }
                            }
                        }
                        if (data[12] == 136) //88
                        {
                            if (data[13] == 183) //b7
                            {
                                stringPackets += "(IEEE)";
                                ieee++;
                            }
                        }
                        break;

                }
            }
            string ToReadableByteArray(byte[] bytes)
            {
                return string.Join(", ", bytes);
            }
            stringPackets += Environment.NewLine + Environment.NewLine;
            byteCounter = 0;
            stringPackets += "Raw Data" + Environment.NewLine;

            //Process each byte in our captured packet
            foreach(byte b in data)
            {
                //Add the byte to our string (in hexa)
                stringPackets += b.ToString("X2") + " ";
                byteCounter++;

                if(byteCounter == 16)
                {
                    byteCounter = 0;
                    stringPackets += Environment.NewLine;
                }
            }
            stringPackets += Environment.NewLine;
            stringPackets += Environment.NewLine;
        }
        private void btnStartStop_Click(object sender, EventArgs e)
        {
            try
            {
                if(btnStartStop.Text == "Start")
                {
                    device.StartCapture();
                    timer1.Enabled = true;
                    btnStartStop.Text = "Stop";
                }
                else
                {
                    device.StopCapture();
                    timer1.Enabled = false;
                    btnStartStop.Text = "Start";
                }
            }
            catch(Exception exp)
            {

            }
        }
        //Dump packet data from stringPackets to text box
        private void timer1_Tick(object sender, EventArgs e)
        {
            txtCapturedData.AppendText(stringPackets);
            stringPackets = "";
            txtNumPackets.Text = Convert.ToString(numPackets);
            //txtNumPackets.Text = numPackets + ""; //diff method
        }


        private void cmbDevices_SelectedIndexChanged(object sender, EventArgs e)
        {
            device = devices[cmbDevices.SelectedIndex];
            cmbDevices.Text = device.Description;

            //Register our handler function to the 'packet arrival' event
            device.OnPacketArrival += new SharpPcap.PacketArrivalEventHandler(device_OnPacketArrival);

            //Open  the device for capturing
            int readTimeoutMilliseconds = 1000;
            device.Open(DeviceMode.Promiscuous, readTimeoutMilliseconds);
        }

        private void resultBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            resultBox.Text += "Ethertypes" + Environment.NewLine;
            resultBox.Text += "IPv4: " + ip + Environment.NewLine;
            resultBox.Text += "ARP: " + arp + Environment.NewLine;
            resultBox.Text += "IPv6: " + ipv6 + Environment.NewLine;
            resultBox.Text += "RTP: " + rtp + Environment.NewLine;
            resultBox.Text += "IEEE: " + ieee + Environment.NewLine;
            resultBox.Text += "VLAN: " + vlan + Environment.NewLine;
            resultBox.Text += "NEW: " + new1 + Environment.NewLine + Environment.NewLine;

            resultBox.Text += "Protocols" + Environment.NewLine;
            resultBox.Text += "TCP: " + tcp + Environment.NewLine;
            resultBox.Text += "UDP: " + udp + Environment.NewLine;
            resultBox.Text += "ARP: " + arp + Environment.NewLine;
            resultBox.Text += "ICMPv6: " + icmpv6 + Environment.NewLine;
            resultBox.Text += "RTP: " + rtp + Environment.NewLine;
            resultBox.Text += "IEEE: " + ieee + Environment.NewLine;
            resultBox.Text += "NEW: " + new1 + Environment.NewLine;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "Text Files|*.txt|All Files|*.*";
            saveFileDialog1.Title = "Save the Captured Packets";
            saveFileDialog1.ShowDialog();

            //Check to see if file name is given
            if(saveFileDialog1.FileName != "")
            {
                System.IO.File.WriteAllText(saveFileDialog1.FileName, txtCapturedData.Text);
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Text Files|*.txt|All Files|*.*";
            openFileDialog1.Title = "Open the Captured Packets";
            openFileDialog1.ShowDialog();

            //Check to see if file name is given
            if (openFileDialog1.FileName != "")
            {
                txtCapturedData.Text = System.IO.File.ReadAllText(openFileDialog1.FileName);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void sendToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (frmSend.instantiations == 0)
            {
                fSend = new frmSend(); //Creates new frmSend
                fSend.Show();
            }
        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void frmCapture_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            chart1.Series["Ethertype"].Points.AddXY("IP", ip);
            chart1.Series["Ethertype"].Points.AddXY("ARP", arp);
            chart1.Series["Ethertype"].Points.AddXY("IPv6", ipv6);
            chart1.Series["Ethertype"].Points.AddXY("IEEE", ieee);
            chart1.Series["Ethertype"].Points.AddXY("RTP", rtp);
            chart1.Series["Ethertype"].Points.AddXY("VLAN", vlan);
            chart1.Series["Ethertype"].Points.AddXY("NEW", new1);

        }
        private void button2_Click(object sender, EventArgs e)
        {
            chart2.Series["Protocol"].Points.AddXY("UDP", udp);
            chart2.Series["Protocol"].Points.AddXY("TCP", tcp);
            chart2.Series["Protocol"].Points.AddXY("ARP", arp);
            chart2.Series["Protocol"].Points.AddXY("ICMPv6", icmpv6);
            chart2.Series["Protocol"].Points.AddXY("IEEE", ieee);
            chart2.Series["Protocol"].Points.AddXY("RTP", rtp);
            chart2.Series["Protocol"].Points.AddXY("VLAN", vlan);
        }
    }
}
