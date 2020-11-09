using System.Collections.Generic;

namespace BCC.Utilities
{
    public class BoilerData
    {
        public static List<data> dataList { get; set; }
    }

    public class data
    {
        public int ParentSheetIndex { get; set; }

        public int Stack { get; set; }

        public int HoldingComponentID { get; set; }

        public data(int index, int stack, int component)
        {
            ParentSheetIndex = index;
            Stack = stack;
            HoldingComponentID = component;
        }
    }
}
