﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using IsraelHiking.API.Converters;
using IsraelHiking.API.Executors;
using IsraelHiking.API.Services;
using IsraelHiking.API.Services.Poi;
using IsraelHiking.Common;
using IsraelHiking.DataAccessInterfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NSubstitute;
using OsmSharp;
using OsmSharp.Complete;
using OsmSharp.Tags;

namespace IsraelHiking.API.Tests.Services.Poi
{
    [TestClass]
    public class OsmPointsOfInterestAdapterTests : BasePointsOfInterestAdapterTestsHelper
    {
        private OsmPointsOfInterestAdapter _adapter;
        private IElasticSearchGateway _elasticSearchGateway;
        private IElevationDataStorage _elevationDataStorage;
        private IHttpGatewayFactory _httpGatewayFactory;
        private IOsmGeoJsonPreprocessorExecutor _osmGeoJsonPreprocessorExecutor;
        private IOsmRepository _osmRepository;
        private IDataContainerConverterService _dataContainerConverterService;
        private ITagsHelper _tagsHelper;

        [TestInitialize]
        public void TestInitialize()
        {
            _elasticSearchGateway = Substitute.For<IElasticSearchGateway>();
            _elevationDataStorage = Substitute.For<IElevationDataStorage>();
            _httpGatewayFactory = Substitute.For<IHttpGatewayFactory>();
            _tagsHelper = new TagsHelper(new OptionsWrapper<ConfigurationData>(new ConfigurationData()));
            _osmGeoJsonPreprocessorExecutor = new OsmGeoJsonPreprocessorExecutor(Substitute.For<ILogger>(), new OsmGeoJsonConverter(), _tagsHelper);
            _osmRepository = Substitute.For<IOsmRepository>();
            _dataContainerConverterService = Substitute.For<IDataContainerConverterService>();
            _adapter = new OsmPointsOfInterestAdapter(_elasticSearchGateway, _elevationDataStorage, _httpGatewayFactory, _osmGeoJsonPreprocessorExecutor, _osmRepository, _dataContainerConverterService, _tagsHelper);
        }

        private IOsmGateway SetupHttpFactory()
        {
            var gateway = Substitute.For<IOsmGateway>();
            _httpGatewayFactory.CreateOsmGateway(Arg.Any<TokenAndSecret>()).Returns(gateway);
            return gateway;
        }

        [TestMethod]
        public void GetPointsOfInterest_FilterRelevant_ShouldReturnEmptyElist()
        {
            _elasticSearchGateway.GetPointsOfInterest(null, null, null).Returns(new List<Feature>
            {
                new Feature { Geometry = new Point(null), Attributes = new AttributesTable() }
            });

            var results = _adapter.GetPointsOfInterest(null, null, null, null).Result;

            Assert.AreEqual(0, results.Length);
        }

        [TestMethod]
        public void GetPointsOfInterest_EnglishTitleOnly_ShouldReturnIt()
        {
            var name = "English name";
            var feature = GetValidFeature("poiId", _adapter.Source);
            feature.Attributes.DeleteAttribute(FeatureAttributes.NAME);
            feature.Attributes.AddAttribute("name:en", name);
            _elasticSearchGateway.GetPointsOfInterest(null, null, null).Returns(new List<Feature> { feature });

            var result = _adapter.GetPointsOfInterest(null, null, null, "en").Result;

            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(name, result.First().Title);
        }

        [TestMethod]
        public void GetPointsOfInterest_ImageAndDescriptionOnly_ShouldReturnIt()
        {
            var feature = GetValidFeature("poiId", _adapter.Source);
            feature.Attributes.DeleteAttribute(FeatureAttributes.NAME);
            feature.Attributes.AddAttribute(FeatureAttributes.IMAGE_URL, FeatureAttributes.IMAGE_URL);
            feature.Attributes.AddAttribute(FeatureAttributes.DESCRIPTION, FeatureAttributes.DESCRIPTION);
            _elasticSearchGateway.GetPointsOfInterest(null, null, null).Returns(new List<Feature> { feature });

            var result = _adapter.GetPointsOfInterest(null, null, null, "he").Result;

            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(string.Empty, result.First().Title);
        }

