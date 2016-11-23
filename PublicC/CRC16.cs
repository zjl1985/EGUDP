using System;
namespace CRC
{
    /// <summary> 
    /// CRC16 的摘要说明。 
    /// </summary> 
    public class CRC16 
    {

        public string  GetCrc16_string(string  str_info)
        {
            long lon = GetModBusCRC(str_info);
            long h1, l0;
            h1 = lon % 256;
            l0 = lon / 256;

            string s = "";
            if (Convert.ToString(h1, 16).Length < 2)
            {
                s = "0" + Convert.ToString(h1, 16);
            }
            else
            {
                s = Convert.ToString(h1, 16);
            }

            if (Convert.ToString(l0, 16).Length < 2)
            {
                s = s + "0" + Convert.ToString(l0, 16);
            }
            else
            {
                s = s + Convert.ToString(l0, 16);
            }
            return s;
        }

        private static byte[] strToToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }


        public long GetModBusCRC(string DATA)
        {
            long functionReturnValue = 0;

            long i = 0;
            long J = 0;
            byte[] v = null;
            v = strToToHexByte(DATA);

            //1.预置1个16位的寄存器为十六进制FFFF（即全为1）：称此寄存器为CRC寄存器；
            long CRC = 0;
            CRC = 0xffffL;
            for (i = 0; i <= (v).Length - 1; i++)
            {
                //2.把第一个8位二进制数据（既通讯信息帧的第一个字节）与16位的CRC寄存器的低8位相异或，把结果放于CRC寄存器；
                CRC = (CRC / 256) * 256L + (CRC % 256L) ^ v[i];
                for (J = 0; J <= 7; J++)
                {
                    //3.把CRC寄存器的内容右移一位（朝低位）用0填补最高位，并检查最低位；
                    //4.如果最低位为0：重复第3步（再次右移一位）；
                    // 如果最低位为1：CRC寄存器与多项式A001（1010 0000 0000 0001）进行异或；
                    //5.重复步骤3和4，直到右移8次，这样整个8位数据全部进行了处理；
                    long d0 = 0;
                    d0 = CRC & 1L;
                    CRC = CRC / 2;
                    if (d0 == 1)
                        CRC = CRC ^ 0xa001L;

                }

                //6.重复步骤2到步骤5，进行通讯信息帧下一字节的处理；
            }

            //7.最后得到的CRC寄存器内容即为：CRC码。
            CRC = CRC % 65536;
            functionReturnValue = CRC;
            return functionReturnValue;
        }
    }
}