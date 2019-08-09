using IBApi;
using System;
using System.Collections.Generic;

namespace TwsSharpApp
{
    public class SocketConnected_EventArgs : EventArgs
    {
        public SocketConnected_EventArgs(bool isConnected)
        {
            this.IsConnected = isConnected;
        }

        public bool IsConnected { get; private set; }
    }
}