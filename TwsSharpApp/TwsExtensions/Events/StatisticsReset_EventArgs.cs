using System;
using System.Collections.Generic;

namespace TwsSharpApp
{
    public class StatisticsReset_EventArgs : EventArgs
    {
        public StatisticsReset_EventArgs(List<string> uniqueNames_List)
        {
            this.UniqueNames_List = uniqueNames_List;
        }

        public List<string> UniqueNames_List { get; private set; }
    }
}