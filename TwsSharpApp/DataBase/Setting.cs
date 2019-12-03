using IBApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwsSharpApp.Data
{
    public class Setting
    {
        public Setting ()
        {
        }

        public Setting (string key, string value)
        {
            Key   = key;
            Value = value;
        }

        public int    Id    { get; set; }
        public string Key   { get; set; }
        public string Value { get; set; }
    }
}
