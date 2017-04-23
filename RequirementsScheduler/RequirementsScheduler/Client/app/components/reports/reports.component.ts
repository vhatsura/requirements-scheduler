import { Component, OnInit, Output, EventEmitter, Inject } from '@angular/core';
import { Http } from '@angular/http';

import { PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser, isPlatformServer } from '@angular/common';

import { GtConfig, GenericTableComponent } from '@angular-generic-table/core';

@Component({
    selector: 'reports',
    template: require('./reports.component.html')
})
export class ReportsComponent implements OnInit {
     public configObject: GtConfig<any>;

    @Output() data = new EventEmitter();

    constructor(
        @Inject(PLATFORM_ID) private platformId: Object,
        private http: Http) {
        this.configObject = {
            settings: [
                {
                    objectKey: 'experimentId',
                    visible: true,
                    sort: 'enable',
                    columnOrder: 0
                },
                {
                    objectKey: 'n',
                    visible: true,
                    sort: 'enable',
                    columnOrder: 1
                },
                {
                    objectKey: 'requirementsAmount',
                    visible: true,
                    sort: 'enable',
                    columnOrder: 2
                },
                {
                    objectKey: 'percentages',
                    visible: true,
                    sort: 'enable',
                    columnOrder: 3
                },
                {
                    objectKey: 'borders',
                    visible: true,
                    sort: 'enable',
                    columnOrder: 4
                },
                {
                    objectKey: 'onlineResolvedConflictPercentage',
                    visible: true,
                    sort: 'enable',
                    columnOrder: 5
                },
                {
                    objectKey: 'stop1Percentage',
                    visible: true,
                    sort: 'enable',
                    columnOrder: 6
                },
                {
                    objectKey: 'stop2Percentage',
                    visible: true,
                    sort: 'enable',
                    columnOrder: 7
                },
                {
                    objectKey: 'stop3Percentage',
                    visible: true,
                    sort: 'desc',
                    columnOrder: 8
                },
                {
                    objectKey: 'stop4Percentage',
                    visible: true,
                    sort: 'desc',
                    columnOrder: 9
                },
                {
                    objectKey: 'deltaCmaxMax',
                    visible: true,
                    sort: 'desc',
                    columnOrder: 10
                },
                {
                    objectKey: 'deltaCmaxAverage',
                    visible: true,
                    sort: 'desc',
                    columnOrder: 11
                },
                {
                    objectKey: 'onlineExecutionTime',
                    visible: true,
                    sort: 'desc',
                    columnOrder: 12
                }
            ],
            fields: [
                {
                    name: 'Experiment id',
                    objectKey: 'experimentId',
                    classNames: 'clickable sort-string',
                    expand: true
                },
                {
                    name: 'Tests amount',
                    objectKey: 'n',
                    classNames: 'sort-numeric',
                },
                {
                    name: 'Requirements amount',
                    objectKey: 'requirementsAmount',
                    classNames: 'sort-numeric',
                },
                {
                    name: 'n1, n2, n12, n21, %',
                    objectKey: 'percentages',
                    value:function(row){ return `${row.n1Percentage}, ${row.n2Percentage}, ${row.n12Percentage}, ${row.n21Percentage}`; }
                },
                {
                    name: 'L, %',
                    objectKey: 'borders',
                    classNames: 'sort-string',
                    value:function(row){ return `[${row.aBorder}; ${row.bBorder}]`;}
                },
                {
                    name: 'Conflicts resolved on on-line, %',
                    objectKey: 'onlineResolvedConflictPercentage',
                    classNames: 'sort-numeric',
                    value:function(row) { 
                        if(row.onlineResolvedConflictAmount == 0)
                            return 0;
                        return ((row.onlineResolvedConflictAmount / (row.onlineResolvedConflictAmount + row.onlineUnResolvedConflictAmount)) * 100).toFixed(1);
                    }
                },
                {
                    name: 'STOP1, %',
                    objectKey: 'stop1Percentage',
                    classNames: 'sort-numeric',
                },
                {
                    name: 'STOP2, %',
                    objectKey: 'stop2Percentage',
                    classNames: 'sort-numeric',
                },
                {
                    name: 'STOP3, %',
                    objectKey: 'stop3Percentage',
                    classNames: 'clickable sort-string'
                },
                {
                    name: 'STOP4, %',
                    objectKey: 'stop4Percentage',
                    classNames: 'clickable sort-string'
                },
                {
                    name: 'DeltaCmax max',
                    objectKey: 'deltaCmaxMax',
                    classNames: 'clickable sort-numeric'
                },
                {
                    name: 'DeltaCmax average',
                    objectKey: 'deltaCmaxAverage',
                    classNames: 'clickable sort-numeric'
                },
                {
                    name: 'Online execution time',
                    objectKey: 'onlineExecutionTime',
                    classNames: 'clickable sort-string'
                }
            ],
            data: []
        };    
    }

    ngOnInit(): void {
        this.http.get('/api/reports').subscribe(result => {
            this.configObject.data.length = 0;
                this.configObject.data = this.configObject.data.concat(result.json());
                return this.configObject.data;
        });
    }

    isBrowser() {
        return isPlatformBrowser(this.platformId);
    }
}

interface ExperimentReport {
    id: number;
    experimentId: string;
    
    N: number;
    requirementsAmount: number;

    n1Percentage: number;
    n2Percentage: number;
    n12Percentage: number;
    n21Percentage: number;
    
    aBorder: number;
    bBorder: number;
    
    offlineResolvedConflictAmount: number;
    onlineResolvedConflictAmount: number;
    onlineUnResolvedConflictAmount: number;

    stop1Percentage: number;   
    stop2Percentage: number;   
    stop3Percentage: number;   
    stop4Percentage: number;   

    onlineExecutionTime: number;    
    deltaCmaxAverage: number;
    deltaCmaxMax: number;
    conflictsAmount: number;
    conflictsResolutionAmount: number;
    onlineResolvedConflictPercentage: number;
}
