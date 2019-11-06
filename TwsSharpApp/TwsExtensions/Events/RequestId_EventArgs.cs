using System;

namespace TwsSharpApp
{
    public class RequestId_EventArgs : EventArgs
    {
        public RequestId_EventArgs(int reqId)
        {
            this.RequestId = reqId;
        }

        public int RequestId { get; private set; }
    }
}