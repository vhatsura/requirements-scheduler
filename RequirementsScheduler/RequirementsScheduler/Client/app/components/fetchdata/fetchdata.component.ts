import { Component } from '@angular/core';
import { Http } from '@angular/http';

@Component({
    selector: 'fetchdata',
    template: require('./fetchdata.component.html')
})
export class FetchDataComponent {
    public reports: ExperimentReport[];

    constructor(http: Http) {
        http.get('/api/reports').subscribe(result => {
            this.reports = result.json();
            this.reports[0].nRatio = "25% 25% 25% 25%";
            this.reports[1].nRatio = "30% 30% 20% 20%";
        });
    }
}

interface ExperimentReport {
    id: number;
    experimentId: number;
    nRatio: string;
    stop1: number;   
    stop2: number;   
    stop3: number;   
    stop4: number;   

    executionTime: number;    
    deltaCmaxAverage: number;
    deltaCmaxMax: number;
    conflictsAmount: number;
    conflictsResolutionAmount: number;
}
