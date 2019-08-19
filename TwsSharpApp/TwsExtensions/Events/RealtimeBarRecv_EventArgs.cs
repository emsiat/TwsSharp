using IBApi;
using System;

namespace TwsSharpApp
{
    public class RealtimeBarRecv_EventArgs : EventArgs
    {
        public RealtimeBarRecv_EventArgs(int requestId, Bar bar)
        {
            this.RequestId   = requestId;
            this.RealtimeBar = bar;
        }

        public int RequestId   { get; private set; }
        public Bar RealtimeBar { get; private set; }
    }
}