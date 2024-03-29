﻿using System;
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

        public void init(String proxyAddress) {
            if (!ConverterHandler.Equals(IntPtr.Zero)) {
                return;
            }
            int hr = ZGIntf.ZG_Initialize(ZPIntf.ZP_IF_NO_MSG_LOOP);
            if (hr < 0)
            {
                log.Fatal("Ошибка ZG_Initialize: " + hr);
                throw new ZCommonException("Ошибка ZG_Initialize").setErrorCode(hr);
            }
            ZG_CVT_INFO ConverterInfo = new ZG_CVT_INFO();
            ZG_CVT_OPEN_PARAMS OpenParams = new ZG_CVT_OPEN_PARAMS();
            if (proxyAddress.Contains(":"))
            {
                OpenParams.nPortType = ZP_PORT_TYPE.ZP_PORT_IP;
            }
            else
            {
                OpenParams.nPortType = ZP_PORT_TYPE.ZP_PORT_COM;
            }
            OpenParams.pszName = @proxyAddress;
            OpenParams.nSpeed = ZG_CVT_SPEED.ZG_SPEED_57600;

            hr = ZGIntf.ZG_Cvt_Open(ref ConverterHandler, ref OpenParams, ConverterInfo);
            if (hr < 0)
            {
                log.Fatal("Ошибка ZG_Cvt_Open: " + hr);
                throw new ZCommonException("Ошибка ZG_Cvt_Open").setErrorCode(hr);
            }
        }
        public void close() {
            if (ConverterHandler == IntPtr.Zero) {
                return;
            }
            int hr = ZGIntf.ZG_CloseHandle(ConverterHandler);
            if (hr < 0)
            {
                log.Fatal("Ошибка ZG_CloseHandle: " + hr);
                throw new ZCommonException("Ошибка ZG_CloseHandle").setErrorCode(hr);
            }
            ConverterHandler = IntPtr.Zero;
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
            newControllerInfoShort.maxKeys = pInfo.nMaxKeys;
            newControllerInfoShort.maxEvents = pInfo.nMaxEvents;
            tmpControllerInfoShortList.Add(newControllerInfoShort);
            return true;
        }

        public List<ControllerInfoShort> GetControllers() {
            IntPtr ControllerHandler = new IntPtr(0);
            ZG_CTR_INFO ControllerInfo = new ZG_CTR_INFO();
            ZG_CTR_MODE ControllerMode = ZG_CTR_MODE.ZG_MODE_UNDEF;
            uint ControllerModeFlags = 0;
            tmpControllerInfoShortList = new List<ControllerInfoShort>();
            int hr = ZGIntf.ZG_Cvt_EnumControllers(ConverterHandler, tmpEnumConverters, IntPtr.Zero);
            if (hr < 0)
            {
                log.Fatal("Ошибка ZG_Cvt_EnumControllers (" + hr + ").");
                throw new ZCommonException("Ошибка ZG_Cvt_EnumControllers").setErrorCode(hr);
            }
            if (tmpControllerInfoShortList.Count > 0) {
                for (int i = 0; i < tmpControllerInfoShortList.Count; i++)
                {
                    try
                    {
                        //Открываем контроллер
                        hr = ZGIntf.ZG_Ctr_Open(ref ControllerHandler, ConverterHandler, 255, tmpControllerInfoShortList[i].serialNumber, ref ControllerInfo);
                        if (hr < 0)
                        {
                            log.Fatal("Ошибка ZG_Ctr_Open (" + hr + ")");
                            throw new ZCommonException("Ошибка ZG_Ctr_Open").setErrorCode(hr);
                        }
                        //Читаем режим
                        hr = ZGIntf.ZG_Ctr_GetCtrModeInfo(ControllerHandler, ref ControllerMode, ref ControllerModeFlags);
                        if (hr < 0)
                        {
                            log.Fatal("Ошибка ZG_Ctr_GetCtrModeInfo (" + hr + ")");
                            throw new ZCommonException("Ошибка ZG_Ctr_GetCtrModeInfo").setErrorCode(hr);
                        }
                        tmpControllerInfoShortList[i].mode = ControllerMode;
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
            return tmpControllerInfoShortList;
        }

        public void setControllerMode(ushort serialNumber, ZG_CTR_MODE mode)
        {
            IntPtr ControllerHandler = new IntPtr(0);
            ZG_CTR_INFO ControllerInfo = new ZG_CTR_INFO();
            try
            {
                //Открываем контроллер
                int hr = ZGIntf.ZG_Ctr_Open(ref ControllerHandler, ConverterHandler, 255, serialNumber, ref ControllerInfo);
                if (hr < 0)
                {
                    log.Fatal("Ошибка ZG_Ctr_Open (" + hr + ")");
                    throw new ZCommonException("Ошибка ZG_Ctr_Open").setErrorCode(hr);
                }
                //Читаем режим
                hr = ZGIntf.ZG_Ctr_SetCtrMode(ControllerHandler, mode);
                if (hr < 0)
                {
                    log.Fatal("Ошибка ZG_Ctr_SetCtrMode (" + hr + ")");
                    throw new ZCommonException("Ошибка ZG_Ctr_SetCtrMode").setErrorCode(hr);
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

        public void setControllerDateTime(ushort serialNumber, ushort year, ushort month, ushort day, ushort hour, ushort minute, ushort second)
        {
            IntPtr ControllerHandler = new IntPtr(0);
            ZG_CTR_INFO ControllerInfo = new ZG_CTR_INFO();
            try
            {
                //Открываем контроллер
                int hr = ZGIntf.ZG_Ctr_Open(ref ControllerHandler, ConverterHandler, 255, serialNumber, ref ControllerInfo);
                if (hr < 0)
                {
                    log.Fatal("Ошибка ZG_Ctr_Open (" + hr + ")");
                    throw new ZCommonException("Ошибка ZG_Ctr_Open").setErrorCode(hr);
                }
                //Ставим контроллеру дату/время
                ZG_CTR_CLOCK clock = new ZG_CTR_CLOCK();
                clock.fStopped = false;
                clock.nYear = year;
                clock.nMonth = month;
                clock.nDay = day;
                clock.nHour = hour;
                clock.nMinute = minute;
                clock.nSecond = second;
                hr = ZGIntf.ZG_Ctr_SetClock(ControllerHandler, ref clock);
                if (hr < 0)
                {
                    log.Fatal("Ошибка ZG_Ctr_SetClock (" + hr + ")");
                    throw new ZCommonException("Ошибка ZG_Ctr_SetClock").setErrorCode(hr);
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

        public ControllerDateTime getControllerDateTime(ushort serialNumber)
        {
            IntPtr ControllerHandler = new IntPtr(0);
            ZG_CTR_INFO ControllerInfo = new ZG_CTR_INFO();
            try
            {
                //Открываем контроллер
                int hr = ZGIntf.ZG_Ctr_Open(ref ControllerHandler, ConverterHandler, 255, serialNumber, ref ControllerInfo);
                if (hr < 0)
                {
                    log.Fatal("Ошибка ZG_Ctr_Open (" + hr + ")");
                    throw new ZCommonException("Ошибка ZG_Ctr_Open").setErrorCode(hr);
                }
                //Ставим контроллеру дату/время
                ZG_CTR_CLOCK clock = new ZG_CTR_CLOCK();
                hr = ZGIntf.ZG_Ctr_GetClock(ControllerHandler, ref clock);
                if (hr < 0)
                {
                    log.Fatal("Ошибка ZG_Ctr_GetClock (" + hr + ")");
                    throw new ZCommonException("Ошибка ZG_Ctr_GetClock").setErrorCode(hr);
                }
                ControllerDateTime response = new ControllerDateTime();
                response.year = clock.nYear;
                response.month = clock.nMonth;
                response.day = clock.nDay;
                response.hour = clock.nHour;
                response.minute = clock.nMinute;
                response.second = clock.nSecond;
                return response;
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

        public List<ControllerKey> getKeys(ushort serialNumber)
        {
            return getKeys(serialNumber, 0, 2024);
        }

        public List<ControllerKey> getKeys(ushort serialNumber, int keyIndex, int keyCount) {
            IntPtr ControllerHandler = new IntPtr(0);
            ZG_CTR_INFO ControllerInfo = new ZG_CTR_INFO();
            ZG_CTR_KEY[] aKeys = new ZG_CTR_KEY[keyCount];
            List<ControllerKey> keyList = new List<ControllerKey>();
            ControllerKey newKey;
            try
            {
                //Открываем контроллер
                int hr = ZGIntf.ZG_Ctr_Open(ref ControllerHandler, ConverterHandler, 255, serialNumber, ref ControllerInfo);
                if (hr < 0)
                {
                    log.Fatal("Ошибка ZG_Ctr_Open (" + hr + ")");
                    throw new ZCommonException("Ошибка ZG_Ctr_Open").setErrorCode(hr);
                }
                //Читаем список карт
                hr = ZGIntf.ZG_Ctr_ReadKeys(ControllerHandler, keyIndex, aKeys, keyCount, null, IntPtr.Zero, 0);
                if (hr < 0) {
                    log.Fatal("Ошибка ZG_Ctr_ReadKeys (" + hr + ")");
                    throw new ZCommonException("Ошибка ZG_Ctr_ReadKeys").setErrorCode(hr);
                }
                for (int i = 0; i < keyCount; i++) {
                    if (aKeys[i].nType.Equals(ZG_CTR_KEY_TYPE.ZG_KEY_NORMAL))
                    {
                        newKey = new ControllerKey();
                        newKey.code = CardArrayToString(aKeys[i].rNum);
                        newKey.isErased = aKeys[i].fErased;
                        keyList.Add(newKey);
                        //log.Info("KEY: " + newKey.name + ", " + aKeys[i].nAccess + ", " + aKeys[i].nFlags);
                    }
                    else {
                        keyList.Add(null);
                    }
                }
                return keyList;
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

        public void addKey(ushort serialNumber, int keyIndex, String code) {
            IntPtr ControllerHandler = new IntPtr(0);
            ZG_CTR_INFO ControllerInfo = new ZG_CTR_INFO();
            try
            {
                //Открываем контроллер
                int hr = ZGIntf.ZG_Ctr_Open(ref ControllerHandler, ConverterHandler, 255, serialNumber, ref ControllerInfo);
                if (hr < 0)
                {
                    log.Fatal("Ошибка ZG_Ctr_Open (" + hr + ")");
                    throw new ZCommonException("Ошибка ZG_Ctr_Open").setErrorCode(hr);
                }
                //Пишем ключ
                ZG_CTR_KEY[] aKeys = new ZG_CTR_KEY[1];
                aKeys[0].nType = ZG_CTR_KEY_TYPE.ZG_KEY_NORMAL;
                aKeys[0].rNum = CardStringToArray(code);
                aKeys[0].nAccess = 255; //Доступ без рассписания
                aKeys[0].nFlags = ZGIntf.ZG_KF_SHORTNUM;
                int _keyIndex = -1;
                if (keyIndex == -1)
                {
                    hr = ZGIntf.ZG_Ctr_GetKeyTopIndex(ControllerHandler, ref _keyIndex, 0);
                    if (hr < 0)
                    {
                        log.Fatal("Ошибка ZG_Ctr_GetKeyTopIndex (" + hr + ")");
                        throw new ZCommonException("Ошибка ZG_Ctr_GetKeyTopIndex").setErrorCode(hr);
                    }
                }
                else {
                    _keyIndex = keyIndex;
                }
                //Console.WriteLine("Пишем: {0}", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
                //log.Info("Пишем");
                hr = ZGIntf.ZG_Ctr_WriteKeys(ControllerHandler, _keyIndex, aKeys, 1, null, default(IntPtr), 0);
                if (hr < 0)
                {
                    log.Fatal("Ошибка ZG_Ctr_WriteKeys (" + hr + ")");
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

        public void clearKey(ushort serialNumber, int keyIndex) {
            IntPtr ControllerHandler = new IntPtr(0);
            ZG_CTR_INFO ControllerInfo = new ZG_CTR_INFO();
            try
            {
                //Открываем контроллер
                int hr = ZGIntf.ZG_Ctr_Open(ref ControllerHandler, ConverterHandler, 255, serialNumber, ref ControllerInfo);
                if (hr < 0)
                {
                    log.Fatal("Ошибка ZG_Ctr_Open (" + hr + ")");
                    throw new ZCommonException("Ошибка ZG_Ctr_Open").setErrorCode(hr);
                }
                //Удаляем ключ
                hr = ZGIntf.ZG_Ctr_ClearKeys(ControllerHandler, keyIndex, 1, null, default(IntPtr), 0);
                if (hr < 0)
                {
                    log.Fatal("Ошибка ZG_Ctr_ClearKeys (" + hr + ")");
                    throw new ZCommonException("Ошибка ZG_Ctr_ClearKeys").setErrorCode(hr);
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

        public void clearAllKeys(ushort serialNumber)
        {
            IntPtr ControllerHandler = new IntPtr(0);
            ZG_CTR_INFO ControllerInfo = new ZG_CTR_INFO();
            try
            {
                //Открываем контроллер
                int hr = ZGIntf.ZG_Ctr_Open(ref ControllerHandler, ConverterHandler, 255, serialNumber, ref ControllerInfo);
                if (hr < 0)
                {
                    log.Fatal("Ошибка ZG_Ctr_Open (" + hr + ")");
                    throw new ZCommonException("Ошибка ZG_Ctr_Open").setErrorCode(hr);
                }
                //Удаляем ключ
                hr = ZGIntf.ZG_Ctr_ClearKeys(ControllerHandler, 0, ControllerInfo.nMaxKeys, null, default(IntPtr), 0);
                if (hr < 0)
                {
                    log.Fatal("Ошибка ZG_Ctr_ClearKeys (" + hr + ")");
                    throw new ZCommonException("Ошибка ZG_Ctr_ClearKeys").setErrorCode(hr);
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

        public GetUnreadEventsResult getUnreadEvents(ushort serialNumber, int lastReadIndex, int lastReadMonth, int lastReadDay, int lastReadHour, int lastReadMinute, int lastReadSecond)
        {
            IntPtr ControllerHandler = new IntPtr(0);
            ZG_CTR_INFO ControllerInfo = new ZG_CTR_INFO();
            GetUnreadEventsResult result = new GetUnreadEventsResult();
            result.items = new List<ControllerEvent>();
            try
            {
                //Открываем контроллер
                int hr = ZGIntf.ZG_Ctr_Open(ref ControllerHandler, ConverterHandler, 255, serialNumber, ref ControllerInfo);
                if (hr < 0)
                {
                    log.Fatal("Ошибка ZG_Ctr_Open (" + hr + ")");
                    throw new ZCommonException("Ошибка ZG_Ctr_Open").setErrorCode(hr);
                }
                //Читаем указатель на следующее записываемое событие
                int writeIndex = 0;
                int _readIndex = 0;
                hr = ZGIntf.ZG_Ctr_ReadEventIdxs(ControllerHandler, ref writeIndex, ref _readIndex);
                //log.Info("WriteIndex: " + writeIndex);
                if (lastReadIndex == -1)
                {
                    //Никогда не читали событий из контроллера
                    //log.Info("Не читали события из контроллера");
                    if (writeIndex > 0)
                    {
                        FetchEventsResult fetchEventsResult = fetchEventsBlock(ControllerHandler, 0, writeIndex);
                        if (fetchEventsResult.items.Count > 0)
                        {
                            result.items = fetchEventsResult.items;
                            result.lastReadIndex = fetchEventsResult.lastReadIndex;
                            result.lastReadMonth = fetchEventsResult.lastReadMonth;
                            result.lastReadDay = fetchEventsResult.lastReadDay;
                            result.lastReadHour = fetchEventsResult.lastReadHour;
                            result.lastReadMinute = fetchEventsResult.lastReadMinute;
                            result.lastReadSecond = fetchEventsResult.lastReadSecond;
                        }
                        else
                        {
                            result.lastReadIndex = -1;
                        }
                    }
                    else
                    {
                        //Если следующий индекс, в который будет производиться запись, равен нулю, то ничего пока не читаем
                        result.lastReadIndex = -1;
                    }
                }
                else
                {
                    //Читаем последнее полученное в прошлый раз событие
                    FetchEventsResult fetchEventsResult = fetchEventsBlock(ControllerHandler, lastReadIndex, 1);
                    //Если на прежнем месте события нет, или оно по свойствам отличается от последнего прочитанного, значит был сбой в работе контроллера
                    //и нужно перечитать все события с него
                    if ((fetchEventsResult.items.Count == 0) ||
                        !(fetchEventsResult.items[0].month == lastReadMonth) ||
                        !(fetchEventsResult.items[0].day == lastReadDay) ||
                        !(fetchEventsResult.items[0].hour == lastReadHour) ||
                        !(fetchEventsResult.items[0].minute == lastReadMinute) ||
                        !(fetchEventsResult.items[0].second == lastReadSecond)
                        )
                    {
                        //log.Info("Последнее событие отличается");
                        //Читаем хвост от следующего индекса записи до конца списка
                        fetchEventsResult = fetchEventsBlock(ControllerHandler, writeIndex, ControllerInfo.nMaxEvents - writeIndex);
                        if (fetchEventsResult.items.Count > 0)
                        {
                            result.items.AddRange(fetchEventsResult.items);
                            result.lastReadIndex = fetchEventsResult.lastReadIndex;
                            result.lastReadMonth = fetchEventsResult.lastReadMonth;
                            result.lastReadDay = fetchEventsResult.lastReadDay;
                            result.lastReadHour = fetchEventsResult.lastReadHour;
                            result.lastReadMinute = fetchEventsResult.lastReadMinute;
                            result.lastReadSecond = fetchEventsResult.lastReadSecond;
                        }
                        //Читаем от начала списка до следующего индекса записи
                        if (writeIndex > 0)
                        {
                            fetchEventsResult = fetchEventsBlock(ControllerHandler, 0, writeIndex);
                            if (fetchEventsResult.items.Count > 0)
                            {
                                result.items.AddRange(fetchEventsResult.items);
                                result.lastReadIndex = fetchEventsResult.lastReadIndex;
                                result.lastReadMonth = fetchEventsResult.lastReadMonth;
                                result.lastReadDay = fetchEventsResult.lastReadDay;
                                result.lastReadHour = fetchEventsResult.lastReadHour;
                                result.lastReadMinute = fetchEventsResult.lastReadMinute;
                                result.lastReadSecond = fetchEventsResult.lastReadSecond;
                            }
                        }
                        if (result.items.Count == 0)
                        {
                            result.lastReadIndex = -1;
                        }
                    }
                    else
                    {
                        //log.Info("Последнее событие в порядке");
                        if (writeIndex > lastReadIndex)
                        {
                            //Читаем небольшой хвост от текущего индекса до следующего индекса записи
                            if (writeIndex - lastReadIndex - 1 > 0)
                            {
                                fetchEventsResult = fetchEventsBlock(ControllerHandler, lastReadIndex + 1, writeIndex - lastReadIndex - 1);
                                if (fetchEventsResult.items.Count > 0)
                                {
                                    result.items = fetchEventsResult.items;
                                    result.lastReadIndex = fetchEventsResult.lastReadIndex;
                                    result.lastReadMonth = fetchEventsResult.lastReadMonth;
                                    result.lastReadDay = fetchEventsResult.lastReadDay;
                                    result.lastReadHour = fetchEventsResult.lastReadHour;
                                    result.lastReadMinute = fetchEventsResult.lastReadMinute;
                                    result.lastReadSecond = fetchEventsResult.lastReadSecond;
                                }
                                else
                                {
                                    result.lastReadIndex = lastReadIndex;
                                    result.lastReadMonth = lastReadMonth;
                                    result.lastReadDay = lastReadDay;
                                    result.lastReadHour = lastReadHour;
                                    result.lastReadMinute = lastReadMinute;
                                    result.lastReadSecond = lastReadSecond;
                                }
                            }
                            else {
                                result.lastReadIndex = lastReadIndex;
                                result.lastReadMonth = lastReadMonth;
                                result.lastReadDay = lastReadDay;
                                result.lastReadHour = lastReadHour;
                                result.lastReadMinute = lastReadMinute;
                                result.lastReadSecond = lastReadSecond;
                            }
                        }
                        else
                        {
                            //Индекс записи до шёл до конца списка и теперь отстаёт от нас
                            //Читаем вначале хвост от нас до конца списка
                            if (lastReadIndex < ControllerInfo.nMaxEvents - 1)
                            {
                                fetchEventsResult = fetchEventsBlock(ControllerHandler, lastReadIndex + 1, ControllerInfo.nMaxEvents - lastReadIndex - 1);
                                if (fetchEventsResult.items.Count > 0)
                                {
                                    result.items.AddRange(fetchEventsResult.items);
                                    result.lastReadIndex = fetchEventsResult.lastReadIndex;
                                    result.lastReadMonth = fetchEventsResult.lastReadMonth;
                                    result.lastReadDay = fetchEventsResult.lastReadDay;
                                    result.lastReadHour = fetchEventsResult.lastReadHour;
                                    result.lastReadMinute = fetchEventsResult.lastReadMinute;
                                    result.lastReadSecond = fetchEventsResult.lastReadSecond;
                                }
                            }
                            //Читаем от начала списка до индекса записи
                            if (writeIndex >= 0)
                            {
                                fetchEventsResult = fetchEventsBlock(ControllerHandler, 0, writeIndex);
                                if (fetchEventsResult.items.Count > 0)
                                {
                                    result.items.AddRange(fetchEventsResult.items);
                                    result.lastReadIndex = fetchEventsResult.lastReadIndex;
                                    result.lastReadMonth = fetchEventsResult.lastReadMonth;
                                    result.lastReadDay = fetchEventsResult.lastReadDay;
                                    result.lastReadHour = fetchEventsResult.lastReadHour;
                                    result.lastReadMinute = fetchEventsResult.lastReadMinute;
                                    result.lastReadSecond = fetchEventsResult.lastReadSecond;
                                }
                            }
                            if (result.items.Count == 0)
                            {
                                result.lastReadIndex = lastReadIndex;
                                result.lastReadMonth = lastReadMonth;
                                result.lastReadDay = lastReadDay;
                                result.lastReadHour = lastReadHour;
                                result.lastReadMinute = lastReadMinute;
                                result.lastReadSecond = lastReadSecond;
                            }
                        }
                    }
                }
                //log.Info("Событий вернулось: " + result.items.Count);
                return result;
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

        private FetchEventsResult fetchEventsBlock(IntPtr ControllerHandler, int eventIndex, int eventCount)
        {
            //log.Info("Requested from " + eventIndex + ", count of " + eventCount);
            ZG_EV_TIME rTime = new ZG_EV_TIME();
            ZG_CTR_DIRECT nDirect = new ZG_CTR_DIRECT();
            ZG_CTR_EVENT[] aEvents = new ZG_CTR_EVENT[eventCount];
            int hr = ZGIntf.ZG_Ctr_ReadEvents(ControllerHandler, eventIndex, aEvents, eventCount, null, IntPtr.Zero);
            if (hr < 0)
            {
                log.Fatal("Ошибка ZG_Ctr_ReadEvents (" + hr + ")");
                throw new ZCommonException("Ошибка ZG_Ctr_ReadEvents").setErrorCode(hr);
            }
            ZG_CTR_EVENT rEv;
            FetchEventsResult result = new FetchEventsResult();
            result.items = new List<ControllerEvent>();
            for (int j = 0; j < eventCount; j++)
            {
                rEv = aEvents[j];
                switch (rEv.nType)
                {
                    case ZG_CTR_EV_TYPE.ZG_EV_KEY_NOT_FOUND:
                    case ZG_CTR_EV_TYPE.ZG_EV_KEY_OPEN:
                    case ZG_CTR_EV_TYPE.ZG_EV_KEY_ACCESS:
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
                        result.items.Add(newEvent);
                        result.lastReadIndex = eventIndex + j;
                        result.lastReadMonth = rTime.nMonth;
                        result.lastReadDay = rTime.nDay;
                        result.lastReadHour = rTime.nHour;
                        result.lastReadMinute = rTime.nMinute;
                        result.lastReadSecond = rTime.nSecond;
                        break;
                }
            }
            return result;
        }

        public List<ControllerEvent> getEvents(ushort serialNumber, int eventIndex, int eventCount) {
            IntPtr ControllerHandler = new IntPtr(0);
            ZG_CTR_INFO ControllerInfo = new ZG_CTR_INFO();
            try
            {
                //Открываем контроллер
                int hr = ZGIntf.ZG_Ctr_Open(ref ControllerHandler, ConverterHandler, 255, serialNumber, ref ControllerInfo);
                if (hr < 0)
                {
                    log.Fatal("Ошибка ZG_Ctr_Open (" + hr + ")");
                    throw new ZCommonException("Ошибка ZG_Ctr_Open").setErrorCode(hr);
                }
                return fetchEventsBlock(ControllerHandler, eventIndex, eventCount).items;
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

    public class FetchEventsResult
    {
        public List<ControllerEvent> items;
        public int lastReadIndex;
        public int lastReadMonth;
        public int lastReadDay;
        public int lastReadHour;
        public int lastReadMinute;
        public int lastReadSecond;
    }

    public class GetUnreadEventsResult {
        public List<ControllerEvent> items;
        public int lastReadIndex;
        public int lastReadMonth;
        public int lastReadDay;
        public int lastReadHour;
        public int lastReadMinute;
        public int lastReadSecond;
    }

    public class ControllerKey {
        public String code;
        public bool isErased;
    }

    public class ControllerInfoShort {
        public String name;
        public int address;
        public int serialNumber;
        public int maxKeys;
        public int maxEvents;

        public ZG_CTR_MODE mode;
    }

    public class ControllerEvent {
        public byte month;
        public byte day;
        public byte hour;
        public byte minute;
        public byte second;
        public int keyIndex;
    }

    public class ControllerDateTime {
        public ushort year;
        public ushort month;
        public ushort day;
        public ushort hour;
        public ushort minute;
        public ushort second;
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
