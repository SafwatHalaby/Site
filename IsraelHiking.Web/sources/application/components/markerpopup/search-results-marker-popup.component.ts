﻿import { Component, ApplicationRef } from "@angular/core";
import { Http } from "@angular/http";

import { ResourcesService } from "../../services/resources.service";
import { PoiService, IPointOfInterestExtended } from "../../services/poi.service";
import { RoutesService } from "../../services/layers/routelayers/routes.service";
import { MapService } from "../../services/map.service";
import { ElevationProvider } from "../../services/elevation.provider";
import { BaseMarkerPopupComponent } from "./base-marker-popup.component";
import * as Common from "../../common/IsraelHiking";


@Component({
    selector: "search-results-marker-popup",
    templateUrl: "./search-results-marker-popup.component.html"
})
export class SearchResultsMarkerPopupComponent extends BaseMarkerPopupComponent {
    public id: string;
    public source: string;
    public type: string;

    private poiExtended: IPointOfInterestExtended;

    constructor(resources: ResourcesService,
        http: Http,
        applicationRef: ApplicationRef,
        elevationProvider: ElevationProvider,
        private poiService: PoiService,
        private routesService: RoutesService,
        private mapService: MapService) {
        super(resources, http, applicationRef, elevationProvider);
    }

    public selectRoute = (routeData: Common.RouteData): void => {
        console.log(routeData);
        throw new Error("This function must be assigned by containing layer!");
    };
    public clearSelectedRoute = (): void => { throw new Error("This function must be assigned by the containing layer!") };

    public setIdSourceAndType(id: string, source: string, type: string) {
        this.id = id;
        this.source = source;
        this.type = type;
        this.poiService.getPoint(this.id, this.source, this.type).then((response) => {
            this.poiExtended = response.json() as IPointOfInterestExtended;
            this.mapService.routesJsonToRoutesObject(this.poiExtended.dataContainer.routes);
            this.selectRoute(this.poiExtended.dataContainer.routes[0]);
        });
    }

    public convertToRoute = () => {
        let routesCopy = JSON.parse(JSON.stringify(this.poiExtended.dataContainer.routes)) as Common.RouteData[];
        this.mapService.routesJsonToRoutesObject(routesCopy);
        this.routesService.setData([routesCopy[0]]);
        this.clearSelectedRoute();
        this.marker.closePopup();
    }
}