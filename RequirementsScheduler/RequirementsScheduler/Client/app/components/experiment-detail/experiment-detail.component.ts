import { Component, OnInit, Input } from '@angular/core';
import { GtExpandedRow } from 'angular-generic-table';

import { Test, Report } from '../../models/index';

import { ExperimentService } from '../../services/index';

@Component({
    selector: 'experiment-detail',
    templateUrl: './experiment-detail.component.html'
})
export class ExperimentDetailComponent extends GtExpandedRow<any> implements OnInit {

    selectedTest: Test;
    tests: Test[];
    report: Report;

    constructor(private _experimentService: ExperimentService) { super(); }

    ngOnInit() {
        console.log(this.row);
        this._experimentService.getResultInfo(this.row.id)
            .subscribe(report => { this.report = report; });
    }
}
