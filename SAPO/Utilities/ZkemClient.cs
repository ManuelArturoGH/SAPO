
using System;
using Microsoft.Extensions.Logging;
using SAPO.Controllers;
using zkemkeeper;   

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using zkemkeeper;

namespace SAPO.utilities
{
    public class ZkemClient : IZKEM, IDisposable
    {
        private readonly Action<object, string> _raiseDeviceEvent;
        private readonly ILogger _logger;
        private readonly CZKEM _inner;
        private bool _eventsAttached;
        private bool _disposed;

        public ZkemClient(ILogger logger, Action<object, string> raiseDeviceEvent)
        {
            _logger = logger;
            _raiseDeviceEvent = raiseDeviceEvent;
            _inner = new CZKEM();
        }

        private void AttachEventsIfNeeded()
        {
            if (_eventsAttached) return;
            _inner.OnConnected += Inner_OnConnected;
            _inner.OnDisConnected += Inner_OnDisConnected;
            _inner.OnEnrollFinger += Inner_OnEnrollFinger;
            _inner.OnFinger += Inner_OnFinger;
            _inner.OnAttTransactionEx += Inner_OnAttTransactionEx;
            _eventsAttached = true;
        }

        private void DetachEvents()
        {
            if (!_eventsAttached) return;
            _inner.OnConnected -= Inner_OnConnected;
            _inner.OnDisConnected -= Inner_OnDisConnected;
            _inner.OnEnrollFinger -= Inner_OnEnrollFinger;
            _inner.OnFinger -= Inner_OnFinger;
            _inner.OnAttTransactionEx -= Inner_OnAttTransactionEx;
            _eventsAttached = false;
        }

        #region Event Handlers
        private void Inner_OnFinger()
        {
            _logger?.LogDebug("Finger detected.");
            SafeRaise("FingerDetected");
        }

        private void Inner_OnEnrollFinger(int enrollNumber, int fingerIndex, int actionResult, int templateLength)
        {
            _logger?.LogDebug("Enroll finger event. Enroll:{Enroll} Finger:{Finger} Result:{Result}", enrollNumber, fingerIndex, actionResult);
        }

        private void Inner_OnConnected()
        {
            _logger?.LogInformation("Device connected.");
        }

        private void Inner_OnAttTransactionEx(string enrollNumber, int isInValid, int attState, int verifyMethod,
            int year, int month, int day, int hour, int minute, int second, int workCode)
        {
            _logger?.LogTrace("Attendance event Enroll:{Enroll} {Y}-{M}-{D} {H}:{Min}:{S}", enrollNumber, year, month, day, hour, minute, second);
        }

        private void Inner_OnDisConnected()
        {
            _logger?.LogWarning("Device disconnected.");
            SafeRaise(UniversalStatic.acx_Disconnect);
        }

        private void SafeRaise(string value)
        {
            try { _raiseDeviceEvent?.Invoke(this, value); }
            catch (Exception ex) { _logger?.LogError(ex, "Error raising device event {Event}", value); }
        }
        #endregion

        public bool Connect_Net(string ip, int port)
        {
            ThrowIfDisposed();
            if (_inner.Connect_Net(ip, port))
            {
                if (_inner.RegEvent(254, 32767))
                {
                    AttachEventsIfNeeded();
                }
                return true;
            }
            return false;
        }

        public void Disconnect()
        {
            if (_disposed) return;
            try
            {
                DetachEvents();
                _inner.Disconnect();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during Disconnect");
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(ZkemClient));
        }

        #region Corrected Proxy Implementations (Previously Recursive or Incorrect)
        public bool DelUserTmp(int dwMachineNumber, int dwEnrollNumber, int dwFingerIndex)
            => _inner.DelUserTmp(dwMachineNumber, dwEnrollNumber, dwFingerIndex);

        public bool GetUserInfo(int dwMachineNumber, int dwEnrollNumber, ref string name, ref string password,
            ref int privilege, ref bool enabled)
            => _inner.GetUserInfo(dwMachineNumber, dwEnrollNumber, ref name, ref password, ref privilege, ref enabled);

        public bool StartEnroll(int userID, int fingerID)
            => _inner.StartEnroll(userID, fingerID);

        public bool StartEnrollEx(string userID, int fingerID, int flag)
            => _inner.StartEnrollEx(userID, fingerID, flag);

        public bool StartIdentify()
            => _inner.StartIdentify();
        #endregion

        #region Fixed ref Passing
        public bool GetAllUserID(int dwMachineNumber, ref int dwEnrollNumber, ref int dwEMachineNumber,
            ref int dwBackupNumber, ref int dwMachinePrivilege, ref int dwEnable)
            => _inner.GetAllUserID(dwMachineNumber, ref dwEnrollNumber, ref dwEMachineNumber, ref dwBackupNumber,
                                   ref dwMachinePrivilege, ref dwEnable);

        public bool GetFirmwareVersion(int dwMachineNumber, ref string strVersion)
            => _inner.GetFirmwareVersion(dwMachineNumber, ref strVersion);

        public bool GetVendor(ref string strVendor)
            => _inner.GetVendor(ref strVendor);

        public bool GetWiegandFmt(int dwMachineNumber, ref string sWiegandFmt)
            => _inner.GetWiegandFmt(dwMachineNumber, ref sWiegandFmt);

        public bool GetSDKVersion(ref string strVersion)
            => _inner.GetSDKVersion(ref strVersion);

        public bool GetSerialNumber(int dwMachineNumber, out string dwSerialNumber)
            => _inner.GetSerialNumber(dwMachineNumber, out dwSerialNumber);

        public bool GetDeviceMAC(int dwMachineNumber, ref string sMAC)
            => _inner.GetDeviceMAC(dwMachineNumber, ref sMAC);

        public void GetLastError(ref int dwErrorCode)
            => _inner.GetLastError(ref dwErrorCode);
        #endregion

