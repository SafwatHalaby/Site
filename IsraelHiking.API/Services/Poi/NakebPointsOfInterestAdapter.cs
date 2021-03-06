﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GeoAPI.Geometries;
using IsraelHiking.Common;
using IsraelHiking.DataAccessInterfaces;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;

namespace IsraelHiking.API.Services.Poi
{
    /// <summary>
    /// Adapts from nakeb interface to business logic point of interest
    /// </summary>
    public class NakebPointsOfInterestAdapter : BasePointsOfInterestAdapter, IPointsOfInterestAdapter
    {
        /// <inheritdoc />
        public string Source => Sources.NAKEB;

        private readonly INakebGateway _nakebGateway;
        private readonly ILogger _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="nakebGateway"></param>
        /// <param name="elevationDataStorage"></param>
        /// <param name="elasticSearchGateway"></param>
        /// <param name="dataContainerConverterService"></param>
        /// <param name="logger"></param>
        public NakebPointsOfInterestAdapter(INakebGateway nakebGateway,
            IElevationDataStorage elevationDataStorage,
            IElasticSearchGateway elasticSearchGateway,
            IDataContainerConverterService dataContainerConverterService,
            ILogger logger) : base(elevationDataStorage, elasticSearchGateway, dataContainerConverterService)
        {
            _nakebGateway = nakebGateway;
            _logger = logger;
        }

        /// <inheritdoc />
        public Task<PointOfInterest[]> GetPointsOfInterest(Coordinate northEast, Coordinate southWest, string[] categories, string language)
        {
            // points are stored in elastic search
            return Task.Run(() => new PointOfInterest[0]);
        }

        /// <inheritdoc />
        public async Task<PointOfInterestExtended> GetPointOfInterestById(string id, string language, string type = "")
        {
            var featureCollection = await _nakebGateway.GetById(int.Parse(id));
            var mainFeature = featureCollection.Features.FirstOrDefault(f => f.Geometry is LineString);
            var poiItem = await ConvertToPoiItem<PointOfInterestExtended>(mainFeature, "he");
            await AddExtendedData(poiItem, mainFeature, language);
            poiItem.IsEditable = false;
            poiItem.IsRoute = true;
            poiItem.SourceImageUrl = "https://www.nakeb.co.il/static/images/hikes/logo_1000x667.jpg";
            return poiItem;
        }

        /// <inheritdoc />
        public Task<PointOfInterestExtended> AddPointOfInterest(PointOfInterestExtended pointOfInterest, TokenAndSecret tokenAndSecret, string language)
        {
            throw new Exception("Nakeb does not support adding.");
        }

        /// <inheritdoc />
        public Task<PointOfInterestExtended> UpdatePointOfInterest(PointOfInterestExtended pointOfInterest, TokenAndSecret tokenAndSecret, string language)
        {
            throw new Exception("Nakeb does not support updating.");
        }

        /// <inheritdoc />
        public async Task<List<Feature>> GetPointsForIndexing(Stream memoryStream)
        {
            _logger.LogInformation("Getting data from Nakeb.");
            var features = await _nakebGateway.GetAll();
            _logger.LogInformation($"Got {features.Count} routes from Nakeb.");
            return features;
        }
    }
}
