using System;

namespace TongHuaGPRSserver.PublicC
{
    public class TagValue
    {
        public TagValue()
        {
        }

        private string _deviceId;
        private string _tagName;
        private double _value;
        private DateTime _dateTime;
        private string _PLCadd;
        private string _remark;

        public string Remark
        {
            get { return _remark; }
            set { _remark = value; }
        }

        public string PLCadd
        {
            get { return _PLCadd; }
            set { _PLCadd = value; }
        }


        public string DeviceId
        {
            get { return _deviceId; }
            set { _deviceId = value; }
        }

        public string TagName
        {
            get { return _tagName; }
            set { _tagName = value; }
        }

        public double Value
        {
            get { return _value; }
            set { _value = value; }
        }


        public DateTime DateTime
        {
            get { return _dateTime; }
            set { _dateTime = value; }
        }
    }
}
