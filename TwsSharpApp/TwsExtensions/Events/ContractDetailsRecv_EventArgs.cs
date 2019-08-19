using IBApi;
using System;
using System.Collections.Generic;

namespace TwsSharpApp
{
    public class ContractDetailsRecv_EventArgs : EventArgs
    {
        public ContractDetailsRecv_EventArgs(int requestId, ContractDetails contractDet)
        {
            this.RequestId    = requestId;
            this.ContractData = contractDet;
        }

        public int             RequestId    { get; private set; }
        public ContractDetails ContractData { get; private set; }
    }
}