using System;
using System.Collections.Generic;

namespace DataRetrieve
{
    public enum EOverlapType
    {
        /// <summary>
        /// Interval covers other interval completedly
        /// </summary>
        CoverAll,
        /// <summary>
        /// Interval overlaps begin of other interval
        /// </summary>
        OverlapBegin,
        /// <summary>
        /// Interval overlaps end of other interval
        /// </summary>
        OverlapEnd,
        /// <summary>
        /// Interval is inside of other interval
        /// </summary>
        Inside,
        /// <summary>
        /// Interval has no ovarlapping with other interval
        /// </summary>
        NoOverlap
    }

    public class DataRetriever
    {
        public DataRetriever()
        {

        }

        #region Interval helpers

        /// <summary>
        /// Creates intervals from paramters
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public List<RetrieveInterval> CreateIntervals(RetrieveParams parameters)
        {
            var intervals = new List<RetrieveInterval>();
            var begin = parameters.BeginDate;
            var end = parameters.BeginDate;
            var first = true;

            while (begin < parameters.EndDate)
            {
                if (
                    first &&
                    (parameters.IntervalFullYears 
                        + parameters.IntervalFullMonths
                        + parameters.IntervalFullWeeks) > 0
                    )
                {
                    first = false;

                    // Jos alkuaika ei täsmää määritettyyn aikaväliin, niin ensimmäinen intervalli
                    // muodostetaan "vajaana" (seuraavaan aikaväliin soveltuvaan päivään)

                    // if given begin time is not "same" as first interval begin (should get full months but begin date is not first date of month)
                    // then first interval will be short

                    if (parameters.IntervalFullYears > 0 && !Utils.IsFirstDayOfYear(begin, true))
                    {
                        // first date of next year
                        var d = begin.AddYears(1);
                        end = new DateTime(d.Year, 1, 1, 0, 0, 0);
                    }
                    else if (parameters.IntervalFullMonths > 0 && !Utils.IsFirstDayOfMonth(begin, true))
                    {
                        // first date of next month
                        var d = begin.AddMonths(1);
                        end = new DateTime(d.Year, d.Month, 1, 0, 0, 0);
                    }
                    else if (parameters.IntervalFullWeeks > 0 && !Utils.IsFirstDayOfWeek(begin, true))
                    {
                        // TODO: IntervalFullWeeks handles only Monday as first day of the week

                        // next Monday

                        // -> weekday: Monday = 0, Tuesday = 1, ... Sunday = 6
                        var weekday = (int)begin.DayOfWeek == 0 ? 6 : (int)begin.DayOfWeek - 1;
                        var d = begin.AddDays(7 - weekday);
                        end = new DateTime(d.Year, d.Month, d.Day, 0, 0, 0);
                    }
                    else
                    {
                        end = this.AddInterval(begin, parameters);
                    }

                }
                else
                {
                    // add normal interval
                    end = this.AddInterval(begin, parameters);
                }

                // if end time is not "same" as end of interval (should get full months but end date is in middle of the month)
                // then last interval will be short

                if (end > parameters.EndDate) { end = parameters.EndDate; }

                intervals.Add(new RetrieveInterval() { IntervalBegin = begin, IntervalEnd = end });

                begin = end;

            }

            return intervals;
        }

        /// <summary>
        /// Adds interval from parameters
        /// </summary>
        /// <param name="d"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private DateTime AddInterval(DateTime d, RetrieveParams parameters)
        {
            if (parameters.IntervalFullYears > 0) 
            {
                return d.AddYears(parameters.IntervalFullYears);
            } 
            else if (parameters.IntervalFullMonths > 0)
            {
                return d.AddMonths(parameters.IntervalFullMonths);
            }
            else if (parameters.IntervalFullWeeks > 0)
            {
                return d.AddDays(parameters.IntervalFullWeeks * 7);
            }
            else if (parameters.IntervalMonths > 0)
            {
                return d.AddMonths(parameters.IntervalMonths);
            }
            else
            {
                var seconds = parameters.IntervalSeconds
                            + (parameters.IntervalMinutes * 60)
                            + (parameters.IntervalHours * 60 * 60)
                            + (parameters.IntervalDays * 24 * 60 * 60)
                            + (parameters.IntervalWeeks * 7 * 24 * 60 * 60);

                return d.AddSeconds(seconds);
            }
        }

