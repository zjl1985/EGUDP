using System;
using System.Collections.Generic;
using System.Text;
using TongHuaGPRSserver.PublicC;
using System.Data;
using System.Configuration;
using System.Diagnostics;
namespace TongHuaGPRSserver.PublicC
{
    public class OtherClass
    {
        /// <summary>
        /// 监听IP地址
        /// </summary>
        /// <returns></returns>
        public string IPAddress()
        {
            return ConfigurationManager.AppSettings["lisAddress"].ToString();
        }


        /// <summary>
        /// 监听端口
        /// </summary>
        /// <returns></returns>
        public int lPort()
        {
            return int.Parse(ConfigurationManager.AppSettings["lisPort"].ToString());
        }

        /// <summary>
        /// abblength1
        /// </summary>
        /// <returns></returns>
        public int abblength1()
        {
            return int.Parse(ConfigurationManager.AppSettings["abblength1"].ToString());
        }

        /// <summary>
        /// abblength2
        /// </summary>
        /// <returns></returns>
        public int abblength2()
        {
            return int.Parse(ConfigurationManager.AppSettings["abblength2"].ToString());
        }

        #region 封装LIST
        /// <summary>
        /// 封装LIST
        /// </summary>
        /// <param name="valuelist">读取的byte</param>
        /// <param name="dt">读取的table</param>
        /// <returns></returns>
        public List<TagValue> getValue(byte[] resultAry, DataTable dt)
        {
            List<TagValue> tagList = new List<TagValue>();
            byte[] valuelist = new byte[256];

            //if (resultAry.Length != 120 && resultAry.Length != 196 && resultAry.Length != 148)
            //{
            //    return tagList;
            //}
            Array.Copy(resultAry, valuelist, resultAry.Length);
            try
            {

                TagValue objValue;
                //   int diviceId = BitConverter.ToInt32(valuelist, 0);
                int num0 = BitConverter.ToInt32(valuelist, 0);
                if (num0 < 1 || num0 > 100)
                {
                    return tagList;
                }
                DataRow[] rows = dt.Select("PLCAddress='" + num0 + "'");
                Console.WriteLine("PLC" + num0 + "|" + resultAry.Length);
                if (rows.Length > 0)
                {
                    DataRow resultRow = dt.Select("PLCAddress='" + num0 + "'")[0];
                    int bitcount = int.Parse(resultRow["BitCount"].ToString());
                    for (int i = 0; i < bitcount; i++)
                    {
                        int intValue = BitConverter.ToInt32(valuelist, (i + 1) * 4);
                        string str_tagname = resultRow[5 + i].ToString();
                        if (str_tagname == "1" || str_tagname.Trim() == "")
                        {
                            continue;
                        }
                        objValue = new TagValue();
                        objValue.TagName = str_tagname;
                        if (num0 == 82)
                        {
                            objValue.DateTime = DateTime.Now.Date.AddHours(DateTime.Now.Hour).AddMinutes(DateTime.Now.Minute).AddSeconds(DateTime.Now.Second);
                        }
                        else
                        {
                            objValue.DateTime = DateTime.Now.Date.AddHours(DateTime.Now.Hour).AddMinutes(DateTime.Now.Minute);
                        }
                        objValue.DeviceId = resultRow["HotName"].ToString();
                        objValue.Remark = resultRow["Remark"].ToString();
                        objValue.Value = double.Parse(intValue.ToString());
                        objValue.PLCadd = resultRow["PLCAddress"].ToString();
                        tagList.Add(objValue);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("getValue:error:" + ex.Message);
                WriteError("getValue:error:" + ex.Message);
            }
            return tagList;
        }




        /// <summary>
        /// 封装LISTABB
        /// </summary>
        /// <param name="valuelist">读取的byte</param>
        /// <param name="dt">读取的table</param>
        /// <returns></returns>
        public List<TagValue> getValueABB(byte[] resultAry, DataTable dt)
        {
            List<TagValue> tagList = new List<TagValue>();

            byte[] valuelist = new byte[resultAry.Length - 9];

            //if (resultAry.Length != 137)
            //{
            //    return tagList;
            //}
            Array.Copy(resultAry, 7, valuelist, 0, resultAry.Length - 9);
            // Console.WriteLine("-----:"+BitConverter.ToString(valuelist));
         //  Console.WriteLine(BitConverter.ToString(valuelist));
           Array.Reverse(valuelist);
            //  Console.WriteLine("-----:" + BitConverter.ToString(valuelist));
            try
            {

                TagValue objValue;
                //   int diviceId = BitConverter.ToInt32(valuelist, 0);
                int num0 = int.Parse(resultAry[0].ToString());
                if (num0 > 25)
                {
                    return tagList;
                }
                DataRow[] rows = dt.Select("PLCAddress='" + num0 + "'");
                Console.WriteLine("PLC" + num0 + "|" + resultAry.Length);
                if (rows.Length > 0)
                {
                    DataRow resultRow = dt.Select("PLCAddress='" + num0 + "'")[0];
                    int bitcount = int.Parse(resultRow["BitCount"].ToString());
                    float[] fvlist = new float[bitcount];
                    for (int i = 0; i < bitcount; i++)
                    {
                        float floatValue = BitConverter.ToSingle(valuelist, i * 4);
                        fvlist[i] = floatValue;
                    }
                    
                    Array.Reverse(fvlist);
                    DateTime reTime = DateTime.Now.Date.AddHours(DateTime.Now.Hour).AddMinutes(DateTime.Now.Minute).AddSeconds(DateTime.Now.Second).AddMilliseconds(DateTime.Now.Millisecond);
                    for (int i = 0; i < bitcount; i++)
                    {
                        string str_tagname = resultRow[5 + i].ToString();
                        if (str_tagname == "1" || str_tagname.Trim() == "")
                        {
                            continue;
                        }
                        objValue = new TagValue();
                        objValue.TagName = str_tagname;
                      //  objValue.DateTime = DateTime.Now.Date.AddHours(DateTime.Now.Hour).AddMinutes(DateTime.Now.Minute).AddSeconds(DateTime.Now.Second);

                        objValue.DateTime = reTime;
                        objValue.DeviceId = resultRow["HotName"].ToString();
                        objValue.Remark = resultRow["Remark"].ToString();
                        if (fvlist[i] < 0)
                        {
                            objValue.Value = 0;
                        }
                        else
                        {
                            objValue.Value = Math.Round(fvlist[i], 2);
                        }
                        if (double.IsNaN(objValue.Value))
                        {
                            objValue.Value = -123456;
                        }
                        objValue.PLCadd = resultRow["PLCAddress"].ToString();
                        tagList.Add(objValue);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("error:" + ex.Message);
            }
            return tagList;
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
