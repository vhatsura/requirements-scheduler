import { Component, Input, OnInit, Output, Inject, EventEmitter, ViewChild } from "@angular/core";

import { Experiment, ExperimentStatus } from "../../models/index";
import { ExperimentService } from "../../services/index";

import { Subscription } from 'rxjs';

import { PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser, isPlatformServer } from '@angular/common';

import { GtConfig, GenericTableComponent } from 'angular-generic-table';

import { ExperimentDetailComponent } from '../experiment-detail/experiment-detail.component';

@Component({
    selector: "experiments",
    styles: [`
  `],
    template: require("./experiments.component.html")
})
export class ExperimentsComponent implements OnInit {
    public configObject: GtConfig<any>;

    @Output() data = new EventEmitter();

    public expandedRow = ExperimentDetailComponent;
    public showColumnControls = false;

    constructor(
        @Inject(PLATFORM_ID) private platformId: Object,
        private experimentService: ExperimentService) {
        this.configObject = {
            settings: [
                {
                    objectKey: 'id',
                    visible: true,
                    sort: 'enable',
                    columnOrder: 0
                },
                {
                    objectKey: 'testsAmount',
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
                    objectKey: 'n1',
                    visible: true,
                    sort: 'enable',
                    columnOrder: 3
                },
                {
                    objectKey: 'n2',
                    visible: true,
                    sort: 'enable',
                    columnOrder: 4
                },
                {
                    objectKey: 'n12',
                    visible: true,
                    sort: 'enable',
                    columnOrder: 5
                },
                {
                    objectKey: 'n21',
                    visible: true,
                    sort: 'enable',
                    columnOrder: 6
                },
                {
                    objectKey: 'created',
                    visible: true,
                    sort: 'desc',
                    columnOrder: 7
                }
            ],
            fields: [
                {
                    name: 'Id',
                    objectKey: 'id',
                    classNames: 'clickable sort-string',
                    expand: true
                },
                {
                    name: 'Tests',
                    objectKey: 'testsAmount',
                    classNames: 'sort-numeric',
                },
                {
                    name: 'Requirements',
                    objectKey: 'requirementsAmount',
                    classNames: 'sort-numeric',
                },
                {
                    name: 'N1, %',
                    objectKey: 'n1',
                    classNames: 'sort-numeric',
                },
                {
                    name: 'N2, %',
                    objectKey: 'n2',
                    classNames: 'sort-numeric',
                },
                {
                    name: 'N12, %',
                    objectKey: 'n12',
                    classNames: 'sort-numeric',
                },
                {
                    name: 'N21, %',
                    objectKey: 'n21',
                    classNames: 'sort-numeric',
                },
                {
                    name: 'Created',
                    objectKey: 'created',
                    classNames: 'clickable sort-string'
                }
            ],
            data: []
        };

    }

    busy: Subscription;

    public ExperimentStatus = ExperimentStatus;
    public experimentStatus : ExperimentStatus;
    @Input('experimentStatus') set status(value: ExperimentStatus) {

        const status = this.ExperimentStatus[value.toString()] as ExperimentStatus;
        switch (status) {
            case ExperimentStatus.New:
                this.experimentStatus = ExperimentStatus.New;
                break;
            case ExperimentStatus.InProgress:
                this.experimentStatus = ExperimentStatus.InProgress;
                break;
            case ExperimentStatus.Completed:
                this.experimentStatus = ExperimentStatus.Completed;
                break;
            default:
                this.experimentStatus = value;
                break;
        }
    }

    ngOnInit(): void {
        this.busy = this.experimentService.getByStatus(this.experimentStatus)
            .subscribe(experiments => this.configObject.data = experiments);
    }

    updateExperiments() {
        this.busy = this.experimentService.getByStatus(this.experimentStatus)
            .subscribe(experiments => {              
                this.configObject.data.length = 0;
                this.configObject.data = this.configObject.data.concat(experiments);
                return this.configObject.data;
            });    
    }

    isBrowser() {
        return isPlatformBrowser(this.platformId);
    }
}