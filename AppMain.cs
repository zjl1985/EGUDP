using System;
using System.Collections;							// Access to the Array list
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
// Sleeping
using System.Net;									// Used to local machine info
using System.Net.Sockets;							// Socket namespace
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using TongHuaGPRSserver.PublicC;
using System.Text.RegularExpressions;

namespace SocketListerGaoxin
{
    /// <summary>
    /// Main class from which all objects are created
    /// </summary>
    class AppMain
    {

        //static DataTable dt;
        //static DataTable Abbdt;
        static SqlHelper objhelper;
        static SqlConnection conn;
        static OtherClass oc;
        // Attributes
        private ArrayList m_aryClients = new ArrayList();	// List of Client Connections
        public const string GaoxinUDPProtocol = @"FDFDV\d.\d{2}.\d{2}L\d{3}DT(2\d\d\d[-](0[1-9]|1[012])[-](0[1-9]|[12][0-9]|3[01])\s+\d{2}:\d{2}:\d{2}),(.*)FEFE";
        public const string GaoxinUDPData = @"^[0-9]*:[-+]?[0-9]*\.?[0-9]+([eE][-+]?[0-9]+)?:[0-9]*$";
        public const char Comma = ',';
        public const char Colon = ':';

        static Regex reg;
        static Regex dataReg;


        public static UdpClient uc;
        public static IPEndPoint ipp;
        public static UdpClient uc_back;
        public static IPEndPoint ipp_back;
        static UpdateLastTimedDelegate updateLastTimedDelegate;
        static SendTankLevelDelegate sendTankLevelDelegate;
        /// <summary>
        /// Application starts here. Create an instance of this class and use it
        /// as the main object.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            objhelper = new SqlHelper();
            conn = objhelper.getConn();
            conn.Open();
            oc = new OtherClass();

            reg = new Regex(GaoxinUDPProtocol);
            dataReg = new Regex(GaoxinUDPData);

            int nPortListen = oc.lPort();


            uc = new UdpClient();
            ipp = new IPEndPoint(System.Net.IPAddress.Parse(ConfigurationManager.AppSettings["udpDoubleServer"]), int.Parse(ConfigurationManager.AppSettings["updprot"]));


            uc_back = new UdpClient();
            ipp_back = new System.Net.IPEndPoint(System.Net.IPAddress.Parse(ConfigurationManager.AppSettings["udpDoubleServer_back"]), int.Parse(ConfigurationManager.AppSettings["updprot_back"]));


            IPAddress ip = IPAddress.Parse(oc.IPAddress());
            IPAddress ipAny = IPAddress.Any;
            IPEndPoint ipe = new IPEndPoint(ip, nPortListen);


            updateLastTimedDelegate = UpdateLastTime;
            sendTankLevelDelegate = SendTankLevel;

            m_sListen = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            m_sListen.Bind(ipe);

            //IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            //EndPoint remote = (EndPoint)sender;


            EndPoint remote = (EndPoint)ipe;

            PostRecv(ipe);
            Console.WriteLine("请按回车键结束!");

            Console.ReadKey();

            conn.Close();


            GC.Collect();
            GC.WaitForPendingFinalizers();
        }


        #region UDP

        static Socket m_sListen;
        private static void ListenThreadMothed()
        {


        }

