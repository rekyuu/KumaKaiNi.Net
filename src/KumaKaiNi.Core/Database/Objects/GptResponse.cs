using System.Collections.Generic;

namespace KumaKaiNi.Core
{
    public class GptResponse : DatabaseObject
    {
        public string Message;
        public bool Returned;

        public GptResponse() { }
        
        public GptResponse(Dictionary<string, object> row) : base(row) { }
    }
}
