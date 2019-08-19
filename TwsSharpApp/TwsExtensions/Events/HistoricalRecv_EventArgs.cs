using IBApi;
using System;
using System.Collections.Generic;

namespace TwsSharpApp
{
    public class HistoricalRecv_EventArgs : EventArgs
    {
        public HistoricalRecv_EventArgs(int requestId, List<Bar> barsList)
        {
            this.RequestId      = requestId;
            this.HistoricalList = barsList;
        }

        public int       RequestId      { get; private set; }
        public List<Bar> HistoricalList { get; private set; }
    }
}