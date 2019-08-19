using System;

namespace TwsSharpApp
{
    public class TwsError
    {
        public int id;
        public int errorCode;
        public string errorMsg;
        
        public TwsError(int i, int code, string msg)
        {
            id = i;
            errorCode = code;
            errorMsg = msg;
        }

        public TwsError(string msg)
        {
            id = -1;
            errorCode = -1;
            errorMsg = msg;
        }
    }

    public class ErrorRecv_EventArgs : EventArgs
    {
        public ErrorRecv_EventArgs(TwsError err)
        {
            this.Error = err;
        }

        public TwsError Error { get; private set; }
    }
}