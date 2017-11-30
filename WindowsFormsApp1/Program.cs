﻿using System;
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
using Errors;
using Newtonsoft.Json;

namespace WindowsFormsApp1
{
    static class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static HttpListener listener;

        public static int requestsCount = 0;
        public static DateTime lastRequestDateTime;
        public static int controllerErrors = 0;

        public static Form1 form1;

        public static ZApi ZApi = new ZApi();

        public static void ProcessRequest(HttpListenerContext context)
        {
            Stream output;
            byte[] b;
            String responseBody = "";
            StatusResponse statusResponse = new StatusResponse();
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
                Program.ZApi.init();
                switch (context.Request.RawUrl)
                {
                    case "/ping/":
                        responseBody = "Pong";
                        break;
                    case "/converter/getControllers/":
                        Program.requestsCount++;
                        Program.lastRequestDateTime = DateTime.Now;
                        GetControllersResponse getControllersResponse = new GetControllersResponse();
                        getControllersResponse.items = Program.ZApi.GetControllers();
                        responseBody = JsonConvert.SerializeObject(getControllersResponse);
                        break;
                    case "/controller/getEvents/":
                        Program.requestsCount++;
                        Program.lastRequestDateTime = DateTime.Now;
                        GetEventsRequest getEventsRequest = JsonConvert.DeserializeObject<GetEventsRequest>(body);
                        GetEventsResponse getEventsResponse = new GetEventsResponse();
                        getEventsResponse.items = Program.ZApi.getEvents(getEventsRequest.serialNumber);
                        responseBody = JsonConvert.SerializeObject(getEventsResponse);
                        break;
                    case "/controller/getKeys/":
                        Program.requestsCount++;
                        Program.lastRequestDateTime = DateTime.Now;
                        GetKeysRequest getKeysRequest = JsonConvert.DeserializeObject<GetKeysRequest>(body);
                        GetKeysResponse getKeysResponse = new GetKeysResponse();
                        getKeysResponse.items = Program.ZApi.getKeys(getKeysRequest.serialNumber, getKeysRequest.keyIndex, getKeysRequest.keyCount);
                        responseBody = JsonConvert.SerializeObject(getKeysResponse);
                        break;
                    case "/controller/addKey/":
                        Program.requestsCount++;
                        Program.lastRequestDateTime = DateTime.Now;
                        AddKeyRequest addKeyRequest = JsonConvert.DeserializeObject<AddKeyRequest>(body);
                        if (addKeyRequest.name == null || addKeyRequest.name.Trim().Equals("") || !addKeyRequest.name.Contains(",")) {
                            throw new DataErrorException("Поле name должно содержать имя ключа в формате ###,#####");
                        }
                        Program.ZApi.addKey(addKeyRequest.serialNumber, addKeyRequest.keyIndex, addKeyRequest.name);
                        responseBody = JsonConvert.SerializeObject(statusResponse);
                        break;
                    case "/controller/clearKey/":
                        Program.requestsCount++;
                        Program.lastRequestDateTime = DateTime.Now;
                        ClearKeyRequest clearKeyRequest = JsonConvert.DeserializeObject<ClearKeyRequest>(body);
                        Program.ZApi.clearKey(clearKeyRequest.serialNumber, clearKeyRequest.keyIndex);
                        responseBody = JsonConvert.SerializeObject(statusResponse);
                        break;
                }
            }
            catch (ZCommonException e) {
                Program.controllerErrors++;
                responseBody = JsonConvert.SerializeObject(new DataError().setApiErrorType("CONTROLLER_ERROR").setErrorString(e.Message + " (" + e.getErrorCode() + ")"));
            }
            catch (DataErrorException e)
            {
                responseBody = JsonConvert.SerializeObject(new DataError().setApiErrorType("DATA_ERROR").setErrorString(e.Message));
            }
            catch (RequestErrorException e)
            {
                responseBody = JsonConvert.SerializeObject(new DataError().setApiErrorType("REQUEST_ERROR").setErrorString(e.Message));
            }
            finally
            {
                b = Encoding.UTF8.GetBytes(responseBody);
                context.Response.StatusCode = 200;
                context.Response.KeepAlive = false;
                context.Response.ContentLength64 = b.Length;
                output.Write(b, 0, b.Length);
                context.Response.Close();
                Program.ZApi.close();
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

    public class StatusResponse {
        public int status = 1;
    }

    public class ClearKeyRequest
    {
        public ushort serialNumber;
        public int keyIndex;
    }

    public class GetKeysRequest {
        public ushort serialNumber;
        public int keyIndex;
        public int keyCount;
    }

    public class GetKeysResponse
    {
        public List<ControllerKey> items;
    }

    public class AddKeyRequest
    {
        public ushort serialNumber;
        public int keyIndex;
        public String name;
    }

    public class GetEventsRequest {
        public ushort serialNumber;
    }

    public class GetControllersResponse {
        public List<ControllerInfoShort> items;
    }

    public class GetEventsResponse {
        public List<ControllerEvent> items;
    }
}
