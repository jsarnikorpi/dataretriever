using System;
using System.Collections.Generic;
using System.Text;

namespace DataRetrieve
{
    public interface IDataRepository
    {
        /// <summary>
        /// Sum of values from ValueDate >= begin and ValueDate <= end
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        DataValueItem GetSummaryBetween(DateTime begin, DateTime end);
        
        /// <summary>
        /// Returns all values which overlaps with given dates
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        IEnumerable<DataValueItem> GetOverlapping(DateTime begin, DateTime end);
    }
}
