using System;
using System.Collections.Generic;
using System.Text;

namespace DataRetrieve
{
    /// <summary>
    /// Parameters for retrieve interval.
    /// Interval will be calculated as follws
    /// - If some of these params is given it will be the only one 
    ///   which is taken into account in following order:
    ///     1. IntervalFullYears
    ///     2. IntervalFullMonths
    ///     3. IntervalFullWeeks
    ///     4. IntervalMonths
    /// - If all above are zero then interval is calculated as sum:
    ///     * IntervalWeeks + IntervalDays + IntervalHours + IntervalMinutes + IntervalSeconds
    /// </summary>
    public class RetrieveParams
    {
        /// <summary>
        /// Begin of the interval
        /// </summary>
        public DateTime BeginDate { get; set; }
        /// <summary>
        /// End of the interval
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Interval as seconds
        /// Total interval is calculated as sum: 
        ///   * IntervalWeeks + IntervalDays + IntervalHours + IntervalMinutes + IntervalSeconds
        /// </summary>
        public int IntervalSeconds { get; set; }

        /// <summary>
        /// Interval as minutes
        /// Total interval is calculated as sum: 
        ///   * IntervalWeeks + IntervalDays + IntervalHours + IntervalMinutes + IntervalSeconds
        /// </summary>
        public int IntervalMinutes { get; set; }

        /// <summary>
        /// Interval as hours
        /// Total interval is calculated as sum: 
        ///   * IntervalWeeks + IntervalDays + IntervalHours + IntervalMinutes + IntervalSeconds
        /// </summary>
        public int IntervalHours { get; set; }

        /// <summary>
        /// Interval as days
        /// Total interval is calculated as sum: 
        ///   * IntervalWeeks + IntervalDays + IntervalHours + IntervalMinutes + IntervalSeconds
        /// </summary>
        public int IntervalDays { get; set; }

        /// <summary>
        /// Interval as weeks (7 days)
        /// Total interval is calculated as sum: 
        ///   * IntervalWeeks + IntervalDays + IntervalHours + IntervalMinutes + IntervalSeconds
        /// </summary>
        public int IntervalWeeks { get; set; }

        /// <summary>
        /// Interval as calendar months
        /// * Length of interval varies!
        /// * Any other Interval-params are not taken into account
        /// </summary>
        public int IntervalMonths { get; set; }

        /// <summary>
        /// Interval as full calendar weeks (Monday to Sunday)
        /// * If BeginDate is not first day of week then first interval will be short
        /// * Any other Interval-params are not taken into account
        /// </summary>
        public int IntervalFullWeeks { get; set; }
        /// <summary>
        /// Interval as full calendar months
        /// * If BeginDate is not first date of month then first interval will be short
        /// * Any other Interval-params are not taken into account
        /// </summary>
        public int IntervalFullMonths { get; set; }
        /// <summary>
        /// Interval as full calendar years
        /// * If BeginDate is not first date of year then first interval will be short
        /// * Any other Interval-params are not taken into account
        /// </summary>
        public int IntervalFullYears { get; set; }

    }
}
