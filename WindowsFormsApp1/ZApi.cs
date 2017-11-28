using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZGuard;
using ZPort;

namespace Z
{
    public class ZApi
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private ZP_PORT_TYPE PortType = ZP_PORT_TYPE.ZP_PORT_IP;
        private String Address = "192.168.101.12:7000";
        public IntPtr ConverterHandler = IntPtr.Zero;

        public void init() {
            if (!ConverterHandler.Equals(IntPtr.Zero)) {
                return;
            }
            int hr;
            hr = ZGIntf.ZG_Initialize(ZPIntf.ZP_IF_NO_MSG_LOOP);
            if (hr < 0)
            {
                log.Fatal("Ошибка инициализации ZG_Initialize: " + hr);
                throw new ZConnectionException("Ошибка инициализации ZG_Initialize: " + hr);
            }
            ZG_CVT_INFO ConverterInfo = new ZG_CVT_INFO();
            ZG_CVT_OPEN_PARAMS OpenParams = new ZG_CVT_OPEN_PARAMS();
            OpenParams.nPortType = PortType;
            OpenParams.pszName = @Address;
            OpenParams.nSpeed = ZG_CVT_SPEED.ZG_SPEED_57600;

            hr = ZGIntf.ZG_Cvt_Open(ref ConverterHandler, ref OpenParams, ConverterInfo);
            if (hr < 0)
            {
                log.Fatal("Ошибка подключения к конвертеру: " + hr);
                throw new ZConnectionException("Ошибка подключения к конвертеру: " + hr);
            }
        }

        public List<ControllerInfoShort> GetControllers() {
            List<ControllerInfoShort> controllers = new List<ControllerInfoShort>();
            return controllers;
        }
    }

    public class ControllerInfoShort {

    }

    public class ZConnectionException : Exception {
        public ZConnectionException() : base() { }
        public ZConnectionException(string message) : base(message) { }
    }
}
