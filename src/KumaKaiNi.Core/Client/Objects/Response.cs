﻿namespace KumaKaiNi.Core
{
    public class Response
    {
        public string Message;
        public ResponseImage Image;
        public string AdminMessage = "";

        public Response(string message = "")
        {
            Message = message;
        }

        public Response(ResponseImage image)
        {
            Message = "";
            Image = image;
        }

        public Response(string message, ResponseImage image)
        {
            Message = message;
            Image = image;
        }
    }
}
