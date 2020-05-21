﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyPacketCapturer
{
    public partial class frmSend : Form
    {
        public static int instantiations = 0;
        public frmSend()
        {
            InitializeComponent();
            instantiations++;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Text Files|*.txt|All Files|*.*";
            openFileDialog1.Title = "Open the Captured Packets";
            openFileDialog1.ShowDialog();

            //Check to see if file name is given
            if (openFileDialog1.FileName != "")
            {
                txtPacket.Text = System.IO.File.ReadAllText(openFileDialog1.FileName);
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "Text Files|*.txt|All Files|*.*";
            saveFileDialog1.Title = "Save the Captured Packets";
            saveFileDialog1.ShowDialog();

            //Check to see if file name is given
            if (saveFileDialog1.FileName != "")
            {
                System.IO.File.WriteAllText(saveFileDialog1.FileName, txtPacket.Text);
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            string stringBytes = "";
            //Get hex values from the file
            foreach (string s in txtPacket.Lines){
                //Taking out comments
                string[] noComments = s.Split('#');
                string s1 = noComments[0];
                stringBytes += s1 + Environment.NewLine;
            }
            //Extract hex values into string array
            string[] sBytes = stringBytes.Split(new string[] { "\n", "\r\n", " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);

            //Change the strings into bytes
            byte[] packet = new byte[sBytes.Length];
            int i = 0;
            foreach(string s in sBytes) { packet[i] = Convert.ToByte(s, 16); i++;}
            //Sending out the packet
            try
            {
                frmCapture.device.SendPacket(packet);
            }
            catch(Exception exp) { }
        } //End btnSend

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void frmSend_FormClosed(object sender, FormClosedEventArgs e)
        {
            instantiations--;
        }

        private void frmSend_Load(object sender, EventArgs e)
        {

        }
    }
}
