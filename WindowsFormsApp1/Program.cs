using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using log4net;
using log4net.Config;
using System.Net;
using System.Text;
using System.IO;
using Z;

namespace WindowsFormsApp1
{
    static class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static HttpListener listener;

        public static ZApi ZApi = new ZApi();

        public static void ProcessRequest(HttpListenerContext context)
        {
            Stream output;
            byte[] b;
            String responseBody = "";
            log.Info("M: " + context.Request.HttpMethod.Equals("GET") + ", " + context.Request.RawUrl.Equals("/ping/"));
            if (!(context.Request.HttpMethod.Equals("GET") && context.Request.RawUrl.Equals("/ping/") || context.Request.HttpMethod.Equals("POST"))) {
                output = context.Response.OutputStream;
                b = Encoding.UTF8.GetBytes(responseBody);
                context.Response.StatusCode = 405;
                context.Response.KeepAlive = false;
                context.Response.ContentLength64 = b.Length;
                output.Write(b, 0, b.Length);
                context.Response.Close();
                return;
            }
            String body = new StreamReader(context.Request.InputStream).ReadToEnd();
            output = context.Response.OutputStream;
            try
            {
                switch (context.Request.RawUrl)
                {
                    case "/ping/":
                        responseBody = "Pong";
                        break;
                }
            }
            finally {
                b = Encoding.UTF8.GetBytes(responseBody);
                context.Response.StatusCode = 200;
                context.Response.KeepAlive = false;
                context.Response.ContentLength64 = b.Length;
                output.Write(b, 0, b.Length);
                context.Response.Close();
            }
        }

        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //XmlConfigurator.Configure(new System.IO.FileInfo("log4net.xml"));
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
