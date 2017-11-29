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
        private List<ControllerInfoShort> tmpControllerInfoShortList;

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

        public void init() {
            if (!ConverterHandler.Equals(IntPtr.Zero)) {
                return;
            }
            int hr;
            hr = ZGIntf.ZG_Initialize(ZPIntf.ZP_IF_NO_MSG_LOOP);
            if (hr < 0)
            {
                log.Fatal("Ошибка инициализации ZG_Initialize: " + hr);
                throw new ZCommonException("Ошибка инициализации ZG_Initialize").setErrorCode(hr);
            }
            ZG_CVT_INFO ConverterInfo = new ZG_CVT_INFO();
            ZG_CVT_OPEN_PARAMS OpenParams = new ZG_CVT_OPEN_PARAMS();
            OpenParams.nPortType = PortType;
            OpenParams.pszName = @"192.168.101.12:7000";
            OpenParams.nSpeed = ZG_CVT_SPEED.ZG_SPEED_57600;

            hr = ZGIntf.ZG_Cvt_Open(ref ConverterHandler, ref OpenParams, ConverterInfo);
            if (hr < 0)
            {
                log.Fatal("Ошибка подключения к конвертеру: " + hr);
                throw new ZCommonException("Ошибка подключения к конвертеру").setErrorCode(hr);
            }
        }

        public static string CardArrayToString(Byte[] aNum)
        {
            string s = String.Format("{0:D3},{1:D5}", aNum[3], aNum[1] + (aNum[2] << 8));
            return s;
        }

        public static byte[] CardStringToArray(String value) {
            byte[] res = new byte[16];
            String[] parts = value.Split(',');
            res[0] = 6;
            res[3] = Byte.Parse(parts[0]);
            int p1 = Int32.Parse(parts[1]);
            int p2 = Int32.Parse(parts[1]);
            p2 = p2 >> 8;
            res[1] = (byte)p1;
            res[2] = (byte)p2;
            return res;
        }

        private bool tmpEnumConverters(ref ZG_FIND_CTR_INFO pInfo, int nPos, int nMax, IntPtr pUserData)
        {
            ControllerInfoShort newControllerInfoShort = new ControllerInfoShort();
            newControllerInfoShort.name = CtrTypeStrs[(int)pInfo.nType];
            newControllerInfoShort.address = pInfo.nAddr;
            newControllerInfoShort.serialNumber = pInfo.nSn;
            newControllerInfoShort.maxEvents = pInfo.nMaxEvents;
            //newControllerInfoShort.
            log.Info(CtrTypeStrs[(int)pInfo.nType] + ", адрес: " + pInfo.nAddr + ", с/н: " + pInfo.nSn + ", кл.: {5}, соб.: " + pInfo.nMaxEvents + ";");
            //g_nCtrCount++;
            tmpControllerInfoShortList.Add(newControllerInfoShort);
            return true;
        }

        public List<ControllerInfoShort> GetControllers() {
            tmpControllerInfoShortList = new List<ControllerInfoShort>();
            int hr = ZGIntf.ZG_Cvt_EnumControllers(ConverterHandler, tmpEnumConverters, IntPtr.Zero);
            if (hr < 0)
            {
                log.Fatal("Ошибка ZG_Cvt_EnumControllers (" + hr + ").");
                throw new ZCommonException("Ошибка ZG_Cvt_EnumControllers").setErrorCode(hr);
            }
            return tmpControllerInfoShortList;
        }

        public ControllerKey[] getKeys(ushort serialNumber) {
            IntPtr ControllerHandler = new IntPtr(0);
            ZG_CTR_INFO ControllerInfo = new ZG_CTR_INFO();
            ZG_CTR_KEY[] aKeys = new ZG_CTR_KEY[2024];
            ControllerKey[] keyMap = new ControllerKey[2024];
            try
            {
                //Открываем контроллер
                int hr = ZGIntf.ZG_Ctr_Open(ref ControllerHandler, ConverterHandler, 255, serialNumber, ref ControllerInfo);
                bool m_fProximity = true;// ((ControllerInfo.nFlags & ZGIntf.ZG_CTR_F_PROXIMITY) != 0);
                log.Info("Proximity: " + m_fProximity);
                if (hr < 0)
                {
                    log.Fatal("Ошибка ZG_Ctr_Open (" + hr + ")");
                    throw new ZCommonException("Ошибка ZG_Ctr_Open").setErrorCode(hr);
                }
                //Читаем список карт
                hr = ZGIntf.ZG_Ctr_ReadKeys(ControllerHandler, 0, aKeys, 2024, null, IntPtr.Zero, 0);
                if (hr < 0) {
                    log.Fatal("Ошибка ZG_Ctr_ReadKeys (" + hr + ")");
                    throw new ZCommonException("Ошибка ZG_Ctr_ReadKeys").setErrorCode(hr);
                }
                for (int i = 0; i < 2024; i++) {
                    if (aKeys[i].nType.Equals(ZG_CTR_KEY_TYPE.ZG_KEY_NORMAL))
                    {
                        keyMap[i] = new ControllerKey();
                        keyMap[i].name = CardArrayToString(aKeys[i].rNum);
                        keyMap[i].isErased = aKeys[i].fErased;
                        log.Info("Key [" + i + "]: " + keyMap[i].name);
                    }
                    else {
                        log.Info("Key [" + i + "]: ---");
                    }
                    //log.Info("Key: " + i + ", " + aKeys[i].fErased + ", " + aKeys[i].nType + ", " + CardArrayToString(aKeys[i].rNum) + " ("+ aKeys[i].rNum[0] + "-"+ aKeys[i].rNum[1] + "-"+ aKeys[i].rNum[2] + "-"+ aKeys[i].rNum[3] + "-"+ aKeys[i].rNum[4] + "-"+ aKeys[i].rNum[5] + ")");
                    //byte[] res = new byte[16];
                }
                return keyMap;
            }
            finally
            {
                //Автоматически закрываем контроллер
                if (ControllerHandler != IntPtr.Zero)
                {
                    ZGIntf.ZG_CloseHandle(ControllerHandler);
                }
            }
        }

        public void addKey(ushort serialNumber, String name) {
            IntPtr ControllerHandler = new IntPtr(0);
            ZG_CTR_INFO ControllerInfo = new ZG_CTR_INFO();
            try
            {
                //Открываем контроллер
                int hr = ZGIntf.ZG_Ctr_Open(ref ControllerHandler, ConverterHandler, 255, serialNumber, ref ControllerInfo);
                bool m_fProximity = true;// ((ControllerInfo.nFlags & ZGIntf.ZG_CTR_F_PROXIMITY) != 0);
                log.Info("Proximity: " + m_fProximity);
                if (hr < 0)
                {
                    log.Fatal("Ошибка ZG_Ctr_Open (" + hr + ")");
                    throw new ZCommonException("Ошибка ZG_Ctr_Open").setErrorCode(hr);
                }
                ZG_CTR_KEY[] aKeys = new ZG_CTR_KEY[1];
                aKeys[0].nType = ZG_CTR_KEY_TYPE.ZG_KEY_NORMAL;
                aKeys[0].rNum = CardStringToArray(name);
                hr = ZGIntf.ZG_Ctr_WriteKeys(ControllerHandler, -1, aKeys, 1, null, default(IntPtr), 0, true);
                if (hr < 0)
                {
                    log.Fatal("Ошибка ZG_Ctr_Open (" + hr + ")");
                    throw new ZCommonException("Ошибка ZG_Ctr_WriteKeys").setErrorCode(hr);
                }
            }
            finally
            {
                //Автоматически закрываем контроллер
                if (ControllerHandler != IntPtr.Zero)
                {
                    ZGIntf.ZG_CloseHandle(ControllerHandler);
                }
            }
        }

        public void deleteKey(ushort serialNumber) {
        }

        public List<ControllerEvent> getEvents(ushort serialNumber) {
            ZG_CTR_INFO rCtrInfo = new ZG_CTR_INFO();
            IntPtr ControllerHandler = new IntPtr(0);
            ZG_CTR_INFO ControllerInfo = new ZG_CTR_INFO();
            ZG_CTR_EVENT[] aEvents = new ZG_CTR_EVENT[2048];
            ZG_EV_TIME rTime = new ZG_EV_TIME();
            ZG_EC_SUB_EV nSubEvent = new ZG_EC_SUB_EV();
            ZG_CTR_DIRECT nDirect = new ZG_CTR_DIRECT();
            try
            {
                //Открываем контроллер
                int hr = ZGIntf.ZG_Ctr_Open(ref ControllerHandler, ConverterHandler, 255, serialNumber, ref ControllerInfo);
                if (hr < 0)
                {
                    log.Fatal("Ошибка ZG_Ctr_Open (" + hr + ")");
                    throw new ZCommonException("Ошибка ZG_Ctr_Open").setErrorCode(hr);
                }
                hr = ZGIntf.ZG_Ctr_ReadEvents(ControllerHandler, 0, aEvents, 2048, null, IntPtr.Zero);
                if (hr < 0)
                {
                    log.Fatal("Ошибка ZG_Ctr_ReadEvents (" + hr + ")");
                    throw new ZCommonException("Ошибка ZG_Ctr_ReadEvents").setErrorCode(hr);
                }
                ZG_CTR_EVENT rEv;
                List<ControllerEvent> controllerEventList = new List<ControllerEvent>();
                for (int j = 0; j < 2048; j++)
                {
                    rEv = aEvents[j];
                    switch (rEv.nType) {
                        case ZG_CTR_EV_TYPE.ZG_EV_KEY_NOT_FOUND:
                        case ZG_CTR_EV_TYPE.ZG_EV_KEY_OPEN:
                        case ZG_CTR_EV_TYPE.ZG_EV_KEY_ACCESS:
                        case ZG_CTR_EV_TYPE.ZG_EV_UNKNOWN_KEY:
                            int nKeyIdx = 0;
                            int nKeyBank = 0;
                            ZGIntf.ZG_Ctr_DecodePassEvent(ControllerHandler, rEv.aData, ref rTime, ref nDirect, ref nKeyIdx, ref nKeyBank);
                            ControllerEvent newEvent = new ControllerEvent();
                            newEvent.month = rTime.nMonth;
                            newEvent.day = rTime.nDay;
                            newEvent.hour = rTime.nHour;
                            newEvent.minute = rTime.nMinute;
                            newEvent.second = rTime.nSecond;
                            newEvent.keyIndex = nKeyIdx;
                            controllerEventList.Add(newEvent);
                            break;
                    }
                }
                return controllerEventList;
            }
            finally
            {
                //Автоматически закрываем контроллер
                if (ControllerHandler != IntPtr.Zero)
                {
                    ZGIntf.ZG_CloseHandle(ControllerHandler);
                }
            }
        }
    }

    public class ControllerKey {
        public String name;
        public bool isErased;
    }

    public class ControllerInfoShort {
        public String name;
        public int address;
        public UInt16 serialNumber;
        public int maxEvents;
    }

    public class ControllerEvent {
        public byte month;
        public byte day;
        public byte hour;
        public byte minute;
        public byte second;
        public int keyIndex;
    }

    public class ZCommonException : Exception {
        private int errorCode;
        public int getErrorCode() {
            return errorCode;
        }
        public ZCommonException setErrorCode(int value) {
            errorCode = value;
            return this;
        }
        public ZCommonException() : base() { }
        public ZCommonException(string message) : base(message) { }
    }
}
