using IBApi;
using System;

namespace TwsSharpApp
{
    public class HistoricalBarRecv_EventArgs : EventArgs
    {
        public HistoricalBarRecv_EventArgs(int requestId, Bar bar)
        {
            this.RequestId     = requestId;
            this.HistoricalBar = bar;
        }

        public int RequestId     { get; private set; }
        public Bar HistoricalBar { get; private set; }
    }
}