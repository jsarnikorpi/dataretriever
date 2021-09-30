using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using DataRetrieve;

namespace DataRetrieveTest
{
    public class IntervalTests
    {

        [SetUp]
        public void Setup()
        {
           
        }

        [Test]
        public void FullWeekIntervalTest()
        {
            var retriever = new DataRetriever();

            var param = new RetrieveParams()
            {
                BeginDate = new DateTime(2021, 4, 30, 12, 0, 0, 0), // -> Friday
                EndDate = new DateTime(2021, 5, 13, 14, 0, 0, 0),    // -> Wednasday
                IntervalFullWeeks = 1
            };

            var intervals = retriever.CreateIntervals(param);

            Assert.AreEqual(new DateTime(2021, 4, 30, 12, 0, 0, 0), intervals[0].IntervalBegin, "Should be same as begin date");
            Assert.AreEqual(new DateTime(2021, 5, 3, 0, 0, 0, 0), intervals[0].IntervalEnd, "Should be next Monday");

            Assert.AreEqual(new DateTime(2021, 5, 3, 0, 0, 0, 0), intervals[1].IntervalBegin, "Should be full week (Monday)");
            Assert.AreEqual(new DateTime(2021, 5, 10, 0, 0, 0, 0), intervals[1].IntervalEnd, "Should be full week (Sunday)");

            Assert.AreEqual(new DateTime(2021, 5, 10, 0, 0, 0, 0), intervals[2].IntervalBegin, "Should be last week Monday");
            Assert.AreEqual(new DateTime(2021, 5, 13, 14, 0, 0, 0), intervals[2].IntervalEnd, "Should be same as end date");
        }


    }
}