        [TestMethod]
        public void GetPointsOfInterestById_RouteWithMultipleImagesAndDescriptionOnly_ShouldReturnIt()
        {
            var poiId = "poiId";
            var feature = GetValidFeature(poiId, _adapter.Source);
            feature.Attributes.DeleteAttribute(FeatureAttributes.NAME);
            feature.Attributes.AddAttribute(FeatureAttributes.IMAGE_URL, FeatureAttributes.IMAGE_URL);
            feature.Attributes.AddAttribute(FeatureAttributes.IMAGE_URL + "1", FeatureAttributes.IMAGE_URL + "1");
            feature.Attributes.AddAttribute(FeatureAttributes.DESCRIPTION, FeatureAttributes.DESCRIPTION);
            _elasticSearchGateway.GetPointOfInterestById(poiId, _adapter.Source, Arg.Any<string>()).Returns(feature);
            _dataContainerConverterService.ToDataContainer(Arg.Any<byte[]>(), Arg.Any<string>()).Returns(
                new DataContainer
                {
                    routes = new List<RouteData>
                    {
                        new RouteData
                        {
                            segments = new List<RouteSegmentData>
                            {
                                new RouteSegmentData(),
                                new RouteSegmentData()
                            }
                        }
                    }
                });

            var result = _adapter.GetPointOfInterestById(poiId, null).Result;

            Assert.IsNotNull(result);
            Assert.AreEqual(string.Empty, result.Title);
            Assert.AreEqual(FeatureAttributes.DESCRIPTION, result.Description);
            Assert.AreEqual(2, result.ImagesUrls.Length);
            Assert.AreEqual(FeatureAttributes.IMAGE_URL, result.ImagesUrls.First());
            Assert.IsTrue(result.IsRoute);
        }

        [TestMethod]
        public void AddPointOfInterest_ShouldUpdateOsmAndElasticSearch()
        {
            var gateway = SetupHttpFactory();
            gateway.CreateElement(Arg.Any<string>(), Arg.Any<Node>()).Returns("42");
            var pointOfInterestToAdd = new PointOfInterestExtended
            {
                Location = new LatLng(),
                ImagesUrls = new string[0],
                Icon = _tagsHelper.GetIconsPerCategoryByType(Categories.POINTS_OF_INTEREST).Values.First().First().Icon
            };
            _dataContainerConverterService.ToDataContainer(Arg.Any<byte[]>(), Arg.Any<string>()).Returns(new DataContainer {routes = new List<RouteData>()});

            var resutls = _adapter.AddPointOfInterest(pointOfInterestToAdd, null, "he").Result;

            Assert.IsNotNull(resutls);
            _elasticSearchGateway.Received(1).UpdateNamesData(Arg.Any<Feature>());
        }

        [TestMethod]
        public void UpdatePoint_SyncImages()
        {
            var gateway = SetupHttpFactory();
            var pointOfInterest = new PointOfInterestExtended
            {
                ImagesUrls = new[] { "imageurl2", "imageurl1", "imageurl4" },
                Id = "1",
                Icon = "oldIcon",
                Type = OsmGeoType.Node.ToString().ToLower()
            };
            _dataContainerConverterService.ToDataContainer(Arg.Any<byte[]>(), Arg.Any<string>()).Returns(new DataContainer {routes = new List<RouteData>()});
            gateway.GetNode(pointOfInterest.Id).Returns(new Node
            {
                Id = 1,
                Tags = new TagsCollection
                {
                    new Tag("image", "imageurl1"),
                    new Tag("image1", "imageurl3"),
                }
            });

            var results = _adapter.UpdatePointOfInterest(pointOfInterest, null, "en").Result;

            CollectionAssert.AreEqual(pointOfInterest.ImagesUrls.OrderBy(i => i).ToArray(), results.ImagesUrls.OrderBy(i => i).ToArray());
        }

        [TestMethod]
        public void GetPointsForIndexing_ShouldRemoveKklRoutes()
        {
            var memoryStream = new MemoryStream();
            var osmNamesDictionary = new Dictionary<string, List<ICompleteOsmGeo>>
            {
                {
                    "name",
                    new List<ICompleteOsmGeo>
                    {
                        new Node
                        {
                            Id = 10,
                            Tags = new TagsCollection
                            {
                                {"natural", "spring"},
                            }
                        },
                        new CompleteRelation
                        {
                            Tags = new TagsCollection
                            {
                                {"operator", "kkl"},
                                {"route", "mtb"}
                            },
                            Members = new[]
                            {
                                new CompleteRelationMember {Member = new CompleteWay(), Role = "outer"}
                            }
                        }
                    }
                },
            };
            _osmRepository.GetElementsWithName(memoryStream).Returns(osmNamesDictionary);
            _osmRepository.GetPointsWithNoNameByTags(memoryStream, Arg.Any<List<KeyValuePair<string, string>>>()).Returns(new List<Node>());

            var results = _adapter.GetPointsForIndexing(memoryStream).Result;

            Assert.AreEqual(1, results.Count);
        }
    }
}
