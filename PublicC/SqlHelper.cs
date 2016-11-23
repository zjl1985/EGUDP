using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Text;
using System;


namespace TongHuaGPRSserver.PublicC
{
    public class SqlHelper
    {
        string str_conn = ConfigurationManager.ConnectionStrings["sqlconn"].ToString();

        public SqlConnection getConn()
        {
            SqlConnection conn = new SqlConnection(str_conn);
            return conn;
        }

        public int insterSQL2(SqlConnection conn, List<TagValue> tagvalueList)
        {
            //SqlCommand cmd=new SqlCommand();
            //cmd.Connection = conn;
            // string _strSql = string.Empty;
            StringBuilder _strBuilder = new StringBuilder();
            foreach (TagValue obj in tagvalueList)
            {
                _strBuilder.AppendFormat("insert into HSData_Temp(SName,TagName,TagVal,GetDt,Flag) values ('{0}','{1}','{2}','{3}',{4});", obj.DeviceId, obj.TagName, obj.Value, obj.DateTime.ToString() + "." + obj.DateTime.Millisecond, 0);
                // _strSql = string.Format("insert into HSData_Temp(SName,TagName,TagVal,GetDt,Flag) values ('{0}','{1}','{2}','{3}',{4});", obj.DeviceId, obj.TagName, obj.Value, obj.DateTime,0);
            }
         
            SqlCommand cmd = new SqlCommand(_strBuilder.ToString(), conn);
            IAsyncResult result = cmd.BeginExecuteNonQuery();
            while (!result.IsCompleted)
            {

                System.Threading.Thread.Sleep(100);
            }

            return cmd.EndExecuteNonQuery(result);
        }


        public int UpdateSql(SqlConnection conn, string stationNo, string setp, string inp, string outp, string opctime, string FeedBack, string Total_E, string A_Current, string AB_Voltage, string strA_Current2, string strAB_Voltage2, string No1M_Run, string No2M_Run)
        {
            string strcmd = string.Format("update SLastTime set LastTime ='{0}',SetP={2},InP={3},OutP={4},OPCTime='{5}',FeedBack ={6},Total_E = {7} ,No1A_Current = {8},No1AB_Voltage ={9},No2A_Current={10},No2AB_Voltage={11},No1M_Run={12},No2M_Run={13} where StartNo={1}", DateTime.Now.ToString(), stationNo, setp, inp, outp, opctime, FeedBack, Total_E, A_Current, AB_Voltage, strA_Current2, strAB_Voltage2, No1M_Run, No2M_Run);
            SqlCommand cmd = new SqlCommand(strcmd, conn);
     //       Console.WriteLine(strcmd);
            return cmd.ExecuteNonQuery();
        }


        public int insterSQL(SqlConnection conn, List<TagValue> tagvalueList)
        {
            //SqlCommand cmd=new SqlCommand();
            //cmd.Connection = conn;
            // string _strSql = string.Empty;
            StringBuilder _strBuilder = new StringBuilder();
            foreach (TagValue obj in tagvalueList)
            {
                _strBuilder.AppendFormat("insert into HSData_Temp(SName,TagName,TagVal,GetDt,Flag) values ('{0}','{1}','{2}','{3}',{4});", obj.DeviceId, obj.TagName, obj.Value, obj.DateTime, 0);
                // _strSql = string.Format("insert into HSData_Temp(SName,TagName,TagVal,GetDt,Flag) values ('{0}','{1}','{2}','{3}',{4});", obj.DeviceId, obj.TagName, obj.Value, obj.DateTime,0);
            }

            SqlCommand cmd = new SqlCommand(_strBuilder.ToString(), conn);
            return cmd.ExecuteNonQuery();
        }
    }
}
