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
using System.Threading;
using System.Runtime.InteropServices;
using System.Configuration;

namespace WindowsFormsApp1
{
    static class Program
    {
        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static HttpListener listener;

        public static int requestsCount = 0;
        public static DateTime lastRequestDateTime;
        public static int controllerErrors = 0;

        public static String converterAddress = "";

        public static Form1 form1;

        public static ZApi ZApi = new ZApi();

        public static void ProcessRequest(HttpListenerContext context)
        {
            try
            {
                Stream output;
                byte[] b;
                String responseBody = "";
                StatusResponse statusResponse = new StatusResponse();
                if (!(context.Request.HttpMethod.Equals("GET") && context.Request.RawUrl.Equals("/ping/") || context.Request.HttpMethod.Equals("POST")))
                {
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
                    Program.ZApi.init(converterAddress);
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
                            getEventsResponse.items = Program.ZApi.getEvents(getEventsRequest.serialNumber, getEventsRequest.eventIndex, getEventsRequest.eventCount);
                            responseBody = JsonConvert.SerializeObject(getEventsResponse);
                            break;
                        case "/controller/getUnreadEvents/":
                            Program.requestsCount++;
                            Program.lastRequestDateTime = DateTime.Now;
                            GetUnreadEventsRequest getUnreadEventsRequest = JsonConvert.DeserializeObject<GetUnreadEventsRequest>(body);
                            GetUnreadEventsResponse getUnreadEventsResponse = new GetUnreadEventsResponse();
                            GetUnreadEventsResult getUnreadEventsResult = Program.ZApi.getUnreadEvents(getUnreadEventsRequest.serialNumber, getUnreadEventsRequest.lastReadIndex, getUnreadEventsRequest.lastReadMonth, getUnreadEventsRequest.lastReadDay, getUnreadEventsRequest.lastReadHour, getUnreadEventsRequest.lastReadMinute, getUnreadEventsRequest.lastReadSecond, getUnreadEventsRequest.maxEvents);
                            getUnreadEventsResponse.items = getUnreadEventsResult.items;
                            getUnreadEventsResponse.lastReadIndex = getUnreadEventsResult.lastReadIndex;
                            getUnreadEventsResponse.lastReadMonth = getUnreadEventsResult.lastReadMonth;
                            getUnreadEventsResponse.lastReadDay = getUnreadEventsResult.lastReadDay;
                            getUnreadEventsResponse.lastReadHour = getUnreadEventsResult.lastReadHour;
                            getUnreadEventsResponse.lastReadMinute = getUnreadEventsResult.lastReadMinute;
                            getUnreadEventsResponse.lastReadSecond = getUnreadEventsResult.lastReadSecond;
                            responseBody = JsonConvert.SerializeObject(getUnreadEventsResponse);
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
                            if (addKeyRequest.code == null || addKeyRequest.code.Trim().Equals("") || !addKeyRequest.code.Contains(","))
                            {
                                throw new DataErrorException("Поле code должно содержать код ключа в формате ###,#####");
                            }
                            Program.ZApi.addKey(addKeyRequest.serialNumber, addKeyRequest.keyIndex, addKeyRequest.code);
                            responseBody = JsonConvert.SerializeObject(statusResponse);
                            break;
                        case "/controller/clearKey/":
                            Program.requestsCount++;
                            Program.lastRequestDateTime = DateTime.Now;
                            ClearKeyRequest clearKeyRequest = JsonConvert.DeserializeObject<ClearKeyRequest>(body);
                            Program.ZApi.clearKey(clearKeyRequest.serialNumber, clearKeyRequest.keyIndex);
                            responseBody = JsonConvert.SerializeObject(statusResponse);
                            break;
                        case "/controller/setMode/":
                            Program.requestsCount++;
                            Program.lastRequestDateTime = DateTime.Now;
                            SetModeRequest setModeRequest = JsonConvert.DeserializeObject<SetModeRequest>(body);
                            Program.ZApi.setControllerMode(setModeRequest.serialNumber, setModeRequest.mode);
                            responseBody = JsonConvert.SerializeObject(statusResponse);
                            break;
                        case "/controller/setDateTime/":
                            Program.requestsCount++;
                            Program.lastRequestDateTime = DateTime.Now;
                            SetDateTimeRequest setDateTimeRequest = JsonConvert.DeserializeObject<SetDateTimeRequest>(body);
                            Program.ZApi.setControllerDateTime(setDateTimeRequest.serialNumber, setDateTimeRequest.year, setDateTimeRequest.month, setDateTimeRequest.day, setDateTimeRequest.hour, setDateTimeRequest.minute, setDateTimeRequest.second);
                            responseBody = JsonConvert.SerializeObject(statusResponse);
                            break;
                        case "/controller/getDateTime/":
                            Program.requestsCount++;
                            Program.lastRequestDateTime = DateTime.Now;
                            GetDateTimeRequest getDateTimeRequest = JsonConvert.DeserializeObject<GetDateTimeRequest>(body);
                            ControllerDateTime controllerDateTime = Program.ZApi.getControllerDateTime(getDateTimeRequest.serialNumber);
                            responseBody = JsonConvert.SerializeObject(controllerDateTime);
                            break;
                    }
                }
                catch (ZCommonException e)
                {
                    Program.controllerErrors++;
                    responseBody = JsonConvert.SerializeObject(new DataError().setApiErrorType("CONTROLLER_ERROR").setErrorString(e.Message + " (0x" + e.getErrorCode().ToString("X") + ")"));
                }
                catch (DataErrorException e)
                {
                    responseBody = JsonConvert.SerializeObject(new DataError().setApiErrorType("DATA_ERROR").setErrorString(e.Message));
                }
                catch (RequestErrorException e)
                {
                    responseBody = JsonConvert.SerializeObject(new DataError().setApiErrorType("REQUEST_ERROR").setErrorString(e.Message));
                }
                catch (Exception e) {
                    responseBody = JsonConvert.SerializeObject(new DataError().setApiErrorType("INTERNAL_ERROR").setErrorString(e.Message));
                    throw e;
                }
                finally
                {
                    b = Encoding.UTF8.GetBytes(responseBody);
                    try
                    {
                        context.Response.StatusCode = 200;
                        context.Response.KeepAlive = false;
                        context.Response.ContentLength64 = b.Length;
                        output.Write(b, 0, b.Length);
                        context.Response.Close();
                    }
                    catch (HttpListenerException e) { }
                    Program.ZApi.close();
                }
            }
            catch (Exception e) {
                log.Fatal("FATAL", e);
            }
        }

        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            
            converterAddress = Properties.Settings.Default.converterAddress.Trim();
            