        #region Direct Passthroughs (Selected Needed Members)
        public bool BatchUpdate(int dwMachineNumber) => _inner.BatchUpdate(dwMachineNumber);
        public bool Beep(int delayMs) => _inner.Beep(delayMs);
        public bool BeginBatchUpdate(int dwMachineNumber, int updateFlag) => _inner.BeginBatchUpdate(dwMachineNumber, updateFlag);
        public bool ClearData(int dwMachineNumber, int dataFlag) => _inner.ClearData(dwMachineNumber, dataFlag);
        public bool ClearGLog(int dwMachineNumber) => _inner.ClearGLog(dwMachineNumber);
        public bool DisableDeviceWithTimeOut(int dwMachineNumber, int timeOutSec) => _inner.DisableDeviceWithTimeOut(dwMachineNumber, timeOutSec);
        public bool EnableDevice(int dwMachineNumber, bool bFlag) => _inner.EnableDevice(dwMachineNumber, bFlag);
        public bool GetDeviceTime(int dwMachineNumber, ref int y, ref int m, ref int d, ref int h, ref int min, ref int s)
            => _inner.GetDeviceTime(dwMachineNumber, ref y, ref m, ref d, ref h, ref min, ref s);
        public bool GetUserInfoEx(int dwMachineNumber, int dwEnrollNumber, out int verifyStyle, out byte reserved)
            => _inner.GetUserInfoEx(dwMachineNumber, dwEnrollNumber, out verifyStyle, out reserved);
        public bool GetUserTmp(int dwMachineNumber, int dwEnrollNumber, int dwFingerIndex, ref byte tmpData, ref int tmpLength)
            => _inner.GetUserTmp(dwMachineNumber, dwEnrollNumber, dwFingerIndex, ref tmpData, ref tmpLength);
        public bool GetUserTmpEx(int dwMachineNumber, string dwEnrollNumber, int dwFingerIndex, out int flag, out byte tmpData, out int tmpLength)
            => _inner.GetUserTmpEx(dwMachineNumber, dwEnrollNumber, dwFingerIndex, out flag, out tmpData, out tmpLength);
        public bool GetUserTmpExStr(int dwMachineNumber, string dwEnrollNumber, int dwFingerIndex, out int flag, out string tmpData, out int tmpLength)
            => _inner.GetUserTmpExStr(dwMachineNumber, dwEnrollNumber, dwFingerIndex, out flag, out tmpData, out tmpLength);
        public bool PowerOffDevice(int dwMachineNumber) => _inner.PowerOffDevice(dwMachineNumber);
        public bool QueryState(ref int state) => _inner.QueryState(ref state);
        public bool ReadAllGLogData(int dwMachineNumber) => _inner.ReadAllGLogData(dwMachineNumber);
        public bool ReadAllTemplate(int dwMachineNumber) => _inner.ReadAllTemplate(dwMachineNumber);
        public bool ReadAllUserID(int dwMachineNumber) => _inner.ReadAllUserID(dwMachineNumber);
        public bool RefreshData(int dwMachineNumber) => _inner.RefreshData(dwMachineNumber);
        public bool RegEvent(int dwMachineNumber, int eventMask) => _inner.RegEvent(dwMachineNumber, eventMask);
        public bool RestartDevice(int dwMachineNumber) => _inner.RestartDevice(dwMachineNumber);
        public bool SaveTheDataToFile(int dwMachineNumber, string filePath, int fileFlag)
            => _inner.SaveTheDataToFile(dwMachineNumber, filePath, fileFlag);
        public bool SetUserTmpExStr(int dwMachineNumber, string dwEnrollNumber, int dwFingerIndex, int flag, string tmpData)
            => _inner.SetUserTmpExStr(dwMachineNumber, dwEnrollNumber, dwFingerIndex, flag, tmpData);
        public bool SSR_EnableUser(int dwMachineNumber, string dwEnrollNumber, bool bFlag)
            => _inner.SSR_EnableUser(dwMachineNumber, dwEnrollNumber, bFlag);
        public bool SSR_GetAllUserInfo(int dwMachineNumber, out string enroll, out string name, out string pwd, out int privilege, out bool enabled)
            => _inner.SSR_GetAllUserInfo(dwMachineNumber, out enroll, out name, out pwd, out privilege, out enabled);
        public bool SSR_GetGeneralLogData(int dwMachineNumber, out string enroll, out int verifyMode, out int inOutMode,
            out int year, out int month, out int day, out int hour, out int minute, out int second, ref int workCode)
            => _inner.SSR_GetGeneralLogData(dwMachineNumber, out enroll, out verifyMode, out inOutMode, out year, out month, out day, out hour, out minute, out second, ref workCode);
        public bool SSR_GetUserInfo(int dwMachineNumber, string dwEnrollNumber, out string name, out string password, out int privilege, out bool enabled)
            => _inner.SSR_GetUserInfo(dwMachineNumber, dwEnrollNumber, out name, out password, out privilege, out enabled);
        public bool SSR_GetUserTmpStr(int dwMachineNumber, string dwEnrollNumber, int dwFingerIndex, out string tmpData, out int tmpLength)
            => _inner.SSR_GetUserTmpStr(dwMachineNumber, dwEnrollNumber, dwFingerIndex, out tmpData, out tmpLength);
        public bool SSR_SetUserInfo(int dwMachineNumber, string dwEnrollNumber, string name, string password, int privilege, bool enabled)
            => _inner.SSR_SetUserInfo(dwMachineNumber, dwEnrollNumber, name, password, privilege, enabled);
        public bool PlayVoice(int position, int length) => _inner.PlayVoice(position, length);
        public bool GetConnectStatus(ref int dwErrorCode) => _inner.GetConnectStatus(ref dwErrorCode);
        #endregion

