using System;
namespace CRC
{
    public interface ICRC
    {

        long Value
        {
            get;
        }

        void Reset();

        void Crc(int bval);

        void Crc(byte[] buffer);

        void Crc(byte[] buf, int off, int len);
    }
}