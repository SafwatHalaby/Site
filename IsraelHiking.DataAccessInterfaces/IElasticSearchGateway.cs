﻿using System.Collections.Generic;
using System.Threading.Tasks;
using GeoAPI.Geometries;
using IsraelHiking.Common;
using NetTopologySuite.Features;

namespace IsraelHiking.DataAccessInterfaces
{
    public interface IElasticSearchGateway
    {
        void Initialize(string uri = "http://localhost:9200/");
        Task<List<Feature>> Search(string searchTerm, string fieldName);
        Task UpdateDataZeroDownTime(List<Feature> names, List<Feature> highways);
        Task UpdateHighwaysData(List<Feature> features);
        Task UpdateNamesData(Feature feature);
        Task<List<Feature>> GetHighways(Coordinate northEast, Coordinate southWest);
        Task<List<Feature>> GetPointsOfInterest(Coordinate northEast, Coordinate southWest, string[] categories);
        Task<Feature> GetPointOfInterestById(string id, string source, string type);
        Task<Rating> GetRating(string id, string source);
        Task UpdateRating(Rating rating);
    }
}