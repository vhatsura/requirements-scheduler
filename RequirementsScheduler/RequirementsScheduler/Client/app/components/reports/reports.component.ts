import { Component, OnInit, Output, EventEmitter, Inject, ViewChild } from '@angular/core';
import { Http } from '@angular/http';

import { PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser, isPlatformServer } from '@angular/common';

import { GtConfig, GenericTableComponent } from 'angular-generic-table';

import { AlertService } from '../../services/alert.service';

@Component({
    selector: 'reports',
    styles: [`
    .margin-bottom {
        margin-bottom:10px;
    }
    `],
    templateUrl: './reports.component.html'
})
export class ReportsComponent implements OnInit {

    public configObject: GtConfig<any>;

    public testsAmountFilter: number;
    public requirementsAmountFilter: number;
    
    public n1Filter: number;
    public n2Filter: number;
    public n12Filter: number;
    public n21Filter: number;

    public aBorderFilter: number;
    public bBorderFilter: number;

    public isChartsVisible: boolean = false;
    public isFilterApplied: boolean = false;

    public firstLineChartOptions = {
        chartType: 'LineChart',
        dataTable: [ ],
        options: {
            'title': 'Chart',
            'pointSize': 3,
            'legend': {
                position: 'bottom'
            }
        }
    };

    public secondLineChartOptions = {
        chartType: 'LineChart',
        dataTable: [ ],
        options: {
            'title': 'Chart',
            'pointSize': 3,
            'legend': {
                position: 'bottom'
            }
        }
    };

    @Output() data = new EventEmitter();

    @ViewChild(GenericTableComponent)
    private myTable: GenericTableComponent<any, any>;

    constructor(
        @Inject(PLATFORM_ID) private platformId: Object,
        private http: Http,
        private alertService: AlertService) {
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
                    classNames: 'sort-numeric'
                },
                {
                    name: 'Requirements amount',
                    objectKey: 'requirementsAmount',
                    classNames: 'sort-numeric'
                },
                {
                    name: 'n1, n2, n12, n21, %',
                    objectKey: 'percentages',
                    value: function(row){ return `${row.n1Percentage}, ${row.n2Percentage}, ${row.n12Percentage}, ${row.n21Percentage}`; }
                },
                {
                    name: 'L, %',
                    objectKey: 'borders',
                    classNames: 'sort-string',
                    value: function(row){ return `[${row.aBorder}; ${row.bBorder}]`; }
                },
                {
                    name: 'Conflicts resolved on on-line, %',
                    objectKey: 'onlineResolvedConflictPercentage',
                    classNames: 'sort-numeric',
                    value: function(row) { 
                        if (row.onlineResolvedConflictAmount === 0)
                            return 0;
                        return ((row.onlineResolvedConflictAmount / (row.onlineResolvedConflictAmount + row.onlineUnResolvedConflictAmount)) * 100).toFixed(1);
                    }
                },
                {
                    name: 'STOP1, %',
                    objectKey: 'stop1Percentage',
                    classNames: 'sort-numeric'
                },
                {
                    name: 'STOP2, %',
                    objectKey: 'stop2Percentage',
                    classNames: 'sort-numeric'
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

    public applyFilters() {
        this.isChartsVisible = false;     

        let filterObject = {};

        if (this.testsAmountFilter !== undefined &&
            this.testsAmountFilter != null) {
            filterObject['n'] = [this.testsAmountFilter];
        }
        if (this.requirementsAmountFilter !== undefined &&
            this.requirementsAmountFilter != null) {
            filterObject['requirementsAmount'] = [this.requirementsAmountFilter];
        }
        if (this.n1Filter !== undefined &&
            this.n1Filter != null) {
           filterObject['n1Percentage'] = [this.n1Filter]; 
        }
        if (this.n2Filter !== undefined &&
            this.n2Filter != null) {
           filterObject['n2Percentage'] = [this.n2Filter]; 
        }
        if (this.n12Filter !== undefined &&
            this.n12Filter != null) {
           filterObject['n12Percentage'] = [this.n12Filter]; 
        }
        if (this.n21Filter !== undefined &&
            this.n21Filter != null) {
           filterObject['n21Percentage'] = [this.n21Filter]; 
        }
        if (this.aBorderFilter !== undefined &&
            this.aBorderFilter != null) {
            filterObject['aBorder'] = [this.aBorderFilter];
        }
        if (this.bBorderFilter !== undefined &&
            this.bBorderFilter != null) {
            filterObject['bBorder'] = [this.bBorderFilter];
        }

        this.myTable.gtClearFilter();

        if (Object.keys(filterObject).length !== 0) {
            this.myTable.gtApplyFilter(filterObject);
            this.isFilterApplied = true;
        }
    }

    public removeFilters() {
        this.isChartsVisible = false;
        this.isFilterApplied = false;

        this.testsAmountFilter = undefined; 
        this.requirementsAmountFilter = undefined;
        
        this.n1Filter = undefined;
        this.n2Filter = undefined;
        this.n12Filter = undefined;
        this.n21Filter = undefined;

        this.aBorderFilter = undefined;
        this.bBorderFilter = undefined;

        this.myTable.gtClearFilter();
    }

    public drawCharts() {
        if (this.testsAmountFilter === undefined ||
            this.n1Filter === undefined ||
            this.n2Filter === undefined ||
            this.n12Filter === undefined ||
            this.n21Filter === undefined ||
            this.aBorderFilter === undefined ||
            this.bBorderFilter === undefined) {

            this.alertService.error('Not all required fileds are filled');
            
            return;
        }
        // console.log(this.myTable.gtInfo);
        
        let filteredData = this.getFilteredData();
        console.log(filteredData);
        
        let firstDataTable = [
            ['Requirements amount', 'STOP1', 'STOP2', 'STOP3', 'STOP4']
        ];

        let secondDataTable = [
            ['Requirements amount', 'STOP1+2+3+4', 'STOP1+2+3', 'STOP1+2', 'STOP1']
        ];

        filteredData.forEach(element => {
            firstDataTable.push([
                element.requirementsAmount,
                element.stop1Percentage,
                element.stop2Percentage,
                element.stop3Percentage,
                element.stop4Percentage]);

            secondDataTable.push([
                element.requirementsAmount,
                element.stop1Percentage + element.stop2Percentage + element.stop3Percentage + element.stop4Percentage,
                element.stop1Percentage + element.stop2Percentage + element.stop3Percentage,
                element.stop1Percentage + element.stop2Percentage,
                element.stop1Percentage
            ]);
        });

        console.log(firstDataTable);

        this.firstLineChartOptions.dataTable = firstDataTable;
        this.secondLineChartOptions.dataTable = secondDataTable;
        this.isChartsVisible = true;
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

    private getFilteredData() {
     let output = [];
     let filterBy = this.myTable.gtInfo.filter;
        for (let i = 0; i < this.configObject.data.length; i++) {
            let rowObject = this.configObject.data[i];
            let match = true;
            for (let property in filterBy) {
                if (filterBy.hasOwnProperty(property)) {
                    if (filterBy[property].indexOf(rowObject[property]) === -1) {
                        match = false;
                    }
                }
            }
            if (match) {
                output.push(rowObject);
            }
        }
        return output;   
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

    minPercentageFromA: number;
    maxPercentageFromA: number;
    
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