            if (converterAddress.Length == 0) {
                log.Fatal("В настройках не указан адрес конвертера");
                return;
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }

    public class StatusResponse {
        public int status = 1;
    }

    public class SetModeRequest {
        public ushort serialNumber;
        public ZGuard.ZG_CTR_MODE mode;
    }

    public class SetDateTimeRequest
    {
        public ushort serialNumber;
        public ushort year;
        public ushort month;
        public ushort day;
        public ushort hour;
        public ushort minute;
        public ushort second;
    }

    public class GetDateTimeRequest
    {
        public ushort serialNumber;
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
        public String code;
    }

    public class GetEventsRequest {
        public ushort serialNumber;
        public int eventIndex;
        public int eventCount;
    }

    public class GetEventsResponse
    {
        public List<ControllerEvent> items;
    }

    public class GetUnreadEventsRequest
    {
        public ushort serialNumber;
        public int lastReadIndex;
        public int lastReadMonth;
        public int lastReadDay;
        public int lastReadHour;
        public int lastReadMinute;
        public int lastReadSecond;
        public int maxEvents;
    }

    public class GetUnreadEventsResponse
    {
        public List<ControllerEvent> items;
        public int lastReadIndex;
        public int lastReadMonth;
        public int lastReadDay;
        public int lastReadHour;
        public int lastReadMinute;
        public int lastReadSecond;
    }

    public class GetControllersResponse {
        public List<ControllerInfoShort> items;
    }
}