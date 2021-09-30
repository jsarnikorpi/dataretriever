using DataRetrieve;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataRetrieveTest
{
    public class TestRepository : IDataRepository
    {
        public List<DataEntry> MockData { get; set; }

        public int GetOverLappingCalled { get; private set; } = 0;
        public int GetSummaryCalled { get; private set; } = 0;
        public int GetCalledTotal => GetOverLappingCalled + GetSummaryCalled;

        public TestRepository()
        {
            MockData = new List<DataEntry>();
        }

        public IEnumerable<DataValueItem> GetOverlapping(DateTime begin, DateTime end)
        {
            GetOverLappingCalled++;

            var query = MockData.Where(x =>
                (x.BeginDate < begin && x.EndDate > end)
                || (x.BeginDate < begin && x.EndDate > begin && x.EndDate < end)
                || (x.BeginDate > begin && x.BeginDate < end && x.EndDate > end)
            );

            return query.Select(x => new DataValueItem() { BeginDate = x.BeginDate, EndDate = x.EndDate, Value = x.Value } ).ToList();
        }

        public DataValueItem GetSummaryBetween(DateTime begin, DateTime end)
        {
            GetSummaryCalled++;

            var entries = MockData.Where(x => x.BeginDate >= begin && x.EndDate <= end).ToList();

            if (entries.Count == 0)
            {
                return null;
            } 
            else
            {
                return new DataValueItem()
                {
                    BeginDate = entries.Min(e => e.BeginDate),
                    EndDate = entries.Max(e => e.EndDate),
                    Value = entries.Sum(e => e.Value)
                };
            }

        }
    }
}
