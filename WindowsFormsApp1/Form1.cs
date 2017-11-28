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
            if (Program.listener == null || Program.listener.IsListening)
            {
                log.Fatal("Listener не смог стартовать");
            }
            HttpListenerContext context = null;
            do
            {
                context = Program.listener.GetContext();
                Program.ProcessRequest(context);
            } while (true);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Program.listener = new HttpListener();
            Program.listener.Prefixes.Add("http://*:27099/");
            Program.listener.Start();
            var listenerThread = new Thread(listenerThreadWatcher);
            listenerThread.IsBackground = true;
            listenerThread.Start();
            Console.WriteLine("Start");
            HttpListenerContext context = null;


            Program.ZApi.init();

            //UInt32 nVersion = ZGIntf.ZG_GetVersion();
            //long z = 0;
            //log.Info("Found: " + z);

            /*int hr;
            hr = ZGIntf.ZG_Initialize(ZPIntf.ZP_IF_NO_MSG_LOOP);
            if (hr < 0)
            {
                log.Fatal("Ошибка ZG_Initialize (" + hr + ").");
                return;
            }
            ZG_CVT_INFO rInfo = new ZG_CVT_INFO();
            ZG_CVT_OPEN_PARAMS rOp = new ZG_CVT_OPEN_PARAMS();
            rOp.nPortType = ZP_PORT_TYPE.ZP_PORT_IP;
            rOp.pszName = @"192.168.101.12:7000";
            rOp.nSpeed = ZG_CVT_SPEED.ZG_SPEED_57600;

            hr = ZGIntf.ZG_Cvt_Open(ref m_hCvt, ref rOp, rInfo);
            if (hr < 0)
            {
                log.Fatal("Unable to connect");
                return;
            }
            g_nCtrCount = 0;*/
            int hr = ZGIntf.ZG_Cvt_EnumControllers(Program.ZApi.ConverterHandler, EnumCtrsCB, IntPtr.Zero);
            if (hr < 0)
            {
                log.Fatal("Ошибка ZG_Cvt_EnumControllers (" + hr + ").");
                return;
            }
            if (g_nCtrCount > 0)
            {
                log.Info("Найдено " + g_nCtrCount + " контроллеров.");
            }
            else
            {
                log.Info("Контроллеры не найдены.");
            }




            log.Info("Found");




            Console.WriteLine("End");
            log.Info("Test");
            /*while (listener.IsListening)
            {
                context = listener.GetContext();
                ProcessRequest(context);
            }
            listener.Close();*/
        }
    }
}
