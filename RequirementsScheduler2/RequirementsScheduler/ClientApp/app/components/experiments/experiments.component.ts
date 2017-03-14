import { Component, Input, AfterContentInit } from "@angular/core";

import { Experiment, ExperimentStatus } from "../../models/index";
import { ExperimentService } from "../../services/index";

import { Subscription } from 'rxjs';

import { isBrowser } from 'angular2-universal';

@Component({
    selector: "experiments",
    styles: [`
  `],
    template: require("./experiments.component.html")
})
export class ExperimentsComponent implements AfterContentInit {
    columns = [
        { name: 'id' },
        { name: 'testAmount' },
        { name: 'requirementsAmount' }
    ];

    rows = [];

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

    constructor(
        private experimentService: ExperimentService
    ) { }

    ngAfterContentInit(): void {
        this.busy = this.experimentService.getByStatus(this.experimentStatus)
            .subscribe(experiments => this.rows = experiments);
    }

    updateExperiments(): void {
        this.busy = this.experimentService.getByStatus(this.experimentStatus)
            .subscribe(experiments => this.rows = experiments);    
    }
}