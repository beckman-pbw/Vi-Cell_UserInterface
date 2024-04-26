using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BAFW
{
    /// <summary>
    /// A utility class to keep track if the data is valid and the last time the data was updated.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LastReading<T>
    {
        public DateTime TimeStamp { get; set; } = DateTime.MinValue;
        public Int32 ErrorNum { get; set; } = -1;
        public String StrInfo { get; set; } = "";
        public double Age { get { return DateTime.Now.Subtract(TimeStamp).TotalSeconds; } }
        public bool IsValid() { return (ErrorNum == 0); }
        public T Data { get { return _data; } }
        private T _data;

        // ...........................................................
        public LastReading(T initVal)
        {
            _data = initVal;
            ErrorNum = -1;
            TimeStamp = DateTime.Now;
            StrInfo = "Constructed";
        }

        // ...........................................................
        public void SetValid(T val)
        {
            _data = val;
            ErrorNum = 0;
            TimeStamp = DateTime.Now;
            StrInfo = "valid";
        }

        // ...........................................................
        public void SetValid(T val, string strInfo)
        {
            _data = val;
            ErrorNum = 0;
            StrInfo = strInfo;
            TimeStamp = DateTime.Now;
        }

        // ...........................................................
        public void SetInvalid(T val, Int32 err = -1, string strInfo = "SetInvalid")
        {
            _data = val;
            StrInfo = strInfo;            
            ErrorNum = err; 
            if (ErrorNum == 0)
            {
                ErrorNum = 1;
                StrInfo += " SetInvalid, (err==0)";
            }            
            TimeStamp = DateTime.Now;
        }
    }
}
