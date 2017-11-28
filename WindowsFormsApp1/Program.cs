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
        public static HttpListener listener;

        public static ZApi ZApi = new ZApi();

        public static void ProcessRequest(HttpListenerContext context)
        {
            // Get the data from the HTTP stream
            var body = new StreamReader(context.Request.InputStream).ReadToEnd();

            Console.WriteLine("Request: " + body);

            byte[] b = Encoding.UTF8.GetBytes("Response");
            context.Response.StatusCode = 200;
            context.Response.KeepAlive = false;
            context.Response.ContentLength64 = b.Length;

            var output = context.Response.OutputStream;
            output.Write(b, 0, b.Length);
            context.Response.Close();
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
