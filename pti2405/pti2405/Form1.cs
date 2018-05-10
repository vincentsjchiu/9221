using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Linq;
namespace pti2405
{
    public partial class Form1 : Form
    {
        string[] filenames;
        string timeformat;
        uint timefactor;
        FileInfo[] files;
        DirectoryInfo dir;
        device_config daqcontrol = new device_config();
        uint m_dwOverrunCnt = 0, timetofileindex = 0;
        CallbackDelegate ai_buf_ready_cbdel;
        public FileStream outputch;
        public StreamWriter txtWtr;
        public string[] path;
        DateTime Time, Timetemp, Timetofile;
        Thread check;
        int aliveindex = 0, aliveindextemp1 = -1, aliveindextemp2 = -1, aliveindextemp3 = -1;
        bool start = false;
        string sysname;
        public class device_config
        {
            public short result;
            public ushort Chconfig;
            public double samplerate;
            public uint numbchans;
            public ushort[] ai_chans;
            public ushort[] ai_chans_range;
            public IntPtr airowdata;
            public uint allchanlength;
            public uint perchanlength;
            public uint resolution;
            public uint AccessCnt;
            public double[] aivoltagedata;
            public double[] tempvoltagedata;
            public double[] ch0data;
            public double[] ch1data;
            public double[] ch2data;
            public double[] ch3data;
            public double sens0;
            public double sens1;
            public double sens2;
            public double sens3;
        }
        public Form1()
        {
            InitializeComponent();
            check = new Thread(checkcallback);
            ai_buf_ready_cbdel = new CallbackDelegate(ai_buf_ready_cbfunc);
            comboBoxchan.SelectedIndex = 0;
            comboBoxsamplerate.SelectedIndex = 0;
            CreateIfFolderMissing("c:\\Setting");
            bool filecheck = File.Exists("c:\\Setting\\Setting.txt");
            if (!filecheck)
            {
                txtWtr = new StreamWriter("c:\\Setting\\Setting.txt", false);
                txtWtr.WriteLine("c:\\Data\\");
                txtWtr.WriteLine("c:\\Data\\DataTemp\\");
                txtWtr.Close();
            }

            buttonstart.Enabled = true;
            buttonstop.Enabled = false;
            comboBoxchan.Enabled = true;
            comboBoxsamplerate.Enabled = true;
            path = File.ReadAllLines(@"c:\Setting\Setting.txt", Encoding.UTF8);
            CreateIfFolderMissing(path[0]);
            CreateIfFolderMissing(path[1]);
            dir = new DirectoryInfo(path[0]);
            files = dir.GetFiles("*", SearchOption.TopDirectoryOnly);
            filenames = files.Select(f => f.Name).ToArray();

            for (int i = 0; i < filenames.Length; i++)
            {
                File.Move(path[0] + filenames[i], path[1] + filenames[i]);
            }
            sysname = System.Environment.MachineName;
        }
        private void CreateIfFolderMissing(string path)
        {
            bool folderExists = Directory.Exists(path);
            if (!folderExists)
                Directory.CreateDirectory(path);
        }

