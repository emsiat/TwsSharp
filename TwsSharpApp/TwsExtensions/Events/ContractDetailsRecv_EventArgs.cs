using IBApi;
using System;
using System.Collections.Generic;

namespace TwsSharpApp
{
    public class ContractDetailsRecv_EventArgs : EventArgs
    {
        public ContractDetailsRecv_EventArgs(int requestId, ContractDetails contractDetails)
        {
            this.RequestId       = requestId;
            this.ContractDetails = contractDetails;
        }

        public int             RequestId       { get; private set; }
        public ContractDetails ContractDetails { get; private set; }
    }
}