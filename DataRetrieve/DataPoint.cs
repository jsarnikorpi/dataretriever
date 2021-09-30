using System;
using System.Collections.Generic;
using System.Text;

namespace DataRetrieve
{
    /// <summary>
    /// Data point value which is returned from DataRetriever
    /// </summary>
    public class DataPoint
    {
        public DateTime DataDate { get; set; }
        public double Value { get; set; }
        public int IntervalInSeconds { get; set; }
    }
}
