using GeneralDef;
using NETSDKHelper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static GeneralDef.NETDEMO;

namespace TranData.Driver
{
    public class PTZControl
    {
        private static PTZControl _instance;
        public static DeviceInfo DeviceInfo;
        public static TreeNodeInfo TreeNodeInfo = new TreeNodeInfo();
        public string VideoUrl
        {
            get
            {
                if (DeviceInfo != null && DeviceInfo.m_channelInfoList.Count > 0)
                {
                    return $"rtsp://{DeviceInfo.m_userName}:{DeviceInfo.m_password}@{DeviceInfo.m_ip}/media/video1";
                }
                return string.Empty;

            }
        }
        public static PTZControl Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PTZControl();
                }
                return _instance;
            }
        }
        int m_speed = 8;
        IntPtr CurrentHandle = IntPtr.Zero;

        private List<NETDEV_DISCOVERY_DEVINFO_S> oDeviceInfoList = new List<NETDEV_DISCOVERY_DEVINFO_S>();
        public NETDEVSDK.NETDEV_DISCOVERY_CALLBACK_PF discoveryCB = null;
        NETDEVSDK.NETDEV_ExceptionCallBack_PF excepCB = null;

        public PTZControl()
        {
            Discovery("0.0.0.0", "0.0.0.0");
        }

        public void Init()
        {
            excepCB = new NETDEVSDK.NETDEV_ExceptionCallBack_PF(exceptionCallBack);
        }

        public void exceptionCallBack(IntPtr lpUserID, Int32 dwType, IntPtr lpExpHandle, IntPtr lpUserData)
        {
            Debug.Fail($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}-----lpUserID:{lpUserID};dwType:{dwType};lpExpHandle:{lpExpHandle};");
        }


        public void Discovery(String strStartIP, String strEndIP)
        {
            if (strStartIP == "" || strEndIP == "")
            {
                return;
            }

            oDeviceInfoList.Clear();

            discoveryCB = new NETDEVSDK.NETDEV_DISCOVERY_CALLBACK_PF(DiscoveryCallBack);
            Int32 iRet = NETDEVSDK.NETDEV_SetDiscoveryCallBack(discoveryCB, IntPtr.Zero);
            if (NETDEVSDK.FALSE == iRet)
            {
                Debug.WriteLine("set discovery callBack failed,the error is [" + NETDEVSDK.NETDEV_GetLastError().ToString() + "]");
                return;
            }

            iRet = NETDEVSDK.NETDEV_Discovery(strStartIP, strEndIP);
            if (NETDEVSDK.FALSE == iRet)
            {
                Debug.WriteLine("discovery failed,the error is [" + NETDEVSDK.NETDEV_GetLastError().ToString() + "]");
                return;
            }
        }
        private void DiscoveryCallBack(IntPtr pstDevInfo, IntPtr lpUserData)
        {
            NETDEV_DISCOVERY_DEVINFO_S stDevInfo = (NETDEV_DISCOVERY_DEVINFO_S)Marshal.PtrToStructure(pstDevInfo, typeof(NETDEV_DISCOVERY_DEVINFO_S));
            for (int i = 0; i < oDeviceInfoList.Count(); i++)
            {
                if (stDevInfo.szDevAddr == oDeviceInfoList[i].szDevAddr && stDevInfo.szDevSerailNum == oDeviceInfoList[i].szDevSerailNum)
                {
                    return;
                }
            }

            oDeviceInfoList.Add(stDevInfo);
            if (stDevInfo.enDevType == NETDEV_DEVICETYPE_E.NETDEV_DTYPE_IPC)
            {
                Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var ipaddress = config.AppSettings.Settings["ipaddress"].Value;
                var ipcpass = config.AppSettings.Settings["ipcpass"].Value;
                var ipclogin = config.AppSettings.Settings["ipclogin"].Value;
                if (!string.IsNullOrEmpty(ipaddress) && stDevInfo.szDevAddr == ipaddress)
                {
                    DeviceInfo deviceInfoTemp = new DeviceInfo();
                    deviceInfoTemp.m_ip = stDevInfo.szDevAddr;
                    deviceInfoTemp.m_port = (short)stDevInfo.dwDevPort;
                    deviceInfoTemp.m_userName = ipclogin;
                    deviceInfoTemp.m_password = ipcpass;
                    deviceInfoTemp.m_eDeviceType = NETDEMO_DEVICE_TYPE_E.NETDEMO_DEVICE_IPC_OR_NVR;
                    DevLogin(deviceInfoTemp);
                    DeviceInfo = deviceInfoTemp;
                    StartRtc();
                }

            }
        }

        private void StartRtc()
        {
            try
            {
                Process proc = new Process();
                var path = AppDomain.CurrentDomain.BaseDirectory;
                var flieName = path + @"webrtc\webrtc.exe";
                var root = path + @"webrtc\html";

                proc.StartInfo.CreateNoWindow = false;
                proc.StartInfo.UseShellExecute = false;

                proc.StartInfo.FileName = flieName;
                proc.StartInfo.Arguments = $" -w {root} {VideoUrl}";
                proc.Start();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        public void DevLogin(DeviceInfo devInfo)
        {
            if (DeviceInfo != null
                && DeviceInfo.m_ip == devInfo.m_ip
                && DeviceInfo.m_port == devInfo.m_port
                )
            {
                return;
            }

            NETDEMO.NETDEV_LOGIN_TYPE_E loginFlag = NETDEMO.NETDEV_LOGIN_TYPE_E.NETDEV_NEW_LOGIN;
            NETDEV_DEVICE_LOGIN_INFO_S pstDevLoginInfo = new NETDEV_DEVICE_LOGIN_INFO_S();
            NETDEV_SELOG_INFO_S pstSELogInfo = new NETDEV_SELOG_INFO_S();
            pstDevLoginInfo.szIPAddr = devInfo.m_ip;
            pstDevLoginInfo.dwPort = devInfo.m_port;
            pstDevLoginInfo.szUserName = devInfo.m_userName;
            pstDevLoginInfo.szPassword = devInfo.m_password;
            if (NETDEMO.NETDEMO_DEVICE_TYPE_E.NETDEMO_DEVICE_VMS == devInfo.m_eDeviceType)
            {
                pstDevLoginInfo.dwLoginProto = (int)NETDEV_LOGIN_PROTO_E.NETDEV_LOGIN_PROTO_PRIVATE;
            }
            else
            {
                pstDevLoginInfo.dwLoginProto = (int)NETDEV_LOGIN_PROTO_E.NETDEV_LOGIN_PROTO_ONVIF;
            }
            IntPtr lpDevHandle = NETDEVSDK.NETDEV_Login_V30(ref pstDevLoginInfo, ref pstSELogInfo);

            if (lpDevHandle == IntPtr.Zero)
            {
                Debug.WriteLine(devInfo.m_ip + " : " + devInfo.m_port, "login", NETDEVSDK.NETDEV_GetLastError());
                return;
            }
            if (loginFlag == NETDEMO.NETDEV_LOGIN_TYPE_E.NETDEV_AGAIN_LOGIN)
            {
                devInfo.m_lpDevHandle = lpDevHandle;
            }
            int iRet = NETDEVSDK.NETDEV_SetExceptionCallBack(excepCB, IntPtr.Zero);
            if (NETDEVSDK.FALSE == iRet)
            {
                Debug.WriteLine(devInfo.m_ip + " : " + devInfo.m_port, "register ExceptionCallBack", NETDEVSDK.NETDEV_GetLastError());
                return;
            }
            devInfo.m_lpDevHandle = lpDevHandle;
            DeviceInfo deviceInfoTemp = devInfo;
            //DeviceInfo deviceInfoTemp = new DeviceInfo();
            //deviceInfoTemp.m_lpDevHandle = lpDevHandle;
            //deviceInfoTemp.m_ip = devInfo.m_ip;
            //deviceInfoTemp.m_port = devInfo.m_port;
            //deviceInfoTemp.m_userName = devInfo.m_userName;
            //deviceInfoTemp.m_password = devInfo.m_password;
            //deviceInfoTemp.m_eDeviceType = devInfo.m_eDeviceType;

            //get the channel list
            if (NETDEMO.NETDEMO_DEVICE_TYPE_E.NETDEMO_DEVICE_VMS == devInfo.m_eDeviceType)
            {
                deviceInfoTemp.stVmsDevInfo = new NETDEMO_VMS_DEVICE_INFO_S();

                NETDEV_ORG_FIND_COND_S stFindCond = new NETDEV_ORG_FIND_COND_S();
                stFindCond.udwRootOrgID = 0;
                IntPtr lpFindOrgHandle = NETDEVSDK.NETDEV_FindOrgInfoList(lpDevHandle, ref stFindCond);
                if (IntPtr.Zero == lpFindOrgHandle)
                {
                    Debug.WriteLine(devInfo.m_ip + " : " + devInfo.m_port, "find org list", NETDEVSDK.NETDEV_GetLastError());
                    return;
                }

                while (true)
                {
                    NETDEMO_VMS_ORG_INFO_S stDemoOrgInfo = new NETDEMO_VMS_ORG_INFO_S();
                    NETDEV_ORG_INFO_S stOrgInfo = new NETDEV_ORG_INFO_S();
                    iRet = NETDEVSDK.NETDEV_FindNextOrgInfo(lpFindOrgHandle, ref stOrgInfo);
                    if (NETDEVSDK.FALSE == iRet)
                    {
                        break;
                    }

                    stDemoOrgInfo.stOrgInfo = stOrgInfo;
                    deviceInfoTemp.stVmsDevInfo.stOrgInfoList.Add(stDemoOrgInfo);
                }

                NETDEVSDK.NETDEV_FindCloseOrgInfo(lpFindOrgHandle);

                IntPtr lpFindDevHandle = NETDEVSDK.NETDEV_FindDevList(lpDevHandle, (int)NETDEV_DEVICE_MAIN_TYPE_E.NETDEV_DTYPE_MAIN_ENCODE);
                if (IntPtr.Zero == lpFindDevHandle)
                {
                    Debug.WriteLine(devInfo.m_ip + " : " + devInfo.m_port, "NETDEV_FindDevList", NETDEVSDK.NETDEV_GetLastError());
                    //return;
                }

                while (true)
                {
                    NETDEMO_VMS_DEV_BASIC_INFO_S stDemoVmsBasicInfo = new NETDEMO_VMS_DEV_BASIC_INFO_S();

                    NETDEV_DEV_BASIC_INFO_S pstDevBasicInfo = new NETDEV_DEV_BASIC_INFO_S();
                    iRet = NETDEVSDK.NETDEV_FindNextDevInfo(lpFindDevHandle, ref pstDevBasicInfo);
                    if (NETDEVSDK.FALSE == iRet)
                    {
                        break;
                    }

                    stDemoVmsBasicInfo.stDevBasicInfo = pstDevBasicInfo;

                    IntPtr lpFindDevChnHandle = NETDEVSDK.NETDEV_FindDevChnList(lpDevHandle, pstDevBasicInfo.dwDevID, 0);
                    if (IntPtr.Zero == lpFindDevChnHandle)
                    {
                        Debug.WriteLine(devInfo.m_ip + " : " + devInfo.m_port, "NETDEV_FindDevChnList", NETDEVSDK.NETDEV_GetLastError());
                        break;
                    }

                    while (true)
                    {
                        NETDEMO_VMS_DEV_CHANNEL_INFO_S stDemoVmsChnInfo = new NETDEMO_VMS_DEV_CHANNEL_INFO_S();

                        int pdwBytesReturned = 0;
                        NETDEV_DEV_CHN_ENCODE_INFO_S stDevChnInfo = new NETDEV_DEV_CHN_ENCODE_INFO_S();
                        IntPtr lpOutBuffer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(NETDEV_DEV_CHN_ENCODE_INFO_S)));
                        Marshal.StructureToPtr(stDevChnInfo, lpOutBuffer, true);
                        iRet = NETDEVSDK.NETDEV_FindNextDevChn(lpFindDevChnHandle, lpOutBuffer, Marshal.SizeOf(typeof(NETDEV_DEV_CHN_ENCODE_INFO_S)), ref pdwBytesReturned);
                        if (NETDEVSDK.FALSE == iRet)
                        {
                            Marshal.FreeHGlobal(lpOutBuffer);
                            break;
                        }
                        else
                        {
                            stDevChnInfo = (NETDEV_DEV_CHN_ENCODE_INFO_S)Marshal.PtrToStructure(lpOutBuffer, typeof(NETDEV_DEV_CHN_ENCODE_INFO_S));
                            stDemoVmsChnInfo.stChnInfo = stDevChnInfo;
                            stDemoVmsBasicInfo.stChnInfoList.Add(stDemoVmsChnInfo);

                            Marshal.FreeHGlobal(lpOutBuffer);
                        }


                    }

                    NETDEVSDK.NETDEV_FindCloseDevChn(lpFindDevChnHandle);

                    for (int k = 0; k < deviceInfoTemp.stVmsDevInfo.stOrgInfoList.Count; k++)
                    {
                        if (stDemoVmsBasicInfo.stDevBasicInfo.dwOrgID == deviceInfoTemp.stVmsDevInfo.stOrgInfoList[k].stOrgInfo.dwOrgID)
                        {
                            deviceInfoTemp.stVmsDevInfo.stOrgInfoList[k].stVmsDevBasicInfoList.Add(stDemoVmsBasicInfo);
                        }
                    }
                }

                NETDEVSDK.NETDEV_FindCloseDevInfo(lpFindDevHandle);

                //if (loginFlag == NETDEMO.NETDEV_LOGIN_TYPE_E.NETDEV_AGAIN_LOGIN)//again login
                //{
                //    m_deviceInfoList[DeviceNodeIndex] = deviceInfoTemp;
                //    TreeNode treeNode = TreeViewFindNode(DeviceTree, DeviceNodeIndex, 0, NETDEMO.NETDEMO_FIND_TREE_NODE_TYPE_E.NETDEMO_FIND_TREE_NODE_DEVICE_INDEX);
                //    if (null != treeNode)
                //    {
                //        setDeviceTreeNode(treeNode, DeviceNodeIndex, deviceInfoTemp);
                //        //updatedeviceTreeStatus(DeviceNodeIndex);

                //        for (int j = 0; j < deviceInfo.m_RealPlayInfoList.Count; j++)
                //        {
                //            m_CurSelectTreeNodeInfo = new TreeNodeInfo();
                //            m_CurSelectTreeNodeInfo.dwDeviceIndex = DeviceNodeIndex;
                //            m_CurSelectTreeNodeInfo.dwChannelID = deviceInfo.m_RealPlayInfoList[j].m_channel;
                //            m_curRealPanel = arrayRealPanel[deviceInfo.m_RealPlayInfoList[j].m_panelIndex];
                //            startRealPlay();
                //        }
                //    }
                //}
                //else
                //{
                //    setTreeView(deviceInfoTemp);
                //    m_deviceInfoList.Add(deviceInfoTemp);
                //}
            }
            else
            {
                int pdwChlCount = 256;
                IntPtr pstVideoChlList = new IntPtr();
                //pstVideoChlList = Marshal.AllocHGlobal(NETDEVSDK.NETDEV_LEN_32 * Marshal.SizeOf(typeof(NETDEV_VIDEO_CHL_DETAIL_INFO_S)));
                pstVideoChlList = Marshal.AllocHGlobal(256 * Marshal.SizeOf(typeof(NETDEV_VIDEO_CHL_DETAIL_INFO_S)));
                iRet = NETDEVSDK.NETDEV_QueryVideoChlDetailList(deviceInfoTemp.m_lpDevHandle, ref pdwChlCount, pstVideoChlList);
                if (NETDEVSDK.TRUE == iRet)
                {
                    deviceInfoTemp.m_channelNumber = pdwChlCount;
                    NETDEV_VIDEO_CHL_DETAIL_INFO_S stCHLItem = new NETDEV_VIDEO_CHL_DETAIL_INFO_S();
                    for (int i = 0; i < pdwChlCount; i++)
                    {
                        IntPtr ptrTemp = new IntPtr(pstVideoChlList.ToInt64() + Marshal.SizeOf(typeof(NETDEV_VIDEO_CHL_DETAIL_INFO_S)) * i);
                        stCHLItem = (NETDEV_VIDEO_CHL_DETAIL_INFO_S)Marshal.PtrToStructure(ptrTemp, typeof(NETDEV_VIDEO_CHL_DETAIL_INFO_S));

                        ChannelInfo channelInfo = new ChannelInfo();
                        channelInfo.m_devVideoChlInfo = stCHLItem;
                        deviceInfoTemp.m_channelInfoList.Add(channelInfo);
                        TreeNodeInfo.dwChannelID = stCHLItem.dwChannelID;
                    }

                    NETDEV_PREVIEWINFO_S stPreviewInfo = new NETDEV_PREVIEWINFO_S();
                    stPreviewInfo.dwChannelID = TreeNodeInfo.dwChannelID;
                    //stPreviewInfo.dwFluency = 
                    stPreviewInfo.dwLinkMode = (int)NETDEV_PROTOCAL_E.NETDEV_TRANSPROTOCAL_RTPTCP;
                    stPreviewInfo.dwStreamType = (int)NETDEV_LIVE_STREAM_INDEX_E.NETDEV_LIVE_STREAM_INDEX_MAIN;
                    stPreviewInfo.hPlayWnd = CurrentHandle;
                    IntPtr Handle = NETDEVSDK.NETDEV_RealPlay(devInfo.m_lpDevHandle, ref stPreviewInfo, IntPtr.Zero, IntPtr.Zero);
                    if (Handle == IntPtr.Zero)
                    {
                        return;
                    }
                    CurrentHandle = Handle;
                    //if (loginFlag == NETDEMO.NETDEV_LOGIN_TYPE_E.NETDEV_AGAIN_LOGIN)//again login
                    //{
                    //    m_deviceInfoList[DeviceNodeIndex] = deviceInfoTemp;
                    //    TreeNode treeNode = TreeViewFindNode(DeviceTree, DeviceNodeIndex, 0, NETDEMO.NETDEMO_FIND_TREE_NODE_TYPE_E.NETDEMO_FIND_TREE_NODE_DEVICE_INDEX);
                    //    if (null != treeNode)
                    //    {
                    //        setDeviceTreeNode(treeNode, DeviceNodeIndex, deviceInfoTemp);
                    //        //updatedeviceTreeStatus(DeviceNodeIndex);

                    //        for (int j = 0; j < deviceInfo.m_RealPlayInfoList.Count; j++)
                    //        {
                    //            m_CurSelectTreeNodeInfo = new TreeNodeInfo();
                    //            m_CurSelectTreeNodeInfo.dwDeviceIndex = DeviceNodeIndex;
                    //            m_CurSelectTreeNodeInfo.dwChannelID = deviceInfo.m_RealPlayInfoList[j].m_channel;
                    //            m_curRealPanel = arrayRealPanel[deviceInfo.m_RealPlayInfoList[j].m_panelIndex];
                    //            startRealPlay();
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    setTreeView(deviceInfoTemp);
                    //    m_deviceInfoList.Add(deviceInfoTemp);
                    //}
                }
                Marshal.FreeHGlobal(pstVideoChlList);

                NETDEV_DEVICE_INFO_S pstDevInfo = new NETDEV_DEVICE_INFO_S();
                NETDEVSDK.NETDEV_GetDeviceInfo(deviceInfoTemp.m_lpDevHandle, ref pstDevInfo);
                deviceInfoTemp.m_stDevInfo = pstDevInfo;
            }


        }

        public void SetPTZSpeed(int speed)
        {
            this.m_speed = speed;
        }


        public bool Control(int PTZCommand)
        {
            //NETDEVSDK.NETDEV_PTZControl(CurrentHandle, (int)NETDEV_PTZ_E.NETDEV_PTZ_ALLSTOP, this.m_speed);
            if (NETDEVSDK.FALSE == NETDEVSDK.NETDEV_PTZControl(CurrentHandle, PTZCommand, this.m_speed))
            {
                return false;
            }
            return true;
        }

        public bool ControlZoomWide()
        {
            if (NETDEVSDK.FALSE == NETDEVSDK.NETDEV_PTZControl(CurrentHandle, (int)NETDEV_PTZ_E.NETDEV_PTZ_ZOOMWIDE, this.m_speed))
            {
                return false;
            }
            //先按500ms走
            Task.Run(async delegate
            {
                await Task.Delay(500);
                NETDEVSDK.NETDEV_PTZControl(CurrentHandle, (int)NETDEV_PTZ_E.NETDEV_PTZ_ZOOMWIDE_STOP, this.m_speed);
            });

            return true;
        }

        public bool ControlZoomTele()
        {
            if (NETDEVSDK.FALSE == NETDEVSDK.NETDEV_PTZControl(CurrentHandle, (int)NETDEV_PTZ_E.NETDEV_PTZ_ZOOMTELE, this.m_speed))
            {
                return false;
            }
            //先按500ms走
            Task.Run(async delegate
            {
                await Task.Delay(500);
                NETDEVSDK.NETDEV_PTZControl(CurrentHandle, (int)NETDEV_PTZ_E.NETDEV_PTZ_ZOOMTELE_STOP, this.m_speed);
            });

            return true;
        }

        public bool Control_Other(IntPtr lpHandle, int ChannelID, int PTZCommand)
        {
            if (NETDEVSDK.FALSE == NETDEVSDK.NETDEV_PTZControl_Other(lpHandle, ChannelID, PTZCommand, this.m_speed))
            {
                return false;
            }

            return true;
        }

        public void GetUTF8Buffer(string inputString, int bufferLen, out byte[] utf8Buffer)
        {
            utf8Buffer = new byte[bufferLen];
            byte[] tempBuffer = System.Text.Encoding.UTF8.GetBytes(inputString);
            for (int i = 0; i < tempBuffer.Length; ++i)
            {
                utf8Buffer[i] = tempBuffer[i];
            }
        }

        public void SetPreset()
        {
            string szPresetName = "原点";
            int lPresetID = 1;
            byte[] byPresetName;
            GetUTF8Buffer(szPresetName, NETDEVSDK.NETDEV_LEN_32, out byPresetName);
            int dwChannelID = PTZControl.TreeNodeInfo.dwChannelID;
            int bRet = NETDEVSDK.NETDEV_PTZPreset_Other(CurrentHandle, dwChannelID, (int)NETDEV_PTZ_PRESETCMD_E.NETDEV_PTZ_SET_PRESET, byPresetName, lPresetID);
        }
    }

    public class TreeNodeInfo
    {
        public int dwOrgID = -1;
        public int dwDeviceIndex = -1;
        public int dwSubDeviceID = -1;
        public int dwChannelID = -1;
    }
}
