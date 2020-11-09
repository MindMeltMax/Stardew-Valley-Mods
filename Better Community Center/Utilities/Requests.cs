using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BCC.Utilities
{
    public class Requests
    {
        public static List<Request> RequestList { get; set; }
    }

    public class Request
    {
        public int itemIndex { get; set; }

        public int itemCount { get; set; }

        public int totalPrice { get; set; }

        public int CreationDate { get; set; }

        public Request(int index, int count, int price, int date)
        {
            itemIndex = index;
            itemCount = count;
            totalPrice = price;
            CreationDate = date;
        }
    }
}
