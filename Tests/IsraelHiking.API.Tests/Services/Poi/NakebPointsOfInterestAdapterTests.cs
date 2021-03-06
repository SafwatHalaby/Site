﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GeoAPI.Geometries;
using IsraelHiking.API.Services;
using IsraelHiking.API.Services.Poi;
using IsraelHiking.Common;
using IsraelHiking.DataAccessInterfaces;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NSubstitute;

namespace IsraelHiking.API.Tests.Services.Poi
{
    [TestClass]
    public class NakebPointsOfInterestAdapterTests : BasePointsOfInterestAdapterTestsHelper
    {
        private NakebPointsOfInterestAdapter _adapter;
        private INakebGateway _nakebGateway;
        private IElevationDataStorage _elevationDataStorage;
        private IElasticSearchGateway _elasticSearchGateway;

        [TestInitialize]
        public void TestInitialize()
        {
            _nakebGateway = Substitute.For<INakebGateway>();
            _elevationDataStorage = Substitute.For<IElevationDataStorage>();
            _elasticSearchGateway = Substitute.For<IElasticSearchGateway>();
            _adapter = new NakebPointsOfInterestAdapter(_nakebGateway, _elevationDataStorage, _elasticSearchGateway, Substitute.For<IDataContainerConverterService>(),  Substitute.For<ILogger>());
        }

        [TestMethod]
        public void GetPointsOfInterest_ShouldReturnEmptyList()
        {
            Assert.AreEqual(0, _adapter.GetPointsOfInterest(null, null, null, null).Result.Length);
        }

        [TestMethod]
        public void GetPointOfInterestById_ShouldGetIt()
        {
            var poiId = "42";
            var language = "en";
            var featureCollection = new FeatureCollection
            {
                Features = { GetValidFeature(poiId, _adapter.Source) }
            };
            _nakebGateway.GetById(42).Returns(featureCollection);

            var results = _adapter.GetPointOfInterestById(poiId, language).Result;

            Assert.IsNotNull(results);
            Assert.IsNotNull(results.SourceImageUrl);
            Assert.IsFalse(results.IsEditable);
            _elevationDataStorage.Received().GetElevation(Arg.Any<Coordinate>());
            _elasticSearchGateway.Received().GetRating(poiId, Arg.Any<string>());
        }

        [TestMethod]
        public void AddPointOfInterest_ShouldThrow()
        {
            Assert.ThrowsException<Exception>(() => _adapter.AddPointOfInterest(null, null, null).Result);
        }

        [TestMethod]
        public void UpdatePointOfInterest_ShouldThrow()
        {
            Assert.ThrowsException<Exception>(() => _adapter.UpdatePointOfInterest(null, null, null).Result);
        }

        [TestMethod]
        public void GetPointsForIndexing_ShouldGetAllPointsFromGateway()
        {
            var featuresList = new List<Feature> { new Feature(null, null)};
            _nakebGateway.GetAll().Returns(featuresList);

            var points = _adapter.GetPointsForIndexing(null).Result;

            Assert.AreEqual(featuresList.Count, points.Count);
        }
    }
}
