﻿using System.Collections.Generic;
using System.Linq;
using GeoAPI.Geometries;
using IsraelHiking.API.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetTopologySuite.Geometries;

namespace IsraelHiking.API.Tests.Services
{
    [TestClass]
    public class GpxSplitterServiceTests
    {
        private IGpxSplitterService _service;

        [TestInitialize]
        public void TestInitialize()
        {
            _service = new GpxSplitterService();
        }

        [TestMethod]
        public void GetMissingLines_GpxWithoutEnoughPoints_ShouldReturnEmptyList()
        {
            var gpxLine = new LineString(new[] {new Coordinate(1, 1), new Coordinate(2, 2)});

            var results = _service.GetMissingLines(gpxLine, new LineString[0]);

            Assert.AreEqual(0, results.Count);
        }

        [TestMethod]
        public void GetMissingLines_GpxDistanceIsTooSmall_ShouldReturnEmptyResults()
        {
            var gpxLines = new LineString(new[] {new Coordinate(1, 1), new Coordinate(2, 2), new Coordinate(3, 3)});

            var results = _service.GetMissingLines(gpxLines, new LineString[0]);

            Assert.AreEqual(0, results.Count);
        }

        [TestMethod]
        public void GetMissingLines_SimpleGpx_ShouldReturnIt()
        {
            var gpxLine = new LineString(new[] {new Coordinate(1, 1), new Coordinate(20, 20), new Coordinate(300, 300)});

            var results = _service.GetMissingLines(gpxLine, new LineString[0]);

            Assert.AreEqual(1, results.Count);
        }

        [TestMethod]
        public void GetMissingLines_GpxCloseToALine_ShouldSplitIt()
        {
            var gpxLine = new LineString(new[]
            {
                new Coordinate(1, 1),
                new Coordinate(20, 20),
                new Coordinate(300, 300),
                new Coordinate(400, 400),
                new Coordinate(500, 500),
                new Coordinate(600, 600),
                new Coordinate(700, 700),
            });

            var results = _service.GetMissingLines(gpxLine,
                new[] {new LineString(new[] {new Coordinate(399, 399), new Coordinate(399, 0)})});

            Assert.AreEqual(2, results.Count);
        }

        [TestMethod]
        public void SplitSelfLoops_GpxSharpTShape_ShouldSplitIt()
        {
            var gpxLine = new LineString(new[]
            {
                new Coordinate(0, 0),
                new Coordinate(100, 0),
                new Coordinate(200, 0),
                new Coordinate(300, 0),
                new Coordinate(300, 300),
                new Coordinate(301, 0),
                new Coordinate(400, 0),
                new Coordinate(600, 0)
            });

            var results = _service.SplitSelfLoops(gpxLine);

            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(5, results.First().Count);
            Assert.AreEqual(3, results.Last().Count);
        }

        [TestMethod]
        public void SplitSelfLoops_Gpx90DegreesTShape_ShouldSplitIt()
        {
            var gpxLine = new LineString(new[]
            {
                new Coordinate(0, 0),
                new Coordinate(100, 0),
                new Coordinate(300, 0),
                new Coordinate(300, 300),
                new Coordinate(301, 300),
                new Coordinate(301, 0),
                new Coordinate(400, 0),
                new Coordinate(600, 0)
            });

            var results = _service.SplitSelfLoops(gpxLine);

            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(5, results.First().Count);
            Assert.AreEqual(3, results.Last().Count);
        }

        [TestMethod]
        public void SplitSelfLoops_Gpx90DegreesTShapeNegative_ShouldSplitIt()
        {
            var gpxLine = new LineString(new[]
            {
                new Coordinate(0, 0),
                new Coordinate(100, 0),
                new Coordinate(300, 0),
                new Coordinate(300, -300),
                new Coordinate(301, -300),
                new Coordinate(301, 0),
                new Coordinate(400, 0),
                new Coordinate(600, 0)
            });

            var results = _service.SplitSelfLoops(gpxLine);

            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(5, results.First().Count);
            Assert.AreEqual(3, results.Last().Count);
        }

        [TestMethod]
        public void SplitSelfLoops_Gpx90DegreesLassoShape_ShouldSplitItAndRemoveDuplication()
        {
            var gpxLine = new LineString(new[]
            {
                new Coordinate(0, 0),
                new Coordinate(100, 0),
                new Coordinate(175, 0),
                new Coordinate(400, 0),
                new Coordinate(400, 200),
                new Coordinate(200, 200),
                new Coordinate(200, 1),
                new Coordinate(150, 1),
                new Coordinate(50, 1)
            });

            var results = _service.SplitSelfLoops(gpxLine);

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(6, results.First().Count);
        }

        /// <summary>
        ///     __   __         __   __
        /// ___|  |_|  | => ___|  |_| _|
        /// ___________|              
        /// </summary>
        [TestMethod]
        public void SplitSelfLoops_GpxCamelShape_ShouldSplitItAndRemoveLowerPart()
        {
            var gpxLine = new LineString(new[]
            {
                new Coordinate(0, 0),
                new Coordinate(100, 0),
                new Coordinate(175, 0),
                new Coordinate(400, 0),
                new Coordinate(400, 200),
                new Coordinate(500, 200),
                new Coordinate(500, 0),
                new Coordinate(800, 0),
                new Coordinate(1000, 0),
                new Coordinate(1000, 200),
                new Coordinate(1100, 200),
                new Coordinate(1100, -1),
                new Coordinate(1050, -1),
                new Coordinate(900, -1),
                new Coordinate(700, -1),
                new Coordinate(450, -1),
                new Coordinate(300, -1),
                new Coordinate(100, -1),
            });

            var results = _service.SplitSelfLoops(gpxLine);

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(13, results.First().Count);
        }

        /// <summary>
        ///     __ |        __       |
        /// ___| _|| => ___| _| ,    | 
        ///     |__|              |__|
        /// </summary>
        [TestMethod]
        public void SplitSelfLoops_GpxQuestionMarkShape_ShouldSplitItAndRemoveDuplication()
        {
            var gpxLine = new LineString(new[]
            {
                new Coordinate(0, 0),
                new Coordinate(200, 0),
                new Coordinate(400, 0),
                new Coordinate(400, 200),
                new Coordinate(600, 200),
                new Coordinate(600, 0),
                new Coordinate(400, -1),
                new Coordinate(400, -200),
                new Coordinate(600, -200),
                new Coordinate(601, 0),
                new Coordinate(601, 100),
                new Coordinate(601, 200),
                new Coordinate(601, 400),
                new Coordinate(601, 600)
            });

            var results = _service.SplitSelfLoops(gpxLine);

            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(6, results.First().Count);
            Assert.AreEqual(8, results.Last().Count);
        }
    }
}
