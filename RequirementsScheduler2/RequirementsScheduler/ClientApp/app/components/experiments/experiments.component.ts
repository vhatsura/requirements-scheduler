import { Component, Input, OnInit, Output, EventEmitter, ViewChild } from "@angular/core";

import { Experiment, ExperimentStatus } from "../../models/index";
import { ExperimentService } from "../../services/index";

import { Subscription } from 'rxjs';

import { isBrowser } from 'angular2-universal';
import { GtConfig, GenericTableComponent } from 'angular2-generic-table';

import { ExperimentDetailComponent } from '../experiment-detail/experiment-detail.component';

@Component({
    selector: "experiments",
    styles: [`
  `],
    template: require("./experiments.component.html")
})
export class ExperimentsComponent implements OnInit {
    public configObject: GtConfig<any>;
    public tableInfo = {};

    @Output() data = new EventEmitter();

    @ViewChild(GenericTableComponent)
    private myTable: GenericTableComponent<any, ExperimentDetailComponent>;
    public expandedRow = ExperimentDetailComponent;
    public showColumnControls = false;

    constructor(private experimentService: ExperimentService) {
        this.configObject = {
            settings: [
                {
                    objectKey: 'id',
                    visible: true,
                    sort: 'desc',
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
                    name: 'Amount of tests',
                    objectKey: 'testsAmount',
                    classNames: 'sort-numeric',
                },
                {
                    name: 'Amount of requirements',
                    objectKey: 'requirementsAmount',
                    classNames: 'sort-numeric',
                }
            ],
            data:[]
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
                //console.log(experiments);
                
                this.configObject.data = [];
                this.configObject.data.push(experiments);
                return this.configObject.data = experiments;
            });    
    }

    isBrowser() {
        return isBrowser;
    }
}