        private void UpateDeviceConfig()
        {
            daqcontrol.sens0 = 100;
            daqcontrol.sens1 = 100;
            daqcontrol.sens2 = 100;
            daqcontrol.sens3 = 100;
            daqcontrol.numbchans = 0;
            daqcontrol.Chconfig = USBDASK.P2405_AI_Differential | USBDASK.P2405_AI_Coupling_AC | USBDASK.P2405_AI_EnableIEPE;

            if (comboBoxchan.SelectedIndex == 0)
            {
                daqcontrol.numbchans = 1;
            }
            else if (comboBoxchan.SelectedIndex == 1)
            {
                daqcontrol.numbchans = 2;
            }
            else if (comboBoxchan.SelectedIndex == 2)
            {
                daqcontrol.numbchans = 3;
            }
            else
            {
                daqcontrol.numbchans = 4;
            }
            if (comboBoxsamplerate.SelectedIndex == 0)
            {
                daqcontrol.samplerate = 1000;
                daqcontrol.perchanlength = 1024;
                timeformat = "yyyy-MM-dd HH:mm:ss.fff";
                timefactor = 10000;
            }
            else if (comboBoxsamplerate.SelectedIndex == 1)
            {
                daqcontrol.samplerate = 10000;
                daqcontrol.perchanlength = 10240;
                timeformat = "yyyy-MM-dd HH:mm:ss.ffff";
                timefactor = 1000;
            }

            else
            {
                daqcontrol.samplerate = 20000;
                daqcontrol.perchanlength = 20480;
                timeformat = "yyyy-MM-dd HH:mm:ss.fffff";
                timefactor = 500;
            }
            daqcontrol.ai_chans = new ushort[daqcontrol.numbchans];
            daqcontrol.ai_chans_range = new ushort[daqcontrol.numbchans];
            for (int i = 0; i < daqcontrol.numbchans; i++)
            {
                daqcontrol.ai_chans[i] = (ushort)i;
                daqcontrol.ai_chans_range[i] = USBDASK.AD_B_10_V;
            }

            //daqcontrol.perchanlength = (uint)(daqcontrol.samplerate);
            daqcontrol.allchanlength = daqcontrol.perchanlength * daqcontrol.numbchans;
            daqcontrol.airowdata = Marshal.AllocHGlobal((int)(sizeof(uint) * daqcontrol.allchanlength));
            daqcontrol.aivoltagedata = new double[daqcontrol.allchanlength];
            daqcontrol.tempvoltagedata = new double[daqcontrol.allchanlength];
            daqcontrol.ch0data = new double[daqcontrol.perchanlength];
            daqcontrol.ch1data = new double[daqcontrol.perchanlength];
            daqcontrol.ch2data = new double[daqcontrol.perchanlength];
            daqcontrol.ch3data = new double[daqcontrol.perchanlength];
        }

        private void buttonstart_Click(object sender, EventArgs e)
        {
            daqcontrol.result = USBDASK.UD_Register_Card(USBDASK.USB_2405, 0);
            if (daqcontrol.result < 0)
            {

                this.Cursor = Cursors.Default;
                MessageBox.Show("Please Connecnt USB-2405 To your PC");
                return;
            }
            buttonstart.Enabled = false;
            buttonstop.Enabled = true;
            comboBoxchan.Enabled = false;
            comboBoxsamplerate.Enabled = false;
            UpateDeviceConfig();
            Configuredaq();
            start = true;
            check = new Thread(checkcallback);
            check.Start();
            // txtWtr = new StreamWriter(path[1], true);
        }