        #region IDisposable
        public void Dispose()
        {
            if (_disposed) return;
            Disconnect();
            try
            {
                if (_inner != null && Marshal.IsComObject(_inner))
                {
                    Marshal.FinalReleaseComObject(_inner);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error releasing COM instance");
            }
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        ~ZkemClient()
        {
            Dispose();
        }
        #endregion

        #region Unimplemented IZKEM Members (throw to indicate not yet needed)
        public bool GetAllGLogData(int dwMachineNumber, ref int dwTMachineNumber, ref int dwEnrollNumber,
            ref int dwEMachineNumber, ref int dwVerifyMode, ref int dwInOutMode, ref int dwYear, ref int dwMonth,
            ref int dwDay, ref int dwHour, ref int dwMinute) => throw new NotImplementedException();
        public bool GetAllSLogData(int dwMachineNumber, ref int dwTMachineNumber, ref int dwSEnrollNumber, ref int Params4,
            ref int Params1, ref int Params2, ref int dwManipulation, ref int Params3, ref int dwYear, ref int dwMonth,
            ref int dwDay, ref int dwHour, ref int dwMinute) => throw new NotImplementedException();
        public bool GetAllUserInfo(int dwMachineNumber, ref int dwEnrollNumber, ref string Name, ref string Password,
            ref int Privilege, ref bool Enabled) => throw new NotImplementedException();

        public bool ClearAdministrators(int dwMachineNumber)
        {
            throw new NotImplementedException();
        }

        public bool DeleteEnrollData(int dwMachineNumber, int dwEnrollNumber, int dwEMachineNumber, int dwBackupNumber)
        {
            throw new NotImplementedException();
        }

        public bool ReadSuperLogData(int dwMachineNumber)
        {
            throw new NotImplementedException();
        }

        public bool ReadAllSLogData(int dwMachineNumber)
        {
            throw new NotImplementedException();
        }

        public bool ReadGeneralLogData(int dwMachineNumber)
        {
            throw new NotImplementedException();
        }

        public bool EnableUser(int dwMachineNumber, int dwEnrollNumber, int dwEMachineNumber, int dwBackupNumber, bool bFlag)
        {
            throw new NotImplementedException();
        }

        public bool GetDeviceStatus(int dwMachineNumber, int dwStatus, ref int dwValue)
        {
            throw new NotImplementedException();
        }

        public bool GetDeviceInfo(int dwMachineNumber, int dwInfo, ref int dwValue)
        {
            throw new NotImplementedException();
        }

        public bool SetDeviceInfo(int dwMachineNumber, int dwInfo, int dwValue)
        {
            throw new NotImplementedException();
        }

        public bool SetDeviceTime(int dwMachineNumber)
        {
            throw new NotImplementedException();
        }

        public void PowerOnAllDevice()
        {
            throw new NotImplementedException();
        }

        public bool ModifyPrivilege(int dwMachineNumber, int dwEnrollNumber, int dwEMachineNumber, int dwBackupNumber,
            int dwMachinePrivilege)
        {
            throw new NotImplementedException();
        }

        public bool GetEnrollData(int dwMachineNumber, int dwEnrollNumber, int dwEMachineNumber, int dwBackupNumber,
            ref int dwMachinePrivilege, ref int dwEnrollData, ref int dwPassWord)
        {
            throw new NotImplementedException();
        }

        public bool SetEnrollData(int dwMachineNumber, int dwEnrollNumber, int dwEMachineNumber, int dwBackupNumber,
            int dwMachinePrivilege, ref int dwEnrollData, int dwPassWord)
        {
            throw new NotImplementedException();
        }

        public bool GetGeneralLogData(int dwMachineNumber, ref int dwTMachineNumber, ref int dwEnrollNumber, ref int dwEMachineNumber,
            ref int dwVerifyMode, ref int dwInOutMode, ref int dwYear, ref int dwMonth, ref int dwDay, ref int dwHour,
            ref int dwMinute)
        {
            throw new NotImplementedException();
        }

        public bool GetSuperLogData(int dwMachineNumber, ref int dwTMachineNumber, ref int dwSEnrollNumber, ref int Params4,
            ref int Params1, ref int Params2, ref int dwManipulation, ref int Params3, ref int dwYear, ref int dwMonth,
            ref int dwDay, ref int dwHour, ref int dwMinute)
        {
            throw new NotImplementedException();
        }

        public void ConvertPassword(int dwSrcPSW, ref int dwDestPSW, int dwLength)
        {
            throw new NotImplementedException();
        }

        public bool ClearKeeperData(int dwMachineNumber)
        {
            throw new NotImplementedException();
        }

        public int GetBackupNumber(int dwMachineNumber)
        {
            throw new NotImplementedException();
        }

        public bool GetProductCode(int dwMachineNumber, [UnscopedRef] out string lpszProductCode)
        {
            throw new NotImplementedException();
        }

        public int GetFPTempLength(ref byte dwEnrollData)
        {
            throw new NotImplementedException();
        }

        public bool Connect_Com(int ComPort, int MachineNumber, int BaudRate)
        {
            throw new NotImplementedException();
        }

        public bool SetUserInfo(int dwMachineNumber, int dwEnrollNumber, string Name, string Password, int Privilege, bool Enabled)
        {
            throw new NotImplementedException();
        }

        public bool SetDeviceIP(int dwMachineNumber, string IPAddr)
        {
            throw new NotImplementedException();
        }

        public bool GetDeviceIP(int dwMachineNumber, ref string IPAddr)
        {
            throw new NotImplementedException();
        }

        public bool SetUserTmp(int dwMachineNumber, int dwEnrollNumber, int dwFingerIndex, ref byte TmpData)
        {
            throw new NotImplementedException();
        }

        public bool FPTempConvert(ref byte TmpData1, ref byte TmpData2, ref int Size)
        {
            throw new NotImplementedException();
        }

        public bool SetCommPassword(int CommKey)
        {
            throw new NotImplementedException();
        }

        public bool GetUserGroup(int dwMachineNumber, int dwEnrollNumber, ref int UserGrp)
        {
            throw new NotImplementedException();
        }

        public bool SetUserGroup(int dwMachineNumber, int dwEnrollNumber, int UserGrp)
        {
            throw new NotImplementedException();
        }

        public bool GetTZInfo(int dwMachineNumber, int TZIndex, ref string TZ)
        {
            throw new NotImplementedException();
        }

        public bool SetTZInfo(int dwMachineNumber, int TZIndex, string TZ)
        {
            throw new NotImplementedException();
        }

        public bool GetUnlockGroups(int dwMachineNumber, ref string Grps)
        {
            throw new NotImplementedException();
        }

        public bool SetUnlockGroups(int dwMachineNumber, string Grps)
        {
            throw new NotImplementedException();
        }

        public bool GetGroupTZs(int dwMachineNumber, int GroupIndex, ref int TZs)
        {
            throw new NotImplementedException();
        }

        public bool SetGroupTZs(int dwMachineNumber, int GroupIndex, ref int TZs)
        {
            throw new NotImplementedException();
        }

        public bool GetUserTZs(int dwMachineNumber, int dwEnrollNumber, ref int TZs)
        {
            throw new NotImplementedException();
        }

        public bool SetUserTZs(int dwMachineNumber, int dwEnrollNumber, ref int TZs)
        {
            throw new NotImplementedException();
        }

        public bool ACUnlock(int dwMachineNumber, int Delay)
        {
            throw new NotImplementedException();
        }

        public bool GetACFun(ref int ACFun)
        {
            throw new NotImplementedException();
        }

        public bool GetGeneralLogDataStr(int dwMachineNumber, ref int dwEnrollNumber, ref int dwVerifyMode, ref int dwInOutMode,
            ref string TimeStr)
        {
            throw new NotImplementedException();
        }

        public bool GetUserTmpStr(int dwMachineNumber, int dwEnrollNumber, int dwFingerIndex, ref string TmpData, ref int TmpLength)
        {
            throw new NotImplementedException();
        }

        public bool SetUserTmpStr(int dwMachineNumber, int dwEnrollNumber, int dwFingerIndex, string TmpData)
        {
            throw new NotImplementedException();
        }

        public bool GetEnrollDataStr(int dwMachineNumber, int dwEnrollNumber, int dwEMachineNumber, int dwBackupNumber,
            ref int dwMachinePrivilege, ref string dwEnrollData, ref int dwPassWord)
        {
            throw new NotImplementedException();
        }

        public bool SetEnrollDataStr(int dwMachineNumber, int dwEnrollNumber, int dwEMachineNumber, int dwBackupNumber,
            int dwMachinePrivilege, string dwEnrollData, int dwPassWord)
        {
            throw new NotImplementedException();
        }

        public bool GetGroupTZStr(int dwMachineNumber, int GroupIndex, ref string TZs)
        {
            throw new NotImplementedException();
        }

        public bool SetGroupTZStr(int dwMachineNumber, int GroupIndex, string TZs)
        {
            throw new NotImplementedException();
        }

        public bool GetUserTZStr(int dwMachineNumber, int dwEnrollNumber, ref string TZs)
        {
            throw new NotImplementedException();
        }

        public bool SetUserTZStr(int dwMachineNumber, int dwEnrollNumber, string TZs)
        {
            throw new NotImplementedException();
        }

        public bool FPTempConvertStr(string TmpData1, ref string TmpData2, ref int Size)
        {
            throw new NotImplementedException();
        }

        public int GetFPTempLengthStr(string dwEnrollData)
        {
            throw new NotImplementedException();
        }

        public bool GetUserInfoByPIN2(int dwMachineNumber, ref string Name, ref string Password, ref int Privilege, ref bool Enabled)
        {
            throw new NotImplementedException();
        }

        public bool GetUserInfoByCard(int dwMachineNumber, ref string Name, ref string Password, ref int Privilege, ref bool Enabled)
        {
            throw new NotImplementedException();
        }

        public bool CaptureImage(bool FullImage, ref int Width, ref int Height, ref byte Image, string ImageFile)
        {
            throw new NotImplementedException();
        }

        public bool UpdateFirmware(string FirmwareFile)
        {
            throw new NotImplementedException();
        }

        public bool StartVerify(int UserID, int FingerID)
        {
            throw new NotImplementedException();
        }

        public bool CancelOperation()
        {
            throw new NotImplementedException();
        }

        public bool BackupData(string DataFile)
        {
            throw new NotImplementedException();
        }

        public bool RestoreData(string DataFile)
        {
            throw new NotImplementedException();
        }

        public bool WriteLCD(int Row, int Col, string Text)
        {
            throw new NotImplementedException();
        }

        public bool ClearLCD()
        {
            throw new NotImplementedException();
        }

        public bool PlayVoiceByIndex(int Index)
        {
            throw new NotImplementedException();
        }

        public bool EnableClock(int Enabled)
        {
            throw new NotImplementedException();
        }

        public bool GetUserIDByPIN2(int PIN2, ref int UserID)
        {
            throw new NotImplementedException();
        }

        public bool GetPIN2(int UserID, ref int PIN2)
        {
            throw new NotImplementedException();
        }

        public bool FPTempConvertNew(ref byte TmpData1, ref byte TmpData2, ref int Size)
        {
            throw new NotImplementedException();
        }

        public bool FPTempConvertNewStr(string TmpData1, ref string TmpData2, ref int Size)
        {
            throw new NotImplementedException();
        }

        public bool SetDeviceTime2(int dwMachineNumber, int dwYear, int dwMonth, int dwDay, int dwHour, int dwMinute, int dwSecond)
        {
            throw new NotImplementedException();
        }

        public bool ClearSLog(int dwMachineNumber)
        {
            throw new NotImplementedException();
        }

        public bool SetDeviceMAC(int dwMachineNumber, string sMAC)
        {
            throw new NotImplementedException();
        }

        public bool SetWiegandFmt(int dwMachineNumber, string sWiegandFmt)
        {
            throw new NotImplementedException();
        }

        public bool ClearSMS(int dwMachineNumber)
        {
            throw new NotImplementedException();
        }

        public bool GetSMS(int dwMachineNumber, int ID, ref int Tag, ref int ValidMinutes, ref string StartTime, ref string Content)
        {
            throw new NotImplementedException();
        }

        public bool SetSMS(int dwMachineNumber, int ID, int Tag, int ValidMinutes, string StartTime, string Content)
        {
            throw new NotImplementedException();
        }

        public bool DeleteSMS(int dwMachineNumber, int ID)
        {
            throw new NotImplementedException();
        }

        public bool SetUserSMS(int dwMachineNumber, int dwEnrollNumber, int SMSID)
        {
            throw new NotImplementedException();
        }

        public bool DeleteUserSMS(int dwMachineNumber, int dwEnrollNumber, int SMSID)
        {
            throw new NotImplementedException();
        }

        public bool GetCardFun(int dwMachineNumber, ref int CardFun)
        {
            throw new NotImplementedException();
        }

        public bool ClearUserSMS(int dwMachineNumber)
        {
            throw new NotImplementedException();
        }

        public bool SetDeviceCommPwd(int dwMachineNumber, int CommKey)
        {
            throw new NotImplementedException();
        }

        public bool GetDoorState(int MachineNumber, ref int State)
        {
            throw new NotImplementedException();
        }

        public bool GetSensorSN(int dwMachineNumber, ref string SensorSN)
        {
            throw new NotImplementedException();
        }

        public bool ReadCustData(int dwMachineNumber, ref string CustData)
        {
            throw new NotImplementedException();
        }

        public bool WriteCustData(int dwMachineNumber, string CustData)
        {
            throw new NotImplementedException();
        }

        public bool GetDataFile(int dwMachineNumber, int DataFlag, string FileName)
        {
            throw new NotImplementedException();
        }

        public bool WriteCard(int dwMachineNumber, int dwEnrollNumber, int dwFingerIndex1, ref byte TmpData1, int dwFingerIndex2,
            ref byte TmpData2, int dwFingerIndex3, ref byte TmpData3, int dwFingerIndex4, ref byte TmpData4)
        {
            throw new NotImplementedException();
        }

        public bool GetGeneralExtLogData(int dwMachineNumber, ref int dwEnrollNumber, ref int dwVerifyMode, ref int dwInOutMode,
            ref int dwYear, ref int dwMonth, ref int dwDay, ref int dwHour, ref int dwMinute, ref int dwSecond,
            ref int dwWorkCode, ref int dwReserved)
        {
            throw new NotImplementedException();
        }

        public bool EmptyCard(int dwMachineNumber)
        {
            throw new NotImplementedException();
        }

        public bool GetDeviceStrInfo(int dwMachineNumber, int dwInfo, [UnscopedRef] out string Value)
        {
            throw new NotImplementedException();
        }

        public bool GetSysOption(int dwMachineNumber, string Option, [UnscopedRef] out string Value)
        {
            throw new NotImplementedException();
        }

        public bool SetUserInfoEx(int dwMachineNumber, int dwEnrollNumber, int VerifyStyle, ref byte Reserved)
        {
            throw new NotImplementedException();
        }

        public bool DeleteUserInfoEx(int dwMachineNumber, int dwEnrollNumber)
        {
            throw new NotImplementedException();
        }

        public bool SSR_GetUserTmp(int dwMachineNumber, string dwEnrollNumber, int dwFingerIndex, [UnscopedRef] out byte TmpData,
            [UnscopedRef] out int TmpLength)
        {
            throw new NotImplementedException();
        }

        public bool SSR_DeleteEnrollData(int dwMachineNumber, string dwEnrollNumber, int dwBackupNumber)
        {
            throw new NotImplementedException();
        }

        public bool SSR_SetUserTmp(int dwMachineNumber, string dwEnrollNumber, int dwFingerIndex, ref byte TmpData)
        {
            throw new NotImplementedException();
        }

        public bool SSR_SetUserTmpStr(int dwMachineNumber, string dwEnrollNumber, int dwFingerIndex, string TmpData)
        {
            throw new NotImplementedException();
        }

        public bool SSR_DelUserTmp(int dwMachineNumber, string dwEnrollNumber, int dwFingerIndex)
        {
            throw new NotImplementedException();
        }

        public bool SetWorkCode(int WorkCodeID, int AWorkCode)
        {
            throw new NotImplementedException();
        }

        public bool GetWorkCode(int WorkCodeID, [UnscopedRef] out int AWorkCode)
        {
            throw new NotImplementedException();
        }

        public bool DeleteWorkCode(int WorkCodeID)
        {
            throw new NotImplementedException();
        }

        public bool ClearWorkCode()
        {
            throw new NotImplementedException();
        }

        public bool ReadAttRule(int dwMachineNumber)
        {
            throw new NotImplementedException();
        }

        public bool ReadDPTInfo(int dwMachineNumber)
        {
            throw new NotImplementedException();
        }

        public bool ReadTurnInfo(int dwMachineNumber)
        {
            throw new NotImplementedException();
        }

        public bool SSR_OutPutHTMLRep(int dwMachineNumber, string dwEnrollNumber, string AttFile, string UserFile, string DeptFile,
            string TimeClassFile, string AttruleFile, int BYear, int BMonth, int BDay, int BHour, int BMinute, int BSecond,
            int EYear, int EMonth, int EDay, int EHour, int EMinute, int ESecond, string TempPath, string OutFileName,
            int HTMLFlag, int resv1, string resv2)
        {
            throw new NotImplementedException();
        }

        public bool ReadAOptions(string AOption, [UnscopedRef] out string AValue)
        {
            throw new NotImplementedException();
        }

        public bool ReadRTLog(int dwMachineNumber)
        {
            throw new NotImplementedException();
        }

        public bool GetRTLog(int dwMachineNumber)
        {
            throw new NotImplementedException();
        }

        public bool GetHIDEventCardNumAsStr([UnscopedRef] out string strHIDEventCardNum)
        {
            throw new NotImplementedException();
        }

        public bool GetStrCardNumber([UnscopedRef] out string ACardNumber)
        {
            throw new NotImplementedException();
        }

        public bool SetStrCardNumber(string ACardNumber)
        {
            throw new NotImplementedException();
        }

        public bool CancelBatchUpdate(int dwMachineNumber)
        {
            throw new NotImplementedException();
        }

        public bool SetSysOption(int dwMachineNumber, string Option, string Value)
        {
            throw new NotImplementedException();
        }

        public bool Connect_Modem(int ComPort, int MachineNumber, int BaudRate, string Telephone)
        {
            throw new NotImplementedException();
        }

        public bool UseGroupTimeZone()
        {
            throw new NotImplementedException();
        }

        public bool SetHoliday(int dwMachineNumber, string Holiday)
        {
            throw new NotImplementedException();
        }

        public bool GetHoliday(int dwMachineNumber, ref string Holiday)
        {
            throw new NotImplementedException();
        }

        public bool SetDaylight(int dwMachineNumber, int Support, string BeginTime, string EndTime)
        {
            throw new NotImplementedException();
        }

        public bool GetDaylight(int dwMachineNumber, ref int Support, ref string BeginTime, ref string EndTime)
        {
            throw new NotImplementedException();
        }

        public bool SSR_SetUnLockGroup(int dwMachineNumber, int CombNo, int Group1, int Group2, int Group3, int Group4, int Group5)
        {
            throw new NotImplementedException();
        }

        public bool SSR_GetUnLockGroup(int dwMachineNumber, int CombNo, ref int Group1, ref int Group2, ref int Group3, ref int Group4,
            ref int Group5)
        {
            throw new NotImplementedException();
        }

        public bool SSR_SetGroupTZ(int dwMachineNumber, int GroupNo, int Tz1, int Tz2, int Tz3, int VaildHoliday, int VerifyStyle)
        {
            throw new NotImplementedException();
        }

        public bool SSR_GetGroupTZ(int dwMachineNumber, int GroupNo, ref int Tz1, ref int Tz2, ref int Tz3, ref int VaildHoliday,
            ref int VerifyStyle)
        {
            throw new NotImplementedException();
        }

        public bool SSR_GetHoliday(int dwMachineNumber, int HolidayID, ref int BeginMonth, ref int BeginDay, ref int EndMonth,
            ref int EndDay, ref int TimeZoneID)
        {
            throw new NotImplementedException();
        }

        public bool SSR_SetHoliday(int dwMachineNumber, int HolidayID, int BeginMonth, int BeginDay, int EndMonth, int EndDay,
            int TimeZoneID)
        {
            throw new NotImplementedException();
        }

        public bool GetPlatform(int dwMachineNumber, ref string Platform)
        {
            throw new NotImplementedException();
        }

        public bool SSR_SetUserSMS(int dwMachineNumber, string dwEnrollNumber, int SMSID)
        {
            throw new NotImplementedException();
        }

        public bool SSR_DeleteUserSMS(int dwMachineNumber, string dwEnrollNumber, int SMSID)
        {
            throw new NotImplementedException();
        }

        public bool IsTFTMachine(int dwMachineNumber)
        {
            throw new NotImplementedException();
        }

        public bool SendCMDMsg(int dwMachineNumber, int Param1, int Param2)
        {
            throw new NotImplementedException();
        }

        public bool SendFile(int dwMachineNumber, string FileName)
        {
            throw new NotImplementedException();
        }

        public bool SetLanguageByID(int dwMachineNumber, int LanguageID, string Language)
        {
            throw new NotImplementedException();
        }

        public bool ReadFile(int dwMachineNumber, string FileName, string FilePath)
        {
            throw new NotImplementedException();
        }

        public bool SetLastCount(int count)
        {
            throw new NotImplementedException();
        }

        public bool SetCustomizeAttState(int dwMachineNumber, int StateID, int NewState)
        {
            throw new NotImplementedException();
        }

        public bool DelCustomizeAttState(int dwMachineNumber, int StateID)
        {
            throw new NotImplementedException();
        }

        public bool EnableCustomizeAttState(int dwMachineNumber, int StateID, int Enable)
        {
            throw new NotImplementedException();
        }

        public bool SetCustomizeVoice(int dwMachineNumber, int VoiceID, string FileName)
        {
            throw new NotImplementedException();
        }

        public bool DelCustomizeVoice(int dwMachineNumber, int VoiceID)
        {
            throw new NotImplementedException();
        }

        public bool EnableCustomizeVoice(int dwMachineNumber, int VoiceID, int Enable)
        {
            throw new NotImplementedException();
        }

        public bool SSR_SetUserTmpExt(int dwMachineNumber, int IsDeleted, string dwEnrollNumber, int dwFingerIndex, ref byte TmpData)
        {
            throw new NotImplementedException();
        }

        public bool SSR_DelUserTmpExt(int dwMachineNumber, string dwEnrollNumber, int dwFingerIndex)
        {
            throw new NotImplementedException();
        }

        public bool SSR_DeleteEnrollDataExt(int dwMachineNumber, string dwEnrollNumber, int dwBackupNumber)
        {
            throw new NotImplementedException();
        }

        public bool SSR_GetWorkCode(int AWorkCode, [UnscopedRef] out string Name)
        {
            throw new NotImplementedException();
        }

        public bool SSR_SetWorkCode(int AWorkCode, string Name)
        {
            throw new NotImplementedException();
        }

        public bool SSR_DeleteWorkCode(int PIN)
        {
            throw new NotImplementedException();
        }

        public bool SSR_ClearWorkCode()
        {
            throw new NotImplementedException();
        }

        public bool SSR_GetSuperLogData(int MachineNumber, [UnscopedRef] out int Number, [UnscopedRef] out string Admin,
            [UnscopedRef] out string User, [UnscopedRef] out int Manipulation, [UnscopedRef] out string Time,
            [UnscopedRef] out int Params1, [UnscopedRef] out int Params2, [UnscopedRef] out int Params3)
        {
            throw new NotImplementedException();
        }

        public bool SSR_SetShortkey(int ShortKeyID, int ShortKeyFun, int StateCode, string StateName, int StateAutoChange,
            string StateAutoChangeTime)
        {
            throw new NotImplementedException();
        }

        public bool SSR_GetShortkey(int ShortKeyID, ref int ShortKeyFun, ref int StateCode, ref string StateName, ref int AutoChange,
            ref string AutoChangeTime)
        {
            throw new NotImplementedException();
        }

        public bool Connect_USB(int MachineNumber)
        {
            throw new NotImplementedException();
        }

        public bool GetSuperLogData2(int dwMachineNumber, ref int dwTMachineNumber, ref int dwSEnrollNumber, ref int Params4,
            ref int Params1, ref int Params2, ref int dwManipulation, ref int Params3, ref int dwYear, ref int dwMonth,
            ref int dwDay, ref int dwHour, ref int dwMinute, ref int dwSecs)
        {
            throw new NotImplementedException();
        }

        public bool GetUserFace(int dwMachineNumber, string dwEnrollNumber, int dwFaceIndex, ref byte TmpData, ref int TmpLength)
        {
            throw new NotImplementedException();
        }

        public bool SetUserFace(int dwMachineNumber, string dwEnrollNumber, int dwFaceIndex, ref byte TmpData, int TmpLength)
        {
            throw new NotImplementedException();
        }

        public bool DelUserFace(int dwMachineNumber, string dwEnrollNumber, int dwFaceIndex)
        {
            throw new NotImplementedException();
        }

        public bool GetUserFaceStr(int dwMachineNumber, string dwEnrollNumber, int dwFaceIndex, ref string TmpData, ref int TmpLength)
        {
            throw new NotImplementedException();
        }

        public bool SetUserFaceStr(int dwMachineNumber, string dwEnrollNumber, int dwFaceIndex, string TmpData, int TmpLength)
        {
            throw new NotImplementedException();
        }

        public bool SetUserTmpEx(int dwMachineNumber, string dwEnrollNumber, int dwFingerIndex, int Flag, ref byte TmpData)
        {
            throw new NotImplementedException();
        }

        public bool MergeTemplate(IntPtr Templates, int FingerCount, ref byte TemplateDest, ref int FingerSize)
        {
            throw new NotImplementedException();
        }

        public bool SplitTemplate(ref byte Template, IntPtr Templates, ref int FingerCount, ref int FingerSize)
        {
            throw new NotImplementedException();
        }

        public bool ReadUserAllTemplate(int dwMachineNumber, string dwEnrollNumber)
        {
            throw new NotImplementedException();
        }

        public bool UpdateFile(string FileName)
        {
            throw new NotImplementedException();
        }

        public bool ReadLastestLogData(int dwMachineNumber, int NewLog, int dwYear, int dwMonth, int dwDay, int dwHour, int dwMinute,
            int dwSecond)
        {
            throw new NotImplementedException();
        }

        public bool SetOptionCommPwd(int dwMachineNumber, string CommKey)
        {
            throw new NotImplementedException();
        }

        public bool ReadSuperLogDataEx(int dwMachineNumber, int dwYear_S, int dwMonth_S, int dwDay_S, int dwHour_S, int dwMinute_S,
            int dwSecond_S, int dwYear_E, int dwMonth_E, int dwDay_E, int dwHour_E, int dwMinute_E, int dwSecond_E,
            int dwLogIndex)
        {
            throw new NotImplementedException();
        }

        public bool GetSuperLogDataEx(int dwMachineNumber, ref string EnrollNumber, ref int Params4, ref int Params1, ref int Params2,
            ref int dwManipulation, ref int Params3, ref int dwYear, ref int dwMonth, ref int dwDay, ref int dwHour,
            ref int dwMinute, ref int dwSecond)
        {
            throw new NotImplementedException();
        }

        public bool GetPhotoByName(int dwMachineNumber, string PhotoName, [UnscopedRef] out byte PhotoData,
            [UnscopedRef] out int PhotoLength)
        {
            throw new NotImplementedException();
        }

        public bool GetPhotoNamesByTime(int dwMachineNumber, int iFlag, string sTime, string eTime,
            [UnscopedRef] out string AllPhotoName)
        {
            throw new NotImplementedException();
        }

        public bool ClearPhotoByTime(int dwMachineNumber, int iFlag, string sTime, string eTime)
        {
            throw new NotImplementedException();
        }

        public bool GetPhotoCount(int dwMachineNumber, [UnscopedRef] out int count, int iFlag)
        {
            throw new NotImplementedException();
        }

        public bool ClearDataEx(int dwMachineNumber, string TableName)
        {
            throw new NotImplementedException();
        }

        public bool GetDataFileEx(int dwMachineNumber, string SourceFileName, string DestFileName)
        {
            throw new NotImplementedException();
        }

        public bool SSR_SetDeviceData(int dwMachineNumber, string TableName, string Datas, string Options)
        {
            throw new NotImplementedException();
        }

        public bool SSR_GetDeviceData(int dwMachineNumber, [UnscopedRef] out string Buffer, int BufferSize, string TableName,
            string FiledNames, string Filter, string Options)
        {
            throw new NotImplementedException();
        }

        public bool UpdateLogo(int dwMachineNumber, string FileName)
        {
            throw new NotImplementedException();
        }

        public bool SetCommuTimeOut(int timeOut)
        {
            throw new NotImplementedException();
        }

        public bool SendFileByType(int dwMachineNumber, string FileName, int iType)
        {
            throw new NotImplementedException();
        }

        public bool SetCommProType(int proType)
        {
            throw new NotImplementedException();
        }

        public bool SetCompatOldFirmware(int nCompatOkdFirmware)
        {
            throw new NotImplementedException();
        }

        public bool Connect_P4P(string uid)
        {
            throw new NotImplementedException();
        }

        public bool SetDeviceTableData(int dwMachineNumber, string TableName, string Datas, string Options,
            [UnscopedRef] out int count)
        {
            throw new NotImplementedException();
        }

        public bool SetManufacturerData(int dwMachineNumber, string Name, string Value)
        {
            throw new NotImplementedException();
        }

        public int GetDeviceStatusEx(int dwMachineNumber)
        {
            throw new NotImplementedException();
        }

        public void CancelByUser()
        {
            throw new NotImplementedException();
        }

        public int SSR_GetDeviceDataCount(string TableName, string Filter, string Options)
        {
            throw new NotImplementedException();
        }

        public bool SSR_DeleteDeviceData(int dwMachineNumber, string TableName, string Datas, string Options)
        {
            throw new NotImplementedException();
        }

        public bool ReadTimeGLogData(int dwMachineNumber, string sTime, string eTime)
        {
            throw new NotImplementedException();
        }

        public bool DeleteAttlogBetweenTheDate(int dwMachineNumber, string sTime, string eTime)
        {
            throw new NotImplementedException();
        }

        public bool DeleteAttlogByTime(int dwMachineNumber, string sTime)
        {
            throw new NotImplementedException();
        }

        public bool ReadNewGLogData(int dwMachineNumber)
        {
            throw new NotImplementedException();
        }

        public bool IsNewFirmwareMachine(int dwMachineNumber)
        {
            throw new NotImplementedException();
        }

        public bool UploadUserPhoto(int dwMachineNumber, string FileName)
        {
            throw new NotImplementedException();
        }

        public bool DownloadUserPhoto(int dwMachineNumber, string FileName, string FilePath)
        {
            throw new NotImplementedException();
        }

        public bool DeleteUserPhoto(int dwMachineNumber, string FileName)
        {
            throw new NotImplementedException();
        }

        public bool GetAllUserPhoto(int dwMachineNumber, string dlDir)
        {
            throw new NotImplementedException();
        }

        public bool SetBellSchDataEx(int dwMachineNumber, int weekDay, int Index, int Enable, int Hour, int min, int voice, int way,
            int InerBellDelay, int ExtBellDelay)
        {
            throw new NotImplementedException();
        }

        public bool GetBellSchDataEx(int dwMachineNumber, int weekDay, int Index, [UnscopedRef] out int Enable,
            [UnscopedRef] out int Hour, [UnscopedRef] out int min, [UnscopedRef] out int voice, [UnscopedRef] out int way,
            [UnscopedRef] out int InerBellDelay, [UnscopedRef] out int ExtBellDelay)
        {
            throw new NotImplementedException();
        }

        public bool GetDayBellSchCount(int dwMachineNumber, [UnscopedRef] out int DayBellCnt)
        {
            throw new NotImplementedException();
        }

        public bool GetMaxBellIDInBellSchData(int dwMachineNumber, [UnscopedRef] out int MaxBellID)
        {
            throw new NotImplementedException();
        }

        public bool ReadAllBellSchData(int dwMachineNumber)
        {
            throw new NotImplementedException();
        }

        public bool GetEachBellInfo(int dwMachineNumber, [UnscopedRef] out int weekDay, [UnscopedRef] out int Index,
            [UnscopedRef] out int Enable, [UnscopedRef] out int Hour, [UnscopedRef] out int min, [UnscopedRef] out int voice,
            [UnscopedRef] out int way, [UnscopedRef] out int InerBellDelay, [UnscopedRef] out int ExtBellDelay)
        {
            throw new NotImplementedException();
        }

        public bool SetUserValidDate(int dwMachineNumber, string UserID, int Expires, int ValidCount, string StartDate,
            string EndDate)
        {
            throw new NotImplementedException();
        }

        public bool GetUserValidDate(int dwMachineNumber, string UserID, [UnscopedRef] out int Expires,
            [UnscopedRef] out int ValidCount, [UnscopedRef] out string StartDate, [UnscopedRef] out string EndDate)
        {
            throw new NotImplementedException();
        }

        public bool SetUserValidDateBatch(int dwMachineNumber, string Datas)
        {
            throw new NotImplementedException();
        }

        public bool GetUserValidDateBatch(int dwMachineNumber, [UnscopedRef] out string Buffer, int BufferSize)
        {
            throw new NotImplementedException();
        }

        public bool SetUserVerifyStyle(int dwMachineNumber, string dwEnrollNumber, int VerifyStyle, ref byte Reserved)
        {
            throw new NotImplementedException();
        }

        public bool GetUserVerifyStyle(int dwMachineNumber, string dwEnrollNumber, [UnscopedRef] out int VerifyStyle,
            [UnscopedRef] out byte Reserved)
        {
            throw new NotImplementedException();
        }

        public bool SetUserVerifyStyleBatch(int dwMachineNumber, string Datas, ref byte Reserved)
        {
            throw new NotImplementedException();
        }

        public bool GetUserVerifyStyleBatch(int dwMachineNumber, [UnscopedRef] out string Buffer, int BufferSize,
            [UnscopedRef] out byte Reserved)
        {
            throw new NotImplementedException();
        }

        public bool GetDeviceFirmwareVersion(int dwMachineNumber, ref string strVersion)
        {
            throw new NotImplementedException();
        }

        public bool SendFileEx(int dwMachineNumber, string FileName, string FilePath)
        {
            throw new NotImplementedException();
        }

        public bool UploadTheme(int dwMachineNumber, string FileName, string InDevName)
        {
            throw new NotImplementedException();
        }

        public bool UploadPicture(int dwMachineNumber, string FileName, string InDevName)
        {
            throw new NotImplementedException();
        }

        public bool DeletePicture(int dwMachineNumber, string FileName)
        {
            throw new NotImplementedException();
        }

        public bool DownloadPicture(int dwMachineNumber, string FileName, string FilePath)
        {
            throw new NotImplementedException();
        }

        public bool TurnOffAlarm(int dwMachineNumber)
        {
            throw new NotImplementedException();
        }

        public bool CloseAlarm(int dwMachineNumber)
        {
            throw new NotImplementedException();
        }

        public bool SSR_SetWorkCodeEx(int dwMachineNumber, string WorkCodeNum, string Name)
        {
            throw new NotImplementedException();
        }

        public bool SSR_GetWorkCodeEx(int dwMachineNumber, string WorkCodeNum, [UnscopedRef] out string Name)
        {
            throw new NotImplementedException();
        }

        public bool SSR_DeleteWorkCodeEx(int dwMachineNumber, string WorkCodeNum)
        {
            throw new NotImplementedException();
        }

        public bool SSR_GetGeneralLogDataEx(int dwMachineNumber, [UnscopedRef] out string dwEnrollNumber,
            [UnscopedRef] out int dwVerifyMode, [UnscopedRef] out int dwInOutMode, [UnscopedRef] out int dwYear,
            [UnscopedRef] out int dwMonth, [UnscopedRef] out int dwDay, [UnscopedRef] out int dwHour,
            [UnscopedRef] out int dwMinute, [UnscopedRef] out int dwSecond, [UnscopedRef] out string dwWorkCode)
        {
            throw new NotImplementedException();
        }

        public bool SSR_SetWorkCodeExBatch(int dwMachineNumber, string Datas)
        {
            throw new NotImplementedException();
        }

        public bool SSR_GetWorkCodeExBatch(int dwMachineNumber, [UnscopedRef] out string Buffer, int BufferSize)
        {
            throw new NotImplementedException();
        }

        public bool SSR_GetWorkCodeExByID(int dwMachineNumber, int WorkCodeID, [UnscopedRef] out string WorkCodeNum,
            [UnscopedRef] out string Name)
        {
            throw new NotImplementedException();
        }

        public bool SSR_GetWorkCodeIDByName(int dwMachineNumber, string workcodeName, [UnscopedRef] out int WorkCodeID)
        {
            throw new NotImplementedException();
        }

        public bool SetEventMode(int nType)
        {
            throw new NotImplementedException();
        }

        public bool GetAllSFIDName(int dwMachineNumber, [UnscopedRef] out string ShortkeyIDName, int BufferSize1,
            [UnscopedRef] out string FunctionIDName, int BufferSize2)
        {
            throw new NotImplementedException();
        }

        public bool SetShortkey(int dwMachineNumber, int ShortKeyID, string ShortKeyName, string FunctionName, int ShortKeyFun,
            int StateCode, string StateName, string Description, int StateAutoChange, string StateAutoChangeTime)
        {
            throw new NotImplementedException();
        }

        public bool GetShortkey(int dwMachineNumber, int ShortKeyID, ref string ShortKeyName, ref string FunctionName,
            ref int ShortKeyFun, ref int StateCode, ref string StateName, ref string Description, ref int AutoChange,
            ref string AutoChangeTime)
        {
            throw new NotImplementedException();
        }

        public bool GetAllAppFun(int dwMachineNumber, [UnscopedRef] out string AppName, [UnscopedRef] out string FunofAppName)
        {
            throw new NotImplementedException();
        }

        public bool GetAllRole(int dwMachineNumber, [UnscopedRef] out string RoleName)
        {
            throw new NotImplementedException();
        }

        public bool GetAppOfRole(int dwMachineNumber, int Permission, [UnscopedRef] out string AppName)
        {
            throw new NotImplementedException();
        }

        public bool GetFunOfRole(int dwMachineNumber, int Permission, [UnscopedRef] out string FunName)
        {
            throw new NotImplementedException();
        }

        public bool SetPermOfAppFun(int dwMachineNumber, int Permission, string AppName, string FunName)
        {
            throw new NotImplementedException();
        }

        public bool DeletePermOfAppFun(int dwMachineNumber, int Permission, string AppName, string FunName)
        {
            throw new NotImplementedException();
        }

        public bool IsUserDefRoleEnable(int dwMachineNumber, int Permission, [UnscopedRef] out bool Enable)
        {
            throw new NotImplementedException();
        }

        public bool SearchDevice(string commType, string address, [UnscopedRef] out string DevBuffer, int DevBufferSize)
        {
            throw new NotImplementedException();
        }

        public bool SetUserIDCardInfo(int dwMachineNumber, string strEnrollNumber, ref byte IDCardData, int DataLen)
        {
            throw new NotImplementedException();
        }

        public bool GetUserIDCardInfo(int dwMachineNumber, string strEnrollNumber, [UnscopedRef] out byte IDCardData, ref int DataLen)
        {
            throw new NotImplementedException();
        }

        public bool DelUserIDCardInfo(int dwMachineNumber, string strEnrollNumber)
        {
            throw new NotImplementedException();
        }

        public bool GetPhotoByNameToFile(int dwMachineNumber, string PhotoName, string LocalFileName)
        {
            throw new NotImplementedException();
        }

        public bool SendUserFacePhoto(int dwMachineNumber, string FileName)
        {
            throw new NotImplementedException();
        }

        public bool GetUserFacePhotoNames(int dwMachineNumber, [UnscopedRef] out string AllPhotoName)
        {
            throw new NotImplementedException();
        }

        public bool GetUserFacePhotoCount(int dwMachineNumber, [UnscopedRef] out int count)
        {
            throw new NotImplementedException();
        }

        public bool GetUserFacePhotoByName(int dwMachineNumber, string PhotoName, [UnscopedRef] out byte PhotoData,
            [UnscopedRef] out int PhotoLength)
        {
            throw new NotImplementedException();
        }

        public bool SetUserInfoPR(int dwMachineNumber, bool IsSameUser, string dwEnrollNumber, string Name, string Remark, string Rank,
            string Photo)
        {
            throw new NotImplementedException();
        }

        public bool ClearDram(int dwMachineNumber)
        {
            throw new NotImplementedException();
        }

        public bool SSR_GetGeneralLogDataWithMask(int dwMachineNumber, [UnscopedRef] out string dwEnrollNumber,
            [UnscopedRef] out int dwVerifyMode, [UnscopedRef] out int dwInOutMode, [UnscopedRef] out int dwYear,
            [UnscopedRef] out int dwMonth, [UnscopedRef] out int dwDay, [UnscopedRef] out int dwHour,
            [UnscopedRef] out int dwMinute, [UnscopedRef] out int dwSecond, ref int dwWorkCode, [UnscopedRef] out int dwMask,
            [UnscopedRef] out string dwTemperature)
        {
            throw new NotImplementedException();
        }

        public bool SaveThermalImage(int dwMachineNumber)
        {
            throw new NotImplementedException();
        }

        public bool SendFileByProduce(int dwMachineNumber, string FileName)
        {
            throw new NotImplementedException();
        }

        public bool SSR_GetGeneralLogDataWithMaskEx(int dwMachineNumber, [UnscopedRef] out string dwEnrollNumber,
            [UnscopedRef] out int dwVerifyMode, [UnscopedRef] out int dwInOutMode, [UnscopedRef] out int dwYear,
            [UnscopedRef] out int dwMonth, [UnscopedRef] out int dwDay, [UnscopedRef] out int dwHour,
            [UnscopedRef] out int dwMinute, [UnscopedRef] out int dwSecond, ref int dwWorkCode, [UnscopedRef] out int dwMask,
            [UnscopedRef] out string dwTemperature, [UnscopedRef] out int dwhelmelhat)
        {
            throw new NotImplementedException();
        }

        public bool SaveThermalImage_V2(int dwMachineNumber, string Reserved)
        {
            throw new NotImplementedException();
        }

        public bool GetUserFacePhotoByNameEx(int dwMachineNumber, string PhotoName, [UnscopedRef] out string PhotoData,
            [UnscopedRef] out int PhotoLength)
        {
            throw new NotImplementedException();
        }

        public bool UploadUserPhotoDataStr(int dwMachineNumber, string FileName, string FileDataStr, int DataLen)
        {
            throw new NotImplementedException();
        }

        public bool DownloadUserPhotoDataStr(int dwMachineNumber, string FileName, [UnscopedRef] out string FileDataStr,
            [UnscopedRef] out int DataLen)
        {
            throw new NotImplementedException();
        }

        public bool SetCommPasswordEx(string CommKey)
        {
            throw new NotImplementedException();
        }

        public bool ReadMark { get; set; }
        public int CommPort { get; set; }
        public int ConvertBIG5 { get; set; }
        public int BASE64 { get; set; }
        public uint PIN2 { get; set; }
        public int AccGroup { get; set; }
        public int get_AccTimeZones(int Index)
        {
            throw new NotImplementedException();
        }

        public void set_AccTimeZones(int Index, int pVal)
        {
            throw new NotImplementedException();
        }

        public int get_CardNumber(int Index)
        {
            throw new NotImplementedException();
        }

        public void set_CardNumber(int Index, int pVal)
        {
            throw new NotImplementedException();
        }

        public int PINWidth { get; }
        public int MachineNumber { get; set; }
        public string get_STR_CardNumber(int Index)
        {
            throw new NotImplementedException();
        }

        public void set_STR_CardNumber(int Index, string pVal)
        {
            throw new NotImplementedException();
        }

        public int SSRPin { get; }
        public int PullMode { get; set; }
        public int MaxP4PConnect { get; }
        public int BatchDataMode { get; set; }
        public int SecureMode { get; }

        // (Keep other unimplemented members as in original or prune if interface allows)
        #endregion
    }
}
