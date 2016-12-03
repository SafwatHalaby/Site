﻿using IsraelHiking.Common;
using IsraelHiking.DataAccess.ElasticSearch;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IsraelHiking.DataAccess.Tests.ElasticSearch
{
    [TestClass]
    public class ElasticSearchGatewayTests
    {
        [TestMethod]
        [Ignore]
        public void Search_ShouldReturnResults()
        {
            var gateway = new ElasticSearchGateway(new TraceLogger());
            gateway.Initialize();
            var results = gateway.Search("מנות", "name").Result;
            Assert.AreEqual(10, results.Count);
        }

        [TestMethod]
        [Ignore]
        public void GetHighways_ShouldReturnResults()
        {
            var gateway = new ElasticSearchGateway(new TraceLogger());
            gateway.Initialize();
            var northEast = new LatLng(31.7553, 35.0516);
            var southWest = new LatLng(31.7467, 35.0251);
            var results = gateway.GetHighways(northEast, southWest).Result;
            Assert.AreEqual(36, results.Count);
        }
    }
}

