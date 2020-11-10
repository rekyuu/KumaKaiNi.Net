using System;
using System.Collections.Generic;

namespace KumaKaiNi.Core
{
    public class Error : DatabaseObject
    {
        public DateTime Timestamp;
        public string Source;
        public string Message;
        public string StackTrace;

        public Error() : base() { }
        public Error(Dictionary<string, object> row) : base(row) { }
        public Error(Exception ex)
        {
            Timestamp = DateTime.UtcNow;
            Source = ex.Source;
            Message = ex.Message;
            StackTrace = ex.StackTrace;
        }
    }
}
