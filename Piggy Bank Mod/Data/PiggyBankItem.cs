using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiggyBank.Data
{
    public class PiggyBankItem
    {
        public string Label { get; set; }

        public int Gold { get; set; } = 0;

        public long Owner { get; set; }
    }
}
