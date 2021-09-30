using System;
using System.Collections.Generic;
using System.Text;

namespace DataRetrieve
{
    /// <summary>
    /// Class for data value between dates.
    /// </summary>
    public class DataValueItem
    {
        public double Value { get; set; }
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