        #endregion

        public IEnumerable<DataPoint> RetrieveData(IDataRepository repository, RetrieveParams parameters)
        {
            // -----------------------------------------------------------
            // Create intervals
            // -----------------------------------------------------------
            var intervals = this.CreateIntervals(parameters);

            // -----------------------------------------------------------
            // Create data for intervals
            // -----------------------------------------------------------
            DataValueItem summaryBetween = null;
            DataValueItem overlapBegin = null;
            DataValueItem overlapEnd = null;
            DataValueItem overlapInterval = null;

            DataPoint newPoint;
            var returnData = new List<DataPoint>();

            foreach(var interval in intervals)
            {
                // DataPoint for this interval
                newPoint = new DataPoint() { 
                    DataDate = interval.IntervalBegin, 
                    Value = 0, 
                    IntervalInSeconds = (int)Math.Round(this.SecondsBetween(interval.IntervalBegin, interval.IntervalEnd), 0, MidpointRounding.AwayFromZero)
                };

                // is there previous data entry which overlaps this interval
                (overlapBegin, overlapEnd, overlapInterval) = GetOverlapping(new DataValueItem[] { overlapBegin, overlapEnd, overlapInterval }, interval);

                if(overlapInterval != null)
                {
                    // ------------------------------------------------------------------------------------------
                    // Found previous data-entry which covers whole interval -> no need to get new data
                    // ------------------------------------------------------------------------------------------
                    newPoint.Value = CalculateOverlapValue(overlapInterval, interval.IntervalBegin, interval.IntervalEnd);
                }
                else
                {
                    // ------------------------------------------------------------------------------------------
                    // should get more data
                    // ------------------------------------------------------------------------------------------

                    // get summary data for this interval
                    summaryBetween = repository.GetSummaryBetween(interval.IntervalBegin, interval.IntervalEnd);

                    // check if summary data covers whole interval
                    if (summaryBetween != null && summaryBetween.BeginDate == interval.IntervalBegin && summaryBetween.EndDate == interval.IntervalEnd)
                    {
                        // ----------------------------------------------------------
                        // summary data covers whole interval -> no need to get more data
                        // ----------------------------------------------------------
                        newPoint.Value = summaryBetween.Value;
                    }
                    else
                    {
                        // summary data does not cover all -> should check overlapping data

                        if( overlapBegin != null && summaryBetween != null && summaryBetween.EndDate == interval.IntervalEnd )
                        {
                            // -------------------------------------------------------------------------
                            // data entry overlapping interval begin with summary data covers whole interval
                            // -> no need to get more data
                            // -------------------------------------------------------------------------

                            newPoint.Value = CalculateOverlapValue(overlapBegin, interval.IntervalBegin, interval.IntervalEnd) + summaryBetween.Value;
                        }
                        else
                        {
                            // should get at least data which overlaps end of interval
                            // -> we don't have it, so it must be fetched from repository
                            //    -> at the same time we will fetch all data which overlaps with this interval (especially needed at first round)
                            (overlapBegin, overlapEnd, overlapInterval) = 
                                GetOverlapping(
                                    repository.GetOverlapping(interval.IntervalBegin, interval.IntervalEnd), 
                                    interval
                                );

                            if (overlapInterval != null)
                            {
                                // Found data which covers (overlaps) whole interval (possible at first round)
                                // -> summary data should have been 'null' in this situation...
                                newPoint.Value = CalculateOverlapValue(overlapInterval, interval.IntervalBegin, interval.IntervalEnd);
                            }
                            else
                            {
                                // -------------------------------------------------------------------------
                                // count all together:
                                //  - data overlapping the begin of this interval
                                //  - summary data
                                //  - data overlapping the end of this interval
                                // -------------------------------------------------------------------------
                                newPoint.Value = CalculateOverlapValue(overlapBegin, interval.IntervalBegin, interval.IntervalEnd)
                                + (summaryBetween?.Value ?? 0)
                                + CalculateOverlapValue(overlapEnd, interval.IntervalBegin, interval.IntervalEnd);
                            }

                        }
                    }
                }

                returnData.Add(newPoint);
            }

            return returnData;
        }

