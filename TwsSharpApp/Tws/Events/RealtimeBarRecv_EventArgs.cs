using IBApi;
using System;

namespace TwsSharpApp
{
    public class RealtimeBarRecv_EventArgs : EventArgs
    {
        public RealtimeBarRecv_EventArgs(Bar bar)
        {
            this.RealtimeBar = bar;
        }

        public Bar RealtimeBar { get; private set; }
    }
}