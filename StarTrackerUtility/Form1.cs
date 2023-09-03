using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StarTrackerUtility
{
    public partial class Form1 : Form
    {
        string selected_serial = "";
        SerialPort serial = new SerialPort();

        double currentX = 0.0;
        double currentY = 0.0;

        double[] stepSizes = { 0.01, 0.1, 0.2, 0.5, 1, 5, 10, 15 };
        double stepSize = 0.1;

        double Xdir = 1.0;

        void UpdateSerial()
        {
            string[] ports = SerialPort.GetPortNames();
            comboBoxSerial.Items.Clear();
            comboBoxSerial.Items.AddRange(ports);
        }

        void sendSerial(string text, string author = "HOST")
        {
            try
            {
                serial.WriteLine(text);
                WriteLog(text, author);
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
        }

        void UpdateTarget(double feedrate = 0.25)
        {
            sendSerial("G1 X" + currentX + " Y" + currentY + " F" + feedrate, "HOST");
        }

        void TickTarget(double duration)
        {
            double increment = 0.25 / 60 * (TimeSpan.FromSeconds(duration)).Ticks / TimeSpan.TicksPerSecond;
            currentX += increment * 100 * Xdir;
            UpdateTarget(25);
        }

        void TickTarget()
        {
            double increment = 0.25 / 60 * (DateTime.Now - lastUpdate).Ticks / TimeSpan.TicksPerSecond;
            currentX += increment * 100 * Xdir;
            UpdateTarget(25);
        }

        void WriteLog(string text, string author = "System")
        {
            richTextBox1.Invoke(new MethodInvoker(delegate { richTextBox1.AppendText("\n" + author + ": " + text); }));
        }

        public Form1()
        {
            InitializeComponent();
            UpdateSerial();
            if (comboBoxSerial.Items.Count != 0) comboBoxSerial.SelectedIndex = 0;
        }

        private void buttonYP_Click(object sender, EventArgs e)
        {
            currentY += stepSize;
            UpdateTarget(360);
        }

        private void buttonXP_Click(object sender, EventArgs e)
        {
            currentX += stepSize;
            UpdateTarget(360);
        }

        private void buttonYN_Click(object sender, EventArgs e)
        {
            currentY -= stepSize;
            UpdateTarget(360);
        }

        private void buttonXN_Click(object sender, EventArgs e)
        {
            currentX -= stepSize;
            UpdateTarget(360);
        }

        private void buttonEnable_Click(object sender, EventArgs e)
        {
            lastUpdate = DateTime.Now;
            TickTarget(1.0);
            Thread.Sleep(1);
            TrackerTimer.Start();
        }

        private void buttonDisable_Click(object sender, EventArgs e)
        {
            lastUpdate = DateTime.Now;
            TrackerTimer.Stop();
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            sendSerial("G92 X0 Y0");
            currentX = 0;
            currentY = 0;
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            if(serial.IsOpen)
            {
                sendSerial(textBox1.Text, "USER");
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            try
            {
                serial = new SerialPort(selected_serial, 115200);
                serial.RtsEnable = true;
                serial.DtrEnable = true;
                serial.NewLine = "\n";
                serial.Open();
                if (serial.IsOpen)
                {
                    WriteLog("Opened serial port " + selected_serial);
                    serial.WriteLine("M115");
                    serial.DataReceived += new SerialDataReceivedEventHandler(Serial_Data_Received);
                }
            }
            catch (Exception ex)
            {
                WriteLog("Failed opening serial port: " + ex.Message);
            }
        }

        private void comboBoxSerial_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSerial();
            selected_serial = comboBoxSerial.Text;
        }

        void Serial_Data_Received(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort port = ((SerialPort)sender);
            while (port.BytesToRead != 0)
            {
                WriteLog(port.ReadLine(), ((SerialPort)sender).PortName);
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            stepSize = stepSizes[trackBar1.Value];
            label1.Text = "Step Size: " + stepSize.ToString() + "*";
        }

        DateTime lastUpdate = DateTime.Now;
        private void TrackerTimer_Tick(object sender, EventArgs e)
        {
            TickTarget();
            lastUpdate = DateTime.Now;
        }

        private void checkBoxReverse_CheckedChanged(object sender, EventArgs e)
        {
            Xdir = checkBoxReverse.Checked ? -1.0 : 1.0;
        }
    }
}
