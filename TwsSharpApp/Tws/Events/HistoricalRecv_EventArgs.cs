using IBApi;
using System;
using System.Collections.Generic;

namespace TwsSharpApp
{
    public class HistoricalRecv_EventArgs : EventArgs
    {
        public HistoricalRecv_EventArgs(List<Bar> barsList)
        {
            this.HistoricalList = barsList;
        }

        public List<Bar> HistoricalList { get; private set; }
    }
}