        private static void PostRecv(EndPoint endPoint)
        {
            byte[] buffer = new byte[1048576];
            m_sListen.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref endPoint, EndRecv, new AsyncState(buffer, 0, buffer.Length, endPoint));
        }

        private static void PostSend(byte[] buffer, int offset, int size, EndPoint endPoint)
        {
            m_sListen.BeginSendTo(buffer,
                offset,
                size,
                SocketFlags.None,
                endPoint,
                EndSend,
                new AsyncState(buffer, offset, size, endPoint));
        }


        public delegate void UpdateLastTimedDelegate(Match mat);
        public delegate void SendTankLevelDelegate(Match mat);

        public static void UpdateLastTime(Match mat)
        {
            Random random = new Random();
            string[] UDPItems = mat.Groups[4].Value.Split(Comma);
            string[] UDPItem0;
            string strSetp = "0", strInp = "0", strOutp = "0", strFeedBack = "0", strTotal_E = "0", strA_Current = "0", strAB_Voltage = "0", strA_Current2 = "0", strAB_Voltage2 = "0", No1M_Run = "0", No2M_Run = "0";
            string[] udpItemFor;

            UDPItem0 = UDPItems[0].Split(Colon);
            int intNO = int.Parse(UDPItem0[0]);

            if (intNO % 80 == 0)
            {
                foreach (var item in UDPItems)
                {
                    udpItemFor = item.Split(Colon);
                    int strNO = int.Parse(udpItemFor[0]);
                    int theV = strNO % 80;
                    if (strNO - intNO >= 80)
                    {
                        break;
                    }
                    else
                    {
                        switch (theV)
                        {
                            case 0:
                                strSetp = udpItemFor[1];
                                break;
                            case 1:
                                strInp = udpItemFor[1];
                                break;
                            case 2:
                                strOutp = udpItemFor[1];
                                break;
                            case 4:
                                strFeedBack = udpItemFor[1];
                                break;
                            case 37:
                                strTotal_E = udpItemFor[1];
                                break;
                            case 5:
                                strA_Current = udpItemFor[1];
                                break;
                            case 17:
                                strAB_Voltage = udpItemFor[1];
                                break;
                            case 8:
                                strA_Current2 = udpItemFor[1];
                                break;
                            case 20:
                                strAB_Voltage2 = udpItemFor[1];
                                break;
                            case 42:
                                No1M_Run = udpItemFor[1];
                                break;
                            case 45:
                                No2M_Run = udpItemFor[1];
                                break;
                        }
                    }
                }
                if (UDPItem0.Length > 0)
                {
                    Console.WriteLine("No:" + UDPItem0[0] + "设压:" + strSetp + "|进压:" + strInp + "|出压:" + strOutp + "|OPC时间：" + mat.Groups[1] + ":");
                    objhelper.UpdateSql(conn, UDPItem0[0], strSetp, strInp, strOutp, mat.Groups[1].ToString(), strFeedBack, strTotal_E, strA_Current, strAB_Voltage, strA_Current2, strAB_Voltage2, No1M_Run, No2M_Run);
                }
            }
            else if (intNO % 100 == 0)
            {
                foreach (var item in UDPItems)
                {
                    udpItemFor = item.Split(Colon);
                    int strNO = int.Parse(udpItemFor[0]);
                    int theV = strNO % 100;
                    if (strNO - intNO >= 100)
                    {
                        break;
                    }
                    else
                    {
                        switch (theV)
                        {
                            case 0:
                                strSetp = udpItemFor[1];
                                break;
                            case 1:
                                strInp = udpItemFor[1];
                                break;
                            case 2:
                                strOutp = udpItemFor[1];
                                break;
                            case 4:
                                strFeedBack = udpItemFor[1];
                                break;
                            case 37:
                                strTotal_E = udpItemFor[1];
                                break;
                            case 5:
                                strA_Current = udpItemFor[1];
                                break;
                            case 17:
                                strAB_Voltage = udpItemFor[1];
                                break;
                            case 8:
                                strA_Current2 = udpItemFor[1];
                                break;
                            case 20:
                                strAB_Voltage2 = udpItemFor[1];
                                break;
                            case 42:
                                No1M_Run = udpItemFor[1];
                                break;
                            case 45:
                                No2M_Run = udpItemFor[1];
                                break;
                        }
                    }
                }
                if (UDPItem0.Length > 0)
                {
                    Console.WriteLine("No:" + UDPItem0[0] + "设压:" + strSetp + "|进压:" + strInp + "|出压:" + strOutp + "|OPC时间：" + mat.Groups[1] + ":");
                    objhelper.UpdateSql(conn, UDPItem0[0], strSetp, strInp, strOutp, mat.Groups[1].ToString(), strFeedBack, strTotal_E, strA_Current, strAB_Voltage, strA_Current2, strAB_Voltage2, No1M_Run, No2M_Run);
                }
            }
        }
        public static void SendTankLevel(Match mat)
        {
            Random random = new Random();
            string[] UDPItems = mat.Groups[4].Value.Split(Comma);
            string[] udpItemFor;
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("FDFDV1.00.00L999DT{0}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            int index = 0;
            foreach (String item in UDPItems)
            {
                udpItemFor = item.Split(Colon);
                int strNO = int.Parse(udpItemFor[0]);
                int theV = strNO % 80;
                if(strNO % 80==0 || strNO % 100 == 0)
                {
                    index++;
                    sb.AppendFormat(",{0}:{1}:192", int.Parse(udpItemFor[0]) + 33, random.Next(1, 101));
                }
            }
            if (index > 0)
            {
                sb.Append("FEFE");
                byte[] bytes = Encoding.ASCII.GetBytes(sb.ToString());
                uc.Send(bytes, bytes.Length, ipp);
                sb.Length = 0;
            }
        }

        public static void UpdateLastTimeComeplete(IAsyncResult result)
        {
            (result.AsyncState as UpdateLastTimedDelegate).EndInvoke(result);
        }

        public static void SendTankLevelComeplete(IAsyncResult result)
        {
            (result.AsyncState as SendTankLevelDelegate).EndInvoke(result);
        }


        private static void EndRecv(IAsyncResult asyncResult)
        {
            AsyncState state = (AsyncState)asyncResult.AsyncState;
            try
            {
                m_sListen.EndReceiveFrom(asyncResult, ref state.EndPoint);
                Random random = new Random();
                if (state.Buffer.Length > 20)
                {
                    string recString = Encoding.Default.GetString(state.Buffer);
                    Match mat = reg.Match(recString.Trim(), 0);
                    //recString.IndexOf(',');
                    //recString.IndexOf(':');
                    if (mat.Success)
                    {
                        updateLastTimedDelegate.BeginInvoke(mat, UpdateLastTimeComeplete, updateLastTimedDelegate);
                        sendTankLevelDelegate.BeginInvoke(mat, SendTankLevelComeplete, sendTankLevelDelegate);
                    }

                }

                PostRecv(state.EndPoint);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                PostRecv(state.EndPoint);
            }

            //  PostSend(state.Buffer, state.Offset, state.Size, state.EndPoint);
        }

        private static void EndSend(IAsyncResult asyncResult)
        {
            AsyncState state = (AsyncState)asyncResult.AsyncState;
            byte[] buffer = state.Buffer;
            int sendBytes = m_sListen.EndSendTo(asyncResult);
            int remainBytes = state.Size - sendBytes;
            if (remainBytes <= 0)
                return;
            PostSend(buffer, buffer.Length - remainBytes, remainBytes, state.EndPoint);
        }

        [Serializable, StructLayout(LayoutKind.Sequential)]
        internal struct AsyncState
        {
            private readonly byte[] buffer;
            private readonly int offset;
            private readonly int size;

            public AsyncState(byte[] buffer, EndPoint endPoint)
            {
                if (endPoint == null)
                {
                    throw new ArgumentNullException("endPoint");
                }
                this.buffer = buffer;
                this.offset = 0;
                this.size = buffer.Length;
                this.EndPoint = endPoint;
            }

            public AsyncState(byte[] buffer, int offset, int size, EndPoint endPoint)
            {
                if (buffer == null)
                {
                    throw new ArgumentNullException("array");
                }
                if (offset < 0)
                {
                    throw new ArgumentOutOfRangeException("offset", "ArgumentOutOfRange_NeedNonNegNum");
                }
                if (size < 0)
                {
                    throw new ArgumentOutOfRangeException("count", "ArgumentOutOfRange_NeedNonNegNum");
                }
                if ((buffer.Length - offset) < size)
                {
                    throw new ArgumentException("Argument_InvalidOffLen");
                }
                if (endPoint == null)
                {
                    throw new ArgumentNullException("endPoint");
                }
                this.buffer = buffer;
                this.offset = offset;
                this.size = size;
                this.EndPoint = endPoint;
            }

            public byte[] Buffer
            {
                get { return buffer; }
            }

            public int Offset
            {
                get { return offset; }
            }

            public int Size
            {
                get { return size; }
            }

            public EndPoint EndPoint;

        }
        #endregion

        #region 写入日志
        private void WriteError(string sText)
        {
            string sEventSource = "高信GPRS采集控制台";
            string sEventLog = "错误日志";
            if (!EventLog.SourceExists(sEventSource))
                EventLog.CreateEventSource(sEventSource, sEventLog);
            EventLog.WriteEntry(sEventSource, sText, EventLogEntryType.Error);

        }

        private void WriteInfo(string sText)
        {
            string sEventSource = "高信GPRS采集控制台";
            string sEventLog = "信息";
            if (!EventLog.SourceExists(sEventSource))
                EventLog.CreateEventSource(sEventSource, sEventLog);
            EventLog.WriteEntry(sEventSource, sText, EventLogEntryType.Information);
        }

        #endregion
    }


}
