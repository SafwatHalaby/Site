﻿export class Urls {
    // api
    public static baseAddress = window.location.protocol + "//" + window.location.host;
    public static apiBase = Urls.baseAddress + "/api/";
    public static urls = Urls.apiBase + "urls/";
    public static elevation = Urls.apiBase + "elevation";
    public static routing = Urls.apiBase + "routing";
    public static itmGrid = Urls.apiBase + "itmGrid";
    public static files = Urls.apiBase + "files";
    public static fileFormats = Urls.files + "/formats";
    public static openFile = Urls.apiBase + "files/open";
    public static search = Urls.apiBase + "search/";
    public static images = Urls.apiBase + "images/";
    public static colors = Urls.images + "colors/";
    public static translations = Urls.baseAddress + "/translations/";
    public static osm = Urls.apiBase + "osm/";
    public static osmConfiguration = Urls.osm + "configuration";
    public static osmTrace = Urls.osm + "trace/";
    public static userLayers = Urls.apiBase + "userLayers/";
    public static poi = Urls.apiBase + "poi/";
    public static poiCategories = Urls.poi + "categories/";
    public static poiImage = Urls.poi + "image/";
    public static rating = Urls.apiBase + "rating/";
    
    public static DEFAULT_TILES_ADDRESS = "/Tiles/{z}/{x}/{y}.png";
    public static MTB_TILES_ADDRESS = "/mtbTiles/{z}/{x}/{y}.png";
    public static OVERLAY_TILES_ADDRESS = "/OverlayTiles/{z}/{x}/{y}.png";
    public static OVERLAY_MTB_ADDRESS = "/OverlayMTB/{z}/{x}/{y}.png";
}
