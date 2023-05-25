using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using log4net;
using System.Threading;
using ZGuard;
using ZPort;
using Z;

//using ZGuard;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static IntPtr m_hCvt = IntPtr.Zero;

        //public static readonly ILog log = LogManager.GetLogger(typeof(Program));


        public static readonly string[] CtrTypeStrs = {
                                                          "",
                                                          "Gate 2000",
                                                          "Matrix II Net",
                                                          "Matrix III Net",
                                                          "Z5R Net",
                                                          "Z5R Net 8000",
                                                          "Guard Net",
                                                          "Z-9 EHT Net",
                                                          "EuroLock EHT net",
                                                          "Z5R Web",
                                                          "Matrix II Wi-Fi"
                                                      };
        public static readonly string[] KeyModeStrs = { "Touch Memory", "Proximity" };
        public static int g_nCtrCount;


        public Form1()
        {
            InitializeComponent();
        }

        static bool EnumCtrsCB(ref ZG_FIND_CTR_INFO pInfo, int nPos, int nMax, IntPtr pUserData)
        {
            log.Info(CtrTypeStrs[(int)pInfo.nType] + ", адрес: " + pInfo.nAddr + ", с/н: " + pInfo.nSn + ", кл.: {5}, соб.: " + pInfo.nMaxEvents + ", " + KeyModeStrs[((pInfo.nFlags & ZGIntf.ZG_CTR_F_PROXIMITY) != 0) ? 1 : 0] + ";");
            g_nCtrCount++;
            return true;
        }

        private static void listenerThreadWatcher()
        {
            DateTime start = DateTime.Now;
            //var sw = Stopwatch.StartNew();
            log.Info("listener thread watcher started");
            HttpListenerContext context = null;
            do
            {
                context = Program.listener.GetContext();
                Program.ProcessRequest(context);
            } while (true);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Program.form1 = (Form1)sender;
            Program.listener = new HttpListener();
            Program.listener.Prefixes.Add("http://*:27099/");
            try
            {
                Program.listener.Start();
            }
            catch (HttpListenerException ex)
            {
                Program.form1.labelListening.Text = ex.Message;
                Program.form1.labelListening.ForeColor = System.Drawing.Color.Red;
                log.Fatal("Listener не смог стартовать: " + ex.Message);
                return;
            }
            if (Program.listener == null || !Program.listener.IsListening)
            {
                Program.form1.labelListening.Text = "Не удалось открыть порт";
                Program.form1.labelListening.ForeColor = System.Drawing.Color.Red;
                log.Fatal("Listener не смог стартовать");
                return;
            }

            var listenerThread = new Thread(listenerThreadWatcher);
            listenerThread.IsBackground = true;
            listenerThread.Start();
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            lock (Program.lastRequestDateTimeLocker)
            {
                Program.form1.labelRequestsCount.Text = Program.requestsCount.ToString();
                Program.form1.labelControllerErrors.Text = Program.controllerErrors.ToString();
                if (Program.lastRequestDateTime.Year > 1) {
                    Program.form1.labelLastRequestDateTime.Text = Program.lastRequestDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                }
                if (DateTime.Now.AddSeconds(-7) > Program.lastRequestDateTime) {
                    lock (Program.isActiveRequestLocker)
                    {
                        if (!Program.isActiveRequest)
                        {
                            lock (Program.initLocker)
                            {
                                Program.ZApi.close();
                            }
                        }
                    }
                }
            }
        }
    }
}