        /// <summary>
        /// Returns data entries which overlaps given interval.
        /// data entries should not overlap each other!
        /// </summary>
        /// <param name="entries">data entries</param>
        /// <param name="interval">interval</param>
        /// <returns></returns>
        private (DataValueItem overlapBegin, DataValueItem overlapEnd, DataValueItem overlapInterval) GetOverlapping(IEnumerable<DataValueItem> entries, RetrieveInterval interval)
        {
            DataValueItem overlapBeginReturn = null;
            DataValueItem overlapEndReturn = null;
            DataValueItem overlapIntervalReturn = null;

            foreach (var entry in entries)
            {
                if (entry != null)
                {
                    switch (OverlapType(entry.BeginDate, entry.EndDate, interval.IntervalBegin, interval.IntervalEnd))
                    {
                        case EOverlapType.CoverAll:
                            overlapIntervalReturn = entry; // -> there shouldn't be any other entries!!
                            break;
                        case EOverlapType.OverlapBegin:
                            overlapBeginReturn = entry;
                            break;
                        case EOverlapType.OverlapEnd:
                            overlapEndReturn = entry;
                            break;
                        case EOverlapType.Inside:
                            // basically this kind of should not be found -> something wrong with the repository function...
                            break;
                        default:
                            break;
                    }
                }
            }

            return (overlapBegin: overlapBeginReturn, overlapEnd: overlapEndReturn, overlapInterval: overlapIntervalReturn);
        }

        private double CalculateOverlapValue(DataValueItem overlapEntry, DateTime beginDate, DateTime endDate)
        {
            if (overlapEntry == null) { return 0.0; }

            // count "value per second"
            var valuePerSecond = overlapEntry.Value / SecondsBetween(overlapEntry.BeginDate, overlapEntry.EndDate);

            switch (OverlapType(overlapEntry.BeginDate, overlapEntry.EndDate, beginDate, endDate))
            {
                case EOverlapType.CoverAll:
                    return valuePerSecond * SecondsBetween(beginDate, endDate);
                    
                case EOverlapType.OverlapBegin:
                    return valuePerSecond * SecondsBetween(beginDate, overlapEntry.EndDate);
                    
                case EOverlapType.OverlapEnd:
                    return valuePerSecond * SecondsBetween(overlapEntry.BeginDate, endDate);
                    
                case EOverlapType.Inside:
                    return overlapEntry.Value;

                case EOverlapType.NoOverlap:
                    return 0.0;

                default:
                    throw new Exception("OverlapType handling is missing!");
                    
            }
        }

        private double SecondsBetween(DateTime beginDate, DateTime endDate)
        {
            return System.Math.Abs((beginDate - endDate).TotalSeconds);
        }

        /// <summary>
        /// How two interval are related
        /// </summary>
        /// <param name="begin1"></param>
        /// <param name="end1"></param>
        /// <param name="begin2"></param>
        /// <param name="end2"></param>
        /// <returns></returns>
        private EOverlapType OverlapType(DateTime begin1, DateTime end1, DateTime begin2, DateTime end2)
        {
            if (begin1 <= begin2 && end1 >= end2)
            {
                return EOverlapType.CoverAll;
            }
            else if (begin1 < begin2 && (end1 > begin2 && end1 < end2))
            {
                return EOverlapType.OverlapBegin;
            }
            else if ((begin1 > begin2 && begin1 < end2) && end1 > end2)
            {
                return EOverlapType.OverlapEnd;
            }
            else if (begin1 > begin2 && end1 < end2)
            {
                return EOverlapType.Inside;
            }
            else
            {
                return EOverlapType.NoOverlap;
            }
        }
    }
}
