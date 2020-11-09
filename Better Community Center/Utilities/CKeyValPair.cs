using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BCC.Utilities
{
    public class CKeyValPair<Key, Val>
    {
        public Key Id { get; set; }

        public Val Value { get; set; }

        public CKeyValPair() { }

        public CKeyValPair(Key key, Val val)
        {
            Id = key;
            Value = val;
        }
    }
}
