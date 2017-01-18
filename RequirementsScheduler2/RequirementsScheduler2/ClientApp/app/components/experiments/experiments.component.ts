import { Component, Input, AfterContentInit } from '@angular/core';

import { Experiment, ExperimentStatus } from '../../models/index';
import { ExperimentService } from "../../services/index";

@Component({
    selector: 'experiments',
    styles: [`
  `],
    template: require('./experiments.component.html'),
})
export class ExperimentsComponent implements AfterContentInit {
    public ExperimentStatus = ExperimentStatus;
    public experimentStatus : ExperimentStatus;
    @Input('experimentStatus') set status(value: ExperimentStatus) {

        let status = this.ExperimentStatus[value.toString()] as ExperimentStatus;
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

    private experiments : Array<Experiment>;

    ngAfterContentInit(): void {
        this.experimentService.getByStatus(this.experimentStatus)
            .subscribe(experiments => this.experiments = experiments);
    }
}