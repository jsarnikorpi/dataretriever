using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using DataRetrieve;

namespace DataRetrieveTest
{
    public class DataRetrieveTests
    {
        private TestRepository _repo;

        [SetUp]
        public void Setup()
        {
            // ------------------------------------------------------
            // MockData: 10000 data entries, one per hour, value = 10/h
            // ------------------------------------------------------
            _repo = new TestRepository();
            var d1 = new DateTime(2020, 1, 1, 0, 0, 0);
            DateTime d2;

            for (var i = 0; i < 10000; i++)
            {
                d2 = d1.AddHours(1);

                _repo.MockData.Add(new DataEntry()
                {
                    BeginDate = d1,
                    EndDate = d2,
                    Value = 10
                });

                d1 = d2;
            }
        }

        [Test]
        public void SummaryWithOverlap()
        {
            var retriever = new DataRetriever();
            var param = new RetrieveParams()
            {
                 BeginDate = new DateTime(2020, 1, 1, 12, 0, 0, 0),
                 EndDate = new DateTime(2020, 1, 1, 16, 0, 0, 0),
                 IntervalMinutes = 90
            };

            var result = retriever.RetrieveData(_repo, param).ToList();

            Assert.AreEqual(3, result.Count(), "Result count");
            Assert.AreEqual(15, result[0].Value, "First result value");
            Assert.AreEqual(15, result[1].Value, "Second result value");
            // last interval should be short
            Assert.AreEqual(10, result[2].Value, "Third result value");
        }

        [Test]
        public void OverlapCoverAll_IntervalMinutes()
        {
            var retriever = new DataRetriever();
            var param = new RetrieveParams()
            {
                BeginDate = new DateTime(2020, 1, 1, 12, 15, 0, 0),
                EndDate = new DateTime(2020, 1, 1, 12, 45, 0, 0),
                IntervalMinutes = 30
            };

            var result = retriever.RetrieveData(_repo, param).ToList();

            Assert.AreEqual(1, result.Count(), "Result count");
            Assert.AreEqual(5, result[0].Value, "Result value");
        }

        [Test]
        public void MultiSummaryWithOverlap_IntervalHours()
        {
            var retriever = new DataRetriever();
            var param = new RetrieveParams()
            {
                BeginDate = new DateTime(2020, 1, 1, 12, 30, 0, 0),
                EndDate = new DateTime(2020, 1, 1, 16, 30, 0, 0),
                IntervalHours = 4
            };

            var result = retriever.RetrieveData(_repo, param).ToList();

            Assert.AreEqual(1, result.Count(), "Result count");
            Assert.AreEqual(40, result[0].Value, "Result value");
        }

        [Test]
        public void MultiOverlap()
        {
            var retriever = new DataRetriever();
            var param = new RetrieveParams()
            {
                BeginDate = new DateTime(2020, 1, 1, 12, 30, 0, 0),
                EndDate = new DateTime(2020, 1, 2, 12, 30, 0, 0),
                IntervalMinutes = 120
            };

            var result = retriever.RetrieveData(_repo, param).ToList();

            // should get 12 data points (data from one day using 2 hours intervals)
            Assert.AreEqual(12, result.Count(), "Result count");
            // value will be 20 for every interval
            Assert.AreEqual(0, result.Where(x => x.Value != 20).Count(), "Result values");
            // check repository queries
            Assert.AreEqual(12, _repo.GetOverLappingCalled, "Repository searches for overlapping");
            Assert.AreEqual(12, _repo.GetSummaryCalled, "Repository searches for summary");
        }

        [Test]
        public void MultiSummary_IntervalDays()
        {
            var retriever = new DataRetriever();
            var param = new RetrieveParams()
            {
                BeginDate = new DateTime(2020, 1, 1, 12, 00, 0, 0),
                EndDate = new DateTime(2020, 2, 1, 12, 00, 0, 0),
                IntervalDays = 1
            };

            var result = retriever.RetrieveData(_repo, param).ToList();

            Assert.AreEqual(31, result.Count(), "Result count");
            // value of every data point should be 240
            Assert.AreEqual(0, result.Where(x => x.Value != 240).Count(), "Result values");

            // check repository queries
            Assert.AreEqual(0, _repo.GetOverLappingCalled, "Repository searches for overlapping");
            Assert.AreEqual(31, _repo.GetSummaryCalled, "Repository searches for summary");
        }

        [Test]
        public void OverlapCoversMultiple()
        {
            var retriever = new DataRetriever();
            var param = new RetrieveParams()
            {
                BeginDate = new DateTime(2020, 1, 1, 12, 15, 0, 0),
                EndDate = new DateTime(2020, 1, 1, 13, 00, 0, 0),
                IntervalMinutes = 15
            };

            var result = retriever.RetrieveData(_repo, param).ToList();

            Assert.AreEqual(3, result.Count(), "Result count");
            Assert.AreEqual(0, result.Where(x => x.Value != 2.5).Count(), "Result values");

            Assert.AreEqual(1, _repo.GetOverLappingCalled, "Repository searches for overlapping");
            Assert.AreEqual(1, _repo.GetSummaryCalled, "Repository searches for summary");
        }

    }
}