using IBApi;
using System;

namespace TwsSharpApp
{
    public class Quote_EventArgs : EventArgs
    {
        public Quote_EventArgs(Quote_ViewModel qVM)
        {
            this.Quote_VM = qVM;
        }

        public Quote_ViewModel Quote_VM { get; private set; }
    }
}