        private void buttonstop_Click(object sender, EventArgs e)
        {
            buttonstop.Enabled = false;
            buttonstart.Enabled = true;
            comboBoxchan.Enabled = true;
            comboBoxsamplerate.Enabled = true;
            start = false;
            check.Abort();
            daqcontrol.result = USBDASK.UD_AI_AsyncClear(0, out daqcontrol.AccessCnt);
            if (daqcontrol.result < 0)
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show("Falied to perform UD_AI_AsyncClear(), error: " + daqcontrol.result, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            daqcontrol.result = USBDASK.UD_Release_Card(0);
            if (daqcontrol.result < 0)
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show("Falied to perform UD_Release_Card(), error: " + daqcontrol.result, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            timetofileindex = 0;
            files = dir.GetFiles("*", SearchOption.TopDirectoryOnly);
            filenames = files.Select(f => f.Name).ToArray();

            for (int i = 0; i < filenames.Length; i++)
            {
                File.Move(path[0] + filenames[i], path[1] + filenames[i]);
            }
            //File.Move(path[0] + Timetofile.ToString("yyyyMMddHHmmss") + ".txt", path[1] + Timetofile.ToString("yyyyMMddHHmmss") + ".txt");
            // txtWtr.Close();
        }
        void ai_buf_ready_cbfunc()
        {
            aliveindex++;
            daqcontrol.result = USBDASK.UD_AI_AsyncDblBufferTransfer32(0, daqcontrol.airowdata);
            if (daqcontrol.result != USBDASK.NoError)
            {
                //this.Cursor = Cursors.Default;
                MessageBox.Show("Falied to perform UD_AI_AsyncDblBufferTransfer32s(), error: " + daqcontrol.result, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }

            daqcontrol.result = USBDASK.UD_AI_ContVScale32(0, USBDASK.AD_B_10_V, 0/*inType*/, daqcontrol.airowdata, daqcontrol.aivoltagedata, (int)(daqcontrol.allchanlength));
            if (daqcontrol.result != USBDASK.NoError)
            {
                //this.Cursor = Cursors.Default;
                MessageBox.Show("Falied to perform UD_AI_ContVScale32(), error: " + daqcontrol.result, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }
            WriteChannelData();

            MethodInvoker mi = new MethodInvoker(this.UpdateUI);

            ushort OverrunFlag;

            USBDASK.UD_AI_AsyncDblBufferHandled(0);
            USBDASK.UD_AI_AsyncDblBufferOverrun(0, 0, out OverrunFlag);

            if (OverrunFlag == 1)
            {
                m_dwOverrunCnt = m_dwOverrunCnt + 1;
                USBDASK.UD_AI_AsyncDblBufferOverrun(0, 1, out OverrunFlag);
                this.BeginInvoke(mi, null);
            }
            if (aliveindex == 10)
                aliveindex = 0;
            this.BeginInvoke(mi, null);
        }
        private void Configuredaq()
        {


            daqcontrol.result = USBDASK.UD_AI_2405_Chan_Config(0, daqcontrol.Chconfig, daqcontrol.Chconfig, daqcontrol.Chconfig, daqcontrol.Chconfig);
            if (daqcontrol.result != USBDASK.NoError)
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show("Falied to perform UD_AI_2405_Chan_Config(), error: " + daqcontrol.result, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }
            daqcontrol.result = USBDASK.UD_AI_2405_Trig_Config(0, USBDASK.P2405_AI_CONVSRC_INT, USBDASK.UD_AI_TRGMOD_POST, 0, 0, 0, 0, 0);
            if (daqcontrol.result != USBDASK.NoError)
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show("Falied to perform UD_AI_2405_Trig_Config(), error: " + daqcontrol.result, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            daqcontrol.result = USBDASK.UD_AI_AsyncDblBufferMode(0, true);
            if (daqcontrol.result != USBDASK.NoError)
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show("Falied to perform UD_AI_AsyncDblBufferMode(), error: " + daqcontrol.result, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }
            daqcontrol.result = USBDASK.UD_AI_EventCallBack(0, 1/*add*/, USBDASK.DBEvent/*EventType*/, ai_buf_ready_cbdel);
            if (daqcontrol.result != USBDASK.NoError)
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show("Falied to perform UD_AI_EventCallBack(), error: " + daqcontrol.result, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }
            daqcontrol.result = USBDASK.UD_AI_ContReadMultiChannels(0, (ushort)daqcontrol.numbchans, daqcontrol.ai_chans, daqcontrol.ai_chans_range, daqcontrol.airowdata, daqcontrol.allchanlength * 2, daqcontrol.samplerate, USBDASK.ASYNCH_OP);
            if (daqcontrol.result != USBDASK.NoError)
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show("Falied to perform UD_AI_ContReadMultiChannels(), error: " + daqcontrol.result, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }
            Time = DateTime.Now;
            Timetofile = Time;


        }
        private void WriteChannelData()
        {
            try
            {
                int tick = 0;
                timetofileindex++;
                if (timetofileindex == 60)
                {
                    File.Move(path[0] + sysname + "_" + Timetofile.ToString("yyyyMMddHHmmss") + ".txt", path[1] + sysname + "_" + Timetofile.ToString("yyyyMMddHHmmss") + ".txt");
                    Timetofile = Time;
                    timetofileindex = 0;

                }
                txtWtr = new StreamWriter(path[0] + sysname + "_" + Timetofile.ToString("yyyyMMddHHmmss") + ".txt", true);
                //Time = DateTime.Now;
                for (int j = 0; j < (int)(daqcontrol.allchanlength); j++)
                {

                    if ((j % daqcontrol.numbchans) == 0)
                    {
                        tick++;

                        //txtWtr.WriteLine(Time.AddMilliseconds(millisec).ToString("yyyy-MM-dd HH:mm:ss.ffffff") + ",CH0," + (daqcontrol.aivoltagedata[j]) / (0.001 * daqcontrol.sens0));
                        txtWtr.WriteLine(Time.AddTicks(tick * timefactor).ToString(timeformat) + ",CH0," + (daqcontrol.aivoltagedata[j]) / (0.001 * daqcontrol.sens0));

                    }

                    else if ((j % daqcontrol.numbchans) == 1)
                    {
                        // txtWtr.WriteLine(Time.AddMilliseconds(millisec).ToString("yyyy-MM-dd HH:mm:ss.fff") + ",CH1," + (daqcontrol.aivoltagedata[j]) / (0.001 * daqcontrol.sens1));
                        txtWtr.WriteLine(Time.AddTicks(tick * timefactor).ToString(timeformat) + ",CH1," + (daqcontrol.aivoltagedata[j]) / (0.001 * daqcontrol.sens1));
                    }
                    else if ((j % daqcontrol.numbchans) == 2)
                    {
                        //txtWtr.WriteLine(Time.AddMilliseconds(millisec).ToString("yyyy-MM-dd HH:mm:ss.fff") + ",CH2," + (daqcontrol.aivoltagedata[j]) / (0.001 * daqcontrol.sens2));
                        txtWtr.WriteLine(Time.AddTicks(tick * timefactor).ToString(timeformat) + ",CH2," + (daqcontrol.aivoltagedata[j]) / (0.001 * daqcontrol.sens2));
                    }
                    else
                    {
                        //txtWtr.WriteLine(Time.AddMilliseconds(millisec).ToString("yyyy-MM-dd HH:mm:ss.fff") + ",CH3," + (daqcontrol.aivoltagedata[j]) / (0.001 * daqcontrol.sens3));
                        txtWtr.WriteLine(Time.AddTicks(tick * timefactor).ToString(timeformat) + ",CH3," + (daqcontrol.aivoltagedata[j]) / (0.001 * daqcontrol.sens3));
                    }
                }

                txtWtr.Close();
                Timetemp = Time.AddTicks(tick * timefactor);
                Time = Timetemp;

            }
            catch
            {

            }
        }
        private void UpdateUI()
        {
            textBox1daqtime.Text = Convert.ToString(m_dwOverrunCnt);
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (start)
            {
                daqcontrol.result = USBDASK.UD_AI_AsyncClear(0, out daqcontrol.AccessCnt);
                daqcontrol.result = USBDASK.UD_Release_Card(0);
            }

            if (txtWtr != null)
                txtWtr.Close();
            if (check.IsAlive)
            {
                if (false == check.Join(200))
                {
                    check.Abort();
                }
            }


        }
        private void checkcallback()
        {

            while (start)
            {
                aliveindextemp1 = aliveindex;
                Thread.Sleep(5000);
                aliveindextemp2 = aliveindex;
                Thread.Sleep(1000);
                aliveindextemp3 = aliveindex;
                if (aliveindextemp1 == aliveindextemp2 && aliveindextemp2 == aliveindextemp3)
                {
                    start = false;
                }
            }
            if (aliveindextemp1 == aliveindextemp2 && aliveindextemp2 == aliveindextemp3)
            {
                MessageBox.Show("USB-2405 disconnected ,please connect it again and restart this program");
                //Environment.Exit(1);
                Process.GetCurrentProcess().Kill();
            }
        }
    }
}
